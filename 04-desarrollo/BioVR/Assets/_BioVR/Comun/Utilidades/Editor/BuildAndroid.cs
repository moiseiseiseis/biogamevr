using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Genera el APK para el visor con las escenas activas de Build Settings.
    // Desde el editor: menú BioVR → Build APK.
    // Desde la terminal: Unity -batchmode -buildTarget Android -executeMethod BioVR.Comun.Editor.BuildAndroid.Construir
    // BioVR → Build APK Expresión Genética genera solo la escena del minijuego, con otro nombre de paquete
    // para que se instale junto a BioVR sin reemplazarlo.
    public static class BuildAndroid
    {
        const string RutaApk = "Builds/BioVR.apk";
        const string RutaApkExpresion = "Builds/ExpresionGenetica.apk";
        const string PaqueteExpresion = "com.biogamevr.expresiongenetica";
        const string NombreExpresion = "BioVR Expresión Genética";

        [MenuItem("BioVR/Build APK")]
        public static void Construir()
        {
            var escenas = EditorBuildSettings.scenes.Where(e => e.enabled).Select(e => e.path).ToArray();
            Terminar(Generar(escenas, RutaApk));
        }

        [MenuItem("BioVR/Build APK Expresión Genética")]
        public static void ConstruirExpresion()
        {
            var escena = ArmarEscenaExpresionGenetica.Escena;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(escena) == null)
            {
                Debug.LogError($"[BuildAndroid] No existe {escena}. Corre primero BioVR → Armar escena Expresión Genética.");
                Terminar(false);
                return;
            }

            // Se cambian el paquete y el nombre solo durante la compilación y luego se regresan
            var android = NamedBuildTarget.Android;
            var paquete = PlayerSettings.GetApplicationIdentifier(android);
            var nombre = PlayerSettings.productName;
            bool ok;
            try
            {
                PlayerSettings.SetApplicationIdentifier(android, PaqueteExpresion);
                PlayerSettings.productName = NombreExpresion;
                ok = Generar(new[] { escena }, RutaApkExpresion);
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(android, paquete);
                PlayerSettings.productName = nombre;
            }
            Terminar(ok);
        }

        static bool Generar(string[] escenas, string ruta)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var opciones = new BuildPlayerOptions
            {
                scenes = escenas,
                locationPathName = ruta,
                target = BuildTarget.Android,
                options = BuildOptions.Development // permite ver los logs del visor con adb logcat
            };

            var resumen = BuildPipeline.BuildPlayer(opciones).summary;
            Debug.Log($"[BuildAndroid] {resumen.result}: {ruta} · {resumen.totalSize / (1024f * 1024f):F1} MB · " +
                      $"{resumen.totalTime.TotalMinutes:F1} min · {resumen.totalErrors} errores, {resumen.totalWarnings} advertencias");
            return resumen.result == BuildResult.Succeeded;
        }

        static void Terminar(bool ok)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
