using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Pieza de la mesa de prueba (Mo2): se atrae con el rayo, se sujeta con el grip y se rota con la muñeca
    // o con el stick del mismo mando. La "marca" (cara blanca) es la que debe mirar al frente para encajar
    // en su ranura, igual que después el anticodón del ARNt frente al codón.
    // El stick rota solo el modelo (hijo "Modelo"), no la raíz: la raíz la mueve el agarre de XRI.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class PiezaManipulable : MonoBehaviour
    {
        [Tooltip("Lo que pide la ranura para aceptarla, p. ej. \"cubo\". Después será el anticodón.")]
        public string clave;

        [Tooltip("El modelo que se rota con el stick (con sus colliders).")]
        public Transform modelo;

        [Tooltip("Cara que debe mirar al frente de la ranura (su eje Z azul).")]
        public Transform marca;

        [Tooltip("Grados por segundo con el stick al tope.")]
        [SerializeField] float velocidadGiro = 120f;

        [Tooltip("Si cae por debajo de esta altura (m) regresa a su lugar en la mesa.")]
        [SerializeField] float alturaMinima = 0.2f;

        XRGrabInteractable agarre;
        Rigidbody cuerpo;
        Pose inicio;
        InputAction stickIzquierdo, stickDerecho;
        InputAction stickActivo;

        // En la mano (no en una ranura)
        public bool Sujetada => agarre != null && agarre.isSelected && !(agarre.firstInteractorSelecting is XRSocketInteractor);

        void Awake()
        {
            agarre = GetComponent<XRGrabInteractable>();
            cuerpo = GetComponent<Rigidbody>();
            inicio = new Pose(transform.position, transform.rotation);
            stickIzquierdo = new InputAction("Rotar pieza (izq)", InputActionType.Value, "<XRController>{LeftHand}/{Primary2DAxis}");
            stickDerecho = new InputAction("Rotar pieza (der)", InputActionType.Value, "<XRController>{RightHand}/{Primary2DAxis}");
        }

        void OnEnable()
        {
            stickIzquierdo.Enable();
            stickDerecho.Enable();
            agarre.selectEntered.AddListener(AlSeleccionar);
            agarre.selectExited.AddListener(AlSoltar);
        }

        void OnDisable()
        {
            stickIzquierdo.Disable();
            stickDerecho.Disable();
            agarre.selectEntered.RemoveListener(AlSeleccionar);
            agarre.selectExited.RemoveListener(AlSoltar);
        }

        void OnDestroy()
        {
            stickIzquierdo.Dispose();
            stickDerecho.Dispose();
        }

        void AlSeleccionar(SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor) { stickActivo = null; return; }
            // Solo el stick de la mano que la sujeta
            stickActivo = args.interactorObject.handedness == InteractorHandedness.Left ? stickIzquierdo : stickDerecho;
        }

        void AlSoltar(SelectExitEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor || args.isCanceled) return;
            stickActivo = null;
            // Si la soltó sobre una ranura que no la acepta, la ranura la rechaza (y la regresa)
            foreach (var ranura in RanuraSeleccion.Todas)
                if (ranura.Contiene(this) && ranura.Rechazar(this)) return;
        }

        void Update()
        {
            if (stickActivo != null && Sujetada && modelo != null)
            {
                var stick = stickActivo.ReadValue<Vector2>();
                var cam = Camera.main != null ? Camera.main.transform : transform;
                float paso = velocidadGiro * Time.deltaTime;
                // Izquierda/derecha: gira alrededor del eje vertical. Arriba/abajo: la inclina hacia el jugador.
                modelo.Rotate(Vector3.up, -stick.x * paso, Space.World);
                modelo.Rotate(cam.right, stick.y * paso, Space.World);
            }

            if (!agarre.isSelected && transform.position.y < alturaMinima) Regresar();
        }

        // Ángulo (grados) entre la marca y una dirección; lo usa la ranura para saber si está bien rotada
        public float AnguloMarca(Vector3 direccion) => marca != null ? Vector3.Angle(marca.forward, direccion) : 0f;

        // Al encajar: el modelo vuelve a su rotación original para quedar exactamente alineado con la ranura
        public void Alinear()
        {
            if (modelo != null) modelo.localRotation = Quaternion.identity;
        }

        public void Regresar()
        {
            if (agarre.isSelected) return;
            if (cuerpo != null)
            {
                cuerpo.linearVelocity = Vector3.zero;
                cuerpo.angularVelocity = Vector3.zero;
            }
            transform.SetPositionAndRotation(inicio.position, inicio.rotation);
            if (modelo != null) modelo.localRotation = Quaternion.identity;
        }
    }
}
