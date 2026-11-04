using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Un ARNt de la escena (decisión D3): Met, Gly, Phe o el distractor Gln.
    // Llega ya cargado con su aminoácido (ficha 4: lo unió la aminoacil-ARNt sintetasa); el jugador nunca lo une.
    // Se agarra con PiezaManipulable (Mo2). Su "clave" es el codón que lee, para que el sitio de lectura
    // (RanuraSeleccion) solo acepte el ARNt cuyo anticodón encaja con el codón activo.
    [RequireComponent(typeof(PiezaManipulable))]
    public class ARNt : MonoBehaviour
    {
        [Tooltip("El codón que lee este ARNt (Gln lee CAG, que no aparece en ninguna secuencia de la escena).")]
        public CodonData codon;

        [Tooltip("El aminoácido que trae (hijo del modelo). Al encajar pasa a la cadena.")]
        public Transform aminoacido;

        // Avisa cada vez que el jugador lo sujeta con la mano (paso 3 de M4: inspeccionar)
        public event System.Action<ARNt> OnSujetado;

        public PiezaManipulable Pieza { get; private set; }
        public XRGrabInteractable Agarre { get; private set; }
        public bool Cargado => aminoacido != null && aminoacido.parent == padreAminoacido;

        Transform padreAminoacido;
        Vector3 posicionAminoacido;
        Quaternion rotacionAminoacido;
        Vector3 escalaAminoacido;

        void Awake()
        {
            Pieza = GetComponent<PiezaManipulable>();
            Agarre = GetComponent<XRGrabInteractable>();
            if (aminoacido != null)
            {
                padreAminoacido = aminoacido.parent;
                posicionAminoacido = aminoacido.localPosition;
                rotacionAminoacido = aminoacido.localRotation;
                escalaAminoacido = aminoacido.localScale;
            }
        }

        void OnEnable() => Agarre.selectEntered.AddListener(AlSujetar);
        void OnDisable() => Agarre.selectEntered.RemoveListener(AlSujetar);

        void AlSujetar(SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor) return;
            OnSujetado?.Invoke(this);
        }

        // Para volver a empezar: recupera su aminoácido (si lo había dejado en la cadena) y regresa a su lugar
        public void Recargar()
        {
            gameObject.SetActive(true);
            if (aminoacido != null)
            {
                aminoacido.SetParent(padreAminoacido, false);
                aminoacido.localPosition = posicionAminoacido;
                aminoacido.localRotation = rotacionAminoacido;
                aminoacido.localScale = escalaAminoacido;
                aminoacido.gameObject.SetActive(true);
            }
            Pieza.Regresar();
        }
    }
}
