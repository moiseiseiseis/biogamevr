using UnityEngine;

namespace BioVR.Comun
{
    // Un paso del tutorial. Para agregar uno: clic derecho en Comun/Tutorial/Pasos → Create → BioVR → Paso del tutorial,
    // ponle un número al inicio del nombre (los pasos van en orden alfabético) y vuelve a correr BioVR → Crear tutorial.
    [CreateAssetMenu(fileName = "00-Paso", menuName = "BioVR/Paso del tutorial")]
    public class PasoTutorial : ScriptableObject
    {
        public string titulo;

        [TextArea(2, 5)] public string instruccion;

        [Tooltip("Acción que completa el paso (la lista está en Tutorial.cs). Vacío: se avanza con el botón Siguiente.")]
        public string clave;

        [Tooltip("Muestra también el botón Siguiente, para quien no quiera hacer este paso.")]
        public bool opcional;

        [Tooltip("Nombres de los objetos de la escena que se resaltan mientras este paso está activo, p. ej. \"Boton Salto Matrioska\". Vacío: no se resalta nada.")]
        public string[] resaltar = new string[0];
    }
}
