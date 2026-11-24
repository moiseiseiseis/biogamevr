using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BioVR.Comun
{
    // Hace que un Canvas en el mundo se dibuje encima de todo (nada de la escena lo tapa), como el menú de un juego.
    // Prueba del 23-nov: "el menú de pausa debe quedar sobrepuesto sobre todo".
    //
    // Cómo: a cada gráfico y texto del Canvas se le da una copia de su material con la prueba de profundidad
    // apagada (ZTest Always) y la cola de dibujo de superposición (Overlay), así se pinta al final y sin
    // esconderse detrás de lo que esté más cerca. Sirve para UI/Default y para los shaders de TextMeshPro.
    public static class SobreTodo
    {
        const int Cola = 4000; // RenderQueue.Overlay
        static readonly int IdZTest = Shader.PropertyToID("unity_GUIZTestMode");

        public static void Aplicar(GameObject raiz)
        {
            if (raiz == null) return;
            foreach (var g in raiz.GetComponentsInChildren<Graphic>(true))
            {
                if (g is TMP_Text t)
                {
                    var m = t.fontMaterial; // crea su copia
                    if (m == null) continue;
                    m.SetFloat(IdZTest, (float)CompareFunction.Always);
                    m.renderQueue = Cola;
                    t.fontMaterial = m;
                }
                else
                {
                    // El fondo casi opaco: encima de todo, lo de atrás estorbaba para leer
                    if (g is Image img && img.GetComponent<Button>() == null && img.color.a < 0.92f)
                    { var c = img.color; c.a = 0.92f; img.color = c; }
                    var m = new Material(g.material);
                    m.SetFloat(IdZTest, (float)CompareFunction.Always);
                    m.renderQueue = Cola;
                    g.material = m;
                }
            }
            // Que además quede delante de otros Canvas del mismo tipo
            foreach (var c in raiz.GetComponentsInChildren<Canvas>(true))
            {
                c.overrideSorting = true;
                c.sortingOrder = 100;
            }
        }
    }
}
