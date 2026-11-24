using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Prueba de la corriente en la escena real (24-nov: "desaparecían" CAA y UUG y se repetían los señuelos).
    // Juega los 3 turnos por código como un jugador que siempre acierta: cuando un ribosoma pide un codón, espera a
    // que su ARNt se VEA en la corriente, lo carga y lo pone. Mide cuánto tarda en verse cada ARNt pedido.
    // Falla si alguno no aparece o tarda más de dos vueltas del camino.
    // Se corre en batch con Unity cerrado:
    //   Unity.exe -batchmode -projectPath <BioVR> -executeMethod BioVR.Minijuegos.Cocina.Herramientas.PruebaCorriente.Probar -logFile <log>
    [InitializeOnLoad]
    public static class PruebaCorriente
    {
        const string ClaveFase = "BioVR.PruebaCorriente.Fase";
        const float Fps = 30f;
        const float LimiteTurno = 300f; // s de juego por turno

        static int cuadro, turnoProbado, fallas, aciertos;
        static float inicioTurno, ultimaAccion;
        static readonly Dictionary<string, float> pedidoDesde = new Dictionary<string, float>(); // "r0:12" → t en que se pidió
        static readonly List<float> esperas = new List<float>();
        static int maxVisibles, maxEscena;

        static PruebaCorriente()
        {
            if (SessionState.GetInt(ClaveFase, 0) == 1) EditorApplication.update += Tick;
        }

        public static void Probar()
        {
            EditorSceneManager.OpenScene(ArmarEscenaCocina.Escena, OpenSceneMode.Single);
            SessionState.SetInt(ClaveFase, 1);
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        static float T => cuadro / Fps;

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (cuadro == 0) { Time.captureDeltaTime = 1f / Fps; EditorApplication.isPaused = true; }
            EditorApplication.Step();
            cuadro++;

            var c = ControlCocina.Instancia;
            if (T < 3f || c == null) return;

            if (turnoProbado == 0 && inicioTurno == 0f)
            {
                // Sin guía ni tutorial (pausan el juego) y con la estación encendida, como en las capturas
                foreach (var g in Object.FindObjectsByType<GuiaCocina>(FindObjectsSortMode.None)) Object.DestroyImmediate(g.gameObject);
                foreach (var tc in Object.FindObjectsByType<TutorialCocina>(FindObjectsSortMode.None)) tc.enabled = false;
                foreach (var tr in Resources.FindObjectsOfTypeAll<Transform>())
                    if (tr.name == "AnclaCocina" && tr.gameObject.scene.IsValid()) tr.gameObject.SetActive(true);
                c.PonerPausa(false);
                c.RepetirTurno();
                EmpezarTurno(c);
                return;
            }

            if (c.Pausado) c.PonerPausa(false);
            if (c.HorquillaActiva) c.GolpearHorquilla();

            var corriente = Object.FindFirstObjectByType<CorrienteARNt>();
            var todos = Object.FindObjectsByType<ARNtCocina>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int visibles = 0;
            foreach (var a in todos) if (a.Lugar == LugarARNt.Corriente && a.gameObject.activeInHierarchy) visibles++;
            maxVisibles = Mathf.Max(maxVisibles, visibles);
            maxEscena = Mathf.Max(maxEscena, todos.Length);

            if (c.Fase == FaseCocina.Armando)
            {
                // Un jugador que tarda 1 s entre acción y acción
                if (T - ultimaAccion >= 1f)
                {
                    ultimaAccion = T;
                    foreach (var a in todos) if (a.Lugar == LugarARNt.Salida) c.Reciclar(a);

                    for (int i = 0; i < c.Ribosomas.Count; i++)
                    {
                        var r = c.Ribosomas[i];
                        if (c.EnParo(r)) { c.TocarFin(r); continue; }
                        var pide = c.PideAhora(r);
                        if (pide == null) continue;
                        string clave = $"r{i}:{r.indice}";
                        if (!pedidoDesde.ContainsKey(clave)) pedidoDesde[clave] = T;

                        ARNtCocina visto = null;
                        foreach (var a in todos)
                            if (a.Lugar == LugarARNt.Corriente && a.gameObject.activeInHierarchy && a.CodonQueLee == pide.Value.tripleta) { visto = a; break; }
                        if (visto == null)
                        {
                            if (T - pedidoDesde[clave] > 30f)
                            {
                                Debug.LogError($"[PruebaCorriente] FALLA turno {turnoProbado + 1}: el ribosoma {i} pide {pide.Value.tripleta} (anticodón {pide.Value.Anticodon}) y no se ve en la corriente después de 30 s. " +
                                               $"Visibles: {Lista(todos, true)} · escondidos: {Lista(todos, false)}");
                                fallas++;
                                pedidoDesde[clave] = T; // no repetir el error cada segundo
                            }
                            continue;
                        }
                        float espera = T - pedidoDesde[clave];
                        esperas.Add(espera);
                        if (espera > 25f) { Debug.LogError($"[PruebaCorriente] LENTO: {pide.Value.tripleta} tardó {espera:F1} s en verse"); fallas++; }
                        c.Agarrar(visto);
                        c.TerminarCarga(visto, forzarError: false);
                        c.Aceptar(visto, r);
                        aciertos++;
                    }
                }
                if (T - inicioTurno > LimiteTurno)
                {
                    Debug.LogError($"[PruebaCorriente] FALLA turno {turnoProbado + 1}: no terminó en {LimiteTurno} s");
                    fallas++;
                    SiguienteOTerminar(c);
                }
                return;
            }

            // El turno terminó (plegado, búsqueda de la mutación o final): pasar al siguiente
            if (c.Fase == FaseCocina.Busca) { c.TocarCuenta(c.Turno.mutacion + 1); return; }
            if (c.Fase == FaseCocina.Decide) { c.UsarProteasoma(); return; }
            if (c.Fase == FaseCocina.Final || c.Fase == FaseCocina.Fin) SiguienteOTerminar(c);
        }

        static void EmpezarTurno(ControlCocina c)
        {
            inicioTurno = T;
            ultimaAccion = T;
            pedidoDesde.Clear();
            Debug.Log($"[PruebaCorriente] Empieza turno {c.NumeroTurno + 1}");
        }

        static void SiguienteOTerminar(ControlCocina c)
        {
            float max = 0f, suma = 0f;
            foreach (var e in esperas) { max = Mathf.Max(max, e); suma += e; }
            Debug.Log($"[PruebaCorriente] Turno {turnoProbado + 1}: {aciertos} ARNt puestos, espera promedio {(esperas.Count > 0 ? suma / esperas.Count : 0):F1} s, máxima {max:F1} s, " +
                      $"máximo visibles a la vez {maxVisibles}, máximo en escena {maxEscena}");
            esperas.Clear(); aciertos = 0; maxVisibles = 0; maxEscena = 0;
            turnoProbado++;
            if (turnoProbado < 3 && c.HaySiguienteTurno)
            {
                c.SiguienteTurno();
                EmpezarTurno(c);
                return;
            }
            Debug.Log(fallas == 0 ? "[PruebaCorriente] RESULTADO: OK" : $"[PruebaCorriente] RESULTADO: {fallas} FALLAS");
            SessionState.SetInt(ClaveFase, 0);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(fallas == 0 ? 0 : 1);
        }

        static string Lista(ARNtCocina[] todos, bool visibles)
        {
            var l = new List<string>();
            foreach (var a in todos)
                if (a.Lugar == LugarARNt.Corriente && a.gameObject.activeInHierarchy == visibles) l.Add(a.CodonQueLee);
            return string.Join(" ", l);
        }
    }
}
