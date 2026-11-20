using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(ARNtCocina))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class DetectorSacudida : MonoBehaviour
    {
        struct CambioDireccion
        {
            public float tiempo;
            public int signo;
        }

        ARNtCocina datos;
        XRGrabInteractable grab;
        IXRSelectInteractor interactorActual;

        Vector3 posAnterior;
        int ultimoSigno;
        readonly List<CambioDireccion> historial = new List<CambioDireccion>();

        void Awake()
        {
            datos = GetComponent<ARNtCocina>();
            grab = GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(AlAgarrar);
                grab.selectExited.AddListener(AlSoltar);
            }
        }

        void AlAgarrar(SelectEnterEventArgs args)
        {
            interactorActual = args.interactorObject;
            posAnterior = transform.position;
            ultimoSigno = 0;
            historial.Clear();
        }

        void AlSoltar(SelectExitEventArgs args)
        {
            interactorActual = null;
            historial.Clear();
        }

        void Update()
        {
            if (datos == null || !datos.Cargado || datos.Lugar != LugarARNt.Mano) return;
            if (ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado) return;

            float dt = Time.deltaTime;
            if (dt <= 0.0001f) return;

            float ahora = Time.time;
            historial.RemoveAll(c => ahora - c.tiempo > ControlesCocina.VentanaSacudida);

            Vector3 desplazamiento = transform.position - posAnterior;
            posAnterior = transform.position;

            Vector3 ejeLateral = Camera.main != null ? Camera.main.transform.right : Vector3.right;
            float velocidadLateral = Vector3.Dot(desplazamiento, ejeLateral) / dt;

            // Usa exactamente los umbrales de ControlesCocina (VelocidadSacudida, VentanaSacudida, CambiosSacudida)
            if (Mathf.Abs(velocidadLateral) >= ControlesCocina.VelocidadSacudida)
            {
                int signoActual = velocidadLateral > 0f ? 1 : -1;
                if (signoActual != ultimoSigno)
                {
                    ultimoSigno = signoActual;
                    historial.Add(new CambioDireccion { tiempo = ahora, signo = signoActual });

                    if (historial.Count >= ControlesCocina.CambiosSacudida)
                    {
                        EjecutarSacudida();
                    }
                }
            }
        }

        void EjecutarSacudida()
        {
            historial.Clear();
            ultimoSigno = 0;

            if (ControlCocina.Instancia != null)
                ControlCocina.Instancia.Sacudir(datos);

            if (interactorActual != null && interactorActual.transform != null)
            {
                var haptics = interactorActual.transform.GetComponentInParent<HapticImpulsePlayer>();
                if (haptics != null)
                    haptics.SendHapticImpulse(0.6f, 0.15f);
            }
        }
    }
}


