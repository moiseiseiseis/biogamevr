using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BioVR.Escala.Editor
{
    // Crea el material Cabina_Holograma (cian semitransparente que brilla) y agrega CabinaHolografica
    // al objeto "Cabina (provisional)" de 00-EscenaInicial. Se puede correr varias veces.
    // Menú: BioVR → Agregar cabina holográfica
    public static class AgregarCabinaHolografica
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string RutaMaterial = "Assets/_BioVR/Escala/Materiales/Cabina_Holograma.mat";

        [MenuItem("BioVR/Agregar cabina holográfica")]
        public static void Agregar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var cabina = escena.GetRootGameObjects().FirstOrDefault(r => r.name == "Cabina (provisional)");
            if (cabina == null) { Debug.LogError("[CabinaHolografica] No hay 'Cabina (provisional)' en la escena inicial."); return; }

            // (sin ??: con objetos de Unity el operador ?? no detecta los "null falsos" del editor)
            var comp = cabina.GetComponent<CabinaHolografica>();
            if (comp == null) comp = cabina.AddComponent<CabinaHolografica>();
            var so = new SerializedObject(comp);
            so.FindProperty("holograma").objectReferenceValue = MaterialHolograma();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[CabinaHolografica] Listo: la cabina se vuelve holográfica desde el salto 3.");
        }

        static Material MaterialHolograma()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterial);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, RutaMaterial);
            }
            mat.shader = shader;
            var cian = new Color(0.3f, 0.9f, 1f, 0.25f);
            mat.SetColor("_BaseColor", cian);
            // Transparente con mezcla alfa normal
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            // Brillo propio (URP exige el flag Realtime para conservar _EMISSION)
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.9f, 1f) * 0.6f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }
    }
}
