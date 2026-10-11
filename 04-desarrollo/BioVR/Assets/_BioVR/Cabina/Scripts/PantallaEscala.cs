using UnityEngine;
using TMPro;
using BioVR.Escala;

//Namespace
namespace BioVR.Cabina
{
    public class PantallaEscala : MonoBehaviour
    {
        public TextMeshProUGUI textoPantalla;

        void Start()
        {
            // 1. Dale al like y suscribete para reaccionar a futuros saltos
            GestorEscala.OnCambioEscala += ActualizarTexto;

            // 2. Leer la escala inicial (por si el evento ya pasó antes de que cargara la pantalla)
            // Nota: Descomentar las siguientes líneas cuando el script GestorEscala exista en el proyecto
            /*
            var gestor = FindFirstObjectByType<GestorEscala>();
            if (gestor != null && gestor.EscalaActual != null)
            {
                ActualizarTexto(gestor.EscalaActual);
            }
            */
        }

        void OnDestroy()
        {
            // Dale al dislike y desuscribete
            GestorEscala.OnCambioEscala -= ActualizarTexto;
        }

        // Función a partir del trigger
        private void ActualizarTexto(NivelEscalaData datos)
        {
            //formato
            textoPantalla.text = $"{datos.nombreNivel} escala ~{datos.tamanoMecha}";
        }
    }
}