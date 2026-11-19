using System.IO;
using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M6 (Moisés) · Menú BioVR → Cocina → Armar prueba M6.
    //
    //   1. Importa horquilla-arnm.fbx de Uli (03-diseno/exports/minijuego-cocina/) a Cocina/Modelos/Uli/,
    //      sin cámaras ni luces y con su blend shape "abierta".
    //   2. Arma Prefabs/Cocina_Horquilla.prefab: el modelo, el letrero, un collider para el rayo y la pinza,
    //      Senalable y HorquillaARNm.
    //   3. Crea Escenas/Pruebas/Cocina_M6.unity copiando Cocina_Base (si no existe) y le pone:
    //      la horquilla (en la marca "Ribosoma de mesa"), GestorImprevistos, MesaGiratoria (mueve las
    //      marcas "Sintetasa" y "Salida") y PruebaImprevistos (atajos de teclado). Empieza en el turno 2.
    //
    // Se puede correr varias veces: vuelve a generar el prefab y quita de la escena lo que puso antes.
    public static class ArmarPruebaM6
    {
        const string Raiz = CrearDatosCocina.Raiz;
        const string CarpetaUli = Raiz + "/Modelos/Uli";
        const string CarpetaPrefabs = Raiz + "/Prefabs";
        const string RutaPrefab = CarpetaPrefabs + "/Cocina_Horquilla.prefab";
        const string Escena = "Assets/_BioVR/Escenas/Pruebas/Cocina_M6.unity";
        const string ExportsUli = "../../../03-diseno/exports/minijuego-cocina/"; // desde Assets/
        const string NombreGrupo = "Imprevistos (M6)";

        static readonly Color Naranja = new Color(1f, 0.70f, 0.28f); // #ffb347, el del letrero en el HTML

        [MenuItem("BioVR/Cocina/Armar prueba M6")]
        public static void Armar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ArmarCocinaBase.Escena))
            {
                Debug.LogError($"[M6] No existe {ArmarCocinaBase.Escena}. Córrela primero: BioVR → Cocina → Armar escena base.");
                return;
            }
            CrearDatosCocina.Carpeta(CarpetaUli);
            CrearDatosCocina.Carpeta(CarpetaPrefabs);

            // ---- 1. Modelo ----
            string modelo = Importar("horquilla-arnm.fbx");
            if (modelo == null) return;

            // ---- 2. Prefab (en una escena vacía para no ensuciar ninguna) ----
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = PrefabHorquilla(modelo);
            if (prefab == null) return;

            // ---- 3. Escena de prueba ----
            if (!File.Exists(Escena) && !AssetDatabase.CopyAsset(ArmarCocinaBase.Escena, Escena))
            {
                Debug.LogError($"[M6] No se pudo copiar Cocina_Base a {Escena}.");
                return;
            }
            var escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);

            GameObject ancla = null;
            foreach (var go in escena.GetRootGameObjects()) if (go.name == "AnclaCocina") ancla = go;
            if (ancla == null) { Debug.LogError("[M6] La escena no tiene AnclaCocina. Vuelve a copiarla de Cocina_Base."); return; }

            // Quitar lo que puso una corrida anterior
            var viejo = ancla.transform.Find(NombreGrupo);
            if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

            var ribosoma = ancla.transform.Find("Puntos/Ribosoma de mesa");
            var sintetasa = ancla.transform.Find("Puntos/Sintetasa");
            var salida = ancla.transform.Find("Puntos/Salida");
            if (ribosoma == null || sintetasa == null || salida == null)
                Debug.LogWarning("[M6] Faltan marcas en AnclaCocina/Puntos (Ribosoma de mesa, Sintetasa o Salida). Vuelve a armar Cocina_Base.");

            var grupo = new GameObject(NombreGrupo);
            grupo.transform.SetParent(ancla.transform, false);

            var gestor = grupo.AddComponent<GestorImprevistos>();

            var mesa = grupo.AddComponent<MesaGiratoria>();
            var mso = new SerializedObject(mesa);
            mso.FindProperty("sintetasa").objectReferenceValue = sintetasa;
            mso.FindProperty("salida").objectReferenceValue = salida;
            mso.ApplyModifiedPropertiesWithoutUndo();

            var horquilla = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
            horquilla.transform.SetParent(grupo.transform, false);
            var h = horquilla.GetComponent<HorquillaARNm>();
            var hso = new SerializedObject(h);
            hso.FindProperty("ribosoma").objectReferenceValue = ribosoma;
            hso.ApplyModifiedPropertiesWithoutUndo();
            if (ribosoma != null) horquilla.transform.SetPositionAndRotation(ribosoma.TransformPoint(h.posicionEnRibosoma), ribosoma.rotation);

            var prueba = grupo.AddComponent<PruebaImprevistos>();
            var pso = new SerializedObject(prueba);
            pso.FindProperty("horquilla").objectReferenceValue = h;
            pso.FindProperty("gestor").objectReferenceValue = gestor;
            pso.ApplyModifiedPropertiesWithoutUndo();

            // En esta escena se empieza en el turno 2, el primero con imprevistos
            var control = ancla.GetComponentInChildren<ControlCocina>(true);
            if (control != null)
            {
                var cso = new SerializedObject(control);
                cso.FindProperty("turnoInicial").intValue = 1;
                cso.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[M6] Listo: {Escena}. Play: empieza el turno 2; el primer imprevisto sale a los 22 s. " +
                      "Atajos: 1/2/3 turno, H horquilla, G golpe, R codón raro, P pausa.");
        }

        // Copia el FBX de exports a Cocina/Modelos/Uli (si no está) y lo importa sin cámaras ni luces.
        static string Importar(string archivo)
        {
            string destino = $"{CarpetaUli}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.Combine(Application.dataPath, ExportsUli + archivo);
                if (!File.Exists(origen)) { Debug.LogError($"[M6] No encuentro {origen}. Haz git pull (y git lfs pull)."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter mi &&
                (mi.importCameras || mi.importLights || !mi.importBlendShapes))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.importBlendShapes = true;
                mi.SaveAndReimport();
            }
            return destino;
        }

        static GameObject PrefabHorquilla(string rutaModelo)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(rutaModelo);
            if (fbx == null) { Debug.LogError($"[M6] No se pudo cargar {rutaModelo}."); return null; }

            var raiz = new GameObject("Cocina_Horquilla");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(raiz.transform, false);

            // El modelo tal cual (en el HTML se usa sin escalar; el FBX viene en cm y Unity lo pasa a metros)
            var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, visual);
            modelo.name = "Horquilla ARNm (Uli)";
            modelo.transform.localPosition = Vector3.zero;
            modelo.transform.localRotation = Quaternion.identity;

            var malla = modelo.GetComponentInChildren<SkinnedMeshRenderer>();
            if (malla == null) Debug.LogWarning("[M6] El FBX no trae SkinnedMeshRenderer: revisa que 'Import BlendShapes' esté encendido.");
            var b = Limites(modelo);
            Debug.Log($"[M6] Horquilla: tamaño {b.size.x:F3} × {b.size.y:F3} × {b.size.z:F3} m.");

            // Letrero arriba (en el HTML: 0.15 m sobre la horquilla, 1.8 cm de alto, naranja)
            var letreroGo = new GameObject("Letrero", typeof(TextMeshPro));
            letreroGo.transform.SetParent(visual, false);
            letreroGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            var tmp = letreroGo.GetComponent<TextMeshPro>();
            tmp.text = "horquilla";
            tmp.fontSize = 0.25f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Naranja;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.06f);

            // Collider para el rayo y la pinza: una esfera que envuelve el modelo, de al menos 5 cm
            var col = raiz.AddComponent<SphereCollider>();
            col.center = raiz.transform.InverseTransformPoint(b.center);
            col.radius = Mathf.Max(0.05f, b.extents.magnitude);

            raiz.AddComponent<Senalable>(); // agrega también XRSimpleInteractable
            var h = raiz.AddComponent<HorquillaARNm>();
            var so = new SerializedObject(h);
            so.FindProperty("visual").objectReferenceValue = visual;
            so.FindProperty("malla").objectReferenceValue = malla;
            so.FindProperty("letrero").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
            Object.DestroyImmediate(raiz);
            return prefab;
        }

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
