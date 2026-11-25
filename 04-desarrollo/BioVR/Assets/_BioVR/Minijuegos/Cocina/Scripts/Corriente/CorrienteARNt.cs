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

        // Prueba del 23-nov: en el tubo de la especificación (66° → 14°, 0.72 m, 1.06 m de alto; unos 65 cm) los ARNt
        // se encimaban y pasaban sobre la sintetasa y junto al ribosoma. Ahora flotan en el aire por un camino más
        // largo (unos 1.12 m), corrido a la izquierda y por encima del letrero de la sintetasa; termina antes del
        // frente. Un hilo delgado marca el camino (TuboCorrienteVisual).
        [Header("Camino de la corriente (en el aire)")]
        [SerializeField] float anguloInicio = 115f;
        [SerializeField] float anguloFin = 35f;
        [SerializeField] float radioTubo = 0.80f;
        [SerializeField] float alturaBase = 1.15f;
        [SerializeField] float amplitudOnda = 0.02f;

        // Punto a la mitad del camino: a dónde señala la flecha del tutorial cuando habla de la corriente
        public Transform PuntoMedio { get; private set; }

        void Awake()
        {
            PuntoMedio = new GameObject("Mitad de la corriente").transform;
            PuntoMedio.SetParent(transform, false);
        }

        void Start()
        {
            if (anclaCocina != null) PuntoMedio.position = anclaCocina.TransformPoint(PosicionMesa.En((anguloInicio + anguloFin) * 0.5f, radioTubo, alturaBase));
            var tubo = visualTubo != null ? visualTubo.GetComponent<TuboCorrienteVisual>() : null;
            if (tubo != null) tubo.Configurar(anguloInicio, anguloFin, radioTubo, alturaBase);
        }

        const int TOPE_ESCENA = 9;

        class ItemARNt
        {
            public ARNtCocina datos;
            public ARNtVisual visual;
            public XRGrabInteractable grab;
            public float u;
            public float fase;
            public bool dioVuelta; // llegó al final del camino en este cuadro: se forma al final de la fila
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
                // Separados desde el principio (los que no caben esperan antes del inicio del tubo)
                float uInicial = 1f - separacionMinima * 0.5f - i * separacionMinima;
                CrearARNt(codon, uInicial);
            }
            foreach (var item in activos)
            {
                bool cabe = item.u >= 0f;
                if (item.datos != null) item.datos.gameObject.SetActive(cabe);
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

            // Avanzar los que van en el camino y separarlos (prueba del 23-nov: se encimaban la forma y el texto)
            enTubo.Clear();
            int enCorriente = 0;
            foreach (var item in activos)
                if (item.datos != null && item.datos.Lugar == LugarARNt.Corriente) enCorriente++;
            foreach (var item in activos)
                if (item.datos != null && item.datos.Lugar == LugarARNt.Corriente)
                {
                    item.u += vel * dt;
                    if (item.u > 1f)
                    {
                        // Prueba del 24-nov: los señuelos daban vueltas para siempre y los que pedían los ribosomas
                        // se quedaban escondidos en la fila. Un señuelo que llega al final cuando sobran ARNt se retira.
                        if (enCorriente > control.MaximoEnCorriente && !Sirve(item, necesarios) && !item.datos.Cargado)
                        {
                            Destroy(item.datos.gameObject);
                            item.datos = null;
                            enCorriente--;
                            continue;
                        }
                        item.dioVuelta = true;
                    }
                    enTubo.Add(item);
                }
            activos.RemoveAll(x => x.datos == null);
            Separar(necesarios);

            // Mover y actualizar visuales de los ARNt
            for (int i = 0; i < activos.Count; i++)
            {
                var item = activos[i];
                if (item.datos == null) continue;

                if (item.datos.Lugar == LugarARNt.Corriente)
                {
                    // El que no cabe todavía espera escondido antes del inicio del tubo
                    bool cabe = item.u >= 0f;
                    if (item.datos.gameObject.activeSelf != cabe) item.datos.gameObject.SetActive(cabe);
                    if (!cabe) continue;
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

        [Header("Separación entre ARNt (fracción del camino: 0.2 ≈ 22 cm; caben 5)")]
        [SerializeField] float separacionMinima = 0.2f;
        readonly List<ItemARNt> enTubo = new List<ItemARNt>();
        readonly List<ItemARNt> visibles = new List<ItemARNt>();
        readonly List<ItemARNt> fila = new List<ItemARNt>();

        static bool Sirve(ItemARNt item, List<Codon> necesarios) =>
            item.datos != null && necesarios.Exists(c => c.tripleta == item.datos.CodonQueLee);

        // Cada uno queda al menos "separacionMinima" detrás del de adelante. Caben 5 a la vez en el camino (también
        // en el codón raro: decisión del 23-nov); los demás esperan escondidos antes del inicio (u < 0), en una fila.
        // Prueba del 24-nov: el que llegaba al final daba la vuelta y se metía DELANTE de la fila, así que los de la
        // fila nunca entraban ("desaparecieron" CAA y UUG). Ahora la fila va primero, con prioridad para los ARNt
        // que pide algún ribosoma, y el que da la vuelta se forma al final.
        void Separar(List<Codon> necesarios)
        {
            visibles.Clear();
            fila.Clear();
            foreach (var item in enTubo)
                if (item.u >= 0f && !item.dioVuelta) visibles.Add(item); else fila.Add(item);

            visibles.Sort((a, b) => b.u.CompareTo(a.u));
            fila.Sort((a, b) =>
            {
                int sa = Sirve(a, necesarios) ? 0 : 1, sb = Sirve(b, necesarios) ? 0 : 1;
                if (sa != sb) return sa.CompareTo(sb);
                // Dentro de cada grupo, por orden de llegada; los que acaban de dar la vuelta, al último
                float ka = a.dioVuelta ? -1000f : a.u, kb = b.dioVuelta ? -1000f : b.u;
                return kb.CompareTo(ka);
            });

            float tope = float.MaxValue;
            foreach (var v in visibles)
            {
                if (v.u > tope) v.u = tope;
                tope = v.u - separacionMinima;
            }
            // La fila va pegada detrás del último visible: el primero entra en cuanto hay lugar
            foreach (var q in fila)
            {
                q.dioVuelta = false;
                q.u = Mathf.Min(tope, 0f);
                tope = q.u - separacionMinima;
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
                    // El brazo del aminoácido (local −Z) apunta hacia atrás y arriba, no de lado hacia el vecino;
                    // el anticodón queda abajo, de frente al jugador. Se mece poco (±9°) para no invadir al vecino.
                    float mecidoY = 0.15f * Mathf.Rad2Deg * Mathf.Sin(Time.time * 1.8f + item.fase);
                    Quaternion baseRot = Quaternion.LookRotation(dirJugador, Vector3.up);
                    tr.rotation = baseRot * Quaternion.Euler(35f, mecidoY, 0f);
                }
            }
        }

        void RevisarReposicion(ControlCocina control, List<Codon> necesarios)
        {
            activos.RemoveAll(x => x.datos == null);

            // 1. Verificar si falta algún codón necesario (contando todos los ARNt en juego excepto los de Salida)
            foreach (var nec in necesarios)
            {
                int requeridos = necesarios.FindAll(c => c.tripleta == nec.tripleta).Count;
                int enJuego = activos.FindAll(a => a.datos.Lugar != LugarARNt.Salida && a.datos.CodonQueLee == nec.tripleta).Count;

                if (enJuego < requeridos)
                {
                    // Prueba del 24-nov: con la escena llena (señuelos + bandeja + sintetasa) ya no se creaba el que
                    // faltaba. Lo que pide un ribosoma va primero: se retira un señuelo de la corriente.
                    if (activos.Count >= TOPE_ESCENA && !RetirarUnSenuelo(necesarios)) return;
                    CrearARNt(nec.tripleta, 0f);
                    return;
                }
            }

            if (activos.Count >= TOPE_ESCENA) return;

            // 2. Si no falta ninguno, rellenar señuelos hasta el máximo permitido (5 normal, 9 en codón raro).
            // El señuelo evita repetir codones que ya van en la corriente.
            int enCorriente = activos.FindAll(a => a.datos.Lugar == LugarARNt.Corriente).Count;
            if (enCorriente < control.MaximoEnCorriente)
            {
                var yaEstan = new HashSet<string>();
                foreach (var a in activos) if (a.datos.Lugar == LugarARNt.Corriente) yaEstan.Add(a.datos.CodonQueLee);
                CrearARNt(control.Senuelo(yaEstan), 0f);
            }
        }

        // Quita de la corriente un señuelo vacío (de preferencia uno que espera escondido). false si no hay ninguno.
        bool RetirarUnSenuelo(List<Codon> necesarios)
        {
            ItemARNt elegido = null;
            foreach (var a in activos)
            {
                if (a.datos == null || a.datos.Lugar != LugarARNt.Corriente || a.datos.Cargado || Sirve(a, necesarios)) continue;
                if (elegido == null || a.u < elegido.u) elegido = a;
            }
            if (elegido == null) return false;
            Destroy(elegido.datos.gameObject);
            activos.Remove(elegido);
            return true;
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
            // Un ARNt usado no vuelve al tubo: regresa a la bandeja de salida (solo el embudo lo recicla)
            if (datos != null && datos.Usado && ControlCocina.Instancia != null)
            {
                ControlCocina.Instancia.RegresarASalida(datos);
                return;
            }
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
