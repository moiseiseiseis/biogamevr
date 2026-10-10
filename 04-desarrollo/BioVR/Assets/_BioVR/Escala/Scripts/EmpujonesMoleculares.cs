using UnityEngine;

namespace BioVR.Escala
{
    // Hace temblar a todos sus hijos de forma errática (ruido Perlin), como moléculas empujadas por el
    // movimiento térmico del citoplasma. La fuerza sale de NivelEscalaData.intensidadEmpujones del nivel actual.
    // Solo se mueve lo de afuera: nunca la cámara ni la cabina, para no marear.
    public class EmpujonesMoleculares : MonoBehaviour
    {
        [Tooltip("Cuánto se aleja cada molécula de su lugar (en metros) con intensidad de empujones = 1.")]
        [SerializeField] float amplitud = 0.25f;

        [Tooltip("Qué tan rápido cambia el temblor.")]
        [SerializeField] float frecuencia = 0.8f;

        Transform[] hijos;
        Vector3[] posicionesBase;
        float intensidad = 1f;

        void Start()
        {
            hijos = new Transform[transform.childCount];
            posicionesBase = new Vector3[hijos.Length];
            for (int i = 0; i < hijos.Length; i++)
            {
                hijos[i] = transform.GetChild(i);
                posicionesBase[i] = hijos[i].localPosition;
            }

            var gestor = FindFirstObjectByType<GestorEscala>();
            if (gestor != null && gestor.EscalaActual != null)
                intensidad = gestor.EscalaActual.intensidadEmpujones;
            GestorEscala.OnCambioEscala += AlCambiarEscala;
        }

        void OnDestroy() => GestorEscala.OnCambioEscala -= AlCambiarEscala;

        void AlCambiarEscala(NivelEscalaData datos) => intensidad = datos.intensidadEmpujones;

        void Update()
        {
            float t = Time.time * frecuencia;
            float fuerza = 2f * amplitud * intensidad;
            for (int i = 0; i < hijos.Length; i++)
            {
                float semilla = i * 7.31f; // cada molécula con su propio ruido
                var desplazamiento = new Vector3(
                    Mathf.PerlinNoise(t, semilla) - 0.5f,
                    Mathf.PerlinNoise(semilla, t) - 0.5f,
                    Mathf.PerlinNoise(t + semilla, semilla * 0.5f) - 0.5f);
                hijos[i].localPosition = posicionesBase[i] + desplazamiento * fuerza;
            }
        }
    }
}
