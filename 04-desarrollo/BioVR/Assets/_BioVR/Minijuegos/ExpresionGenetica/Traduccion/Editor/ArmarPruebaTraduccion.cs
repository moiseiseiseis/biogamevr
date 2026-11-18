using System.Linq;
using BioVR.Jugador;
using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.ExpresionGenetica.Herramientas
{
    // Arma la escena de prueba de I4 (Iker, rol 3): Escenas/Pruebas/Rol3_Traduccion.unity.
    // No toca la escena del minijuego de Moisés ni su armador; cuando esto funcione, Moisés lo integra (Mo3).
    //
    //   - Datos (solo si faltan, para no borrar cambios de G4): codones, secuencias Sana y Paciente, textos
    //   - XR Origin sin locomoción y una "Estacion" (mesa, ribosoma, ARNt, botones Reiniciar, Continuar y Recentrar)
    //     que se acomoda a la cabeza del jugador al iniciar (AnclarAlJugador)
    //   - Ribosoma 80S de Diseño (Diego, PR #60, PDB 4UG0), semitransparente para ver el ARNt adentro.
    //     Si falta el FBX, queda el ribosoma provisional de 2 cajas (P3: arranca armado)
    //   - Cinta de ARNm (se arma en el juego), sitio de lectura (RanuraSeleccion de Mo2), cadena y factor de liberación
    //   - 4 ARNt (Met, Gly, Phe y el distractor Gln, D3), cada uno con su aminoácido y su anticodón. El cuerpo es el
    //     ARNt de Diego (PR #60, PDB 1EHZ) para los 4; si falta, los proxies del repo
    //
    // Se puede correr varias veces: la escena se vuelve a generar desde cero (también la Estacion).
    // Menú: BioVR → Armar prueba Traducción (Rol 3)
    public static class ArmarPruebaTraduccion
    {
        const string Minijuego = "Assets/_BioVR/Minijuegos/ExpresionGenetica";
        const string Carpeta = Minijuego + "/Traduccion";
        const string Datos = Carpeta + "/Datos";
        const string Materiales = Carpeta + "/Materiales";
        const string Prefabs = Minijuego + "/Prefabs";
        // Modelos de Diseño (Diego, PR #60; copias de 03-diseno/exports/)
        const string ModeloRibosoma = Minijuego + "/Modelos/Diseno/ribosoma_80S_4ug0.fbx";
        const string ModeloARNt = Minijuego + "/Modelos/Diseno/arnt_modelo.fbx";
        const float AnchoRibosoma = 0.45f;     // escala didáctica: cabe en la mesa sin tapar los ARNt
        const float OpacidadRibosoma = 0.3f;   // deja ver el ARNt y el codón dentro del ribosoma
        public const string Escena = "Assets/_BioVR/Escenas/Pruebas/Rol3_Traduccion.unity";
        const string PrefabOrigen = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string PrefabBoton = "Assets/_BioVR/Cabina/Button1.prefab";

        static readonly string[] Locomocion = { "Turn", "Move", "Grab Move", "Teleportation", "Climb", "Gravity", "Jump" };

        // Medidas (m). Escala didáctica: nada de esto es real.
        const float AlturaMesa = 0.72f;
        // Ojos del jugador con los que se arma todo. En el juego la estación se mueve para que este punto
        // quede en la cabeza real (AnclarAlJugador): mesa 53 cm abajo de los ojos, codón 25 cm abajo y 62 cm al frente.
        static readonly Vector3 OjosDeReferencia = new Vector3(0f, 1.25f, 0f);
        const float AlturaARNt = 0.16f;
        const float TamanoAminoacido = 0.045f;
        const float TamanoFactor = 0.17f;
        static readonly Vector3 PosicionSitio = new Vector3(0f, 1.0f, 0.62f);  // centro del codón en lectura
        const float MitadCodon = 0.02f;                                         // alto del codón / 2

        static readonly Color ColorTexto = new Color(0.08f, 0.1f, 0.15f);
        static readonly Color ColorPanel = new Color(0.93f, 0.92f, 0.88f);

        // (proxy del ARNt, proxy del aminoácido, codón, anticodón, aminoácido, posición en el "estante")
        static readonly (string, string, string, string, string, Vector3)[] ARNts =
        {
            ("ARNt_Met_Proxy", "Met_Proxy", "AUG", "UAC", "Met", new Vector3(-0.55f, 1.15f, 0.95f)),
            ("ARNt_Gly_Proxy", "Gly_Proxy", "GGC", "CCG", "Gly", new Vector3(0.32f, 1.15f, 0.95f)),
            ("ARNt_Phe_Proxy", "Phe_Proxy", "UUU", "AAA", "Phe", new Vector3(-0.32f, 1.15f, 0.95f)),
            ("ARNt_Gln_Proxy", null,        "CAG", "GUC", "Gln", new Vector3(0.55f, 1.15f, 0.95f)),
        };

        [MenuItem("BioVR/Armar prueba Traducción (Rol 3)")]
        public static void Armar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Carpetas();

            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Object.DestroyImmediate(GameObject.Find("Main Camera"));
            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            // ---- Datos (no se sobrescriben si ya existen) ----
            // Se cargan DESPUÉS de crear la escena: al crearla, Unity descarga los assets que nadie usa
            // y las referencias cargadas antes quedarían vacías (igual que en CrearTutorial)
            var aug = Codon("AUG", "UAC", "Met", false);
            var ggc = Codon("GGC", "CCG", "Gly", false);
            var uuu = Codon("UUU", "AAA", "Phe", false);
            var cag = Codon("CAG", "GUC", "Gln", false);
            var uaa = Codon("UAA", "", "", true);
            var uag = Codon("UAG", "", "", true);
            var sana = Secuencia("Sana", aug, ggc, uuu, uaa);
            var paciente = Secuencia("Paciente", aug, ggc, uag);
            var textos = Asset<TextosTraduccion>(Datos + "/TextosTraduccion.asset", null);
            var porCodon = new[] { aug, ggc, uuu, cag };
            AssetDatabase.SaveAssets();


            // ---- Jugador sentado ----
            var origen = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigen), escena);
            if (origen == null) { Debug.LogError("[Traduccion] No se encontró el XR Origin de los Starter Assets."); return; }
            foreach (var t in origen.GetComponentsInChildren<Transform>(true))
                if ((Locomocion.Contains(t.name) && t.parent != null && t.parent.name == "Locomotion") || t.name == "Teleport Interactor")
                    t.gameObject.SetActive(false);

            // ---- Sala ----
            Primitiva("Piso", PrimitiveType.Plane, null, Vector3.zero, Vector3.one, Material("Piso", new Color(0.32f, 0.34f, 0.38f)));

            // ---- Estación: todo lo que el jugador usa. Se arma con el jugador en OjosDeReferencia y en el juego
            // se acomoda a su cabeza real (AnclarAlJugador). El piso se queda fijo, igual que el piso real. ----
            var estacion = new GameObject("Estacion");
            var puntoOjos = new GameObject("Punto ojos").transform;
            puntoOjos.SetParent(estacion.transform, false);
            puntoOjos.position = OjosDeReferencia;
            Primitiva("Mesa", PrimitiveType.Cube, estacion.transform, new Vector3(0f, AlturaMesa / 2f, 0.42f),
                new Vector3(1.0f, AlturaMesa, 0.36f), Material("Mesa", new Color(0.78f, 0.66f, 0.5f)));

            var raiz = new GameObject("Traduccion");
            raiz.transform.SetParent(estacion.transform, false);
            var secuencia = raiz.AddComponent<SecuenciaTraduccion>();

            // ---- Ribosoma: el de Diseño o, si falta, el provisional (P3: armado desde el inicio) ----
            var (mayor, menor) = RibosomaDiseno(raiz.transform);
            if (mayor == null)
            {
                var ribosoma = new GameObject("Ribosoma (provisional)").transform;
                ribosoma.SetParent(raiz.transform, false);
                var matRibosoma = Material("Ribosoma", new Color(0.55f, 0.45f, 0.75f));
                mayor = Primitiva("Subunidad mayor", PrimitiveType.Cube, ribosoma,
                    PosicionSitio + new Vector3(0f, 0.09f, 0.1f), new Vector3(0.34f, 0.2f, 0.1f), matRibosoma);
                menor = Primitiva("Subunidad menor", PrimitiveType.Cube, ribosoma,
                    PosicionSitio + new Vector3(0f, -0.045f, 0.01f), new Vector3(0.28f, 0.05f, 0.12f), matRibosoma, conCollider: false);
            }
            var senalRibosoma = mayor.AddComponent<Senalable>();

            // ---- Cinta de ARNm (los codones se crean en el juego) ----
            var tiraGo = new GameObject("Cinta de ARNm");
            tiraGo.transform.SetParent(raiz.transform, false);
            tiraGo.transform.position = PosicionSitio;
            var tira = tiraGo.AddComponent<TiraARNm>();
            var tso = new SerializedObject(tira);
            tso.FindProperty("materialCodon").objectReferenceValue = Material("Codon", new Color(0.92f, 0.9f, 0.84f));
            tso.FindProperty("materialCinta").objectReferenceValue = Material("Cinta ARNm", new Color(0.95f, 0.72f, 0.45f));
            tso.ApplyModifiedPropertiesWithoutUndo();

            // ---- Sitio de lectura: RanuraSeleccion de Mo2, con la clave del codón activo ----
            var sitio = new GameObject("Sitio de lectura");
            sitio.transform.SetParent(raiz.transform, false);
            sitio.transform.position = PosicionSitio + Vector3.up * MitadCodon; // el ARNt se para sobre el codón
            var zona = sitio.AddComponent<SphereCollider>();
            zona.isTrigger = true;
            zona.radius = 0.13f; // antes 0.09: Met "a veces no encaja" (prueba de Marcos, 13 nov)
            zona.center = new Vector3(0f, AlturaARNt / 2f, 0f);
            var socket = sitio.AddComponent<XRSocketInteractor>();
            socket.showInteractableHoverMeshes = false;
            // Marca del sitio: una placa detrás del codón activo, para no tapar su texto
            var indicador = Primitiva("Indicador", PrimitiveType.Cube, sitio.transform,
                PosicionSitio + new Vector3(0f, 0.035f, 0.045f), new Vector3(0.1f, 0.11f, 0.005f),
                Material("Indicador sitio", new Color(0.6f, 0.62f, 0.66f)), conCollider: false);
            var ranura = sitio.AddComponent<RanuraSeleccion>();
            ranura.clave = "";
            var rso = new SerializedObject(ranura);
            rso.FindProperty("indicador").objectReferenceValue = indicador.GetComponent<Renderer>();
            rso.FindProperty("tolerancia").floatValue = 180f; // el ARNt entra en cualquier orientación
            rso.ApplyModifiedPropertiesWithoutUndo();

            // ---- ARNt (D3) ----
            var arnts = new ARNt[ARNts.Length];
            for (int i = 0; i < ARNts.Length; i++)
                arnts[i] = CrearARNt(raiz.transform, ARNts[i], porCodon[i]);

            // ---- Cadena: donde queda el aminoácido de un ARNt acoplado ----
            var cadenaGo = new GameObject("Cadena");
            cadenaGo.transform.SetParent(raiz.transform, false);
            cadenaGo.transform.position = sitio.transform.position + Vector3.up * (AlturaARNt + TamanoAminoacido / 2f + 0.005f);
            var caja = cadenaGo.AddComponent<BoxCollider>();
            caja.enabled = false;
            var cuerpoCadena = cadenaGo.AddComponent<Rigidbody>();
            cuerpoCadena.isKinematic = true;
            cuerpoCadena.useGravity = false;
            var agarreCadena = cadenaGo.AddComponent<XRGrabInteractable>();
            ConfigurarAgarre(agarreCadena);
            agarreCadena.enabled = false; // se activa cuando se suelta la cadena
            var cadena = cadenaGo.AddComponent<CadenaPolipeptidica>();

            // ---- Factor de liberación (entra solo, D1) ----
            var factor = new GameObject("Factor de liberacion").transform;
            factor.SetParent(raiz.transform, false);
            factor.position = new Vector3(0.62f, 1.12f, 0.6f); // a la derecha, cerca y a la vista, sin tapar el ARNt Gln
            var modeloFactor = new GameObject("Modelo").transform;
            modeloFactor.SetParent(factor, false);
            modeloFactor.localPosition = new Vector3(0f, TamanoFactor / 2f, 0f);
            Proxy("FactorLiberacion_Proxy", modeloFactor, TamanoFactor, PrimitiveType.Capsule, "Factor de liberacion");
            Etiqueta(factor, "Factor de\nliberación", new Vector3(0f, TamanoFactor + 0.03f, 0f), 0.14f, new Vector2(0.2f, 0.06f));

            // ---- Letrero de la cabina ----
            // Más cerca y un poco más abajo que antes (y 1.5 m, a 1.25 m): Marcos no encontraba las instrucciones.
            // Mismo tamaño aparente; queda detrás y arriba del ribosoma.
            var letrero = Texto("Letrero", raiz.transform, "", 0.34f, new Vector2(1.05f, 0.25f));
            letrero.transform.position = new Vector3(0f, 1.45f, 1.0f);
            var fondo = Primitiva("Fondo", PrimitiveType.Quad, letrero.transform, Vector3.zero, new Vector3(1.12f, 0.31f, 1f),
                Material("Panel", ColorPanel), conCollider: false);
            fondo.transform.localPosition = new Vector3(0f, 0f, 0.005f);

            // ---- Aviso fijo de escala didáctica (Mo7 #12, M4) ----
            var aviso = new GameObject("Aviso escala didactica").transform;
            aviso.SetParent(raiz.transform, false);
            aviso.position = new Vector3(-0.62f, 1.2f, 0.55f); // a la izquierda, cerca y casi a la altura de los ojos, sin tapar el ARNt Met
            aviso.rotation = Quaternion.Euler(0f, -48f, 0f); // de frente al jugador
            var fondoAviso = Primitiva("Fondo", PrimitiveType.Quad, aviso, Vector3.zero, new Vector3(0.38f, 0.13f, 1f),
                Material("Panel", ColorPanel), conCollider: false);
            fondoAviso.transform.localPosition = new Vector3(0f, 0f, 0.005f);
            fondoAviso.transform.localRotation = Quaternion.identity;
            var textoAviso = Texto("Texto", aviso, "<b>Escala didáctica</b>\nTamaños, colores y secuencia simplificados.", 0.17f, new Vector2(0.36f, 0.12f));
            textoAviso.transform.localPosition = Vector3.zero;

            // ---- Botones: Reiniciar y Continuar ----
            var reiniciar = Boton(escena, raiz.transform, "Reiniciar", new Vector3(0.38f, AlturaMesa + 0.005f, 0.33f));
            if (reiniciar.Item2 != null) UnityEventTools.AddVoidPersistentListener(reiniciar.Item2.selectEntered, secuencia.Reiniciar);
            var continuar = Boton(escena, raiz.transform, "Continuar", new Vector3(-0.38f, AlturaMesa + 0.005f, 0.33f));
            if (continuar.Item2 != null) UnityEventTools.AddVoidPersistentListener(continuar.Item2.selectEntered, secuencia.Continuar);

            // ---- Anclaje a la cabeza y recentrado (B / Y o el botón de la mesa) ----
            var xrOrigin = origen.GetComponent<XROrigin>();
            var anclar = origen.AddComponent<AnclarAlJugador>();
            var anso = new SerializedObject(anclar);
            anso.FindProperty("origen").objectReferenceValue = xrOrigin;
            anso.FindProperty("estacion").objectReferenceValue = estacion;
            anso.FindProperty("puntoOjos").objectReferenceValue = puntoOjos;
            anso.ApplyModifiedPropertiesWithoutUndo();
            var recentrar = origen.AddComponent<Recentrar>();
            recentrar.xrOrigin = xrOrigin;
            recentrar.puntoOjos = puntoOjos;
            var botonRecentrar = Boton(escena, raiz.transform, "Recentrar", new Vector3(0f, AlturaMesa + 0.005f, 0.3f));
            if (botonRecentrar.Item2 != null) UnityEventTools.AddVoidPersistentListener(botonRecentrar.Item2.selectEntered, recentrar.Centrar);

            // ---- Referencias de la secuencia ----
            var so = new SerializedObject(secuencia);
            so.FindProperty("secuenciaSana").objectReferenceValue = sana;
            so.FindProperty("secuenciaPaciente").objectReferenceValue = paciente;
            so.FindProperty("textos").objectReferenceValue = textos;
            so.FindProperty("letrero").objectReferenceValue = letrero;
            so.FindProperty("sitioLectura").objectReferenceValue = ranura;
            so.FindProperty("tira").objectReferenceValue = tira;
            so.FindProperty("ribosoma").objectReferenceValue = senalRibosoma;
            so.FindProperty("subunidadMayor").objectReferenceValue = mayor.transform;
            so.FindProperty("subunidadMenor").objectReferenceValue = menor.transform;
            so.FindProperty("cadena").objectReferenceValue = cadena;
            so.FindProperty("factorLiberacion").objectReferenceValue = factor;
            so.FindProperty("botonContinuar").objectReferenceValue = continuar.Item1;
            var lista = so.FindProperty("arnts");
            lista.arraySize = arnts.Length;
            for (int i = 0; i < arnts.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = arnts[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            estacion.SetActive(false); // la enciende AnclarAlJugador cuando ya sabe dónde está la cabeza

            if (!EditorSceneManager.SaveScene(escena, Escena)) { Debug.LogError("[Traduccion] No se pudo guardar la escena."); return; }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Traduccion] Listo: {Escena}. Dale Play. Teclas: N = completar paso, R = reiniciar, P = ir al paciente.");
        }

        // ---------------------------------------------------------------------------------------------
        static ARNt CrearARNt(Transform padre, (string, string, string, string, string, Vector3) datos, CodonData codon)
        {
            var (proxy, proxyAminoacido, clave, anticodon, aminoacido, posicion) = datos;
            var raiz = new GameObject("ARNt " + aminoacido);
            raiz.transform.SetParent(padre, false);
            raiz.transform.position = posicion; // el origen es la base (anticodón), para pararse sobre el codón

            var modelo = new GameObject("Modelo").transform;
            modelo.SetParent(raiz.transform, false);
            modelo.localPosition = new Vector3(0f, AlturaARNt / 2f, 0f);
            var cuerpo3D = ARNtDiseno(modelo, out var puntaAceptora)
                ?? Proxy(proxy, modelo, AlturaARNt, PrimitiveType.Capsule, "ARNt " + aminoacido);

            // Aminoácido que trae cargado (ficha 4), arriba
            var aa = new GameObject("Aminoacido " + aminoacido).transform;
            aa.SetParent(modelo, false);
            aa.localPosition = new Vector3(0f, AlturaARNt / 2f + TamanoAminoacido / 2f + 0.005f, 0f);
            if (puntaAceptora.HasValue) aa.position = puntaAceptora.Value; // donde el ARNt de Diseño trae su aminoácido
            if (proxyAminoacido != null) Proxy(proxyAminoacido, aa, TamanoAminoacido, PrimitiveType.Sphere, aminoacido);
            else
            {
                var esfera = Primitiva(aminoacido, PrimitiveType.Sphere, aa, aa.position, Vector3.one * TamanoAminoacido,
                    AssetDatabase.LoadAssetAtPath<Material>(Minijuego + "/Materiales/M_Glutamina.mat") ?? Material("Gln", new Color(0.9f, 0.6f, 0.2f)),
                    conCollider: false);
                esfera.transform.localPosition = Vector3.zero;
            }
            Etiqueta(aa, aminoacido, new Vector3(0f, 0f, -TamanoAminoacido / 2f - 0.004f), 0.14f, new Vector2(0.08f, 0.03f));

            // Anticodón visible abajo, de 3' a 5' (respuesta de Mo8)
            var limites = Limites(cuerpo3D);
            Etiqueta(modelo, $"3'-{anticodon}-5'", new Vector3(0f, -AlturaARNt / 2f + 0.012f, -limites.extents.z - 0.004f), 0.15f, new Vector2(0.14f, 0.03f));

            // Collider para agarrarlo: cubre el ARNt y su aminoácido
            var todo = Limites(modelo.gameObject);
            var col = modelo.gameObject.AddComponent<BoxCollider>();
            col.center = modelo.InverseTransformPoint(todo.center);
            col.size = todo.size;

            var rb = raiz.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.useGravity = false;        // los ARNt flotan quietos (Mo7 #16, didáctico)
            rb.linearDamping = 4f;
            rb.angularDamping = 4f;
            var agarre = raiz.AddComponent<XRGrabInteractable>();
            ConfigurarAgarre(agarre);

            var pieza = raiz.AddComponent<PiezaManipulable>();
            pieza.clave = clave;          // el sitio de lectura acepta el ARNt cuyo codón coincide
            pieza.modelo = modelo;
            pieza.marca = null;           // sin orientación obligatoria

            var arnt = raiz.AddComponent<ARNt>();
            arnt.codon = codon;
            arnt.aminoacido = aa;
            return arnt;
        }

        // ---- Modelos de Diseño (Diego, PR #60) ----

        // Ribosoma 80S (PDB 4UG0). Se escala a AnchoRibosoma y se acomoda con sus marcadores: la línea del ARNm
        // (ARNm_5prima) pasa por el codón en lectura y el Sitio_A queda sobre el sitio de lectura.
        // Devuelve (subunidad mayor 60S, subunidad menor 40S), o (null, null) si falta el modelo.
        static (GameObject, GameObject) RibosomaDiseno(Transform padre)
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModeloRibosoma);
            if (fbx == null) { Debug.LogWarning($"[Traduccion] No está {ModeloRibosoma}; uso el ribosoma provisional."); return (null, null); }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, padre);
            go.name = "Ribosoma 80S (Diseño)";
            var t = go.transform;
            t.localPosition = Vector3.zero;
            // Unity invierte X al importar: con el giro de 180° el extremo 5' queda a la izquierda, como la cinta
            t.localRotation = Quaternion.Euler(0f, 180f, 0f);
            t.localScale = Vector3.one;

            var s60 = Hijo(t, "Ribosoma_60S");
            var s40 = Hijo(t, "Ribosoma_40S");
            var sitioA = Hijo(t, "Sitio_A");
            var arnm = Hijo(t, "ARNm_5prima");
            if (s60 == null || s40 == null || sitioA == null || arnm == null)
            {
                Debug.LogWarning("[Traduccion] El ribosoma de Diseño no trae Ribosoma_60S/40S, Sitio_A o ARNm_5prima; uso el provisional.");
                Object.DestroyImmediate(go);
                return (null, null);
            }

            float ancho = Limites(go).size.x;
            if (ancho > 0.0001f) t.localScale = Vector3.one * (AnchoRibosoma / ancho);
            var ancla = new Vector3(sitioA.position.x, arnm.position.y, sitioA.position.z);
            t.position += PosicionSitio - ancla;

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.sharedMaterials = r.sharedMaterials.Select(Transparente).ToArray();
            }
            // Collider para señalarlo (paso 1). Senalable lo apaga mientras no toca, para no tapar los codones
            var filtro = s60.GetComponent<MeshFilter>();
            var col = s60.gameObject.AddComponent<MeshCollider>();
            if (filtro != null) col.sharedMesh = filtro.sharedMesh;
            return (s60.gameObject, s40.gameObject);
        }

        // ARNt (PDB 1EHZ, viene con Phe). Se usa el mismo cuerpo para los 4: se oculta su aminoácido (cada ARNt
        // lleva el suyo, que es el que pasa a la cadena) y las bases van del color del esqueleto, porque los colores
        // de su anticodón (el de Phe) contradirían la etiqueta de los otros tres.
        // puntaAceptora: dónde traía su aminoácido (extremo 3' aceptor), para poner ahí el de cada ARNt.
        static GameObject ARNtDiseno(Transform padre, out Vector3? puntaAceptora)
        {
            puntaAceptora = null;
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModeloARNt);
            if (fbx == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, padre);
            go.name = "ARNt (Diseño)";
            var t = go.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;

            Material matEsqueleto = null;
            var esqueleto = Hijo(t, "ARNt_Esqueleto");
            if (esqueleto != null && esqueleto.TryGetComponent<Renderer>(out var rEsqueleto)) matEsqueleto = rEsqueleto.sharedMaterial;

            var b = Limites(go);
            float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (mayor > 0.0001f) t.localScale = Vector3.one * (AlturaARNt / mayor);
            b = Limites(go);
            t.position += padre.position - b.center;

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (r.name.StartsWith("ARNt_Aminoacido"))
                {
                    puntaAceptora = r.bounds.center;
                    r.gameObject.SetActive(false);
                }
                else if (r.name == "ARNt_Bases" && matEsqueleto != null)
                    r.sharedMaterials = Enumerable.Repeat(matEsqueleto, r.sharedMaterials.Length).ToArray();
            }
            return go;
        }

        static Transform Hijo(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == nombre);

        // Copia transparente (URP Lit) de un material del FBX, guardada en Materiales/ para no tocar el FBX
        static Material Transparente(Material original)
        {
            var nombre = "Ribosoma " + (original != null ? original.name : "sin material");
            var color = new Color(0.6f, 0.5f, 0.75f);
            if (original != null && original.HasProperty("_BaseColor")) color = original.GetColor("_BaseColor");
            else if (original != null && original.HasProperty("_Color")) color = original.GetColor("_Color");
            color.a = OpacidadRibosoma;
            var mat = Material(nombre, color);
            mat.SetFloat("_Surface", 1f);   // Transparent
            mat.SetFloat("_Blend", 0f);     // Alpha
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void ConfigurarAgarre(XRGrabInteractable agarre)
        {
            // Igual que la mesa de prueba de Moisés (Mo2)
            agarre.farAttachMode = InteractableFarAttachMode.Near;
            agarre.movementType = XRBaseInteractable.MovementType.Kinematic;
            agarre.useDynamicAttach = true;
            agarre.matchAttachPosition = false;
            agarre.matchAttachRotation = false;
            agarre.snapToColliderVolume = false;
            agarre.throwOnDetach = false;
        }

        // Instancia un proxy del repo (o una primitiva si no está), lo escala a "tamano" y lo centra en el padre
        static GameObject Proxy(string nombre, Transform padre, float tamano, PrimitiveType respaldo, string nombreRespaldo)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Prefabs}/{nombre}.prefab");
            GameObject go;
            if (prefab != null) go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
            else
            {
                Debug.LogWarning($"[Traduccion] No está {nombre}.prefab; uso una primitiva provisional.");
                go = GameObject.CreatePrimitive(respaldo);
                go.name = nombreRespaldo;
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(padre, false);
            }
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var b = Limites(go);
            float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (mayor > 0.0001f) go.transform.localScale = Vector3.one * (tamano / mayor);
            b = Limites(go);
            go.transform.position += padre.position - b.center;
            if (go.GetComponentsInChildren<Collider>().Length > 0)
                Debug.LogWarning($"[Traduccion] {nombre} trae colliders propios; pueden estorbar al agarrarlo.");
            return go;
        }

        static Bounds Limites(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static (GameObject, XRSimpleInteractable) Boton(UnityEngine.SceneManagement.Scene escena, Transform padre, string texto, Vector3 posicion)
        {
            var grupo = new GameObject("Boton " + texto);
            grupo.transform.SetParent(padre, false);
            var boton = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBoton), escena);
            if (boton == null) { Debug.LogWarning("[Traduccion] No se encontró Button1.prefab; la escena queda sin " + texto + "."); return (grupo, null); }
            boton.name = "Boton";
            boton.transform.SetParent(grupo.transform, false);
            boton.transform.position = posicion;
            var etiqueta = Texto("Etiqueta", grupo.transform, texto, 0.35f, new Vector2(0.3f, 0.06f));
            etiqueta.transform.position = posicion + new Vector3(0f, 0f, -0.07f);
            etiqueta.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            return (grupo, boton.GetComponent<XRSimpleInteractable>());
        }

        static void Etiqueta(Transform padre, string texto, Vector3 posicionLocal, float tamano, Vector2 caja)
        {
            var t = Texto("Etiqueta " + texto.Replace("\n", " "), padre, texto, tamano, caja);
            t.transform.localPosition = posicionLocal;
            t.transform.localRotation = Quaternion.identity;
            t.fontStyle = FontStyles.Bold;
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

        // ---- Assets ----
        static void Carpetas()
        {
            foreach (var ruta in new[] { Carpeta, Datos, Materiales })
                if (!AssetDatabase.IsValidFolder(ruta))
                {
                    var i = ruta.LastIndexOf('/');
                    AssetDatabase.CreateFolder(ruta.Substring(0, i), ruta.Substring(i + 1));
                }
        }

        static T Asset<T>(string ruta, System.Action<T> iniciar) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(ruta);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<T>();
            iniciar?.Invoke(a);
            AssetDatabase.CreateAsset(a, ruta);
            return a;
        }

        static CodonData Codon(string codon, string anticodon, string aminoacido, bool paro) =>
            Asset<CodonData>($"{Datos}/Codon_{codon}.asset", c =>
            {
                c.codon = codon;
                c.anticodon = anticodon;
                c.aminoacido = aminoacido;
                c.esParo = paro;
                // Todos del mismo color: si el paro tuviera otro color, el ciclo del paciente se resolvería solo
            });

        // La secuencia sí se reescribe siempre: es la del caso (D2), no un texto que alguien edite
        static SecuenciaARNm Secuencia(string nombre, params CodonData[] codones)
        {
            var s = Asset<SecuenciaARNm>($"{Datos}/Secuencia_{nombre}.asset", null);
            s.nombre = nombre;
            s.codones = codones;
            EditorUtility.SetDirty(s);
            return s;
        }

        static Material Material(string nombre, Color color)
        {
            var ruta = $"{Materiales}/{nombre}.mat";
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
    }
}
