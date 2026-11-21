using UnityEngine;

namespace BioVR.Pokedex
{
    // Una entrada del pokédex biológico, resumida de las fichas de Investigación
    // (01-investigacion/fichas, "Fichas de biología celular", 2ª versión, 18-nov).
    // El texto es contenido: lo revisa Investigación y se cambia aquí, en el Inspector, sin tocar código.
    [CreateAssetMenu(fileName = "Ficha", menuName = "BioVR/Ficha del pokédex", order = 2)]
    public class FichaPokedex : ScriptableObject
    {
        [Tooltip("Número de la ficha de Investigación (1 a 16).")]
        public int numero;
        public string nombre;
        [Tooltip("Dónde se encuentra en el viaje: torrente sanguíneo o interior de la célula.")]
        public string zona;
        [Tooltip("Tamaño real, como lo da la ficha.")]
        public string tamano;
        [Tooltip("Lo que ve el jugador: máximo 2 líneas (campo \"Datos para el pokédex\" de la plantilla).")]
        [TextArea(2, 4)] public string descripcion;
        [Tooltip("Un dato curioso de la ficha, opcional.")]
        [TextArea(1, 3)] public string curiosidad;
    }
}
