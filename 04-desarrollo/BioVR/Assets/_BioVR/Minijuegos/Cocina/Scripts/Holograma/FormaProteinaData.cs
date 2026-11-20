using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    [CreateAssetMenu(fileName = "FormaProteina", menuName = "BioVR/Cocina/Forma de Proteína")]
    public class FormaProteinaData : ScriptableObject
    {
        [Tooltip("Identificador de la estructura (ej. Horquilla S20G Diego, Fibra 8AZ4)")]
        public string nombreForma = "Amilina S20G (Horquilla)";

        [Tooltip("Posiciones locales de los 37 residuos en orden (1 al 37)")]
        public List<Vector3> posiciones = new List<Vector3>();
    }
}