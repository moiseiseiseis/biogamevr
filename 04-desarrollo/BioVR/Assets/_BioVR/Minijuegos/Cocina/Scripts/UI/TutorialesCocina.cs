using System;
using System.Collections.Generic;
using UnityEngine;



namespace BioVR.Minijuegos.Cocina
{
    [Serializable]
    public class EntradaTutorial
    {
        public string clave;
        [Range(1, 3)] public int turno = 1;
        public string titulo;
        [TextArea(2, 5)] public string cuerpo;
        public Sprite imagenOpcional;
    }

    /// <summary>
    /// ScriptableObject que almacena las 19 ventanas del tutorial de la Cocina
    /// para evitar textos escritos directamente en los scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialesCocina", menuName = "BioVR/Cocina/Tutoriales Cocina")]
    public class TutorialesCocina : ScriptableObject
    {
        public List<EntradaTutorial> entradas = new List<EntradaTutorial>();

        public EntradaTutorial Buscar(string clave)
        {
            for (int i = 0; i < entradas.Count; i++)
            {
                if (entradas[i].clave == clave)
                    return entradas[i];
            }
            return null;
        }
    }
}