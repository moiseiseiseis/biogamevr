using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Pone android:allowBackup="false" en el manifiesto de la app. El escaneo de seguridad de la tienda de Meta
    // lo marca como vulnerabilidad cuando no está definido (Android lo toma como true y permite copiar los datos
    // privados de la app). Corre en todas las builds de Android, después de que Unity genera el proyecto de Gradle.
    public class SinRespaldoAndroid : IPostGenerateGradleAndroidProject
    {
        const string EspacioAndroid = "http://schemas.android.com/apk/res/android";

        public int callbackOrder => 100;

        // ruta = carpeta unityLibrary; el manifiesto que define la <application> de la app está en launcher
        public void OnPostGenerateGradleAndroidProject(string ruta)
        {
            var manifiesto = Path.Combine(ruta, "..", "launcher", "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifiesto))
            {
                Debug.LogWarning($"[SinRespaldoAndroid] No encontré {manifiesto}; allowBackup se queda como esté.");
                return;
            }

            var xml = new XmlDocument();
            xml.Load(manifiesto);
            if (xml.SelectSingleNode("/manifest/application") is not XmlElement aplicacion)
            {
                Debug.LogWarning("[SinRespaldoAndroid] El manifiesto de launcher no tiene <application>.");
                return;
            }
            aplicacion.SetAttribute("allowBackup", EspacioAndroid, "false");
            xml.Save(manifiesto);
            Debug.Log("[SinRespaldoAndroid] android:allowBackup=\"false\" en launcher/src/main/AndroidManifest.xml");
        }
    }
}
