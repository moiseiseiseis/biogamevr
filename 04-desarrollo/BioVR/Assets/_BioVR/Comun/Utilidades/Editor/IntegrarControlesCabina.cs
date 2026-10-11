using System.Linq;
using BioVR.Cabina;
using BioVR.Jugador;
using BioVR.Secuencia;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

namespace BioVR.Comun.Editor
{
    // Integra en 00-EscenaInicial los controles de la cabina entregados por los roles 2 y 3:
    //   rol 3 → BotonInicio (IniciarInmersion), BotonSalto y Palanca (SaltoMatrioska), PantallaEscala
    //   rol 2 → asiento, viñeta durante el salto (VinetaSalto + TunnelingVignette) y Recentrar (B/Y)
    // Sustituye al "Boton Avanzar" provisional. Se puede correr varias veces: quita lo anterior y lo vuelve a poner.
    // Menú: BioVR → Integrar controles de cabina
    public static class IntegrarControlesCabina
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string Cabina = "Assets/_BioVR/Cabina/";
        const string PrefabVineta = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";

        static readonly string[] Generados =
            { "Boton Avanzar", "Boton Iniciar inmersion", "Boton Salto Matrioska", "Base palanca", "Palanca",
              "Pantalla de escala", "Asiento (provisional)", "Punto ojos" };

        [MenuItem("BioVR/Integrar controles de cabina")]
        public static void Integrar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var raices = escena.GetRootGameObjects();
            var cabina = raices.FirstOrDefault(r => r.name == "Cabina (provisional)");
            var gm = raices.Select(r => r.GetComponentInChildren<GameManager>(true)).FirstOrDefault(g => g != null);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (cabina == null || gm == null || origen == null)
            {
                Debug.LogError("[Integrar] Falta 'Cabina (provisional)', el GameManager o el XR Origin en la escena inicial.");
                return;
            }

            // Limpiar lo que haya puesto una corrida anterior (y el botón provisional)
            foreach (Transform t in cabina.transform.Cast<Transform>().ToArray())
                if (Generados.Contains(t.name)) Object.DestroyImmediate(t.gameObject);
            foreach (var v in origen.Camera.GetComponentsInChildren<TunnelingVignetteController>(true))
                Object.DestroyImmediate(v.gameObject);
            var vinetaVieja = origen.GetComponent<VinetaSalto>();
            if (vinetaVieja != null) Object.DestroyImmediate(vinetaVieja);
            var recentrarViejo = origen.GetComponent<Recentrar>();
            if (recentrarViejo != null) Object.DestroyImmediate(recentrarViejo);

            // ---- Rol 3: botones sobre la consola (consola: centro (0, 0.35, 0.45), arriba en y = 0.7) ----
            var inicio = Poner(Cabina + "BotonInicio.prefab", "Boton Iniciar inmersion", cabina.transform, new Vector3(-0.12f, 0.71f, 0.45f));
            Conectar(inicio.GetComponent<XRSimpleInteractable>().selectEntered, gm.IniciarInmersion);
            var salto = Poner(Cabina + "BotonSalto.prefab", "Boton Salto Matrioska", cabina.transform, new Vector3(0.12f, 0.71f, 0.45f));
            Conectar(salto.GetComponent<XRSimpleInteractable>().selectEntered, gm.SaltoMatrioska);

            // ---- Rol 3: palanca a la derecha del piloto, sobre una base ----
            var basePalanca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePalanca.name = "Base palanca";
            basePalanca.transform.SetParent(cabina.transform, false);
            basePalanca.transform.localPosition = new Vector3(0.45f, 0.25f, 0.35f);
            basePalanca.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            var palanca = Poner(Cabina + "Lever1.prefab", "Palanca", cabina.transform, new Vector3(0.45f, 0.5f, 0.35f));
            var logica = palanca.GetComponentInChildren<Palanca>(true);
            Conectar(logica.alLlegarAlTope, gm.SaltoMatrioska);

            // ---- Rol 3: pantalla de escala arriba del tablero ----
            var pantalla = Poner(Cabina + "PantallaEscala.prefab", "Pantalla de escala", cabina.transform, new Vector3(0f, 1.1f, 1.6f));
            pantalla.transform.localRotation = Quaternion.Euler(-10f, 0f, 0f); // un poco inclinada hacia el piloto

            // ---- Rol 2: asiento, punto de los ojos, recentrar y viñeta ----
            var asiento = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asiento.name = "Asiento (provisional)";
            asiento.transform.SetParent(cabina.transform, false);
            asiento.transform.localPosition = new Vector3(0f, 0.25f, -0.3f);
            asiento.transform.localScale = Vector3.one * 0.5f;

            var ojos = new GameObject("Punto ojos").transform;
            ojos.SetParent(cabina.transform, false);
            ojos.localPosition = new Vector3(0f, 1.2f, 0f);

            var recentrar = origen.gameObject.AddComponent<Recentrar>();
            recentrar.xrOrigin = origen;
            recentrar.puntoOjos = ojos;

            var vineta = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabVineta), escena);
            vineta.transform.SetParent(origen.Camera.transform, false);
            var vinetaSalto = origen.gameObject.AddComponent<VinetaSalto>();
            vinetaSalto.controlador = vineta.GetComponentInChildren<TunnelingVignetteController>(true);

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[Integrar] Listo: botones, palanca, pantalla de escala, asiento, recentrar y viñeta en 00-EscenaInicial.");
        }

        static GameObject Poner(string ruta, string nombre, Transform padre, Vector3 posicion)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
            go.name = nombre;
            go.transform.localPosition = posicion;
            return go;
        }

        static void Conectar(UnityEventBase evento, UnityAction accion) =>
            UnityEventTools.AddVoidPersistentListener(evento, accion);
    }
}
