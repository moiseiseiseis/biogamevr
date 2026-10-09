using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] Renderer quad;
    Material mat;

    void Awake() => mat = quad.material;

    public IEnumerator FadeA(float alphaFinal, float duracion)
    {
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