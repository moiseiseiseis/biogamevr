using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Genera el APK para el visor con las escenas activas de Build Settings.
    // Desde el editor: menú BioVR → Build APK.
    // Desde la terminal: Unity -batchmode -buildTarget Android -executeMethod BioVR.Comun.Editor.BuildAndroid.Construir
    public static class BuildAndroid
    {
        const string RutaApk = "Builds/BioVR.apk";

        [MenuItem("BioVR/Build APK")]
        public static void Construir()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var opciones = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(e => e.enabled).Select(e => e.path).ToArray(),
                locationPathName = RutaApk,
                target = BuildTarget.Android,
                options = BuildOptions.Development // permite ver los logs del visor con adb logcat
            };

            var resumen = BuildPipeline.BuildPlayer(opciones).summary;
            Debug.Log($"[BuildAndroid] {resumen.result}: {RutaApk} · {resumen.totalSize / (1024f * 1024f):F1} MB · " +
                      $"{resumen.totalTime.TotalMinutes:F1} min · {resumen.totalErrors} errores, {resumen.totalWarnings} advertencias");

            if (Application.isBatchMode)
                EditorApplication.Exit(resumen.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
