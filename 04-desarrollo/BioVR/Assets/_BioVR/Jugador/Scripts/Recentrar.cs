using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using BioVR.Secuencia;

namespace BioVR.Jugador
{
    // Regresa la vista al asiento del piloto mirando al frente (+Z) al presionar B o Y.
    // También lo sienta de nuevo en cada cambio de estado (siempre ocurre bajo un fundido), por si
    // caminó por la cabina o salió al hangar. Escotilla usa LlevarA para sacarlo de la cabina.
    public class Recentrar : MonoBehaviour
    {
        [Header("Configuración VR")]
        public XROrigin xrOrigin;
        public Transform puntoOjos;

        [Header("Botón de Control (B / Y)")]
        [Tooltip("Por defecto: botón secundario de cualquier mando (B o Y) y la tecla Inicio (Home) en el editor. " +
                 "Ojo: las acciones del XR Device Simulator solo funcionan en el editor, no en el visor.")]
        public InputActionProperty botonRecentrar = new InputActionProperty(CrearAccionPorDefecto());

        // Avisa cuando el jugador presiona B / Y; lo usa el tutorial
        public static event System.Action OnRecentrado;

        static InputAction CrearAccionPorDefecto()
        {
            var accion = new InputAction("Recentrar", InputActionType.Button);
            accion.AddBinding("<XRController>{RightHand}/secondaryButton");
            accion.AddBinding("<XRController>{LeftHand}/secondaryButton");
            accion.AddBinding("<Keyboard>/home"); // R ya la usa el simulador para rotar
            return accion;
        }

        GameManager gm;

        System.Collections.IEnumerator Start()
        {
            gm = GameManager.Instance;
            if (gm != null) gm.OnEstadoCambiado += AlCambiarEstado;

            // Al abrir, sentarlo en el asiento mirando al frente: el visor arranca mirando hacia donde quedó el
            // límite del Quest (en la prueba del 21-nov, hacia la puerta del hangar). Donde hay una estación que
            // se ancla a la cabeza (la Cocina), de eso se encarga AnclarAlJugador.
            if (FindFirstObjectByType<AnclarAlJugador>() != null || xrOrigin == null) yield break;
            float limite = Time.time + 2f;
            while (xrOrigin.CameraInOriginSpaceHeight < 0.2f && Time.time < limite) yield return null; // el visor tarda en reportar la cabeza
            yield return null;
            Centrar();
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
        }

        void AlCambiarEstado(GameState estado) => Centrar();

        void OnEnable()
        {
            var accion = botonRecentrar.action;
            if (accion == null) return;
            accion.performed += AlPresionarBoton;
            accion.Enable();
        }

        void OnDisable()
        {
            var accion = botonRecentrar.action;
            if (accion == null) return;
            accion.performed -= AlPresionarBoton;
            if (botonRecentrar.reference == null) accion.Disable(); // solo apagamos la acción si es nuestra
        }

        private void AlPresionarBoton(InputAction.CallbackContext context)
        {
            if (gm != null && gm.Pausado) return; // con el menú abierto no se mueve al jugador
            Centrar();
            Debug.Log("[Jugador] Vista recentrada.");
            OnRecentrado?.Invoke();
        }

        // Al asiento del piloto, mirando al frente (+Z)
        public void Centrar()
        {
            if (puntoOjos != null) LlevarA(puntoOjos.position, Vector3.forward);
        }

        // Pone los ojos del jugador sobre "lugar" (a la altura del Punto ojos) mirando hacia "adelante"
        public void LlevarA(Vector3 lugar, Vector3 adelante)
        {
            if (xrOrigin == null || puntoOjos == null) return;
            xrOrigin.MoveCameraToWorldLocation(new Vector3(lugar.x, puntoOjos.position.y, lugar.z));
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, adelante); // gira alrededor de la cámara
            Physics.SyncTransforms(); // el CharacterController del XR Origin debe enterarse del cambio
        }
    }
}
