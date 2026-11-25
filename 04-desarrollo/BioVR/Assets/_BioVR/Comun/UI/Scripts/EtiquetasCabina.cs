using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    // Letreros sobre los controles de la cabina (prueba del 23-nov: "poner indicadores de los botones en la cabina,
    // por ejemplo botón Matrioska a la derecha"). Cada letrero flota arriba de su control y gira para verse de frente.
    //
    // No hay que ponerlo en la escena: se crea solo al cargar una escena que tenga alguno de estos controles
    // (como ResaltadorTutorial). Para agregar un letrero, una línea más en Letreros con el nombre exacto del objeto.
    public class EtiquetasCabina : MonoBehaviour
    {
        static readonly (string objeto, string texto)[] Letreros =
        {
            ("Boton Iniciar inmersion", "INICIAR\nINMERSIÓN"),
            ("Boton Salto Matrioska", "SALTO\nMATRIOSKA"),
            ("Palanca", "PALANCA\n<size=70%>salto Matrioska</size>"),
            ("INT_Escotilla", "ESCOTILLA\n<size=70%>salir al hangar</size>"),
        };

        const float Altura = 0.09f;      // m arriba del control
        const float Tamano = 0.28f;      // tamaño de letra de TextMeshPro (en metros ≈ tamaño/10)
        static readonly Color Color = new Color(0.55f, 0.95f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Registrar()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        static void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (FindFirstObjectByType<EtiquetasCabina>() != null) return;
            if (FindFirstObjectByType<BioVR.Secuencia.GameManager>() == null) return; // solo en la escena del viaje
            new GameObject("Etiquetas de la cabina").AddComponent<EtiquetasCabina>();
        }

        class Letrero { public Transform control; public TextMeshPro texto; }
        readonly List<Letrero> letreros = new List<Letrero>();
        Transform camara;

        void Start()
        {
            var porNombre = new Dictionary<string, Transform>();
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                porNombre.TryAdd(t.name, t);
            foreach (var (objeto, texto) in Letreros)
            {
                if (!porNombre.TryGetValue(objeto, out var control)) continue;
                var go = new GameObject($"Letrero {objeto}");
                go.transform.SetParent(transform, false);
                var tmp = go.AddComponent<TextMeshPro>();
                tmp.text = texto;
                tmp.fontSize = Tamano;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Bottom;
                tmp.color = Color;
                tmp.outlineWidth = 0.2f;
                tmp.outlineColor = new Color32(0, 0, 0, 200);
                tmp.rectTransform.sizeDelta = new Vector2(0.4f, 0.1f);
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                letreros.Add(new Letrero { control = control, texto = tmp });
            }
        }

        void LateUpdate()
        {
            if (camara == null) camara = Camera.main != null ? Camera.main.transform : null;
            foreach (var l in letreros)
            {
                if (l.control == null) { l.texto.gameObject.SetActive(false); continue; }
                bool visible = l.control.gameObject.activeInHierarchy;
                if (l.texto.gameObject.activeSelf != visible) l.texto.gameObject.SetActive(visible);
                if (!visible) continue;
                var arriba = Arriba(l.control);
                l.texto.transform.position = arriba + Vector3.up * Altura;
                if (camara != null)
                {
                    var hacia = l.texto.transform.position - camara.position;
                    hacia.y = 0f;
                    if (hacia.sqrMagnitude > 1e-6f) l.texto.transform.rotation = Quaternion.LookRotation(hacia, Vector3.up);
                }
            }
        }

        // Punto más alto del control (centro de su caja visible)
        static Vector3 Arriba(Transform t)
        {
            bool hay = false;
            var b = new Bounds(t.position, Vector3.zero);
            foreach (var r in t.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer || r.GetComponent<TMP_Text>() != null) continue;
                if (!hay) { b = r.bounds; hay = true; } else b.Encapsulate(r.bounds);
            }
            return new Vector3(b.center.x, b.max.y, b.center.z);
        }
    }
}
