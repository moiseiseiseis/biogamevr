using System.IO;
using System.Linq;
using BioVR.Secuencia;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Pilotaje.Editor
{
    // Menú BioVR → Pilotaje → Armar prueba de pilotaje.
    //
    // Hace Escenas/Pruebas/Pilotaje.unity, una copia de 00-EscenaInicial que empieza directo en la llegada a la célula,
    // con el pilotaje libre, la brújula en el tablero y la misión del modo investigación (registrar 4 organelos
    // para desbloquear el salto a 5 nm, con su propio tutorial; el del hangar queda apagado). Es para probar la comodidad del vuelo en el visor
    // con su propio APK (BioVR → Build APK Pilotaje), sin tocar el juego. Si se aprueba, se integra al flujo normal.
    // Correr después de que la escena inicial tenga el escenario low poly y el pokédex. Se puede correr varias veces.
    public static class ArmarPruebaPilotaje
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        public const string Escena = "Assets/_BioVR/Escenas/Pruebas/Pilotaje.unity";
        const string CarpetaMateriales = "Assets/_BioVR/Pilotaje/Materiales";

        // Brújula sobre la consola, entre los botones y la pantalla, y los ojos del piloto (en la cabina)
        static readonly Vector3 PosicionBrujula = new Vector3(0f, 0.92f, 0.6f);
        static readonly Vector3 OjosPiloto = new Vector3(0f, 1.4f, 0f);

        [MenuItem("BioVR/Pilotaje/Armar prueba de pilotaje")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            Directory.CreateDirectory(Path.GetDirectoryName(Escena));
            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError($"[Pilotaje] No se pudo guardar {Escena}."); return; }

            var raices = escena.GetRootGameObjects();
            var gm = raices.Select(r => r.GetComponentInChildren<GameManager>(true)).FirstOrDefault(g => g != null);
            var cabina = raices.FirstOrDefault(r => r.name == "Cabina (provisional)");
            var llegada = raices.FirstOrDefault(r => r.name == "Zonas")?.transform.Find("Llegada");
            if (gm == null || cabina == null || llegada == null)
            {
                Debug.LogError("[Pilotaje] Falta el GameManager, 'Cabina (provisional)' o 'Zonas/Llegada' en la escena.");
                return;
            }

            // Empieza en la llegada y sin tutorial (sus pasos son del hangar)
            var gso = new SerializedObject(gm);
            gso.FindProperty("estadoInicial").enumValueIndex = (int)GameState.Llegada;
            gso.ApplyModifiedPropertiesWithoutUndo();
            var tutorial = raices.FirstOrDefault(r => r.name == "Tutorial");
            if (tutorial != null) tutorial.SetActive(false);

            foreach (var viejo in raices.Where(r => r.name == "Pilotaje")) Object.DestroyImmediate(viejo);
            var viejaBrujula = cabina.transform.Find("Brujula");
            if (viejaBrujula != null) Object.DestroyImmediate(viejaBrujula.gameObject);

            var (flecha, texto) = Brujula(cabina.transform);

            var go = new GameObject("Pilotaje");
            var so = new SerializedObject(go.AddComponent<PilotajeLibre>());
            so.FindProperty("mundo").objectReferenceValue = llegada;
            so.FindProperty("cabina").objectReferenceValue = cabina.transform;
            so.FindProperty("flecha").objectReferenceValue = flecha;
            so.FindProperty("textoBrujula").objectReferenceValue = texto;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Misión: 4 organelos para desbloquear el salto a 5 nm; su letrero va arriba de la brújula
            var letrero = Texto("Letrero de la mision", cabina.transform, PosicionBrujula + new Vector3(0f, 0.09f, 0f), 0.34f, new Vector2(0.6f, 0.12f));
            letrero.transform.SetParent(flecha.parent, true);
            var mso = new SerializedObject(go.AddComponent<MisionOrganelos>());
            mso.FindProperty("letrero").objectReferenceValue = letrero;
            mso.ApplyModifiedPropertiesWithoutUndo();

            // Tutorial de la misión (el del hangar queda apagado)
            var pasos = PasosMision();
            if (BioVR.Comun.Editor.CrearTutorial.ArmarEn(escena, "Tutorial de la mision", pasos, "Misión") == null) return;

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Pilotaje] Listo: {Escena}. Empieza en la llegada. Stick izquierdo: avanzar y de lado; stick derecho: " +
                      "subir, bajar y girar; A: piloto automático. Misión: 4 organelos para el salto a 5 nm.");
        }

        // Flecha hecha de dos cubos (cuerpo y punta) que apunta al organelo más cercano sin registrar, y su texto
        static (Transform flecha, TextMeshPro texto) Brujula(Transform cabina)
        {
            var mat = Material();
            var raiz = new GameObject("Brujula").transform;
            raiz.SetParent(cabina, false);
            raiz.localPosition = PosicionBrujula;

            var flecha = new GameObject("Flecha").transform;
            flecha.SetParent(raiz, false);
            Cubo("Cuerpo", flecha, new Vector3(0f, 0f, -0.01f), new Vector3(0.012f, 0.012f, 0.07f), Quaternion.identity, mat);
            Cubo("Punta", flecha, new Vector3(0f, 0f, 0.03f), new Vector3(0.03f, 0.012f, 0.03f), Quaternion.Euler(0f, 45f, 0f), mat);

            var tmp = Texto("Texto", cabina, PosicionBrujula + new Vector3(0f, -0.06f, 0f), 0.3f, new Vector2(0.5f, 0.12f));
            tmp.transform.SetParent(raiz, true);
            tmp.text = "Sin registrar";
            return (flecha, tmp);
        }

        // Texto 3D en la cabina, de frente a los ojos del piloto
        static TextMeshPro Texto(string nombre, Transform cabina, Vector3 posicionLocal, float tamano, Vector2 caja)
        {
            var go = new GameObject(nombre, typeof(TextMeshPro));
            go.transform.SetParent(cabina, false);
            go.transform.localPosition = posicionLocal;
            go.transform.rotation = Quaternion.LookRotation(cabina.TransformPoint(posicionLocal) - cabina.TransformPoint(OjosPiloto));
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.fontSize = tamano;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.95f, 1f);
            tmp.rectTransform.sizeDelta = caja;
            return tmp;
        }

        // ---------------- Tutorial de la misión ----------------

        const string CarpetaPasos = "Assets/_BioVR/Pilotaje/Tutorial";

        // archivo, título, instrucción, clave, opcional. Claves: "pilotar" y "automatico" (PilotajeLibre),
        // "organelo" y "mision" (MisionOrganelos), "salto:4" (el salto a 5 nm, del GameManager)
        static readonly (string, string, string, string, bool)[] Pasos =
        {
            ("01-ModoInvestigacion", "Modo investigación",
             "Antes de entrar al ribosoma, explora la célula y registra 4 organelos distintos en el pokédex. Con eso se desbloquea el salto a 5 nm.", "", false),
            ("02-Pilotar", "Pilota la nave",
             "Stick izquierdo: avanza y muévete de lado. Stick derecho: arriba y abajo para subir y bajar; a los lados para girar.", "pilotar", false),
            ("03-Brujula", "Sigue la brújula",
             "La flecha sobre la consola apunta al organelo más cercano que no has registrado. Presiona A y el piloto automático te lleva.", "automatico", true),
            ("04-Registrar", "Registra un organelo",
             "Apúntale con el mando y aprieta el gatillo. Su ficha aparece en el panel de la derecha.", "organelo", false),
            ("05-Mision", "Completa la misión",
             "Registra 4 organelos distintos. Tu avance se ve sobre la consola.", "mision", false),
            ("06-Salto", "¡Misión cumplida!",
             "Toca Salto Matrioska o empuja la palanca: la nave se reduce a 5 nm y entra al ribosoma.", "salto:4", false),
        };

        static BioVR.Comun.PasoTutorial[] PasosMision()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaPasos)) AssetDatabase.CreateFolder("Assets/_BioVR/Pilotaje", "Tutorial");
            foreach (var (archivo, titulo, instruccion, clave, opcional) in Pasos)
            {
                var ruta = $"{CarpetaPasos}/{archivo}.asset";
                if (AssetDatabase.LoadAssetAtPath<BioVR.Comun.PasoTutorial>(ruta) != null) continue; // respeta lo que se haya corregido
                var paso = ScriptableObject.CreateInstance<BioVR.Comun.PasoTutorial>();
                paso.titulo = titulo;
                paso.instruccion = instruccion;
                paso.clave = clave;
                paso.opcional = opcional;
                if (clave == "salto:4") paso.resaltar = new[] { "Boton Salto Matrioska", "Palanca" };
                AssetDatabase.CreateAsset(paso, ruta);
            }
            AssetDatabase.SaveAssets();
            return AssetDatabase.FindAssets("t:PasoTutorial", new[] { CarpetaPasos })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(Path.GetFileName)
                .Select(AssetDatabase.LoadAssetAtPath<BioVR.Comun.PasoTutorial>)
                .ToArray();
        }

        static void Cubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Quaternion rot, Material mat)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = nombre;
            Object.DestroyImmediate(c.GetComponent<Collider>()); // no debe estorbar al rayo ni a la cabina
            c.transform.SetParent(padre, false);
            c.transform.localPosition = pos;
            c.transform.localRotation = rot;
            c.transform.localScale = escala;
            c.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static Material Material()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaMateriales)) AssetDatabase.CreateFolder("Assets/_BioVR/Pilotaje", "Materiales");
            string ruta = CarpetaMateriales + "/Pilotaje_Flecha.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", new Color(0.3f, 0.95f, 1f));
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }
    }
}
