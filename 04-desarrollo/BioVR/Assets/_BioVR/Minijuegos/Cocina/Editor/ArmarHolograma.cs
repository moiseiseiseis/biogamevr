using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M7 (Ascencio) · Menú BioVR → Cocina → Armar holograma (M7).
    //
    // Arma Prefabs/Cocina_Holograma.prefab según la especificación 8.2:
    //   - proyector-holograma.fbx de Uli escalado (0.8, 0.3, 0.8), apoyado en la marca
    //   - aro cian (radio 0.12 a 0.16 m) a 0.137 m y cono de luz cian casi transparente (0.14 a 0.33 m de radio, 0.62 m de alto)
    //   - letreros "HOLOGRAMA · se dobla solo", "representación didáctica" y el crédito de AlphaFold (8.6)
    //   - LectorCoordenadas y HologramaCocina con los valores de Cocina_M7, y un material para la línea lila
    //     (antes se creaba con Shader.Find al jugar, y en el APK ese shader no se incluye)
    //
    // Los textos viven en el prefab: Diseño o Investigación los pueden cambiar sin tocar código.
    // Se puede correr varias veces: vuelve a generar el prefab y sus mallas.
    public static class ArmarHolograma
    {
        const string Raiz = CrearDatosCocina.Raiz;
        public const string RutaPrefab = Raiz + "/Prefabs/Cocina_Holograma.prefab";
        const string CarpetaUli = Raiz + "/Modelos/Uli";
        const string CarpetaMallas = Raiz + "/Modelos/Generados";
        const string CarpetaMateriales = Raiz + "/Materiales";
        const string ModeloProyector = CarpetaUli + "/proyector-holograma.fbx";
        const string ExportsUli = "../../../03-diseno/exports/minijuego-cocina/props/"; // desde Assets/

        static readonly Vector3 EscalaProyector = new Vector3(0.8f, 0.3f, 0.8f);
        const float AlturaAro = 0.137f, RadioAroInterno = 0.12f, RadioAroExterno = 0.16f;
        const float RadioConoAbajo = 0.14f, RadioConoArriba = 0.33f, AltoCono = 0.62f, OpacidadCono = 0.05f;

        static readonly Color Cian = new Color(0.2f, 0.9f, 1f);
        static readonly Color Lila = new Color(0.72f, 0.45f, 1f, 0.6f);

        [MenuItem("BioVR/Cocina/Armar holograma (M7)")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CrearDatosCocina.Carpeta(CarpetaUli);
            CrearDatosCocina.Carpeta(CarpetaMallas);
            CrearDatosCocina.Carpeta(CarpetaMateriales);

            var fbx = Importar();
            var cuenta = AssetDatabase.LoadAssetAtPath<GameObject>(Raiz + "/Prefabs/Cocina_CuentaHolograma.prefab");
            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(Raiz + "/Datos/coordenadas_normales.csv");
            var forma = AssetDatabase.LoadAssetAtPath<FormaProteinaData>(Raiz + "/Datos/FormaS20G.asset");
            if (cuenta == null || csv == null || forma == null)
            {
                Debug.LogError("[M7] Falta Cocina_CuentaHolograma.prefab, coordenadas_normales.csv o FormaS20G.asset.");
                return;
            }

            // Escena vacía para armar el prefab sin ensuciar ninguna
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var raiz = new GameObject("Cocina_Holograma");

            // ---- Proyector ----
            float alturaProyector = 0.134f;
            if (fbx != null)
            {
                var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
                modelo.name = "Proyector (Uli)";
                modelo.transform.localScale = EscalaProyector;
                var b = Limites(modelo);
                modelo.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z); // centrado y apoyado en la marca
                alturaProyector = b.size.y;
                Debug.Log($"[M7] Proyector: {b.size.x:F3} × {b.size.y:F3} × {b.size.z:F3} m.");
            }
            else
            {
                var cilindro = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cilindro.name = "Proyector (provisional)";
                Object.DestroyImmediate(cilindro.GetComponent<Collider>());
                cilindro.transform.SetParent(raiz.transform, false);
                cilindro.transform.localPosition = new Vector3(0f, alturaProyector / 2f, 0f);
                cilindro.transform.localScale = new Vector3(0.4f, alturaProyector / 2f, 0.4f);
            }

            // ---- Aro y cono de luz ----
            Pieza("Aro", raiz.transform, Malla("Cocina_HoloAro", Aro()), MaterialUnlit("Cocina_HoloAro", Cian, false), AlturaAro);
            Pieza("Cono de luz", raiz.transform, Malla("Cocina_HoloCono", Cono()),
                  MaterialUnlit("Cocina_HoloCono", new Color(Cian.r, Cian.g, Cian.b, OpacidadCono), true), AlturaAro);

            // ---- Letreros (frente al jugador: la marca tiene su +Z hacia él) ----
            Texto("Titulo", raiz.transform, "HOLOGRAMA · se dobla solo", 0.22f, new Vector3(0f, AlturaAro + AltoCono + 0.04f, 0f), Cian);
            Texto("Aviso", raiz.transform, "representación didáctica", 0.14f, new Vector3(0f, alturaProyector + 0.015f, RadioConoArriba * 0.6f), new Color(0.8f, 0.9f, 1f));
            Texto("Creditos", raiz.transform, "estructura: AlphaFold AF-P10997-F1 · CC BY 4.0", 0.1f, new Vector3(0f, 0.025f, 0.21f), new Color(0.7f, 0.75f, 0.8f));

            // ---- Lógica (valores de Cocina_M7) ----
            var lector = raiz.AddComponent<LectorCoordenadas>();
            lector.csvNormal = csv;
            lector.formaS20G = forma;
            var holo = raiz.AddComponent<HologramaCocina>();
            holo.lector = lector;
            holo.cuentaPrefab = cuenta;
            holo.escalaCuenta = 0.022f;
            holo.materialLinea = MaterialUnlit("Cocina_HoloLinea", Lila, true); // URP Unlit no usa el color de vértice de la línea
            holo.colorLineaLila = Lila;

            PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
            Object.DestroyImmediate(raiz);
            AssetDatabase.SaveAssets();
            Debug.Log($"[M7] Listo: {RutaPrefab}. Vuelve a correr BioVR → Cocina → Armar escena Cocina para ponerlo en la Cocina.");
        }

        // Copia el FBX de Uli a Cocina/Modelos/Uli (si no está) y lo importa sin cámaras ni luces
        static GameObject Importar()
        {
            if (!File.Exists(ModeloProyector))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, ExportsUli + "proyector-holograma.fbx"));
                if (!File.Exists(origen)) { Debug.LogWarning($"[M7] No encontré {origen}: se usa un cilindro provisional."); return null; }
                File.Copy(origen, ModeloProyector);
                AssetDatabase.ImportAsset(ModeloProyector);
            }
            if (AssetImporter.GetAtPath(ModeloProyector) is ModelImporter mi && (mi.importCameras || mi.importLights))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(ModeloProyector);
        }

        // Anillo plano en XZ entre los dos radios, visible por arriba y por abajo
        static Mesh Aro()
        {
            const int lados = 64;
            var v = new Vector3[lados * 2];
            var t = new int[lados * 12];
            for (int i = 0; i < lados; i++)
            {
                float a = i * Mathf.PI * 2f / lados;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                v[i * 2] = d * RadioAroInterno;
                v[i * 2 + 1] = d * RadioAroExterno;
                int j = (i + 1) % lados;
                int k = i * 12;
                // cara de arriba y cara de abajo
                t[k] = i * 2; t[k + 1] = j * 2; t[k + 2] = i * 2 + 1;
                t[k + 3] = i * 2 + 1; t[k + 4] = j * 2; t[k + 5] = j * 2 + 1;
                t[k + 6] = i * 2; t[k + 7] = i * 2 + 1; t[k + 8] = j * 2;
                t[k + 9] = i * 2 + 1; t[k + 10] = j * 2 + 1; t[k + 11] = j * 2;
            }
            var m = new Mesh { vertices = v, triangles = t };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // Cono truncado abierto (solo la pared), con caras hacia afuera y hacia adentro
        static Mesh Cono()
        {
            const int lados = 48;
            var v = new Vector3[lados * 2];
            var t = new int[lados * 12];
            for (int i = 0; i < lados; i++)
            {
                float a = i * Mathf.PI * 2f / lados;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                v[i * 2] = d * RadioConoAbajo;
                v[i * 2 + 1] = d * RadioConoArriba + Vector3.up * AltoCono;
                int j = (i + 1) % lados;
                int k = i * 12;
                t[k] = i * 2; t[k + 1] = i * 2 + 1; t[k + 2] = j * 2;
                t[k + 3] = j * 2; t[k + 4] = i * 2 + 1; t[k + 5] = j * 2 + 1;
                t[k + 6] = i * 2; t[k + 7] = j * 2; t[k + 8] = i * 2 + 1;
                t[k + 9] = j * 2; t[k + 10] = j * 2 + 1; t[k + 11] = i * 2 + 1;
            }
            var m = new Mesh { vertices = v, triangles = t };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // Guarda la malla como asset (si ya existe, la reemplaza conservando su GUID)
        static Mesh Malla(string nombre, Mesh nueva)
        {
            string ruta = $"{CarpetaMallas}/{nombre}.asset";
            nueva.name = nombre;
            var vieja = AssetDatabase.LoadAssetAtPath<Mesh>(ruta);
            if (vieja == null) { AssetDatabase.CreateAsset(nueva, ruta); return nueva; }
            EditorUtility.CopySerialized(nueva, vieja);
            EditorUtility.SetDirty(vieja);
            return vieja;
        }

        static void Pieza(string nombre, Transform padre, Mesh malla, Material material, float altura)
        {
            var go = new GameObject(nombre, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(0f, altura, 0f);
            go.GetComponent<MeshFilter>().sharedMesh = malla;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void Texto(string nombre, Transform padre, string texto, float tamano, Vector3 posicion, Color color)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.rectTransform.sizeDelta = new Vector2(0.6f, 0.06f);
        }

        // URP Unlit guardado como asset; transparente si se pide
        static Material MaterialUnlit(string nombre, Color color, bool transparente)
        {
            string ruta = $"{CarpetaMateriales}/{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(m, ruta);
            }
            if (transparente) PuntoCocina.Transparente(m);
            m.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
