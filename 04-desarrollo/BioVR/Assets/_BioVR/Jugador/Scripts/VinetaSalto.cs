using UnityEngine;
using BioVR.Secuencia;

namespace BioVR.Jugador
{
    public class VinetaSalto : MonoBehaviour
    {
        // Guardará la viñeta para prenderla y apagarla
        public GameObject objetoVineta;

        void Start()
        {
            // Nos aseguramos de arrancar con la viñeta apagada
            if (objetoVineta != null)
            {
                objetoVineta.SetActive(false);
            }

            // Te conectas a los eventos unificados de Moisés
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSaltoIniciado += ActivarVineta;
                GameManager.Instance.OnSaltoCambiado += DesactivarVineta;
            }
        }

        private void ActivarVineta(int saltoNuevo)
        {
            // Prende la viñeta en cuanto inicia el túnel de partículas
            if (objetoVineta != null) objetoVineta.SetActive(true);
        }

        private void DesactivarVineta(int salto)
        {
            // Apaga la viñeta al llegar al nuevo entorno
            if (objetoVineta != null) objetoVineta.SetActive(false);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSaltoIniciado -= ActivarVineta;
                GameManager.Instance.OnSaltoCambiado -= DesactivarVineta;
            }
        }
    }
}







