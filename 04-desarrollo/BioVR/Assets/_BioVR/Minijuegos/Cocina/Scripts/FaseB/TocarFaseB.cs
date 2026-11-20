using BioVR.Minijuegos.ExpresionGenetica;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public enum AccionFaseB { Chaperona, Proteasoma }

    // M10 · Al tocar la BiP o el proteasoma (rayo + gatillo, o la punta de la pinza) se avisa a M1.
    // M1 decide si se puede (una sola chaperona por turno, solo en la fase Decide), suma los puntos y lanza
    // OnChaperona / OnProteasoma; las animaciones escuchan esos eventos.
    [RequireComponent(typeof(Senalable))]
    public class TocarFaseB : MonoBehaviour
    {
        [SerializeField] AccionFaseB accion;

        Senalable senalable;

        void Awake() => senalable = GetComponent<Senalable>();
        void OnEnable() => senalable.OnSenalado += AlTocar;
        void OnDisable() => senalable.OnSenalado -= AlTocar;

        void AlTocar(Senalable _)
        {
            var control = ControlCocina.Instancia;
            if (control == null) return;
            if (accion == AccionFaseB.Chaperona) control.UsarChaperona();
            else control.UsarProteasoma();
        }
    }
}
