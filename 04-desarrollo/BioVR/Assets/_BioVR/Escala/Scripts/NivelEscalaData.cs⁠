using UnityEngine;

namespace BioVR.Escala
{
    [CreateAssetMenu(fileName = "NivelEscalaData", menuName = "BioVR/Nivel de Escala Data", order = 1)]
    public class NivelEscalaData : ScriptableObject
    {
        [Header("Información General")]
        [Tooltip("Nombre descriptivo del nivel de escala.")]
        public string nombreNivel;

        [Tooltip("Descripción del tamaño del mecha (ej. '~1-5 mm', '~200-500 nm').")]
        public string tamanoMecha;

        [Header("Físicas y Comportamiento")]
        [Tooltip("Factor de inercia (1.0 = normal, 0.0 = sin inercia/se detiene al instante).")]
        [Range(0f, 1f)]
        public float inercia = 1f;

        [Tooltip("Resistencia o viscosidad del medio ('como moverse en miel').")]
        [Range(0f, 10f)]
        public float viscosidad = 1f;

        [Tooltip("Intensidad del empuje molecular errático sobre las partículas exteriores.")]
        public float intensidadEmpujones = 0f;

        [Header("Aspecto Visual")]
        [Tooltip("Color ambiental o del fluido para este nivel de escala.")]
        public Color colorFluido = Color.clear;
    }
}
