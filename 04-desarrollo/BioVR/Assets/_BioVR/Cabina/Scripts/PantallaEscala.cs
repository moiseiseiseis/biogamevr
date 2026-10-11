using UnityEngine;
using TMPro;
using BioVR.Escala;

namespace BioVR.Cabina
{
    // Letrero de la cabina que muestra el nivel de escala actual, p. ej. "Vasos sanguíneos · mecha ~1-5 mm".
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
            // tamanoMecha ya trae su "~" (p. ej. "~1-5 mm")
            textoPantalla.text = $"{datos.nombreNivel} · mecha {datos.tamanoMecha}";
        }
    }
}
