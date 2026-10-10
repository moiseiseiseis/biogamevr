using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BioVR.Escala.Editor
{
    // Crea en 00-EscenaInicial el objeto "EfectoSalto": un ParticleSystem de rayas que salen de un anillo
    // 60 m al frente y pasan alrededor de la cabina hacia atrás, más el componente EfectoSalto que lo controla.
    // El anillo es angosto (radio 2.5 a 4.2 m) para que las rayas se vean también dentro del capilar.
    // Se puede correr varias veces: borra el anterior y lo vuelve a crear.
    // Menú: BioVR → Agregar efecto de salto
    public static class AgregarEfectoSalto
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string CarpetaMateriales = "Assets/_BioVR/Escala/Materiales";
        const string RutaMaterial = CarpetaMateriales + "/Salto_Particula.mat";

        [MenuItem("BioVR/Agregar efecto de salto")]
        public static void Agregar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var raices = escena.GetRootGameObjects();

            var gestor = raices.Select(r => r.GetComponentInChildren<GestorEscala>(true)).FirstOrDefault(g => g != null);
            if (gestor == null) { Debug.LogError("[EfectoSalto] No hay GestorEscala en la escena inicial."); return; }

            foreach (var viejo in raices.Where(r => r.name == "EfectoSalto"))
                Object.DestroyImmediate(viejo);

            var go = new GameObject("EfectoSalto");
            go.transform.position = new Vector3(0f, 1f, 60f);
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // el cono emite hacia -Z: hacia la cabina y de largo

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.duration = 5f;
            main.playOnAwake = false;
            main.startLifetime = 2f;  // 2 s × 40 m/s = 80 m: de z = 60 a z = -20
            main.startSpeed = 40f;
            main.startSize = 0.12f;
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emision = ps.emission;
            emision.rateOverTime = 0f; // EfectoSalto la sube durante el salto

            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Cone;
            forma.angle = 0f;
            forma.radius = 4.2f;
            forma.radiusThickness = 0.4f; // emite entre 2.5 y 4.2 m del eje: nunca atraviesa la cabina

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = ParticleSystemRenderMode.Stretch;
            rend.velocityScale = 0.05f;
            rend.lengthScale = 3f;
            rend.sharedMaterial = MaterialParticula();
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;

            var efecto = go.AddComponent<EfectoSalto>();
            var so = new SerializedObject(efecto);
            so.FindProperty("particulas").objectReferenceValue = ps;
            so.FindProperty("gestorEscala").objectReferenceValue = gestor;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[EfectoSalto] Listo: EfectoSalto agregado a 00-EscenaInicial.");
        }

        // Partícula sin textura, aditiva (suma luz) y del color que le da EfectoSalto
        static Material MaterialParticula()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaMateriales))
                AssetDatabase.CreateFolder("Assets/_BioVR/Escala", "Materiales");

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterial);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, RutaMaterial);
            }
            mat.shader = shader;
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Surface", 1f);  // transparente
            mat.SetFloat("_Blend", 2f);    // aditivo
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.One);
            mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            mat.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }
    }
}
