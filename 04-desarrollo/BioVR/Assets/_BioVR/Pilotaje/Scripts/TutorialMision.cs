using BioVR.Comun;
using BioVR.Secuencia;
using UnityEngine;

namespace BioVR.Pilotaje
{
    // Juego completo: al llegar a la célula (estado Llegada) abre el tutorial en el primer paso de la misión
    // (el aviso de mareo), aunque el jugador haya saltado el tutorial del hangar. La misión bloquea el salto a la
    // Cocina, así que nadie debe llegar sin saber qué hacer. Lo pone BioVR → Pilotaje → Integrar al juego completo.
    public class TutorialMision : MonoBehaviour
    {
        [SerializeField] PasoTutorial primerPaso;

        GameManager gm;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm != null) gm.OnEstadoCambiado += AlCambiarEstado;
        }

        void OnDestroy()
        {
            if (gm != null) gm.OnEstadoCambiado -= AlCambiarEstado;
        }

        void AlCambiarEstado(GameState estado)
        {
            if (estado == GameState.Llegada) Tutorial.MostrarDesde(primerPaso);
        }
    }
}
