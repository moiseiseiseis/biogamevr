using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Entornos.Editor
{
    // Corre el juego en Play, lo lleva al primer salto (Viaje, salto 1 = zona Vasos) y toma fotos desde la cámara
    // del jugador, con los scripts funcionando como en el visor. Registra qué zonas y modelos están activos.
    // Batch (SIN -quit; sale sola): -executeMethod BioVR.Entornos.Editor.PruebaVistaTorrente.Iniciar -capturas <carpeta>
    [InitializeOnLoad]
    public static class PruebaVistaTorrente
    {
        const string Clave = "BioVR.PruebaVistaTorrente";
        static int cuadros;

        static PruebaVistaTorrente()
        {
            if (!SessionState.GetBool(Clave, false)) return;
            EditorApplication.playModeStateChanged += AlCambiarPlay;
            if (EditorApplication.isPlaying) EditorApplication.update += Paso;
        }

        public static void Iniciar()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturas");
            SessionState.SetString(Clave + ".carpeta", i >= 0 && i + 1 < args.Length ? args[i + 1] : "Capturas/VistaTorrente");
            SessionState.SetBool(Clave, true);
            EditorSceneManager.OpenScene("Assets/_BioVR/Escenas/00-EscenaInicial.unity");
            EditorApplication.playModeStateChanged += AlCambiarPlay;
            EditorApplication.isPlaying = true;
        }

        static void AlCambiarPlay(PlayModeStateChange cambio)
        {
            if (cambio == PlayModeStateChange.EnteredPlayMode) { cuadros = 0; EditorApplication.update -= Paso; EditorApplication.update += Paso; }
        }

        static void Paso()
        {
            if (!EditorApplication.isPlaying) return;
            cuadros++;
            var gm = BioVR.Secuencia.GameManager.Instance;
            if (cuadros == 60)
            {
                if (gm == null) { Terminar("no hay GameManager"); return; }
                gm.IrA(BioVR.Secuencia.GameState.Viaje, instantaneo: true);
            }
            if (cuadros == 180) Fotos(gm);
        }

        static void Fotos(BioVR.Secuencia.GameManager gm)
        {
            string carpeta = SessionState.GetString(Clave + ".carpeta", "Capturas/VistaTorrente");
            Directory.CreateDirectory(carpeta);
            Debug.Log($"[VistaTorrente] Estado {gm.Estado}, salto {gm.Salto}");
            var zonas = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Zonas" && t.parent == null);
            if (zonas != null)
                foreach (Transform z in zonas)
                    Debug.Log($"[VistaTorrente] zona {z.name}: activa={z.gameObject.activeInHierarchy} pos={z.position} rot={z.eulerAngles} escala={z.lossyScale}");
            var modelo = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name.Contains("Torrente sanguíneo (Ulises)"));
            if (modelo != null)
            {
                var rs = modelo.GetComponentsInChildren<Renderer>(false);
                var vaso = rs.FirstOrDefault(r => r.name.StartsWith("Vaso_Sanguineo"));
                Debug.Log($"[VistaTorrente] modelo activo={modelo.gameObject.activeInHierarchy} renderers={rs.Length} " +
                          $"vaso={(vaso != null ? vaso.bounds.ToString() : "no")} visible={(vaso != null && vaso.isVisible)}");
            }
            else Debug.Log("[VistaTorrente] NO está el modelo de Ulises en la escena en Play");

            var principal = Camera.main;
            Debug.Log($"[VistaTorrente] cámara {(principal != null ? principal.name + " en " + principal.transform.position + " far " + principal.farClipPlane + " máscara " + principal.cullingMask : "ninguna")}");
            var cam = new GameObject("Camara prueba").AddComponent<Camera>();
            if (principal != null) { cam.CopyFrom(principal); cam.stereoTargetEye = StereoTargetEyeMask.None; }
            cam.fieldOfView = 95f;
            var ojos = principal != null ? principal.transform.position : new Vector3(0f, 1.4f, 0f);
            var rt = new RenderTexture(1024, 1024, 24);
            cam.targetTexture = rt;
            foreach (var (nombre, y) in new[] { ("frente", 0f), ("lado", 70f), ("atras", 180f) })
            {
                cam.transform.SetPositionAndRotation(ojos, Quaternion.Euler(0f, y, 0f));
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(carpeta, $"play_{nombre}.png"), tex.EncodeToPNG());
            }
            RenderTexture.active = null;
            Terminar("listo");
        }

        static void Terminar(string motivo)
        {
            Debug.Log($"[VistaTorrente] Fin: {motivo}");
            EditorApplication.update -= Paso;
            SessionState.SetBool(Clave, false);
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }
    }
}
