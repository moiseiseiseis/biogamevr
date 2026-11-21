using System.IO;
using UnityEditor;
using UnityEngine;

namespace BioVR.Entornos.Editor
{
    // Menú BioVR → Poner nave Seraphim.
    //
    // La Seraphim de Diseño (#91, seraphim-mecha.fbx: el Mecha final decidido el 21-nov, una libélula con 6 brazos
    // y 2 patas, 14.6 m) sustituye en Hangar.prefab a la nave anterior (submarino-exterior). Se importa como pide
    // Diseño (sin Apply Transform, para no romper la animación) pero SIN Bake Axis Conversion (ver Correccion) y con la
    // animación de despliegue como Legacy, para NaveDespliegue. Lleva una caja de colisión para caminar alrededor en el hangar.
    // La cabina (submarino-cabina) no cambia. También la usa el prólogo (ImportarSeraphim).
    public static class PonerNaveSeraphim
    {
        public const string Ruta = "Assets/_BioVR/Modelos/NaveExterior/seraphim-mecha.fbx";
        const string Exports = "../../../03-diseno/exports/"; // desde Assets/
        const string Hangar = "Assets/_BioVR/Entornos/Hangar/Hangar.prefab";
        const string Sonidos = "Assets/_BioVR/Comun/Audio/SFX/";

        // El FBX sale de Blender sin Apply Transform: su raíz ("Seraphim") lleva −90° en X y la animación de despliegue
        // TAMBIÉN anima esa raíz con −90°. Con Bake Axis Conversion Unity ya endereza las mallas y la animación le vuelve
        // a sumar −90°: la nave queda con la nariz hacia arriba. Por eso se importa SIN Bake Axis Conversion (la raíz
        // conserva su −90° y la animación pone el mismo). Este giro extra queda en cero; está por si hiciera falta.
        public static readonly Quaternion Correccion = Quaternion.identity;

        [MenuItem("BioVR/Poner nave Seraphim")]
        public static void Poner()
        {
            var fbx = ImportarSeraphim();
            if (fbx == null) return;

            var raiz = PrefabUtility.LoadPrefabContents(Hangar);
            var vieja = raiz.transform.Find(PonerModelosDiseno.PrefijoModelo + "Nave Seraphim");
            if (vieja != null) Object.DestroyImmediate(vieja.gameObject);

            // El giro de corrección va en un objeto padre: la animación mueve el modelo y no debe borrar el giro
            var nave = new GameObject(PonerModelosDiseno.PrefijoModelo + "Nave Seraphim");
            nave.transform.SetParent(raiz.transform, false);
            var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, nave.transform);
            modelo.transform.localPosition = Vector3.zero;
            modelo.transform.localRotation = Quaternion.identity;
            // Al frente de la cabina, como la nave anterior; parada sobre el piso del hangar (y = -0.05) ya desplegada
            nave.transform.localPosition = new Vector3(0f, 0f, 17f);
            nave.transform.localRotation = Correccion;
            var b = Limites(nave);
            nave.transform.localPosition += new Vector3(0f, -0.05f - b.min.y, 0f);
            b = Limites(nave);

            // Una caja para no atravesarla al caminar por el hangar (las patas y brazos se mueven, la caja no)
            var caja = new GameObject("Colision", typeof(BoxCollider));
            caja.transform.SetParent(nave.transform, false);
            var bc = caja.GetComponent<BoxCollider>();
            caja.transform.SetPositionAndRotation(b.center, Quaternion.identity); // alineada con el hangar, no con el giro de la nave
            bc.size = new Vector3(b.size.x / Mathf.Max(nave.transform.lossyScale.x, 1e-4f),
                                  b.size.y / Mathf.Max(nave.transform.lossyScale.y, 1e-4f),
                                  b.size.z / Mathf.Max(nave.transform.lossyScale.z, 1e-4f)) * 0.9f;

            var so = new SerializedObject(nave.AddComponent<NaveDespliegue>());
            so.FindProperty("sonidoPliegue").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Sonidos + "SFX-MEC-03_elitros_v01.wav");
            so.FindProperty("sonidoDespliegue").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Sonidos + "SFX-MEC-02_apoyo-pesado_v01.wav");
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(raiz, Hangar);
            PrefabUtility.UnloadPrefabContents(raiz);
            Debug.Log($"[Nave] Seraphim en el hangar: {b.size.x:F1} × {b.size.y:F1} × {b.size.z:F1} m.");
        }

        // Copia el FBX de 03-diseno/exports (si falta) y lo importa: sin cámaras ni luces, sin Bake Axis Conversion
        // y animación Legacy (NaveDespliegue la reproduce hacia adelante o hacia atrás)
        public static GameObject ImportarSeraphim()
        {
            if (!File.Exists(Ruta))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, Exports + "seraphim-mecha.fbx"));
                if (!File.Exists(origen)) { Debug.LogError($"[Nave] No encontré {origen}. ¿Ya está fusionado el #91?"); return null; }
                File.Copy(origen, Ruta);
                AssetDatabase.ImportAsset(Ruta);
            }
            if (AssetImporter.GetAtPath(Ruta) is ModelImporter mi &&
                (mi.importCameras || mi.importLights || mi.bakeAxisConversion || mi.animationType != ModelImporterAnimationType.Legacy || !mi.importAnimation))
            {
                mi.importCameras = false;
                mi.importLights = false;
                mi.bakeAxisConversion = false; // ver Correccion
                mi.animationType = ModelImporterAnimationType.Legacy;
                mi.importAnimation = true;
                mi.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(Ruta);
        }

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
