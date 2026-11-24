using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BioVR.Pilotaje.Editor
{
    // Fotos del panel del tutorial de la misión, un paso por imagen, vistas desde los ojos del piloto.
    // Para revisar tamaño de letra y que el texto quepa sin abrir el visor.
    // Batch: -executeMethod BioVR.Pilotaje.Editor.CapturasPilotaje.Capturar -capturas <carpeta> -quit
    public static class CapturasPilotaje
    {
        public static void Capturar()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturas");
            string carpeta = i >= 0 && i + 1 < args.Length ? args[i + 1] : "Capturas/Pilotaje";
            Directory.CreateDirectory(carpeta);

            var escena = EditorSceneManager.OpenScene(ArmarPruebaPilotaje.Escena, OpenSceneMode.Single);
            // Dónde quedan la palanca y la brújula, en coordenadas de la cabina
            var cab = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "Cabina (provisional)");
            if (cab != null)
                foreach (var t in cab.GetComponentsInChildren<Transform>(true))
                    if (t.name == "Palanca" || t.name == "Brujula" || t.name.Contains("Boton Salto"))
                    {
                        var rs = t.GetComponentsInChildren<Renderer>(true);
                        var b = rs.Length > 0 ? rs[0].bounds : new Bounds(t.position, Vector3.zero);
                        foreach (var r in rs) b.Encapsulate(r.bounds);
                        Debug.Log($"[CapturasPilotaje] {t.name}: local {cab.transform.InverseTransformPoint(t.position)} limites {b.min} → {b.max}");
                    }
            var raiz = escena.GetRootGameObjects().First(g => g.name == "Tutorial de la mision");
            var tutorial = raiz.GetComponent<BioVR.Comun.Tutorial>();
            var so = new SerializedObject(tutorial);
            var pasos = Enumerable.Range(0, so.FindProperty("pasos").arraySize)
                .Select(k => (BioVR.Comun.PasoTutorial)so.FindProperty("pasos").GetArrayElementAtIndex(k).objectReferenceValue).ToArray();
            var panel = (GameObject)so.FindProperty("panel").objectReferenceValue;
            var titulo = (TextMeshProUGUI)so.FindProperty("titulo").objectReferenceValue;
            var instruccion = (TextMeshProUGUI)so.FindProperty("instruccion").objectReferenceValue;
            var progreso = (TextMeshProUGUI)so.FindProperty("progreso").objectReferenceValue;
            var siguiente = (GameObject)so.FindProperty("botonSiguiente").objectReferenceValue;
            var textoSig = (TextMeshProUGUI)so.FindProperty("textoSiguiente").objectReferenceValue;

            // Como en el visor: a 0.8 m del jugador (SeguirAlJugador), de frente
            var ojos = new Vector3(0f, 1.4f, 0f);
            panel.transform.SetPositionAndRotation(ojos + new Vector3(0f, -0.02f, 0.8f), Quaternion.identity);

            var go = new GameObject("Camara captura");
            var cam = go.AddComponent<Camera>();
            cam.transform.SetPositionAndRotation(ojos, Quaternion.identity);
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.25f, 0.1f, 0.12f);
            var rt = new RenderTexture(1280, 1280, 24);
            cam.targetTexture = rt;

            for (int k = 0; k < pasos.Length; k++)
            {
                var p = pasos[k];
                progreso.text = $"Misión · paso {k + 1} de {pasos.Length}";
                titulo.text = p.titulo;
                instruccion.text = p.instruccion;
                bool conBoton = string.IsNullOrEmpty(p.clave) || p.opcional;
                siguiente.SetActive(conBoton);
                textoSig.text = (k == pasos.Length - 1 ? "Terminar" : "Siguiente") +
                    $" ({BioVR.Comun.EtiquetasMandos.Colores.Etiqueta("X", BioVR.Comun.EtiquetasMandos.Colores.X)})";
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
                Canvas.ForceUpdateCanvases();
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(carpeta, $"paso{k + 1}.png"), tex.EncodeToPNG());
                Debug.Log($"[CapturasPilotaje] paso {k + 1}: alto del panel {((RectTransform)panel.transform).sizeDelta.y:0} mm");
            }
            // Vista amplia (campo de visión parecido al del Quest) con el panel donde lo pone SeguirAlJugador:
            // de frente, 0.85 m y 0.12 m abajo de los ojos. Sirve para ver qué tapa a qué (ASTRA, consola).
            panel.transform.SetPositionAndRotation(ojos + new Vector3(0f, -0.12f, 0.85f), Quaternion.identity);
            cam.fieldOfView = 95f;
            var astra = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "ASTRA");
            if (astra != null) Debug.Log($"[CapturasPilotaje] ASTRA en {astra.transform.position}");
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var amplia = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            amplia.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            amplia.Apply();
            File.WriteAllBytes(Path.Combine(carpeta, "amplia.png"), amplia.EncodeToPNG());
            if (astra != null)
            {
                astra.SetActive(false);
                cam.Render();
                RenderTexture.active = rt;
                amplia.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                amplia.Apply();
                File.WriteAllBytes(Path.Combine(carpeta, "amplia_sin_astra.png"), amplia.EncodeToPNG());
                astra.SetActive(true);
            }
            // Mirando a la derecha y un poco abajo, donde queda ASTRA
            cam.transform.rotation = Quaternion.Euler(20f, 50f, 0f);
            cam.Render();
            RenderTexture.active = rt;
            amplia.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            amplia.Apply();
            File.WriteAllBytes(Path.Combine(carpeta, "derecha.png"), amplia.EncodeToPNG());
            // Mirando a la izquierda y un poco abajo, donde queda la leyenda de pilotaje
            cam.transform.rotation = Quaternion.Euler(18f, -35f, 0f);
            cam.Render();
            RenderTexture.active = rt;
            amplia.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            amplia.Apply();
            File.WriteAllBytes(Path.Combine(carpeta, "izquierda.png"), amplia.EncodeToPNG());
            cam.transform.rotation = Quaternion.identity;
            // Qué hay justo detrás del centro del panel
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if (r.enabled && r.gameObject.activeInHierarchy && r.bounds.Contains(new Vector3(0f, 1.6f, r.bounds.center.z)) &&
                    Mathf.Abs(r.bounds.center.x) < 0.3f && r.bounds.center.z > 0.9f && r.bounds.center.z < 4f)
                    Debug.Log($"[CapturasPilotaje] Al centro: {r.transform.root.name}/{r.name} {r.bounds.center} {r.bounds.size}");
            RenderTexture.active = null;
        }
    }
}
