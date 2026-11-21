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
    // para que se instale junto a BioVR sin reemplazarlo. BioVR → Build APK Fase A hace lo mismo con la escena de traducción,
    // y BioVR → Build APK Cocina con la escena integrada de la Cocina de proteínas.
    public static class BuildAndroid
    {
        const string RutaApk = "Builds/BioVR.apk";
        const string RutaApkExpresion = "Builds/ExpresionGenetica.apk";
        const string PaqueteExpresion = "com.biogamevr.expresiongenetica";
        const string NombreExpresion = "BioVR Expresión Genética";
        const string RutaApkFaseA = "Builds/FaseA.apk";
        const string PaqueteFaseA = "com.biogamevr.fasea";
        const string NombreFaseA = "BioVR Fase A";
        const string RutaApkCocina = "Builds/Cocina.apk";
        const string PaqueteCocina = "com.biogamevr.cocina";
        const string NombreCocina = "BioVR Cocina";
        const string RutaApkPilotaje = "Builds/Pilotaje.apk";
        const string PaquetePilotaje = "com.biogamevr.pilotaje";
        const string NombrePilotaje = "BioVR Pilotaje";

        // Prueba del pilotaje libre en la célula (modo investigación), aparte del juego: empieza en la célula y,
        // con la misión cumplida, el salto a 5 nm lleva a la Cocina (que también va en este APK). Al terminar la
        // Cocina, su pantalla final regresa a la escena 0, que aquí es la del pilotaje.
        [MenuItem("BioVR/Build APK Pilotaje")]
        public static void ConstruirPilotaje() =>
            Terminar(GenerarAparte(new[] { BioVR.Pilotaje.Editor.ArmarPruebaPilotaje.Escena,
                    BioVR.Minijuegos.Cocina.Herramientas.ArmarEscenaCocina.Escena },
                "BioVR → Pilotaje → Armar prueba de pilotaje", PaquetePilotaje, NombrePilotaje, RutaApkPilotaje));

        [MenuItem("BioVR/Build APK")]
        public static void Construir()
        {
            var escenas = EditorBuildSettings.scenes.Where(e => e.enabled).Select(e => e.path).ToArray();
            Terminar(Generar(escenas, RutaApk));
        }

        [MenuItem("BioVR/Build APK Expresión Genética")]
        public static void ConstruirExpresion() =>
            Terminar(GenerarAparte(ArmarEscenaExpresionGenetica.Escena, "BioVR → Armar escena Expresión Genética",
                PaqueteExpresion, NombreExpresion, RutaApkExpresion));

        // Fase A de "Construye la proteína": la secuencia de traducción de Iker (Rol3_Traduccion), sola.
        [MenuItem("BioVR/Build APK Fase A (Construye la proteína)")]
        public static void ConstruirFaseA() =>
            Terminar(GenerarAparte(BioVR.Minijuegos.ExpresionGenetica.Herramientas.ArmarPruebaTraduccion.Escena,
                "BioVR → Armar prueba Traducción (Rol 3)", PaqueteFaseA, NombreFaseA, RutaApkFaseA));

        // La Cocina de proteínas (M9): la escena integrada, sola, para probarla sin pasar por el hangar.
        [MenuItem("BioVR/Build APK Cocina")]
        public static void ConstruirCocina() =>
            Terminar(GenerarAparte(BioVR.Minijuegos.Cocina.Herramientas.ArmarEscenaCocina.Escena,
                "BioVR → Cocina → Armar escena Cocina", PaqueteCocina, NombreCocina, RutaApkCocina));

        // Compila una sola escena con otro paquete y otro nombre, para que se instale junto a BioVR sin reemplazarlo.
        static bool GenerarAparte(string escena, string armador, string paqueteNuevo, string nombreNuevo, string ruta) =>
            GenerarAparte(new[] { escena }, armador, paqueteNuevo, nombreNuevo, ruta);

        // Igual, con varias escenas (la primera es la escena 0, con la que abre la app)
        static bool GenerarAparte(string[] escenas, string armador, string paqueteNuevo, string nombreNuevo, string ruta)
        {
            foreach (var escena in escenas)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(escena) == null)
                {
                    Debug.LogError($"[BuildAndroid] No existe {escena}. Corre primero {armador}.");
                    return false;
                }

            // Se cambian el paquete y el nombre solo durante la compilación y luego se regresan
            var android = NamedBuildTarget.Android;
            var paquete = PlayerSettings.GetApplicationIdentifier(android);
            var nombre = PlayerSettings.productName;
            try
            {
                PlayerSettings.SetApplicationIdentifier(android, paqueteNuevo);
                PlayerSettings.productName = nombreNuevo;
                return Generar(escenas, ruta);
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(android, paquete);
                PlayerSettings.productName = nombre;
            }
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
