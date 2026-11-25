using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Marca visual (aro cian pulsante + flecha) que señala en el espacio 3D
    /// el objeto sobre el que habla la ventana activa del tutorial.
    ///
    /// Prueba del 24-nov: si el jugador está volteado hacia otro lado y no ve el objeto, aparece frente a
    /// su cara una flecha amarilla con la palabra VOLTEA que apunta hacia dónde girar. Se esconde sola en
    /// cuanto el objeto entra a la vista.
    /// </summary>
    public class MarcaTutorial : MonoBehaviour
    {
        [SerializeField] private Transform aroCian;
        [SerializeField] private Transform flecha;
        [SerializeField] private float alturaSobreObjetivo = 0.22f;
        [SerializeField] private float velocidadPulso = 4f;
        [SerializeField] private float amplitudEscala = 0.15f;

        // Flecha de "voltea": constantes y no campos serializados, para que el prefab no las deje con valores viejos
        private const float AnguloVisible = 40f;     // giro a los lados más allá del cual se muestra
        private const float AnguloOcultar = 28f;     // y se esconde al bajar de esto (así no parpadea en el borde)
        private const float DistanciaFlecha = 0.6f;  // metros frente a los ojos
        private const float DesvioFlecha = 0.13f;    // metros hacia el lado del objeto
        private static readonly Color Amarillo = new Color(1f, 0.85f, 0.1f);

        // Desde el turno 2 la flecha sale solo como ayuda (prueba del 24-nov: el jugador volteaba por instinto
        // y ya no leía el tutorial, y se trababa). Aparece si algo rompe el flujo (GuiaCocina → PedirAyuda)
        // o si lleva SegundosTrabado en el mismo objetivo sin avanzar. En el turno 1 sale siempre, como antes.
        private const float SegundosTrabado = 20f;
        public static MarcaTutorial Instancia { get; private set; }
        private Transform ultimoObjetivo;
        private float desdeObjetivo;
        private static float ayudaPedida = -100f; // estática: sirve aunque la marca aún no haya despertado
        private bool ayuda;

        private Transform objetivoActual;
        private Vector3 escalaBaseAro;
        private Transform voltea;
        private Transform volteaFlecha;
        private bool volteaVisible;

        private void Awake()
        {
            if (aroCian != null) escalaBaseAro = aroCian.localScale;
            Instancia = this;
            Ocultar();
        }

        public void Senalar(Transform objetivo)
        {
            if (objetivo != ultimoObjetivo)
            {
                // Objetivo nuevo: vuelve a contar el tiempo. Si la ayuda se pidió justo antes (una interrupción
                // trae su propio objetivo), se conserva.
                ultimoObjetivo = objetivo;
                desdeObjetivo = Time.unscaledTime;
                ayuda = Time.unscaledTime - ayudaPedida < 1f;
            }
            objetivoActual = objetivo;
            gameObject.SetActive(objetivo != null);
        }

        // Algo rompió el flujo (equivocación, horquilla, acción rechazada): que salga la flecha si hace falta
        public static void PedirAyuda()
        {
            ayudaPedida = Time.unscaledTime;
            if (Instancia != null) Instancia.ayuda = true;
        }

        private bool FlechaPermitida()
        {
            var c = ControlCocina.Instancia;
            if (c == null || c.NumeroTurno <= 0) return true; // turno 1: siempre
            return ayuda || Time.unscaledTime - desdeObjetivo > SegundosTrabado;
        }

        public void Ocultar()
        {
            objetivoActual = null;
            MostrarVoltea(false);
            gameObject.SetActive(false);
        }

        private void OnDisable() => MostrarVoltea(false);

        private void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
            if (voltea != null) Destroy(voltea.gameObject);
        }

        private void Update()
        {
            if (objetivoActual == null)
            {
                Ocultar();
                return;
            }

            transform.position = objetivoActual.position + Vector3.up * alturaSobreObjetivo;
            PosicionMesa.MirarAlJugador(transform);

            float pulso = 1f + Mathf.Sin(Time.unscaledTime * velocidadPulso) * amplitudEscala;
            if (aroCian != null)
                aroCian.localScale = escalaBaseAro * pulso;

            if (flecha != null)
                flecha.localPosition = new Vector3(0f, 0.08f + Mathf.Sin(Time.unscaledTime * velocidadPulso) * 0.02f, 0f);
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (objetivoActual == null || cam == null || !FlechaPermitida()) { MostrarVoltea(false); return; }

            // Solo cuenta el giro a los lados (no si el objeto está más abajo, en la mesa): la flecha es para
            // quien está volteado hacia otro lado. Si el objeto queda atrás, se gira por el lado más corto.
            var ojos = cam.transform;
            var frente = Vector3.ProjectOnPlane(ojos.forward, Vector3.up);
            var hacia = Vector3.ProjectOnPlane(transform.position - ojos.position, Vector3.up);
            if (frente.sqrMagnitude < 1e-6f || hacia.sqrMagnitude < 1e-6f) { MostrarVoltea(false); return; }
            float giro = Vector3.SignedAngle(frente, hacia, Vector3.up); // + = a la derecha
            if (Mathf.Abs(giro) > AnguloVisible) MostrarVoltea(true);
            else if (Mathf.Abs(giro) < AnguloOcultar) MostrarVoltea(false);
            if (!volteaVisible) return;

            float signo = giro >= 0f ? 1f : -1f;
            var plano = Quaternion.LookRotation(frente.normalized, Vector3.up);
            voltea.position = ojos.position + frente.normalized * DistanciaFlecha + plano * Vector3.right * (signo * DesvioFlecha) + Vector3.down * 0.05f;
            voltea.rotation = plano;
            // La flecha (su +X) apunta hacia el lado del objeto, y late hacia allá
            volteaFlecha.localRotation = signo > 0f ? Quaternion.identity : Quaternion.Euler(0f, 0f, 180f);
            volteaFlecha.localPosition = new Vector3(signo * Mathf.Sin(Time.unscaledTime * 6f) * 0.015f, 0f, 0f);
        }

        private void MostrarVoltea(bool mostrar)
        {
            if (mostrar && voltea == null) CrearVoltea();
            volteaVisible = mostrar && voltea != null;
            if (voltea != null && voltea.gameObject.activeSelf != volteaVisible) voltea.gameObject.SetActive(volteaVisible);
        }

        // Flecha hecha de cubos (cuerpo + punta en rombo) con una copia amarilla del material de la marca
        // (ese material ya viaja en el APK; Shader.Find no sirve en el visor) y la palabra VOLTEA.
        private void CrearVoltea()
        {
            // UI/Default va siempre en el APK (Graphics → Always Included Shaders) y deja dibujar encima de todo
            // (ZTest Always): si no, cualquier cosa más cerca que la flecha (la sintetasa, la mesa) la tapa.
            Material mat;
            var ui = Shader.Find("UI/Default");
            if (ui != null)
            {
                mat = new Material(ui) { color = Amarillo, renderQueue = 4000 };
                mat.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);
            }
            else
            {
                var r = flecha != null ? flecha.GetComponentInChildren<Renderer>(true) : null;
                if (r == null && aroCian != null) r = aroCian.GetComponentInChildren<Renderer>(true);
                if (r == null || r.sharedMaterial == null) return;
                mat = new Material(r.sharedMaterial);
                foreach (var p in new[] { "_BaseColor", "_Color" }) if (mat.HasProperty(p)) mat.SetColor(p, Amarillo);
                if (mat.HasProperty("_EmissionColor")) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Amarillo * 0.8f); }
            }

            voltea = new GameObject("Flecha voltea (tutorial)").transform;
            volteaFlecha = new GameObject("Flecha").transform;
            volteaFlecha.SetParent(voltea, false);
            // Apunta a +X: cuerpo horizontal y punta en chevrón (dos barras que se juntan en la punta)
            Cubo("Cuerpo", volteaFlecha, new Vector3(-0.015f, 0f, 0f), new Vector3(0.09f, 0.016f, 0.008f), Quaternion.identity, mat);
            Cubo("Punta arriba", volteaFlecha, new Vector3(0.017f, 0.017f, 0f), new Vector3(0.05f, 0.016f, 0.008f), Quaternion.Euler(0f, 0f, -45f), mat);
            Cubo("Punta abajo", volteaFlecha, new Vector3(0.017f, -0.017f, 0f), new Vector3(0.05f, 0.016f, 0.008f), Quaternion.Euler(0f, 0f, 45f), mat);

            var texto = new GameObject("Texto").AddComponent<TextMeshPro>();
            texto.transform.SetParent(voltea, false);
            texto.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            texto.text = "VOLTEA";
            texto.fontSize = 0.32f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            texto.color = Amarillo;
            texto.outlineWidth = 0.25f;
            texto.outlineColor = new Color32(0, 0, 0, 220);
            texto.rectTransform.sizeDelta = new Vector2(0.2f, 0.05f);
            var mt = texto.fontMaterial; // copia propia: no cambia el resto de los textos
            mt.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);
            mt.renderQueue = 4001;
            voltea.localScale = Vector3.one * 1.4f;
            voltea.gameObject.SetActive(false);
        }

        private static void Cubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Quaternion rot, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nombre;
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(padre, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = escala;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
