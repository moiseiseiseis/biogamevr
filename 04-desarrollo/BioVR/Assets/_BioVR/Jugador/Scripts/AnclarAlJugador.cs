using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace BioVR.Jugador
{
    // Acomoda una estación de trabajo (mesa, piezas, botones) a la cabeza del jugador, en vez de mover
    // al jugador a una altura fija. Así funciona igual en el simulador, sentado o de pie, y con cualquier estatura.
    //
    // La estación se arma en el editor como si los ojos del jugador estuvieran en "puntoOjos" (hijo de la estación).
    // Al iniciar, la estación está APAGADA: se espera a que el visor reporte la cabeza, se mueve la estación
    // para que su Punto ojos quede justo en la cabeza y hasta entonces se enciende. Así los scripts de adentro
    // guardan sus posiciones iniciales (Awake) ya en el lugar correcto.
    //
    // Después, para "recentrar", se mueve al jugador (Recentrar, botón B/Y o el botón de la mesa), no la estación.
    public class AnclarAlJugador : MonoBehaviour
    {
        [SerializeField] XROrigin origen;

        [Tooltip("Raíz de la estación. Debe estar apagada en la escena; este script la enciende.")]
        [SerializeField] GameObject estacion;

        [Tooltip("Dónde se armaron los ojos del jugador, dentro de la estación.")]
        [SerializeField] Transform puntoOjos;

        [Tooltip("Si el visor no reporta la cabeza en este tiempo (s), se ancla donde esté la cámara.")]
        [SerializeField] float esperaMaxima = 2f;

        IEnumerator Start()
        {
            if (origen == null || estacion == null || puntoOjos == null)
            {
                Debug.LogError("[Jugador] AnclarAlJugador necesita el XR Origin, la estación y su Punto ojos.", this);
                if (estacion != null) estacion.SetActive(true);
                yield break;
            }

            // El visor tarda unos cuadros en reportar la cabeza; antes de eso la cámara está en el piso
            float limite = Time.time + esperaMaxima;
            while (origen.CameraInOriginSpaceHeight < 0.2f && Time.time < limite) yield return null;
            yield return null; // un cuadro más para que la pose se asiente

            // El visor no siempre arranca mirando al frente: depende de hacia dónde quedó el límite del Quest
            // (en la prueba del 21-nov la mesa quedó a la espalda). Se gira al jugador, sobre su cabeza, para que
            // mire hacia el frente de la estación; luego la estación se traslada a la cabeza.
            var frente = Vector3.ProjectOnPlane(puntoOjos.forward, Vector3.up);
            origen.MatchOriginUpCameraForward(Vector3.up, frente.sqrMagnitude > 1e-4f ? frente.normalized : Vector3.forward);
            var cabeza = origen.Camera.transform.position;
            estacion.transform.position += cabeza - puntoOjos.position;
            Physics.SyncTransforms();
            estacion.SetActive(true);
            Debug.Log($"[Jugador] Estación anclada a la cabeza (ojos a {cabeza.y:0.00} m del piso).");
        }
    }
}
