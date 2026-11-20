using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · La zona del sitio A del ribosoma de mesa: si el jugador suelta el grip con un ARNt dentro,
    // se lo entrega al ribosoma (que le pregunta a M1). Es el "socket" de la especificación 1.4, pero sin
    // XRSocketInteractor: un socket se quedaría con el ARNt y no dejaría agarrarlo para girarlo como llave.
    [RequireComponent(typeof(Collider))]
    public class ZonaSitioA : MonoBehaviour
    {
        [SerializeField] RibosomaCocina ribosoma;

        // ARNt dentro de la zona y la función que escucha cuándo lo suelta
        readonly Dictionary<ARNtCocina, UnityAction<SelectExitEventArgs>> dentro =
            new Dictionary<ARNtCocina, UnityAction<SelectExitEventArgs>>();

        void OnTriggerEnter(Collider otro)
        {
            var t = otro.GetComponentInParent<ARNtCocina>();
            if (t == null || dentro.ContainsKey(t)) return;
            var agarre = t.GetComponent<XRGrabInteractable>();
            if (agarre == null) return;
            UnityAction<SelectExitEventArgs> alSoltar = args => AlSoltar(t, args);
            agarre.selectExited.AddListener(alSoltar);
            dentro[t] = alSoltar;
        }

        void OnTriggerExit(Collider otro)
        {
            var t = otro.GetComponentInParent<ARNtCocina>();
            if (t != null) Quitar(t);
        }

        void OnDisable()
        {
            foreach (var t in new List<ARNtCocina>(dentro.Keys)) Quitar(t);
        }

        void Quitar(ARNtCocina t)
        {
            if (!dentro.TryGetValue(t, out var alSoltar)) return;
            dentro.Remove(t);
            if (t == null) return;
            var agarre = t.GetComponent<XRGrabInteractable>();
            if (agarre != null) agarre.selectExited.RemoveListener(alSoltar);
        }

        void AlSoltar(ARNtCocina t, SelectExitEventArgs args)
        {
            if (args.isCanceled || args.interactorObject is XRSocketInteractor) return;
            // Solo un ARNt que venía en la mano (no el que ya está girando en la llave o dentro del ribosoma)
            if (t.Lugar == LugarARNt.Llave || t.Lugar == LugarARNt.Ribosoma || t.Lugar == LugarARNt.Vuelo) return;
            ribosoma.Recibir(t);
        }
    }
}
