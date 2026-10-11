using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Cabina
{
    // Palanca sin física: mientras la agarran, gira sobre su eje X según cuánto se mueve la mano
    // hacia adelante o hacia atrás desde donde la agarró (subir, bajar o mover a los lados no cuenta),
    // limitada entre anguloMin y anguloMax. No puede separarse de la base ni salir volando.
    // Al llegar al tope (anguloMax) dispara alLlegarAlTope una vez; se rearma al regresarla.
    // Al soltarla vuelve sola a anguloMin.
    // Va en el objeto "pivote" junto con un XRSimpleInteractable; los colliders de sus hijos son lo que se agarra.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class Palanca : MonoBehaviour
    {
        public UnityEvent alLlegarAlTope;

        [Tooltip("Ángulo de reposo y ángulo del tope, en grados sobre el eje X (positivo = hacia adelante, +Z).")]
        [SerializeField] float anguloMin = 0f, anguloMax = 45f;

        [Tooltip("Grados antes del tope en los que ya cuenta como 'tope'.")]
        [SerializeField] float margen = 3f;

        [Tooltip("Grados que hay que regresarla desde el tope para poder dispararla otra vez.")]
        [SerializeField] float rearmar = 10f;

        [Tooltip("Distancia (m) del pivote a la mano: cuánto hay que mover la mano para girarla. Más chico = más sensible.")]
        [SerializeField] float radio = 0.25f;

        [Tooltip("Grados por segundo con los que regresa al reposo al soltarla.")]
        [SerializeField] float velocidadRegreso = 120f;

        XRSimpleInteractable interactable;
        IXRSelectInteractor mano;
        float angulo;
        float anguloAlAgarrar, zAlAgarrar;
        bool eventoDisparado;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(e =>
            {
                mano = e.interactorObject;
                anguloAlAgarrar = angulo;
                zAlAgarrar = ZDeLaMano();
            });
            interactable.selectExited.AddListener(_ => mano = null);
            angulo = anguloMin;
        }

        void Update()
        {
            if (mano != null)
            {
                // Solo cuenta lo que avanzó la mano hacia adelante/atrás (eje Z de la base) desde que agarró;
                // ese desplazamiento sobre el radio de la palanca es el giro en radianes
                float avance = ZDeLaMano() - zAlAgarrar;
                angulo = anguloAlAgarrar + avance / radio * Mathf.Rad2Deg;
            }
            else
            {
                angulo = Mathf.MoveTowards(angulo, anguloMin, velocidadRegreso * Time.deltaTime);
            }

            angulo = Mathf.Clamp(angulo, anguloMin, anguloMax);
            transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);

            if (!eventoDisparado && angulo >= anguloMax - margen)
            {
                eventoDisparado = true;
                Debug.Log("[Cabina] La palanca llegó al tope.");
                alLlegarAlTope.Invoke();
            }
            else if (eventoDisparado && angulo < anguloMax - rearmar)
            {
                eventoDisparado = false;
            }
        }

        // Posición de la mano en el eje Z de la base (el padre del pivote), en metros
        float ZDeLaMano()
        {
            var punto = mano.GetAttachTransform(interactable).position;
            var referencia = transform.parent != null ? transform.parent : transform;
            return referencia.InverseTransformPoint(punto).z * referencia.lossyScale.z;
        }
    }
}
