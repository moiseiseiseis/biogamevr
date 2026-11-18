using System.Linq;
using BioVR.Jugador;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Arma la escena base de la Cocina: Escenas/Pruebas/Cocina_Base.unity (menú BioVR → Cocina → Armar escena base).
    // Cada quien la copia como Cocina_MX.unity para probar su módulo. Nadie edita Cocina_Base a mano:
    // si hace falta algo en todas, se agrega aquí y se vuelve a correr.
    //
    //   - Jugador sentado: XR Origin de los Starter Assets sin locomoción
    //   - AnclaCocina: el piso debajo del asiento, +Z al frente. Se acomoda a la cabeza real al iniciar
    //     (AnclarAlJugador), igual que la estación de la fase A. Recentrar con B / Y
    //   - La mesa curva de Uli y una marca vacía (PuntoCocina) en cada posición de la tabla 1.2
    //   - ControlCocina con los datos de Cocina/Datos (los crea si faltan)
    public static class ArmarCocinaBase
    {
        public const string Escena = "Assets/_BioVR/Escenas/Pruebas/Cocina_Base.unity";
        const string PrefabOrigen = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string ModeloMesa = CrearDatosCocina.Raiz + "/Modelos/Uli/mesa-curva.fbx";
        static readonly string[] Locomocion = { "Turn", "Move", "Grab Move", "Teleportation", "Climb", "Gravity", "Jump" };

        // Ojos del jugador sentado con los que se arma todo (especificación 1.2: ~1.2 m)
        const float AlturaOjos = 1.2f;

        // (nombre, ángulo°, radio m, altura m, inclinación° hacia el jugador, escala, ¿panel?, nota)
        static readonly (string, float, float, float, float, float, bool, string)[] Puntos =
        {
            ("Ribosoma de mesa",      0f,  0.62f, 0.86f, 12.6f, 1f,   false, "M4. Inclinado 0.22 rad hacia el jugador, con un pie debajo"),
            ("Ribosoma lejano",     -17f,  1.90f, 1.30f, 31.5f, 2.1f, false, "M4. Escala ×2.1, inclinado 0.55 rad. Plataforma a 1.04 m con anillo magenta"),
            ("Corriente inicio",     66f,  0.72f, 1.06f, 0f,    1f,   false, "M2. La corriente va de +66° a +14°, radio 0.72, altura 1.06 ± 0.035. Tubo magenta de 0.06 m de radio"),
            ("Corriente fin",        14f,  0.72f, 1.06f, 0f,    1f,   false, "M2. Fin de la corriente"),
            ("Sintetasa",            46f,  0.66f, 0.88f, 0f,    0.75f,false, "M3. Modelo ×0.75; base-sintetasa.fbx debajo, a 0.78 m"),
            ("Salida",              -30f,  0.52f, 0.78f, 0f,    1f,   false, "M5. bandeja-salida.fbx"),
            ("Regreso al citoplasma",-36f, 1.60f, 1.68f, 0f,    1f,   false, "M5. Embudo con aro naranja"),
            ("Holograma",           -47f,  0.85f, 0.78f, 0f,    1f,   false, "M7. proyector-holograma.fbx"),
            ("Chaperona (BiP)",      30f,  0.50f, 0.78f, 0f,    1f,   false, "M10. Solo en la fase B"),
            ("Proteasoma (26S)",    -30f,  0.50f, 0.78f, 0f,    1f,   false, "M10. Solo en la fase B, donde estaba la salida"),
            ("Panel de mensajes",    38f,  1.45f, 1.64f, 0f,    1f,   true,  "M8. Panel de 0.70 × 0.34 m. Orientado para un Canvas (su +Z se aleja del jugador)"),
            ("Tablero del pedido",    4f,  2.00f, 1.95f, 0f,    1f,   true,  "M8. Panel de 0.80 × 0.26 m sobre tablero-pedido.fbx. Orientado para un Canvas"),
        };

        [MenuItem("BioVR/Cocina/Armar escena base")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Object.DestroyImmediate(GameObject.Find("Main Camera"));
            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            // Los datos se cargan DESPUÉS de crear la escena (al crearla, Unity descarga los assets que nadie usa)
            CrearDatosCocina.Crear();
            var config = AssetDatabase.LoadAssetAtPath<ConfigCocina>(CrearDatosCocina.RutaConfig);
            var codigo = AssetDatabase.LoadAssetAtPath<CodigoGenetico>(CrearDatosCocina.RutaCodigo);
            var textos = AssetDatabase.LoadAssetAtPath<TextosCocina>(CrearDatosCocina.RutaTextos);
            var turnos = CrearDatosCocina.RutasTurnos.Select(AssetDatabase.LoadAssetAtPath<TurnoData>).ToArray();

            // ---- Jugador sentado ----
            var origen = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigen), escena);
            if (origen == null) { Debug.LogError("[Cocina] No se encontró el XR Origin de los Starter Assets."); return; }
            foreach (var t in origen.GetComponentsInChildren<Transform>(true))
                if ((Locomocion.Contains(t.name) && t.parent != null && t.parent.name == "Locomotion") || t.name == "Teleport Interactor")
                    t.gameObject.SetActive(false);

            var piso = GameObject.CreatePrimitive(PrimitiveType.Plane);
            piso.name = "Piso";
            piso.GetComponent<Renderer>().sharedMaterial = Material("Piso", new Color(0.16f, 0.11f, 0.27f));

            // ---- AnclaCocina: origen de todas las posiciones (PosicionMesa) ----
            var ancla = new GameObject("AnclaCocina");
            var puntoOjos = new GameObject("Punto ojos").transform;
            puntoOjos.SetParent(ancla.transform, false);
            puntoOjos.localPosition = new Vector3(0f, AlturaOjos, 0f);

            Mesa(ancla.transform);

            var matMarca = MaterialMarca();
            var puntos = new GameObject("Puntos").transform;
            puntos.SetParent(ancla.transform, false);
            foreach (var (nombre, angulo, radio, altura, inclinacion, escala, panel, nota) in Puntos)
            {
                var p = new GameObject(nombre);
                p.transform.SetParent(puntos, false);
                PosicionMesa.Colocar(p.transform, angulo, radio, altura, panel);
                if (inclinacion != 0f) p.transform.localRotation *= Quaternion.Euler(inclinacion, 0f, 0f);
                p.transform.localScale = Vector3.one * escala;
                var marca = p.AddComponent<PuntoCocina>();
                marca.nota = $"{angulo}° · {radio} m · {altura} m de alto. {nota}";
                if (panel) marca.color = new Color(0.5f, 0.9f, 1f);
                marca.material = matMarca;
                // La BiP y el proteasoma ocupan el lugar de la sintetasa y la salida en la fase B: su nombre va más arriba
                if (nombre.StartsWith("Chaperona") || nombre.StartsWith("Proteasoma"))
                {
                    marca.subirEtiqueta = 0.07f;
                    marca.detalle = "fase B";
                }
            }

            // ---- ControlCocina ----
            var control = new GameObject("ControlCocina").AddComponent<ControlCocina>();
            control.transform.SetParent(ancla.transform, false);
            var so = new SerializedObject(control);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("codigo").objectReferenceValue = codigo;
            so.FindProperty("textos").objectReferenceValue = textos;
            var lista = so.FindProperty("turnos");
            lista.arraySize = turnos.Length;
            for (int i = 0; i < turnos.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = turnos[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // ---- Anclaje a la cabeza y recentrado (B / Y) ----
            var xrOrigin = origen.GetComponent<XROrigin>();
            var anclar = origen.AddComponent<AnclarAlJugador>();
            var anso = new SerializedObject(anclar);
            anso.FindProperty("origen").objectReferenceValue = xrOrigin;
            anso.FindProperty("estacion").objectReferenceValue = ancla;
            anso.FindProperty("puntoOjos").objectReferenceValue = puntoOjos;
            anso.ApplyModifiedPropertiesWithoutUndo();
            var recentrar = origen.AddComponent<Recentrar>();
            recentrar.xrOrigin = xrOrigin;
            recentrar.puntoOjos = puntoOjos;

            ancla.SetActive(false); // la enciende AnclarAlJugador cuando ya sabe dónde está la cabeza

            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError("[Cocina] No se pudo guardar la escena."); return; }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Cocina] Listo: {Escena}. Cópiala como Cocina_MX.unity para tu módulo.");
        }

        // La mesa curva de Uli (150°, 0.78 m de alto). Se gira para que su centro quede al frente (+Z),
        // venga como venga del FBX. Si falta el modelo, una caja provisional.
        static void Mesa(Transform padre)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModeloMesa);
            if (fbx == null)
            {
                Debug.LogWarning($"[Cocina] Falta {ModeloMesa}: se pone una mesa provisional.");
                var caja = GameObject.CreatePrimitive(PrimitiveType.Cube);
                caja.name = "Mesa (provisional)";
                caja.transform.SetParent(padre, false);
                caja.transform.localPosition = new Vector3(0f, PosicionMesa.AlturaMesa / 2f, 0.63f);
                caja.transform.localScale = new Vector3(1.6f, PosicionMesa.AlturaMesa, 0.7f);
                return;
            }
            var mesa = (GameObject)PrefabUtility.InstantiatePrefab(fbx, padre);
            mesa.name = "Mesa curva";
            mesa.transform.localPosition = Vector3.zero;
            var b = Limites(mesa);
            float giro = Mathf.Atan2(b.center.x, b.center.z) * Mathf.Rad2Deg;
            mesa.transform.localRotation = Quaternion.Euler(0f, -giro, 0f) * mesa.transform.localRotation;
            b = Limites(mesa);
            Debug.Log($"[Cocina] Mesa curva: centro {b.center:F2}, tamaño {b.size:F2} (se giró {-giro:F0}° para quedar al frente).");
        }

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Unlit translúcido para las esferas que PuntoCocina muestra en Play (el color lo pone cada marca)
        static Material MaterialMarca()
        {
            string ruta = CrearDatosCocina.Raiz + "/Materiales/Cocina_Marca.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            CrearDatosCocina.Carpeta(CrearDatosCocina.Raiz + "/Materiales");
            m = PuntoCocina.Transparente(new Material(Shader.Find("Universal Render Pipeline/Unlit")));
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }

        static Material Material(string nombre, Color color)
        {
            string carpeta = CrearDatosCocina.Raiz + "/Materiales";
            CrearDatosCocina.Carpeta(carpeta);
            string ruta = $"{carpeta}/Cocina_{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }
    }
}
