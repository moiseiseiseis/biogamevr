using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Marca vacía de la escena base: dónde va cada estación (tabla 1.2 de la especificación).
    // No hace nada en el juego; en el editor dibuja una esfera y una flecha hacia donde mira (+Z).
    // Cada módulo pone su prefab como hijo de su marca (o copia su posición) en su escena de prueba.
    public class PuntoCocina : MonoBehaviour
    {
        [TextArea(1, 4)] public string nota;
        public Color color = new Color(1f, 0.24f, 0.65f);
        public float radio = 0.05f;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, radio);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * radio * 2.5f);
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(transform.position + Vector3.up * radio * 1.5f, name);
        }
#endif
    }
}
