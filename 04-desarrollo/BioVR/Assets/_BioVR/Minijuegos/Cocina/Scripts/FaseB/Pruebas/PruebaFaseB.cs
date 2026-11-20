using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BioVR.Minijuegos.Cocina
{
    // SOLO PARA Cocina_M10. Lleva el turno con mutación hasta la fase B sin jugarlo, con la misma secuencia
    // que PruebasNucleo (ARNt invisibles que se cargan, entran y se reciclan solos), para probar el
    // holograma, los depósitos, la BiP y el proteasoma.
    //
    // Teclas (editor y development build):
    //   B = turno con mutación completo hasta la búsqueda   N = tocar la cuenta 20 (fase Decide)
    //   C = chaperona   P = proteasoma
    public class PruebaFaseB : MonoBehaviour
    {
        readonly List<GameObject> falsos = new List<GameObject>();

        static ControlCocina Control => ControlCocina.Instancia;

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var k = Keyboard.current;
            if (k == null || Control == null) return;
            if (k.bKey.wasPressedThisFrame) AdelantarABusca();
            if (k.nKey.wasPressedThisFrame && Control.Turno != null) Control.TocarCuenta(Control.Turno.CodonMutacion);
            if (k.cKey.wasPressedThisFrame) Control.UsarChaperona();
            if (k.pKey.wasPressedThisFrame) Control.UsarProteasoma();
#endif
        }

        void AdelantarABusca()
        {
            int turno = -1;
            for (int i = 0; i < Control.Turnos.Count; i++) if (Control.Turnos[i].TieneMutacion) turno = i;
            if (turno < 0) { Debug.LogWarning("[M10 prueba] Ningún turno tiene mutación."); return; }

            Limpiar();
            Control.IniciarTurno(turno);
            int vueltas = 0;
            while (!Control.Ribosomas.All(r => Control.EnParo(r)) && vueltas++ < 2000)
            {
                foreach (var r in Control.Ribosomas)
                {
                    var pide = Control.PideAhora(r);
                    if (pide == null) continue;
                    var t = ARNtFalso(pide.Value.tripleta);
                    Control.TerminarCarga(t, forzarError: false);
                    if (Control.IntentarColocarEnRibosoma(t, r, out _) == ResultadoColocacion.Llave) Control.Aceptar(t, r);
                }
                Control.Paso(0.2f);
                foreach (var usado in Control.Salida.ToList()) { Control.Agarrar(usado); Control.Reciclar(usado); }
            }
            foreach (var r in Control.Ribosomas) Control.TocarFin(r);
            Debug.Log($"[M10 prueba] Turno {turno + 1} armado en {vueltas} vueltas. En {Control.Config.esperaPlegado} s empieza la búsqueda: N toca la cuenta {Control.Turno.CodonMutacion}.");
        }

        ARNtCocina ARNtFalso(string codon)
        {
            var go = new GameObject($"ARNt falso {codon}") { hideFlags = HideFlags.HideInHierarchy };
            go.transform.SetParent(transform, false);
            falsos.Add(go);
            var t = go.AddComponent<ARNtCocina>();
            t.Configurar(codon, Control.Codigo.Traducir(codon));
            return t;
        }

        void Limpiar()
        {
            foreach (var go in falsos) if (go != null) Destroy(go);
            falsos.Clear();
        }

        void OnDestroy() => Limpiar();
    }
}
