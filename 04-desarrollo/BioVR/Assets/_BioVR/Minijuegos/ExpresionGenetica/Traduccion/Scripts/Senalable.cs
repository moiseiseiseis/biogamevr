using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Algo que el jugador "señala": apunta con el rayo y aprieta el gatillo (decisión D6).
    // Lo usan el ribosoma, la cinta de ARNm y los codones (AUG y el codón de paro, filas 3 y 9 de I1).
    // También acepta el grip, para que nadie se atore si aprieta el botón equivocado.
    // Solo responde mientras está Habilitado; así la secuencia decide qué se puede señalar en cada paso.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class Senalable : MonoBehaviour
    {
        public event System.Action<Senalable> OnSenalado;

        static InputAction gatilloIzquierdo, gatilloDerecho;

        XRSimpleInteractable interactable;
        readonly List<IXRHoverInteractor> apuntando = new List<IXRHoverInteractor>();
        bool habilitado;

        public bool Habilitado
        {
            get => habilitado;
            set
            {
                habilitado = value;
                if (interactable != null) interactable.enabled = value;
                if (!value) apuntando.Clear();
            }
        }

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (gatilloIzquierdo == null)
            {
                gatilloIzquierdo = new InputAction("Señalar (izq)", InputActionType.Button, "<XRController>{LeftHand}/{TriggerButton}");
                gatilloDerecho = new InputAction("Señalar (der)", InputActionType.Button, "<XRController>{RightHand}/{TriggerButton}");
            }
            gatilloIzquierdo.Enable();
            gatilloDerecho.Enable();
            interactable.enabled = habilitado;
        }

        void OnEnable()
        {
            interactable.hoverEntered.AddListener(AlApuntar);
            interactable.hoverExited.AddListener(AlDejarDeApuntar);
            interactable.selectEntered.AddListener(AlSeleccionar);
        }

        void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(AlApuntar);
            interactable.hoverExited.RemoveListener(AlDejarDeApuntar);
            interactable.selectEntered.RemoveListener(AlSeleccionar);
            apuntando.Clear();
        }

        void AlApuntar(HoverEnterEventArgs args) => apuntando.Add(args.interactorObject);
        void AlDejarDeApuntar(HoverExitEventArgs args) => apuntando.Remove(args.interactorObject);
        void AlSeleccionar(SelectEnterEventArgs args) => Senalar();

        void Update()
        {
            if (!habilitado) return;
            foreach (var mano in apuntando)
            {
                var gatillo = mano.handedness == InteractorHandedness.Left ? gatilloIzquierdo : gatilloDerecho;
                if (gatillo.WasPressedThisFrame()) { Senalar(); return; }
            }
        }

        public void Senalar()
        {
            if (!habilitado) return;
            OnSenalado?.Invoke(this);
        }
    }
}
