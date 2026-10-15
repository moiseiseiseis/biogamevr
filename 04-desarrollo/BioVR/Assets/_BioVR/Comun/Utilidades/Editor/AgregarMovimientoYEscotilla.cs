using System.Linq;
using BioVR.Cabina;
using BioVR.Jugador;
using BioVR.Secuencia;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Comun.Editor
{
    // Deja caminar al jugador y salir de la cabina por la escotilla (solo en el Hangar):
    //   XR Origin   → prende Move (stick izquierdo, 1.2 m/s, sin gravedad) y Turn (stick derecho, de 45° en 45°)
    //   Cabina      → colisiones invisibles: 4 paredes y la parte baja de la consola (abajo de los botones,
    //                 para no estorbar al rayo ni a la mano); el asiento no lleva, porque ahí empieza el jugador
    //               → INT_Escotilla interactuable + letrero "Salir", panel "Volver a la cabina" afuera,
    //                 "Punto afuera" y el componente Escotilla
    //   Hangar      → colisiones en las paredes, la nave y el explorador
    // Se puede correr varias veces. PonerModelosDiseno lo vuelve a correr al final, porque reemplaza los modelos.
    // Menú: BioVR → Agregar movimiento y escotilla
    public static class AgregarMovimientoYEscotilla
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string RutaHangar = "Assets/_BioVR/Entornos/Hangar/Hangar.prefab";
        const string RutaMaterialPanel = "Assets/_BioVR/Cabina/Materiales/Escotilla_Panel.mat";
        const float VelocidadCaminar = 1.2f; // m/s: la cabina mide 4 × 5 m
        const float Grosor = 0.1f;           // de las paredes invisibles
        const float AlturaConsola = 0.6f;    // los botones de la consola empiezan a 0.71 m

        static readonly string[] Paredes = { "Pared izquierda", "Pared derecha", "Pared trasera", "Pared frontal" };

        [MenuItem("BioVR/Agregar movimiento y escotilla")]
        public static void Agregar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            var cabina = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "Cabina (provisional)");
            var modelo = cabina != null ? cabina.transform.Find("Modelo Cabina de Diseño") : null;
            var recentrar = Object.FindFirstObjectByType<Recentrar>();
            if (origen == null || modelo == null || recentrar == null)
            {
                Debug.LogError("[Movimiento] Faltan el XR Origin, el 'Modelo Cabina de Diseño' (corre Poner modelos de Diseño) o Recentrar.");
                return;
            }

            PrenderLocomocion(origen);

            var estructura = Limites(Buscar(modelo, "CAB_Estructura"));
            var consola = Limites(Buscar(modelo, "CAB_Consola"));
            Colisiones(cabina.transform, estructura, consola);
            PonerEscotilla(cabina.transform, modelo, estructura, recentrar);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);

            ColisionesHangar();
            Debug.Log("[Movimiento] Listo: stick izquierdo camina, stick derecho gira; escotilla trasera para salir al hangar.");
        }

        // ---------------- XR Origin ----------------
        static void PrenderLocomocion(XROrigin origen)
        {
            var locomocion = origen.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Locomotion");
            if (locomocion == null) { Debug.LogError("[Movimiento] El XR Origin no tiene 'Locomotion'."); return; }
            foreach (Transform hijo in locomocion)
            {
                if (hijo.name != "Move" && hijo.name != "Turn") continue;
                hijo.gameObject.SetActive(true);
                if (hijo.name != "Move") continue;
                foreach (var comp in hijo.GetComponents<MonoBehaviour>())
                {
                    var so = new SerializedObject(comp);
                    var velocidad = so.FindProperty("m_MoveSpeed");
                    if (velocidad == null) continue;
                    velocidad.floatValue = VelocidadCaminar;
                    var gravedad = so.FindProperty("m_UseGravity"); // sin gravedad: no hay piso con colisión
                    if (gravedad != null) gravedad.boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        // ---------------- Cabina ----------------
        static void Colisiones(Transform cabina, Bounds b, Bounds consola)
        {
            var grupo = Nuevo("Colisiones (movimiento)", cabina);
            float alto = b.size.y, y = b.center.y;
            Caja("Pared izquierda", grupo, new Vector3(b.min.x + Grosor / 2, y, b.center.z), new Vector3(Grosor, alto, b.size.z));
            Caja("Pared derecha", grupo, new Vector3(b.max.x - Grosor / 2, y, b.center.z), new Vector3(Grosor, alto, b.size.z));
            Caja("Pared trasera", grupo, new Vector3(b.center.x, y, b.min.z + Grosor / 2), new Vector3(b.size.x, alto, Grosor));
            Caja("Pared frontal", grupo, new Vector3(b.center.x, y, b.max.z - Grosor / 2), new Vector3(b.size.x, alto, Grosor));
            Caja("Consola", grupo, new Vector3(consola.center.x, consola.min.y + AlturaConsola / 2, consola.center.z),
                 new Vector3(consola.size.x, AlturaConsola, consola.size.z));
        }

        static void PonerEscotilla(Transform cabina, Transform modelo, Bounds estructura, Recentrar recentrar)
        {
            // Botón de adentro: la escotilla del modelo
            var escotilla = Buscar(modelo, "INT_Escotilla");
            if (escotilla == null) { Debug.LogError("[Movimiento] El modelo de la cabina no tiene INT_Escotilla."); return; }
            if (escotilla.GetComponent<BoxCollider>() == null) escotilla.gameObject.AddComponent<BoxCollider>(); // se ajusta a la malla
            var salir = escotilla.GetComponent<XRSimpleInteractable>();
            if (salir == null) salir = escotilla.gameObject.AddComponent<XRSimpleInteractable>();
            var be = Limites(escotilla);

            // Letrero arriba de la escotilla, para leerse desde adentro (mirando hacia atrás)
            var letrero = Letrero("Letrero Salir", cabina, "Salir al hangar",
                                  new Vector3(be.center.x, be.max.y + 0.12f, be.max.z + 0.02f), Quaternion.Euler(0f, 180f, 0f),
                                  new Vector2(1.1f, 0.2f));

            // Panel de afuera, en la pared izquierda cerca de la escotilla, de cara a quien está afuera (-X).
            // El interactable va en el padre y usa el collider de la placa.
            var panel = Nuevo("Panel volver a la cabina", cabina).gameObject;
            panel.transform.SetPositionAndRotation(new Vector3(estructura.min.x - 0.03f, 1.3f, estructura.min.z + 0.9f),
                                                   Quaternion.Euler(0f, 90f, 0f)); // su +Z apunta a la cabina
            var placa = GameObject.CreatePrimitive(PrimitiveType.Cube); // trae su BoxCollider
            placa.name = "Placa";
            placa.transform.SetParent(panel.transform, false);
            placa.transform.localScale = new Vector3(0.5f, 0.3f, 0.04f);
            placa.GetComponent<Renderer>().sharedMaterial = MaterialPanel();
            Letrero("Texto", panel.transform, "Volver a la cabina",
                    panel.transform.TransformPoint(0f, 0f, -0.03f), panel.transform.rotation, // un poco afuera de la placa
                    new Vector2(0.46f, 0.26f));
            panel.AddComponent<XRSimpleInteractable>();

            // Dónde aparece al salir: a un lado de la cabina, mirando hacia la nave (+Z)
            var afuera = Nuevo("Punto afuera", cabina);
            afuera.position = new Vector3(estructura.min.x - 1.2f, 0f, estructura.min.z + 0.6f);
            afuera.rotation = Quaternion.identity;

            var go = Nuevo("Escotilla", cabina).gameObject;
            var comp = go.AddComponent<Escotilla>();
            var so = new SerializedObject(comp);
            so.FindProperty("botonSalir").objectReferenceValue = salir;
            so.FindProperty("botonEntrar").objectReferenceValue = panel.GetComponent<XRSimpleInteractable>();
            so.FindProperty("letreroSalir").objectReferenceValue = letrero;
            so.FindProperty("puntoAfuera").objectReferenceValue = afuera;
            so.FindProperty("jugador").objectReferenceValue = recentrar;
            so.FindProperty("fader").objectReferenceValue = Object.FindFirstObjectByType<ScreenFader>();
            so.ApplyModifiedPropertiesWithoutUndo();
            panel.SetActive(false); // solo se ve cuando el jugador está afuera
        }

        // ---------------- Hangar ----------------
        static void ColisionesHangar()
        {
            var raiz = PrefabUtility.LoadPrefabContents(RutaHangar);
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
            {
                bool pared = Paredes.Contains(t.name);
                bool modelo = t.GetComponent<MeshFilter>() != null &&
                              (t.GetComponentsInParent<Transform>(true).Any(p => p.name == "Modelo Explorador") || t.name == "Casco");
                if ((pared || modelo) && t.GetComponent<Collider>() == null) t.gameObject.AddComponent<BoxCollider>();
            }
            PrefabUtility.SaveAsPrefabAsset(raiz, RutaHangar);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        // ---------------- Utilidades ----------------
        static Transform Nuevo(string nombre, Transform padre)
        {
            Reemplazar(padre, nombre);
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            return go.transform;
        }

        static void Reemplazar(Transform padre, string nombre)
        {
            var viejo = padre.Find(nombre);
            if (viejo != null) Object.DestroyImmediate(viejo.gameObject);
        }

        static void Caja(string nombre, Transform padre, Vector3 centro, Vector3 tamano)
        {
            var go = new GameObject(nombre, typeof(BoxCollider));
            go.transform.SetParent(padre, false);
            go.transform.position = centro;
            go.GetComponent<BoxCollider>().size = tamano;
        }

        // Texto 3D de TextMeshPro; "tamano" es el área en metros y la letra se ajusta para caber
        static GameObject Letrero(string nombre, Transform padre, string texto, Vector3 posicion, Quaternion rotacion, Vector2 tamano)
        {
            Reemplazar(padre, nombre);
            var go = new GameObject(nombre, typeof(TextMeshPro));
            go.transform.SetParent(padre, false);
            go.transform.SetPositionAndRotation(posicion, rotacion);
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.2f;
            tmp.fontSizeMax = 1.2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.85f, 0.3f);
            tmp.rectTransform.sizeDelta = tamano;
            return go;
        }

        static Material MaterialPanel()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterialPanel);
            if (mat != null) return mat;
            if (!AssetDatabase.IsValidFolder("Assets/_BioVR/Cabina/Materiales"))
                AssetDatabase.CreateFolder("Assets/_BioVR/Cabina", "Materiales");
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.1f, 0.35f, 0.6f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.05f, 0.3f, 0.6f));
            AssetDatabase.CreateAsset(mat, RutaMaterialPanel);
            return mat;
        }

        static Transform Buscar(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);

        static Bounds Limites(Transform t)
        {
            var rends = t != null ? t.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            if (rends.Length == 0) return new Bounds(t != null ? t.position : Vector3.zero, Vector3.zero);
            var b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
