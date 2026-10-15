using UnityEngine;

namespace BioVR.Escala
{
    // Hace temblar a todos sus hijos de forma errática (ruido Perlin), como moléculas empujadas por el
    // movimiento térmico del citoplasma. La fuerza sale de NivelEscalaData.intensidadEmpujones del nivel actual.
    // Solo se mueve lo de afuera: nunca la cámara ni la cabina, para no marear.
    // Con "prefijos" se eligen solo algunos hijos (p. ej. las burbujas del modelo de Diseño, no la membrana).
    public class EmpujonesMoleculares : MonoBehaviour
    {
        [Tooltip("Cuánto se aleja cada molécula de su lugar (en metros reales) con intensidad de empujones = 1.")]
        [SerializeField] float amplitud = 0.25f;

        [Tooltip("Qué tan rápido cambia el temblor.")]
        [SerializeField] float frecuencia = 0.8f;

        [Tooltip("Si tiene nombres, solo tiemblan los hijos cuyo nombre empieza con alguno. Vacío = todos los hijos.")]
        [SerializeField] string[] prefijos = new string[0];

        Transform[] hijos;
        Vector3[] posicionesBase;
        float intensidad = 1f;

        void Start()
        {
            var elegidos = new System.Collections.Generic.List<Transform>();
            foreach (Transform hijo in transform)
                if (prefijos.Length == 0 || System.Array.Exists(prefijos, p => hijo.name.StartsWith(p)))
                    elegidos.Add(hijo);
            hijos = elegidos.ToArray();
            posicionesBase = new Vector3[hijos.Length];
            for (int i = 0; i < hijos.Length; i++)
                posicionesBase[i] = hijos[i].localPosition;

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
                // El desplazamiento está en metros del mundo; se pasa a las unidades locales del padre
                // (el modelo de Diseño viene de Blender en centímetros y su escala no es 1)
                hijos[i].localPosition = posicionesBase[i] + transform.InverseTransformVector(desplazamiento * fuerza);
            }
        }
    }
}
