using System.IO;
using System.Linq;
using BioVR.Comun;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Entornos.Editor
{
    // Menú BioVR → Espacios finales (decisiones del 21-nov: usar los modelos finales de Diseño en todo).
    //
    //   1. Sala de enlace (#92, Diego): la cápsula desde donde el Operador controla el Mecha a distancia sustituye a la
    //      cabina retro. La pantalla envolvente de 270° es una "ventana con efecto de monitor": se ve el mundo a través de
    //      ella con un velo verde de líneas de barrido (la imagen de Diego), sin una segunda cámara (que costaría el doble).
    //   2. Investigadora (#43, Diego) en el hangar, frente a la cápsula, en lugar del Explorador (fuera de la narrativa).
    //   3. ASTRA (Diego) como holograma pequeño junto a la consola.
    //   4. Sala de reducción (low poly de Uli) como lo que se ve en el abordaje: la Seraphim en la plataforma y los
    //      anillos que se cargan y giran rápido en la reducción (el primer salto).
    // "Todo" corre 1 a 4 en orden. Cada uno se puede correr varias veces.
    public static class PonerEspaciosFinales
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string Hangar = "Assets/_BioVR/Entornos/Hangar/Hangar.prefab";
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/
        const string Modelos = "Assets/_BioVR/Modelos/";
        const string Materiales = "Assets/_BioVR/Entornos/Materiales/";
        const string Prefijo = "Modelo ";

        [MenuItem("BioVR/Espacios finales/Todo (1 a 4)", priority = 0)]
        public static void Todo()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SalaEnlace();
            Investigadora();
            Astra();
            SalaReduccion();
            Debug.Log("[Espacios] Listo: sala de enlace, Investigadora, ASTRA y sala de reducción. Corre también BioVR → Agregar sonidos a la escena inicial.");
        }

        // ---------------- 1. Sala de enlace ----------------

        [MenuItem("BioVR/Espacios finales/1 Sala de enlace (cabina)", priority = 1)]
        public static void SalaEnlace()
        {
            if (Importar("sala-enlace.fbx", "Cabina", "sala-enlace") == null) return;
            PonerModelosDiseno.Cabina(); // usa la sala de enlace si ya está en Modelos/Cabina
            BioVR.Comun.Editor.AgregarMovimientoYEscotilla.Agregar(); // colisiones y escotilla con las medidas de la cápsula

            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var cabina = escena.GetRootGameObjects().FirstOrDefault(r => r.name == "Cabina (provisional)");
            var modelo = cabina != null ? cabina.transform.Find(Prefijo + "Cabina de Diseño") : null;
            if (modelo == null) { Debug.LogError("[Espacios] No encontré la cabina en la escena inicial."); return; }

            // La pantalla envolvente deja ver el mundo: velo verde translúcido con la imagen de Diego (pixelada)
            var envolvente = Buscar(modelo, "INT_Pantalla_Envolvente");
            if (envolvente != null && envolvente.TryGetComponent<Renderer>(out var r))
                r.sharedMaterials = Enumerable.Repeat(MaterialPantalla(), r.sharedMaterials.Length).ToArray();
            else Debug.LogWarning("[Espacios] La sala de enlace no trae INT_Pantalla_Envolvente.");

            // En la célula la cápsula se vuelve holograma: también debe ocultar la cápsula (antes, CAB_Estructura)
            var holo = cabina.GetComponent<BioVR.Escala.CabinaHolografica>();
            if (holo != null)
            {
                var so = new SerializedObject(holo);
                var lista = so.FindProperty("ocultar");
                var nombres = new[] { "CAB_Estructura", "CAB_Cristal", "CAB_Mobiliario", "CAB_Capsula" };
                lista.arraySize = nombres.Length;
                for (int i = 0; i < nombres.Length; i++) lista.GetArrayElementAtIndex(i).stringValue = nombres[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[Espacios] Sala de enlace puesta (cápsula, controles en sus INT_, pantalla envolvente como ventana).");
        }

        static Material MaterialPantalla()
        {
            var ruta = Materiales + "SalaEnlace_PantallaEnvolvente.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Modelos + "Cabina/texturas/sala-enlace/sala-enlace_camaras.png");
            if (tex != null && AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) is TextureImporter ti && ti.filterMode != FilterMode.Point)
            {
                ti.filterMode = FilterMode.Point; // pixelada, como pide Diseño
                ti.SaveAndReimport();
            }
            m = Transparente("Universal Render Pipeline/Unlit", new Color(0.55f, 1f, 0.7f, 0.16f));
            if (tex != null) m.SetTexture("_BaseMap", tex);
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }

        // ---------------- 2. Investigadora ----------------

        [MenuItem("BioVR/Espacios finales/2 Investigadora en el hangar", priority = 2)]
        public static void Investigadora()
        {
            var fbx = Importar("investigadora.fbx", "Personajes", "investigadora");
            if (fbx == null) return;
            var raiz = PrefabUtility.LoadPrefabContents(Hangar);
            var explorador = raiz.transform.Find(Prefijo + "Explorador");
            if (explorador != null) explorador.gameObject.SetActive(false); // fuera de la narrativa actual (GDD 3.3)
            var vieja = raiz.transform.Find(Prefijo + "Investigadora");
            if (vieja != null) Object.DestroyImmediate(vieja.gameObject);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
            go.name = Prefijo + "Investigadora";
            // Frente a la cápsula, un poco a la izquierda, de cara al jugador (que mira hacia +Z)
            go.transform.localPosition = new Vector3(-1.3f, 0f, 3.4f);
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            BrazosAbajo(go.transform);
            var b = Limites(go);
            go.transform.localPosition += new Vector3(0f, -0.05f - b.min.y, 0f); // pies en el piso del hangar
            Debug.Log($"[Espacios] Investigadora: {b.size.y:F2} m de alto.");

            PrefabUtility.SaveAsPrefabAsset(raiz, Hangar);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        // El modelo viene en pose T (está riggeado pero sin animaciones): baja los brazos a los costados
        static void BrazosAbajo(Transform modelo)
        {
            foreach (var lado in new[] { "L", "R" })
            {
                var brazo = Buscar(modelo, $"J_Bip_{lado}_UpperArm");
                var codo = Buscar(modelo, $"J_Bip_{lado}_LowerArm");
                if (brazo == null || codo == null) { Debug.LogWarning($"[Espacios] La Investigadora no tiene J_Bip_{lado}_UpperArm/LowerArm: queda en pose T."); continue; }
                var actual = (codo.position - brazo.position).normalized;
                var afuera = Vector3.ProjectOnPlane(actual, Vector3.up).normalized; // hacia su lado
                var deseada = (Vector3.down * 0.95f + afuera * 0.25f).normalized;
                brazo.rotation = Quaternion.FromToRotation(actual, deseada) * brazo.rotation;
            }
        }

        // ---------------- 3. ASTRA ----------------

        [MenuItem("BioVR/Espacios finales/3 ASTRA en la consola", priority = 3)]
        public static void Astra()
        {
            var fbx = Importar("astra-ia-de-viaje.fbx", "Personajes", "astra");
            if (fbx == null) return;
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            foreach (var viejo in escena.GetRootGameObjects().Where(g => g.name == "ASTRA").ToArray()) Object.DestroyImmediate(viejo);
            var cabina = escena.GetRootGameObjects().FirstOrDefault(r => r.name == "Cabina (provisional)");
            var modelo = cabina != null ? cabina.transform.Find(Prefijo + "Cabina de Diseño") : null;
            var consola = modelo != null ? Buscar(modelo, "CAB_Consola") : null;

            // Fuera de la cabina (en la raíz), para que el modo holograma de la cabina no le cambie el material
            var raiz = new GameObject("ASTRA");
            var b = consola != null ? Limites(consola.gameObject) : new Bounds(new Vector3(0f, 0.9f, 0.7f), Vector3.one * 0.6f);
            raiz.transform.position = new Vector3(b.max.x - 0.12f, b.max.y + 0.14f, b.center.z);
            var astra = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
            astra.transform.localPosition = Vector3.zero;
            astra.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // la lente hacia el piloto
            var ba = Limites(astra);
            float mayor = Mathf.Max(ba.size.x, ba.size.y, ba.size.z);
            if (mayor > 0f) astra.transform.localScale = Vector3.one * (0.2f / mayor); // 20 cm
            ba = Limites(astra);
            astra.transform.position -= ba.center - raiz.transform.position;
            var so = new SerializedObject(raiz.AddComponent<AstraHolograma>());
            so.FindProperty("modelo").objectReferenceValue = astra.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[Espacios] ASTRA junto a la consola.");
        }

        // ---------------- 4. Sala de reducción ----------------

        [MenuItem("BioVR/Espacios finales/4 Sala de reducción (abordaje)", priority = 4)]
        public static void SalaReduccion()
        {
            var fbx = Importar("sala-reduccion_lowpoly.fbx", "SalaReduccion", "sala-reduccion", sinAnimacion: true);
            var seraphim = PonerNaveSeraphim.ImportarSeraphim();
            if (fbx == null) return;
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var zona = escena.GetRootGameObjects().FirstOrDefault(r => r.name == "Zonas")?.transform.Find("Abordaje");
            if (zona == null) { Debug.LogError("[Espacios] No hay Zonas/Abordaje en la escena inicial."); return; }

            // El abordaje mostraba otra copia del hangar: ahora muestra la sala de reducción (la "transmisión")
            foreach (Transform hijo in zona)
                if (!hijo.name.StartsWith(Prefijo + "Sala de reduccion")) hijo.gameObject.SetActive(false);
            var vieja = zona.Find(Prefijo + "Sala de reduccion");
            if (vieja != null) Object.DestroyImmediate(vieja.gameObject);

            var sala = (GameObject)PrefabUtility.InstantiatePrefab(fbx, zona);
            sala.name = Prefijo + "Sala de reduccion";
            sala.transform.localPosition = Vector3.zero;
            sala.transform.localRotation = Quaternion.identity;

            // La cápsula (el origen) queda en la entrada de la sala, mirando a la plataforma
            var entrada = Buscar(sala.transform, "Spawn_Jugador");
            var soporte = Buscar(sala.transform, "Soporte_Nave");
            if (entrada != null && soporte != null)
            {
                var hacia = Vector3.ProjectOnPlane(soporte.position - entrada.position, Vector3.up);
                float yaw = Vector3.SignedAngle(hacia, Vector3.forward, Vector3.up);
                sala.transform.RotateAround(entrada.position, Vector3.up, yaw);
                var destino = entrada.position + (soporte.position - entrada.position) * 0.15f; // un poco adentro
                sala.transform.position += new Vector3(-destino.x, -entrada.position.y, -destino.z);
            }
            else Debug.LogWarning("[Espacios] La sala no trae Spawn_Jugador / Soporte_Nave: queda en el origen.");

            // La Seraphim plegada en el centro de los anillos (medido con la malla del anillo interior, no con los
            // empties del FBX), de frente al jugador y del tamaño que quepa adentro
            var interior = Buscar(sala.transform, "Anillo_Interior");
            if (seraphim != null && interior != null)
            {
                var bi = Limites(interior.gameObject);
                var nave = new GameObject(Prefijo + "Seraphim en los anillos");
                nave.transform.SetParent(sala.transform, true);
                nave.transform.position = bi.center;
                nave.transform.rotation = Quaternion.LookRotation(Vector3.back) * PonerNaveSeraphim.MedirCorreccion(seraphim); // cúpula hacia el jugador
                var m = (GameObject)PrefabUtility.InstantiatePrefab(seraphim, nave.transform);
                m.transform.localPosition = Vector3.zero;
                m.transform.localRotation = Quaternion.identity;
                // El anillo interior es un aro: su lado más chico es el grosor, los otros dos son el diámetro
                var s = bi.size;
                float diametro = Mathf.Max(Mathf.Min(s.x, s.y), Mathf.Min(Mathf.Max(s.x, s.y), s.z));
                float cabe = diametro * 0.55f;
                var bn = Limites(nave);
                float mayor = Mathf.Max(bn.size.x, bn.size.y, bn.size.z);
                if (mayor > 0f) nave.transform.localScale *= cabe / mayor;
                bn = Limites(nave);
                nave.transform.position += bi.center - bn.center; // centrada en el anillo
                var dso = new SerializedObject(nave.AddComponent<NaveDespliegue>());
                dso.FindProperty("seguirAlJuego").boolValue = false; // se queda plegada, lista para reducirse
                dso.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[Espacios] Seraphim en los anillos: centro {bi.center:F1}, anillo de {diametro:F1} m, nave de {cabe:F1} m.");
            }

            // Anillos: el script los carga en el abordaje y los acelera en la reducción
            var anillos = new[] { "Anillo_Exterior", "Anillo_Medio", "Anillo_Interior" }.Select(n => Buscar(sala.transform, n)).Where(t => t != null).ToArray();
            var aso = new SerializedObject(sala.AddComponent<AnillosReduccion>());
            var lista = aso.FindProperty("anillos");
            lista.arraySize = anillos.Length;
            for (int i = 0; i < anillos.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = anillos[i];
            var ejes = aso.FindProperty("ejes");
            ejes.arraySize = anillos.Length;
            for (int i = 0; i < anillos.Length; i++) ejes.GetArrayElementAtIndex(i).vector3Value = i == 1 ? Vector3.forward : Vector3.right;
            aso.ApplyModifiedPropertiesWithoutUndo();

            var bs = Limites(sala);
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[Espacios] Sala de reducción en el abordaje: {bs.size.x:F0} × {bs.size.y:F0} × {bs.size.z:F0} m, {anillos.Length} anillos.");
        }

        // ---------------- 5. Escenario celular v2 (prueba) ----------------

        // El v2 de Uli (escenario-celular-v2, ~550k triángulos y ~5,900 objetos) optimizado para el Quest, como prueba.
        // Para regresar a la low poly: BioVR → Poner escenario celular (low poly). Después de cualquiera de los dos,
        // volver a correr BioVR → Agregar pokédex biológico (marca los organelos del escenario que quede).
        //   - Quita los LODGroup que crea Unity (agrupan cada "_Bloques" entero, que abarca la célula: siempre elegirían
        //     el nivel más pesado). Deja un solo nivel por pieza: LOD1 en organelos, LOD2 en bloques repetidos y redes.
        //   - Apaga virus, E. coli y bacteriófagos (el juego va sin virus).
        //   - Marca todo como estático para que Unity junte mallas (menos dibujos por cuadro). OJO: con esto la célula
        //     ya no se puede mover, así que no sirve para la prueba de pilotaje libre (que mueve la célula).
        //   - La nave queda en Spawn_Nave (la pista del retículo rugoso).
        [MenuItem("BioVR/Espacios finales/5 Escenario celular v2 (prueba)", priority = 20)]
        public static void EscenarioV2()
        {
            var fbx = Importar("escenario-celular-v2.fbx", "EscenarioCelular", "escenario-celular-v2");
            if (fbx == null) return;
            const string ruta = "Assets/_BioVR/Entornos/EspacioCelular/EspacioCelular.prefab";
            var raiz = PrefabUtility.LoadPrefabContents(ruta);
            var viejo = raiz.transform.Find(Prefijo + "Escenario celular");
            if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
            go.name = Prefijo + "Escenario celular";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            foreach (var lg in go.GetComponentsInChildren<LODGroup>(true)) Object.DestroyImmediate(lg);
            int apagados = 0;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                var m = System.Text.RegularExpressions.Regex.Match(t.name, @"_LOD(\d)$");
                bool repetido = t.parent != null && (t.parent.name.Contains("Bloques") || t.parent.name.Contains("Telarana"));
                bool fuera = t.name.StartsWith("Virus") || t.name.StartsWith("E_Coli") || t.name.StartsWith("Bacteriofago") || t.name.StartsWith("Luz_");
                bool otroNivel = m.Success && int.Parse(m.Groups[1].Value) != (repetido ? 2 : 1);
                if (fuera || otroNivel) { t.gameObject.SetActive(false); apagados++; }
            }

            // Estático (excepto lo que tiembla con los empujones moleculares)
            int renderers = 0, triangulos = 0;
            foreach (var r in go.GetComponentsInChildren<Renderer>(false))
            {
                renderers++;
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) for (int i = 0; i < mf.sharedMesh.subMeshCount; i++) triangulos += (int)mf.sharedMesh.GetIndexCount(i) / 3;
                bool tiembla = r.GetComponentsInParent<Transform>(true).Any(p => p.name.StartsWith("Burbuja") || p.name.StartsWith("Vesicula_Libre") || p.name.StartsWith("Polirribosoma"));
                if (!tiembla) GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
            }

            // La nave (el origen de la zona) en Spawn_Nave
            var spawn = Buscar(go.transform, "Spawn_Nave");
            if (spawn != null) go.transform.position -= spawn.position - raiz.transform.position;
            else Debug.LogWarning("[Espacios] El v2 no trae Spawn_Nave: queda en el origen.");

            PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            PrefabUtility.UnloadPrefabContents(raiz);
            Debug.Log($"[Espacios] Escenario v2 (prueba): {triangulos:N0} triángulos en {renderers:N0} objetos visibles ({apagados:N0} apagados). " +
                      "Ahora corre BioVR → Agregar pokédex biológico. Para regresar: BioVR → Poner escenario celular (low poly).");
        }

        // ---------------- Ayudantes ----------------

        // Copia un FBX de 03-diseno/exports a Modelos/<carpeta> (y sus texturas con la misma ruta relativa que buscan
        // los FBX: texturas/<texturas>/) y lo importa sin cámaras ni luces (y sin animación si se pide)
        static GameObject Importar(string archivo, string carpeta, string texturas, bool sinAnimacion = false)
        {
            string dir = Modelos + carpeta;
            Carpeta(dir);
            string origenTex = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "texturas/" + texturas));
            if (Directory.Exists(origenTex))
            {
                string destinoTex = $"{dir}/texturas/{texturas}";
                Carpeta(destinoTex);
                foreach (var f in Directory.GetFiles(origenTex))
                {
                    string d = $"{destinoTex}/{Path.GetFileName(f)}";
                    if (!File.Exists(d)) { File.Copy(f, d); AssetDatabase.ImportAsset(d); }
                    if (AssetImporter.GetAtPath(d) is TextureImporter ti && ti.maxTextureSize > 1024) { ti.maxTextureSize = 1024; ti.SaveAndReimport(); }
                }
            }
            string destino = $"{dir}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[Espacios] No encontré {origen}. ¿Ya está fusionado su PR y hiciste pull?"); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter mi &&
                (mi.importCameras || mi.importLights || mi.bakeAxisConversion || (sinAnimacion && mi.importAnimation)))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.bakeAxisConversion = false; // Diseño exporta sin Apply Transform: no marcar Bake Axis Conversion
                if (sinAnimacion) mi.importAnimation = false;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(destino);
        }

        static Material Transparente(string shader, Color color)
        {
            var m = new Material(Shader.Find(shader));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Cull", 0f); // se ve de los dos lados
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return m;
        }

        static Transform Buscar(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void Carpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            var padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
            Carpeta(padre);
            AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
        }
    }
}
