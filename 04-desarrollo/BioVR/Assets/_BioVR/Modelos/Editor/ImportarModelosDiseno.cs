using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace BioVR.Modelos.Editor
{
    // Reglas de importación para todo lo que esté en Assets/_BioVR/Modelos/ (los FBX y texturas de Diseño).
    // Se aplican solas al importar o reimportar; así todos tienen los mismos ajustes sin tocar el Inspector.
    //  - Modelos: escala del archivo (cm o m → metros), sin cámaras, luces, animaciones ni colliders.
    //  - Texturas: máximo 1024 px y compresión ASTC en Android (el visor); los "normal" como Normal Map.
    public class ImportarModelosDiseno : AssetPostprocessor
    {
        const string Carpeta = "Assets/_BioVR/Modelos/";

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Carpeta)) return;
            var m = (ModelImporter)assetImporter;
            m.useFileScale = true;        // Blender exporta en cm o m; Unity lo convierte a metros
            m.importCameras = false;
            m.importLights = false;
            m.importAnimation = false;
            m.animationType = ModelImporterAnimationType.None;
            m.addCollider = false;        // los colliders los pone Desarrollo donde hagan falta
            m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            // Las rutas de textura del FBX apuntan a la carpeta de Blender (p. ej. escenario_celula_juego.fbm\...);
            // se buscan por nombre de archivo en todo el proyecto
            m.materialSearch = ModelImporterMaterialSearch.Everywhere;
        }

        // Brillo máximo de la emisión (Blender la exporta hasta ×10 y en el visor todo se satura a blanco)
        const float EmisionMaxima = 0.6f;
        // Brillo de la emisión cuando viene de una textura (los "_Bake" usan su propio color como emisión)
        const float EmisionConTextura = 0.4f;

        // Subir este número cuando cambien las reglas: Unity reimporta solo los modelos y texturas afectados
        public override uint GetVersion() => 5;

        // Se llama al crear cada material del FBX (modo Material Description). Corre después del de URP
        // (orden −980), así que aquí ya están puestos sus colores y solo los ajustamos:
        //  1. Texturas: el FBX las pide con la ruta de la computadora de Diseño (carpeta *.fbm de Blender);
        //     se busca el mismo nombre de archivo en Assets/_BioVR/Modelos y se asigna (color, normal y emisión).
        //  2. Emisión: con textura queda en EmisionConTextura; sin textura se normaliza a EmisionMaxima
        //     (mismo tono); si es blanca o gris y sin textura se quita (es el valor por defecto de Blender).
        void OnPreprocessMaterialDescription(MaterialDescription descripcion, Material material, AnimationClip[] clips)
        {
            if (!assetPath.StartsWith(Carpeta) || !material.HasProperty("_EmissionColor")) return;

            var color = Textura(descripcion, "DiffuseColor");
            if (color != null)
            {
                material.SetTexture("_BaseMap", color);
                material.SetColor("_BaseColor", Color.white); // el color ya viene en la textura
            }

            var normal = Textura(descripcion, "NormalMap");
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }

            var emision = Textura(descripcion, "EmissiveColor");
            if (emision != null)
            {
                material.SetTexture("_EmissionMap", emision);
                Emitir(material, Color.white * EmisionConTextura);
                return;
            }

            var e = material.GetColor("_EmissionColor");
            bool gris = Mathf.Abs(e.r - e.g) < 0.01f && Mathf.Abs(e.g - e.b) < 0.01f;
            float fuerte = Mathf.Max(e.r, e.g, e.b);
            if (gris || fuerte <= 0f)
            {
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            else if (fuerte > EmisionMaxima)
            {
                Emitir(material, new Color(e.r, e.g, e.b) * (EmisionMaxima / fuerte));
            }
        }

        static void Emitir(Material material, Color color)
        {
            material.SetColor("_EmissionColor", color);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        // Busca en Assets/_BioVR/Modelos la textura con el mismo nombre de archivo que pide el FBX
        Texture2D Textura(MaterialDescription descripcion, string propiedad)
        {
            if (!descripcion.TryGetProperty(propiedad, out TexturePropertyDescription t)) return null;
            var archivo = string.IsNullOrEmpty(t.relativePath) ? t.path : t.relativePath;
            archivo = System.IO.Path.GetFileName((archivo ?? "").Replace('\\', '/'));
            if (string.IsNullOrEmpty(archivo)) return null;

            var nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
            foreach (var guid in AssetDatabase.FindAssets(nombre, new[] { Carpeta.TrimEnd('/') }))
            {
                var ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileName(ruta) != archivo) continue;
                context.DependsOnSourceAsset(ruta); // si cambia la textura, se reimporta el modelo
                return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
            }
            Debug.LogWarning($"[Modelos] {descripcion.materialName}: no se encontró la textura '{archivo}' en {Carpeta}");
            return null;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Carpeta)) return;
            var t = (TextureImporter)assetImporter;
            var nombre = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            if (nombre.Contains("normal")) t.textureType = TextureImporterType.NormalMap;
            if (nombre.Contains("metallic") || nombre.Contains("roughness")) t.sRGBTexture = false;

            t.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = (int)TextureCompressionQuality.Normal,
            });
        }
    }
}
