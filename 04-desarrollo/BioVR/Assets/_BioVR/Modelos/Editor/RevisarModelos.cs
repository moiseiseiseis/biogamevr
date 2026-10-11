using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BioVR.Modelos.Editor
{
    // Reporta en la Console, por cada FBX de Assets/_BioVR/Modelos/: tamaño en metros, triángulos (todos y solo LOD0),
    // materiales y la posición de las piezas importantes (ancla del jugador y controles INT_ de la cabina).
    // Sirve para revisar que vengan en metros y dentro del presupuesto del visor antes de ponerlos en la escena.
    // Menú: BioVR → Revisar modelos de Diseño
    public static class RevisarModelos
    {
        [MenuItem("BioVR/Revisar modelos de Diseño")]
        public static void Revisar()
        {
            var sb = new StringBuilder("[Modelos] Reporte\n");
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_BioVR/Modelos" }))
            {
                var ruta = AssetDatabase.GUIDToAssetPath(guid);
                var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
                var filtros = modelo.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToArray();
                var pieles = modelo.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s => s.sharedMesh != null).ToArray();

                long tris = filtros.Sum(f => (long)f.sharedMesh.triangles.Length / 3) + pieles.Sum(s => (long)s.sharedMesh.triangles.Length / 3);
                // Lo que se dibuja de cerca: lo que no es LOD1/LOD2
                long trisLod0 = filtros.Where(f => !f.name.Contains("_LOD1") && !f.name.Contains("_LOD2"))
                                       .Sum(f => (long)f.sharedMesh.triangles.Length / 3);

                var rends = modelo.GetComponentsInChildren<Renderer>(true);
                var b = new Bounds(modelo.transform.position, Vector3.zero);
                bool hay = false;
                foreach (var r in rends) { if (!hay) { b = r.bounds; hay = true; } else b.Encapsulate(r.bounds); }
                int mats = rends.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().Count();

                sb.AppendLine($"• {ruta}");
                sb.AppendLine($"    tamaño: {b.size.x:F2} × {b.size.y:F2} × {b.size.z:F2} m · centro {b.center}");
                sb.AppendLine($"    triángulos: {tris:N0} (sin LOD1/LOD2: {trisLod0:N0}) · mallas: {filtros.Length + pieles.Length} · materiales: {mats}");
                foreach (var t in modelo.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("ANCLA") || t.name.StartsWith("INT_") || t.name.StartsWith("CAB_"))
                        sb.AppendLine($"    {t.name}: pos {t.position} · rot {t.eulerAngles}");
            }
            Debug.Log(sb.ToString());
        }

        // Qué domina la vista en el escenario celular: los objetos más grandes y los materiales más claros o con brillo propio
        [MenuItem("BioVR/Revisar materiales del escenario celular")]
        public static void RevisarEscenario()
        {
            var modelo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BioVR/Modelos/EscenarioCelular/escenario-celular.fbx");
            var sb = new StringBuilder("[Modelos] Escenario celular\nObjetos más grandes:\n");
            foreach (var r in modelo.GetComponentsInChildren<Renderer>(true).OrderByDescending(r => r.bounds.size.magnitude).Take(12))
                sb.AppendLine($"    {r.name}: {r.bounds.size.x:F0}×{r.bounds.size.y:F0}×{r.bounds.size.z:F0} m · centro {r.bounds.center} · {string.Join(", ", r.sharedMaterials.Where(m => m != null).Select(m => m.name))}");

            sb.AppendLine("Materiales (color base · emisión · shader · transparente):");
            var mats = modelo.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct();
            foreach (var m in mats.OrderByDescending(m => Brillo(m)))
            {
                var baseC = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.clear;
                var emis = m.HasProperty("_EmissionColor") && m.IsKeywordEnabled("_EMISSION") ? m.GetColor("_EmissionColor") : Color.black;
                bool mapaEmision = m.HasProperty("_EmissionMap") && m.GetTexture("_EmissionMap") != null;
                var mapa = m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null ? m.GetTexture("_BaseMap").name : "sin textura";
                sb.AppendLine($"    {m.name}: base {baseC} ({mapa}) · emisión {emis}{(mapaEmision ? " + mapa" : "")} · {m.shader.name} · cola {m.renderQueue}");
            }
            Debug.Log(sb.ToString());
        }

        static float Brillo(Material m)
        {
            var e = m.HasProperty("_EmissionColor") && m.IsKeywordEnabled("_EMISSION") ? m.GetColor("_EmissionColor").maxColorComponent : 0f;
            var b = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").grayscale : 0f;
            return e * 10f + b;
        }
    }
}
