using System.IO;
using System.Linq;
using BioVR.Secuencia;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Pilotaje.Editor
{
    // Prueba en Play del juego completo con el pilotaje integrado: salta el tutorial (como un jugador que lo cierra),
    // lleva el juego a la llegada a la célula y revisa que el tutorial se reabra en el aviso de mareo, que el
    // pilotaje esté activo y que la Cocina siga bloqueada sin los 4 organelos. Toma una foto desde el asiento.
    // Batch (SIN -quit; sale sola): -executeMethod BioVR.Pilotaje.Editor.PruebaFlujoPilotaje.Iniciar -capturas <carpeta>
    [InitializeOnLoad]
    public static class PruebaFlujoPilotaje
    {
        const string Clave = "BioVR.PruebaFlujoPilotaje";
        static int cuadros;

        static PruebaFlujoPilotaje()
        {
            if (!SessionState.GetBool(Clave, false)) return;
            EditorApplication.playModeStateChanged += AlCambiarPlay;
        }

        public static void Iniciar()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturas");
            SessionState.SetString(Clave + ".carpeta", i >= 0 && i + 1 < args.Length ? args[i + 1] : "Capturas/FlujoPilotaje");
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
            var gm = GameManager.Instance;
            var tutorial = Object.FindFirstObjectByType<BioVR.Comun.Tutorial>();
            if (cuadros == 60)
            {
                Debug.Log($"[FlujoPilotaje] Inicio: estado {gm?.Estado}, paso del tutorial: {BioVR.Comun.Tutorial.PasoActual?.titulo ?? "(ninguno)"}, " +
                          $"pasos: {(tutorial != null ? ((BioVR.Comun.PasoTutorial[])typeof(BioVR.Comun.Tutorial).GetField("pasos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tutorial)).Length : 0)}");
                tutorial?.Terminar(); // el jugador salta el tutorial
                gm.IrA(GameState.Llegada, instantaneo: true);
            }
            if (cuadros == 200)
            {
                var motivo = gm.BloqueoCocina?.Invoke();
                var brujula = GameObject.Find("Brujula");
                Debug.Log($"[FlujoPilotaje] Llegada: estado {gm.Estado}, salto {gm.Salto}, pilotaje activo {PilotajeLibre.Activo}, " +
                          $"brújula {(brujula != null && brujula.activeInHierarchy)}, paso del tutorial: {BioVR.Comun.Tutorial.PasoActual?.titulo ?? "(ninguno)"}, " +
                          $"Cocina bloqueada: {(string.IsNullOrEmpty(motivo) ? "NO" : motivo)}");
                Foto();
                Terminar();
            }
        }

        static void Foto()
        {
            string carpeta = SessionState.GetString(Clave + ".carpeta", "Capturas/FlujoPilotaje");
            Directory.CreateDirectory(carpeta);
            var principal = Camera.main;
            var cam = new GameObject("Camara prueba").AddComponent<Camera>();
            if (principal != null) { cam.CopyFrom(principal); cam.stereoTargetEye = StereoTargetEyeMask.None; }
            cam.fieldOfView = 95f;
            var rt = new RenderTexture(1024, 1024, 24);
            cam.targetTexture = rt;
            cam.transform.SetPositionAndRotation(principal != null ? principal.transform.position : new Vector3(0f, 1.4f, 0f), Quaternion.identity);
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(carpeta, "llegada.png"), tex.EncodeToPNG());
            RenderTexture.active = null;
        }

        static void Terminar()
        {
            EditorApplication.update -= Paso;
            SessionState.SetBool(Clave, false);
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }
    }
}
