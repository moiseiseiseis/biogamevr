using BioVR.Minijuegos.ExpresionGenetica;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BioVR.Minijuegos.Cocina
{
    // Solo para Escenas/Pruebas/Cocina_M6. Atajos de teclado para probar M6 sin esperar al azar:
    //
    //   1 / 2 / 3  empieza ese turno (el 1 no tiene imprevistos; el 3 trae la mesa giratoria)
    //   H          pone la horquilla ya        G  la golpea (como el gatillo con el rayo)
    //   R          empieza el codón raro       P  pausa / quita la pausa (como el tutorial)
    //
    // Escribe en la consola cada imprevisto con el segundo del turno, para revisar la frecuencia (22 s y luego 20–32 s).
    public class PruebaImprevistos : MonoBehaviour
    {
        [SerializeField] HorquillaARNm horquilla;
        [SerializeField] GestorImprevistos gestor;

        void OnEnable()
        {
            EventosCocina.OnHorquilla += AlHorquilla;
            EventosCocina.OnCodonRaro += AlRaro;
            EventosCocina.OnTurnoIniciado += AlTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnHorquilla -= AlHorquilla;
            EventosCocina.OnCodonRaro -= AlRaro;
            EventosCocina.OnTurnoIniciado -= AlTurno;
        }

        static float Segundo => ControlCocina.Instancia != null ? ControlCocina.Instancia.Marcador.tiempo : 0f;

        void AlTurno(TurnoData t) =>
            Debug.Log($"[M6] Empieza {t.name}: imprevistos {(t.imprevistos ? "sí" : "no")}, mesa giratoria {(t.mesaGira ? "sí" : "no")}.");
        void AlHorquilla(bool aparece) =>
            Debug.Log($"[M6] {Segundo:F1} s · horquilla {(aparece ? "APARECE" : "abierta")}. Siguiente imprevisto en {Siguiente()}.");
        void AlRaro(bool activo) =>
            Debug.Log($"[M6] {Segundo:F1} s · codón raro {(activo ? "EMPIEZA" : "termina")}. Siguiente imprevisto en {Siguiente()}.");

        string Siguiente() => gestor != null ? $"{gestor.Faltan:F1} s" : "?";

        void Update()
        {
            var c = ControlCocina.Instancia;
            var k = Keyboard.current;
            if (c == null || k == null) return;

            // Sirven los números de arriba y los del teclado numérico
            if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) c.IniciarTurno(0);
            if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) c.IniciarTurno(1);
            if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) c.IniciarTurno(2);
            if (k.hKey.wasPressedThisFrame && !c.ActivarHorquilla())
                Debug.Log("[M6] No se puede poner la horquilla ahora (turno sin imprevistos, ya hay una, la mesa terminó o hay pausa).");
            if (k.rKey.wasPressedThisFrame && !c.ActivarCodonRaro())
                Debug.Log("[M6] No se puede empezar el codón raro ahora (turno sin imprevistos o hay pausa).");
            if (k.gKey.wasPressedThisFrame && horquilla != null) horquilla.GetComponent<Senalable>().Senalar();
            if (k.pKey.wasPressedThisFrame) c.PonerPausa(!c.Pausado);
        }
    }
}
