using System.Collections;
using BioVR.Secuencia;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace BioVR.Pilotaje
{
    // Pilotaje libre en la célula (modo investigación, prueba aparte).
    //
    // En la llegada a la célula el jugador pilota la nave con los sticks para acercarse a los organelos y
    // registrarlos con el pokédex. Para no marear, sigue la regla del juego: la cabina nunca se mueve, se mueve
    // la célula (la zona "Llegada") alrededor de ella.
    //
    //   Stick izquierdo   avanzar, retroceder y moverse de lado (en la dirección de la nave, no de la cabeza)
    //   Stick derecho     arriba y abajo subir y bajar; a los lados, girar la nave de 30° en 30°
    //   A (mando derecho) piloto automático: va al organelo más cercano que no se ha registrado
    //
    //   - Velocidad constante con arranque y frenado suaves; nunca se inclina ni rueda (el horizonte queda recto).
    //   - Sin viñeta al moverse, para ver el paisaje (se puede prender en el Inspector si marea).
    //   - Se frena antes de chocar con un organelo y no puede salir de la membrana.
    //   - Brújula en el tablero: apunta al organelo más cercano sin registrar, con su distancia.
    //   - Mientras pilota se apagan caminar y girar del XR Origin (los sticks son de la nave); al salir se regresan.
    // Lo arma el menú BioVR → Pilotaje → Armar prueba de pilotaje.
    public class PilotajeLibre : MonoBehaviour, ITunnelingVignetteProvider
    {
        [Tooltip("Lo que se mueve: la zona Llegada (con su espacio celular).")]
        [SerializeField] Transform mundo;
        [Tooltip("La cabina: su posición es el centro de la nave y su eje Z azul, hacia dónde mira.")]
        [SerializeField] Transform cabina;
        [Tooltip("Altura del centro de la nave sobre el origen de la cabina (m).")]
        [SerializeField] float alturaCentro = 1.2f;

        [Header("Movimiento")]
        [SerializeField] float velocidad = 12f;         // m/s hacia adelante y a los lados
        [SerializeField] float velocidadVertical = 7f;  // m/s
        [SerializeField] float suavizado = 3f;          // qué tan rápido llega a la velocidad pedida (1/s)
        [SerializeField] float giro = 30f;              // grados por toque del stick derecho
        [SerializeField] float zonaMuerta = 0.2f;

        [Header("Choques")]
        [SerializeField] float radioNave = 2.5f;
        [SerializeField] float holgura = 1.5f;
        [Tooltip("Qué tanto de la membrana se puede recorrer (1 = hasta el borde).")]
        [SerializeField] float margenMembrana = 0.85f;
        [SerializeField] string nombreMembrana = "Membrana_Celular";

        [Header("Piloto automático")]
        [SerializeField] float velocidadAutomatico = 20f;
        [SerializeField] float duracionMinima = 2f, duracionMaxima = 6f;

        [Header("Brújula del tablero")]
        [SerializeField] Transform flecha;
        [SerializeField] TextMeshPro textoBrujula;

        [Header("Viñeta de comodidad")]
        [Tooltip("Cerrar la viñeta al pilotar con los sticks. Apagada: el jugador ve todo el paisaje mientras avanza.")]
        [SerializeField] bool vinetaAlPilotar = false;
        [Tooltip("Cerrar la viñeta durante el piloto automático (incluye un giro, que marea más).")]
        [SerializeField] bool vinetaEnAutomatico = false;
        [SerializeField] VignetteParameters vineta = new VignetteParameters
        {
            apertureSize = 0.7f, featheringEffect = 0.25f, easeInTime = 0.3f, easeOutTime = 0.4f,
            vignetteColor = Color.black, vignetteColorBlend = Color.black,
        };
        public VignetteParameters vignetteParameters => vineta;

        // Para el sonido: qué tan rápido va (0 a 1) y qué tan cerca está algo sin registrar (0 a 1)
        public static bool Activo { get; private set; }
        public static float Rapidez { get; private set; }
        public static float Cercania { get; private set; }

        GameManager gm;
        InputAction stickIzquierdo, stickDerecho, automatico;
        ContinuousMoveProvider caminar;
        SnapTurnProvider girar;
        TunnelingVignetteController controladorVineta;
        Vector3 posicionInicial;
        Quaternion rotacionInicial;
        Vector3 velocidadActual;
        bool giroListo = true, vinetaPuesta, enAutomatico;
        Bounds membranaLocal;
        bool hayMembrana;
        readonly RaycastHit[] golpes = new RaycastHit[16];
        Pokedex.Descubrible[] descubribles = new Pokedex.Descubrible[0];
        Pokedex.Descubrible masCercano;

        Vector3 Centro => cabina.position + Vector3.up * alturaCentro;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null || mundo == null || cabina == null)
            {
                Debug.LogError("[Pilotaje] Necesita el GameManager, la zona que se mueve y la cabina.", this);
                enabled = false;
                return;
            }
            posicionInicial = mundo.position;
            rotacionInicial = mundo.rotation;
            // Con un Rigidbody cinemático, mover la zona mueve sus colliders como un solo cuerpo (barato);
            // sin él, PhysX trataría cada collider como estático y los reconstruiría en cada cuadro
            if (!mundo.TryGetComponent<Rigidbody>(out var cuerpo)) cuerpo = mundo.gameObject.AddComponent<Rigidbody>();
            cuerpo.isKinematic = true;
            cuerpo.useGravity = false;
            caminar = FindFirstObjectByType<ContinuousMoveProvider>(FindObjectsInactive.Include);
            girar = FindFirstObjectByType<SnapTurnProvider>(FindObjectsInactive.Include);
            controladorVineta = FindFirstObjectByType<TunnelingVignetteController>(FindObjectsInactive.Include);

            stickIzquierdo = Accion("Pilotaje mover", InputActionType.Value, "<XRController>{LeftHand}/{Primary2DAxis}");
            stickDerecho = Accion("Pilotaje subir y girar", InputActionType.Value, "<XRController>{RightHand}/{Primary2DAxis}");
            automatico = Accion("Pilotaje automatico", InputActionType.Button, "<XRController>{RightHand}/{PrimaryButton}");

            gm.OnEstadoCambiado += AlCambiarEstado;
            AlCambiarEstado(gm.Estado);
        }

        static InputAction Accion(string nombre, InputActionType tipo, string ruta)
        {
            var a = new InputAction(nombre, tipo, ruta);
            a.Enable();
            return a;
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
            stickIzquierdo?.Dispose();
            stickDerecho?.Dispose();
            automatico?.Dispose();
            Activo = false;
        }

        // Se pilota solo en la llegada; al salir, la célula vuelve a su lugar y se regresan caminar y girar
        void AlCambiarEstado(GameState estado)
        {
            bool activar = estado == GameState.Llegada;
            if (activar == Activo) return;
            Activo = activar;
            StopAllCoroutines();
            enAutomatico = false;
            velocidadActual = Vector3.zero;
            Vineta(false);
            if (caminar != null) caminar.enabled = !activar;
            if (girar != null) girar.enabled = !activar;
            if (flecha != null) flecha.parent.gameObject.SetActive(activar);
            if (!activar)
            {
                mundo.SetPositionAndRotation(posicionInicial, rotacionInicial);
                return;
            }
            descubribles = mundo.GetComponentsInChildren<Pokedex.Descubrible>(true);
            var membrana = Buscar(mundo, nombreMembrana);
            hayMembrana = membrana != null;
            if (hayMembrana) membranaLocal = LimitesLocales(membrana);
            Debug.Log($"[Pilotaje] Activo: {descubribles.Length} organelos, membrana {(hayMembrana ? "encontrada" : "no encontrada (sin límite)")}.");
        }

        void Update()
        {
            if (!Activo) return;
            ActualizarBrujula();
            if (gm.Pausado || gm.EnTransicion || enAutomatico) return;

            if (automatico.WasPressedThisFrame() && masCercano != null)
            {
                StartCoroutine(IrA(masCercano));
                return;
            }

            // Velocidad pedida, en el marco de la nave (la cabina), nunca el de la cabeza
            var izq = ConZonaMuerta(stickIzquierdo.ReadValue<Vector2>());
            var der = stickDerecho.ReadValue<Vector2>();
            var adelante = Plano(cabina.forward);
            var lado = Plano(cabina.right);
            var pedida = (adelante * izq.y + lado * izq.x) * velocidad
                       + Vector3.up * (Mathf.Abs(der.y) > zonaMuerta ? der.y : 0f) * velocidadVertical;
            velocidadActual = Vector3.MoveTowards(velocidadActual, pedida, Mathf.Max(velocidad, velocidadVertical) * suavizado * Time.deltaTime);
            Rapidez = Mathf.Clamp01(velocidadActual.magnitude / velocidad);

            // Giro por pasos con el stick derecho a los lados
            if (Mathf.Abs(der.x) > 0.7f && giroListo)
            {
                Girar(Mathf.Sign(der.x) * giro);
                giroListo = false;
            }
            else if (Mathf.Abs(der.x) < 0.3f) giroListo = true;

            var paso = Limitar(velocidadActual * Time.deltaTime);
            if (paso.sqrMagnitude > 0f) mundo.position -= paso; // la nave avanza = la célula retrocede
            else if (velocidadActual.sqrMagnitude > 0f && pedida.sqrMagnitude == 0f) velocidadActual = Vector3.zero;
            Vineta(vinetaAlPilotar && Rapidez > 0.05f);
        }

        Vector2 ConZonaMuerta(Vector2 v) => v.magnitude < zonaMuerta ? Vector2.zero : v;
        static Vector3 Plano(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 0f ? v.normalized : Vector3.forward; }

        // La nave gira = la célula gira al revés alrededor del centro de la nave
        void Girar(float grados) => mundo.RotateAround(Centro, Vector3.up, -grados);

        // Recorta el paso si chocaría con un organelo o saldría de la membrana
        Vector3 Limitar(Vector3 paso)
        {
            float largo = paso.magnitude;
            if (largo <= 0f) return Vector3.zero;
            var dir = paso / largo;
            int n = Physics.SphereCastNonAlloc(Centro, radioNave, dir, golpes, largo + holgura, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                if (!golpes[i].collider.transform.IsChildOf(mundo)) continue; // la cabina y el jugador no cuentan
                if (golpes[i].distance <= 0f) continue;                     // ya estaba encimado: dejarlo salir
                largo = Mathf.Min(largo, Mathf.Max(0f, golpes[i].distance - holgura));
            }
            if (hayMembrana)
            {
                // Dónde quedaría la nave dentro de la célula (en sus coordenadas): si sale del margen y se aleja, no avanza
                var ahora = Normalizado(mundo.InverseTransformPoint(Centro));
                var despues = Normalizado(mundo.InverseTransformPoint(Centro + dir * largo));
                if (despues.magnitude > 1f && despues.magnitude > ahora.magnitude) largo = 0f;
            }
            if (largo <= 0f) velocidadActual = Vector3.zero;
            return dir * largo;
        }

        Vector3 Normalizado(Vector3 local)
        {
            var e = membranaLocal.extents * margenMembrana;
            var d = local - membranaLocal.center;
            return new Vector3(d.x / Mathf.Max(e.x, 0.01f), d.y / Mathf.Max(e.y, 0.01f), d.z / Mathf.Max(e.z, 0.01f));
        }

        // ---------------- Piloto automático ----------------

        // Gira la nave hacia el organelo y la lleva frente a él, con la viñeta puesta. Lo que pasa en el camino
        // no frena (es un traslado corto y tapado por la viñeta)
        IEnumerator IrA(Pokedex.Descubrible d)
        {
            enAutomatico = true;
            velocidadActual = Vector3.zero;
            Vineta(vinetaEnAutomatico);

            var b = Limites(d.transform);
            var P = Centro;
            var T = b.center;
            var haciaT = T - P;
            float distanciaParada = Mathf.Max(b.extents.magnitude * 0.8f, 0f) + radioNave + 6f;
            float yaw = Vector3.SignedAngle(Plano(haciaT), Plano(cabina.forward), Vector3.up);
            var R = Quaternion.AngleAxis(yaw, Vector3.up);
            // Después del traslado, el organelo queda adelante de la nave, a su altura, a "distanciaParada"
            var t = Plano(cabina.forward) * distanciaParada - R * haciaT;

            var pos0 = mundo.position;
            var rot0 = mundo.rotation;
            float duracion = Mathf.Clamp(haciaT.magnitude / velocidadAutomatico, duracionMinima, duracionMaxima);
            for (float s = 0f; s < duracion; s += Time.deltaTime)
            {
                if (gm.Pausado) { yield return null; s -= Time.deltaTime; continue; }
                float k = Mathf.SmoothStep(0f, 1f, s / duracion);
                var Rk = Quaternion.Slerp(Quaternion.identity, R, k);
                mundo.SetPositionAndRotation(P + Rk * (pos0 - P) + t * k, Rk * rot0);
                Rapidez = Mathf.Sin(k * Mathf.PI);
                yield return null;
            }
            mundo.SetPositionAndRotation(P + R * (pos0 - P) + t, R * rot0);
            Rapidez = 0f;
            Vineta(false);
            enAutomatico = false;
            Debug.Log($"[Pilotaje] Piloto automático: llegó a {d.name}.");
        }

        // ---------------- Brújula ----------------

        void ActualizarBrujula()
        {
            var pokedex = Pokedex.Pokedex.Instancia;
            masCercano = null;
            float mejor = float.MaxValue;
            foreach (var d in descubribles)
            {
                if (d == null || !d.isActiveAndEnabled || (pokedex != null && pokedex.EstaRegistrada(d.ficha))) continue;
                float dist = (Limites(d.transform).center - Centro).sqrMagnitude;
                if (dist < mejor) { mejor = dist; masCercano = d; }
            }
            float metros = masCercano != null ? Mathf.Sqrt(mejor) : 0f;
            Cercania = masCercano != null ? Mathf.Clamp01(1f - metros / 150f) : 0f;

            if (flecha == null) return;
            if (masCercano == null)
            {
                if (textoBrujula != null) textoBrujula.text = "Todo registrado";
                flecha.gameObject.SetActive(false);
                return;
            }
            flecha.gameObject.SetActive(true);
            var dir = Limites(masCercano.transform).center - flecha.position;
            if (dir.sqrMagnitude > 0f) flecha.rotation = Quaternion.LookRotation(dir);
            if (textoBrujula != null) textoBrujula.text = $"Sin registrar · {metros:F0} m\n<size=70%>A: piloto automático</size>";
        }

        // ---------------- Ayudantes ----------------

        void Vineta(bool poner)
        {
            if (controladorVineta == null || poner == vinetaPuesta) return;
            vinetaPuesta = poner;
            if (poner) controladorVineta.BeginTunnelingVignette(this);
            else controladorVineta.EndTunnelingVignette(this);
        }

        static Bounds Limites(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.one);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Límites de la membrana en las coordenadas de la zona (no cambian al mover la célula)
        Bounds LimitesLocales(Transform t)
        {
            var b = Limites(t);
            var c = mundo.InverseTransformPoint(b.center);
            var e = mundo.InverseTransformVector(b.extents);
            return new Bounds(c, new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z)) * 2f);
        }

        static Transform Buscar(Transform raiz, string nombre)
        {
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                if (t.name == nombre && t.gameObject.activeInHierarchy) return t;
            return null;
        }
    }
}
