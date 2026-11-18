using UnityEditor;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Dibuja en la vista Scene todas las marcas PuntoCocina, también las que están apagadas
    // (AnclaCocina se guarda apagada y Unity no dibuja gizmos de objetos apagados).
    // No depende del botón Gizmos: se ve siempre que haya marcas en la escena abierta.
    [InitializeOnLoad]
    static class DibujarPuntosCocina
    {
        static PuntoCocina[] cache; // se vuelve a buscar cuando cambia la jerarquía

        static DibujarPuntosCocina()
        {
            SceneView.duringSceneGui += Dibujar;
            EditorApplication.hierarchyChanged += () => cache = null;
        }

        static void Dibujar(SceneView vista)
        {
            if (Event.current.type != EventType.Repaint) return;

            cache ??= Object.FindObjectsByType<PuntoCocina>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            var estilo = new GUIStyle(EditorStyles.boldLabel);
            foreach (var p in cache)
            {
                if (p == null) continue;
                var t = p.transform;
                var pos = t.position;
                Handles.color = p.color;
                Handles.DrawWireDisc(pos, Vector3.up, p.radio, 2f);
                Handles.DrawWireDisc(pos, Vector3.right, p.radio, 2f);
                Handles.DrawWireDisc(pos, Vector3.forward, p.radio, 2f);
                Handles.DrawLine(pos, pos + t.forward * p.radio * 2.5f, 2f);
                estilo.normal.textColor = p.color;
                Handles.Label(pos + Vector3.up * p.AlturaEtiqueta, p.Etiqueta, estilo);
            }
        }
    }
}
