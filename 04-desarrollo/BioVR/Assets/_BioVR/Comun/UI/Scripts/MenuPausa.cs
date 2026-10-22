using System;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    // Menú de pausa: se abre y se cierra con el botón ☰ del mando izquierdo (Esc o P en el editor).
    // Al abrirse aparece frente a la vista del jugador y pausa el juego (GameManager.Pausar).
    // Botones: Continuar, Reiniciar (vuelve a cargar la escena desde el Hangar) y Salir.
    // Reiniciar y Salir primero preguntan "¿Estás seguro?" en el panel de confirmación (Sí / No).
    // Lo arma el menú BioVR → Agregar menú de pausa.
    public class MenuPausa : MonoBehaviour
    {
        [Tooltip("Canvas del menú (world space). Empieza apagado.")]
        [SerializeField] GameObject panel;

        [Tooltip("Canvas con la pregunta ¿Estás seguro? y los botones Sí / No. Si se deja vacío, Reiniciar y Salir no preguntan.")]
        [SerializeField] GameObject panelConfirmacion;

        [Tooltip("Cámara del jugador: el menú aparece frente a ella.")]
        [SerializeField] Transform cabeza;

        [Tooltip("Metros entre los ojos y el menú.")]
        [SerializeField] float distancia = 0.8f;

        [Tooltip("Objetos que se apagan mientras el menú está abierto (p. ej. la locomoción).")]
        [SerializeField] GameObject[] apagarEnPausa = new GameObject[0];

        [Tooltip("Por defecto: botón de menú del mando izquierdo (☰) y las teclas Esc y P en el editor.")]
        public InputActionProperty botonMenu = new InputActionProperty(CrearAccionPorDefecto());

        // Avisa cuando el menú se abre (true) o se cierra (false); lo usa el tutorial
        public static event Action<bool> OnMenu;

        // Abierto = se ve el menú o la confirmación
        public bool Abierto => (panel != null && panel.activeSelf) || (panelConfirmacion != null && panelConfirmacion.activeSelf);

        // Qué se hace si el jugador contesta "Sí": "Reiniciar" o "Salir"
        string accionPendiente = "";

        static InputAction CrearAccionPorDefecto()
        {
            var accion = new InputAction("Menu", InputActionType.Button);
            accion.AddBinding("<XRController>{LeftHand}/menuButton"); // el ☰ derecho es del sistema de Meta
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
            if (botonMenu.reference == null) accion.Disable(); // solo apagamos la acción si es nuestra
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

        // Botón "Continuar" (y el mismo botón ☰ con el menú o la confirmación abiertos)
        public void Continuar()
        {
            accionPendiente = "";
            panel.SetActive(false);
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);

            foreach (var go in apagarEnPausa)
                if (go != null) go.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.Pausar(false);
            OnMenu?.Invoke(false);
        }

        // Botón "Reiniciar": pregunta antes; con "Sí" todo vuelve a empezar desde el Hangar
        public void Reiniciar() => Preguntar("Reiniciar");

        // Botón "Salir": pregunta antes; con "Sí" cierra la app en el visor (en el editor detiene el Play)
        public void Salir() => Preguntar("Salir");

        // Botón "Sí" de la confirmación
        public void ConfirmarAccionSi()
        {
            if (accionPendiente == "Reiniciar") EjecutarReiniciar();
            else if (accionPendiente == "Salir") EjecutarSalir();
        }

        // Botón "No" de la confirmación: regresa al menú de pausa
        public void CancelarConfirmacionNo()
        {
            accionPendiente = "";
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
            panel.SetActive(true);
        }

        // Cambia el menú por la confirmación; sin panel de confirmación hace la acción de una vez
        void Preguntar(string accion)
        {
            accionPendiente = accion;
            if (panelConfirmacion == null) { ConfirmarAccionSi(); return; }
            panel.SetActive(false);
            panelConfirmacion.SetActive(true);
        }

        void EjecutarReiniciar()
        {
            if (GameManager.Instance != null) GameManager.Instance.Pausar(false);
            Debug.Log("[Menu] Reiniciar.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void EjecutarSalir()
        {
            Debug.Log("[Menu] Salir.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Pone el menú al frente de la vista, a la altura de los ojos y derecho (sin inclinarse con la cabeza).
        // Se mueve el objeto completo (no solo el panel) para que la confirmación también quede enfrente.
        void Colocar()
        {
            var adelante = Vector3.ProjectOnPlane(cabeza.forward, Vector3.up);
            if (adelante.sqrMagnitude < 0.01f) adelante = Vector3.forward; // mirando justo arriba o abajo
            adelante.Normalize();
            transform.SetPositionAndRotation(cabeza.position + adelante * distancia, Quaternion.LookRotation(adelante));
        }
    }
}
