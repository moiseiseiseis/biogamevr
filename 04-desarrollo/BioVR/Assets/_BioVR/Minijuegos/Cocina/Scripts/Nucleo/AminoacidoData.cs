using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Un aminoácido: su letra, su abreviatura, su nombre y su color (paleta de Diego,
    // 03-diseno/documentacion/proteinas-p9/LEEME.md). Un asset por aminoácido en Cocina/Datos/Aminoacidos/.
    // Los usa todo lo que pinta aminoácidos: la carga del ARNt, la cadena naciente y las cuentas del holograma.
    [CreateAssetMenu(menuName = "BioVR/Cocina/Aminoácido", fileName = "AA_")]
    public class AminoacidoData : ScriptableObject
    {
        [Tooltip("Código de una letra, por ejemplo K")]
        public char letra;
        [Tooltip("Código de tres letras, por ejemplo Lys")]
        public string abreviatura;
        [Tooltip("Nombre en minúsculas, por ejemplo lisina")]
        public string nombre;
        public Color color = Color.white;

        public override string ToString() => abreviatura;
    }
}
