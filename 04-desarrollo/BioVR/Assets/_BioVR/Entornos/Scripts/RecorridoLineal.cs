using UnityEngine;

namespace BioVR.Entornos
{
    // Lleva el objeto de 'inicio' a 'fin' a velocidad constante y vuelve a empezar.
    // Se usa para la cinesina que "camina" sobre un microtúbulo arrastrando una vesícula.
    public class RecorridoLineal : MonoBehaviour
    {
        public Vector3 inicio;
        public Vector3 fin;

        [Tooltip("Velocidad en m/s.")]
        [SerializeField] float velocidad = 1f;

        [Tooltip("Pequeño vaivén para que parezca que da pasos.")]
        [SerializeField] float balanceo = 8f;

        float recorrido;

        void Update()
        {
            float largo = Vector3.Distance(inicio, fin);
            if (largo <= 0f) return;

            recorrido = (recorrido + velocidad * Time.deltaTime) % largo;
            transform.localPosition = Vector3.Lerp(inicio, fin, recorrido / largo);

            var direccion = (fin - inicio).normalized;
            transform.localRotation = Quaternion.LookRotation(direccion) *
                                      Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 6f) * balanceo);
        }
    }
}
