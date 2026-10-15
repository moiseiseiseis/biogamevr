using System.Linq;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Arma el tutorial en 00-EscenaInicial:
    //   - Crea en Comun/Tutorial/Pasos los pasos básicos que falten (no toca los que ya existen, por si alguien los editó)
    //   - "Tutorial" con el componente Tutorial y su panel a la izquierda de la consola, de cara al piloto
    //   - Le pasa TODOS los pasos de la carpeta en orden alfabético (por eso empiezan con número)
    // Para agregar un paso: crea el asset en la carpeta (Create → BioVR → Paso del tutorial) y vuelve a correr esto.
    // Menú: BioVR → Crear tutorial
    public static class CrearTutorial
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string CarpetaPasos = "Assets/_BioVR/Comun/Tutorial/Pasos";
        const string NombreTutorial = "Tutorial";

        // Dónde va el panel: adelante a la izquierda del asiento, arriba de la consola (que llega a 1.22 m)
        static readonly Vector3 PosicionPanel = new Vector3(-0.7f, 1.45f, 0.9f);
        static readonly Vector3 OjosPiloto = new Vector3(0f, 1.4f, 0f);

        // archivo, título, instrucción, clave, opcional
        static readonly (string, string, string, string, bool)[] PasosBasicos =
        {
            ("01-Bienvenida", "¡Bienvenido, piloto!",
             "Aquí aprenderás a manejar la nave. Apunta al botón Siguiente con el mando y presiona el gatillo.", "", false),
            ("02-Recentrar", "Vuelve a tu asiento",
             "Si te pierdes o quedas de lado, presiona B o Y: regresas al asiento mirando al frente.", "recentrar", false),
            ("03-Caminar", "Camina",
             "Empuja el stick izquierdo para caminar por la cabina. Puedes acercarte al cristal para asomarte.", "caminar", false),
            ("04-Girar", "Gira",
             "Inclina el stick derecho hacia un lado para girar.", "girar", false),
            ("05-Menu", "Menú de pausa",
             "Presiona el botón de menú del mando izquierdo. Ahí puedes pausar, reiniciar o salir del juego.", "menu", false),
            ("06-Asiento", "Ajusta tu altura",
             "En el menú, usa Subir o Bajar hasta ver bien por el cristal. Después toca Continuar.", "asiento", true),
            ("07-Escotilla", "Mira la nave por fuera",
             "Camina a la escotilla de atrás y tócala para salir al hangar. Para volver, toca el panel azul junto a la cabina.", "entrar", true),
            ("08-Abordar", "Aborda la nave",
             "Toca el botón Iniciar inmersión en la consola.", "estado:Abordaje", false),
            ("09-Sumergirse", "Sumérgete",
             "Toca Iniciar inmersión otra vez para entrar al cuerpo humano.", "estado:Viaje", false),
            ("10-Salto", "Salto Matrioska",
             "Toca Salto Matrioska o empuja la palanca para reducir la nave otra vez.", "salto:2", false),
            ("11-Listo", "¡Listo!",
             "Sigue saltando hasta llegar a la célula. Al final, Iniciar inmersión te regresa al hangar.", "", false),
        };

        [MenuItem("BioVR/Crear tutorial")]
        public static void Crear()
        {
            CrearPasosQueFalten();
            var pasos = AssetDatabase.FindAssets("t:PasoTutorial", new[] { CarpetaPasos })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(System.IO.Path.GetFileName)
                .Select(AssetDatabase.LoadAssetAtPath<PasoTutorial>)
                .ToArray();

            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (origen == null || origen.Camera == null) { Debug.LogError("[Tutorial] No hay XR Origin con cámara en la escena inicial."); return; }
            UIVR.AsegurarEventSystem();

            var viejo = escena.GetRootGameObjects().FirstOrDefault(g => g.name == NombreTutorial);
            if (viejo != null) Object.DestroyImmediate(viejo);
            var raiz = new GameObject(NombreTutorial);
            var tutorial = raiz.AddComponent<Tutorial>();

            var panel = UIVR.Panel("Panel", raiz.transform, new Vector2(460f, 440f), origen.Camera); // 0.46 × 0.44 m
            panel.transform.SetPositionAndRotation(PosicionPanel, Quaternion.LookRotation(PosicionPanel - OjosPiloto));
            var progreso = UIVR.Texto(panel.transform, "Progreso", "Tutorial", 20f, FontStyles.Italic, 26f);
            var titulo = UIVR.Texto(panel.transform, "Titulo", "Título", 38f, FontStyles.Bold, 50f);
            var instruccion = UIVR.Texto(panel.transform, "Instruccion", "Instrucción", 26f, FontStyles.Normal, 150f);
            var fila = UIVR.Fila("Botones", panel.transform, 70f);
            var saltar = UIVR.Boton(fila, "Boton Saltar", "Saltar tutorial", UIVR.ColorSalir, tutorial.Terminar);
            var siguiente = UIVR.Boton(fila, "Boton Siguiente", "Siguiente", UIVR.ColorBoton, tutorial.Siguiente);
            foreach (var b in new[] { saltar, siguiente })
            {
                b.GetComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1f;
                b.GetComponentInChildren<TextMeshProUGUI>().fontSize = 28f;
            }
            UIVR.CapaDeUI(panel);

            var so = new SerializedObject(tutorial);
            var lista = so.FindProperty("pasos");
            lista.arraySize = pasos.Length;
            for (int i = 0; i < pasos.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = pasos[i];
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("progreso").objectReferenceValue = progreso;
            so.FindProperty("titulo").objectReferenceValue = titulo;
            so.FindProperty("instruccion").objectReferenceValue = instruccion;
            so.FindProperty("botonSiguiente").objectReferenceValue = siguiente;
            so.FindProperty("textoSiguiente").objectReferenceValue = siguiente.GetComponentInChildren<TextMeshProUGUI>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[Tutorial] Listo con {pasos.Length} pasos.");
        }

        static void CrearPasosQueFalten()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaPasos))
                AssetDatabase.CreateFolder("Assets/_BioVR/Comun/Tutorial", "Pasos");
            foreach (var (archivo, titulo, instruccion, clave, opcional) in PasosBasicos)
            {
                var ruta = $"{CarpetaPasos}/{archivo}.asset";
                if (AssetDatabase.LoadAssetAtPath<PasoTutorial>(ruta) != null) continue;
                var paso = ScriptableObject.CreateInstance<PasoTutorial>();
                paso.titulo = titulo;
                paso.instruccion = instruccion;
                paso.clave = clave;
                paso.opcional = opcional;
                AssetDatabase.CreateAsset(paso, ruta);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
