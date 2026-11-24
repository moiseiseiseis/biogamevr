using System.Collections.Generic;
using System.IO;
using System.Linq;
using BioVR.Comun.Editor;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Pokedex.Editor
{
    // Menú BioVR → Agregar pokédex biológico.
    //
    //   1. Pokedex/Fichas/: las 16 fichas de Investigación (2ª versión, 18-nov) resumidas en 2 líneas.
    //      El texto es borrador de Desarrollo para que Investigación lo revise; se cambia en cada asset.
    //   2. Torrente: copia de 03-diseno/exports los modelos de Diego (eritrocito, plaquetas, neutrófilo, linfocito,
    //      monocito) a Modelos/Torrente y los pone en el flujo de VasoSanguineo.prefab en lugar de los discos
    //      provisionales (que se apagan). Eosinófilo y basófilo no tienen modelo: se usa el neutrófilo teñido.
    //   3. Célula: en el escenario low poly de EspacioCelular.prefab marca los organelos que tienen ficha
    //      (Descubrible + MeshCollider). Correr después de BioVR → Poner escenario celular (low poly).
    //   4. 00-EscenaInicial: objeto "Pokedex" con el panel de la cabina (a la derecha) y la etiqueta del rayo.
    // Se puede correr varias veces.
    public static class CrearPokedex
    {
        const string Raiz = "Assets/_BioVR/Pokedex";
        const string CarpetaFichas = Raiz + "/Fichas";
        const string CarpetaMateriales = Raiz + "/Materiales";
        const string CarpetaTorrente = "Assets/_BioVR/Modelos/Torrente";
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string PrefabVaso = "Assets/_BioVR/Entornos/VasoSanguineo/VasoSanguineo.prefab";
        const string PrefabCelula = "Assets/_BioVR/Entornos/EspacioCelular/EspacioCelular.prefab";
        const string Prefijo = "[Pokedex] ";

        const string ZonaTorrente = "Torrente sanguíneo", ZonaCelula = "Interior de la célula";

        // numero, archivo, nombre, zona, tamaño, descripción (2 líneas), curiosidad
        static readonly (int n, string archivo, string nombre, string zona, string tamano, string descripcion, string curiosidad)[] Fichas =
        {
            (1, "01-globulo-rojo", "Glóbulo rojo (eritrocito)", ZonaTorrente, "6 a 8 µm",
                "Disco flexible lleno de hemoglobina: lleva oxígeno de los pulmones a las células y regresa con dióxido de carbono.",
                "No tiene núcleo ni mitocondrias. Un adulto tiene unos 25 billones."),
            (2, "02-eosinofilo", "Eosinófilo", ZonaTorrente, "12 a 17 µm",
                "Glóbulo blanco de la inmunidad innata: combate parásitos y regula las alergias y la inflamación.",
                "Sus gránulos se tiñen de rojo anaranjado con eosina y dañan a los parásitos."),
            (3, "03-basofilo", "Basófilo", ZonaTorrente, "10 a 14 µm",
                "Libera histamina, que inflama y dilata los vasos en alergias y asma, y heparina, un anticoagulante natural.",
                "Es el glóbulo blanco menos abundante."),
            (4, "04-neutrofilo", "Neutrófilo", ZonaTorrente, "10 a 15 µm",
                "Primera línea de defensa: llega al sitio de la infección y fagocita bacterias y hongos.",
                "Es el glóbulo blanco más abundante: del 50 al 70 %."),
            (5, "05-linfocito", "Linfocito", ZonaTorrente, "7 a 15 µm",
                "Los B fabrican anticuerpos; los T destruyen células infectadas o tumorales; las NK eliminan células anómalas.",
                "Tienen memoria inmunológica a largo plazo."),
            (6, "06-plaqueta", "Plaqueta (trombocito)", ZonaTorrente, "1.5 a 4 µm",
                "Se pega y se agrupa donde se lastima un vaso para formar un tapón y detener el sangrado.",
                "No es una célula completa: es un fragmento de un megacariocito, sin núcleo."),
            (7, "07-monocito", "Monocito", ZonaTorrente, "12 a 20 µm",
                "Fagocita microbios y restos de células. Circula unas horas y pasa a los tejidos, donde se vuelve macrófago.",
                "Es el glóbulo blanco más grande de la sangre."),
            (8, "08-mitocondria", "Mitocondria", ZonaCelula, "0.5 a 1 µm de ancho",
                "Hace la respiración celular: produce ATP, la energía que usa la célula, y ayuda a regular el calcio.",
                "Tiene su propio ADN, una pista de su origen evolutivo."),
            (9, "09-reticulo-endoplasmatico", "Retículo endoplasmático", ZonaCelula, "red de cisternas y tubos",
                "El rugoso fabrica y transporta proteínas; el liso fabrica lípidos, desintoxica y guarda calcio.",
                "Envía lo que fabrica al aparato de Golgi dentro de vesículas."),
            (10, "10-membrana-nuclear", "Membrana nuclear", ZonaCelula, "dos bicapas con poros",
                "Envuelve el núcleo y separa el ADN del citoplasma; sus poros controlan el paso de ARN y proteínas.",
                "Su capa externa se continúa con el retículo endoplasmático."),
            (11, "11-nucleolo", "Nucléolo", ZonaCelula, "zona del núcleo sin membrana",
                "Fabrica el ARN ribosómico y arma las subunidades de los ribosomas.",
                "Las subunidades salen al citoplasma por los poros nucleares."),
            (12, "12-vacuola", "Vacuola", ZonaCelula, "en animales, vesículas pequeñas",
                "Guarda agua, sales, nutrientes y desechos; en las células animales, por poco tiempo.",
                "En las plantas puede ocupar hasta el 90 % de la célula."),
            (13, "13-citoesqueleto", "Citoesqueleto", ZonaCelula, "red de filamentos",
                "Microfilamentos, filamentos intermedios y microtúbulos: dan forma, mueven la célula y llevan vesículas.",
                "Se arma y se desarma todo el tiempo, según lo que necesite la célula."),
            (14, "14-ribosoma", "Ribosoma", ZonaCelula, "dos subunidades de ARN y proteínas",
                "Lee el ARN mensajero y une aminoácidos para fabricar proteínas.",
                "Sus subunidades se arman en el nucléolo."),
            (15, "15-aparato-de-golgi", "Aparato de Golgi", ZonaCelula, "cisternas aplanadas apiladas",
                "Recibe proteínas y lípidos del retículo, los modifica, los empaca en vesículas y los envía.",
                "Es el centro de clasificación y envío de la célula."),
            (16, "16-lisosoma", "Lisosoma", ZonaCelula, "vesículas con enzimas",
                "Sus enzimas digieren desechos, organelos dañados y microbios.",
                "Es el sistema de limpieza y defensa de la célula."),
            // Capilar (prueba del 23-nov: "en los capilares ya no sale la pokédex": ahí no había nada registrable)
            (17, "17-albumina", "Albúmina", ZonaCapilar, "unos 8 nm",
                "La proteína más abundante del plasma: retiene agua dentro de los vasos y transporta hormonas, ácidos grasos y fármacos.",
                "La fabrica el hígado."),
            (18, "18-anticuerpo", "Anticuerpo", ZonaCapilar, "unos 10 nm, en forma de Y",
                "Proteína de defensa que reconoce una molécula extraña (antígeno) y la marca para que el sistema inmune la elimine.",
                "Lo fabrican los linfocitos B; cada uno reconoce un antígeno distinto."),
        };

        const string ZonaCapilar = "Capilar";
        const string PrefabCapilar = "Assets/_BioVR/Entornos/Capilar/Capilar.prefab";

        // Piezas del escenario low poly → ficha. Se marca la pieza más alta cuyo nombre coincide.
        // "Nucleo" es exacto para no tomar los nucleosomas.
        static readonly (string[] prefijos, bool exacto, int ficha)[] Organelos =
        {
            (new[] { "Mitocondria" }, false, 8),
            (new[] { "REL", "RER", "RE_" }, false, 9), // RE_Completo_Rugoso_y_Liso en el escenario v2
            (new[] { "Nucleo" }, true, 10),
            (new[] { "Vesicula" }, false, 12),
            (new[] { "Microtubulo", "Actina", "Filamento" }, false, 13),
            (new[] { "Polirribosoma", "Ribosomas" }, false, 14),
            (new[] { "Golgi" }, false, 15),
            (new[] { "Lisosoma" }, false, 16),
        };

        // Torrente: modelo, ficha, cuántos, tamaño (lado mayor en m; el disco provisional del glóbulo medía 1.4 m)
        // y color si se tiñe. Tamaños en proporción a las fichas.
        // 24-nov: el vaso es el torrente de Ulises a escala real (1 µm = 1 m, tronco de 30 m; PonerTorrente). Los
        // tamaños de antes (glóbulo de 1.4 m en un tubo de 12 m) se multiplican para que el glóbulo mida 7.5 m como
        // los de Ulises, con un tope para que los glóbulos blancos quepan entre la cabina y la pared.
        const float EscalaTorrente = 7.5f / 1.4f, TamanoMaximo = 11f;
        static float T(float antes) => Mathf.Min(antes * EscalaTorrente, TamanoMaximo);
        static readonly (string fbx, int ficha, int cuantos, float tamano, Color? tinte)[] Celulas =
        {
            ("eritrocito.fbx", 1, 45, T(1.4f), null),
            ("plaquetas.fbx", 6, 10, T(0.5f), null),
            ("neutrofilo.fbx", 4, 2, T(2.2f), null),
            ("neutrofilo.fbx", 2, 1, T(2.4f), new Color(1f, 0.55f, 0.35f)),   // eosinófilo: gránulos rojo anaranjado
            ("neutrofilo.fbx", 3, 1, T(2.0f), new Color(0.55f, 0.35f, 0.85f)), // basófilo: violeta
            ("linfocito.fbx", 5, 1, T(1.8f), null),
            ("monocito.fbx", 7, 1, T(2.8f), null),
        };

        // Solo el torrente (lo usa PonerTorrente después de cambiar el vaso)
        public static void SoloTorrente()
        {
            var fichas = CrearFichas();
            Torrente(fichas, new HashSet<FichaPokedex>());
            AssetDatabase.SaveAssets();
        }

        static System.Random azar;

        [MenuItem("BioVR/Agregar pokédex biológico")]
        public static void Agregar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var fichas = CrearFichas();
            var usadas = new HashSet<FichaPokedex>();
            Torrente(fichas, usadas);
            Capilar(fichas, usadas);
            Celula(fichas, usadas);
            Escena(fichas.Values.Where(usadas.Contains).OrderBy(f => f.numero).ToArray());
            AssetDatabase.SaveAssets();
            var sinLugar = fichas.Values.Where(f => !usadas.Contains(f)).Select(f => f.nombre);
            Debug.Log($"[Pokedex] Listo: {usadas.Count} fichas en el viaje. Sin nada que apuntar: {string.Join(", ", sinLugar)}.");
        }

        // ---------------- Fichas ----------------

        static Dictionary<int, FichaPokedex> CrearFichas()
        {
            Carpeta(CarpetaFichas);
            var r = new Dictionary<int, FichaPokedex>();
            foreach (var f in Fichas)
            {
                string ruta = $"{CarpetaFichas}/{f.archivo}.asset";
                var a = AssetDatabase.LoadAssetAtPath<FichaPokedex>(ruta);
                if (a == null)
                {
                    // Solo al crearla: si ya existe, se respeta lo que haya corregido Investigación
                    a = ScriptableObject.CreateInstance<FichaPokedex>();
                    a.numero = f.n; a.nombre = f.nombre; a.zona = f.zona; a.tamano = f.tamano;
                    a.descripcion = f.descripcion; a.curiosidad = f.curiosidad;
                    AssetDatabase.CreateAsset(a, ruta);
                }
                r[f.n] = a;
            }
            return r;
        }

        // ---------------- Torrente ----------------

        static void Torrente(Dictionary<int, FichaPokedex> fichas, HashSet<FichaPokedex> usadas)
        {
            Carpeta(CarpetaTorrente);
            var raiz = PrefabUtility.LoadPrefabContents(PrefabVaso);
            var flujo = raiz.transform.Find("Globulos (flujo)");
            if (flujo == null) { Debug.LogError("[Pokedex] VasoSanguineo.prefab no tiene 'Globulos (flujo)'."); PrefabUtility.UnloadPrefabContents(raiz); return; }
            foreach (Transform t in flujo.Cast<Transform>().ToArray())
            {
                if (t.name.StartsWith(Prefijo)) Object.DestroyImmediate(t.gameObject);
                else t.gameObject.SetActive(false); // discos y esferas provisionales
            }

            azar = new System.Random(7);
            foreach (var c in Celulas)
            {
                var fbx = Importar(c.fbx);
                if (fbx == null) continue;
                var materiales = c.tinte.HasValue ? Tenidos(fbx, fichas[c.ficha].nombre, c.tinte.Value) : null;
                for (int i = 0; i < c.cuantos; i++)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, flujo);
                    go.name = $"{Prefijo}{fichas[c.ficha].nombre} {i + 1}";
                    Ajustar(go.transform, c.tamano);
                    go.transform.localPosition = PuntoLibre(c.tamano * 0.5f);
                    go.transform.localRotation = Quaternion.Euler(R(0, 360), R(0, 360), R(0, 360));
                    if (materiales != null)
                        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                            r.sharedMaterials = r.sharedMaterials.Select(m => m != null && materiales.TryGetValue(m, out var t) ? t : m).ToArray();
                    // Collider para el rayo y Rigidbody cinemático: se mueven cada cuadro con el flujo
                    var col = go.AddComponent<SphereCollider>();
                    col.radius = MedioLocal(go);
                    col.isTrigger = true;
                    var rb = go.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    go.AddComponent<Descubrible>().ficha = fichas[c.ficha];
                    usadas.Add(fichas[c.ficha]);
                }
            }
            PrefabUtility.SaveAsPrefabAsset(raiz, PrefabVaso);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        // Copia el FBX de Diego a Modelos/Torrente (si falta) y lo importa sin cámaras ni luces
        static GameObject Importar(string archivo)
        {
            string destino = $"{CarpetaTorrente}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[Pokedex] No encontré {origen}."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter mi && (mi.importCameras || mi.importLights))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(destino);
        }

        // Copia de los materiales del modelo con otro color (para eosinófilo y basófilo, que no tienen modelo propio)
        static Dictionary<Material, Material> Tenidos(GameObject fbx, string nombre, Color tinte)
        {
            Carpeta(CarpetaMateriales);
            var r = new Dictionary<Material, Material>();
            foreach (var m in fbx.GetComponentsInChildren<Renderer>(true).SelectMany(x => x.sharedMaterials).Where(x => x != null).Distinct())
            {
                string ruta = $"{CarpetaMateriales}/{Archivo(nombre)}_{m.name}.mat";
                var t = AssetDatabase.LoadAssetAtPath<Material>(ruta);
                if (t == null)
                {
                    t = new Material(m);
                    var baseColor = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
                    var nuevo = Color.Lerp(baseColor, tinte, 0.7f);
                    nuevo.a = baseColor.a;
                    if (t.HasProperty("_BaseColor")) t.SetColor("_BaseColor", nuevo);
                    else t.color = nuevo;
                    AssetDatabase.CreateAsset(t, ruta);
                }
                r[m] = t;
            }
            return r;
        }

        // Escala el modelo para que su lado mayor mida "lado" metros
        static void Ajustar(Transform t, float lado)
        {
            t.localScale = Vector3.one;
            var b = Limites(t.gameObject);
            float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (mayor > 0f) t.localScale = Vector3.one * (lado / mayor);
        }

        static float MedioLocal(GameObject go)
        {
            var b = Limites(go);
            float s = go.transform.lossyScale.x;
            return s > 0f ? Mathf.Max(b.extents.x, b.extents.y, b.extents.z) / s : 1f;
        }

        // Dentro del tubo del vaso (eje Z en y = 1), sin cruzar la cabina retro (x ±2.1 m, y −0.1 a 2.7 m),
        // igual que los discos provisionales de GenerarEntornos
        static Vector3 PuntoLibre(float alcance)
        {
            for (int i = 0; i < 200; i++)
            {
                // Hasta la pared del tronco de Ulises (radio 15 m) con medio metro de holgura
                float ang = R(0, Mathf.PI * 2), r = R(1.2f, BioVR.Entornos.Editor.PonerTorrente.RadioVaso - 0.5f - alcance);
                var p = new Vector3(Mathf.Cos(ang) * r, 1 + Mathf.Sin(ang) * r, R(BioVR.Entornos.Editor.PonerTorrente.FlujoZMin, BioVR.Entornos.Editor.PonerTorrente.FlujoZMax));
                float m = alcance + 0.2f;
                bool cruza = Mathf.Abs(p.x) < 2.1f + m && p.y > -0.1f - m && p.y < 2.7f + m;
                if (!cruza) return p;
            }
            return new Vector3(2.4f + alcance, 1f, R(BioVR.Entornos.Editor.PonerTorrente.FlujoZMin, BioVR.Entornos.Editor.PonerTorrente.FlujoZMax)); // a un lado de la cabina, sin tocarla
        }

        static float R(float a, float b) => a + (float)azar.NextDouble() * (b - a);

        // ---------------- Capilar ----------------
        // Las proteínas del plasma que flotan en Capilar.prefab (GenerarEntornos): "Proteina" = albúmina, "Anticuerpo".
        static void Capilar(Dictionary<int, FichaPokedex> fichas, HashSet<FichaPokedex> usadas)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabCapilar) == null) { Debug.LogWarning($"[Pokedex] No está {PrefabCapilar}."); return; }
            var raiz = PrefabUtility.LoadPrefabContents(PrefabCapilar);
            int marcadas = 0;
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
            {
                int n = t.name == "Proteina" ? 17 : t.name == "Anticuerpo" ? 18 : 0;
                if (n == 0) continue;
                var d = t.GetComponent<Descubrible>();
                if (d == null) d = t.gameObject.AddComponent<Descubrible>();
                d.ficha = fichas[n];
                usadas.Add(fichas[n]);
                // El colisionador de la primitiva sirve para el rayo; como trigger no estorba. Se mueven con el flujo.
                foreach (var c in t.GetComponents<Collider>()) c.isTrigger = true;
                if (t.GetComponent<Collider>() == null) t.gameObject.AddComponent<SphereCollider>().isTrigger = true;
                var rb = t.GetComponent<Rigidbody>();
                if (rb == null) rb = t.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                marcadas++;
            }
            PrefabUtility.SaveAsPrefabAsset(raiz, PrefabCapilar);
            PrefabUtility.UnloadPrefabContents(raiz);
            Debug.Log($"[Pokedex] Capilar: {marcadas} proteínas se pueden registrar.");
        }

        // ---------------- Célula ----------------

        static void Celula(Dictionary<int, FichaPokedex> fichas, HashSet<FichaPokedex> usadas)
        {
            var raiz = PrefabUtility.LoadPrefabContents(PrefabCelula);
            var escenario = raiz.transform.Cast<Transform>().FirstOrDefault(t => t.name.StartsWith("Modelo Escenario celular"));
            if (escenario == null)
            {
                Debug.LogError("[Pokedex] EspacioCelular.prefab no tiene el escenario de Diseño. Corre primero BioVR → Poner escenario celular (low poly).");
                PrefabUtility.UnloadPrefabContents(raiz);
                return;
            }
            // Limpiar una corrida anterior
            foreach (var d in escenario.GetComponentsInChildren<Descubrible>(true)) Object.DestroyImmediate(d);
            foreach (var c in escenario.GetComponentsInChildren<MeshCollider>(true)) Object.DestroyImmediate(c);

            int marcadas = 0;
            Marcar(escenario, fichas, usadas, ref marcadas);
            Debug.Log($"[Pokedex] Célula: {marcadas} piezas del escenario se pueden registrar.");
            PrefabUtility.SaveAsPrefabAsset(raiz, PrefabCelula);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        static void Marcar(Transform t, Dictionary<int, FichaPokedex> fichas, HashSet<FichaPokedex> usadas, ref int marcadas)
        {
            foreach (Transform hijo in t)
            {
                int ficha = FichaDe(hijo.name);
                // Un grupo enorme (p. ej. los cientos de bloques de actina del escenario v2) abarca toda la célula:
                // marcado, cualquier punto lo registraría. Esos se saltan; sus piezas sueltas no se revisan.
                int mallas = hijo.GetComponentsInChildren<MeshFilter>(false).Length;
                if (ficha > 0 && hijo.gameObject.activeInHierarchy && mallas > 60) continue;
                if (ficha > 0 && hijo.gameObject.activeInHierarchy)
                {
                    hijo.gameObject.AddComponent<Descubrible>().ficha = fichas[ficha];
                    foreach (var mf in hijo.GetComponentsInChildren<MeshFilter>(false)) // solo lo visible (el v2 apaga niveles de detalle)
                        if (mf.sharedMesh != null) mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
                    usadas.Add(fichas[ficha]);
                    marcadas++;
                }
                else Marcar(hijo, fichas, usadas, ref marcadas);
            }
        }

        static int FichaDe(string nombre)
        {
            foreach (var o in Organelos)
                foreach (var p in o.prefijos)
                    if (o.exacto ? nombre == p : nombre.StartsWith(p)) return o.ficha;
            return 0;
        }

        // ---------------- Escena ----------------

        // Más arriba y a la derecha que antes (0.75, 1.45, 0.85): ASTRA, sobre la consola, tapaba la parte de abajo
        // del panel (prueba del 23-nov)
        static readonly Vector3 PosicionPanel = new Vector3(0.85f, 1.62f, 0.8f);
        static readonly Vector3 OjosPiloto = new Vector3(0f, 1.4f, 0f);

        static void Escena(FichaPokedex[] fichas)
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            foreach (var viejo in escena.GetRootGameObjects().Where(g => g.name == "Pokedex").ToArray())
                Object.DestroyImmediate(viejo);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (origen == null || origen.Camera == null) { Debug.LogError("[Pokedex] No hay XR Origin con cámara en la escena inicial."); return; }
            UIVR.AsegurarEventSystem();

            var raiz = new GameObject("Pokedex");
            var pokedex = raiz.AddComponent<Pokedex>();

            var panel = UIVR.Panel("Panel", raiz.transform, new Vector2(440f, 400f), origen.Camera); // 0.44 × 0.40 m
            panel.transform.SetPositionAndRotation(PosicionPanel, Quaternion.LookRotation(PosicionPanel - OjosPiloto));
            var contador = UIVR.Texto(panel.transform, "Contador", "Registradas: 0 de 0", 20f, FontStyles.Italic, 26f);
            var titulo = UIVR.Texto(panel.transform, "Titulo", "Pokédex biológico", 34f, FontStyles.Bold, 46f);
            var tamano = UIVR.Texto(panel.transform, "Tamano", "", 20f, FontStyles.Normal, 28f);
            var descripcion = UIVR.Texto(panel.transform, "Descripcion", "", 24f, FontStyles.Normal, 150f);
            var curiosidad = UIVR.Texto(panel.transform, "Curiosidad", "", 20f, FontStyles.Italic, 80f);
            UIVR.CapaDeUI(panel);

            // Etiqueta 3D sobre el rayo (no es UI: se dibuja en el mundo, siempre de frente a la cámara)
            var et = new GameObject("Etiqueta del rayo", typeof(TextMeshPro));
            et.transform.SetParent(raiz.transform, false);
            var tmp = et.GetComponent<TextMeshPro>();
            tmp.text = "¿ ?";
            tmp.fontSize = 1.2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.95f, 1f);
            tmp.rectTransform.sizeDelta = new Vector2(1.6f, 0.6f);

            var so = new SerializedObject(pokedex);
            var lista = so.FindProperty("fichas");
            lista.arraySize = fichas.Length;
            for (int i = 0; i < fichas.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = fichas[i];
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("titulo").objectReferenceValue = titulo;
            so.FindProperty("tamano").objectReferenceValue = tamano;
            so.FindProperty("descripcion").objectReferenceValue = descripcion;
            so.FindProperty("curiosidad").objectReferenceValue = curiosidad;
            so.FindProperty("contador").objectReferenceValue = contador;
            so.FindProperty("etiqueta").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
        }

        // ---------------- Ayudantes ----------------

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static string Archivo(string s) => new string(s.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => char.IsLetterOrDigit(c) && c < 128).ToArray());

        static void Carpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            var padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
            Carpeta(padre);
            AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
        }
    }
}
