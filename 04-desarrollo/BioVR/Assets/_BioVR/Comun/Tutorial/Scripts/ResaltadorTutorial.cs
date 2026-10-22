using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    // Resalta los controles que pide el paso actual del tutorial: los hace parpadear y les pone una flecha encima.
    // Cada PasoTutorial dice en "resaltar" los nombres de los objetos de la escena (p. ej. "Boton Salto Matrioska").
    // El resaltado se quita en cuanto el paso se completa ("¡Bien!"), se salta o termina el tutorial.
    //
    // No hay que ponerlo en la escena: se crea solo al cargar cualquier escena que tenga un Tutorial, así nadie
    // tiene que guardar cambios en 00-EscenaInicial. Si alguien lo pone a mano, se usa ese y no se crea otro.
    // Los objetos apagados (p. ej. el panel "Volver a la cabina" mientras el jugador está adentro) también se
    // encuentran; su flecha aparece cuando se encienden.
    public class ResaltadorTutorial : MonoBehaviour
    {
        [Header("Parpadeo")]
        [SerializeField] Color colorResaltado = new Color(1f, 0.85f, 0.1f);

        [Tooltip("Parpadeos por segundo.")]
        [SerializeField] float velocidad = 1.5f;

        [Tooltip("Qué tanto se acerca el color del control al color de resaltado (0 = nada, 1 = todo).")]
        [Range(0f, 1f)] [SerializeField] float intensidad = 0.6f;

        [Header("Flecha")]
        [Tooltip("Opcional: prefab de la flecha, apuntando hacia abajo. Vacío: se usa un rombo del color de resaltado.")]
        [SerializeField] GameObject prefabFlecha;

        [SerializeField] float tamanoFlecha = 0.065f;

        [Tooltip("Metros por encima del control.")]
        [SerializeField] float alturaFlecha = 0.08f;

        [Tooltip("Metros que sube y baja la flecha.")]
        [SerializeField] float rebote = 0.015f;

        static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor"); // URP Lit / Unlit
        static readonly int IdColor = Shader.PropertyToID("_Color");         // shaders viejos

        class Objetivo
        {
            public GameObject go;
            public Renderer[] renderers;
            public Transform flecha;
        }

        readonly List<Objetivo> activos = new();
        readonly Dictionary<string, GameObject> porNombre = new();
        MaterialPropertyBlock mpb;

        // ---- Se crea solo en las escenas con tutorial (también al Reiniciar, que recarga la escena) ----
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Registrar()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        static void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (FindFirstObjectByType<Tutorial>() == null) return;
            if (FindFirstObjectByType<ResaltadorTutorial>() != null) return;
            new GameObject("Resaltador del tutorial").AddComponent<ResaltadorTutorial>();
        }

        void Awake() => mpb = new MaterialPropertyBlock();

        void OnEnable() => Tutorial.OnPasoCambiado += Resaltar;

        void OnDisable()
        {
            Tutorial.OnPasoCambiado -= Resaltar;
            Limpiar();
        }

        // Por si el tutorial ya mostró su paso antes de que este componente existiera
        void Start() => Resaltar(Tutorial.PasoActual);

        void Resaltar(PasoTutorial paso)
        {
            Limpiar();
            if (paso == null || paso.resaltar == null) return;

            foreach (var nombre in paso.resaltar)
            {
                if (string.IsNullOrWhiteSpace(nombre)) continue;
                var go = Buscar(nombre.Trim());
                if (go == null)
                {
                    Debug.LogWarning($"[Tutorial] No encontré \"{nombre}\" en la escena para resaltarlo (paso {paso.name}).", this);
                    continue;
                }
                activos.Add(new Objetivo { go = go, renderers = RenderersDe(go), flecha = CrearFlecha() });
            }
        }

        void Update()
        {
            if (activos.Count == 0) return;

            // Tiempo sin escalar: sigue parpadeando aunque el juego esté en pausa
            float t = (Mathf.Sin(Time.unscaledTime * velocidad * 2f * Mathf.PI) * 0.5f + 0.5f) * intensidad;
            for (int i = activos.Count - 1; i >= 0; i--)
            {
                var o = activos[i];
                if (o.go == null) { Quitar(o); activos.RemoveAt(i); continue; }
                Parpadear(o.renderers, t);
                MoverFlecha(o);
            }
        }

        // ---- Parpadeo: mezcla el color de cada material con el de resaltado, sin tocar los materiales ----
        // Se lee el color del material cada cuadro, así respeta los cambios de color al presionar un botón.
        void Parpadear(Renderer[] renderers, float t)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    int id = IdDeColor(mats[m]);
                    if (id < 0) continue;
                    var original = mats[m].GetColor(id);
                    var c = Color.Lerp(original, colorResaltado, t);
                    c.a = original.a;
                    r.GetPropertyBlock(mpb, m);
                    mpb.SetColor(id, c);
                    r.SetPropertyBlock(mpb, m);
                }
            }
        }

        void Restaurar(Renderer[] renderers)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                int n = r.sharedMaterials.Length;
                for (int m = 0; m < n; m++)
                {
                    mpb.Clear();
                    r.SetPropertyBlock(mpb, m);
                }
            }
        }

        static int IdDeColor(Material mat)
        {
            if (mat == null) return -1;
            if (mat.HasProperty(IdBaseColor)) return IdBaseColor;
            if (mat.HasProperty(IdColor)) return IdColor;
            return -1;
        }

        // ---- Flecha encima del control ----
        Transform CrearFlecha()
        {
            GameObject raiz;
            if (prefabFlecha != null)
            {
                raiz = Instantiate(prefabFlecha, transform);
            }
            else
            {
                // Rombo plano con una punta hacia abajo; la raíz gira sobre Y para que se vea desde cualquier lado
                raiz = new GameObject("Flecha tutorial");
                raiz.transform.SetParent(transform, false);
                var rombo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rombo.name = "Rombo";
                rombo.transform.SetParent(raiz.transform, false);
                rombo.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                rombo.transform.localScale = new Vector3(1f, 1f, 0.35f);
                var r = rombo.GetComponent<Renderer>();
                r.shadowCastingMode = ShadowCastingMode.Off;
                mpb.Clear();
                mpb.SetColor(IdBaseColor, colorResaltado);
                mpb.SetColor(IdColor, colorResaltado);
                r.SetPropertyBlock(mpb);
            }
            // Sin colisiones: que no estorbe al dedo ni a los rayos del mando
            foreach (var c in raiz.GetComponentsInChildren<Collider>(true)) Destroy(c);
            raiz.transform.localScale = Vector3.one * tamanoFlecha;
            raiz.SetActive(false);
            return raiz.transform;
        }

        void MoverFlecha(Objetivo o)
        {
            bool visible = Limites(o, out var b);
            if (o.flecha.gameObject.activeSelf != visible) o.flecha.gameObject.SetActive(visible);
            if (!visible) return;

            float y = b.max.y + alturaFlecha + Mathf.Sin(Time.unscaledTime * 3f) * rebote;
            o.flecha.SetPositionAndRotation(new Vector3(b.center.x, y, b.center.z),
                                            Quaternion.Euler(0f, Time.unscaledTime * 90f, 0f));
        }

        // Caja que ocupan las partes visibles del control; si no tiene ninguna, su posición
        static bool Limites(Objetivo o, out Bounds b)
        {
            b = default;
            if (!o.go.activeInHierarchy) return false;
            bool hay = false;
            foreach (var r in o.renderers)
            {
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!hay) { b = r.bounds; hay = true; }
                else b.Encapsulate(r.bounds);
            }
            if (!hay) b = new Bounds(o.go.transform.position, Vector3.zero);
            return true;
        }

        // ---- Búsqueda por nombre (incluye objetos apagados) ----
        GameObject Buscar(string nombre)
        {
            if (porNombre.TryGetValue(nombre, out var go) && go != null) return go;

            // No estaba o ya se destruyó: volver a leer la escena
            porNombre.Clear();
            var repetidos = new HashSet<string>();
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.IsChildOf(transform)) continue; // nuestras flechas
                if (!porNombre.TryAdd(t.name, t.gameObject)) repetidos.Add(t.name);
            }
            if (repetidos.Contains(nombre))
                Debug.LogWarning($"[Tutorial] Hay varios objetos llamados \"{nombre}\"; se resalta solo uno. Usa un nombre único.", this);
            return porNombre.TryGetValue(nombre, out go) ? go : null;
        }

        // Partes que se pueden pintar: se dejan fuera textos 3D y partículas
        static Renderer[] RenderersDe(GameObject go)
        {
            var lista = new List<Renderer>();
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                if (r.GetComponent<TMP_Text>() != null) continue;
                lista.Add(r);
            }
            return lista.ToArray();
        }

        void Quitar(Objetivo o)
        {
            Restaurar(o.renderers);
            if (o.flecha != null) Destroy(o.flecha.gameObject);
        }

        void Limpiar()
        {
            foreach (var o in activos) Quitar(o);
            activos.Clear();
        }
    }
}
