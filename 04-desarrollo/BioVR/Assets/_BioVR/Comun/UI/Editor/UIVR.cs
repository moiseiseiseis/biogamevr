using TMPro;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace BioVR.Comun.Editor
{
    // Piezas de UI para VR que usan los armadores (menú de pausa, tutorial): un Canvas world space
    // que se puede usar con los rayos de los mandos, textos y botones con el mismo estilo.
    public static class UIVR
    {
        public const int CapaUI = 5; // capa "UI" de Unity

        public static readonly Color Fondo = new Color(0.05f, 0.09f, 0.14f, 0.92f);
        public static readonly Color ColorBoton = new Color(0.16f, 0.45f, 0.7f);
        public static readonly Color ColorSalir = new Color(0.65f, 0.2f, 0.22f);

        // Panel con fondo y acomodo vertical. 1 unidad = 1 mm: un panel de 500 × 700 mide 0.5 × 0.7 m.
        public static GameObject Panel(string nombre, Transform padre, Vector2 tamanoMm, Camera camara)
        {
            var panel = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                                       typeof(TrackedDeviceGraphicRaycaster), typeof(Image));
            panel.transform.SetParent(padre, false);
            var canvas = panel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camara;
            panel.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f; // texto nítido en world space
            var rect = (RectTransform)panel.transform;
            rect.sizeDelta = tamanoMm;
            rect.localScale = Vector3.one * 0.001f;
            panel.GetComponent<Image>().color = Fondo;

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 30, 30);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return panel;
        }

        // Fila horizontal dentro de un panel (p. ej. para poner dos botones lado a lado)
        public static Transform Fila(string nombre, Transform padre, float alto)
        {
            var fila = new GameObject(nombre, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            fila.transform.SetParent(padre, false);
            fila.GetComponent<LayoutElement>().preferredHeight = alto;
            var layout = fila.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            return fila.transform;
        }

        public static TextMeshProUGUI Texto(Transform padre, string nombre, string contenido, float tamano, FontStyles estilo, float alto)
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

        public static GameObject Boton(Transform padre, string nombre, string etiqueta, Color color, UnityAction alPresionar)
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

        // Un solo EventSystem y con el módulo de entrada de XR (el de ratón/teclado no sirve con los mandos)
        public static void AsegurarEventSystem()
        {
            var sistema = Object.FindFirstObjectByType<EventSystem>();
            if (sistema == null) sistema = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            foreach (var modulo in sistema.GetComponents<BaseInputModule>())
                if (!(modulo is XRUIInputModule)) Object.DestroyImmediate(modulo);
            if (sistema.GetComponent<XRUIInputModule>() == null) sistema.gameObject.AddComponent<XRUIInputModule>();
        }

        // Todo el panel en la capa UI (la que usan los rayos para la interfaz)
        public static void CapaDeUI(GameObject panel)
        {
            foreach (var t in panel.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = CapaUI;
        }
    }
}
