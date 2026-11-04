using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Un ARNm didáctico: los codones en orden de 5' a 3'.
    // Sana: AUG–GGC–UUU–UAA. Paciente (β-talasemia, paro prematuro; decisión D2): AUG–GGC–UAG.
    [CreateAssetMenu(fileName = "Secuencia", menuName = "BioVR/Expresión Genética/Secuencia de ARNm")]
    public class SecuenciaARNm : ScriptableObject
    {
        public string nombre = "Sana";
        public CodonData[] codones = new CodonData[0];

        public int Largo => codones.Length;
        public CodonData this[int i] => codones[i];
    }
}
