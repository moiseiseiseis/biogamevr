using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    /// <summary>
    /// Marca visual (aro cian pulsante + flecha) que señala en el espacio 3D
    /// el objeto sobre el que habla la ventana activa del tutorial.
    /// </summary>
    public class MarcaTutorial : MonoBehaviour
    {
        [SerializeField] private Transform aroCian;
        [SerializeField] private Transform flecha;
        [SerializeField] private float alturaSobreObjetivo = 0.22f;
        [SerializeField] private float velocidadPulso = 4f;
        [SerializeField] private float amplitudEscala = 0.15f;

        private Transform objetivoActual;
        private Vector3 escalaBaseAro;

        private void Awake()
        {
            if (aroCian != null) escalaBaseAro = aroCian.localScale;
            Ocultar();
        }

        public void Senalar(Transform objetivo)
        {
            objetivoActual = objetivo;
            gameObject.SetActive(objetivo != null);
        }

        public void Ocultar()
        {
            objetivoActual = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (objetivoActual == null)
            {
                Ocultar();
                return;
            }

            transform.position = objetivoActual.position + Vector3.up * alturaSobreObjetivo;
            PosicionMesa.MirarAlJugador(transform);

            float pulso = 1f + Mathf.Sin(Time.unscaledTime * velocidadPulso) * amplitudEscala;
            if (aroCian != null)
                aroCian.localScale = escalaBaseAro * pulso;

            if (flecha != null)
                flecha.localPosition = new Vector3(0f, 0.08f + Mathf.Sin(Time.unscaledTime * velocidadPulso) * 0.02f, 0f);
        }
    }
}