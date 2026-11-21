using System.IO;
using System.Linq;
using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M10 (Ascencio, con Moisés) · Menú BioVR → Cocina → Armar fase B (M10).
    //
    //   1. Copia bip_chaperona.fbx y proteasoma_26S.fbx de Diego (03-diseno/exports/) a Cocina/Modelos/Diego/,
    //      sin cámaras ni luces. Vienen en angstroms: se escalan al tamaño de los de Uli ×0.6 (especificación 9.3).
    //   2. Prefabs/Cocina_BiP.prefab: los dos estados de Diego (BiP_Abierta 5E84 y BiP_Cerrada 5E85) alineados
    //      en el mismo lugar, con la cerrada apagada. La animación (abre, sujeta, suelta) la hace ChaperonaBiP
    //      (Ascencio) intercambiando esos dos hijos al recibir OnChaperona.
    //   3. Prefabs/Cocina_Proteasoma.prefab: el 26S de pie, con la entrada de arriba como destino de la cadena
    //      (Proteasoma26S).
    //   Los dos se tocan con el rayo o la pinza (TocarFaseB) y solo se ven en la fase B (MostrarEnFaseB).
    //   4. Escenas/Pruebas/Cocina_M10.unity (si no existe la copia de Cocina_Base): holograma, BiP, proteasoma y
    //      PruebaFaseB (B, N, C, P). Al volver a correr el menú solo se reemplazan esas piezas.
    public static class ArmarFaseB
    {
        const string Raiz = CrearDatosCocina.Raiz;
        public const string RutaBiP = Raiz + "/Prefabs/Cocina_BiP.prefab";
        public const string RutaProteasoma = Raiz + "/Prefabs/Cocina_Proteasoma.prefab";
        const string CarpetaDiego = Raiz + "/Modelos/Diego";
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/
        const string Escena = "Assets/_BioVR/Escenas/Pruebas/Cocina_M10.unity";
        const string NombrePrueba = "[M10] Prueba fase B";

        // Lado mayor de chaperona-1aon.fbx y proteasoma-1ryp.fbx de Uli (0.212 y 0.213 m), ×0.6
        const float TamanoBiP = 0.212f * 0.6f;
        const float TamanoProteasoma = 0.213f * 0.6f;

        static readonly Color VerdeLima = new Color(0.65f, 1f, 0.2f);

        [MenuItem("BioVR/Cocina/Armar fase B (M10)")]
        public static void Armar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CrearDatosCocina.Carpeta(CarpetaDiego);
            CrearDatosCocina.Carpeta(Raiz + "/Prefabs");
            var bip = Importar("bip_chaperona.fbx");
            var proteasoma = Importar("proteasoma_26S.fbx");
            if (bip == null || proteasoma == null) return;
            var ubiquitina = MaterialLit("Cocina_Ubiquitina", VerdeLima);

            // Escena vacía para armar los prefabs sin ensuciar ninguna
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PrefabBiP(bip);
            PrefabProteasoma(proteasoma, ubiquitina);
            AssetDatabase.SaveAssets();
            EscenaPrueba();
        }

        // ---------------- BiP ----------------

        static void PrefabBiP(GameObject fbx)
        {
            var (raiz, visual, modelo) = Base("Cocina_BiP", fbx, "BiP (Diego)");
            var abierta = Buscar(modelo, "BiP_Abierta");
            var cerrada = Buscar(modelo, "BiP_Cerrada");
            var baseAbierta = Buscar(modelo, "BiP_Abierta_Base_ATPasa");
            var baseCerrada = Buscar(modelo, "BiP_Cerrada_Base_ATPasa");
            if (abierta == null || cerrada == null || baseAbierta == null || baseCerrada == null)
            {
                Debug.LogError("[M10] bip_chaperona.fbx no trae BiP_Abierta / BiP_Cerrada con su Base_ATPasa.");
                Object.DestroyImmediate(raiz);
                return;
            }
            // En el FBX los dos estados están uno junto al otro: la cerrada se pone donde la abierta,
            // haciendo coincidir la base ATPasa (es la misma en los dos)
            cerrada.position += Limites(baseAbierta.gameObject).center - Limites(baseCerrada.gameObject).center;
            cerrada.gameObject.SetActive(false);

            Ajustar(modelo, abierta.gameObject, TamanoBiP);
            Letrero(visual, "chaperona BiP", Limites(abierta.gameObject).max.y + 0.03f);
            Interaccion(raiz, visual, AccionFaseB.Chaperona, abierta.gameObject);

            float escala = modelo.localScale.x; // antes de destruir la raíz
            PrefabUtility.SaveAsPrefabAsset(raiz, RutaBiP);
            Object.DestroyImmediate(raiz);
            Debug.Log($"[M10] {RutaBiP}: escala del modelo ×{escala:F5}.");
        }

        // ---------------- Proteasoma ----------------

        static void PrefabProteasoma(GameObject fbx, Material ubiquitina)
        {
            var (raiz, visual, modelo) = Base("Cocina_Proteasoma", fbx, "Proteasoma 26S (Diego)");
            Ajustar(modelo, modelo.gameObject, TamanoProteasoma);

            // La cadena entra por la entrada de arriba
            var entradas = new[] { Buscar(modelo, "Proteasoma_Entrada_1"), Buscar(modelo, "Proteasoma_Entrada_2") }.Where(t => t != null).ToArray();
            if (entradas.Length == 0) Debug.LogWarning("[M10] proteasoma_26S.fbx no trae Proteasoma_Entrada_1/2: la cadena irá al centro.");
            var entrada = entradas.Length > 0 ? entradas.OrderByDescending(t => t.position.y).First() : modelo;

            Letrero(visual, "proteasoma 26S", Limites(modelo.gameObject).max.y + 0.03f);
            Interaccion(raiz, visual, AccionFaseB.Proteasoma, modelo.gameObject);
            var so = new SerializedObject(raiz.AddComponent<Proteasoma26S>());
            so.FindProperty("entrada").objectReferenceValue = entrada;
            so.FindProperty("materialUbiquitina").objectReferenceValue = ubiquitina;
            so.ApplyModifiedPropertiesWithoutUndo();

            float escala = modelo.localScale.x;
            string nombreEntrada = entrada.name;
            PrefabUtility.SaveAsPrefabAsset(raiz, RutaProteasoma);
            Object.DestroyImmediate(raiz);
            Debug.Log($"[M10] {RutaProteasoma}: escala del modelo ×{escala:F5}, entrada {nombreEntrada}.");
        }

        // ---------------- Escena de prueba ----------------

        static void EscenaPrueba()
        {
            if (!File.Exists(Escena) && !AssetDatabase.CopyAsset(ArmarCocinaBase.Escena, Escena))
            {
                Debug.LogError($"[M10] No se pudo copiar {ArmarCocinaBase.Escena} a {Escena}.");
                return;
            }
            var escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);
            var ancla = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "AnclaCocina");
            if (ancla == null) { Debug.LogError("[M10] Cocina_M10 no tiene AnclaCocina."); return; }
            var a = ancla.transform;

            // Quitar lo que puso una corrida anterior
            foreach (var t in a.GetComponentsInChildren<Transform>(true).ToArray())
                if (t != null && (t.name.StartsWith("Cocina_Holograma") || t.name.StartsWith("Cocina_BiP") ||
                                  t.name.StartsWith("Cocina_Proteasoma") || t.name == NombrePrueba))
                    Object.DestroyImmediate(t.gameObject);

            ArmarEscenaCocina.PonerEnMarca(AssetDatabase.LoadAssetAtPath<GameObject>(ArmarHolograma.RutaPrefab), a, "Holograma");
            ArmarEscenaCocina.PonerEnMarca(AssetDatabase.LoadAssetAtPath<GameObject>(RutaBiP), a, "Chaperona (BiP)");
            ArmarEscenaCocina.PonerEnMarca(AssetDatabase.LoadAssetAtPath<GameObject>(RutaProteasoma), a, "Proteasoma (26S)");
            var prueba = new GameObject(NombrePrueba, typeof(PruebaFaseB));
            prueba.transform.SetParent(a, false);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[M10] Listo: {Escena}. Play y teclas: B = turno 3 hasta la búsqueda, N = cuenta 20, C = chaperona, P = proteasoma.");
        }

        // ---------------- Utilidades ----------------

        // Raíz (lógica) → Visual (se prende y apaga por fase) → modelo de Diego desempacado
        static (GameObject raiz, Transform visual, Transform modelo) Base(string nombre, GameObject fbx, string nombreModelo)
        {
            var raiz = new GameObject(nombre);
            var visual = new GameObject("Visual").transform;
            visual.SetParent(raiz.transform, false);
            var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, visual);
            PrefabUtility.UnpackPrefabInstance(modelo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            modelo.name = nombreModelo;
            return (raiz, visual, modelo.transform);
        }

        // Escala el modelo para que "medida" tenga ese lado mayor, centrado en X/Z y apoyado en la marca (y = 0)
        static void Ajustar(Transform modelo, GameObject medida, float lado)
        {
            var b = Limites(medida);
            float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (mayor > 1e-6f) modelo.localScale *= lado / mayor;
            b = Limites(medida);
            modelo.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);
        }

        // Collider para el rayo y la pinza, Senalable, TocarFaseB y MostrarEnFaseB en la raíz
        static void Interaccion(GameObject raiz, Transform visual, AccionFaseB accion, GameObject medida)
        {
            var b = Limites(medida);
            var col = raiz.AddComponent<SphereCollider>();
            col.center = raiz.transform.InverseTransformPoint(b.center);
            col.radius = Mathf.Max(0.05f, b.extents.magnitude);
            raiz.AddComponent<Senalable>();
            var tso = new SerializedObject(raiz.AddComponent<TocarFaseB>());
            tso.FindProperty("accion").enumValueIndex = (int)accion;
            tso.ApplyModifiedPropertiesWithoutUndo();
            var mso = new SerializedObject(raiz.AddComponent<MostrarEnFaseB>());
            mso.FindProperty("visual").objectReferenceValue = visual.gameObject;
            mso.ApplyModifiedPropertiesWithoutUndo();
        }

        // Frente al jugador: las marcas tienen su +Z hacia él
        static void Letrero(Transform padre, string texto, float altura)
        {
            var go = new GameObject("Letrero");
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(0f, altura, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = 0.18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.05f);
        }

        // Copia el FBX de Diego a Cocina/Modelos/Diego (si no está) y lo importa sin cámaras ni luces
        static GameObject Importar(string archivo)
        {
            string destino = $"{CarpetaDiego}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[M10] No encontré {origen}. Haz git pull (y git lfs pull)."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter mi && (mi.importCameras || mi.importLights))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(destino);
        }

        static Material MaterialLit(string nombre, Color color)
        {
            string carpeta = Raiz + "/Materiales";
            CrearDatosCocina.Carpeta(carpeta);
            string ruta = $"{carpeta}/{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(m, ruta);
            return m;
        }

        static Transform Buscar(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
