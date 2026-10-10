using System.Linq;
using BioVR.Escala;
using BioVR.Secuencia;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Comun.Editor
{
    // Integra en 00-EscenaInicial el trabajo de cada rol:
    //   rol 2 → cabina provisional (de Pruebas/Rol2_JugadorSentado)
    //   rol 3 → botón de la cabina (Cabina/Button1.prefab) que llama a GameManager.Avanzar()
    //   rol 4 → zonas y GameManager/ZonasManager/ScreenFader (de Pruebas/Rol4_Secuencia)
    //   rol 5 → GestorEscala con los 4 NivelEscalaData de Escala/Datos
    // Además quita la locomoción del XR Origin (el jugador va sentado) y pone el quad del fundido en la cámara.
    // Desde el editor: menú BioVR → Armar escena inicial.
    // Desde la terminal: Unity -batchmode -executeMethod BioVR.Comun.Editor.ArmarEscenaInicial.Armar
    public static class ArmarEscenaInicial
    {
        const string Escenas = "Assets/_BioVR/Escenas/";
        const string EscenaInicial = Escenas + "00-EscenaInicial.unity";
        const string EscenaRol2 = Escenas + "Pruebas/Rol2_JugadorSentado.unity";
        const string EscenaRol4 = Escenas + "Pruebas/Rol4_Secuencia.unity";
        const string PrefabBoton = "Assets/_BioVR/Cabina/Button1.prefab";
        const string MaterialFade = "Assets/_BioVR/Secuencia/FadeMat.mat";
        const string CarpetaNiveles = "Assets/_BioVR/Escala/Datos";

        static readonly string[] PiezasCabina =
            { "piso_cabina", "pared_frontal", "pared_posterior", "pared_izquierda", "pared_derecha", "techo", "tablero_frontal" };

        // Proveedores de locomoción del XR Origin (XR Rig) de los Starter Assets que se apagan
        static readonly string[] Locomocion = { "Turn", "Move", "Grab Move", "Teleportation", "Climb", "Gravity", "Jump" };

        [MenuItem("BioVR/Armar escena inicial")]
        public static void Armar()
        {
            bool ok = ArmarEscena();
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        static bool ArmarEscena()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            if (escena.GetRootGameObjects().Any(g => g.name == "Secuencia"))
                return Error("La escena inicial ya tiene 'Secuencia'; ya se armó antes. Para repetir, restaura la escena desde git.");

            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (origen == null) return Error("No hay XR Origin en la escena inicial.");

            // ---- Rol 4: zonas y secuencia ----
            var rol4 = EditorSceneManager.OpenScene(EscenaRol4, OpenSceneMode.Additive);
            var zonas = Raiz(rol4, "Zonas");
            var secuencia = Raiz(rol4, "Secuencia");
            if (zonas == null || secuencia == null) return Error("Rol4_Secuencia no tiene 'Zonas' o 'Secuencia'.");
            SceneManager.MoveGameObjectToScene(zonas, escena);
            SceneManager.MoveGameObjectToScene(secuencia, escena);

            // ---- Rol 2: cabina provisional ----
            var rol2 = EditorSceneManager.OpenScene(EscenaRol2, OpenSceneMode.Additive);
            var cabina = new GameObject("Cabina (provisional)");
            SceneManager.MoveGameObjectToScene(cabina, escena);
            foreach (var nombre in PiezasCabina)
            {
                var pieza = Raiz(rol2, nombre);
                if (pieza == null) { Debug.LogWarning($"[ArmarEscena] Falta la pieza de cabina '{nombre}'."); continue; }
                SceneManager.MoveGameObjectToScene(pieza, escena);
                pieza.transform.SetParent(cabina.transform, true);
            }

            EditorSceneManager.CloseScene(rol4, true);
            EditorSceneManager.CloseScene(rol2, true);

            // El piso de la cabina sustituye al provisional (estaban en el mismo plano)
            var piso = Raiz(escena, "Piso (provisional)");
            if (piso != null) piso.SetActive(false);

            // ---- Jugador sentado: sin locomoción ni teleport ----
            foreach (var t in origen.GetComponentsInChildren<Transform>(true))
                if ((Locomocion.Contains(t.name) && t.parent != null && t.parent.name == "Locomotion")
                    || t.name == "Teleport Interactor")
                    t.gameObject.SetActive(false);

            // ---- Fundido: quad pegado a la cámara ----
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Fundido";
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(origen.Camera.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            quad.transform.localScale = new Vector3(2f, 2f, 1f);
            var rend = quad.GetComponent<MeshRenderer>();
            rend.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialFade);
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.enabled = false; // ScreenFader lo enciende solo durante el fundido

            var fader = secuencia.GetComponent<ScreenFader>();
            var so = new SerializedObject(fader);
            so.FindProperty("quad").objectReferenceValue = rend;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ---- Rol 5: GestorEscala escuchando al GameManager ----
            var gestor = secuencia.AddComponent<GestorEscala>();
            gestor.nivelesEscala = AssetDatabase.FindAssets("t:NivelEscalaData", new[] { CarpetaNiveles })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p) // Escala0_Hangar, Escala1_Vasos, Escala2_Capilar, Escala3_Citoplasma
                .Select(AssetDatabase.LoadAssetAtPath<NivelEscalaData>)
                .ToArray();
            if (gestor.nivelesEscala.Length != GameManager.SaltoMaximo + 1)
                Debug.LogWarning($"[ArmarEscena] Se esperaban {GameManager.SaltoMaximo + 1} NivelEscalaData y hay {gestor.nivelesEscala.Length}.");

            // ---- Rol 3: consola con el botón que avanza la secuencia ----
            var consola = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consola.name = "Consola (provisional)";
            consola.transform.SetParent(cabina.transform, false);
            consola.transform.localPosition = new Vector3(0f, 0.35f, 0.45f); // al alcance de la mano estando sentado
            consola.transform.localScale = new Vector3(0.5f, 0.7f, 0.25f);

            var boton = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBoton), escena);
            boton.name = "Boton Avanzar";
            boton.transform.SetParent(cabina.transform, false);
            boton.transform.localPosition = new Vector3(0f, 0.71f, 0.45f); // sobre la consola
            var interactable = boton.GetComponent<XRSimpleInteractable>();
            UnityEventTools.AddVoidPersistentListener(interactable.selectEntered, secuencia.GetComponent<GameManager>().Avanzar);

            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena)) return Error("No se pudo guardar la escena inicial.");
            Debug.Log("[ArmarEscena] Listo: 00-EscenaInicial integra cabina, botón, secuencia, zonas y escalas.");
            return true;
        }

        static GameObject Raiz(Scene escena, string nombre) =>
            escena.GetRootGameObjects().FirstOrDefault(g => g.name == nombre);

        static bool Error(string mensaje)
        {
            Debug.LogError("[ArmarEscena] " + mensaje);
            return false;
        }
    }
}
