using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Manivela : MonoBehaviour
    {
        [SerializeField] EstacionSintetasa estacion;
        [Tooltip("Objeto visual que gira sobre su eje local Z")]
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

            // Proyectar la posición de la mano en el plano XY local del eje de la manivela
            Vector3 localMano = pivoteGiro.parent != null
                ? pivoteGiro.parent.InverseTransformPoint(manoActual.position) - pivoteGiro.localPosition
                : manoActual.position - pivoteGiro.position;

            // La mano debe estar al menos a RadioMinimoManivela (0.04 m) del eje
            Vector2 planoXY = new Vector2(localMano.x, localMano.y);
            if (planoXY.magnitude < ControlesCocina.RadioMinimoManivela) return;

            float anguloActual = Mathf.Atan2(planoXY.y, planoXY.x) * Mathf.Rad2Deg;

            if (!tieneAnguloPrevio)
            {
                anguloAnterior = anguloActual;
                tieneAnguloPrevio = true;
                return;
            }

            float deltaGrados = Mathf.DeltaAngle(anguloAnterior, anguloActual);
            anguloAnterior = anguloActual;

            if (Mathf.Abs(deltaGrados) < 0.01f) return;

            pivoteGiro.Rotate(Vector3.forward, deltaGrados, Space.Self);

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

