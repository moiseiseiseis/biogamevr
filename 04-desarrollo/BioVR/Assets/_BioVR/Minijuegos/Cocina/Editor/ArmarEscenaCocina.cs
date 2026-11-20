using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M9 (Moisés) · Menú BioVR → Cocina → Armar escena Cocina.
    //
    // Arma Escenas/Cocina.unity, la escena integrada, a partir de Cocina_Base: nadie la edita a mano,
    // si hace falta algo se agrega aquí y se vuelve a correr (especificación 1.1).
    //
    //   - Módulos que ya están en main: M2 corriente, M3 sintetasa, M4 ribosomas, M6 imprevistos, M7 holograma
    //     (Cocina_Holograma.prefab, de BioVR → Cocina → Armar holograma (M7)).
    //     Cada uno en su marca de AnclaCocina/Puntos, igual que en su escena de prueba (sin los atajos de prueba).
    //   - Conexión M2 ↔ M4 (ConexionCorrienteRibosomas) y el destino "ribosoma lejano" del lanzamiento.
    //   - Sin cabina: en la Cocina el jugador solo ve las pinzas del Mecha en primera persona (decisión del 20-nov).
    //   - Fondo 360° (Skybox Panoramic) y el escenario celular low poly de Diseño (#59).
    //   - M10 (fase B): la BiP y el proteasoma de BioVR → Cocina → Armar fase B (M10), si ya se corrió.
    //   - M8 (Marcos): panel de mensajes, tablero del pedido, tutorial (con sus objetivos en las marcas),
    //     pantalla final y sonidos, donde los puso en Cocina_M8; y el menú de pausa de la escena inicial,
    //     con Reiniciar turno y Reiniciar tutorial.
    //   - M5 todavía no existe: su marca se queda visible en Play para saber dónde va.
    //
    // Se puede correr varias veces: la escena se vuelve a hacer desde Cocina_Base y conserva su GUID.
    public static class ArmarEscenaCocina
    {
        public const string Escena = "Assets/_BioVR/Escenas/Cocina.unity";

        const string Raiz = CrearDatosCocina.Raiz;
        const string Prefabs = Raiz + "/Prefabs/";
        const string CarpetaFondo = Raiz + "/Fondo";
        const string CarpetaEscenario = "Assets/_BioVR/Modelos/EscenarioCelular";
        const string ModeloEscenario = CarpetaEscenario + "/escenario-celular_lowpoly.fbx";
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/

        // Espacio de juego respecto a AnclaCocina: jugador, mesa, ribosoma lejano (1.9 m) y paneles (hasta 2 m al frente)
        static readonly Bounds EspacioJuego = new Bounds(new Vector3(0f, 1.25f, 0.6f), new Vector3(4.5f, 2.5f, 4f));

        [MenuItem("BioVR/Cocina/Armar escena Cocina")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ArmarCocinaBase.Escena))
            {
                Debug.LogError($"[Cocina] No existe {ArmarCocinaBase.Escena}. Córrela primero: BioVR → Cocina → Armar escena base.");
                return;
            }

            // Recursos que se copian de 03-diseno/exports (antes de abrir la escena: importar cambia la base de datos)
            var fondo = MaterialFondo();
            var escenario = ImportarEscenario();

            var escena = EditorSceneManager.OpenScene(ArmarCocinaBase.Escena, OpenSceneMode.Single);
            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError($"[Cocina] No se pudo guardar {Escena}."); return; }

            var raices = escena.GetRootGameObjects();
            var ancla = raices.FirstOrDefault(g => g.name == "AnclaCocina");
            if (ancla == null) { Debug.LogError("[Cocina] Cocina_Base no tiene AnclaCocina. Vuelve a armarla."); return; }
            // AnclaCocina se guarda apagada (la enciende AnclarAlJugador); encendida mientras se arma para que
            // los Renderer.bounds del escenario sean válidos
            ancla.SetActive(true);
            var a = ancla.transform;
            var lejano = Ribosomas(a);
            Corriente(a, lejano);
            Sintetasa(a);
            Imprevistos(a);
            Holograma(a);
            FaseB(a);
            Interfaz(a);
            MusicaYAmbiente(a);
            BioVR.Comun.Editor.AgregarMenuPausa.ArmarEn(escena, BioVR.Comun.Editor.AgregarMenuPausa.TextoControlesCocina, botonesCocina: true);
            new GameObject("Conexiones (M9)", typeof(ConexionCorrienteRibosomas)).transform.SetParent(a, false);

            if (escenario != null) Escenario(a, escenario);
            if (fondo != null)
            {
                RenderSettings.skybox = fondo;
                DynamicGI.UpdateEnvironment();
            }

            ancla.SetActive(false);
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Cocina] Listo: {Escena}. Falta M5 (salida): su marca se ve en Play.");
        }

        // ---------------- Módulos ----------------

        // M4: los dos ribosomas en sus marcas. Devuelve el lejano (destino del lanzamiento de M2).
        static RibosomaCocina Ribosomas(Transform ancla)
        {
            PonerEnMarca(Prefab("Cocina_Ribosoma"), ancla, "Ribosoma de mesa");
            var lejano = PonerEnMarca(Prefab("Cocina_RibosomaLejano"), ancla, "Ribosoma lejano");
            return lejano != null ? lejano.GetComponent<RibosomaCocina>() : null;
        }

        // M2: el mismo arreglo que Cocina_M2 (Sistema_Corriente con CorrienteARNt, VueloARNt y el tubo)
        static void Corriente(Transform ancla, RibosomaCocina lejano)
        {
            var tubo = Prefab("Cocina_Corriente");
            var arnt = Prefab("Cocina_ARNt");
            if (tubo == null || arnt == null) return;

            var sistema = new GameObject("Sistema_Corriente");
            sistema.transform.SetParent(ancla, false);
            var vuelo = sistema.AddComponent<VueloARNt>();
            var corriente = sistema.AddComponent<CorrienteARNt>();
            var cso = new SerializedObject(corriente);
            cso.FindProperty("anclaCocina").objectReferenceValue = ancla;
            cso.FindProperty("prefabARNt").objectReferenceValue = arnt;
            cso.FindProperty("visualTubo").objectReferenceValue = tubo;
            cso.ApplyModifiedPropertiesWithoutUndo();
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(tubo, sistema.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            // Destino del lanzamiento: el sitio A del lejano. El regreso al citoplasma lo conecta M5 cuando exista.
            if (lejano != null)
            {
                var sitioA = new SerializedObject(lejano).FindProperty("sitioA").objectReferenceValue as Transform;
                var vso = new SerializedObject(vuelo);
                vso.FindProperty("destinoRibosomaLejano").objectReferenceValue = sitioA;
                vso.ApplyModifiedPropertiesWithoutUndo();
            }
            OcultarMarca(ancla, "Corriente inicio");
            OcultarMarca(ancla, "Corriente fin");
        }

        // M3: donde la puso Genaro en Cocina_M3 (0.78 m, girada -46°), pero colgada de la marca "Sintetasa"
        // para que la mesa giratoria del turno 3 (M6) la mueva.
        static void Sintetasa(Transform ancla)
        {
            var prefab = Prefab("Cocina_Sintetasa");
            var marca = Marca(ancla, "Sintetasa");
            if (prefab == null || marca == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, ancla);
            go.transform.localPosition = new Vector3(-0.4748f, 0.78f, 0.4585f);
            go.transform.localRotation = Quaternion.Euler(0f, -46f, 0f);
            go.transform.SetParent(marca, true);
            OcultarMarca(ancla, "Sintetasa");
        }

        // M6: igual que Cocina_M6, sin PruebaImprevistos (atajos de teclado)
        static void Imprevistos(Transform ancla)
        {
            var grupo = new GameObject("Imprevistos (M6)");
            grupo.transform.SetParent(ancla, false);
            grupo.AddComponent<GestorImprevistos>();
            var mso = new SerializedObject(grupo.AddComponent<MesaGiratoria>());
            mso.FindProperty("sintetasa").objectReferenceValue = Marca(ancla, "Sintetasa");
            mso.FindProperty("salida").objectReferenceValue = Marca(ancla, "Salida");
            mso.ApplyModifiedPropertiesWithoutUndo();

            var prefab = Prefab("Cocina_Horquilla");
            var ribosoma = Marca(ancla, "Ribosoma de mesa");
            if (prefab == null || ribosoma == null) return;
            var horquilla = (GameObject)PrefabUtility.InstantiatePrefab(prefab, grupo.transform);
            var h = horquilla.GetComponent<HorquillaARNm>();
            var hso = new SerializedObject(h);
            hso.FindProperty("ribosoma").objectReferenceValue = ribosoma;
            hso.ApplyModifiedPropertiesWithoutUndo();
            horquilla.transform.SetPositionAndRotation(ribosoma.TransformPoint(h.posicionEnRibosoma), ribosoma.rotation);
        }

        // M7: Cocina_Holograma.prefab (BioVR → Cocina → Armar holograma (M7)) en la marca "Holograma"
        static void Holograma(Transform ancla)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArmarHolograma.RutaPrefab);
            if (prefab == null)
            {
                Debug.LogWarning($"[Cocina] Falta {ArmarHolograma.RutaPrefab}. Córrelo primero: BioVR → Cocina → Armar holograma (M7).");
                return;
            }
            PonerEnMarca(prefab, ancla, "Holograma");
        }

        // M10: la BiP y el proteasoma en sus marcas (solo se ven en la fase B)
        static void FaseB(Transform ancla)
        {
            var bip = AssetDatabase.LoadAssetAtPath<GameObject>(ArmarFaseB.RutaBiP);
            var proteasoma = AssetDatabase.LoadAssetAtPath<GameObject>(ArmarFaseB.RutaProteasoma);
            if (bip == null || proteasoma == null)
            {
                Debug.LogWarning("[Cocina] Faltan Cocina_BiP o Cocina_Proteasoma. Córrelo primero: BioVR → Cocina → Armar fase B (M10).");
                return;
            }
            PonerEnMarca(bip, ancla, "Chaperona (BiP)");
            PonerEnMarca(proteasoma, ancla, "Proteasoma (26S)");
        }

        // M8: donde Marcos los puso en Cocina_M8 (posición respecto a AnclaCocina; los Canvas miran al jugador sin girarlos)
        static readonly (string prefab, Vector3 posicion)[] PiezasInterfaz =
        {
            ("Cocina_Sonidos", Vector3.zero),
            ("CocinaMarcaTutorial", Vector3.zero),
            ("Cocina_PanelMensajes", new Vector3(-0.9f, 1.4f, 1.3f)),
            ("Cocina_TableroPedido", new Vector3(0f, 2f, 1.5f)),
            ("CocinaTutorial", new Vector3(0.9f, 1.4f, 1.3f)),
            ("Cocina_PantallaFinal", new Vector3(0f, 1.4f, 1.2f)),
        };

        // A qué marca señala cada ventana del tutorial (las que no aparecen aquí no señalan nada)
        static readonly (string clave, string marca)[] ObjetivosTutorial =
        {
            ("ribosoma_mesa", "Ribosoma de mesa"), ("corriente", "Corriente inicio"), ("sintetasa", "Sintetasa"),
            ("llave", "Ribosoma de mesa"), ("boton_fin", "Ribosoma de mesa"), ("sintetasa_error", "Sintetasa"),
            ("salida", "Salida"), ("reciclaje", "Regreso al citoplasma"), ("ribosoma_lejano", "Ribosoma lejano"),
            ("horquilla", "Ribosoma de mesa"), ("codon_raro", "Corriente inicio"), ("fase_b", "Holograma"),
            ("busca", "Holograma"), ("chaperona", "Chaperona (BiP)"), ("decide", "Proteasoma (26S)"),
        };

        static void Interfaz(Transform ancla)
        {
            GameObject tutorial = null, marca = null;
            foreach (var (nombre, posicion) in PiezasInterfaz)
            {
                var prefab = Prefab(nombre);
                if (prefab == null) continue;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, ancla);
                go.transform.localPosition = posicion;
                go.transform.localRotation = Quaternion.identity;
                if (nombre == "CocinaTutorial") tutorial = go;
                if (nombre == "CocinaMarcaTutorial") marca = go;
            }
            OcultarMarca(ancla, "Panel de mensajes");
            OcultarMarca(ancla, "Tablero del pedido");
            if (tutorial == null) return;

            var so = new SerializedObject(tutorial.GetComponent<TutorialCocina>());
            if (marca != null) so.FindProperty("marcaTutorial").objectReferenceValue = marca.GetComponent<MarcaTutorial>();
            var lista = so.FindProperty("objetivos");
            lista.arraySize = ObjetivosTutorial.Length;
            for (int i = 0; i < ObjetivosTutorial.Length; i++)
            {
                var e = lista.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("clave").stringValue = ObjetivosTutorial[i].clave;
                e.FindPropertyRelative("objetivo").objectReferenceValue = Marca(ancla, ObjetivosTutorial[i].marca);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Música del minijuego y ambiente celular en loop, en 2D. El ambiente va 6 dB abajo (fila AMB-MIN-01 de
        // la plantilla de Sonido) para que no tape los efectos.
        static void MusicaYAmbiente(Transform ancla)
        {
            var go = new GameObject("Musica y ambiente");
            go.transform.SetParent(ancla, false);
            Loop(go, Raiz + "/Audio/MUS/MUS-MIN-01_traduccion-loop_v01.wav", 0.6f);
            Loop(go, Raiz + "/Audio/AMB/AMB-CEL-01_interior-celular-loop_v01.wav", 0.6f * 0.5f); // −6 dB
        }

        static void Loop(GameObject go, string ruta, float volumen)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
            if (clip == null) { Debug.LogWarning($"[Cocina] Falta {ruta}."); return; }
            var fuente = go.AddComponent<AudioSource>();
            fuente.clip = clip;
            fuente.loop = true;
            fuente.playOnAwake = true;
            fuente.spatialBlend = 0f;
            fuente.volume = volumen;
        }

        // ---------------- Escenario y fondo ----------------

        // El escenario low poly alrededor del jugador. Las piezas chicas que caen en el espacio de juego se apagan;
        // las grandes (membrana, núcleo) solo se reportan, porque su caja envuelve el espacio sin tocarlo.
        static void Escenario(Transform ancla, GameObject fbx)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, ancla);
            go.name = "Escenario celular (low poly)";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            int triangulos = 0;
            var volumen = new Bounds(ancla.TransformPoint(EspacioJuego.center), EspacioJuego.size);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var malla = r is SkinnedMeshRenderer s ? s.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
                if (malla != null) for (int i = 0; i < malla.subMeshCount; i++) triangulos += (int)malla.GetIndexCount(i) / 3;
                if (!r.bounds.Intersects(volumen)) continue;
                if (r.bounds.size.magnitude < 20f)
                {
                    r.gameObject.SetActive(false);
                    Debug.Log($"[Cocina] Escenario: se apagó {r.name} (caía en el espacio de juego).");
                }
                else Debug.Log($"[Cocina] Escenario: {r.name} ({r.bounds.size.magnitude:F0} m) envuelve el espacio de juego; revisar que no tape la mesa.");
            }
            Debug.Log($"[Cocina] Escenario low poly: {triangulos:N0} triángulos.");
        }

        // Copia el FBX low poly y sus texturas a Modelos/EscenarioCelular (las rutas relativas del FBX deben
        // coincidir: texturas/escenario-celular-lowpoly/*.png) y lo importa sin cámaras ni luces.
        static GameObject ImportarEscenario()
        {
            string origenTexturas = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "texturas/escenario-celular-lowpoly"));
            string destinoTexturas = CarpetaEscenario + "/texturas/escenario-celular-lowpoly";
            if (Directory.Exists(origenTexturas))
            {
                CrearDatosCocina.Carpeta(destinoTexturas);
                foreach (var png in Directory.GetFiles(origenTexturas, "*.png"))
                {
                    string destino = $"{destinoTexturas}/{Path.GetFileName(png)}";
                    if (!File.Exists(destino)) { File.Copy(png, destino); AssetDatabase.ImportAsset(destino); }
                }
            }
            else Debug.LogWarning($"[Cocina] No encontré {origenTexturas}: el escenario quedará sin texturas.");

            if (!File.Exists(ModeloEscenario))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "escenario-celular_lowpoly.fbx"));
                if (!File.Exists(origen)) { Debug.LogWarning($"[Cocina] No encontré {origen}: la escena queda sin escenario."); return null; }
                File.Copy(origen, ModeloEscenario);
                AssetDatabase.ImportAsset(ModeloEscenario);
            }
            if (AssetImporter.GetAtPath(ModeloEscenario) is ModelImporter mi && (mi.importCameras || mi.importLights))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(ModeloEscenario);
        }

        // Skybox Panoramic con fondo-celula-360-a.jpg (4096 × 2048 en Diseño; 2048 en el Quest)
        static Material MaterialFondo()
        {
            const string archivo = "fondo-celula-360-a.jpg";
            CrearDatosCocina.Carpeta(CarpetaFondo);
            string destino = $"{CarpetaFondo}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "minijuego-cocina/fondo/" + archivo));
                if (!File.Exists(origen)) { Debug.LogWarning($"[Cocina] No encontré {origen}: la escena queda sin fondo 360°."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is TextureImporter ti && (ti.maxTextureSize != 2048 || ti.mipmapEnabled))
            {
                ti.maxTextureSize = 2048;
                ti.mipmapEnabled = false;
                ti.wrapModeU = TextureWrapMode.Repeat;
                ti.wrapModeV = TextureWrapMode.Clamp;
                ti.SaveAndReimport();
            }

            string ruta = $"{CarpetaFondo}/Cocina_Fondo360.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null)
            {
                var shader = Shader.Find("Skybox/Panoramic");
                if (shader == null) { Debug.LogWarning("[Cocina] No está el shader Skybox/Panoramic."); return null; }
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, ruta);
            }
            m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(destino));
            m.SetFloat("_Mapping", 1f);   // latitud-longitud
            m.SetFloat("_ImageType", 0f); // 360°
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------------- Utilidades ----------------

        static GameObject Prefab(string nombre)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>($"{Prefabs}{nombre}.prefab");
            if (p == null) Debug.LogWarning($"[Cocina] Falta {Prefabs}{nombre}.prefab.");
            return p;
        }

        internal static Transform Marca(Transform ancla, string nombre)
        {
            var t = ancla.Find("Puntos/" + nombre);
            if (t == null) Debug.LogWarning($"[Cocina] Falta la marca '{nombre}' en AnclaCocina/Puntos. Vuelve a armar Cocina_Base.");
            return t;
        }

        // La marca ya tiene su módulo: no se dibuja su esfera en Play
        internal static void OcultarMarca(Transform ancla, string nombre)
        {
            var punto = ancla.Find("Puntos/" + nombre)?.GetComponent<PuntoCocina>();
            if (punto != null) punto.mostrarEnJuego = false;
        }

        internal static GameObject PonerEnMarca(GameObject prefab, Transform ancla, string marca)
        {
            var punto = Marca(ancla, marca);
            if (prefab == null || punto == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, punto);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            OcultarMarca(ancla, marca);
            return go;
        }
    }
}
