using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BioVR.Entornos.Editor
{
    // Genera los entornos provisionales (placeholders) con primitivas de Unity y los pone en las zonas de 00-EscenaInicial.
    // Cuando Diseño mande los FBX, se reemplaza el contenido de cada prefab y las zonas no cambian.
    //
    //   Hangar.prefab          → zonas Hangar y Abordaje
    //   VasoSanguineo.prefab   → zona Vasos      (salto 1, ~1-5 mm)
    //   Capilar.prefab         → zona Capilar    (salto 2, ~1 µm)
    //   EspacioCelular.prefab  → zonas Citoplasma y Llegada (salto 3, ~200-500 nm)
    //
    // La cabina está en el origen (4 × 4 m) y el jugador mira hacia +Z: todo lo importante va al frente.
    // Se puede correr varias veces: sobrescribe los prefabs y vuelve a poner las zonas.
    // Menú: BioVR → Generar entornos placeholder
    public static class GenerarEntornos
    {
        const string Carpeta = "Assets/_BioVR/Entornos";
        const string CarpetaMateriales = Carpeta + "/Materiales";
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";

        // Centro aproximado de la cabina: nada debe atravesarla
        static readonly Vector3 CentroCabina = new Vector3(0f, 1f, 0f);

        static System.Random azar;
        static float R(float min, float max) => min + (float)azar.NextDouble() * (max - min);

        [MenuItem("BioVR/Generar entornos placeholder")]
        public static void Generar()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaMateriales))
                AssetDatabase.CreateFolder(Carpeta, "Materiales");

            var prefabs = new Dictionary<string, GameObject>
            {
                ["Hangar"] = Guardar(Hangar(), "Hangar"),
                ["VasoSanguineo"] = Guardar(VasoSanguineo(), "VasoSanguineo"),
                ["Capilar"] = Guardar(Capilar(), "Capilar"),
                ["EspacioCelular"] = Guardar(EspacioCelular(), "EspacioCelular"),
            };

            PonerEnZonas(new Dictionary<string, GameObject>
            {
                ["Hangar"] = prefabs["Hangar"],
                ["Abordaje"] = prefabs["Hangar"],
                ["Vasos"] = prefabs["VasoSanguineo"],
                ["Capilar"] = prefabs["Capilar"],
                ["Citoplasma"] = prefabs["EspacioCelular"],
                ["Llegada"] = prefabs["EspacioCelular"],
            });

            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        // ---------------- Hangar ----------------
        // Piso, paredes, techo, luces y un submarino hecho de cápsulas frente a la cabina.
        static GameObject Hangar()
        {
            azar = new System.Random(1);
            var raiz = new GameObject("Hangar");
            var piso = Mat("Hangar_Piso", new Color(0.32f, 0.33f, 0.35f));
            var pared = Mat("Hangar_Pared", new Color(0.22f, 0.24f, 0.28f));
            var luz = Mat("Hangar_Luz", new Color(1f, 0.97f, 0.85f), emision: 2f);
            var franja = Mat("Hangar_Franja", new Color(1f, 0.8f, 0.1f));
            var casco = Mat("Submarino_Casco", new Color(1f, 0.72f, 0.12f));
            var ventana = Mat("Submarino_Ventana", new Color(0.1f, 0.35f, 0.6f), emision: 0.8f);
            var metal = Mat("Submarino_Metal", new Color(0.55f, 0.57f, 0.6f));

            var sala = Grupo("Sala", raiz);
            Pieza(PrimitiveType.Cube, "Piso", sala, new Vector3(0, -0.1f, 10), new Vector3(40, 0.1f, 40), piso);
            Pieza(PrimitiveType.Cube, "Pared izquierda", sala, new Vector3(-20, 6, 10), new Vector3(0.5f, 12, 40), pared);
            Pieza(PrimitiveType.Cube, "Pared derecha", sala, new Vector3(20, 6, 10), new Vector3(0.5f, 12, 40), pared);
            Pieza(PrimitiveType.Cube, "Pared trasera", sala, new Vector3(0, 6, -10), new Vector3(40, 12, 0.5f), pared);
            Pieza(PrimitiveType.Cube, "Pared frontal", sala, new Vector3(0, 6, 30), new Vector3(40, 12, 0.5f), pared);
            Pieza(PrimitiveType.Cube, "Techo", sala, new Vector3(0, 12, 10), new Vector3(40, 0.5f, 40), pared);

            var luces = Grupo("Luces", raiz);
            for (int i = 0; i < 3; i++)
                Pieza(PrimitiveType.Cube, $"Luz frontal {i + 1}", luces, new Vector3(0, 3 + i * 3, 29.7f), new Vector3(30, 0.3f, 0.1f), luz);
            for (int i = -1; i <= 1; i += 2)
                Pieza(PrimitiveType.Cube, "Luz lateral", luces, new Vector3(i * 19.7f, 5, 12), new Vector3(0.1f, 0.3f, 30), luz);
            // Franjas amarillas en el piso que marcan el muelle del submarino
            for (int i = -1; i <= 1; i += 2)
                Pieza(PrimitiveType.Cube, "Franja", luces, new Vector3(i * 3.2f, -0.04f, 14), new Vector3(0.3f, 0.02f, 14), franja);

            var sub = Grupo("Submarino (capsulas)", raiz);
            sub.localPosition = new Vector3(0, 0, 14);
            Pieza(PrimitiveType.Capsule, "Casco", sub, new Vector3(0, 2.2f, 0), new Vector3(3.5f, 5, 3.5f), casco, Quaternion.Euler(90, 0, 0));
            Pieza(PrimitiveType.Capsule, "Torre", sub, new Vector3(0, 4.1f, -1), new Vector3(1.2f, 1, 2.2f), casco);
            Pieza(PrimitiveType.Cube, "Aleta horizontal", sub, new Vector3(0, 2.2f, -4.6f), new Vector3(5, 0.15f, 1.2f), metal);
            Pieza(PrimitiveType.Cube, "Aleta vertical", sub, new Vector3(0, 2.2f, -4.6f), new Vector3(0.15f, 3, 1.2f), metal);
            Pieza(PrimitiveType.Cylinder, "Helice", sub, new Vector3(0, 2.2f, -5.2f), new Vector3(1.8f, 0.05f, 1.8f), metal, Quaternion.Euler(90, 0, 0));
            for (int i = 0; i < 3; i++)
            for (int lado = -1; lado <= 1; lado += 2)
                Pieza(PrimitiveType.Sphere, "Ventana", sub, new Vector3(lado * 1.55f, 2.6f, -1.5f + i * 1.6f), Vector3.one * 0.6f, ventana);
            Pieza(PrimitiveType.Cube, "Soporte trasero", sub, new Vector3(0, 0.3f, -2.5f), new Vector3(1.2f, 0.6f, 0.8f), metal);
            Pieza(PrimitiveType.Cube, "Soporte delantero", sub, new Vector3(0, 0.3f, 2.5f), new Vector3(1.2f, 0.6f, 0.8f), metal);
            Pieza(PrimitiveType.Cube, "Pasarela", sub, new Vector3(4, 1.6f, 0), new Vector3(2, 0.2f, 8), metal);
            return raiz;
        }

        // ---------------- Vaso sanguíneo ----------------
        // Túnel rojo y glóbulos rojos (discos) que fluyen hacia la cabina con los latidos.
        static GameObject VasoSanguineo()
        {
            azar = new System.Random(2);
            var raiz = new GameObject("VasoSanguineo");
            Fondo(raiz, new Color(0.12f, 0.01f, 0.02f));
            var pared = Mat("Vaso_Pared", new Color(0.5f, 0.04f, 0.06f), doble: true, emision: 0.35f);
            var globulo = Mat("Vaso_GloboRojo", new Color(0.8f, 0.06f, 0.08f), emision: 0.15f);
            var leucocito = Mat("Vaso_Leucocito", new Color(0.92f, 0.9f, 0.85f));

            Pieza(PrimitiveType.Cylinder, "Pared del vaso", raiz.transform, new Vector3(0, 1, 30), new Vector3(12, 40, 12), pared, Quaternion.Euler(90, 0, 0));

            var flujo = Grupo("Globulos (flujo)", raiz.transform);
            Flujo(flujo, velocidad: 3f, pulso: 1.5f, frecuencia: 1.2f, giro: 25f);
            int puestos = 0;
            while (puestos < 70)
            {
                var p = PuntoEnTubo(radioMin: 1.2f, radioMax: 5.2f);
                if (CruzaCabina(p, 0.8f)) continue;
                Pieza(PrimitiveType.Cylinder, "Globulo rojo", flujo, p, new Vector3(1.4f, 0.18f, 1.4f), globulo,
                      Quaternion.Euler(R(0, 360), R(0, 360), R(0, 360)));
                puestos++;
            }
            for (int i = 0; i < 3; i++)
            {
                var p = PuntoEnTubo(3.5f, 4.5f);
                if (CruzaCabina(p, 1.5f)) { i--; continue; }
                Pieza(PrimitiveType.Sphere, "Leucocito", flujo, p, Vector3.one * 2.4f, leucocito);
            }
            return raiz;
        }

        // ---------------- Capilar ----------------
        // Tubo estrecho y viscoso: plasma con proteínas que avanzan lento, sin pulsos.
        static GameObject Capilar()
        {
            azar = new System.Random(3);
            var raiz = new GameObject("Capilar");
            Fondo(raiz, new Color(0.3f, 0.16f, 0.16f));
            var pared = Mat("Capilar_Pared", new Color(0.85f, 0.5f, 0.5f), doble: true, emision: 0.35f);
            var albumina = Mat("Capilar_Proteina", new Color(0.95f, 0.85f, 0.4f), emision: 0.2f);
            var anticuerpo = Mat("Capilar_Anticuerpo", new Color(0.55f, 0.85f, 0.95f), emision: 0.2f);

            Pieza(PrimitiveType.Cylinder, "Pared del capilar", raiz.transform, new Vector3(0, 1, 30), new Vector3(9, 40, 9), pared, Quaternion.Euler(90, 0, 0));

            var flujo = Grupo("Proteinas (flujo)", raiz.transform);
            Flujo(flujo, velocidad: 0.6f, pulso: 0f, frecuencia: 0f, giro: 30f);
            int puestos = 0;
            while (puestos < 90)
            {
                var p = PuntoEnTubo(0.3f, 4f);
                if (CruzaCabina(p, 0.4f)) continue;
                if (puestos % 3 == 0)
                    Pieza(PrimitiveType.Capsule, "Anticuerpo", flujo, p, new Vector3(0.25f, 0.45f, 0.25f), anticuerpo,
                          Quaternion.Euler(R(0, 360), R(0, 360), 0));
                else
                    Pieza(PrimitiveType.Sphere, "Proteina", flujo, p, Vector3.one * R(0.25f, 0.6f), albumina);
                puestos++;
            }
            return raiz;
        }

        // ---------------- Espacio celular ----------------
        // Bajo la membrana de una célula plasmática: membrana con proteínas, malla de actina, filamentos,
        // ribosomas, Golgi, mitocondrias y una cinesina llevando una vesícula.
        static GameObject EspacioCelular()
        {
            azar = new System.Random(4);
            var raiz = new GameObject("EspacioCelular");
            Fondo(raiz, new Color(0.06f, 0.1f, 0.14f));
            var membrana = Mat("Celula_Membrana", new Color(0.55f, 0.75f, 0.95f), emision: 0.25f);
            var protMembrana = Mat("Celula_ProteinaMembrana", new Color(0.95f, 0.55f, 0.2f), emision: 0.2f);
            var actina = Mat("Celula_Actina", new Color(0.95f, 0.3f, 0.45f), emision: 0.3f);
            var microtubulo = Mat("Celula_Microtubulo", new Color(0.35f, 0.85f, 0.55f), emision: 0.3f);
            var intermedio = Mat("Celula_FilamentoIntermedio", new Color(0.65f, 0.5f, 0.95f), emision: 0.3f);
            var ribosoma = Mat("Celula_Ribosoma", new Color(0.95f, 0.93f, 0.6f), emision: 0.2f);
            var enzima = Mat("Celula_Enzima", new Color(0.5f, 0.9f, 0.9f), emision: 0.2f);
            var golgi = Mat("Celula_Golgi", new Color(0.45f, 0.6f, 1f), emision: 0.25f);
            var mitocondria = Mat("Celula_Mitocondria", new Color(1f, 0.5f, 0.25f), emision: 0.2f);
            var vesicula = Mat("Celula_Vesicula", new Color(0.85f, 0.85f, 1f), emision: 0.3f);

            // Membrana arriba: el mecha entra por debajo de ella
            var mem = Grupo("Membrana", raiz.transform);
            Pieza(PrimitiveType.Cube, "Bicapa", mem, new Vector3(0, 10, 15), new Vector3(90, 0.5f, 90), membrana);
            for (int i = 0; i < 35; i++)
            {
                float d = R(0.8f, 1.6f);
                Pieza(PrimitiveType.Cylinder, "Proteina de membrana", mem, new Vector3(R(-35, 35), 10, R(-10, 45)), new Vector3(d, 0.9f, d), protMembrana);
            }
            var malla = Grupo("Malla de actina", mem);
            for (float z = -10; z <= 45; z += 5)
                Filamento("Actina (malla)", malla, new Vector3(-40, 9.2f, z), new Vector3(40, 9.2f, z), 0.12f, actina);
            for (float x = -40; x <= 40; x += 5)
                Filamento("Actina (malla)", malla, new Vector3(x, 9.2f, -10), new Vector3(x, 9.2f, 45), 0.12f, actina);

            // Citoesqueleto cruzando el espacio: microtúbulos (25 nm, gruesos), intermedios (~10 nm), actina (8 nm, delgados)
            var cito = Grupo("Citoesqueleto", raiz.transform);
            var rieles = new Vector3(-20, 2.5f, 18);
            var rielFin = new Vector3(20, 3.5f, 24);
            Filamento("Microtubulo (cinesina)", cito, rieles, rielFin, 0.6f, microtubulo);
            FilamentosAlAzar("Microtubulo", cito, 7, 0.6f, microtubulo);
            FilamentosAlAzar("Filamento intermedio", cito, 6, 0.25f, intermedio);
            FilamentosAlAzar("Actina", cito, 14, 0.15f, actina);

            // Interior lleno: ribosomas y enzimas por todas partes
            var moleculas = Grupo("Ribosomas y enzimas", raiz.transform);
            for (int puestos = 0; puestos < 150;)
            {
                var p = new Vector3(R(-20, 20), R(-4, 8.5f), R(-8, 40));
                if (Mathf.Abs(p.x) < 4f && p.z < 4f) continue; // ni dentro ni detrás de la cabina
                if (puestos % 4 == 0)
                    Pieza(PrimitiveType.Capsule, "Enzima", moleculas, p, new Vector3(0.25f, 0.35f, 0.25f), enzima, Quaternion.Euler(R(0, 360), R(0, 360), 0));
                else
                    Pieza(PrimitiveType.Sphere, "Ribosoma", moleculas, p, Vector3.one * R(0.3f, 0.55f), ribosoma);
                puestos++;
            }

            // Al fondo: Golgi (discos apilados) y mitocondrias (cápsulas)
            var g = Grupo("Golgi", raiz.transform);
            g.localPosition = new Vector3(14, 2, 30);
            g.localRotation = Quaternion.Euler(10, -30, 0);
            for (int i = 0; i < 6; i++)
                Pieza(PrimitiveType.Cylinder, "Cisterna", g, new Vector3(0, i * 0.55f, 0), new Vector3(7 - i * 0.7f, 0.12f, 4 - i * 0.4f), golgi);

            var mitos = Grupo("Mitocondrias", raiz.transform);
            Pieza(PrimitiveType.Capsule, "Mitocondria", mitos, new Vector3(-13, 1.5f, 26), new Vector3(2.2f, 4.5f, 2.2f), mitocondria, Quaternion.Euler(0, 20, 90));
            Pieza(PrimitiveType.Capsule, "Mitocondria", mitos, new Vector3(-5, -2.5f, 36), new Vector3(2.2f, 4.5f, 2.2f), mitocondria, Quaternion.Euler(0, -40, 80));
            Pieza(PrimitiveType.Capsule, "Mitocondria", mitos, new Vector3(17, 6, 20), new Vector3(2.2f, 4.5f, 2.2f), mitocondria, Quaternion.Euler(30, 60, 90));

            // Cinesina "caminando" sobre el microtúbulo y arrastrando una vesícula
            var cinesina = Grupo("Cinesina con vesicula", raiz.transform);
            var recorrido = cinesina.gameObject.AddComponent<RecorridoLineal>();
            recorrido.inicio = rieles;
            recorrido.fin = rielFin;
            cinesina.localPosition = rieles;
            Pieza(PrimitiveType.Capsule, "Pata", cinesina, new Vector3(-0.15f, 0.55f, 0), new Vector3(0.15f, 0.3f, 0.15f), enzima);
            Pieza(PrimitiveType.Capsule, "Pata", cinesina, new Vector3(0.15f, 0.55f, 0), new Vector3(0.15f, 0.3f, 0.15f), enzima);
            Pieza(PrimitiveType.Capsule, "Cuello", cinesina, new Vector3(0, 1.1f, 0), new Vector3(0.12f, 0.3f, 0.12f), enzima);
            Pieza(PrimitiveType.Sphere, "Vesicula", cinesina, new Vector3(0, 2.1f, 0), Vector3.one * 1.6f, vesicula);
            return raiz;
        }

        // ---------------- Escena ----------------
        static void PonerEnZonas(Dictionary<string, GameObject> porZona)
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var raices = escena.GetRootGameObjects();

            var zonas = raices.FirstOrDefault(r => r.name == "Zonas");
            if (zonas == null) { Debug.LogError("[Entornos] No hay 'Zonas' en la escena inicial."); return; }
            zonas.transform.localPosition = Vector3.zero; // los entornos se modelan respecto a la cabina en el origen

            foreach (var (nombreZona, prefab) in porZona)
            {
                var zona = zonas.transform.Find(nombreZona);
                if (zona == null) { Debug.LogWarning($"[Entornos] Falta la zona '{nombreZona}'."); continue; }
                for (int i = zona.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(zona.GetChild(i).gameObject);
                var entorno = (GameObject)PrefabUtility.InstantiatePrefab(prefab, zona);
                entorno.name = "Entorno " + prefab.name;
            }

            // Techo de la cabina: estaba de 1.5 a 2.5 m y tapaba la vista hacia arriba; ahora es una tapa a 2 m
            var techo = raices.FirstOrDefault(r => r.name == "Cabina (provisional)")?.transform.Find("techo");
            if (techo != null)
            {
                techo.localPosition = new Vector3(0, 2.05f, 0);
                techo.localScale = new Vector3(4, 0.1f, 4);
            }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[Entornos] Listo: 4 prefabs en Entornos/ y puestos en las 6 zonas de 00-EscenaInicial.");
        }

        // ---------------- Utilidades ----------------
        static GameObject Guardar(GameObject raiz, string nombre)
        {
            var carpeta = $"{Carpeta}/{nombre}";
            if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder(Carpeta, nombre);
            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, $"{carpeta}/{nombre}.prefab");
            Object.DestroyImmediate(raiz);
            return prefab;
        }

        static Transform Grupo(string nombre, Object padre)
        {
            var t = new GameObject(nombre).transform;
            t.SetParent(padre is GameObject go ? go.transform : (Transform)padre, false);
            return t;
        }

        // Primitiva sin collider ni sombras (en el visor las sombras son caras y aquí no aportan)
        static GameObject Pieza(PrimitiveType tipo, string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat,
                                Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(padre, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot ?? Quaternion.identity;
            go.transform.localScale = escala;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go;
        }

        // Cilindro de 'a' a 'b' con el diámetro dado
        static void Filamento(string nombre, Transform padre, Vector3 a, Vector3 b, float diametro, Material mat)
        {
            var go = Pieza(PrimitiveType.Cylinder, nombre, padre, (a + b) / 2f, new Vector3(diametro, Vector3.Distance(a, b) / 2f, diametro), mat);
            go.transform.up = (b - a).normalized;
        }

        static void FilamentosAlAzar(string nombre, Transform padre, int cantidad, float diametro, Material mat)
        {
            for (int puestos = 0; puestos < cantidad;)
            {
                var centro = new Vector3(R(-18, 18), R(-3, 8), R(6, 40));
                var dir = new Vector3(R(-1, 1), R(-0.4f, 0.4f), R(-0.6f, 0.6f)).normalized;
                float largo = R(25, 50);
                var a = centro - dir * largo / 2f;
                var b = centro + dir * largo / 2f;
                if (DistanciaASegmento(CentroCabina, a, b) < 4.5f) continue;
                Filamento(nombre, padre, a, b, diametro, mat);
                puestos++;
            }
        }

        // Esfera gigante vista por dentro: tapa el cielo y da el color del medio
        static void Fondo(GameObject raiz, Color color)
        {
            var mat = Mat("Fondo_" + raiz.name, color, unlit: true, doble: true);
            Pieza(PrimitiveType.Sphere, "Fondo", raiz.transform, new Vector3(0, 1, 20), Vector3.one * 150f, mat);
        }

        static void Flujo(Transform grupo, float velocidad, float pulso, float frecuencia, float giro)
        {
            var flujo = grupo.gameObject.AddComponent<FlujoPlaceholder>();
            var so = new SerializedObject(flujo);
            so.FindProperty("velocidad").floatValue = velocidad;
            so.FindProperty("pulso").floatValue = pulso;
            so.FindProperty("frecuencia").floatValue = frecuencia;
            so.FindProperty("giro").floatValue = giro;
            so.FindProperty("zMin").floatValue = -8f;
            so.FindProperty("zMax").floatValue = 68f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Punto al azar dentro de un tubo a lo largo de Z con el eje en y = 1
        static Vector3 PuntoEnTubo(float radioMin, float radioMax)
        {
            float ang = R(0, Mathf.PI * 2), r = R(radioMin, radioMax);
            return new Vector3(Mathf.Cos(ang) * r, 1 + Mathf.Sin(ang) * r, R(-8, 68));
        }

        // Lo que fluye en Z atraviesa la cabina si su (x, y) cae dentro de la sección de la cabina
        static bool CruzaCabina(Vector3 p, float margen) =>
            Mathf.Abs(p.x) < 2f + margen && p.y > -0.1f - margen && p.y < 2.1f + margen;

        static float DistanciaASegmento(Vector3 p, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector3.Distance(p, a + t * ab);
        }

        static Material Mat(string nombre, Color color, bool unlit = false, bool doble = false, float emision = 0f)
        {
            var ruta = $"{CarpetaMateriales}/{nombre}.mat";
            var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, ruta);
            }
            mat.shader = shader;
            mat.SetColor("_BaseColor", color);
            mat.enableInstancing = true;
            if (doble)
            {
                mat.SetFloat("_Cull", (float)CullMode.Off); // se ve también por dentro (túneles y fondo)
                mat.doubleSidedGI = true;
            }
            if (emision > 0f)
            {
                // Un poco de brillo propio para que se vea aunque no le llegue la luz (p. ej. dentro de los túneles)
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emision);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
