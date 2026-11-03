using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Ranura de la mesa de prueba (Mo2): "seleccionar" = soltar en ella la pieza correcta y bien rotada.
    // Usa el XRSocketInteractor de XRI con este componente como filtro: solo deja entrar la pieza con su
    // clave y con la marca mirando al frente. Amarilla = la pieza correcta está encima y bien girada (suéltala);
    // verde = encajó; roja un momento = se soltó una que no va (y regresa a la mesa). Después será el sitio A del ribosoma con el codón.
    [RequireComponent(typeof(XRSocketInteractor))]
    public class RanuraSeleccion : MonoBehaviour, IXRHoverFilter, IXRSelectFilter
    {
        public static readonly List<RanuraSeleccion> Todas = new List<RanuraSeleccion>();

        [Tooltip("Clave de la pieza que acepta, p. ej. \"cubo\".")]
        public string clave;

        [Tooltip("Cuántos grados puede estar girada la marca respecto al frente de la ranura (eje Z azul).")]
        [SerializeField] float tolerancia = 40f;

        [Tooltip("Lo que cambia de color: verde al acertar, rojo un momento al fallar.")]
        [SerializeField] Renderer indicador;

        [SerializeField] Color colorListo = new Color(0.95f, 0.8f, 0.2f);
        [SerializeField] Color colorAcierto = new Color(0.2f, 0.85f, 0.35f);
        [SerializeField] Color colorError = new Color(0.9f, 0.2f, 0.2f);

        // Avisos para MesaPrueba
        public event System.Action<RanuraSeleccion> OnAcierto;
        public event System.Action<RanuraSeleccion, string> OnError;

        public bool Llena => socket != null && socket.hasSelection;

        XRSocketInteractor socket;
        readonly HashSet<PiezaManipulable> dentro = new HashSet<PiezaManipulable>();
        MaterialPropertyBlock bloque;
        Color colorBase = Color.gray;

        void Awake()
        {
            socket = GetComponent<XRSocketInteractor>();
            bloque = new MaterialPropertyBlock();
            if (indicador != null && indicador.sharedMaterial.HasProperty("_BaseColor"))
                colorBase = indicador.sharedMaterial.GetColor("_BaseColor");
        }

        void OnEnable()
        {
            Todas.Add(this);
            socket.hoverFilters.Add(this);
            socket.selectFilters.Add(this);
            socket.selectEntered.AddListener(AlEncajar);
            socket.selectExited.AddListener(AlSacar);
            socket.hoverEntered.AddListener(AlAcercar);
            socket.hoverExited.AddListener(AlAlejar);
        }

        void OnDisable()
        {
            Todas.Remove(this);
            socket.hoverFilters.Remove(this);
            socket.selectFilters.Remove(this);
            socket.selectEntered.RemoveListener(AlEncajar);
            socket.selectExited.RemoveListener(AlSacar);
            socket.hoverEntered.RemoveListener(AlAcercar);
            socket.hoverExited.RemoveListener(AlAlejar);
        }

        // ---- Filtro de XRI: ¿esta pieza puede entrar? ----
        public bool canProcess => isActiveAndEnabled;
        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable) => Acepta(interactable, out _);
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) => Acepta(interactable, out _);

        bool Acepta(IXRInteractable interactable, out string motivo)
        {
            var pieza = interactable.transform.GetComponent<PiezaManipulable>();
            if (pieza == null || pieza.clave != clave) { motivo = "Esa pieza no va en esta ranura."; return false; }
            if (pieza.AnguloMarca(transform.forward) > tolerancia)
            {
                motivo = "Casi: gírala para que la cara blanca mire hacia ti.";
                return false;
            }
            motivo = null;
            return true;
        }

        // ---- Qué piezas están encima (para saber cuál se soltó aquí) ----
        void OnTriggerEnter(Collider otro)
        {
            var pieza = otro.GetComponentInParent<PiezaManipulable>();
            if (pieza != null) dentro.Add(pieza);
        }

        void OnTriggerExit(Collider otro)
        {
            var pieza = otro.GetComponentInParent<PiezaManipulable>();
            if (pieza != null) dentro.Remove(pieza);
        }

        public bool Contiene(PiezaManipulable pieza) => dentro.Contains(pieza);

        // La llama la pieza al soltarse encima. Devuelve true si la rechazó.
        public bool Rechazar(PiezaManipulable pieza)
        {
            if (Llena || Acepta(pieza.GetComponent<XRGrabInteractable>(), out var motivo)) return false;
            pieza.Regresar();
            dentro.Remove(pieza);
            StopAllCoroutines();
            StartCoroutine(Parpadear());
            OnError?.Invoke(this, motivo);
            return true;
        }

        void AlEncajar(SelectEnterEventArgs args)
        {
            args.interactableObject.transform.GetComponent<PiezaManipulable>()?.Alinear();
            StopAllCoroutines();
            Pintar(colorAcierto);
            OnAcierto?.Invoke(this);
        }

        void AlSacar(SelectExitEventArgs args) => Pintar(colorBase);

        // El filtro solo deja pasar el hover de la pieza correcta y bien girada
        void AlAcercar(HoverEnterEventArgs args) { if (!Llena) Pintar(colorListo); }
        void AlAlejar(HoverExitEventArgs args) { if (!Llena) Pintar(colorBase); }

        // Para Reiniciar: suelta la pieza encajada
        public void Vaciar()
        {
            if (Llena) socket.interactionManager.SelectExit(socket, socket.firstInteractableSelected);
            dentro.Clear();
            Pintar(colorBase);
        }

        IEnumerator Parpadear()
        {
            Pintar(colorError);
            yield return new WaitForSeconds(0.6f);
            Pintar(Llena ? colorAcierto : colorBase);
        }

        void Pintar(Color color)
        {
            if (indicador == null) return;
            bloque.SetColor("_BaseColor", color);
            indicador.SetPropertyBlock(bloque);
        }
    }
}
