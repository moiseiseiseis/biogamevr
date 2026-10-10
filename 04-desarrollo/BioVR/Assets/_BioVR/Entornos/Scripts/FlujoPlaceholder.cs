using UnityEngine;

namespace BioVR.Entornos
{
    // Mueve a todos sus hijos hacia el jugador (eje -Z) y los regresa al fondo cuando lo pasan.
    // Sirve para los glóbulos rojos del vaso (con pulsos del corazón) y las proteínas del capilar (lento, sin pulsos).
    // La cabina no se mueve: lo que se mueve es el exterior.
    public class FlujoPlaceholder : MonoBehaviour
    {
        [Tooltip("Velocidad base del flujo en m/s.")]
        [SerializeField] float velocidad = 3f;

        [Tooltip("Cuánto acelera en cada latido (0 = flujo constante).")]
        [SerializeField] float pulso = 1.5f;

        [Tooltip("Latidos por segundo (~1.2 = 72 lpm).")]
        [SerializeField] float frecuencia = 1.2f;

        [Tooltip("Velocidad de giro de cada objeto en grados/s.")]
        [SerializeField] float giro = 20f;

        [Tooltip("Al pasar de zMin, el objeto regresa a zMax.")]
        [SerializeField] float zMin = -10f, zMax = 70f;

        void Update()
        {
            // El latido solo suma velocidad en la mitad positiva del seno: empujón y luego calma
            float latido = Mathf.Max(0f, Mathf.Sin(Time.time * frecuencia * 2f * Mathf.PI));
            float paso = velocidad * (1f + pulso * latido) * Time.deltaTime;

            foreach (Transform hijo in transform)
            {
                var p = hijo.localPosition;
                p.z -= paso;
                if (p.z < zMin) p.z += zMax - zMin;
                hijo.localPosition = p;
                hijo.Rotate(giro * Time.deltaTime, giro * 0.5f * Time.deltaTime, 0f, Space.Self);
            }
        }
    }
}
