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
        // Arriba de la consola, a la derecha de la palanca (prueba del 24-nov: al centro y abajo no se veía,
        // la tapaba el tutorial). La palanca ocupa x 0.07–0.33 y su perilla llega a y 1.07.
        static readonly Vector3 PosicionBrujula = new Vector3(0.4f, 1.27f, 0.66f);
        static readonly Vector3 PosicionAviso = new Vector3(0f, 1.84f, 1.0f); // arriba al centro, por encima del tutorial
        static readonly Color ColorFlecha = new Color(1f, 0.85f, 0.1f); // = #FFD91A
        static readonly Vector3 PosicionLeyenda = new Vector3(-0.37f, 1.105f, 0.615f); // sobre la pantalla izquierda de la consola
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

            // Leyenda fija de cómo se pilota (pedido de Implementación, 24-nov): la nave va hacia el frente
            // de la cabina, no hacia donde mira el jugador. Va sobre la pantalla amarilla de la izquierda,
            // con letra oscura que llena la pantalla. Cuelga de la brújula para que se borre al re-armar.
            // Texto corto para que la letra salga grande (los joysticks ya los explica el paso 2 del tutorial)
            var leyenda = Texto("Leyenda de pilotaje", cabina.transform, PosicionLeyenda, 0.45f, new Vector2(0.2f, 0.14f));
            leyenda.transform.SetParent(flecha.parent, true);
            leyenda.color = new Color(0.08f, 0.08f, 0.1f);
            leyenda.enableAutoSizing = true;
            leyenda.fontSizeMin = 0.15f;
            leyenda.fontSizeMax = 0.45f;
            leyenda.fontStyle = FontStyles.Bold;
            leyenda.text = "La nave avanza\nal <color=#0B3D91>FRENTE</color>,\nno a donde\nmiras";

            // Aviso de mareo que reaparece cada tanto mientras se pilota (AvisoMareo), arriba de la consola
            var aviso = Texto("Aviso de mareo", cabina.transform, PosicionAviso, 0.42f, new Vector2(1.2f, 0.2f));
            aviso.transform.SetParent(flecha.parent, true);
            aviso.color = new Color(1f, 0.69f, 0.13f);
            aviso.fontStyle = FontStyles.Bold;
            aviso.outlineWidth = 0.32f;
            aviso.outlineColor = new Color32(0, 0, 0, 255);
            aviso.text = $"AVISO: si te mareas, suelta los joysticks\ny mira la cabina. Con {A} la nave va sola.";
            aviso.gameObject.AddComponent<AvisoMareo>();
            var mso = new SerializedObject(go.AddComponent<MisionOrganelos>());
            mso.FindProperty("letrero").objectReferenceValue = letrero;
            mso.ApplyModifiedPropertiesWithoutUndo();

            // Tutorial de la misión (el del hangar queda apagado)
            var pasos = PasosMision();
            if (BioVR.Comun.Editor.CrearTutorial.ArmarEn(escena, "Tutorial de la mision", pasos, "Misión", grande: true) == null) return;

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
            flecha.localScale = Vector3.one * 1.5f; // más grande para que se note desde el asiento

            var tmp = Texto("Texto", cabina, PosicionBrujula + new Vector3(0f, -0.06f, 0f), 0.3f, new Vector2(0.5f, 0.12f));
            tmp.transform.SetParent(raiz, true);
            tmp.text = "Sin registrar";
            tmp.color = ColorFlecha;
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

        // Botones con los colores de los mandos (EtiquetasMandos), igual que en la Cocina
        static string Etq(string t, Color c) => BioVR.Comun.EtiquetasMandos.Colores.Etiqueta(t, c);
        static string A => Etq("A", BioVR.Comun.EtiquetasMandos.Colores.A);
        static string X => Etq("X", BioVR.Comun.EtiquetasMandos.Colores.X);
        static string Gatillo => Etq("GATILLO", BioVR.Comun.EtiquetasMandos.Colores.Gatillo);
        static string Cian(string t) => $"<b><color=#5CF2FF>{t}</color></b>";

        // archivo, título, instrucción, clave, opcional, qué se resalta. Claves: "pilotar" y "automatico" (PilotajeLibre),
        // "organelo" y "mision" (MisionOrganelos), "salto:4" (el salto a 5 nm, del GameManager).
        // Prueba del 24-nov en la Cocina: textos cortos, una acción por paso y el botón con su color.
        static (string, string, string, string, bool, string[])[] Pasos() => new[]
        {
            ("00-AvisoMareo", "Antes de pilotar",
             $"Mover la nave con los joysticks <b>puede marear</b>.\nJuega <b>sentado</b>.\n\nSi te sientes mal, <b>suelta los joysticks</b> y mira la cabina. Con {A} la nave va sola.\n\nAprieta {X} para seguir.", "", false, new string[0]),
            ("01-ModoInvestigacion", "Explora la célula",
             $"Tu misión: registra <b>4 organelos</b> distintos.\nAsí se abre el salto al ribosoma.\n\nAprieta {X} para empezar.", "", false, new string[0]),
            ("02-Pilotar", "Pilota la nave",
             $"La nave va hacia el\n{Cian("frente de la cabina")},\nno hacia donde miras.\n\n{Cian("Joystick izquierdo")}\navanza y ve de lado\n{Cian("Joystick derecho")}\nsube, baja y gira", "pilotar", false, new string[0]),
            ("03-Brujula", "Sigue la flecha",
             $"La <b><color=#FFD91A>flecha amarilla</color></b> sobre la consola apunta a un organelo nuevo.\nAprieta {A} y la nave te lleva sola.", "automatico", true,
             new[] { "Brujula" }),
            ("04-Registrar", "Registra un organelo",
             $"Apúntale con el rayo y aprieta el {Gatillo}.", "organelo", false, new string[0]),
            ("05-Mision", "Completa la misión",
             "Registra 4 organelos distintos.\nTu avance está sobre la consola.", "mision", false, new[] { "Letrero de la mision" }),
            ("06-Salto", "¡Misión cumplida!",
             "Toca <b>Salto Matrioska</b> o empuja la palanca para entrar al ribosoma.", "salto:4", false,
             new[] { "Boton Salto Matrioska", "Palanca" }),
        };

        static BioVR.Comun.PasoTutorial[] PasosMision()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaPasos)) AssetDatabase.CreateFolder("Assets/_BioVR/Pilotaje", "Tutorial");
            // Los textos salen de aquí: se reescriben cada vez que se arma la prueba
            foreach (var (archivo, titulo, instruccion, clave, opcional, resaltar) in Pasos())
            {
                var ruta = $"{CarpetaPasos}/{archivo}.asset";
                var paso = AssetDatabase.LoadAssetAtPath<BioVR.Comun.PasoTutorial>(ruta);
                bool nuevo = paso == null;
                if (nuevo) paso = ScriptableObject.CreateInstance<BioVR.Comun.PasoTutorial>();
                paso.titulo = titulo;
                paso.instruccion = instruccion;
                paso.clave = clave;
                paso.opcional = opcional;
                paso.resaltar = resaltar;
                if (nuevo) AssetDatabase.CreateAsset(paso, ruta);
                else EditorUtility.SetDirty(paso);
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
            bool nuevo = m == null;
            if (nuevo) m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            // Amarilla (prueba del 24-nov): azul no se distinguía del holograma de la cabina
            m.SetColor("_BaseColor", ColorFlecha);
            if (nuevo) AssetDatabase.CreateAsset(m, ruta);
            else EditorUtility.SetDirty(m);
            return m;
        }
    }
}
