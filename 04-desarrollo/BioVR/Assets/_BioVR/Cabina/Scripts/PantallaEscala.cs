using UnityEngine;
using TMPro;
using BioVR.Escala;

namespace BioVR.Cabina
{
    // Monitor de reducción de la cabina: el nivel de escala actual, p. ej. "Vasos sanguíneos · nave de 3 µm".
    public class PantallaEscala : MonoBehaviour
    {
        public TextMeshProUGUI textoPantalla;

        void Start()
        {
            if (textoPantalla == null)
            {
                Debug.LogError("[Cabina] PantallaEscala no tiene asignado el texto.", this);
                enabled = false;
                return;
            }

            // 1. Suscribirse para reaccionar a los saltos
            GestorEscala.OnCambioEscala += ActualizarTexto;

            // 2. Leer la escala inicial (por si el evento ya pasó antes de que cargara la pantalla)
            var gestor = FindFirstObjectByType<GestorEscala>();
            if (gestor != null && gestor.EscalaActual != null)
                ActualizarTexto(gestor.EscalaActual);
        }

        void OnDestroy()
        {
            // Desuscribirse
            GestorEscala.OnCambioEscala -= ActualizarTexto;
        }

        private void ActualizarTexto(NivelEscalaData datos)
        {
            textoPantalla.text = $"{datos.nombreNivel} · nave de {datos.tamanoMecha}";
        }
    }
}
