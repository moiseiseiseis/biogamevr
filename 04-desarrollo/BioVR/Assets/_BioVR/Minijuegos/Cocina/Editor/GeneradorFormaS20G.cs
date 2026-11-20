using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public static class GeneradorFormaS20G
    {
        private const string RutaFBX = "Assets/_BioVR/Minijuegos/Cocina/Modelos/Diego/amilina_horquilla.fbx";
        private const string RutaAsset = "Assets/_BioVR/Minijuegos/Cocina/Datos/FormaS20G.asset";

        [MenuItem("BioVR/Cocina/Generar FormaS20G.asset")]
        public static void Generar()
        {
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(RutaFBX);
            if (fbx == null)
            {
                Debug.LogError($"[M7] No se encontró el FBX en: {RutaFBX}");
                return;
            }

            SortedDictionary<int, Transform> residuos = new SortedDictionary<int, Transform>();
            Regex patron = new Regex(@"^Horquilla_(\d{2})_");

            foreach (Transform t in fbx.GetComponentsInChildren<Transform>(true))
            {
                Match m = patron.Match(t.name);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int indice))
                {
                    residuos[indice] = t;
                }
            }

            if (residuos.Count != 37)
            {
                Debug.LogWarning($"[M7] Se encontraron {residuos.Count} residuos (se esperaban 37). Revisa {RutaFBX}.");
            }

            List<Vector3> coords = new List<Vector3>(residuos.Count);
            bool todasEnCero = true;

            foreach (var par in residuos)
            {
                Vector3 p = fbx.transform.InverseTransformPoint(par.Value.position);
                if (p.sqrMagnitude > 0.0001f) todasEnCero = false;
                coords.Add(p);
            }

            // Respaldo por si las mallas tienen el pivote en (0,0,0)
            if (todasEnCero)
            {
                coords.Clear();
                foreach (var par in residuos)
                {
                    Renderer r = par.Value.GetComponentInChildren<Renderer>();
                    Vector3 centro = r != null
                        ? fbx.transform.InverseTransformPoint(r.bounds.center)
                        : fbx.transform.InverseTransformPoint(par.Value.position);
                    coords.Add(centro);
                }
            }

            FormaProteinaData asset = AssetDatabase.LoadAssetAtPath<FormaProteinaData>(RutaAsset);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<FormaProteinaData>();
                AssetDatabase.CreateAsset(asset, RutaAsset);
            }

            asset.nombreForma = "Amilina S20G (Horquilla Diego)";
            asset.posiciones = coords;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            Debug.Log($"[M7] FormaS20G.asset generado con {coords.Count} posiciones en {RutaAsset}");
        }
    }
}