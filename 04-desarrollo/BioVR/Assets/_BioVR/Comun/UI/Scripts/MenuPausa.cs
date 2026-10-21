using System;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    public class MenuPausa : MonoBehaviour
    {
        [Tooltip("Canvas principal del menú de pausa.")]
        [SerializeField] GameObject panel;

        [Tooltip("Sub-panel flotante que contiene el texto de confirmación ¿Seguro? y los botones Sí/No.")]
        [SerializeField] GameObject panelConfirmacion;

        [Tooltip("Cámara del jugador: el panel aparece frente a ella.")]
        [SerializeField] Transform cabeza;

        [Tooltip("Metros entre los ojos y el panel.")]
        [SerializeField] float distancia = 0.8f;

        [Tooltip("Objetos que se apagan mientras el menú está abierto.")]
        [SerializeField] GameObject[] apagarEnPausa = new GameObject[0];

        public InputActionProperty botonMenu = new InputActionProperty(CrearAccionPorDefecto());

        public static event Action<bool> OnMenu;

        public bool Abierto => (panel != null && panel.activeSelf) || (panelConfirmacion != null && panelConfirmacion.activeSelf);

        private string accionPendiente = "";

        static InputAction CrearAccionPorDefecto()
        {
            var accion = new InputAction("Menu", InputActionType.Button);
            accion.AddBinding("<XRController>{LeftHand}/menuButton");
            accion.AddBinding("<Keyboard>/escape");
            accion.AddBinding("<Keyboard>/p");
            return accion;
        }

        void Start()
        {
            if (panel == null || cabeza == null)
            {
                Debug.LogError("[Menu] MenuPausa necesita el panel y la cámara del jugador.", this);
                enabled = false;
                return;
            }
            panel.SetActive(false);
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        }

        void OnEnable()
        {
            var accion = botonMenu.action;
            if (accion == null) return;
            accion.performed += AlPresionarBoton;
            accion.Enable();
        }

        void OnDisable()
        {
            var accion = botonMenu.action;
            if (accion == null) return;
            accion.performed -= AlPresionarBoton;
            if (botonMenu.reference == null) accion.Disable();
        }

        void AlPresionarBoton(InputAction.CallbackContext context)
        {
            if (Abierto) Continuar();
            else Abrir();
        }

        public void Abrir()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.EnTransicion) { Debug.Log("[Menu] No se abre durante un fundido o un salto."); return; }

            Colocar();
            panel.SetActive(true);
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);

            foreach (var go in apagarEnPausa)
                if (go != null) go.SetActive(false);
            if (gm != null) gm.Pausar(true);
            OnMenu?.Invoke(true);
        }

        public void Continuar()
        {
            if (panel != null) panel.SetActive(false);
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);

            foreach (var go in apagarEnPausa)
                if (go != null) go.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.Pausar(false);
            OnMenu?.Invoke(false);
        }

        public void Reiniciar()
        {
            if (panelConfirmacion == null)
            {
                EjecutarReiniciar();
                return;
            }
            accionPendiente = "Reiniciar";
            if (panel != null) panel.SetActive(false); // Oculta el menú viejo por completo
            panelConfirmacion.SetActive(true);         // Muestra tu letrero de ¿Seguro?
        }

        public void Salir()
        {
            if (panelConfirmacion == null)
            {
                EjecutarSalir();
                return;
            }
            accionPendiente = "Salir";
            if (panel != null) panel.SetActive(false); // Oculta el menú viejo por completo
            panelConfirmacion.SetActive(true);         // Muestra tu letrero de ¿Seguro?
        }

        public void ConfirmarAccionSi()
        {
            if (accionPendiente == "Reiniciar") EjecutarReiniciar();
            else if (accionPendiente == "Salir") EjecutarSalir();
        }

        public void CancelarConfirmacionNo()
        {
            accionPendiente = "";
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
            if (panel != null) panel.SetActive(true); // Regresa limpiamente al menú de pausa original
        }

        private void EjecutarReiniciar()
        {
            if (GameManager.Instance != null) GameManager.Instance.Pausar(false);
            Debug.Log("[Menu] Reiniciar confirmado.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void EjecutarSalir()
        {
            Debug.Log("[Menu] Salir confirmado.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Colocar()
        {
            var adelante = Vector3.ProjectOnPlane(cabeza.forward, Vector3.up);
            if (adelante.sqrMagnitude < 0.01f) adelante = Vector3.forward;
            adelante.Normalize();

            // Coloca el contenedor principal de forma correcta
            transform.SetPositionAndRotation(cabeza.position + adelante * distancia, Quaternion.LookRotation(adelante));
        }
    }
}



