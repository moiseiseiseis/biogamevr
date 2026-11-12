using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Un codón del ARNm y lo que le corresponde según el código genético (decisión D3, Mo6).
    // Los codones de paro (UAA, UAG) no tienen anticodón ni aminoácido: ningún ARNt los lee (ficha 8).
    [CreateAssetMenu(fileName = "Codon", menuName = "BioVR/Expresión Genética/Codón")]
    public class CodonData : ScriptableObject
    {
        [Tooltip("Tres letras del ARNm, de 5' a 3'. P. ej. AUG.")]
        public string codon = "AUG";

        [Tooltip("Anticodón del ARNt que lo lee, escrito de 3' a 5'. Vacío en los codones de paro.")]
        public string anticodon = "UAC";

        [Tooltip("Abreviatura del aminoácido (Met, Gly, Phe, Gln). Vacío en los codones de paro.")]
        public string aminoacido = "Met";

        [Tooltip("UAA, UAG y UGA: los reconoce un factor de liberación, no un ARNt.")]
        public bool esParo;

        [Tooltip("Color del bloque del codón en la cinta de ARNm (provisional hasta la hoja de convenciones de Albert, A1).")]
        public Color color = new Color(0.85f, 0.85f, 0.8f);

        public string AnticodonConExtremos => string.IsNullOrEmpty(anticodon) ? "" : $"3'-{anticodon}-5'";
    }
}
