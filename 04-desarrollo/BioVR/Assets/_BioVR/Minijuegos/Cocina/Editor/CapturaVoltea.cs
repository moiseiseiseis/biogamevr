using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Foto de la flecha VOLTEA de MarcaTutorial: la cámara mira al lado contrario del objeto señalado
    // (debe salir la flecha) y luego mira hacia el objeto (no debe salir).
    // Batch: -executeMethod BioVR.Minijuegos.Cocina.Herramientas.CapturaVoltea.Capturar -capturas <carpeta> -quit
    public static class CapturaVoltea
    {
        public static void Capturar()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturas");
            string carpeta = i >= 0 && i + 1 < args.Length ? args[i + 1] : "Capturas/Voltea";
            Directory.CreateDirectory(carpeta);

            EditorSceneManager.OpenScene("Assets/_BioVR/Escenas/Cocina.unity", OpenSceneMode.Single);
            var marca = Object.FindFirstObjectByType<MarcaTutorial>(FindObjectsInactive.Include);
            var objetivo = Object.FindFirstObjectByType<EstacionSintetasa>(FindObjectsInactive.Include);
            var cam = Camera.main;
            if (marca == null || objetivo == null || cam == null) { Debug.LogError($"[Voltea] falta algo: marca {marca} objetivo {objetivo} cámara {cam}"); return; }

            var ojos = cam.transform.position; // donde arranca el jugador en la escena
            Debug.Log($"[Voltea] ojos en {ojos}, objetivo en {objetivo.transform.position}");
            var hacia = Vector3.ProjectOnPlane(objetivo.transform.position - ojos, Vector3.up).normalized;
            var rt = new RenderTexture(1280, 1280, 24);
            cam.targetTexture = rt;
            cam.fieldOfView = 95f;

            Foto(cam, marca, objetivo.transform, ojos, Quaternion.LookRotation(-hacia), rt, Path.Combine(carpeta, "voltea_atras.png"));
            Foto(cam, marca, objetivo.transform, ojos, Quaternion.LookRotation(Quaternion.Euler(0f, 90f, 0f) * hacia), rt, Path.Combine(carpeta, "voltea_lado.png"));
            Foto(cam, marca, objetivo.transform, ojos, Quaternion.LookRotation(hacia), rt, Path.Combine(carpeta, "voltea_de_frente.png"));
        }

        static void Foto(Camera cam, MarcaTutorial marca, Transform objetivo, Vector3 ojos, Quaternion mirada, RenderTexture rt, string archivo)
        {
            cam.transform.SetPositionAndRotation(ojos, mirada);
            marca.Senalar(objetivo);
            var t = typeof(MarcaTutorial);
            t.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(marca, null);
            t.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(marca, null);
            var voltea = GameObject.Find("Flecha voltea (tutorial)");
            Debug.Log($"[Voltea] {Path.GetFileName(archivo)}: flecha {(voltea != null && voltea.activeInHierarchy ? "VISIBLE" : "oculta")}");
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(archivo, tex.EncodeToPNG());
            RenderTexture.active = null;
        }
    }
}
