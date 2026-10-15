using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Entornos.Editor
{
    // Pone los modelos de Diseño (Assets/_BioVR/Modelos) en lugar de los placeholders (rol 6):
    //   Hangar.prefab          → la nave Seraphim (submarino-exterior) apoyada en el piso; se apaga el submarino de cápsulas
    //   EspacioCelular.prefab  → escenario-celular; se apagan los placeholders (no se borran, por si hay que regresar)
    //                            y sus burbujas, vesículas libres y polirribosomas tiemblan (EmpujonesMoleculares)
    //   00-EscenaInicial       → submarino-cabina dentro de "Cabina (provisional)" con su ANCLA_Jugador en el origen;
    //                            se apagan los cubos y nuestros controles se mueven encima de los controles del modelo
    // Se puede correr varias veces. Menú: BioVR → Poner modelos de Diseño
    public static class PonerModelosDiseno
    {
        public const string PrefijoModelo = "Modelo ";   // así se reconoce un modelo de Diseño dentro de un prefab

        const string Modelos = "Assets/_BioVR/Modelos/";
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";

        [MenuItem("BioVR/Poner modelos de Diseño")]
        public static void Poner()
        {
            Hangar();
            EspacioCelular();
            Cabina();
            Debug.Log("[ModelosDiseno] Listo: nave en el hangar, escenario celular y cabina de Diseño.");
        }

        // ---------------- Hangar ----------------
        static void Hangar()
        {
            const string ruta = "Assets/_BioVR/Entornos/Hangar/Hangar.prefab";
            var raiz = PrefabUtility.LoadPrefabContents(ruta);
            Apagar(raiz.transform, "Submarino (capsulas)");
            var nave = Instanciar(Modelos + "NaveExterior/submarino-exterior.fbx", "Nave Seraphim", raiz.transform);
            // Al frente de la cabina, con la parte más baja tocando el piso del hangar (y = -0.05)
            nave.localPosition = new Vector3(0f, 0f, 17f);
            var b = Limites(nave.gameObject);
            nave.localPosition += new Vector3(0f, -0.05f - b.min.y, 0f);

            // Explorador de pie frente a la nave, a la izquierda del parabrisas, mirando hacia la cabina
            var explorador = Instanciar(Modelos + "Explorador/explorador_caricatura.fbx", "Explorador", raiz.transform);
            explorador.localPosition = new Vector3(-2.5f, 0f, 6f);
            explorador.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var be = Limites(explorador.gameObject);
            explorador.localPosition += new Vector3(0f, -0.05f - be.min.y, 0f);

            PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        // ---------------- Espacio celular ----------------
        static void EspacioCelular()
        {
            const string ruta = "Assets/_BioVR/Entornos/EspacioCelular/EspacioCelular.prefab";
            var raiz = PrefabUtility.LoadPrefabContents(ruta);
            // El escenario de Diseño mide ~500 m: el fondo provisional (esfera de 150 m) lo taparía
            foreach (var nombre in new[] { "Fondo", "Membrana", "Citoesqueleto", "Ribosomas y enzimas", "Golgi", "Mitocondrias", "Cinesina con vesicula" })
                Apagar(raiz.transform, nombre);
            var escenario = Instanciar(Modelos + "EscenarioCelular/escenario-celular.fbx", "Escenario celular", raiz.transform);

            // El temblor del citoplasma (rol 5) pasa a las piezas sueltas del modelo; el de los placeholders quedó apagado.
            // Son grandes (burbujas de 2-6 m, polirribosomas de ~20 m) y están lejos, por eso la amplitud es mayor.
            if (escenario != null)
            {
                var empujones = new SerializedObject(escenario.gameObject.AddComponent<BioVR.Escala.EmpujonesMoleculares>());
                empujones.FindProperty("amplitud").floatValue = 1.5f;
                empujones.FindProperty("frecuencia").floatValue = 0.5f;
                var prefijos = empujones.FindProperty("prefijos");
                var nombres = new[] { "Burbuja", "Vesicula_Libre", "Polirribosoma" };
                prefijos.arraySize = nombres.Length;
                for (int i = 0; i < nombres.Length; i++) prefijos.GetArrayElementAtIndex(i).stringValue = nombres[i];
                empujones.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            PrefabUtility.UnloadPrefabContents(raiz);
        }

        // ---------------- Cabina ----------------
        static void Cabina()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var cabina = escena.GetRootGameObjects().FirstOrDefault(r => r.name == "Cabina (provisional)");
            if (cabina == null) { Debug.LogError("[ModelosDiseno] No hay 'Cabina (provisional)' en la escena inicial."); return; }

            // Piezas provisionales que sustituye el modelo
            foreach (var nombre in new[] { "piso_cabina", "pared_frontal", "pared_posterior", "pared_izquierda", "pared_derecha",
                                           "techo", "tablero_frontal", "Consola (provisional)", "Asiento (provisional)", "Base palanca" })
                Apagar(cabina.transform, nombre);

            var modelo = Instanciar(Modelos + "Cabina/submarino-cabina.fbx", "Cabina de Diseño", cabina.transform);
            // El asiento del modelo (ANCLA_Jugador) va donde está el jugador: x = 0, z = 0
            var ancla = Buscar(modelo, "ANCLA_Jugador");
            modelo.localPosition = ancla != null ? new Vector3(-ancla.localPosition.x, 0f, -ancla.localPosition.z) : Vector3.zero;

            // Nuestros controles (que ya funcionan) encima de los del modelo; las piezas del modelo se esconden
            Encima(cabina.transform, "Boton Iniciar inmersion", modelo, "INT_Boton_Anclaje", copiarRotacion: true);
            Encima(cabina.transform, "Boton Salto Matrioska", modelo, "INT_Boton_Escala", copiarRotacion: true);
            Encima(cabina.transform, "Palanca", modelo, "INT_Palanca_Potencia", copiarRotacion: false);

            // Pantalla de escala sobre la pantalla principal, 2 cm hacia el piloto para que no se encime
            var pantalla = cabina.transform.Find("Pantalla de escala");
            var principal = Buscar(modelo, "INT_Pantalla_Principal");
            if (pantalla != null && principal != null)
            {
                pantalla.position = principal.position + new Vector3(0f, 0f, -0.02f);
                pantalla.rotation = Quaternion.identity;
            }

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
        }

        // ---------------- Utilidades ----------------
        static Transform Instanciar(string ruta, string nombre, Transform padre)
        {
            var nombreCompleto = PrefijoModelo + nombre;
            var viejo = padre.Find(nombreCompleto);
            if (viejo != null) Object.DestroyImmediate(viejo.gameObject);

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            if (fbx == null) { Debug.LogError($"[ModelosDiseno] No se encontró {ruta}."); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, padre);
            go.name = nombreCompleto;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        static void Apagar(Transform padre, string nombre)
        {
            var t = padre.Find(nombre);
            if (t != null) t.gameObject.SetActive(false);
        }

        static Transform Buscar(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == nombre);

        static void Encima(Transform cabina, string control, Transform modelo, string pieza, bool copiarRotacion)
        {
            var c = cabina.Find(control);
            var p = Buscar(modelo, pieza);
            if (c == null || p == null) { Debug.LogWarning($"[ModelosDiseno] No se pudo poner '{control}' sobre '{pieza}'."); return; }
            c.position = p.position;
            if (copiarRotacion) c.rotation = p.rotation;
            p.gameObject.SetActive(false);
        }

        static Bounds Limites(GameObject go)
        {
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
