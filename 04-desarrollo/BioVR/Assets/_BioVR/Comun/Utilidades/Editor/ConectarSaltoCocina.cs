using System.Linq;
using BioVR.Escala;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Menú BioVR → Conectar salto a la Cocina (A6 y flujo hangar → Cocina).
    //
    // En la llegada a la célula, el último Salto Matrioska (botón o palanca) reduce la nave a 5 nm y carga la
    // Cocina de proteínas (GameManager.IrALaCocina). Al terminar la Cocina, el botón de menú de la pantalla final
    // regresa a la escena 0 (el hangar). Este menú deja todo listo para eso:
    //   1. Escala/Datos/Escala4_Molecular.asset: el nivel de 5 nm (lo lee el monitor de reducción y el túnel del salto).
    //   2. Lo agrega como quinto nivel al GestorEscala de 00-EscenaInicial.
    //   3. Build Settings: 00-EscenaInicial primero (escena 0) y Cocina después.
    // Se puede correr varias veces.
    public static class ConectarSaltoCocina
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string EscenaCocina = "Assets/_BioVR/Escenas/Cocina.unity";
        const string Nivel = "Assets/_BioVR/Escala/Datos/Escala4_Molecular.asset";

        [MenuItem("BioVR/Conectar salto a la Cocina")]
        public static void Conectar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EscenaCocina) == null)
            {
                Debug.LogError($"[Secuencia] No existe {EscenaCocina}. Córrela primero: BioVR → Cocina → Armar escena Cocina.");
                return;
            }

            // 1. Nivel de 5 nm
            var nivel = AssetDatabase.LoadAssetAtPath<NivelEscalaData>(Nivel);
            if (nivel == null)
            {
                nivel = ScriptableObject.CreateInstance<NivelEscalaData>();
                nivel.nombreNivel = "Escala molecular";
                nivel.tamanoMecha = "5 nm";
                nivel.inercia = 0f;
                nivel.viscosidad = 10f;
                nivel.intensidadEmpujones = 1.5f;
                nivel.colorFluido = new Color(0.75f, 0.6f, 0.95f, 1f);
                AssetDatabase.CreateAsset(nivel, Nivel);
            }

            // 2. Quinto nivel del GestorEscala
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var gestor = Object.FindFirstObjectByType<GestorEscala>(FindObjectsInactive.Include);
            if (gestor == null) { Debug.LogError("[Secuencia] 00-EscenaInicial no tiene GestorEscala."); return; }
            var so = new SerializedObject(gestor);
            var niveles = so.FindProperty("nivelesEscala");
            if (niveles.arraySize < 4) { Debug.LogError("[Secuencia] El GestorEscala no tiene sus 4 niveles (Hangar a Citoplasma)."); return; }
            niveles.arraySize = 5;
            niveles.GetArrayElementAtIndex(4).objectReferenceValue = nivel;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);

            // 3. Build Settings: la escena inicial es la 0 (a ella regresa la pantalla final de la Cocina)
            var otras = EditorBuildSettings.scenes.Where(e => e.path != EscenaInicial && e.path != EscenaCocina);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(EscenaInicial, true),
                new EditorBuildSettingsScene(EscenaCocina, true),
            }.Concat(otras).ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log("[Secuencia] Listo: salto a 5 nm conectado (GestorEscala con 5 niveles; Build Settings: 00-EscenaInicial, Cocina). " +
                      "En la llegada, Salto Matrioska o la palanca (o la tecla 5 en el editor) cargan la Cocina.");
        }
    }
}
