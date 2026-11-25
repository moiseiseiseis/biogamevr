using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BioVR.Entornos.Editor
{
    // Pone el torrente sanguíneo final de Ulises (03-diseno/exports/torrente-sanguineo.fbx, PR #54) en
    // VasoSanguineo.prefab (zona Vasos, salto 1), a su escala real: 1 µm = 1 m (vaso de 30 m con bifurcación en Y,
    // glóbulos rojos de 7.5 m). Decisión del 24-nov.
    //
    //   - La cabina queda en Spawn_Nave (el punto de salida que dejó Ulises), mirando hacia la bifurcación.
    //   - Se apaga la nave Eritro que viene dentro del FBX (la nave del juego es la Seraphim) y el tubo provisional.
    //   - Sus glóbulos son fijos: se apagan los que caen en el tramo por donde fluyen los glóbulos del pokédex
    //     (CrearPokedex, z de FlujoZMin a FlujoZMax) para que no se atraviesen, y los que tocarían la cabina.
    //   - Materiales extraídos con GPU Instancing (400 copias de la misma malla), la pared visible por ambos
    //     lados y sus texturas en espejo (LEEME de documentacion/torrente-sanguineo).
    //   - Al final vuelve a poner los glóbulos del pokédex a la misma escala (CrearPokedex.SoloTorrente).
    // Se puede correr varias veces. Menú: BioVR → Poner torrente sanguíneo (Ulises)
    public static class PonerTorrente
    {
        const string Carpeta = "Assets/_BioVR/Modelos/Torrente";
        const string Destino = Carpeta + "/torrente-sanguineo.fbx";
        const string CarpetaTexturas = Carpeta + "/Texturas";
        const string CarpetaMateriales = Carpeta + "/Materiales";
        const string PrefabVaso = "Assets/_BioVR/Entornos/VasoSanguineo/VasoSanguineo.prefab";
        const string Nombre = PonerModelosDiseno.PrefijoModelo + "Torrente sanguíneo (Ulises)";

        public const float RadioVaso = 15f;               // el tronco mide 30 µm de diámetro
        // Tramo donde fluyen los glóbulos del pokédex (antes llegaba a 68 m y tapaba casi todo lo de Ulises:
        // prueba del 24-nov, "se ve igual que antes"). Más allá se ven los glóbulos fijos de Ulises.
        public const float FlujoZMin = -8f, FlujoZMax = 40f;
        static readonly Vector3 CentroNave = new Vector3(0f, 1f, 0f);
        // Caja de la cabina (x ±2.1, y −0.1 a 2.7) con holgura
        static readonly Bounds Cabina = new Bounds(new Vector3(0f, 1.3f, 0f), new Vector3(5.2f, 3.8f, 5.2f));

        static readonly string[] MaterialesCelula = { "Eritrocito_Meshy", "Plaqueta_Meshy", "Leucocito_Meshy" };
        static readonly string[] MaterialesNave = { "Eritro_Nave", "Cupula_Vidrio", "Flagelo_Negro", "Motor_" };

        [MenuItem("BioVR/Poner torrente sanguíneo (Ulises)")]
        public static void Poner()
        {
            var fbx = Importar();
            if (fbx == null) return;

            var raiz = PrefabUtility.LoadPrefabContents(PrefabVaso);
            foreach (var viejo in raiz.transform.Cast<Transform>().Where(t => t.name == Nombre).ToArray())
                Object.DestroyImmediate(viejo.gameObject);
            var pared = raiz.transform.Find("Pared del vaso");
            if (pared != null) pared.gameObject.SetActive(false);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
            go.name = Nombre;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // La nave Eritro (rig, mallas con piel y su piel de camuflaje) no va: la nave del juego es la Seraphim
            int eritro = 0;
            // (sin animación, sus mallas ya no traen piel: se reconocen por sus materiales)
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                var r = t.GetComponent<Renderer>();
                bool deLaNave = t.name == "Eritro_Rig" || r is SkinnedMeshRenderer ||
                                (r != null && r.sharedMaterials.Any(m => m != null && MaterialesNave.Any(n => m.name.StartsWith(n))));
                if (deLaNave) { t.gameObject.SetActive(false); eritro++; }
            }

            // Alinear: Spawn_Nave en el centro de la nave, mirando (en horizontal) hacia el centro del vaso
            var spawn = go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Spawn_Nave");
            var vaso = go.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name.StartsWith("Vaso_Sanguineo"));
            if (spawn == null || vaso == null)
            {
                Debug.LogError("[Torrente] El FBX no trae Spawn_Nave o Vaso_Sanguineo.");
                PrefabUtility.UnloadPrefabContents(raiz);
                return;
            }
            var hacia = vaso.bounds.center - spawn.position; hacia.y = 0f;
            if (hacia.sqrMagnitude > 1e-4f)
                go.transform.rotation = Quaternion.FromToRotation(hacia.normalized, Vector3.forward) * go.transform.rotation;
            go.transform.position += raiz.transform.TransformPoint(CentroNave) - spawn.position;

            // Glóbulos fijos que estorban: en el tramo del flujo del pokédex o encima de la cabina
            int apagados = 0, celulas = 0;
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.sharedMaterials.Any(m => m != null && MaterialesCelula.Any(n => m.name.StartsWith(n)))) continue;
                celulas++;
                var b = r.bounds;
                bool enFlujo = b.max.z > FlujoZMin - 4f && b.min.z < FlujoZMax + 4f;
                if (enFlujo || b.Intersects(Cabina)) { r.gameObject.SetActive(false); apagados++; }
            }

            var bv = vaso.bounds;

            // La esfera de fondo del vaso provisional medía 150 m y terminaba a ~95 m: tapaba el resto del tronco y
            // la bifurcación. Ahora envuelve todo el vaso (la cámara dibuja hasta 1000 m).
            var fondo = raiz.transform.Find("Fondo");
            if (fondo != null)
            {
                fondo.position = bv.center;
                fondo.localScale = Vector3.one * bv.size.magnitude * 1.3f;
            }

            // El flujo de los glóbulos del pokédex: regresan al fondo al llegar a FlujoZMax, no a 68 m
            var flujo = raiz.transform.Find("Globulos (flujo)");
            var fp = flujo != null ? flujo.GetComponent<FlujoPlaceholder>() : null;
            if (fp != null)
            {
                var so = new SerializedObject(fp);
                so.FindProperty("zMin").floatValue = FlujoZMin;
                so.FindProperty("zMax").floatValue = FlujoZMax;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log($"[Torrente] Vaso de {bv.min} a {bv.max}; Eritro apagada ({eritro} piezas); " +
                      $"glóbulos fijos: {celulas - apagados} visibles, {apagados} apagados (flujo del pokédex y cabina).");
            PrefabUtility.SaveAsPrefabAsset(raiz, PrefabVaso);
            PrefabUtility.UnloadPrefabContents(raiz);

            // Los glóbulos del pokédex (de Diego) a la misma escala y repartidos en el tronco de 30 m
            BioVR.Pokedex.Editor.CrearPokedex.SoloTorrente();
            AssetDatabase.SaveAssets();
            Debug.Log("[Torrente] Listo: torrente de Ulises en VasoSanguineo.prefab (zona Vasos).");
        }

        // Fotos del vaso desde el asiento (zona Vasos de la escena inicial, con la cabina), para revisarlo sin el visor.
        // Batch: -executeMethod BioVR.Entornos.Editor.PonerTorrente.Capturar -capturas <carpeta> -quit
        public static void Capturar()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturas");
            string carpeta = i >= 0 && i + 1 < args.Length ? args[i + 1] : "Capturas/Torrente";
            Directory.CreateDirectory(carpeta);

            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_BioVR/Escenas/00-EscenaInicial.unity");
            var zonas = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "Zonas");
            if (zonas == null) { Debug.LogError("[Torrente] No hay Zonas."); return; }
            foreach (Transform z in zonas.transform) z.gameObject.SetActive(z.name == "Vasos");

            var cam = new GameObject("Camara captura").AddComponent<Camera>();
            cam.fieldOfView = 95f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1000f;
            var rt = new RenderTexture(1280, 1280, 24);
            cam.targetTexture = rt;
            var ojos = new Vector3(0f, 1.4f, 0f);
            foreach (var (nombre, rot) in new[] { ("frente", Quaternion.identity), ("lado", Quaternion.Euler(0f, 70f, 0f)),
                                                  ("atras", Quaternion.Euler(0f, 180f, 0f)), ("arriba", Quaternion.Euler(-50f, 0f, 0f)) })
            {
                cam.transform.SetPositionAndRotation(ojos, rot);
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(carpeta, $"torrente_{nombre}.png"), tex.EncodeToPNG());
            }
            RenderTexture.active = null;
        }

        // Qué hay en la zona Vasos de la escena inicial (activo, escala, renderers prendidos)
        public static void Diagnosticar()
        {
            var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_BioVR/Escenas/00-EscenaInicial.unity");
            var zonas = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "Zonas");
            foreach (Transform z in zonas.transform)
            {
                Debug.Log($"[Diag] zona {z.name} activa={z.gameObject.activeSelf} pos={z.position} escala={z.lossyScale}");
                if (z.name != "Vasos") continue;
                foreach (Transform h in z)
                {
                    Debug.Log($"[Diag]   {h.name} activo={h.gameObject.activeSelf} prefab={PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(h.gameObject)}");
                    foreach (Transform n in h)
                        Debug.Log($"[Diag]     {n.name} activo={n.gameObject.activeSelf} escala={n.lossyScale} renderers={n.GetComponentsInChildren<Renderer>(false).Length}");
                }
            }
            foreach (var g in escena.GetRootGameObjects())
                foreach (var t in g.GetComponentsInChildren<Transform>(true))
                    if (t.name.Contains("Torrente") || t.name.Contains("VasoSanguineo"))
                        Debug.Log($"[Diag] encontrado {t.name} en {t.parent?.name} activo={t.gameObject.activeInHierarchy}");
        }

        // Copia el FBX (si cambió), lo importa sin cámaras, luces ni animación, y saca texturas y materiales
        static GameObject Importar()
        {
            string origen = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../03-diseno/exports/torrente-sanguineo.fbx"));
            if (!File.Exists(origen)) { Debug.LogError($"[Torrente] No encontré {origen}."); return null; }
            if (!AssetDatabase.IsValidFolder(Carpeta)) AssetDatabase.CreateFolder("Assets/_BioVR/Modelos", "Torrente");
            if (!File.Exists(Destino) || new FileInfo(Destino).Length != new FileInfo(origen).Length)
            {
                File.Copy(origen, Destino, true);
                AssetDatabase.ImportAsset(Destino, ImportAssetOptions.ForceUpdate);
            }

            var mi = (ModelImporter)AssetImporter.GetAtPath(Destino);
            bool cambio = false;
            if (mi.importCameras || mi.importLights) { mi.importCameras = false; mi.importLights = false; cambio = true; }
            if (mi.importAnimation) { mi.importAnimation = false; cambio = true; }
            if (mi.bakeAxisConversion) { mi.bakeAxisConversion = false; cambio = true; }
            if (cambio) mi.SaveAndReimport();

            // Texturas incrustadas → archivos (si no, los materiales extraídos no las encuentran)
            if (!AssetDatabase.IsValidFolder(CarpetaTexturas))
            {
                AssetDatabase.CreateFolder(Carpeta, "Texturas");
                mi.ExtractTextures(CarpetaTexturas);
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(Destino, ImportAssetOptions.ForceUpdate);
            }

            // Materiales → archivos, para poder activar instancing y doble cara
            if (!AssetDatabase.IsValidFolder(CarpetaMateriales)) AssetDatabase.CreateFolder(Carpeta, "Materiales");
            bool extraidos = false;
            foreach (var m in AssetDatabase.LoadAllAssetsAtPath(Destino).OfType<Material>().ToArray())
            {
                string ruta = $"{CarpetaMateriales}/{m.name}.mat";
                if (File.Exists(ruta)) continue;
                string error = AssetDatabase.ExtractAsset(m, ruta);
                if (!string.IsNullOrEmpty(error)) Debug.LogWarning($"[Torrente] No se pudo sacar {m.name}: {error}");
                else extraidos = true;
            }
            if (extraidos) AssetDatabase.ImportAsset(Destino, ImportAssetOptions.ForceUpdate);

            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { CarpetaMateriales }))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                mat.enableInstancing = true;
                float emision = mat.name.StartsWith("Pared_Vaso") ? 0.25f : mat.name.StartsWith("Eritrocito") ? 0.35f
                              : mat.name.StartsWith("Plaqueta") ? 0.6f : mat.name.StartsWith("Leucocito") ? 0.15f : -1f;
                if (mat.name.StartsWith("Pared_Vaso"))
                {
                    if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f); // se ve por dentro y por fuera
                    foreach (var p in new[] { "_BaseMap", "_MainTex", "_BumpMap", "_MetallicGlossMap" })
                        if (mat.HasProperty(p) && mat.GetTexture(p) is Texture t &&
                            AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t)) is TextureImporter ti && ti.wrapMode != TextureWrapMode.Mirror)
                        {
                            ti.wrapMode = TextureWrapMode.Mirror; // se repiten en espejo a lo largo del tubo
                            ti.SaveAndReimport();
                        }
                }
                if (emision > 0f && mat.HasProperty("_EmissionColor"))
                {
                    var baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    mat.SetColor("_EmissionColor", baseColor * emision);
                    if (mat.HasProperty("_BaseMap") && mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", mat.GetTexture("_BaseMap"));
                }
                EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(Destino);
        }
    }
}
