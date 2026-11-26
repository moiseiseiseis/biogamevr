using System;
using System.IO;
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
    // BioVR → Build APK Tienda (CellVerse) genera el juego completo firmado para el canal privado de la tienda de Meta.
    public static class BuildAndroid
    {
        const string RutaApk = "Builds/BioVR.apk";
        const string PaqueteJuego = "com.biogamevr.biovr";
        const string NombreJuego = "BioVR";

        // Tienda de Meta (canal privado). El paquete queda fijo para siempre tras la primera subida aceptada.
        // VersionTienda debe subir en cada APK que se mande a Meta (la última subida aceptada debe ser menor).
        const string RutaApkTienda = "Builds/CellVerse.apk";
        const string PaqueteTienda = "biogame.cellverse";
        const string NombreTienda = "CellVerse";
        const int VersionTienda = 4;
        // La llave vive fuera del repo, en la laptop de Moisés (ver firma.txt en esa carpeta).
        static string ArchivoFirma =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cellverse-keystore", "firma.txt");
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

        // El paquete y el nombre se fijan aquí porque, si Unity se cierra a media build aparte (apagón del 24-nov),
        // ProjectSettings se queda con los del pilotaje y todas las builds siguientes salen con ese nombre.
        [MenuItem("BioVR/Build APK")]
        public static void Construir()
        {
            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, PaqueteJuego);
            PlayerSettings.productName = NombreJuego;
            Terminar(Generar(EscenasActivas(), RutaApk, BuildOptions.Development));
        }

        // El juego completo con lo que pide la tienda de Meta: Target SDK 34, instalación "auto", orientación
        // horizontal, sin Development Build y firmado con la llave propia. Todo se regresa al terminar.
        [MenuItem("BioVR/Build APK Tienda (CellVerse)")]
        public static void ConstruirTienda()
        {
            if (!LeerFirma(out var keystore, out var alias, out var contrasena))
            {
                Terminar(false);
                return;
            }

            var android = NamedBuildTarget.Android;
            var paquete = PlayerSettings.GetApplicationIdentifier(android);
            var nombre = PlayerSettings.productName;
            var version = PlayerSettings.Android.bundleVersionCode;
            var sdk = PlayerSettings.Android.targetSdkVersion;
            var instalacion = PlayerSettings.Android.preferredInstallLocation;
            var orientacion = PlayerSettings.defaultInterfaceOrientation;
            var llavePropia = PlayerSettings.Android.useCustomKeystore;
            var keystoreAntes = PlayerSettings.Android.keystoreName;
            var aliasAntes = PlayerSettings.Android.keyaliasName;
            var ok = false;
            try
            {
                PlayerSettings.SetApplicationIdentifier(android, PaqueteTienda);
                PlayerSettings.productName = NombreTienda;
                PlayerSettings.Android.bundleVersionCode = VersionTienda;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
                PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keystorePass = contrasena;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = contrasena;
                ok = Generar(EscenasActivas(), RutaApkTienda, BuildOptions.None);
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(android, paquete);
                PlayerSettings.productName = nombre;
                PlayerSettings.Android.bundleVersionCode = version;
                PlayerSettings.Android.targetSdkVersion = sdk;
                PlayerSettings.Android.preferredInstallLocation = instalacion;
                PlayerSettings.defaultInterfaceOrientation = orientacion;
                PlayerSettings.Android.useCustomKeystore = llavePropia;
                PlayerSettings.Android.keystoreName = keystoreAntes;
                PlayerSettings.Android.keyaliasName = aliasAntes;
                PlayerSettings.Android.keystorePass = "";
                PlayerSettings.Android.keyaliasPass = "";
            }
            if (ok) Debug.Log($"[BuildAndroid] Tienda: {PaqueteTienda} versionCode {VersionTienda}. Antes de la próxima subida, sube VersionTienda.");
            Terminar(ok);
        }

        // firma.txt: líneas clave=valor con keystore, alias y contrasena
        static bool LeerFirma(out string keystore, out string alias, out string contrasena)
        {
            keystore = alias = contrasena = null;
            if (!File.Exists(ArchivoFirma))
            {
                Debug.LogError($"[BuildAndroid] No encuentro {ArchivoFirma}. La build de la tienda solo se genera en la laptop que tiene la llave.");
                return false;
            }
            foreach (var linea in File.ReadAllLines(ArchivoFirma))
            {
                var i = linea.IndexOf('=');
                if (linea.StartsWith("#") || i < 0) continue;
                var clave = linea.Substring(0, i).Trim();
                var valor = linea.Substring(i + 1).Trim();
                if (clave == "keystore") keystore = valor;
                else if (clave == "alias") alias = valor;
                else if (clave == "contrasena") contrasena = valor;
            }
            if (string.IsNullOrEmpty(keystore) || !File.Exists(keystore) || string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(contrasena))
            {
                Debug.LogError($"[BuildAndroid] {ArchivoFirma} está incompleto o el keystore no existe.");
                return false;
            }
            return true;
        }

        static string[] EscenasActivas() =>
            EditorBuildSettings.scenes.Where(e => e.enabled).Select(e => e.path).ToArray();

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
                return Generar(escenas, ruta, BuildOptions.Development);
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(android, paquete);
                PlayerSettings.productName = nombre;
            }
        }

        // Development permite ver los logs del visor con adb logcat; la tienda no acepta APK depurables
        static bool Generar(string[] escenas, string ruta, BuildOptions modo)
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var opciones = new BuildPlayerOptions
            {
                scenes = escenas,
                locationPathName = ruta,
                target = BuildTarget.Android,
                options = modo
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
