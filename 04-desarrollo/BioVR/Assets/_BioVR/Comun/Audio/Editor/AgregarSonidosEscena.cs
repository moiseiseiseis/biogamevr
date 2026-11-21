using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BioVR.Comun.Editor
{
    // Menú BioVR → Agregar sonidos a la escena inicial.
    //
    // Pone en 00-EscenaInicial un objeto "Sonidos" con SonidosEscena y le asigna cada audio de Comun/Audio/
    // según la plantilla de banda sonora (tabla en 04-desarrollo/docs/2026-11-21-sonidos-de-la-escena-inicial.md).
    // Se puede correr varias veces: borra el "Sonidos" anterior y lo vuelve a armar.
    // Para cambiar un volumen o un audio, se cambia aquí y se vuelve a correr (o se ajusta en el Inspector).
    public static class AgregarSonidosEscena
    {
        const string EscenaInicial = "Assets/_BioVR/Escenas/00-EscenaInicial.unity";
        const string Audio = "Assets/_BioVR/Comun/Audio/";
        const string AudioCocina = "Assets/_BioVR/Minijuegos/Cocina/Audio/";
        const string Nombre = "Sonidos";

        // Volumen por estado: Hangar, Abordaje, Viaje, Llegada
        static readonly (string nombre, string archivo, float[] volumen, int[] saltos)[] Capas =
        {
            ("Zumbido de la nave", Audio + "AMB/AMB-GEN-02_zumbido-nave-base-loop_v01.wav", V(0.35f, 0.35f, 0.35f, 0.35f), S()),
            ("Sistemas de la nave", Audio + "AMB/AMB-ABO-02_sistemas-reposo-loop_v01.wav", V(0.5f, 0.6f, 0.6f, 0.6f), S()),
            ("Hangar", Audio + "AMB/AMB-HAN-01_hangar-reposo-loop_v01.wav", V(0.7f, 0.3f, 0f, 0f), S()),
            ("Flujo sanguineo", Audio + "AMB/AMB-VAS-01_flujo-sanguineo-loop_v01.wav", V(0f, 0f, 0.6f, 0f), S(1, 2)),
            ("Corriente", Audio + "SFX/SFX-ENT-03_corriente-loop_v01.wav", V(0f, 0f, 0.5f, 0f), S(1)),
            ("Particulas del plasma", Audio + "SFX/SFX-ENT-02_particulas-plasma_v01.wav", V(0f, 0f, 0.7f, 0f), S(1, 2)),
            ("Desplazamiento", Audio + "SFX/SFX-MEC-06_desplazamiento-loop_v01.wav", V(0f, 0f, 0.35f, 0.25f), S()),
            ("Antenas", Audio + "SFX/SFX-MEC-05_antenas-estatica-loop_v01.wav", V(0f, 0f, 0.12f, 0.12f), S()),
            ("Interior celular", AudioCocina + "AMB/AMB-CEL-01_interior-celular-loop_v01.wav", V(0f, 0f, 0.6f, 0.6f), S(3)),
            ("Musica viaje tranquilo", Audio + "MUS/MUS-VAS-01_viaje-tranquilo-loop_v01.wav", V(0f, 0f, 0.4f, 0f), S(1)),
            ("Musica viaje intenso", Audio + "MUS/MUS-VAS-03_viaje-intenso-loop_v01.wav", V(0f, 0f, 0.4f, 0f), S(2)),
            ("Musica celula", Audio + "MUS/MUS-MEN-01_tema-menu_v01.wav", V(0f, 0f, 0.35f, 0.35f), S(3)),
        };

        static readonly (string nombre, string archivo, string[] zonas, string[] objetos, float volumen, float distancia)[] Fijos =
        {
            ("Pantallas del hangar", Audio + "AMB/AMB-HAN-03_pantallas-equipos-3d-loop_v01.wav",
                new[] { "Hangar" }, new[] { "Pantalla", "Torre" }, 0.5f, 15f),
            ("Membrana", Audio + "AMB/AMB-CEL-02_membrana-burbujeo-3d-loop_v01.wav",
                new[] { "Citoplasma", "Llegada" }, new[] { "Membrana_Celular", "Membrana" }, 0.6f, 20f),
            ("Citoesqueleto", Audio + "AMB/AMB-CEL-02_citoesqueleto-crujidos-3d-loop_v01.wav",
                new[] { "Citoplasma", "Llegada" }, new[] { "Citoesqueleto", "Microtubulo", "Actina" }, 0.5f, 20f),
            ("Nucleo", Audio + "AMB/AMB-CEL-02_nucleo-pulso-3d-loop_v01.wav",
                new[] { "Citoplasma", "Llegada" }, new[] { "Nucleo", "Núcleo" }, 0.6f, 30f),
        };

        [MenuItem("BioVR/Agregar sonidos a la escena inicial")]
        public static void Agregar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var escena = EditorSceneManager.OpenScene(EscenaInicial, OpenSceneMode.Single);
            foreach (var viejo in escena.GetRootGameObjects().Where(g => g.name == Nombre).ToArray())
                Object.DestroyImmediate(viejo);

            var go = new GameObject(Nombre);
            var so = new SerializedObject(go.AddComponent<SonidosEscena>());

            var capas = so.FindProperty("capas");
            capas.arraySize = Capas.Length;
            for (int i = 0; i < Capas.Length; i++)
            {
                var c = capas.GetArrayElementAtIndex(i);
                c.FindPropertyRelative("nombre").stringValue = Capas[i].nombre;
                c.FindPropertyRelative("clip").objectReferenceValue = Clip(Capas[i].archivo);
                Flotantes(c.FindPropertyRelative("volumenPorEstado"), Capas[i].volumen);
                Enteros(c.FindPropertyRelative("saltos"), Capas[i].saltos);
            }

            var fijos = so.FindProperty("fijos");
            fijos.arraySize = Fijos.Length;
            for (int i = 0; i < Fijos.Length; i++)
            {
                var f = fijos.GetArrayElementAtIndex(i);
                f.FindPropertyRelative("nombre").stringValue = Fijos[i].nombre;
                f.FindPropertyRelative("clip").objectReferenceValue = Clip(Fijos[i].archivo);
                Textos(f.FindPropertyRelative("zonas"), Fijos[i].zonas);
                Textos(f.FindPropertyRelative("objetos"), Fijos[i].objetos);
                f.FindPropertyRelative("volumen").floatValue = Fijos[i].volumen;
                f.FindPropertyRelative("distanciaMaxima").floatValue = Fijos[i].distancia;
            }

            var esp = so.FindProperty("esporadicos");
            esp.arraySize = 3;
            // SFX-INS-05: golpes de martillo, de metal y la llave neumática, al azar en el hangar
            Esporadico(esp.GetArrayElementAtIndex(0), "Herramientas del hangar",
                Carpeta("SFX", "SFX-INS-05_golpe", "SFX-INS-05_llave"), new[] { true, true, false, false }, S(),
                new Vector2(4f, 10f), "Hangar", new Vector2(4f, 12f), 0.5f);
            // SFX-ENT-01: glóbulos que pasan junto a la cabina en los vasos
            Esporadico(esp.GetArrayElementAtIndex(1), "Globulos junto a la cabina",
                Carpeta("SFX", "SFX-ENT-01"), new[] { false, false, true, false }, S(1),
                new Vector2(2.5f, 6f), "", new Vector2(1.5f, 3.5f), 0.5f);
            // SFX-MEC-07: una molécula roza la nave en el citoplasma
            Esporadico(esp.GetArrayElementAtIndex(2), "Contacto con moleculas",
                Carpeta("SFX", "SFX-MEC-07"), new[] { false, false, true, true }, S(3),
                new Vector2(8f, 18f), "", new Vector2(1f, 2f), 0.4f);

            Asignar(so, "revelacionMecha", "MUS/MUS-HAN-03_revelacion-mecha_v01.wav");
            Asignar(so, "entradaCabina", "MUS/MUS-ABO-01_entrada-cabina_v01.wav");
            Asignar(so, "cinturon", "SFX/SFX-CAB-01_cinturon_v01.wav");
            Asignar(so, "encenderPantallas", "SFX/SFX-CAB-02_encender-consola_v01.wav");
            Asignar(so, "compuerta", "SFX/SFX-MEC-01_compuerta-abre_v01.wav");
            Asignar(so, "repliegue", "SFX/SFX-MEC-04_repliegue-extremidades_v01.wav");
            Asignar(so, "primeraReduccion", "SFX/SFX-ESC-02_reduccion-progresiva_v01.wav");
            Clips(so.FindProperty("reducciones"), new[]
            {
                Clip(Audio + "SFX/SFX-ESC-04_reduccion-2-1um_v01.wav"),
                Clip(Audio + "SFX/SFX-ESC-04_reduccion-3-50nm_v01.wav"),
                Clip(Audio + "SFX/SFX-ESC-04_reduccion-4-5nm_v01.wav"),
            });
            Asignar(so, "reduccionCompletada", "SFX/SFX-ESC-03_reduccion-completada_v01.wav");
            Asignar(so, "entradaCelula", "SFX/SFX-ENT-07_entrada-celula_v01.wav");
            Clips(so.FindProperty("transiciones"), Carpeta("MUS", "MUS-GEN-07"));
            Asignar(so, "musicaCarga", "MUS/MUS-GEN-01_carga-loop_v01.wav");
            Asignar(so, "boton", "SFX/SFX-CAB-03_click-boton_v01.wav");
            Asignar(so, "noDisponible", "UI/UI-FBK-04_no-disponible_v01.wav");
            Asignar(so, "palanca", "SFX/SFX-CAB-04_indicador-luminoso_v01.wav");
            Asignar(so, "ajusteAsiento", "SFX/SFX-CAB-06_matraca-calibracion_v01.wav");
            Asignar(so, "musicaPausa", "MUS/MUS-GEN-02_pausa-loop_v01.wav");
            Asignar(so, "abrirPanel", "UI/UI-NAV-04_abrir-panel_v01.wav");
            so.FindProperty("cerrarPanel").objectReferenceValue = Clip(AudioCocina + "UI/UI-NAV-03_cancelar_v01.wav");
            Asignar(so, "hover", "UI/UI-NAV-01_hover_v01.wav");
            Asignar(so, "tocarPantalla", "UI/UI-INT-04_tocar-pantalla_v01.wav");
            Asignar(so, "pasoTutorial", "UI/UI-FBK-01_acierto_v01.wav");
            Asignar(so, "fichaNueva", "UI/UI-FBK-03_datos-registrados_v01.wav");
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[Sonido] Listo: \"{Nombre}\" en {EscenaInicial} con {Capas.Length} capas, {Fijos.Length} sonidos fijos y 3 esporádicos.");
        }

        // ---------------- Ayudantes ----------------

        static float[] V(params float[] v) => v;
        static int[] S(params int[] s) => s;

        static AudioClip Clip(string ruta)
        {
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
            if (c == null) Debug.LogWarning($"[Sonido] No encontré {ruta}.");
            return c;
        }

        static void Asignar(SerializedObject so, string campo, string archivo) =>
            so.FindProperty(campo).objectReferenceValue = Clip(Audio + archivo);

        // Todos los audios de Comun/Audio/<sub>/ cuyo nombre empiece con alguno de los prefijos
        static AudioClip[] Carpeta(string sub, params string[] prefijos) =>
            AssetDatabase.FindAssets("t:AudioClip", new[] { Audio + sub })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(r => prefijos.Any(p => System.IO.Path.GetFileName(r).StartsWith(p)))
                .OrderBy(r => r)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .ToArray();

        static void Esporadico(SerializedProperty e, string nombre, AudioClip[] clips, bool[] estados, int[] saltos,
            Vector2 intervalo, string zona, Vector2 distancia, float volumen)
        {
            e.FindPropertyRelative("nombre").stringValue = nombre;
            Clips(e.FindPropertyRelative("clips"), clips);
            var est = e.FindPropertyRelative("estados");
            est.arraySize = estados.Length;
            for (int i = 0; i < estados.Length; i++) est.GetArrayElementAtIndex(i).boolValue = estados[i];
            Enteros(e.FindPropertyRelative("saltos"), saltos);
            e.FindPropertyRelative("intervalo").vector2Value = intervalo;
            e.FindPropertyRelative("zona").stringValue = zona;
            e.FindPropertyRelative("distancia").vector2Value = distancia;
            e.FindPropertyRelative("volumen").floatValue = volumen;
            if (clips.Length == 0) Debug.LogWarning($"[Sonido] {nombre}: no encontré sus audios.");
        }

        static void Clips(SerializedProperty p, AudioClip[] clips)
        {
            p.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
        }

        static void Flotantes(SerializedProperty p, float[] v)
        {
            p.arraySize = v.Length;
            for (int i = 0; i < v.Length; i++) p.GetArrayElementAtIndex(i).floatValue = v[i];
        }

        static void Enteros(SerializedProperty p, int[] v)
        {
            p.arraySize = v.Length;
            for (int i = 0; i < v.Length; i++) p.GetArrayElementAtIndex(i).intValue = v[i];
        }

        static void Textos(SerializedProperty p, string[] v)
        {
            p.arraySize = v.Length;
            for (int i = 0; i < v.Length; i++) p.GetArrayElementAtIndex(i).stringValue = v[i];
        }
    }
}
