using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BioVR.Comun.Editor;
using BioVR.Entornos;
using BioVR.Entornos.Editor;
using BioVR.Secuencia;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BioVR.Prologo.Editor
{
    // Menú BioVR → Prólogo → Armar prólogo.
    //
    // Hace Escenas/Prologo.unity: un espacio oscuro con estrellas donde el jugador, quieto, ve pasar los 13 modelos
    // de la pasarela de Diseño (#91, 03-diseno/documentacion/pasarela-prologo) mientras Emi narra el prólogo
    // (Comun/Audio/VOZ, con la transcripción de Sonido para los subtítulos). Botón "Saltar prólogo".
    // Al terminar carga 00-EscenaInicial. Deja Build Settings: Prologo (0), 00-EscenaInicial, Cocina.
    // Por omisión va la versión de la feria (01, 05, 09 y 11); se cambia en el objeto Pasarela (Version Feria).
    // Se puede correr varias veces.
    public static class ArmarPrologo
    {
        public const string Escena = "Assets/_BioVR/Escenas/Prologo.unity";
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string EscenaCocina = "Assets/_BioVR/Escenas/Cocina.unity";
        const string Rig = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string MaterialFade = "Assets/_BioVR/Secuencia/FadeMat.mat";
        const string Modelos = "Assets/_BioVR/Modelos/Prologo";
        const string Materiales = "Assets/_BioVR/Prologo/Materiales";
        const string Voz = "Assets/_BioVR/Comun/Audio/VOZ/";
        const string Transcripcion = Voz + "VOZ-NAR-03_Transcripcion_prologo_Emi.txt";
        const string Musica = "Assets/_BioVR/Comun/Audio/MUS/MUS-PRO-02_prologo_v01.mp3"; // música del prólogo (prologo-game.mp3)
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/

        // Dónde pasa todo, respecto al jugador sentado (ojos a ~1.2 m)
        static readonly Vector3 Centro = new Vector3(0f, 1.25f, 1.9f);
        static readonly Vector3 Ojos = new Vector3(0f, 1.2f, 0f);
        static readonly int[] Feria = { 1, 5, 9, 11 };

        [MenuItem("BioVR/Prólogo/Armar prólogo")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Carpeta(Modelos);
            Carpeta(Materiales);
            CopiarTexturas();
            var seraphim = PonerNaveSeraphim.ImportarSeraphim();

            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- Ambiente: oscuro, con luz suave desde arriba y estrellas ----
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.28f, 0.35f);
            var sol = new GameObject("Luz", typeof(Light));
            sol.GetComponent<Light>().type = LightType.Directional;
            sol.GetComponent<Light>().intensity = 1.1f;
            sol.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            Estrellas();

            // ---- Jugador: rig de XR quieto (sin caminar), cámara con fondo azul muy oscuro, fundido ----
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Rig));
            var origen = rig.GetComponent<XROrigin>();
            var locomocion = rig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Locomotion");
            if (locomocion != null) locomocion.gameObject.SetActive(false);
            origen.Camera.clearFlags = CameraClearFlags.SolidColor;
            origen.Camera.backgroundColor = new Color(0.01f, 0.015f, 0.04f);
            UIVR.AsegurarEventSystem();

            var raiz = new GameObject("Prologo");
            var fader = raiz.AddComponent<ScreenFader>();
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Fundido";
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(origen.Camera.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            quad.transform.localScale = new Vector3(2f, 2f, 1f);
            var rq = quad.GetComponent<MeshRenderer>();
            rq.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialFade);
            rq.shadowCastingMode = ShadowCastingMode.Off;
            rq.enabled = false;
            var fso = new SerializedObject(fader);
            fso.FindProperty("quad").objectReferenceValue = rq;
            fso.ApplyModifiedPropertiesWithoutUndo();

            var centro = new GameObject("Centro").transform;
            centro.SetParent(raiz.transform, false);
            centro.position = Centro;
            var foco = new GameObject("Foco", typeof(Light));
            foco.transform.SetParent(raiz.transform, false);
            foco.transform.SetPositionAndRotation(Centro + new Vector3(0f, 2.5f, -0.8f), Quaternion.Euler(70f, 0f, 0f));
            var lf = foco.GetComponent<Light>();
            lf.type = LightType.Spot;
            lf.range = 6f;
            lf.spotAngle = 60f;
            lf.intensity = 3f;

            // ---- Modelos de la pasarela ----
            var contenedor = new GameObject("Modelos").transform;
            contenedor.SetParent(raiz.transform, false);
            var modelos = CrearModelos(contenedor, seraphim);

            // ---- Sonido ----
            var voz = new GameObject("Voz", typeof(AudioSource)).GetComponent<AudioSource>();
            voz.transform.SetParent(raiz.transform, false);
            voz.playOnAwake = false;
            voz.spatialBlend = 0f;
            var musica = new GameObject("Musica", typeof(AudioSource)).GetComponent<AudioSource>();
            musica.transform.SetParent(raiz.transform, false);
            musica.playOnAwake = false;
            musica.loop = true;
            musica.volume = 0.4f; // abajo de la voz, pero presente
            musica.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Musica);

            // ---- Subtítulos con fondo oscuro, abajo de la pasarela ----
            var posSub = new Vector3(0f, 0.62f, 1.5f);
            var fondo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fondo.name = "Fondo de subtitulos";
            Object.DestroyImmediate(fondo.GetComponent<Collider>());
            fondo.transform.SetParent(raiz.transform, false);
            fondo.transform.SetPositionAndRotation(posSub + new Vector3(0f, 0f, 0.01f), Quaternion.LookRotation(posSub - Ojos));
            fondo.transform.localScale = new Vector3(1.8f, 0.34f, 1f);
            fondo.GetComponent<MeshRenderer>().sharedMaterial = MaterialPlano("Prologo_FondoSubtitulo", new Color(0f, 0f, 0f, 0.55f), transparente: true);
            var sub = new GameObject("Subtitulo", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            sub.transform.SetParent(raiz.transform, false);
            sub.transform.SetPositionAndRotation(posSub, Quaternion.LookRotation(posSub - Ojos));
            sub.rectTransform.sizeDelta = new Vector2(1.7f, 0.3f);
            sub.enableAutoSizing = true;
            sub.fontSizeMin = 0.4f;
            sub.fontSizeMax = 0.75f;
            sub.alignment = TextAlignmentOptions.Center;
            sub.color = new Color(0.92f, 0.95f, 1f);

            // ---- Botón para saltar ----
            var pasarela = raiz.AddComponent<Pasarela>();
            var posBoton = new Vector3(0.95f, 0.85f, 1.15f);
            var panel = UIVR.Panel("Panel saltar", raiz.transform, new Vector2(260f, 90f), origen.Camera);
            panel.transform.SetPositionAndRotation(posBoton, Quaternion.LookRotation(posBoton - Ojos));
            var boton = UIVR.Boton(panel.transform, "Boton Saltar", "Saltar prólogo", UIVR.ColorSalir, pasarela.Saltar);
            boton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 30f;
            UIVR.CapaDeUI(panel);

            // ---- Entradas: modelo + voz + texto de la transcripción ----
            var textos = Textos();
            var so = new SerializedObject(pasarela);
            var lista = so.FindProperty("entradas");
            lista.arraySize = 13;
            for (int n = 1; n <= 13; n++)
            {
                var e = lista.GetArrayElementAtIndex(n - 1);
                e.FindPropertyRelative("numero").intValue = n;
                e.FindPropertyRelative("modelo").objectReferenceValue = modelos.TryGetValue(n, out var m) ? m : null;
                e.FindPropertyRelative("voz").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{Voz}VOZ-NAR-01_prologo-{n:00}_v01.wav");
                e.FindPropertyRelative("texto").stringValue = textos.TryGetValue(n, out var t) ? t : "";
                e.FindPropertyRelative("tipo").enumValueIndex = n == 2 ? (int)Pasarela.Tipo.Reemplaza : (int)Pasarela.Tipo.Pasa;
                e.FindPropertyRelative("enFeria").boolValue = Feria.Contains(n);
            }
            so.FindProperty("centro").objectReferenceValue = centro;
            so.FindProperty("voz").objectReferenceValue = voz;
            so.FindProperty("musica").objectReferenceValue = musica;
            so.FindProperty("subtitulo").objectReferenceValue = sub;
            so.FindProperty("fader").objectReferenceValue = fader;
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(Escena));
            EditorSceneManager.SaveScene(escena, Escena);
            OrdenEscenas();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Prologo] Listo: {Escena} con {modelos.Count} modelos. Versión de la feria: 01, 05, 09 y 11 " +
                      "(se cambia en Prologo → Pasarela → Version Feria). Build Settings: Prologo, 00-EscenaInicial, Cocina.");
        }

        // El prólogo es la escena 0 (con ella abre la app y a ella regresa la pantalla final de la Cocina)
        public static void OrdenEscenas()
        {
            var primeras = new[] { Escena, EscenaInicial, EscenaCocina }.Where(p => AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null).ToArray();
            var otras = EditorBuildSettings.scenes.Where(e => !primeras.Contains(e.path));
            EditorBuildSettings.scenes = primeras.Select(p => new EditorBuildSettingsScene(p, true)).Concat(otras).ToArray();
        }

        // ---------------- Modelos ----------------

        static Dictionary<int, GameObject> CrearModelos(Transform padre, GameObject seraphim)
        {
            var r = new Dictionary<int, GameObject>();
            var holograma = MaterialPlano("Prologo_Holograma", new Color(0.3f, 0.9f, 1f, 0.4f), transparente: true);

            r[1] = Tierra(padre, "01 La Tierra", "pasarela-tierra.fbx", "M_tierra", "tierra_dia_2k.jpg", "tierra_noche_emision_2k.jpg");
            r[2] = Tierra(padre, "02 La Tierra seca", "pasarela-tierra-seca.fbx", "M_tierra_seca", "tierra_seca_dia_2k.jpg", "tierra_seca_noche_emision_2k.jpg");

            var ciudad = Uno(padre, "03 Ciudad bajo el domo", Importar("pasarela-ciudad-domo.fbx"), 1.2f);
            Reemplazar(ciudad, "M_domo_vidrio", MaterialPlano("Prologo_DomoVidrio", new Color(0.6f, 0.85f, 1f, 0.18f), transparente: true));
            r[3] = ciudad;

            var silueta = Uno(padre, "04 Silueta humana", Importar("paciente.fbx", sinMateriales: true), 1.6f);
            foreach (var rend in silueta.GetComponentsInChildren<Renderer>(true))
                rend.sharedMaterials = Enumerable.Repeat(holograma, rend.sharedMaterials.Length).ToArray();
            r[4] = silueta;

            r[5] = Uno(padre, "05 Logo Proyecto Cellverse", Importar("pasarela-logo-cellverse.fbx"), 1.1f);

            // 06: el proyector de holograma sobre la mesa curva, a 0.78 m (los dos de Uli, ya en la Cocina)
            var lab = new GameObject("06 Laboratorio");
            lab.transform.SetParent(padre, false);
            var mesa = Instanciar(Cargar("Assets/_BioVR/Minijuegos/Cocina/Modelos/Uli/mesa-curva.fbx"), lab.transform);
            var proyector = Instanciar(Cargar("Assets/_BioVR/Minijuegos/Cocina/Modelos/Uli/proyector-holograma.fbx"), lab.transform);
            if (mesa != null && proyector != null)
            {
                var bm = Limites(mesa);
                var bp = Limites(proyector);
                proyector.transform.position += new Vector3(bm.center.x - bp.center.x, bm.max.y - bp.min.y, bm.center.z - bp.center.z);
            }
            r[6] = Normalizar(lab, 1.2f);

            r[7] = Uno(padre, "07 La celula", Cargar("Assets/_BioVR/Modelos/EscenarioCelular/escenario-celular_lowpoly.fbx"), 1.6f);

            // 08: el tablero con una pantalla roja de alerta encima
            var alerta = new GameObject("08 Pantalla de alerta");
            alerta.transform.SetParent(padre, false);
            var tablero = Instanciar(Cargar("Assets/_BioVR/Minijuegos/Cocina/Modelos/Uli/tablero-pedido.fbx"), alerta.transform);
            if (tablero != null)
            {
                var bt = Limites(tablero);
                var pantalla = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pantalla.name = "Pantalla roja";
                Object.DestroyImmediate(pantalla.GetComponent<Collider>());
                pantalla.transform.SetParent(alerta.transform, false);
                pantalla.transform.position = new Vector3(bt.center.x, bt.center.y, bt.min.z - 0.01f);
                pantalla.transform.localScale = new Vector3(bt.size.x * 0.8f, bt.size.y * 0.7f, 1f);
                pantalla.GetComponent<MeshRenderer>().sharedMaterial = MaterialPlano("Prologo_Alerta", new Color(0.9f, 0.08f, 0.08f, 1f), transparente: false);
                var signo = new GameObject("Signo", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
                signo.transform.SetParent(pantalla.transform, false);
                signo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                signo.rectTransform.sizeDelta = new Vector2(1f, 1f);
                signo.text = "⚠\n<size=40%>ALERTA</size>";
                signo.enableAutoSizing = true;
                signo.fontSizeMin = 0.1f;
                signo.fontSizeMax = 3f;
                signo.alignment = TextAlignmentOptions.Center;
                signo.color = new Color(1f, 0.95f, 0.85f);
            }
            r[8] = Normalizar(alerta, 1.1f);

            // 09: grumo de 7 amilinas
            r[9] = Grupo(padre, "09 Grumo de amilina", Importar("amilina_modelo.fbx"), 7, 0.45f, 1.1f, semilla: 9);
            r[10] = Uno(padre, "10 Ribosoma 80S", Cargar("Assets/_BioVR/Minijuegos/ExpresionGenetica/Modelos/Diseno/ribosoma_80S_4ug0.fbx"), 1.1f);

            // 11: la Seraphim, plegada; se despliega al llegar al centro
            var nave = Uno(padre, "11 El Mecha Seraphim", seraphim, 1.7f, seraphim != null ? PonerNaveSeraphim.MedirCorreccion(seraphim) : (Quaternion?)null);
            if (nave != null)
            {
                var nso = new SerializedObject(nave.AddComponent<NaveDespliegue>()); // busca la animación en sus hijos
                nso.FindProperty("seguirAlJuego").boolValue = false;
                nso.FindProperty("sonidoDespliegue").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_BioVR/Comun/Audio/SFX/SFX-MEC-02_apoyo-pesado_v01.wav");
                nso.ApplyModifiedPropertiesWithoutUndo();
            }
            r[11] = nave;

            r[12] = Grupo(padre, "12 Eritrocitos", Cargar("Assets/_BioVR/Modelos/Torrente/eritrocito.fbx"), 3, 0.6f, 1.1f, semilla: 12);
            r[13] = Uno(padre, "13 El paciente", Importar("paciente-en-camilla_lowpoly.fbx"), 1.6f);

            foreach (var go in r.Values.Where(g => g != null)) go.transform.position = Centro;
            return r;
        }

        // La Tierra: su material con el día de base y las luces de la noche como emisión; la atmósfera, translúcida
        static GameObject Tierra(Transform padre, string nombre, string fbx, string materialTierra, string dia, string noche)
        {
            var go = Uno(padre, nombre, Importar(fbx), 1.0f);
            if (go == null) return null;
            var m = MaterialPlano("Prologo_" + Path.GetFileNameWithoutExtension(fbx), Color.white, transparente: false, lit: true);
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Modelos}/texturas/pasarela-prologo/{dia}"));
            m.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Modelos}/texturas/pasarela-prologo/{noche}"));
            m.SetColor("_EmissionColor", Color.white * 0.8f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            Reemplazar(go, materialTierra, m);
            Reemplazar(go, "M_atmosfera", MaterialPlano("Prologo_Atmosfera", new Color(0.45f, 0.7f, 1f, 0.15f), transparente: true));
            return go;
        }

        static GameObject Uno(Transform padre, string nombre, GameObject fbx, float lado, Quaternion? giro = null)
        {
            if (fbx == null) return null;
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            // Con giro de corrección, el giro va en un objeto intermedio (la animación del modelo no lo toca)
            var padreModelo = go.transform;
            if (giro.HasValue)
            {
                padreModelo = new GameObject("Giro").transform;
                padreModelo.SetParent(go.transform, false);
                padreModelo.localRotation = giro.Value;
            }
            Instanciar(fbx, padreModelo);
            return Normalizar(go, lado);
        }

        static GameObject Grupo(Transform padre, string nombre, GameObject fbx, int cuantos, float separacion, float lado, int semilla)
        {
            if (fbx == null) return null;
            var azar = new System.Random(semilla);
            float R() => (float)azar.NextDouble() * 2f - 1f;
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            float tam = 0f;
            for (int i = 0; i < cuantos; i++)
            {
                var c = Instanciar(fbx, go.transform);
                if (tam == 0f) { var b = Limites(c); tam = Mathf.Max(b.size.x, b.size.y, b.size.z); }
                c.transform.position = new Vector3(R(), R(), R()) * separacion * tam;
                c.transform.rotation = Quaternion.Euler(R() * 180f, R() * 180f, R() * 180f);
            }
            return Normalizar(go, lado);
        }

        // Escala el contenido para que su lado mayor mida "lado" metros y lo centra en el origen del objeto
        // (la pasarela mueve y gira ese objeto)
        static GameObject Normalizar(GameObject go, float lado)
        {
            var hijos = go.transform.Cast<Transform>().ToArray();
            var contenido = new GameObject("Contenido").transform;
            contenido.SetParent(go.transform, false);
            foreach (var h in hijos) h.SetParent(contenido, true);
            var b = Limites(contenido.gameObject);
            float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (mayor > 0f) contenido.localScale = Vector3.one * (lado / mayor);
            b = Limites(contenido.gameObject);
            contenido.position -= b.center - go.transform.position;
            return go;
        }

        static GameObject Instanciar(GameObject fbx, Transform padre)
        {
            if (fbx == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, padre);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        static void Reemplazar(GameObject go, string material, Material nuevo)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                r.sharedMaterials = r.sharedMaterials.Select(m => m != null && m.name.StartsWith(material) ? nuevo : m).ToArray();
        }

        // ---------------- Importar ----------------

        // Copia un FBX de 03-diseno/exports a Modelos/Prologo (si falta) y lo importa sin cámaras ni luces
        static GameObject Importar(string archivo, bool sinMateriales = false)
        {
            string destino = $"{Modelos}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[Prologo] No encontré {origen}."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter mi &&
                (mi.importCameras || mi.importLights || (sinMateriales && mi.materialImportMode != ModelImporterMaterialImportMode.None)))
            {
                mi.importCameras = false;
                mi.importLights = false;
                if (sinMateriales) mi.materialImportMode = ModelImporterMaterialImportMode.None;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(destino);
        }

        static GameObject Cargar(string ruta)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            if (go == null) Debug.LogWarning($"[Prologo] Falta {ruta}.");
            return go;
        }

        // Las texturas de la pasarela, con la misma ruta relativa que buscan los FBX (texturas/pasarela-prologo/)
        static void CopiarTexturas()
        {
            string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "texturas/pasarela-prologo"));
            string destino = $"{Modelos}/texturas/pasarela-prologo";
            if (!Directory.Exists(origen)) { Debug.LogWarning($"[Prologo] No encontré {origen}."); return; }
            Carpeta(destino);
            foreach (var f in Directory.GetFiles(origen))
            {
                string d = $"{destino}/{Path.GetFileName(f)}";
                if (!File.Exists(d)) { File.Copy(f, d); AssetDatabase.ImportAsset(d); }
                if (AssetImporter.GetAtPath(d) is TextureImporter ti && ti.maxTextureSize > 2048) { ti.maxTextureSize = 2048; ti.SaveAndReimport(); }
            }
        }

        // ---------------- Textos ----------------

        // [01] ... [13] de la transcripción de Sonido
        static Dictionary<int, string> Textos()
        {
            var r = new Dictionary<int, string>();
            var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(Transcripcion);
            if (ta == null) { Debug.LogWarning($"[Prologo] Falta {Transcripcion}: no habrá subtítulos."); return r; }
            foreach (Match m in Regex.Matches(ta.text, @"\[(\d+)\]\s*(.*?)(?=\s*\[\d+\]|\s*$)", RegexOptions.Singleline))
                r[int.Parse(m.Groups[1].Value)] = Regex.Replace(m.Groups[2].Value.Trim(), @"\s+", " ");
            return r;
        }

        // ---------------- Ambiente ----------------

        static void Estrellas()
        {
            var go = new GameObject("Estrellas", typeof(ParticleSystem));
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.7f, 0.85f, 1f), new Color(1f, 0.9f, 0.8f));
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 500) });
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 14f;
            sh.radiusThickness = 0.3f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                MaterialParticulas("Prologo_Estrella");
        }

        // ---------------- Materiales ----------------

        static Material MaterialPlano(string nombre, Color color, bool transparente, bool lit = false)
        {
            string ruta = $"{Materiales}/{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find(lit ? "Universal Render Pipeline/Lit" : "Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", color);
            if (transparente)
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)RenderQueue.Transparent;
            }
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }

        static Material MaterialParticulas(string nombre)
        {
            string ruta = $"{Materiales}/{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true).Where(r => !(r is ParticleSystemRenderer)).ToArray();
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
