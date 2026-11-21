using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BioVR.Comun
{
    // Sonido de un botón de panel (menú de pausa, tutorial): un tic al pasar el rayo y un toque al presionarlo.
    // SonidosEscena se lo agrega solo a cada Button de la escena al empezar; no hace falta ponerlo a mano.
    [RequireComponent(typeof(Button))]
    public class SonidoBotonUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        Button boton;

        void Awake() => boton = GetComponent<Button>();

        public void OnPointerEnter(PointerEventData _)
        {
            if (boton.interactable && SonidosEscena.Instancia != null) SonidosEscena.Instancia.Hover();
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (boton.interactable && SonidosEscena.Instancia != null) SonidosEscena.Instancia.Clic();
        }
    }
}
