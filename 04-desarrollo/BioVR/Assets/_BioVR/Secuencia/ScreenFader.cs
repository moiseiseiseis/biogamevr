using System.Collections;
using UnityEngine;

namespace BioVR.Secuencia
{
    // Fundido a negro: un quad hijo de la Main Camera (a ~0.3 m) con FadeMat.
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] Renderer quad;
        Material mat;

        void Awake()
        {
            if (quad == null)
            {
                Debug.LogError("[Secuencia] ScreenFader no tiene asignado el quad; no habrá fundidos.", this);
                enabled = false;
                return;
            }
            mat = quad.material;
        }

        public IEnumerator FadeA(float alphaFinal, float duracion) => FadeA(alphaFinal, duracion, null);

        // color: a qué color se funde (negro en los cambios de estado, blanco en el destello del salto).
        // Si es null se conserva el color actual (útil para desvanecer de regreso).
        public IEnumerator FadeA(float alphaFinal, float duracion, Color? color)
        {
            if (mat == null) yield break;

            if (color.HasValue)
            {
                var c = color.Value;
                c.a = mat.color.a;
                mat.color = c;
            }

            float inicio = mat.color.a, t = 0;
            quad.enabled = true;
            while (t < duracion)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(inicio, alphaFinal, t / duracion));
                yield return null;
            }
            SetAlpha(alphaFinal);
            if (alphaFinal <= 0f) quad.enabled = false;
        }

        void SetAlpha(float a) { var c = mat.color; c.a = a; mat.color = c; }
    }
}
