using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M5 (Iker, rol 3) · Menú BioVR → Cocina → Armar prueba M5.
    //
    //   1. Importa de 03-diseno/exports/minijuego-cocina/props/ los modelos de Uli bandeja-salida.fbx y
    //      regreso-citoplasma.fbx a Cocina/Modelos/Uli/, sin cámaras ni luces.
    //   2. Arma Prefabs/Cocina_Salida.prefab (bandeja ×(0.95, 0.8, 1.05), fila de lugares y letrero) y
    //      Prefabs/Cocina_Regreso.prefab (embudo 0.46 m bajo la marca, aro naranja de 0.2 m y letrero).
    //   3. Abre Escenas/Pruebas/Cocina_M5.unity (copia de Cocina_M4), pone la salida y el regreso en sus
    //      marcas, apaga el reciclaje automático de PruebaRibosomas y agrega la prueba de M5 (PruebaSalida).
    //
    // Se puede correr varias veces: vuelve a generar los prefabs y quita de la escena lo que puso antes.
    public static class ArmarPruebaM5
    {
        const string Raiz = CrearDatosCocina.Raiz;
        const string CarpetaPrefabs = Raiz + "/Prefabs";
        const string CarpetaUli = Raiz + "/Modelos/Uli";
        const string CarpetaMateriales = Raiz + "/Materiales";
        const string Escena = "Assets/_BioVR/Escenas/Pruebas/Cocina_M5.unity";
        const string ExportsProps = "../../../03-diseno/exports/minijuego-cocina/props/"; // desde Assets/

        // Medidas (especificación 6.2)
        static readonly Vector3 EscalaBandeja = new Vector3(0.95f, 0.8f, 1.05f);
        const float SobreBandeja = 0.02f;        // los ARNt acostados quedan 2 cm sobre la bandeja
        const float BajoRegreso = 0.46f;         // el embudo queda 0.46 m bajo la marca
        const float RadioAro = 0.2f;

        static readonly Color Naranja = new Color(1f, 0.55f, 0.1f);

        [MenuItem("BioVR/Cocina/Armar prueba M5")]
        public static void Armar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(Escena))
            {
                Debug.LogError($"[M5] No existe {Escena}. Duplica Escenas/Pruebas/Cocina_M4 (Ctrl+D) y renómbrala Cocina_M5.");
                return;
            }
            CrearDatosCocina.Carpeta(CarpetaPrefabs);
            CrearDatosCocina.Carpeta(CarpetaUli);
            CrearDatosCocina.Carpeta(CarpetaMateriales);

            // Escena vacía para armar los prefabs sin ensuciar ninguna escena
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            string bandeja = Importar("bandeja-salida.fbx");
            string regreso = Importar("regreso-citoplasma.fbx");

            var salida = Salida(bandeja);
            var destino = Regreso(regreso);

            Colocar(salida, destino);
        }

        // =============================================================================================
        // Salida
        // =============================================================================================

        static GameObject Salida(string fbx)
        {
            var raiz = new GameObject("Cocina_Salida");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(raiz.transform, false);

            var modelo = Modelo(fbx, visual, "Bandeja");
            float arriba = 0.03f, fondo = 0.06f;
            if (modelo != null)
            {
                modelo.localScale = Vector3.Scale(modelo.localScale, EscalaBandeja);
                var b = Limites(modelo.gameObject);
                arriba = b.max.y;
                fondo = b.extents.z;
                Debug.Log($"[M5] Bandeja: {b.size.x:F3} × {b.size.y:F3} × {b.size.z:F3} m (ancho × alto × fondo).");
                if (b.size.x < b.size.z) Debug.LogWarning("[M5] La bandeja es más profunda que ancha: revisa si hay que girarla 90° para que la fila de lugares quede a lo largo.");
            }

            var centro = new GameObject("Lugares (centro de la fila)").transform;
            centro.SetParent(raiz.transform, false);
            centro.localPosition = new Vector3(0f, arriba + SobreBandeja, 0f);

            // Letrero detrás de la bandeja (lado contrario al jugador), de cara al jugador
            var letrero = Texto("Letrero", visual, "SALIDA · usados (máx. 2)", 0.35f, new Vector2(0.45f, 0.06f),
                new Vector3(0f, arriba + 0.09f, -fondo - 0.02f));

            var comp = raiz.AddComponent<BandejaSalida>();
            var so = new SerializedObject(comp);
            so.FindProperty("visual").objectReferenceValue = visual.gameObject;
            so.FindProperty("centro").objectReferenceValue = centro;
            so.FindProperty("letrero").objectReferenceValue = letrero;
            so.ApplyModifiedPropertiesWithoutUndo();

            return Guardar(raiz, "Cocina_Salida");
        }

        // =============================================================================================
        // Regreso al citoplasma
        // =============================================================================================

        static GameObject Regreso(string fbx)
        {
            var raiz = new GameObject("Cocina_Regreso");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(raiz.transform, false);

            var modelo = Modelo(fbx, visual, "Embudo");
            float boca = 0f;
            if (modelo != null)
            {
                modelo.localPosition = new Vector3(0f, -BajoRegreso, 0f);
                var b = Limites(modelo.gameObject);
                boca = b.max.y;
                Debug.Log($"[M5] Regreso: {b.size.x:F3} × {b.size.y:F3} × {b.size.z:F3} m; boca a {boca:F3} m de la marca.");
            }

            var puntoBoca = new GameObject("Boca").transform;
            puntoBoca.SetParent(raiz.transform, false);
            puntoBoca.localPosition = new Vector3(0f, boca, 0f);

            // Aro naranja de 0.2 m de radio en la boca
            var aro = new GameObject("Aro").transform;
            aro.SetParent(puntoBoca, false);
            var linea = aro.gameObject.AddComponent<LineRenderer>();
            linea.useWorldSpace = false;
            linea.loop = true;
            linea.widthMultiplier = 0.018f;
            linea.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            linea.sharedMaterial = Material("Aro_regreso", Naranja, false);
            const int puntos = 48;
            linea.positionCount = puntos;
            for (int i = 0; i < puntos; i++)
            {
                float a = i * Mathf.PI * 2f / puntos;
                linea.SetPosition(i, new Vector3(Mathf.Cos(a) * RadioAro, 0f, Mathf.Sin(a) * RadioAro));
            }

            var letrero = Texto("Letrero", visual, "REGRESO AL CITOPLASMA\n<size=60%>avienta aquí</size>", 1.2f,
                new Vector2(0.9f, 0.25f), new Vector3(0f, boca + 0.25f, 0f));

            var comp = raiz.AddComponent<RegresoCitoplasma>();
            var so = new SerializedObject(comp);
            so.FindProperty("boca").objectReferenceValue = puntoBoca;
            so.FindProperty("aro").objectReferenceValue = aro;
            so.FindProperty("letrero").objectReferenceValue = letrero;
            so.ApplyModifiedPropertiesWithoutUndo();

            return Guardar(raiz, "Cocina_Regreso");
        }

        // =============================================================================================
        // Escena
        // =============================================================================================

        static void Colocar(GameObject salida, GameObject regreso)
        {
            var escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);
            var ancla = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "AnclaCocina");
            if (ancla == null) { Debug.LogError("[M5] Cocina_M5 no tiene AnclaCocina."); return; }

            // Quitar lo que puso una corrida anterior
            foreach (var t in ancla.GetComponentsInChildren<Transform>(true).ToArray())
                if (t != null && (t.name.StartsWith("Cocina_Salida") || t.name.StartsWith("Cocina_Regreso") || t.name.StartsWith("[M5]")))
                    Object.DestroyImmediate(t.gameObject);

            var bandeja = Poner(salida, ancla.transform, "Salida");
            var destino = Poner(regreso, ancla.transform, "Regreso al citoplasma");

            // El vuelo es de M2: la escena necesita un VueloARNt con la boca del regreso como destino
            var vuelo = Object.FindFirstObjectByType<VueloARNt>(FindObjectsInactive.Include);
            if (vuelo == null)
            {
                var go = new GameObject("[M5] Vuelo de M2 (VueloARNt)");
                go.transform.SetParent(ancla.transform, false);
                vuelo = go.AddComponent<VueloARNt>();
            }
            var boca = destino != null ? destino.GetComponent<RegresoCitoplasma>().Boca : null;
            var vso = new SerializedObject(vuelo);
            vso.FindProperty("destinoRegresoCitoplasma").objectReferenceValue = boca;
            vso.ApplyModifiedPropertiesWithoutUndo();

            // La prueba de M4 ya no recicla sola: los usados se quedan en la bandeja
            var pruebaM4 = ancla.GetComponentInChildren<PruebaRibosomas>(true);
            if (pruebaM4 == null)
                Debug.LogError("[M5] Cocina_M5 no tiene la prueba de M4 (PruebaRibosomas). Duplica Cocina_M4 en vez de Cocina_Base.");
            else
            {
                var pso = new SerializedObject(pruebaM4);
                pso.FindProperty("reciclarSalida").boolValue = false;
                pso.ApplyModifiedPropertiesWithoutUndo();
            }

            var prueba = new GameObject("[M5] Prueba salida (vuelo de prueba)");
            prueba.transform.SetParent(ancla.transform, false);
            var so = new SerializedObject(prueba.AddComponent<PruebaSalida>());
            so.FindProperty("bandeja").objectReferenceValue = bandeja != null ? bandeja.GetComponent<BandejaSalida>() : null;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[M5] Listo: Cocina_M5 con la bandeja de salida, el regreso al citoplasma y la prueba. Teclas: C avienta al regreso el primero de la bandeja (más las de M4).");
        }

        static GameObject Poner(GameObject prefab, Transform ancla, string marca)
        {
            if (prefab == null) return null;
            var punto = ancla.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == marca);
            if (punto == null) { Debug.LogError($"[M5] No encontré la marca '{marca}' en AnclaCocina."); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, punto);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var marcaCocina = punto.GetComponent<PuntoCocina>();
            if (marcaCocina != null) marcaCocina.mostrarEnJuego = false;
            return go;
        }

        // =============================================================================================
        // Ayudantes
        // =============================================================================================

        // Copia el FBX de Uli a Cocina/Modelos/Uli si falta y lo importa sin cámaras ni luces
        static string Importar(string archivo)
        {
            string destino = $"{CarpetaUli}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, ExportsProps + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[M5] No encontré {origen}."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter imp &&
                (imp.importCameras || imp.importLights || imp.animationType != ModelImporterAnimationType.None))
            {
                imp.importCameras = false;
                imp.importLights = false;
                imp.animationType = ModelImporterAnimationType.None;
                imp.SaveAndReimport();
            }
            return destino;
        }

        // Instancia el FBX completo (vinculado al modelo) bajo el padre, sin colisionadores
        static Transform Modelo(string fbx, Transform padre, string nombre)
        {
            var modelo = fbx != null ? AssetDatabase.LoadAssetAtPath<GameObject>(fbx) : null;
            if (modelo == null) { Debug.LogWarning($"[M5] Falta el modelo {fbx}; el prefab queda sin malla."); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(modelo, padre);
            go.name = nombre;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            return go.transform;
        }

        static GameObject Guardar(GameObject raiz, string nombre)
        {
            var ruta = $"{CarpetaPrefabs}/{nombre}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            Object.DestroyImmediate(raiz);
            Debug.Log($"[M5] {ruta} listo.");
            return prefab;
        }

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Texto 3D de cara al jugador (la raíz del prefab tiene su +Z hacia el jugador)
        static TextMeshPro Texto(string nombre, Transform padre, string texto, float tamano, Vector2 caja, Vector3 posicion)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = caja;
            return tmp;
        }

        // Material URP Lit guardado como asset (Cocina_M5_<nombre>.mat, sin espacios)
        static Material Material(string nombre, Color color, bool transparente)
        {
            nombre = nombre.Replace(' ', '_');
            string ruta = $"{CarpetaMateriales}/Cocina_M5_{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, ruta);
            }
            m.SetColor("_BaseColor", color);
            m.SetColor("_EmissionColor", color * 0.6f);
            m.EnableKeyword("_EMISSION");
            if (transparente) PuntoCocina.Transparente(m);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
