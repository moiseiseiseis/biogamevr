using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Manivela : MonoBehaviour
    {
        [SerializeField] EstacionSintetasa estacion;
        [Tooltip("Objeto visual que gira (si se deja vacío gira este mismo objeto)")]
        [SerializeField] Transform pivoteGiro;

        XRGrabInteractable grab;
        Transform manoActual;
        Vector3 posInicialManivela;
        float anguloAnterior;
        bool tieneAnguloPrevio;
        float acumuladorVacio;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            if (estacion == null) estacion = GetComponentInParent<EstacionSintetasa>();
            if (pivoteGiro == null) pivoteGiro = transform;

            posInicialManivela = transform.localPosition;

            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.throwOnDetach = false;

            grab.selectEntered.AddListener(AlAgarrar);
            grab.selectExited.AddListener(AlSoltar);
        }

        void AlAgarrar(SelectEnterEventArgs args)
        {
            manoActual = args.interactorObject?.transform;
            tieneAnguloPrevio = false;
        }

        void AlSoltar(SelectExitEventArgs args)
        {
            manoActual = null;
            tieneAnguloPrevio = false;
            transform.localPosition = posInicialManivela;
        }

        void Update()
        {
            if (manoActual == null) return;
            var control = ControlCocina.Instancia;
            if (control != null && control.Pausado) return;

            // Calculamos el punto donde apunta el rayo a la distancia de la manivela (funciona en Quest cerca y con rayo en simulador)
            float dist = Vector3.Distance(manoActual.position, pivoteGiro.position);
            Vector3 puntoPuntero = manoActual.position + manoActual.forward * dist;

            Vector3 localPuntero = pivoteGiro.parent != null
                ? pivoteGiro.parent.InverseTransformPoint(puntoPuntero) - pivoteGiro.localPosition
                : puntoPuntero - pivoteGiro.position;

            // La rueda amarilla de la manivela está en el plano YZ (gira sobre el eje local X)
            Vector2 planoRueda = new Vector2(localPuntero.z, localPuntero.y);
            if (planoRueda.magnitude < 0.005f) return;

            float anguloActual = Mathf.Atan2(planoRueda.y, planoRueda.x) * Mathf.Rad2Deg;

            if (!tieneAnguloPrevio)
            {
                anguloAnterior = anguloActual;
                tieneAnguloPrevio = true;
                return;
            }

            float deltaGrados = Mathf.DeltaAngle(anguloAnterior, anguloActual);
            anguloAnterior = anguloActual;

            if (Mathf.Abs(deltaGrados) < 0.05f) return;

            // Girar visualmente la rueda amarilla sobre su eje local X
            pivoteGiro.Rotate(Vector3.right, -deltaGrados, Space.Self);

            float vueltasRequeridas = (control != null && control.Config.vueltasManivela > 0)
                ? control.Config.vueltasManivela
                : 2f;

            float deltaVueltas = Mathf.Abs(deltaGrados) / 360f;
            float deltaProgreso = deltaVueltas / vueltasRequeridas;

            if (estacion != null)
            {
                if (estacion.TieneARNtVacio)
                {
                    estacion.AvanzarProgreso(deltaProgreso);
                }
                else
                {
                    acumuladorVacio += deltaVueltas;
                    if (acumuladorVacio >= 0.5f)
                    {
                        acumuladorVacio = 0f;
                        if (Random.value < 0.05f)
                            EventosCocina.Mensaje("sint_pon_arnt");
                    }
                }
            }
        }
    }
}


