using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Controla el panel fijo de avisos y consejos (70x34 cm) encima de la mesa.
    /// Arriba muestra el último aviso de evento (2.6 s) y abajo el consejo contextual continuo.
    /// </summary>
    public class PanelMensajes : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private TMP_Text textoAviso;
        [SerializeField] private TMP_Text textoConsejo;
        [SerializeField] private TMP_Text textoFase;

        [Header("Tiempos")]
        [SerializeField] private float duracionAviso = 2.6f;
        [SerializeField] private float intervaloActualizacionConsejo = 0.25f;

        private float temporizadorAviso;
        private float temporizadorConsejo;
        private ARNtCocina[] arntsEnEscena = new ARNtCocina[0];

        private void OnEnable()
        {
            EventosCocina.OnMensaje += MostrarAviso;
            EventosCocina.OnRechazo += MostrarAviso;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        private void OnDisable()
        {
            EventosCocina.OnMensaje -= MostrarAviso;
            EventosCocina.OnRechazo -= MostrarAviso;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        private void Start()
        {
            if (textoAviso != null) textoAviso.text = string.Empty;
            RefrescarListaARNt();
            ActualizarConsejoContextual();
        }

        private void Update()
        {
            if (temporizadorAviso > 0f)
            {
                temporizadorAviso -= Time.deltaTime;
                if (temporizadorAviso <= 0f && textoAviso != null)
                {
                    textoAviso.text = string.Empty;
                }
            }

            temporizadorConsejo -= Time.deltaTime;
            if (temporizadorConsejo <= 0f)
            {
                temporizadorConsejo = intervaloActualizacionConsejo;
                RefrescarListaARNt();
                ActualizarConsejoContextual();
            }
        }

        private void AlIniciarTurno(TurnoData turno)
        {
            if (textoAviso != null) textoAviso.text = string.Empty;
            temporizadorAviso = 0f;
            RefrescarListaARNt();
            ActualizarConsejoContextual();
        }

        private void AlCambiarFase(FaseCocina nuevaFase)
        {
            if (textoFase != null)
            {
                textoFase.text = nuevaFase switch
                {
                    FaseCocina.Armando => "FASE A · SÍNTESIS",
                    FaseCocina.Plegando => "PLEGANDO CADENA",
                    FaseCocina.Busca => "FASE B · INSPECCIÓN",
                    FaseCocina.Decide => "FASE B · DECISIÓN",
                    FaseCocina.Final => "FINALIZANDO",
                    _ => string.Empty
                };
            }
            ActualizarConsejoContextual();
        }

        public void MostrarAviso(string clave)
        {
            if (textoAviso == null || string.IsNullOrEmpty(clave)) return;

            var cocina = ControlCocina.Instancia;
            textoAviso.text = cocina != null ? cocina.Texto(clave) : $"[{clave}]";
            temporizadorAviso = duracionAviso;
        }

        private void RefrescarListaARNt()
        {
            arntsEnEscena = FindObjectsByType<ARNtCocina>(FindObjectsSortMode.None);
        }

        private void ActualizarConsejoContextual()
        {
            if (textoConsejo == null) return;

            var cocina = ControlCocina.Instancia;
            if (cocina == null || cocina.Turno == null)
            {
                textoConsejo.text = string.Empty;
                return;
            }

            string claveConsejo = ResolverClaveConsejo(cocina);
            textoConsejo.text = cocina.Texto(claveConsejo);
        }

        private string ResolverClaveConsejo(ControlCocina cocina)
        {
            switch (cocina.Fase)
            {
                case FaseCocina.Plegando:
                    return "plegando";
                case FaseCocina.Busca:
                    return cocina.PistaCuenta ? "busca_pista" : "busca";
                case FaseCocina.Decide:
                    return cocina.Marcador.usoChaperona ? "chaperona" : "cuenta_encontrada";
                case FaseCocina.Final:
                    return "proteasoma";
            }

            // Fase A: los 9 consejos contextuales por orden de prioridad
            if (cocina.HorquillaActiva)
                return "consejo_horquilla";

            ARNtCocina enMano = null;
            bool hayVacioEnSintetasa = false;
            bool hayEquivocadoEnSintetasa = false;
            bool hayCargadoEnSintetasa = false;

            for (int i = 0; i < arntsEnEscena.Length; i++)
            {
                var t = arntsEnEscena[i];
                if (t == null) continue;

                if (t.Lugar == LugarARNt.Mano)
                    enMano = t;
                else if (t.Lugar == LugarARNt.Sintetasa)
                {
                    if (!t.Cargado) hayVacioEnSintetasa = true;
                    else if (t.Equivocado) hayEquivocadoEnSintetasa = true;
                    else hayCargadoEnSintetasa = true;
                }
            }

            if (enMano != null)
            {
                if (enMano.Equivocado)
                    return "consejo_equivocado";
                if (enMano.Usado)
                    return "consejo_usado";
                if (!enMano.Cargado)
                    return "consejo_vacio";

                bool algunRibosomaLoPide = false;
                var ribosomas = cocina.Ribosomas;
                for (int i = 0; i < ribosomas.Count; i++)
                {
                    var pedido = cocina.PideAhora(ribosomas[i]);
                    if (pedido.HasValue && pedido.Value.tripleta == enMano.CodonQueLee)
                    {
                        algunRibosomaLoPide = true;
                        break;
                    }
                }

                return algunRibosomaLoPide ? "consejo_listo" : "consejo_otro_codon";
            }

            if (cocina.ARNtEnLlave != null)
                return "consejo_llave";

            if (hayVacioEnSintetasa)
                return "consejo_sintetasa_cargar";
            if (hayEquivocadoEnSintetasa)
                return "consejo_equivocado";
            if (hayCargadoEnSintetasa)
                return "consejo_sintetasa_sacar";

            var listaRibosomas = cocina.Ribosomas;
            for (int i = 0; i < listaRibosomas.Count; i++)
            {
                if (cocina.EnParo(listaRibosomas[i]))
                    return "consejo_fin";
            }

            return cocina.NumeroTurno switch
            {
                0 => "consejo_inicio_t1",
                1 => "consejo_inicio_t2",
                _ => "consejo_inicio_t3"
            };
        }
    }
}