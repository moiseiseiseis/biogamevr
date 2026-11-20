using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.Cocina
{
    public class CorrienteARNt : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] Transform anclaCocina;
        [SerializeField] GameObject prefabARNt;
        [SerializeField] GameObject visualTubo;

        [Header("Geometría del tubo (Especificación 1.2 y 3.3)")]
        [SerializeField] float anguloInicio = 66f;
        [SerializeField] float anguloFin = 14f;
        [SerializeField] float radioTubo = 0.72f;
        [SerializeField] float alturaBase = 1.06f;
        [SerializeField] float amplitudOnda = 0.035f;

        const int TOPE_ESCENA = 9;

        class ItemARNt
        {
            public ARNtCocina datos;
            public ARNtVisual visual;
            public XRGrabInteractable grab;
            public float u;
            public float fase;
        }

        readonly List<ItemARNt> activos = new List<ItemARNt>();
        float relojRelleno;

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            EventosCocina.OnReciclado += AlReciclar;
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            EventosCocina.OnReciclado -= AlReciclar;
        }

        void AlIniciarTurno(TurnoData turno)
        {
            LimpiarTodos();
            if (visualTubo != null) visualTubo.SetActive(true);

            var control = ControlCocina.Instancia;
            if (control == null) return;

            int iniciales = control.Config.arntIniciales;
            var necesarios = control.Necesarios();

            for (int i = 0; i < iniciales; i++)
            {
                string codon = (i < necesarios.Count) ? necesarios[i].tripleta : control.Senuelo();
                float uInicial = (float)i / iniciales;
                CrearARNt(codon, uInicial);
            }
            relojRelleno = control.Config.intervaloRelleno;
        }

        void AlCambiarFase(FaseCocina fase)
        {
            if (fase == FaseCocina.Plegando || fase == FaseCocina.Fin)
            {
                if (visualTubo != null) visualTubo.SetActive(false);
                LimpiarTodos();
            }
        }

        void AlReciclar(ARNtCocina arnt)
        {
            var item = activos.Find(x => x.datos == arnt);
            if (item != null)
            {
                item.u = 0f;
                item.datos.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            var control = ControlCocina.Instancia;
            if (control == null || control.Pausado || control.Fase != FaseCocina.Armando) return;

            float dt = Time.deltaTime;
            float vel = control.Config.velocidadCorriente;
            var necesarios = control.Necesarios();
            bool mostrarPista = control.SegundosEspera > control.Config.esperaPista;

            // Mover y actualizar visuales de los ARNt
            for (int i = 0; i < activos.Count; i++)
            {
                var item = activos[i];
                if (item.datos == null) continue;

                if (item.datos.Lugar == LugarARNt.Corriente)
                {
                    item.u += vel * dt;
                    if (item.u > 1f) item.u -= 1f;
                    PosicionarEnCorriente(item);

                    bool sirve = !item.datos.Cargado && necesarios.Exists(c => c.tripleta == item.datos.CodonQueLee);
                    if (item.visual != null) item.visual.EstablecerBrilloPista(mostrarPista && sirve);
                }
                else if (item.datos.Lugar == LugarARNt.Mano)
                {
                    if (item.visual != null) item.visual.EstablecerBrilloPista(false);
                    if (VueloARNt.Instancia != null) VueloARNt.Instancia.ActualizarRastreoMano(item.datos);
                }
                else
                {
                    if (item.visual != null) item.visual.EstablecerBrilloPista(false);
                }
            }

            // Revisión periódica de reposición (cada 0.8 s)
            relojRelleno -= dt;
            if (relojRelleno <= 0f)
            {
                relojRelleno = control.Config.intervaloRelleno;
                RevisarReposicion(control, necesarios);
            }
        }

        void PosicionarEnCorriente(ItemARNt item)
        {
            float angulo = Mathf.Lerp(anguloInicio, anguloFin, item.u);
            float altura = alturaBase + amplitudOnda * Mathf.Sin(item.u * 14f + item.fase);

            Transform tr = item.datos.transform;
            if (anclaCocina != null)
            {
                tr.position = anclaCocina.TransformPoint(PosicionMesa.En(angulo, radioTubo, altura));
                Vector3 dirJugador = anclaCocina.position - tr.position;
                dirJugador.y = 0f;
                if (dirJugador.sqrMagnitude > 0.001f)
                {
                    float mecidoY = 0.4f * Mathf.Rad2Deg * Mathf.Sin(Time.time * 1.8f + item.fase);
                    Quaternion baseRot = Quaternion.LookRotation(dirJugador, Vector3.up);
                    tr.rotation = baseRot * Quaternion.Euler(-0.3f * Mathf.Rad2Deg, mecidoY, 0f);
                }
            }
        }

        void RevisarReposicion(ControlCocina control, List<Codon> necesarios)
        {
            activos.RemoveAll(x => x.datos == null);
            if (activos.Count >= TOPE_ESCENA) return;

            // 1. Verificar si falta algún codón necesario (contando todos los ARNt en juego excepto los de Salida)
            foreach (var nec in necesarios)
            {
                int requeridos = necesarios.FindAll(c => c.tripleta == nec.tripleta).Count;
                int enJuego = activos.FindAll(a => a.datos.Lugar != LugarARNt.Salida && a.datos.CodonQueLee == nec.tripleta).Count;

                if (enJuego < requeridos)
                {
                    CrearARNt(nec.tripleta, 0f);
                    return;
                }
            }

            // 2. Si no falta ninguno, rellenar señuelos hasta el máximo permitido (5 normal, 9 en codón raro)
            int enCorriente = activos.FindAll(a => a.datos.Lugar == LugarARNt.Corriente).Count;
            if (enCorriente < control.MaximoEnCorriente)
            {
                CrearARNt(control.Senuelo(), 0f);
            }
        }

        void CrearARNt(string tripleta, float uInicial)
        {
            var control = ControlCocina.Instancia;
            if (control == null || prefabARNt == null) return;

            var aa = control.Codigo.Traducir(tripleta);
            var go = Instantiate(prefabARNt, transform);
            var datos = go.GetComponent<ARNtCocina>();
            datos.Configurar(tripleta, aa);

            var grab = go.GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(args => AlAgarrar(datos));
                grab.selectExited.AddListener(args => AlSoltar(datos, args));
            }

            if (VueloARNt.Instancia != null)
                VueloARNt.Instancia.RegistrarARNt(datos);

            var item = new ItemARNt
            {
                datos = datos,
                visual = go.GetComponent<ARNtVisual>(),
                grab = grab,
                u = uInicial,
                fase = Random.Range(0f, Mathf.PI * 2f)
            };

            activos.Add(item);
            PosicionarEnCorriente(item);
        }

        void AlAgarrar(ARNtCocina datos)
        {
            // No interferir si M4 lo tiene en modo llave en el ribosoma
            if (datos.Lugar == LugarARNt.Llave) return;

            if (ControlCocina.Instancia != null)
                ControlCocina.Instancia.Agarrar(datos);
        }

        void AlSoltar(ARNtCocina datos, SelectExitEventArgs args)
        {
            // Si M4 lo tiene en modo llave o M3 en la sintetasa, no devolver al tubo
            if (datos.Lugar == LugarARNt.Llave || datos.Lugar == LugarARNt.Sintetasa) return;
            if (datos.Lugar != LugarARNt.Mano)
            {
                if (VueloARNt.Instancia != null) VueloARNt.Instancia.LimpiarRastreo(datos);
                return;
            }

            // Dentro del sitio A lo recibe el ribosoma (M4, ZonaSitioA); si lo rechaza, él lo regresa al tubo
            if (ZonaSitioA.AlgunaContiene(datos)) return;

            // Verificar si se soltó sobre la sintetasa (M3)
            var sint = FindFirstObjectByType<EstacionSintetasa>();
            if (sint != null && sint.IntentarColocar(datos))
                return;

            Vector3 vel = Vector3.zero;
            var rb = datos.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic) vel = rb.linearVelocity;

            if (VueloARNt.Instancia != null && VueloARNt.Instancia.IntentarLanzarVR(datos, vel))
                return;

            // Si no se aventó a un destino válido ni entró a una estación, vuelve al tubo en u = 0.35
            DevolverAlTubo(datos);
        }

        public void DevolverAlTubo(ARNtCocina datos)
        {
            if (ControlCocina.Instancia != null)
                ControlCocina.Instancia.DevolverACorriente(datos);

            var item = activos.Find(x => x.datos == datos);
            if (item != null) item.u = 0.35f;
        }

        void LimpiarTodos()
        {
            foreach (var item in activos)
                if (item.datos != null) Destroy(item.datos.gameObject);
            activos.Clear();
        }
    }
}
