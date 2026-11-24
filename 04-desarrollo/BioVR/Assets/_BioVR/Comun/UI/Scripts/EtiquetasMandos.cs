using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    // Botones de los mandos pintados y con su nombre encima (prueba del 24-nov: "los jugadores nuevos no saben
    // qué botón es A, B o Y"). Colores como los de un control de Xbox:
    //   derecho:   A verde, B rojo        izquierdo: X azul, Y amarillo
    //   los dos:   GATILLO (dedo índice) celeste, GRIP (dedo medio) naranja
    // Los textos del tutorial usan los mismos colores (Colores.Etiqueta).
    //
    // No hay que ponerlo en la escena: se crea solo al cargar cualquier escena y busca los modelos de mando
    // (UniversalController del XR Interaction Toolkit), que el rig crea cuando el visor detecta los mandos.
    public class EtiquetasMandos : MonoBehaviour
    {
        public static class Colores
        {
            public static readonly Color A = new Color(0.25f, 0.85f, 0.3f);
            public static readonly Color B = new Color(0.95f, 0.25f, 0.25f);
            public static readonly Color X = new Color(0.25f, 0.5f, 1f);
            public static readonly Color Y = new Color(1f, 0.85f, 0.15f);
            public static readonly Color Gatillo = new Color(0.35f, 0.85f, 1f);
            public static readonly Color Grip = new Color(1f, 0.55f, 0.1f);

            // "<b><color=#..>A</color></b>" para los textos de los tutoriales
            public static string Etiqueta(string texto, Color c) => $"<b><color=#{ColorUtility.ToHtmlStringRGB(c)}>{texto}</color></b>";
        }

        const string NombreModelo = "UniversalController";
        const float TamanoLetra = 0.11f;   // TextMeshPro: ≈ 1.1 cm (los botones están a 1.5 cm uno del otro)
        const float TamanoPalabra = 0.1f;  // GATILLO / GRIP

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Registrar()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        static void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (FindFirstObjectByType<EtiquetasMandos>() != null) return;
            var go = new GameObject("Etiquetas de los mandos");
            DontDestroyOnLoad(go);
            go.AddComponent<EtiquetasMandos>();
        }

        class Letrero { public Transform pieza; public Transform modelo; public Vector3 offsetLocal; public TextMeshPro texto; }
        readonly List<Letrero> letreros = new List<Letrero>();
        readonly HashSet<Transform> decorados = new HashSet<Transform>();
        float siguienteBusqueda;
        Transform camara;

        void Update()
        {
            // Al cambiar de escena se destruyen los mandos viejos: se olvidan y se vuelven a buscar
            if (decorados.RemoveWhere(t => t == null) > 0)
                letreros.RemoveAll(l => { if (l.pieza != null) return false; if (l.texto != null) Destroy(l.texto.gameObject); return true; });
            // Los modelos aparecen cuando el visor detecta los mandos (y se recrean al cambiar de escena)
            if (decorados.Count >= 2 || Time.unscaledTime < siguienteBusqueda) return;
            siguienteBusqueda = Time.unscaledTime + 1f;
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (t.name.StartsWith(NombreModelo) && !decorados.Contains(t)) Decorar(t);
        }

        void Decorar(Transform modelo)
        {
            bool? izquierdo = Lado(modelo);
            if (izquierdo == null) return;
            decorados.Add(modelo);
            bool izq = izquierdo.Value;

            var piezas = new Dictionary<string, Transform>();
            foreach (var t in modelo.GetComponentsInChildren<Transform>(true)) piezas.TryAdd(t.name, t);

            // El modelo tiene +Y hacia arriba (los botones están en la cara de arriba) y −Z hacia la punta (el gatillo)
            if (piezas.TryGetValue("Button_A", out var a))
                Poner(modelo, a, izq ? "X" : "A", izq ? Colores.X : Colores.A, TamanoLetra, new Vector3(0f, 0.006f, 0.008f)); // un poco hacia atrás y
            if (piezas.TryGetValue("Button_B", out var b))
                Poner(modelo, b, izq ? "Y" : "B", izq ? Colores.Y : Colores.B, TamanoLetra, new Vector3(0f, 0.006f, -0.01f)); // este hacia la punta: vistas desde atrás se enciman
            // Del lado donde está el grip (el modelo del derecho es el del izquierdo reflejado: escala x = −1)
            float lado = piezas.TryGetValue("Bumper", out var p) && p.localPosition.x > 0f ? 1f : -1f;
            if (p != null)
                Poner(modelo, p, "GRIP", Colores.Grip, TamanoPalabra, new Vector3(lado * 0.025f, -0.005f, 0f));
            // El gatillo va abajo de la punta: visto desde atrás lo tapa el mando. El letrero va del otro lado del grip.
            if (piezas.TryGetValue("Trigger", out var g))
                Poner(modelo, g, "GATILLO", Colores.Gatillo, TamanoPalabra, new Vector3(-lado * 0.045f, 0.022f, -0.015f));
        }

        static bool? Lado(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
            {
                var n = p.name.ToLowerInvariant();
                if (n.Contains("left")) return true;
                if (n.Contains("right")) return false;
            }
            return null;
        }

        void Poner(Transform modelo, Transform pieza, string texto, Color color, float tamano, Vector3 desplazamiento)
        {
            var bloque = new MaterialPropertyBlock();
            foreach (var r in pieza.GetComponentsInChildren<Renderer>(true))
            {
                r.GetPropertyBlock(bloque);
                bloque.SetColor("_BaseColor", color);
                bloque.SetColor("_Color", color);
                bloque.SetColor("_EmissionColor", color * 0.6f);
                r.SetPropertyBlock(bloque);
            }

            var go = new GameObject($"Letrero {texto}");
            go.transform.SetParent(transform, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color32(0, 0, 0, 220);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(0.1f, 0.03f);

            // El desplazamiento se mide desde el centro de la pieza, en los ejes del modelo
            var r0 = pieza.GetComponentInChildren<Renderer>(true);
            var centro = r0 != null ? r0.bounds.center : pieza.position;
            var offsetLocal = modelo.InverseTransformPoint(centro) + desplazamiento;
            letreros.Add(new Letrero { pieza = pieza, modelo = modelo, offsetLocal = offsetLocal, texto = tmp });
        }

        void LateUpdate()
        {
            if (camara == null) camara = Camera.main != null ? Camera.main.transform : null;
            foreach (var l in letreros)
            {
                if (l.pieza == null || l.texto == null) continue;
                bool visible = l.pieza.gameObject.activeInHierarchy;
                if (l.texto.gameObject.activeSelf != visible) l.texto.gameObject.SetActive(visible);
                if (!visible) continue;
                l.texto.transform.position = l.modelo.TransformPoint(l.offsetLocal);
                l.texto.transform.localScale = Vector3.one * Mathf.Abs(l.modelo.lossyScale.y);
                if (camara != null)
                {
                    // De frente a la cámara, con el "arriba" del mundo: se lee aunque el mando esté girado
                    var hacia = l.texto.transform.position - camara.position;
                    if (hacia.sqrMagnitude > 1e-6f) l.texto.transform.rotation = Quaternion.LookRotation(hacia, camara.up);
                }
            }
        }
    }
}
