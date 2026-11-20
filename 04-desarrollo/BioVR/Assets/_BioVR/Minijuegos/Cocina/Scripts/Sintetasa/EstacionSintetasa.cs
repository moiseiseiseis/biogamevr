using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class EstacionSintetasa : MonoBehaviour
    {
        [Header("Ranuras (Punto_ARNt_1 y Punto_ARNt_2)")]
        [SerializeField] Transform[] puntosRanura = new Transform[2];

        [Header("Barras de progreso (Barra_carga_1 y Barra_carga_2)")]
        [SerializeField] Transform[] barrasCarga = new Transform[2];

        class Ranura
        {
            public ARNtCocina arnt;
            public float progreso;
            public int ultimoOctavo;
        }

        readonly Ranura[] ranuras = new Ranura[] { new Ranura(), new Ranura() };
        CorrienteARNt corriente;

        public bool TieneARNtVacio
        {
            get
            {
                for (int i = 0; i < ranuras.Length; i++)
                    if (ranuras[i].arnt != null && !ranuras[i].arnt.Cargado)
                        return true;
                return false;
            }
        }

        void Awake()
        {
            corriente = FindFirstObjectByType<CorrienteARNt>();
            for (int i = 0; i < barrasCarga.Length; i++)
                ActualizarBarraVisual(i, 0f);
        }

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
        }

        void AlIniciarTurno(TurnoData turno)
        {
            for (int i = 0; i < ranuras.Length; i++)
            {
                ranuras[i].arnt = null;
                ranuras[i].progreso = 0f;
                ranuras[i].ultimoOctavo = 0;
                ActualizarBarraVisual(i, 0f);
            }
            gameObject.SetActive(true);
        }

        void AlCambiarFase(FaseCocina fase)
        {
            if (fase == FaseCocina.Plegando || fase == FaseCocina.Fin)
                gameObject.SetActive(false);
        }

        void Update()
        {
            for (int i = 0; i < ranuras.Length; i++)
            {
                var r = ranuras[i];
                if (r.arnt == null) continue;

                // Si el jugador agarró y sacó el ARNt de la ranura, liberarla
                if (r.arnt.Lugar != LugarARNt.Sintetasa)
                {
                    r.arnt = null;
                    r.progreso = 0f;
                    r.ultimoOctavo = 0;
                    ActualizarBarraVisual(i, 0f);
                    continue;
                }

                if (i < puntosRanura.Length && puntosRanura[i] != null)
                {
                    r.arnt.transform.SetPositionAndRotation(puntosRanura[i].position, puntosRanura[i].rotation);
                }
            }
        }

        // Llamado desde CorrienteARNt.AlSoltar cuando el jugador suelta un ARNt
        public bool IntentarColocar(ARNtCocina arnt)
        {
            if (arnt == null) return false;

            float dist = Vector3.Distance(arnt.transform.position, transform.position);
            // Usamos RadioEstacion de ControlesCocina con un margen cómodo para las dos ranuras
            if (dist > ControlesCocina.RadioEstacion * 2.5f) return false;

            var control = ControlCocina.Instancia;
            if (control == null) return false;

            // 1. Verificar si hay ranura libre antes de llamar a M1
            int libre = -1;
            for (int i = 0; i < ranuras.Length; i++)
            {
                if (ranuras[i].arnt == null)
                {
                    libre = i;
                    break;
                }
            }

            if (libre == -1)
            {
                EventosCocina.Rechazo("sint_llena");
                EventosCocina.Mensaje("sint_llena");
                control.Sonar("error");
                DevolverACorriente(arnt);
                return true;
            }

            // 2. M1 decide si puede entrar (revisa fase, si ya está cargado o si trae error)
            if (!control.PuedeEntrarASintetasa(arnt, out _))
            {
                DevolverACorriente(arnt);
                return true;
            }

            // 3. Entra en la primera ranura libre con progreso 0
            ranuras[libre].arnt = arnt;
            ranuras[libre].progreso = 0f;
            ranuras[libre].ultimoOctavo = 0;
            ActualizarBarraVisual(libre, 0f);
            return true;
        }

        public void AvanzarProgreso(float deltaProgreso)
        {
            var control = ControlCocina.Instancia;
            if (control == null || control.Pausado) return;

            for (int i = 0; i < ranuras.Length; i++)
            {
                var r = ranuras[i];
                if (r.arnt == null || r.arnt.Cargado) continue;

                r.progreso = Mathf.Clamp01(r.progreso + deltaProgreso);
                ActualizarBarraVisual(i, r.progreso);

                int octavo = Mathf.FloorToInt(r.progreso * 8f);
                if (octavo > r.ultimoOctavo && r.progreso < 1f)
                {
                    r.ultimoOctavo = octavo;
                    control.Sonar("tic");
                }

                if (r.progreso >= 1f)
                {
                    // M1 calcula el errorSintetasa (una vez por ARNt), asigna carga y lanza OnARNtCargado
                    control.TerminarCarga(r.arnt);
                }
            }
        }

        void ActualizarBarraVisual(int indice, float valor)
        {
            if (indice < 0 || indice >= barrasCarga.Length || barrasCarga[indice] == null) return;
            Vector3 esc = barrasCarga[indice].localScale;
            esc.x = Mathf.Lerp(0.001f, 1f, Mathf.Clamp01(valor));
            barrasCarga[indice].localScale = esc;
        }

        void DevolverACorriente(ARNtCocina arnt)
        {
            if (corriente == null) corriente = FindFirstObjectByType<CorrienteARNt>();
            if (corriente != null) corriente.DevolverAlTubo(arnt);
            else if (ControlCocina.Instancia != null) ControlCocina.Instancia.DevolverACorriente(arnt);
        }
    }
}


