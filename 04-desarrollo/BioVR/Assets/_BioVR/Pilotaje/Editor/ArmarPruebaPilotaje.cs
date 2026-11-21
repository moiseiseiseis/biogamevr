using System.IO;
using System.Linq;
using BioVR.Secuencia;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Pilotaje.Editor
{
    // Menú BioVR → Pilotaje → Armar prueba de pilotaje.
    //
    // Hace Escenas/Pruebas/Pilotaje.unity, una copia de 00-EscenaInicial que empieza directo en la llegada a la célula,
    // sin tutorial, con el pilotaje libre y la brújula en el tablero. Es para probar la comodidad del vuelo en el visor
    // con su propio APK (BioVR → Build APK Pilotaje), sin tocar el juego. Si se aprueba, se integra al flujo normal.
    // Correr después de que la escena inicial tenga el escenario low poly y el pokédex. Se puede correr varias veces.
    public static class ArmarPruebaPilotaje
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        public const string Escena = "Assets/_BioVR/Escenas/Pruebas/Pilotaje.unity";
        const string CarpetaMateriales = "Assets/_BioVR/Pilotaje/Materiales";

        // Brújula sobre la consola, entre los botones y la pantalla, y los ojos del piloto (en la cabina)
        static readonly Vector3 PosicionBrujula = new Vector3(0f, 0.92f, 0.6f);
        static readonly Vector3 OjosPiloto = new Vector3(0f, 1.4f, 0f);

        [MenuItem("BioVR/Pilotaje/Armar prueba de pilotaje")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            Directory.CreateDirectory(Path.GetDirectoryName(Escena));
            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError($"[Pilotaje] No se pudo guardar {Escena}."); return; }

            var raices = escena.GetRootGameObjects();
            var gm = raices.Select(r => r.GetComponentInChildren<GameManager>(true)).FirstOrDefault(g => g != null);
            var cabina = raices.FirstOrDefault(r => r.name == "Cabina (provisional)");
            var llegada = raices.FirstOrDefault(r => r.name == "Zonas")?.transform.Find("Llegada");
            if (gm == null || cabina == null || llegada == null)
            {
                Debug.LogError("[Pilotaje] Falta el GameManager, 'Cabina (provisional)' o 'Zonas/Llegada' en la escena.");
                return;
            }

            // Empieza en la llegada y sin tutorial (sus pasos son del hangar)
            var gso = new SerializedObject(gm);
            gso.FindProperty("estadoInicial").enumValueIndex = (int)GameState.Llegada;
            gso.ApplyModifiedPropertiesWithoutUndo();
            var tutorial = raices.FirstOrDefault(r => r.name == "Tutorial");
            if (tutorial != null) tutorial.SetActive(false);

            foreach (var viejo in raices.Where(r => r.name == "Pilotaje")) Object.DestroyImmediate(viejo);
            var viejaBrujula = cabina.transform.Find("Brujula");
            if (viejaBrujula != null) Object.DestroyImmediate(viejaBrujula.gameObject);

            var (flecha, texto) = Brujula(cabina.transform);

            var go = new GameObject("Pilotaje");
            var so = new SerializedObject(go.AddComponent<PilotajeLibre>());
            so.FindProperty("mundo").objectReferenceValue = llegada;
            so.FindProperty("cabina").objectReferenceValue = cabina.transform;
            so.FindProperty("flecha").objectReferenceValue = flecha;
            so.FindProperty("textoBrujula").objectReferenceValue = texto;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Pilotaje] Listo: {Escena}. Empieza en la llegada. Stick izquierdo: avanzar y de lado; stick derecho: " +
                      "subir, bajar y girar; A: piloto automático. En el editor, con el XR Device Simulator.");
        }

        // Flecha hecha de dos cubos (cuerpo y punta) que apunta al organelo más cercano sin registrar, y su texto
        static (Transform flecha, TextMeshPro texto) Brujula(Transform cabina)
        {
            var mat = Material();
            var raiz = new GameObject("Brujula").transform;
            raiz.SetParent(cabina, false);
            raiz.localPosition = PosicionBrujula;

            var flecha = new GameObject("Flecha").transform;
            flecha.SetParent(raiz, false);
            Cubo("Cuerpo", flecha, new Vector3(0f, 0f, -0.01f), new Vector3(0.012f, 0.012f, 0.07f), Quaternion.identity, mat);
            Cubo("Punta", flecha, new Vector3(0f, 0f, 0.03f), new Vector3(0.03f, 0.012f, 0.03f), Quaternion.Euler(0f, 45f, 0f), mat);

            var go = new GameObject("Texto", typeof(TextMeshPro));
            go.transform.SetParent(raiz, false);
            go.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            var mundoTexto = cabina.TransformPoint(PosicionBrujula + new Vector3(0f, -0.06f, 0f));
            go.transform.rotation = Quaternion.LookRotation(mundoTexto - cabina.TransformPoint(OjosPiloto));
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = "Sin registrar";
            tmp.fontSize = 0.3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.95f, 1f);
            tmp.rectTransform.sizeDelta = new Vector2(0.5f, 0.12f);
            return (flecha, tmp);
        }

        static void Cubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Quaternion rot, Material mat)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = nombre;
            Object.DestroyImmediate(c.GetComponent<Collider>()); // no debe estorbar al rayo ni a la cabina
            c.transform.SetParent(padre, false);
            c.transform.localPosition = pos;
            c.transform.localRotation = rot;
            c.transform.localScale = escala;
            c.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static Material Material()
        {
            if (!AssetDatabase.IsValidFolder(CarpetaMateriales)) AssetDatabase.CreateFolder("Assets/_BioVR/Pilotaje", "Materiales");
            string ruta = CarpetaMateriales + "/Pilotaje_Flecha.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", new Color(0.3f, 0.95f, 1f));
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }
    }
}
