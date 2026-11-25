using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Fotos de la Cocina en Play, desde los ojos del jugador, sin visor (para revisar letreros, orientación
    // y tamaños sin ponerse el Quest). Se corre en batch con Unity cerrado:
    //   Unity.exe -batchmode -projectPath <BioVR> -executeMethod BioVR.Minijuegos.Cocina.Herramientas.CapturasCocina.Capturar -capturas <carpeta> [-pasos N]
    // Abre Escenas/Cocina.unity, entra a Play, toma las fotos y cierra Unity.
    //   -pasos N: antes de cada ronda de fotos aprieta "Entendido" N veces (para avanzar la guía).
    [InitializeOnLoad]
    public static class CapturasCocina
    {
        const string ClaveDir = "BioVR.Capturas.Dir";
        const string ClaveFase = "BioVR.Capturas.Fase";
        const string ClaveEscena = "BioVR.Capturas.Escena";

        static double inicio;
        static int ronda;

        // Vistas: nombre, posición relativa a AnclaCocina (m), punto al que mira (relativo a AnclaCocina)
        static readonly (string nombre, Vector3 desde, Vector3 hacia, float fov)[] Vistas =
        {
            ("frente",        new Vector3(0f, 1.2f, 0f),     new Vector3(0f, 1.1f, 1f),       100f),
            ("izquierda",     new Vector3(0f, 1.2f, 0f),     new Vector3(-0.8f, 0.95f, 0.6f), 100f),
            ("derecha",       new Vector3(0f, 1.2f, 0f),     new Vector3(0.8f, 0.95f, 0.6f),  100f),
            ("arriba",        new Vector3(0f, 1.2f, 0f),     new Vector3(0f, 1.8f, 1.5f),     90f),
            ("ribosoma",      new Vector3(0f, 1.15f, 0.25f), new Vector3(0f, 0.9f, 0.62f),    60f),
            ("sintetasa",     new Vector3(-0.1f, 1.15f, 0.2f), new Vector3(-0.47f, 0.85f, 0.46f), 60f),
            ("corriente",     new Vector3(0f, 1.2f, 0.1f),   new Vector3(-0.55f, 1.06f, 0.45f), 60f),
            ("holograma",     new Vector3(0f, 1.2f, 0.1f),   new Vector3(0.62f, 1.1f, 0.58f), 70f),
            ("manivela",      new Vector3(-0.25f, 1.1f, 0.05f), new Vector3(-0.62f, 0.9f, 0.4f), 45f),
            ("tarjeta",       new Vector3(0f, 1.2f, 0.15f),  new Vector3(0f, 0.85f, 0.5f),    50f),
            ("tele",          new Vector3(0f, 1.2f, 0f),     new Vector3(0f, 1.9f, 1.55f),    50f),
            ("horquilla",     new Vector3(0.05f, 1.3f, 0.25f), new Vector3(0.25f, 0.9f, 0.62f), 50f),
            ("corriente2",    new Vector3(0f, 1.2f, 0f),     new Vector3(-0.75f, 1.1f, 0.15f), 100f),
            // Planta: desde arriba y un poco atrás, para ver cómo se reparten los elementos de la mesa
            ("derecha2",      new Vector3(0f, 1.2f, 0f),     new Vector3(1.3f, 1.4f, 0.1f),   90f),
            ("planta",        new Vector3(0f, 2.7f, -0.3f),  new Vector3(0f, 0.8f, 0.8f),     80f),
            // La ventana fija del tutorial desde el turno 2 (75° a la izquierda)
            ("tutorialfijo",  new Vector3(0f, 1.2f, 0f),     new Vector3(-1.25f, 1.6f, 0.34f), 80f),
            ("lejano",        new Vector3(0f, 1.2f, 0f),     new Vector3(0.82f, 1.6f, 1.55f), 35f),
            ("mandos",        new Vector3(0f, 1.2f, 0f),     new Vector3(0f, 0.95f, 0.4f),    55f),
            ("manivela2",     new Vector3(0f, 1.2f, 0f),     new Vector3(-0.2f, 1.05f, 0.6f), 60f),
        };

        static CapturasCocina()
        {
            if (SessionState.GetInt(ClaveFase, 0) == 1) EditorApplication.update += Tick;
        }

        public static void Capturar()
        {
            string dir = Argumento("-capturas") ?? Path.Combine(Path.GetTempPath(), "CapturasCocina");
            Directory.CreateDirectory(dir);
            string escena = Argumento("-escena") ?? ArmarEscenaCocina.Escena;
            SessionState.SetString(ClaveDir, dir);
            SessionState.SetString(ClaveEscena, escena);
            SessionState.SetInt("BioVR.Capturas.Pasos", int.TryParse(Argumento("-pasos"), out var p) ? p : 0);
            SessionState.SetInt("BioVR.Capturas.Rondas", int.TryParse(Argumento("-rondas"), out var r) ? r : 1);
            EditorSceneManager.OpenScene(escena, OpenSceneMode.Single);
            SessionState.SetInt(ClaveFase, 1);
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            // En batch el juego no avanza solo: se pausa y se avanza cuadro por cuadro, a 30 cuadros por segundo de juego
            if (inicio == 0) { inicio = 1; ronda = 0; Time.captureDeltaTime = 1f / 30f; EditorApplication.isPaused = true; }
            EditorApplication.Step();
            double t = Time.frameCount / 30.0;
            int rondas = SessionState.GetInt("BioVR.Capturas.Rondas", 1);
            if (t > 3 && ronda == 0)
            {
                // Sin visor, AnclarAlJugador puede no encender la estación: se enciende aquí
                var a = Ancla();
                if (a != null && !a.gameObject.activeSelf) { a.gameObject.SetActive(true); Debug.Log("[Capturas] Encendí AnclaCocina."); }
            }
            if (t < 6 + ronda * 3) return;

            if (ronda < rondas)
            {
                if (ronda == 0 && Argumento("-volcar") is string volcar)
                    foreach (var nombre in volcar.Split(','))
                        foreach (var tr in Resources.FindObjectsOfTypeAll<Transform>())
                            // "+" en lugar de espacios (la línea de comandos parte los nombres con espacios); basta el inicio del nombre
                            if (tr.name.StartsWith(nombre.Replace('+', ' ')) && tr.gameObject.scene.IsValid()) Volcar(tr, 0);
                Fotos($"r{ronda}");
                int pasos = SessionState.GetInt("BioVR.Capturas.Pasos", 0);
                var guia = Object.FindFirstObjectByType<GuiaCocina>();
                for (int i = 0; i < pasos && guia != null; i++) guia.Entendido();
                // -acciones a,b,c: una por ronda, después de sus fotos (terminar, siguiente, repetir, entendido)
                var acciones = (Argumento("-acciones") ?? "").Split(',');
                if (ronda < acciones.Length) Hacer(acciones[ronda]);
                ronda++;
                return;
            }
            SessionState.SetInt(ClaveFase, 0);
            EditorApplication.update -= Tick;
            Debug.Log("[Capturas] Listo.");
            EditorApplication.Exit(0);
        }

        static void Fotos(string prefijo)
        {
            string dir = SessionState.GetString(ClaveDir, Path.GetTempPath());
            var a = Ancla();
            var cam = new GameObject("CamaraCaptura").AddComponent<Camera>();
            cam.nearClipPlane = 0.02f;
            cam.farClipPlane = 500f;
            var rt = new RenderTexture(1280, 800, 24);
            cam.targetTexture = rt;
            var tex = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            foreach (var (nombre, desde, hacia, fov) in Vistas)
            {
                Vector3 p = a != null ? a.TransformPoint(desde) : desde;
                Vector3 q = a != null ? a.TransformPoint(hacia) : hacia;
                cam.transform.SetPositionAndRotation(p, Quaternion.LookRotation(q - p, Vector3.up));
                cam.fieldOfView = fov;
                cam.Render();
                if (nombre == "frente" || nombre == "derecha")
                    Debug.Log($"[Capturas] Carga {prefijo}_{nombre}: {UnityStats.triangles:N0} triángulos, {UnityStats.batches} lotes, {UnityStats.setPassCalls} SetPass");
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(dir, $"{prefijo}_{nombre}.png"), tex.EncodeToPNG());
            }
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(cam.gameObject);
            Object.DestroyImmediate(rt);
            var c = ControlCocina.Instancia;
            Debug.Log($"[Capturas] Fotos {prefijo} en {dir} · cuadro {Time.frameCount}, t {Time.time:F1}, " +
                      (c != null ? $"turno {c.NumeroTurno}, fase {c.Fase}, pausa {c.Pausado}" : "sin ControlCocina"));
        }

        static void Hacer(string accion)
        {
            var c = ControlCocina.Instancia;
            if (c == null || string.IsNullOrEmpty(accion)) return;
            switch (accion.Trim())
            {
                case "terminar": c.Terminar(); break;
                case "siguiente": c.SiguienteTurno(); break;
                case "repetir": c.RepetirTurno(); break;
                case "entendido": Object.FindFirstObjectByType<GuiaCocina>()?.Entendido(); break;
                case "menu": Object.FindFirstObjectByType<BioVR.Comun.MenuPausa>()?.Abrir(); break;
                case "horquilla": c.ActivarHorquilla(); break;
                case "raro": c.ActivarCodonRaro(); break;
                case "continuar": Object.FindFirstObjectByType<BioVR.Comun.MenuPausa>()?.Continuar(); break;
                case "vertutorial": Object.FindFirstObjectByType<BioVR.Comun.MenuPausa>()?.VerTutorialCocina(); break;
                default:
                    // ocultar:<nombre> apaga un objeto (para ver de dónde sale una pieza)
                    if (accion.StartsWith("ocultar:"))
                        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                            if (t.name == accion.Substring(8) && t.gameObject.scene.IsValid()) t.gameObject.SetActive(false);
                    break;
            }
            var s = Object.FindFirstObjectByType<EstacionSintetasa>(FindObjectsInactive.Include);
            Debug.Log($"[Capturas] Acción '{accion}': turno {c.NumeroTurno}, fase {c.Fase}, sintetasa {(s != null && s.gameObject.activeInHierarchy ? "visible" : "NO visible")}");
        }

        // Jerarquía con posición (relativa a AnclaCocina), si está encendido y sus componentes
        static void Volcar(Transform t, int nivel)
        {
            var a = Ancla();
            var p = a != null ? a.InverseTransformPoint(t.position) : t.position;
            var comps = new List<string>();
            foreach (var c in t.GetComponents<Component>()) if (c != null && !(c is Transform)) comps.Add(c.GetType().Name);
            var r = t.GetComponent<Renderer>();
            string tam = r != null ? $" tam {r.bounds.size.x:F2}x{r.bounds.size.y:F2}x{r.bounds.size.z:F2}" : "";
            if (t is RectTransform rt) tam += $" rect pos {rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0} z {rt.localPosition.z:F0} size {rt.sizeDelta.x:F0}x{rt.sizeDelta.y:F0} anc {rt.anchorMin.x:F1},{rt.anchorMin.y:F1}-{rt.anchorMax.x:F1},{rt.anchorMax.y:F1} esc {rt.localScale.x:F3}";
            Debug.Log($"[Volcar] {new string(' ', nivel * 2)}{t.name} {(t.gameObject.activeInHierarchy ? "" : "(apagado) ")}pos {p.x:F2},{p.y:F2},{p.z:F2} rotY {t.eulerAngles.y:F0} esc {t.lossyScale.x:F2}{tam} [{string.Join(",", comps)}]");
            var mf = t.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.subMeshCount > 1 && r != null)
                for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                {
                    var sb = mf.sharedMesh.GetSubMesh(i).bounds;
                    var c = a != null ? a.InverseTransformPoint(t.TransformPoint(sb.center)) : sb.center;
                    var mat = i < r.sharedMaterials.Length && r.sharedMaterials[i] != null ? r.sharedMaterials[i].name : "-";
                    Debug.Log($"[Volcar] {new string(' ', nivel * 2 + 2)}submalla {i} {mat}: centro {c.x:F2},{c.y:F2},{c.z:F2} tam {sb.size.x * t.lossyScale.x:F2}x{sb.size.y * t.lossyScale.y:F2}x{sb.size.z * t.lossyScale.z:F2}");
                }
            if (nivel < 6) foreach (Transform h in t) Volcar(h, nivel + 1);
        }

        static Transform Ancla()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<Transform>())
                if (go.name == "AnclaCocina" && go.gameObject.scene.IsValid()) return go;
            return null;
        }

        static string Argumento(string nombre)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == nombre) return args[i + 1];
            return null;
        }
    }
}
