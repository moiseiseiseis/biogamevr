using System;
using BioVR.Minijuegos.Cocina;
using BioVR.Secuencia;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BioVR.Comun
{
    // Menú de pausa: se abre y se cierra con el botón ☰ del mando izquierdo (Esc o P en el editor).
    // Al abrirse aparece frente a la vista del jugador y pausa el juego (GameManager.Pausar y, en la Cocina,
    // ControlCocina.PonerPausa).
    // Botones: Continuar, Reiniciar (vuelve a cargar la escena desde el Hangar), Reiniciar turno y Reiniciar
    // tutorial (solo hacen algo en la Cocina) y Salir. Los que reinician o salen preguntan "¿Estás seguro?"
    // en el panel de confirmación (Sí / No).
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

        // Qué se hace si el jugador contesta "Sí": "Reiniciar", "ReiniciarTurno", "ReiniciarTutorial" o "Salir"
        string accionPendiente = "";

        // Si la Cocina ya estaba en pausa al abrir (una ventana del tutorial), al continuar se queda en pausa
        bool cocinaYaPausada;

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
            var cocina = ControlCocina.Instancia; // en la Cocina no hay GameManager
            if (cocina != null)
            {
                cocinaYaPausada = cocina.Pausado;
                cocina.PonerPausa(true);
            }
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
            if (ControlCocina.Instancia != null && !cocinaYaPausada) ControlCocina.Instancia.PonerPausa(false);
            OnMenu?.Invoke(false);
        }

        // Botón "Reiniciar tutorial" (A7): borra las ventanas vistas y reinicia desde el turno 1 (índice 0)
        public void ReiniciarTutorial() => Preguntar("ReiniciarTutorial");

        // Botón "Reiniciar turno" (A7): repite el turno actual sin borrar el historial del tutorial
        public void ReiniciarTurno() => Preguntar("ReiniciarTurno");

        // Botón "Reiniciar": pregunta antes; con "Sí" todo vuelve a empezar desde el Hangar
        public void Reiniciar() => Preguntar("Reiniciar");

        // Botón "Salir": pregunta antes; con "Sí" cierra la app en el visor (en el editor detiene el Play)
        public void Salir() => Preguntar("Salir");

        // Botón "Sí" de la confirmación
        public void ConfirmarAccionSi()
        {
            if (accionPendiente == "Reiniciar") EjecutarReiniciar();
            else if (accionPendiente == "ReiniciarTutorial") EjecutarReiniciarTutorial();
            else if (accionPendiente == "ReiniciarTurno") EjecutarReiniciarTurno();
            else if (accionPendiente == "Salir") EjecutarSalir();
        }

        public void CancelarConfirmacionNo()
        {
            accionPendiente = "";
            if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
            panel.SetActive(true);
        }

        void Preguntar(string accion)
        {
            accionPendiente = accion;
            if (panelConfirmacion == null) { ConfirmarAccionSi(); return; }
            panel.SetActive(false);
            panelConfirmacion.SetActive(true);
        }

        void EjecutarReiniciarTutorial()
        {
            Continuar();
            TutorialCocina.ReiniciarHistorial();
            var cocina = ControlCocina.Instancia;
            if (cocina != null)
            {
                cocina.PonerPausa(false);
                cocina.IniciarTurno(0);
            }
            Debug.Log("[Menu] Reiniciar tutorial.");
        }

        void EjecutarReiniciarTurno()
        {
            Continuar();
            var cocina = ControlCocina.Instancia;
            if (cocina != null)
            {
                cocina.PonerPausa(false);
                cocina.RepetirTurno();
            }
            Debug.Log("[Menu] Reiniciar turno.");
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

        void Colocar()
        {
            var adelante = Vector3.ProjectOnPlane(cabeza.forward, Vector3.up);
            if (adelante.sqrMagnitude < 0.01f) adelante = Vector3.forward;
            adelante.Normalize();
            transform.SetPositionAndRotation(cabeza.position + adelante * distancia, Quaternion.LookRotation(adelante));
        }
    }
}