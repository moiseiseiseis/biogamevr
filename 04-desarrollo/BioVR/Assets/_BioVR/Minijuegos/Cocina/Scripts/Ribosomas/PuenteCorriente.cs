using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.Cocina
{
    // Puente PROVISIONAL entre los ribosomas (M4) y la corriente de ARNt (M2, Genaro).
    // Junta en un solo lugar lo que M4 necesita de M2, para que el cambio sea de un archivo cuando
    // M2 esté en main. Por acordar con Genaro:
    //   - Devolver: cuando un ribosoma rechaza un ARNt, ¿qué función de M2 lo regresa al tubo (u = 0.35)?
    //     Mientras tanto se avisa con OnDevolver (la escena de prueba de M4 lo escucha).
    //   - Soltar / PermitirAgarre: M4 suelta el ARNt de la mano al encajarlo y le apaga el agarre mientras
    //     está en el ribosoma. M2 no debe cancelar el agarre cuando ControlCocina.Agarrar devuelve falso
    //     porque el ARNt está en la llave: ahí el jugador lo agarra para girarlo.
    public static class PuenteCorriente
    {
        // M2 (o la prueba de M4) lo regresa visualmente a la corriente
        public static event Action<ARNtCocina> OnDevolver;

        public static void Devolver(ARNtCocina t)
        {
            if (t == null) return;
            Soltar(t);
            PermitirAgarre(t, true);
            if (ControlCocina.Instancia != null) ControlCocina.Instancia.DevolverACorriente(t);
            OnDevolver?.Invoke(t);
        }

        // Si alguien lo tiene agarrado, lo suelta
        public static void Soltar(ARNtCocina t)
        {
            var agarre = t != null ? t.GetComponent<XRGrabInteractable>() : null;
            if (agarre != null && agarre.isSelected && agarre.interactionManager != null)
                agarre.interactionManager.CancelInteractableSelection((IXRSelectInteractable)agarre);
        }

        public static void PermitirAgarre(ARNtCocina t, bool permitir)
        {
            var agarre = t != null ? t.GetComponent<XRGrabInteractable>() : null;
            if (agarre != null) agarre.enabled = permitir;
        }
    }
}
