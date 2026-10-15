using System.Linq;
using BioVR.Jugador;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace BioVR.Comun.Editor
{
    // Arma el menú de pausa en 00-EscenaInicial:
    //   - EventSystem con XRUIInputModule (para que los rayos de los mandos puedan usar la UI)
    //   - "Menu de pausa" con MenuPausa y un Canvas world space: título, Continuar, Reiniciar,
    //     altura del asiento (Bajar / Subir), Salir y los controles
    //   - AlturaAsiento junto a Recentrar (mismo XR Origin y Punto ojos), si no estaba
    // Se puede correr varias veces: borra el menú anterior y lo vuelve a crear.
    // Menú: BioVR → Agregar menú de pausa
    public static class AgregarMenuPausa
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string NombreMenu = "Menu de pausa";
        const int CapaUI = 5; // capa "UI" de Unity

        // Lo que el jugador puede hacer; se muestra abajo de los botones
        public const string TextoControles =
            "<b>Controles</b>\n" +
            "Stick izquierdo: caminar · Stick derecho: girar\n" +
            "Botones de la cabina: tócalos con la mano\n" +
            "Palanca: agárrala con el botón lateral (grip)\n" +
            "Este menú: apunta y usa el gatillo\n" +
            "B / Y: volver al asiento · Menú (mando izq.): pausa";

        static readonly Color Fondo = new Color(0.05f, 0.09f, 0.14f, 0.92f);
        static readonly Color ColorBoton = new Color(0.16f, 0.45f, 0.7f);
        static readonly Color ColorSalir = new Color(0.65f, 0.2f, 0.22f);

        [MenuItem("BioVR/Agregar menú de pausa")]
        public static void Agregar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (origen == null || origen.Camera == null) { Debug.LogError("[Menu] No hay XR Origin con cámara en la escena inicial."); return; }

            AsegurarEventSystem();
            var altura = AsegurarAlturaAsiento();

            var viejo = escena.GetRootGameObjects().FirstOrDefault(g => g.name == NombreMenu);
            if (viejo != null) Object.DestroyImmediate(viejo);

            var raiz = new GameObject(NombreMenu);
            var menu = raiz.AddComponent<MenuPausa>();

            // ---- Canvas: 1 unidad = 1 mm, así el panel mide 0.5 × 0.74 m ----
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                                       typeof(TrackedDeviceGraphicRaycaster), typeof(Image));
            panel.transform.SetParent(raiz.transform, false);
            var canvas = panel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = origen.Camera;
            panel.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f; // texto nítido en world space
            var rect = (RectTransform)panel.transform;
            rect.sizeDelta = new Vector2(500f, 740f);
            rect.localScale = Vector3.one * 0.001f;
            panel.GetComponent<Image>().color = Fondo;

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 30, 30);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Texto(panel.transform, "Titulo", "Pausa", 52f, FontStyles.Bold, 70f);
            Boton(panel.transform, "Boton Continuar", "Continuar", ColorBoton, menu.Continuar);
            Boton(panel.transform, "Boton Reiniciar", "Reiniciar", ColorBoton, menu.Reiniciar);
            if (altura != null) FilaAltura(panel.transform, altura);
            Boton(panel.transform, "Boton Salir", "Salir del juego", ColorSalir, menu.Salir);
            Texto(panel.transform, "Controles", TextoControles, 21f, FontStyles.Normal, 170f).alignment = TextAlignmentOptions.TopLeft;

            foreach (var t in panel.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = CapaUI;
            panel.SetActive(false);

            // ---- Referencias de MenuPausa ----
            var so = new SerializedObject(menu);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("cabeza").objectReferenceValue = origen.Camera.transform;
            var locomocion = origen.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Locomotion");
            var apagar = so.FindProperty("apagarEnPausa");
            apagar.arraySize = locomocion != null ? 1 : 0;
            if (locomocion != null) apagar.GetArrayElementAtIndex(0).objectReferenceValue = locomocion.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("[Menu] Menú de pausa listo: botón de menú del mando izquierdo, o Esc / P en el editor.");
        }

        // Un solo EventSystem y con el módulo de entrada de XR (el de ratón/teclado no sirve con los mandos)
        static void AsegurarEventSystem()
        {
            var sistema = Object.FindFirstObjectByType<EventSystem>();
            if (sistema == null) sistema = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            foreach (var modulo in sistema.GetComponents<BaseInputModule>())
                if (!(modulo is XRUIInputModule)) Object.DestroyImmediate(modulo);
            if (sistema.GetComponent<XRUIInputModule>() == null) sistema.gameObject.AddComponent<XRUIInputModule>();
        }

        // Pone AlturaAsiento en el mismo objeto que Recentrar y le pasa sus referencias
        static AlturaAsiento AsegurarAlturaAsiento()
        {
            var recentrar = Object.FindFirstObjectByType<Recentrar>();
            if (recentrar == null) { Debug.LogWarning("[Menu] No hay Recentrar en la escena: el menú queda sin altura del asiento."); return null; }
            var altura = recentrar.GetComponent<AlturaAsiento>();
            if (altura == null) altura = recentrar.gameObject.AddComponent<AlturaAsiento>();
            var so = new SerializedObject(altura);
            so.FindProperty("origen").objectReferenceValue = recentrar.xrOrigin;
            so.FindProperty("puntoOjos").objectReferenceValue = recentrar.puntoOjos;
            so.ApplyModifiedPropertiesWithoutUndo();
            return altura;
        }

        // [Bajar]  Altura de los ojos: 1.40 m  [Subir]
        static void FilaAltura(Transform padre, AlturaAsiento altura)
        {
            var fila = new GameObject("Altura del asiento", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            fila.transform.SetParent(padre, false);
            fila.GetComponent<LayoutElement>().preferredHeight = 80f;
            var layout = fila.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            Boton(fila.transform, "Boton Bajar", "Bajar", ColorBoton, altura.Bajar).GetComponent<LayoutElement>().preferredWidth = 110f;
            var etiqueta = Texto(fila.transform, "Altura", "Altura de los ojos", 24f, FontStyles.Normal, 80f);
            etiqueta.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Boton(fila.transform, "Boton Subir", "Subir", ColorBoton, altura.Subir).GetComponent<LayoutElement>().preferredWidth = 110f;

            var so = new SerializedObject(altura);
            so.FindProperty("etiqueta").objectReferenceValue = etiqueta;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TextMeshProUGUI Texto(Transform padre, string nombre, string contenido, float tamano, FontStyles estilo, float alto)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(padre, false);
            go.GetComponent<LayoutElement>().preferredHeight = alto;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = contenido;
            tmp.fontSize = tamano;
            tmp.fontStyle = estilo;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        static GameObject Boton(Transform padre, string nombre, string etiqueta, Color color, UnityAction alPresionar)
        {
            var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            go.name = nombre;
            go.transform.SetParent(padre, false);
            go.AddComponent<LayoutElement>().preferredHeight = 80f;
            go.GetComponent<Image>().color = color;

            var texto = go.GetComponentInChildren<TextMeshProUGUI>();
            texto.text = etiqueta;
            texto.fontSize = 34f;
            texto.color = Color.white;

            var boton = go.GetComponent<Button>();
            var colores = boton.colors;
            colores.normalColor = new Color(0.8f, 0.8f, 0.8f);  // un poco apagado…
            colores.highlightedColor = Color.white;             // …y se aclara al apuntarle
            colores.selectedColor = new Color(0.8f, 0.8f, 0.8f);
            colores.pressedColor = new Color(0.6f, 0.6f, 0.6f);
            boton.colors = colores;
            UnityEventTools.AddPersistentListener(boton.onClick, alPresionar);
            return go;
        }
    }
}
