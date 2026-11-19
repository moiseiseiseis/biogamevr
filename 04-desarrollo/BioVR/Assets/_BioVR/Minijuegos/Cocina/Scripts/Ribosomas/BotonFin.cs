using BioVR.Minijuegos.ExpresionGenetica;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · Botón FIN del ribosoma (especificación 5.5): cilindro rojo a la izquierda del ribosoma (lado 5′).
    // Solo se ve cuando el ribosoma está en el codón de paro, y pulsa. Se toca con el rayo y el gatillo
    // (Senalable de la fase A) y llama a ControlCocina.TocarFin, que pone el sonido "escala".
    [RequireComponent(typeof(Senalable))]
    public class BotonFin : MonoBehaviour
    {
        [SerializeField] RibosomaCocina ribosoma;
        [SerializeField] GameObject visual;     // cilindro y etiqueta
        [SerializeField] float pulso = 0.12f;   // cuánto crece al pulsar

        Senalable senal;
        Vector3 escalaBase;

        void Awake()
        {
            senal = GetComponent<Senalable>();
            escalaBase = transform.localScale;
            Mostrar(false);
        }

        void OnEnable() => senal.OnSenalado += AlTocar;
        void OnDisable() => senal.OnSenalado -= AlTocar;

        public void Mostrar(bool mostrar)
        {
            if (senal == null) senal = GetComponent<Senalable>();
            if (visual != null) visual.SetActive(mostrar);
            senal.Habilitado = mostrar;
            if (!mostrar) transform.localScale = escalaBase;
        }

        void AlTocar(Senalable s)
        {
            var control = ControlCocina.Instancia;
            if (control != null && ribosoma != null && ribosoma.Estado != null) control.TocarFin(ribosoma.Estado);
        }

        void Update()
        {
            if (!senal.Habilitado) return;
            float k = 1f + pulso * (Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
            transform.localScale = escalaBase * k;
        }
    }
}
