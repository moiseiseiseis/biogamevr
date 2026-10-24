using System.Linq;
using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Comun.Editor
{
    // Arma la escena del minijuego Expresión Genética (Mo1) con la mesa de prueba de controles (Mo2):
    //   - XR Origin de los Starter Assets, sin locomoción (el jugador va sentado frente a la mesa)
    //   - un estante lejos con 4 piezas neutras (cubo, esfera, cilindro y cápsula, que no va en ninguna ranura)
    //   - 3 ranuras en la mesa, un letrero con instrucciones y el botón Reiniciar
    // Atraer = rayo + grip (la pieza vuela a la mano), sujetar = mantener el grip, rotar = muñeca o stick,
    // seleccionar = soltarla en su ranura con la cara blanca hacia el jugador.
    // Cuando lleguen los proxies de Albert y Ascencio (Mo3), las piezas serán ARNt y las ranuras, codones.
    // Cada vez que se corre se vuelve a generar la escena desde cero.
    // Desde el editor: menú BioVR → Armar escena Expresión Genética.
    public static class ArmarEscenaExpresionGenetica
    {
        const string Carpeta = "Assets/_BioVR/Minijuegos/ExpresionGenetica";
        public const string Escena = Carpeta + "/Escenas/ExpresionGenetica.unity";
        const string CarpetaMateriales = Carpeta + "/Materiales";
        const string PrefabOrigen = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string PrefabBoton = "Assets/_BioVR/Cabina/Button1.prefab";

        // Igual que en ArmarEscenaInicial: el jugador va sentado
        static readonly string[] Locomocion = { "Turn", "Move", "Grab Move", "Teleportation", "Climb", "Gravity", "Jump" };

        const float AlturaMesa = 0.72f;

        // Texto oscuro sobre paneles claros: el blanco se perdía con el cielo gris de Unity
        static readonly Color ColorTexto = new Color(0.08f, 0.1f, 0.15f);
        static readonly Color ColorPanel = new Color(0.93f, 0.92f, 0.88f);

        // (clave, nombre, primitiva, escala del modelo, color)
        static readonly (string, string, PrimitiveType, Vector3, Color)[] Piezas =
        {
            ("cubo", "Cubo", PrimitiveType.Cube, Vector3.one * 0.1f, new Color(0.85f, 0.3f, 0.25f)),
            ("esfera", "Esfera", PrimitiveType.Sphere, Vector3.one * 0.11f, new Color(0.25f, 0.5f, 0.9f)),
            ("cilindro", "Cilindro", PrimitiveType.Cylinder, new Vector3(0.1f, 0.06f, 0.1f), new Color(0.3f, 0.75f, 0.35f)),
            ("capsula", "Cápsula", PrimitiveType.Capsule, new Vector3(0.08f, 0.06f, 0.08f), new Color(0.65f, 0.4f, 0.85f)),
        };

        // Las piezas empiezan con la cara blanca hacia otro lado, para que haya que rotarlas
        static readonly float[] GiroInicial = { 180f, 90f, -90f, 135f };

        const int NumRanuras = 3; // cubo, esfera y cilindro; la cápsula sobra a propósito

        const string Leyenda =
            "<b>Controles</b>\n" +
            "1. Apunta a una pieza del estante con el rayo.\n" +
            "2. <b>Grip</b> (botón del costado): mantenlo para atraerla y sujetarla.\n" +
            "3. Gírala con la muñeca o con el <b>stick</b>.\n" +
            "4. Llévala a su ranura con la <b>cara blanca hacia ti</b>: se pone amarilla.\n" +
            "5. Suelta el grip. Verde: encajó. Roja: no va ahí.\n" +
            "<b>Reiniciar</b>: apunta al botón de la mesa y aprieta el grip.";

        [MenuItem("BioVR/Armar escena Expresión Genética")]
        public static void Armar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            System.IO.Directory.CreateDirectory(Carpeta + "/Escenas");
            System.IO.Directory.CreateDirectory(CarpetaMateriales);
            AssetDatabase.Refresh();

            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Object.DestroyImmediate(GameObject.Find("Main Camera")); // la cámara la trae el XR Origin

            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            // ---- Jugador sentado ----
            var origen = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigen), escena);
            if (origen == null) { Debug.LogError("[ArmarExpresion] No se encontró el XR Origin de los Starter Assets."); return; }
            foreach (var t in origen.GetComponentsInChildren<Transform>(true))
                if ((Locomocion.Contains(t.name) && t.parent != null && t.parent.name == "Locomotion")
                    || t.name == "Teleport Interactor")
                    t.gameObject.SetActive(false);

            // ---- Sala ----
            var piso = Primitiva("Piso", PrimitiveType.Plane, null, Vector3.zero, Vector3.one, Material("Piso", new Color(0.32f, 0.34f, 0.38f)));
            var mesa = Primitiva("Mesa", PrimitiveType.Cube, null, new Vector3(0f, AlturaMesa / 2f, 0.55f),
                new Vector3(1.1f, AlturaMesa, 0.5f), Material("Mesa", new Color(0.78f, 0.66f, 0.5f)));
            // El estante queda fuera del alcance: las piezas se atraen con el rayo
            var estante = Primitiva("Estante", PrimitiveType.Cube, null, new Vector3(0f, 0.95f, 1.7f),
                new Vector3(1.4f, 0.05f, 0.35f), Material("Estante", new Color(0.45f, 0.47f, 0.52f)));

            var juego = new GameObject("Mesa de prueba");
            var mesaPrueba = juego.AddComponent<MesaPrueba>();
            var blanco = Material("Marca", Color.white);

            // ---- Piezas ----
            var piezas = new PiezaManipulable[Piezas.Length];
            for (int i = 0; i < Piezas.Length; i++)
            {
                var (clave, nombre, tipo, escala, color) = Piezas[i];
                var raiz = new GameObject("Pieza " + nombre);
                raiz.transform.SetParent(juego.transform, false);
                raiz.transform.position = new Vector3(-0.48f + 0.32f * i, 1.05f, 1.7f);
                raiz.transform.rotation = Quaternion.Euler(0f, GiroInicial[i], 0f);

                var modelo = Primitiva("Modelo", tipo, raiz.transform, Vector3.zero, escala, Material(nombre, color));
                modelo.transform.localPosition = Vector3.zero;
                // Cara blanca en el +Z del modelo (sin collider)
                var marca = Primitiva("Marca", PrimitiveType.Cube, modelo.transform, new Vector3(0f, 0f, 0.5f),
                    new Vector3(0.6f, 0.6f, 0.06f), blanco, conCollider: false);
                marca.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                var cuerpo = raiz.AddComponent<Rigidbody>();
                cuerpo.mass = 0.2f;
                var agarre = raiz.AddComponent<XRGrabInteractable>();
                agarre.farAttachMode = InteractableFarAttachMode.Near; // atraer: vuela a la mano
                agarre.movementType = XRBaseInteractable.MovementType.Kinematic; // sigue a la mano sin que la física la empuje
                agarre.useDynamicAttach = true;
                agarre.matchAttachPosition = false;  // se sujeta por el centro (si no, queda a la distancia a la que se atrajo)
                agarre.matchAttachRotation = false;  // conserva su giro: rotarla es parte del reto
                agarre.snapToColliderVolume = false;
                agarre.throwOnDetach = false;        // que no salga volando al soltarla

                var pieza = raiz.AddComponent<PiezaManipulable>();
                pieza.clave = clave;
                pieza.modelo = modelo.transform;
                pieza.marca = marca.transform;
                piezas[i] = pieza;
            }

            // ---- Ranuras ----
            var ranuras = new RanuraSeleccion[NumRanuras];
            for (int i = 0; i < NumRanuras; i++)
            {
                var (clave, nombre, _, escala, color) = Piezas[i];
                var ranura = new GameObject("Ranura " + nombre);
                ranura.transform.SetParent(juego.transform, false);
                ranura.transform.position = new Vector3(-0.3f + 0.3f * i, AlturaMesa + 0.08f, 0.45f);
                ranura.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // su frente (Z) mira al jugador

                var zona = ranura.AddComponent<SphereCollider>();
                zona.isTrigger = true;
                zona.radius = 0.1f;

                var socket = ranura.AddComponent<XRSocketInteractor>();
                socket.showInteractableHoverMeshes = false; // el aviso es el color de la base

                var bas = Primitiva("Base", PrimitiveType.Cylinder, ranura.transform, Vector3.zero,
                    new Vector3(0.16f, 0.01f, 0.16f), Material("Ranura " + nombre, color * 0.55f + Color.black * 0.45f), conCollider: false);
                bas.transform.localPosition = new Vector3(0f, -0.07f, 0f);

                var etiqueta = Texto("Etiqueta", ranura.transform, nombre, 0.35f, new Vector2(0.3f, 0.06f));
                etiqueta.transform.localPosition = new Vector3(0f, -0.07f, 0.11f);
                etiqueta.transform.localRotation = Quaternion.Euler(60f, 180f, 0f); // acostada, legible desde el asiento

                var rs = ranura.AddComponent<RanuraSeleccion>();
                rs.clave = clave;
                var so = new SerializedObject(rs);
                so.FindProperty("indicador").objectReferenceValue = bas.GetComponent<Renderer>();
                so.ApplyModifiedPropertiesWithoutUndo();
                ranuras[i] = rs;
            }

            // ---- Letrero y botón ----
            var letrero = Texto("Letrero", juego.transform, "", 0.6f, new Vector2(1.6f, 0.45f));
            letrero.transform.position = new Vector3(0f, 1.35f, 1.6f);
            var fondoLetrero = Primitiva("Fondo", PrimitiveType.Quad, letrero.transform, Vector3.zero, new Vector3(1.7f, 0.5f, 1f),
                Material("Panel", ColorPanel), conCollider: false);
            fondoLetrero.transform.localPosition = new Vector3(0f, 0f, 0.005f);

            // Leyenda fija de controles para la prueba T1 (el tutorial paso a paso será Mo4, con el guion M3)
            var leyenda = new GameObject("Leyenda de controles");
            leyenda.transform.SetParent(juego.transform, false);
            leyenda.transform.position = new Vector3(-0.85f, 1.15f, 0.75f);
            leyenda.transform.rotation = Quaternion.Euler(-10f, -49f, 0f); // mira al asiento, un poco inclinada
            var fondo = Primitiva("Fondo", PrimitiveType.Quad, leyenda.transform, Vector3.zero, new Vector3(0.62f, 0.5f, 1f),
                Material("Panel", ColorPanel), conCollider: false);
            fondo.transform.localPosition = new Vector3(0f, 0f, 0.005f);
            fondo.transform.localRotation = Quaternion.identity;
            var textoLeyenda = Texto("Texto", leyenda.transform, Leyenda, 0.22f, new Vector2(0.56f, 0.44f));
            textoLeyenda.alignment = TextAlignmentOptions.TopLeft;
            textoLeyenda.enableAutoSizing = true; // que quepa todo en el panel
            textoLeyenda.fontSizeMin = 0.15f;
            textoLeyenda.fontSizeMax = 0.26f;

            var boton = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBoton), escena);
            if (boton != null)
            {
                boton.name = "Boton Reiniciar";
                boton.transform.SetParent(juego.transform, false);
                boton.transform.position = new Vector3(0.45f, AlturaMesa + 0.005f, 0.4f);
                UnityEventTools.AddVoidPersistentListener(boton.GetComponent<XRSimpleInteractable>().selectEntered, mesaPrueba.Reiniciar);
                var texto = Texto("Etiqueta", juego.transform, "Reiniciar", 0.35f, new Vector2(0.3f, 0.06f));
                texto.transform.position = new Vector3(0.45f, AlturaMesa + 0.005f, 0.33f);
                texto.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            }
            else Debug.LogWarning("[ArmarExpresion] No se encontró el botón de la cabina; la escena queda sin Reiniciar.");

            var mso = new SerializedObject(mesaPrueba);
            Asignar(mso.FindProperty("ranuras"), ranuras);
            Asignar(mso.FindProperty("piezas"), piezas);
            mso.FindProperty("letrero").objectReferenceValue = letrero;
            mso.ApplyModifiedPropertiesWithoutUndo();

            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError("[ArmarExpresion] No se pudo guardar la escena."); return; }
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArmarExpresion] Listo: {Escena} ({piezas.Length} piezas, {ranuras.Length} ranuras). " +
                      "Para el APK: BioVR → Build APK Expresión Genética.");
        }

        static GameObject Primitiva(string nombre, PrimitiveType tipo, Transform padre, Vector3 posicion, Vector3 escala,
            Material material, bool conCollider = true)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            if (padre != null) go.transform.SetParent(padre, false);
            go.transform.position = posicion;
            go.transform.localScale = escala;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!conCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static TextMeshPro Texto(string nombre, Transform padre, string texto, float tamano, Vector2 caja)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = ColorTexto;
            tmp.rectTransform.sizeDelta = caja;
            return tmp;
        }

        // Material URP guardado como asset (los creados en memoria no se guardan en la escena)
        static Material Material(string nombre, Color color)
        {
            var ruta = $"{CarpetaMateriales}/{nombre}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, ruta);
            }
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void Asignar(SerializedProperty lista, Object[] valores)
        {
            lista.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
        }
    }
}
