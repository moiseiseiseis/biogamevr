using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

namespace BioVR.Jugador
{
    public class Recentrar : MonoBehaviour
    {
        [Header("Configuración VR")]
        public XROrigin xrOrigin;
        public Transform puntoOjos;

        [Header("Botón de Control (Ej. Botón B/Y)")]
        public InputActionReference botonRecentrar;

        void OnEnable()
        {
            if (botonRecentrar != null)
            {
                botonRecentrar.action.Enable();
                botonRecentrar.action.performed += AlPresionarBoton;
            }
        }

        void OnDisable()
        {
            if (botonRecentrar != null)
            {
                botonRecentrar.action.performed -= AlPresionarBoton;
            }
        }

        private void AlPresionarBoton(InputAction.CallbackContext context)
        {
            if (xrOrigin != null && puntoOjos != null)
            {
                // Regresa la cámara física al punto central de la cabina
                xrOrigin.MoveCameraToWorldLocation(puntoOjos.position);
                // Alinea el frente del visor con el eje frontal (+Z) del juego
                xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
            }
        }
    }
}

