using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(LineRenderer))]
    public class TuboCorrienteVisual : MonoBehaviour
    {
        [SerializeField] int segmentos = 24;
        [SerializeField] float anguloInicio = 66f;
        [SerializeField] float anguloFin = 14f;
        [SerializeField] float radio = 0.72f;
        [SerializeField] float altura = 1.06f;

        void Awake()
        {
            var lr = GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = 0.12f; // Diámetro = 2 * 0.06 m de radio
            lr.endWidth = 0.12f;
            lr.positionCount = segmentos;

            for (int i = 0; i < segmentos; i++)
            {
                float t = (float)i / (segmentos - 1);
                float ang = Mathf.Lerp(anguloInicio, anguloFin, t);
                lr.SetPosition(i, PosicionMesa.En(ang, radio, altura));
            }
        }
    }
}

