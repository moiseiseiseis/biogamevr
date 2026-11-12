using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Parpadeo suave para señalar algo (paso 4 de M4: "el ARNt correcto parpadea suave").
    // Se agrega con Parpadeo.Iniciar(objeto) y se quita con Parpadeo.Detener(objeto). No cambia los materiales.
    public class Parpadeo : MonoBehaviour
    {
        static readonly int IdColor = Shader.PropertyToID("_BaseColor");

        public Color color = new Color(1f, 0.85f, 0.1f);
        public float velocidad = 1.2f;
        [Range(0f, 1f)] public float intensidad = 0.55f;

        Renderer[] renderers;
        MaterialPropertyBlock bloque;

        public static Parpadeo Iniciar(GameObject objeto)
        {
            if (objeto == null) return null;
            var p = objeto.GetComponent<Parpadeo>();
            return p != null ? p : objeto.AddComponent<Parpadeo>();
        }

        public static void Detener(GameObject objeto)
        {
            if (objeto == null) return;
            var p = objeto.GetComponent<Parpadeo>();
            if (p != null) Destroy(p);
        }

        void OnEnable()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            bloque = new MaterialPropertyBlock();
        }

        void Update()
        {
            float t = (Mathf.Sin(Time.unscaledTime * velocidad * 2f * Mathf.PI) * 0.5f + 0.5f) * intensidad;
            foreach (var r in renderers)
            {
                if (r == null || r is ParticleSystemRenderer || r.GetComponent<TMPro.TMP_Text>() != null) continue;
                var mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || !mats[m].HasProperty(IdColor)) continue;
                    var original = mats[m].GetColor(IdColor);
                    r.GetPropertyBlock(bloque, m);
                    var c = Color.Lerp(original, color, t);
                    c.a = original.a;
                    bloque.SetColor(IdColor, c);
                    r.SetPropertyBlock(bloque, m);
                }
            }
        }

        void OnDisable()
        {
            if (renderers == null) return;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                int n = r.sharedMaterials.Length;
                for (int m = 0; m < n; m++)
                {
                    bloque.Clear();
                    r.SetPropertyBlock(bloque, m);
                }
            }
        }
    }
}
