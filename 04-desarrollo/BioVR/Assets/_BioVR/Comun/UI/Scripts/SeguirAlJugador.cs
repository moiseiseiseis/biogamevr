using UnityEngine;

namespace BioVR.Comun
{
    // Mantiene un panel (Canvas en el mundo) frente al jugador, con retraso: no se pega a la cara.
    // Se queda quieto mientras el jugador mira cerca de él; cuando el jugador gira más de "anguloLibre",
    // se desliza suavemente hasta quedar otra vez al frente. Solo sigue el giro horizontal de la cabeza,
    // así el panel no sube ni baja al mirar la mesa.
    //
    // Prueba del 23-nov: "que la ventana del tutorial siga siempre al jugador".
    public class SeguirAlJugador : MonoBehaviour
    {
        [Tooltip("Distancia horizontal a los ojos (m)")]
        public float distancia = 0.9f;
        [Tooltip("Altura respecto a los ojos (m). Positivo = arriba de la mirada")]
        public float altura = 0.08f;
        [Tooltip("Giro respecto al frente de la mirada (grados). Positivo = a la derecha")]
        public float angulo = 0f;
        [Tooltip("Cuánto puede girar el jugador sin que el panel se mueva (grados)")]
        public float anguloLibre = 28f;
        [Tooltip("Qué tan rápido alcanza su lugar (más = más rápido)")]
        public float suavidad = 3f;
        [Tooltip("El panel es un Canvas (se lee con su +Z alejándose del jugador)")]
        public bool esCanvas = true;

        Transform camara;
        float yawObjetivo;
        bool listo, siguiendo;

        void OnEnable() => listo = false;

        void LateUpdate()
        {
            if (camara == null) camara = Camera.main != null ? Camera.main.transform : null;
            if (camara == null) return;

            var adelante = Vector3.ProjectOnPlane(camara.forward, Vector3.up);
            if (adelante.sqrMagnitude < 1e-4f) adelante = Vector3.ProjectOnPlane(camara.up, Vector3.up);
            float yawCabeza = Mathf.Atan2(adelante.x, adelante.z) * Mathf.Rad2Deg + angulo;

            if (!listo) { yawObjetivo = yawCabeza; listo = true; Colocar(1f); return; }

            float diferencia = Mathf.Abs(Mathf.DeltaAngle(yawObjetivo, yawCabeza));
            if (diferencia > anguloLibre) siguiendo = true;
            if (siguiendo)
            {
                yawObjetivo = Mathf.LerpAngle(yawObjetivo, yawCabeza, 1f - Mathf.Exp(-suavidad * Time.unscaledDeltaTime));
                if (diferencia < 2f) siguiendo = false;
            }
            Colocar(1f - Mathf.Exp(-suavidad * 2f * Time.unscaledDeltaTime));
        }

        void Colocar(float t)
        {
            var dir = Quaternion.Euler(0f, yawObjetivo, 0f) * Vector3.forward;
            var destino = camara.position + dir * distancia + Vector3.up * altura;
            transform.position = Vector3.Lerp(transform.position, destino, t);
            var mirar = esCanvas ? transform.position - camara.position : camara.position - transform.position;
            if (mirar.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mirar, Vector3.up), t);
        }

        // Para ponerlo al frente de golpe (por ejemplo, al abrir un paso nuevo con información)
        public void TraerAlFrente() => listo = false;
    }
}
