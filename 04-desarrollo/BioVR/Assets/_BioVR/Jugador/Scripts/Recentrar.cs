using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

namespace BioVR.Jugador
{
    // Regresa la vista al asiento del piloto mirando al frente (+Z) al presionar B o Y.
    public class Recentrar : MonoBehaviour
    {
        [Header("Configuración VR")]
        public XROrigin xrOrigin;
        public Transform puntoOjos;

        [Header("Botón de Control (B / Y)")]
        [Tooltip("Por defecto: botón secundario de cualquier mando (B o Y) y la tecla Inicio (Home) en el editor. " +
                 "Ojo: las acciones del XR Device Simulator solo funcionan en el editor, no en el visor.")]
        public InputActionProperty botonRecentrar = new InputActionProperty(CrearAccionPorDefecto());

        static InputAction CrearAccionPorDefecto()
        {
            var accion = new InputAction("Recentrar", InputActionType.Button);
            accion.AddBinding("<XRController>{RightHand}/secondaryButton");
            accion.AddBinding("<XRController>{LeftHand}/secondaryButton");
            accion.AddBinding("<Keyboard>/home"); // R ya la usa el simulador para rotar
            return accion;
        }

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
            if (xrOrigin != null && puntoOjos != null)
            {
                // Regresa la cámara física al punto central de la cabina
                xrOrigin.MoveCameraToWorldLocation(puntoOjos.position);
                // Alinea el frente del visor con el eje frontal (+Z) del juego
                xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
                Debug.Log("[Jugador] Vista recentrada.");
            }
        }
    }
}
