using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // El camino de la corriente de ARNt. Antes era un tubo de 12 cm de grueso; desde la prueba del 23-nov los ARNt
    // flotan en el aire y esto es solo un hilo delgado que marca por dónde pasan. CorrienteARNt le pasa su camino
    // (Configurar) para que los dos coincidan siempre.
    [RequireComponent(typeof(LineRenderer))]
    public class TuboCorrienteVisual : MonoBehaviour
    {
        [SerializeField] int segmentos = 32;
        [SerializeField] float anguloInicio = 115f;
        [SerializeField] float anguloFin = 35f;
        [SerializeField] float radio = 0.80f;
        [SerializeField] float altura = 1.15f;
        [Tooltip("Grueso del hilo (m)")]
        [SerializeField] float grosor = 0.012f;

        void Awake() => Dibujar();

        public void Configurar(float inicio, float fin, float r, float h)
        {
            anguloInicio = inicio; anguloFin = fin; radio = r; altura = h;
            Dibujar();
        }

        void Dibujar()
        {
            var lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = grosor;
            lr.endWidth = grosor;
            lr.positionCount = segmentos;
            for (int i = 0; i < segmentos; i++)
            {
                float t = (float)i / (segmentos - 1);
                lr.SetPosition(i, PosicionMesa.En(Mathf.Lerp(anguloInicio, anguloFin, t), radio, altura));
            }
        }
    }
}
