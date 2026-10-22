using System.Linq;
using BioVR.Jugador;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BioVR.Comun.Editor
{
    // Arma el menú de pausa en 00-EscenaInicial:
    //   - EventSystem con XRUIInputModule (para que los rayos de los mandos puedan usar la UI)
    //   - "Menu de pausa" con MenuPausa y un Canvas world space: título, Continuar, Reiniciar,
    //     altura del asiento (Bajar / Subir), Salir y los controles
    //   - El panel de confirmación "¿Estás seguro?" (Sí / No) que sale antes de Reiniciar y Salir
    //   - AlturaAsiento junto a Recentrar (mismo XR Origin y Punto ojos), si no estaba
    // Se puede correr varias veces: borra el menú anterior y lo vuelve a crear.
    // Menú: BioVR → Agregar menú de pausa
    public static class AgregarMenuPausa
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string NombreMenu = "Menu de pausa";

        // Lo que el jugador puede hacer; se muestra abajo de los botones
        public const string TextoControles =
            "<b>Controles</b>\n" +
            "Stick izquierdo: caminar · Stick derecho: girar\n" +
            "Botones de la cabina: tócalos con la mano\n" +
            "Palanca: agárrala con el botón lateral (grip)\n" +
            "Este menú: apunta y usa el gatillo\n" +
            "B / Y: volver al asiento · Menú (mando izq.): pausa";

        [MenuItem("BioVR/Agregar menú de pausa")]
        public static void Agregar()
        {
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            var origen = Object.FindFirstObjectByType<XROrigin>();
            if (origen == null || origen.Camera == null) { Debug.LogError("[Menu] No hay XR Origin con cámara en la escena inicial."); return; }

            UIVR.AsegurarEventSystem();
            var altura = AsegurarAlturaAsiento();

            var viejo = escena.GetRootGameObjects().FirstOrDefault(g => g.name == NombreMenu);
            if (viejo != null) Object.DestroyImmediate(viejo);

            var raiz = new GameObject(NombreMenu);
            var menu = raiz.AddComponent<MenuPausa>();

            var panel = UIVR.Panel("Panel", raiz.transform, new Vector2(500f, 740f), origen.Camera); // 0.5 × 0.74 m
            UIVR.Texto(panel.transform, "Titulo", "Pausa", 52f, FontStyles.Bold, 70f);
            UIVR.Boton(panel.transform, "Boton Continuar", "Continuar", UIVR.ColorBoton, menu.Continuar);
            UIVR.Boton(panel.transform, "Boton Reiniciar", "Reiniciar", UIVR.ColorBoton, menu.Reiniciar);
            if (altura != null) FilaAltura(panel.transform, altura);
            UIVR.Boton(panel.transform, "Boton Salir", "Salir del juego", UIVR.ColorSalir, menu.Salir);
            UIVR.Texto(panel.transform, "Controles", TextoControles, 21f, FontStyles.Normal, 170f).alignment = TextAlignmentOptions.TopLeft;
            UIVR.CapaDeUI(panel);
            panel.SetActive(false);

            var confirmacion = PanelConfirmacion(raiz.transform, menu, origen.Camera);

            // ---- Referencias de MenuPausa ----
            var so = new SerializedObject(menu);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("panelConfirmacion").objectReferenceValue = confirmacion;
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

        // ¿Estás seguro de realizar esta acción?  [Sí] [No]   (lo usan Reiniciar y Salir; diseño de Genaro)
        static GameObject PanelConfirmacion(Transform padre, MenuPausa menu, Camera camara)
        {
            var panel = UIVR.Panel("Panel confirmacion", padre, new Vector2(500f, 260f), camara); // 0.5 × 0.26 m
            UIVR.Texto(panel.transform, "Pregunta", "¿Estás seguro de realizar esta acción?", 34f, FontStyles.Bold, 100f);
            var fila = UIVR.Fila("Si o No", panel.transform, 80f);
            fila.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
            UIVR.Boton(fila, "Boton Si", "Sí", UIVR.ColorSalir, menu.ConfirmarAccionSi);
            UIVR.Boton(fila, "Boton No", "No", UIVR.ColorBoton, menu.CancelarConfirmacionNo);
            UIVR.CapaDeUI(panel);
            panel.SetActive(false);
            return panel;
        }

        // [Bajar]  Altura de los ojos: 1.40 m  [Subir]
        static void FilaAltura(Transform padre, AlturaAsiento altura)
        {
            var fila = UIVR.Fila("Altura del asiento", padre, 80f);
            UIVR.Boton(fila, "Boton Bajar", "Bajar", UIVR.ColorBoton, altura.Bajar).GetComponent<LayoutElement>().preferredWidth = 110f;
            var etiqueta = UIVR.Texto(fila, "Altura", "Altura de los ojos", 24f, FontStyles.Normal, 80f);
            etiqueta.GetComponent<LayoutElement>().flexibleWidth = 1f;
            UIVR.Boton(fila, "Boton Subir", "Subir", UIVR.ColorBoton, altura.Subir).GetComponent<LayoutElement>().preferredWidth = 110f;

            var so = new SerializedObject(altura);
            so.FindProperty("etiqueta").objectReferenceValue = etiqueta;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
