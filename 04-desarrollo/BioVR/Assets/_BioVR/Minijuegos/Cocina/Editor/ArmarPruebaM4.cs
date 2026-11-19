using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using BioVR.Minijuegos.ExpresionGenetica;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // M4 (Iker, rol 3) · Menú BioVR → Cocina → Armar prueba M4.
    //
    //   1. Importa de 03-diseno/exports/minijuego-cocina/ los modelos de Uli que usa M4 (arnm-bases.fbx y
    //      aminoacido.fbx) a Cocina/Modelos/Uli/, sin cámaras ni luces (especificación 13).
    //   2. Saca de esos FBX las piezas que M4 instancia (tramo de cinta, bases A/U/G/C, cuenta) como prefabs.
    //   3. Arma Cocina_Ribosoma.prefab (mesa) y Cocina_RibosomaLejano.prefab con el ribosoma 4UG0 de Diego.
    //   4. Arma un ARNt DE PRUEBA (Prefabs/Ribosomas/Pruebas/), que no es el de M2.
    //   5. Abre Escenas/Pruebas/Cocina_M4.unity y pone los ribosomas en sus marcas y la prueba (PruebaRibosomas).
    //
    // Se puede correr varias veces: vuelve a generar los prefabs y quita de la escena lo que puso antes.
    public static class ArmarPruebaM4
    {
        const string Raiz = CrearDatosCocina.Raiz;
        const string CarpetaPrefabs = Raiz + "/Prefabs";
        const string CarpetaPiezas = CarpetaPrefabs + "/Ribosomas";
        const string CarpetaPruebas = CarpetaPiezas + "/Pruebas";
        const string CarpetaUli = Raiz + "/Modelos/Uli";
        const string CarpetaMateriales = Raiz + "/Materiales";
        const string Escena = "Assets/_BioVR/Escenas/Pruebas/Cocina_M4.unity";

        const string ModeloRibosoma = "Assets/_BioVR/Minijuegos/ExpresionGenetica/Modelos/Diseno/ribosoma_80S_4ug0.fbx";
        const string ModeloARNt = "Assets/_BioVR/Minijuegos/ExpresionGenetica/Modelos/Diseno/arnt_modelo.fbx";
        const string ExportsUli = "../../../03-diseno/exports/minijuego-cocina/"; // desde Assets/

        // Medidas (especificación 5.2)
        const float AnchoRibosoma = 0.33f;
        const float BajoSitio = 0.06f;            // la base del ribosoma queda 6 cm bajo la marca (pie)
        const float AlturaARNtPrueba = 0.16f;
        const float OpacidadRibosoma = 0.45f;

        static readonly Color Cian = new Color(0.2f, 0.9f, 1f, 0.5f);
        static readonly Color ColorTexto = Color.white;

        [MenuItem("BioVR/Cocina/Armar prueba M4")]
        public static void Armar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(Escena))
            {
                Debug.LogError($"[M4] No existe {Escena}. Cópiala de Cocina_Base (BioVR → Cocina → Armar escena base).");
                return;
            }
            CrearDatosCocina.Carpeta(CarpetaPiezas);
            CrearDatosCocina.Carpeta(CarpetaPruebas);
            CrearDatosCocina.Carpeta(CarpetaUli);
            CrearDatosCocina.Carpeta(CarpetaMateriales);

            // Escena vacía para armar los prefabs sin ensuciar ninguna escena
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- 1 y 2. Modelos de Uli y piezas ----
            string arnmBases = Importar("arnm-bases.fbx");
            string aminoacido = Importar("aminoacido.fbx");
            var puntosBase = PuntosBase(arnmBases);
            var tramo = Pieza(arnmBases, "ARNm_cinta_codon", "Cocina_ARNm_Tramo");
            var bases = new[] { "A", "U", "G", "C" }.Select(b => Pieza(arnmBases, $"Base_{b}", $"Cocina_ARNm_Base_{b}")).ToArray();
            var cuenta = Pieza(aminoacido, "Aminoacido_cuenta", "Cocina_Cuenta");

            // ---- 3. Ribosomas ----
            var mesa = Ribosoma(false, tramo, bases, cuenta, puntosBase);
            var lejano = Ribosoma(true, tramo, bases, cuenta, puntosBase);

            // ---- 4. ARNt de prueba ----
            var arntPrueba = ARNtPrueba(cuenta); // la cuenta sirve de aminoácido en el ARNt de prueba

            // ---- 5. Escena de prueba ----
            Colocar(mesa, lejano, arntPrueba);
        }

        // =============================================================================================
        // Ribosoma
        // =============================================================================================

        static GameObject Ribosoma(bool lejano, GameObject tramo, GameObject[] bases, GameObject cuenta, Vector3[] puntosBase)
        {
            string nombre = lejano ? "Cocina_RibosomaLejano" : "Cocina_Ribosoma";
            var raiz = new GameObject(nombre);
            var ribo = raiz.AddComponent<RibosomaCocina>();
            ribo.lejano = lejano;
            var visual = new GameObject("Visual").transform;
            visual.SetParent(raiz.transform, false);

            // Modelo 4UG0 (Diego). Su +Z mira al jugador como la raíz; si el ARNm queda con 3′ a la izquierda del
            // jugador (+X de la raíz), se gira 180° para que 5′ quede a la izquierda.
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModeloRibosoma);
            if (fbx == null) { Debug.LogError($"[M4] No está {ModeloRibosoma}."); Object.DestroyImmediate(raiz); return null; }
            var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, visual);
            modelo.name = "Ribosoma 80S 4UG0 (Diego)";
            var m = modelo.transform;
            m.localPosition = Vector3.zero;
            m.localRotation = Quaternion.identity;
            m.localScale = Vector3.one;

            var sitioAModelo = Hijo(m, "Sitio_A");
            var arnm5 = Hijo(m, "ARNm_5prima");
            var arnm3 = Hijo(m, "ARNm_3prima");
            var tunel = Hijo(m, "Tunel_Salida");
            if (sitioAModelo == null || arnm5 == null || arnm3 == null)
                Debug.LogWarning("[M4] El ribosoma no trae Sitio_A / ARNm_5prima / ARNm_3prima: el sitio A se pone al centro.");

            float ancho = Limites(modelo).size.x;
            if (ancho > 0.0001f) m.localScale = Vector3.one * (AnchoRibosoma / ancho);
            if (arnm5 != null && arnm3 != null && (arnm3.position - arnm5.position).x > 0f)
                m.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // Punto del sitio A: sobre la línea del ARNm, bajo el Sitio_A del modelo
            Vector3 sitio = sitioAModelo != null
                ? new Vector3(sitioAModelo.position.x, arnm5 != null ? arnm5.position.y : sitioAModelo.position.y, sitioAModelo.position.z)
                : Limites(modelo).center;
            m.position -= sitio;
            var b = Limites(modelo);
            float subir = -BajoSitio - b.min.y;
            m.position += Vector3.up * subir;
            b = Limites(modelo);
            var puntoSitio = new Vector3(0f, subir, 0f);

            // Hacia 3′ (derecha del jugador = −X de la raíz)
            var hacia3 = arnm5 != null && arnm3 != null ? arnm3.position - arnm5.position : Vector3.left;
            hacia3.y = 0f;
            if (hacia3.sqrMagnitude < 1e-6f) hacia3 = Vector3.left;
            hacia3.Normalize();
            var rotARNm = Quaternion.FromToRotation(Vector3.right, hacia3);

            // Materiales transparentes (copias en Cocina/Materiales, sin tocar el FBX)
            var renderers = modelo.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.sharedMaterials = r.sharedMaterials.Select(TransparenteDe).ToArray();
            }

            // Sitio A: donde se para el ARNt (el anticodón sobre el codón)
            var sitioA = new GameObject("SitioA").transform;
            sitioA.SetParent(visual, false);
            sitioA.localPosition = puntoSitio + Vector3.up * 0.022f;

            // ARNm
            var arnmGo = new GameObject("ARNm");
            arnmGo.transform.SetParent(visual, false);
            arnmGo.transform.localPosition = puntoSitio;
            arnmGo.transform.localRotation = rotARNm;
            var arnm = arnmGo.AddComponent<ARNmCocina>();
            var aso = new SerializedObject(arnm);
            aso.FindProperty("tramo").objectReferenceValue = tramo;
            aso.FindProperty("baseA").objectReferenceValue = bases[0];
            aso.FindProperty("baseU").objectReferenceValue = bases[1];
            aso.FindProperty("baseG").objectReferenceValue = bases[2];
            aso.FindProperty("baseC").objectReferenceValue = bases[3];
            aso.FindProperty("materialParo").objectReferenceValue = Material("Paro", new Color(0.85f, 0.12f, 0.15f), false);
            if (puntosBase != null)
            {
                var pb = aso.FindProperty("puntosBase");
                pb.arraySize = puntosBase.Length;
                for (int i = 0; i < puntosBase.Length; i++) pb.GetArrayElementAtIndex(i).vector3Value = puntosBase[i];
            }
            aso.ApplyModifiedPropertiesWithoutUndo();

            // Caja cian del sitio A
            var caja = Primitiva("Caja sitio A", PrimitiveType.Cube, visual, Material("SitioA", Cian, true));
            caja.transform.localPosition = puntoSitio + Vector3.up * 0.035f;
            caja.transform.localRotation = rotARNm;
            caja.transform.localScale = new Vector3(0.13f, 0.045f, 0.05f);

            // Placa con el codón grande, al frente y abajo (de cara al jugador)
            var placa = Primitiva("Placa sitio A", PrimitiveType.Quad, visual, Material("Placa", new Color(0.2f, 0.9f, 1f, 0.25f), true));
            placa.transform.localPosition = new Vector3(0f, b.min.y + 0.05f, b.max.z + 0.02f);
            placa.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            placa.transform.localScale = new Vector3(0.16f, 0.09f, 1f);
            var codonGrande = Texto("Codon grande", visual, "", 0.45f, new Vector2(0.16f, 0.06f), placa.transform.localPosition + new Vector3(0f, -0.008f, 0.002f));
            var letreroSitio = Texto("Letrero sitio A", visual, "codón en el sitio A", 0.14f, new Vector2(0.16f, 0.02f), placa.transform.localPosition + new Vector3(0f, 0.032f, 0.002f));

            // Letreros arriba
            var titulo = Texto("Titulo", visual, lejano ? "RIBOSOMA LEJANO" : "RIBOSOMA", 0.4f, new Vector2(0.4f, 0.05f), new Vector3(0f, b.max.y + 0.09f, b.center.z));
            titulo.fontStyle = FontStyles.Bold;
            var subtitulo = Texto("Subtitulo", visual, "", 0.16f, new Vector2(0.4f, 0.04f), new Vector3(0f, b.max.y + 0.055f, b.center.z));
            var pide = Texto("Pide", visual, "", 0.3f, new Vector2(0.3f, 0.04f), new Vector3(0f, b.max.y + 0.02f, b.center.z));
            pide.fontStyle = FontStyles.Bold;

            // Cadena saliente en la salida del túnel
            var cadenaGo = new GameObject("Cadena saliente");
            cadenaGo.transform.SetParent(visual, false);
            var posTunel = tunel != null ? visual.InverseTransformPoint(tunel.position) : puntoSitio + new Vector3(0f, 0.2f, -0.1f);
            cadenaGo.transform.localPosition = posTunel;
            var haciaFuera = posTunel - b.center; haciaFuera.y = 0f;
            if (haciaFuera.sqrMagnitude > 1e-6f) cadenaGo.transform.localRotation = Quaternion.FromToRotation(Vector3.right, haciaFuera.normalized);
            var cadena = cadenaGo.AddComponent<CadenaSaliente>();
            var cso = new SerializedObject(cadena);
            cso.FindProperty("cuenta").objectReferenceValue = cuenta;
            cso.ApplyModifiedPropertiesWithoutUndo();

            // Botón FIN: a la izquierda del jugador (lado 5′), al frente
            var fin = new GameObject("Boton FIN");
            fin.transform.SetParent(visual, false);
            fin.transform.localPosition = puntoSitio - hacia3 * 0.2f + new Vector3(0f, -0.02f, 0f) + Vector3.forward * (b.max.z - puntoSitio.z + 0.03f);
            var colFin = fin.AddComponent<BoxCollider>();
            colFin.size = new Vector3(0.08f, 0.05f, 0.08f);
            var finVisual = new GameObject("Visual").transform;
            finVisual.SetParent(fin.transform, false);
            var cilindro = Primitiva("Cilindro", PrimitiveType.Cylinder, finVisual, Material("FIN", new Color(0.9f, 0.1f, 0.12f), false));
            cilindro.transform.localScale = new Vector3(0.07f, 0.012f, 0.07f);
            cilindro.transform.localRotation = Quaternion.Euler(50f, 0f, 0f);
            Texto("Etiqueta FIN", finVisual, "FIN", 0.3f, new Vector2(0.1f, 0.04f), new Vector3(0f, 0.05f, 0.01f)).fontStyle = FontStyles.Bold;
            fin.AddComponent<Senalable>();
            var botonFin = fin.AddComponent<BotonFin>();
            var bso = new SerializedObject(botonFin);
            bso.FindProperty("ribosoma").objectReferenceValue = ribo;
            bso.FindProperty("visual").objectReferenceValue = finVisual.gameObject;
            bso.ApplyModifiedPropertiesWithoutUndo();

            LlaveARNt llave = null;
            if (!lejano)
            {
                // Zona donde se suelta el ARNt
                var zona = new GameObject("Zona sitio A");
                zona.transform.SetParent(visual, false);
                zona.transform.localPosition = puntoSitio + Vector3.up * 0.08f;
                var esfera = zona.AddComponent<SphereCollider>();
                esfera.isTrigger = true;
                esfera.radius = ControlesCocina.RadioEstacion;
                var cuerpo = zona.AddComponent<Rigidbody>();
                cuerpo.isKinematic = true;
                cuerpo.useGravity = false;
                var zso = new SerializedObject(zona.AddComponent<ZonaSitioA>());
                zso.FindProperty("ribosoma").objectReferenceValue = ribo;
                zso.ApplyModifiedPropertiesWithoutUndo();

                // Llave y factor eEF1A (provisional: el de verdad viene en el prefab del ARNt de M2)
                llave = raiz.AddComponent<LlaveARNt>();
                var eef = Primitiva("eEF1A", PrimitiveType.Sphere, raiz.transform, Material("eEF1A", new Color(0.2f, 0.75f, 0.7f), false));
                eef.transform.localScale = Vector3.one * 0.04f;
                var etiquetaEef = Texto("Etiqueta", eef.transform, "eEF1A", 3f, new Vector2(2.5f, 0.8f), new Vector3(0f, 1.1f, 0f));
                etiquetaEef.fontSize = 3f;
                eef.SetActive(false);
                var lso = new SerializedObject(llave);
                lso.FindProperty("eEF1A").objectReferenceValue = eef;
                lso.FindProperty("alturaEEF1A").floatValue = AlturaARNtPrueba + 0.02f;
                lso.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                // Plataforma a 1.04 m con anillo magenta (la marca está a 1.30 m y escala ×2.1)
                float y = (1.04f - 1.30f) / 2.1f;
                var plataforma = Primitiva("Plataforma", PrimitiveType.Cylinder, visual, Material("Plataforma", new Color(0.25f, 0.2f, 0.35f), false));
                plataforma.transform.localPosition = new Vector3(0f, y, b.center.z);
                plataforma.transform.localScale = new Vector3(0.3f, 0.006f, 0.3f);
                var anillo = Primitiva("Anillo", PrimitiveType.Cylinder, visual, Material("Anillo", new Color(1f, 0.2f, 0.7f), false));
                anillo.transform.localPosition = new Vector3(0f, y - 0.002f, b.center.z);
                anillo.transform.localScale = new Vector3(0.32f, 0.004f, 0.32f);
            }

            var so = new SerializedObject(ribo);
            so.FindProperty("visual").objectReferenceValue = visual.gameObject;
            so.FindProperty("sitioA").objectReferenceValue = sitioA;
            so.FindProperty("arnm").objectReferenceValue = arnm;
            so.FindProperty("cadena").objectReferenceValue = cadena;
            so.FindProperty("botonFin").objectReferenceValue = botonFin;
            so.FindProperty("llave").objectReferenceValue = llave;
            so.FindProperty("cajaSitioA").objectReferenceValue = caja.GetComponent<Renderer>();
            var lista = so.FindProperty("renderersRibosoma");
            lista.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++) lista.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            so.FindProperty("titulo").objectReferenceValue = titulo;
            so.FindProperty("subtitulo").objectReferenceValue = subtitulo;
            so.FindProperty("pide").objectReferenceValue = pide;
            so.FindProperty("codonGrande").objectReferenceValue = codonGrande;
            so.FindProperty("letreroSitio").objectReferenceValue = letreroSitio;
            if (lejano)
            {
                so.FindProperty("claveTitulo").stringValue = "ribosoma_lejano";
                so.FindProperty("respaldoTitulo").stringValue = "RIBOSOMA LEJANO";
            }
            so.FindProperty("opacidadNormal").floatValue = OpacidadRibosoma;
            so.ApplyModifiedPropertiesWithoutUndo();

            var ruta = $"{CarpetaPrefabs}/{nombre}.prefab";
            float escalaModelo = m.localScale.x; // leerla antes de destruir la raíz
            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            Object.DestroyImmediate(raiz);
            Debug.Log($"[M4] {ruta}: ribosoma de {AnchoRibosoma} m de ancho; escala del modelo ×{escalaModelo:F4}.");
            return prefab;
        }

        // =============================================================================================
        // ARNt de prueba (no es el de M2)
        // =============================================================================================

        static GameObject ARNtPrueba(GameObject aaPrueba)
        {
            var raiz = new GameObject("Cocina_ARNtPrueba");
            raiz.AddComponent<ARNtCocina>();
            var modelo = new GameObject("Modelo").transform;
            modelo.SetParent(raiz.transform, false);
            modelo.localPosition = new Vector3(0f, AlturaARNtPrueba / 2f, 0f);

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ModeloARNt);
            Bounds b;
            if (fbx != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(fbx, modelo);
                go.name = "ARNt (Diego, solo prueba)";
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                foreach (var t in go.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("ARNt_Aminoacido")) t.gameObject.SetActive(false);
                b = Limites(go);
                float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
                if (mayor > 0.0001f) go.transform.localScale = Vector3.one * (AlturaARNtPrueba / mayor);
                b = Limites(go);
                go.transform.position += modelo.position - b.center;
            }
            else
            {
                var capsula = Primitiva("ARNt (capsula)", PrimitiveType.Capsule, modelo, Material("ARNt prueba", new Color(0.7f, 0.6f, 0.9f), false));
                capsula.transform.localScale = new Vector3(0.05f, AlturaARNtPrueba / 2f, 0.05f);
            }
            b = Limites(raiz);
            float frente = b.extents.z + 0.005f;

            // Aminoácido en la punta y aro gris cuando va vacío
            var matAA = Material("Aminoacido prueba", Color.white, false);
            GameObject aa;
            if (aaPrueba != null) aa = (GameObject)PrefabUtility.InstantiatePrefab(aaPrueba, raiz.transform);
            else aa = Primitiva("Aminoacido", PrimitiveType.Sphere, raiz.transform, matAA);
            aa.name = "Aminoacido";
            var bAA = Limites(aa);
            float mayorAA = Mathf.Max(bAA.size.x, bAA.size.y, bAA.size.z);
            if (mayorAA > 0.0001f) aa.transform.localScale *= 0.035f / mayorAA;
            aa.transform.localPosition = new Vector3(0f, AlturaARNtPrueba + 0.02f, 0f);
            foreach (var r in aa.GetComponentsInChildren<Renderer>()) r.sharedMaterial = matAA;
            var aro = Primitiva("Aro vacio", PrimitiveType.Cylinder, raiz.transform, Material("Aro vacio", new Color(0.55f, 0.5f, 0.65f), false));
            aro.transform.localPosition = new Vector3(0f, AlturaARNtPrueba + 0.02f, 0f);
            aro.transform.localScale = new Vector3(0.035f, 0.003f, 0.035f);

            var anticodon = Texto("Anticodon", raiz.transform, "3′ ··· 5′", 0.16f, new Vector2(0.12f, 0.025f), new Vector3(0f, 0.012f, frente));
            anticodon.fontStyle = FontStyles.Bold;
            var etiquetaAA = Texto("Etiqueta aminoacido", raiz.transform, "", 0.16f, new Vector2(0.1f, 0.025f), new Vector3(0f, AlturaARNtPrueba + 0.05f, 0f));

            var col = raiz.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, (AlturaARNtPrueba + 0.04f) / 2f, 0f);
            col.size = new Vector3(Mathf.Max(b.size.x, 0.05f), AlturaARNtPrueba + 0.04f, Mathf.Max(b.size.z, 0.04f));

            var cuerpo = raiz.AddComponent<Rigidbody>();
            cuerpo.mass = 0.2f;
            cuerpo.useGravity = false;
            cuerpo.linearDamping = 4f;
            cuerpo.angularDamping = 4f;
            var agarre = raiz.AddComponent<XRGrabInteractable>();
            agarre.farAttachMode = InteractableFarAttachMode.Near;
            agarre.movementType = XRBaseInteractable.MovementType.Kinematic;
            agarre.useDynamicAttach = true;
            agarre.matchAttachPosition = false;
            agarre.matchAttachRotation = false;
            agarre.snapToColliderVolume = false;
            agarre.throwOnDetach = false;

            var visual = raiz.AddComponent<ARNtPruebaVisual>();
            var vso = new SerializedObject(visual);
            vso.FindProperty("anticodon").objectReferenceValue = anticodon;
            vso.FindProperty("etiquetaAminoacido").objectReferenceValue = etiquetaAA;
            vso.FindProperty("aminoacido").objectReferenceValue = aa.GetComponentInChildren<Renderer>();
            vso.FindProperty("aroVacio").objectReferenceValue = aro;
            vso.ApplyModifiedPropertiesWithoutUndo();

            var ruta = $"{CarpetaPruebas}/Cocina_ARNtPrueba.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
            Object.DestroyImmediate(raiz);
            return prefab;
        }

        // =============================================================================================
        // Escena
        // =============================================================================================

        static void Colocar(GameObject mesa, GameObject lejano, GameObject arntPrueba)
        {
            var escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Single);
            var ancla = escena.GetRootGameObjects().FirstOrDefault(g => g.name == "AnclaCocina");
            if (ancla == null) { Debug.LogError("[M4] Cocina_M4 no tiene AnclaCocina."); return; }

            // Quitar lo que puso una corrida anterior
            foreach (var t in ancla.GetComponentsInChildren<Transform>(true).ToArray())
                if (t != null && (t.name.StartsWith("Cocina_Ribosoma") || t.name.StartsWith("[M4]")))
                    Object.DestroyImmediate(t.gameObject);

            Poner(mesa, ancla.transform, "Ribosoma de mesa");
            Poner(lejano, ancla.transform, "Ribosoma lejano");

            var prueba = new GameObject("[M4] Prueba ribosomas (ARNt de prueba)");
            prueba.transform.SetParent(ancla.transform, false);
            var pso = new SerializedObject(prueba.AddComponent<PruebaRibosomas>());
            pso.FindProperty("prefabARNt").objectReferenceValue = arntPrueba;
            pso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("[M4] Listo: Cocina_M4 con los dos ribosomas y la prueba. Teclas: 1/2/3 turno, M ARNt equivocado, L aventar al lejano.");
        }

        static void Poner(GameObject prefab, Transform ancla, string marca)
        {
            if (prefab == null) return;
            var punto = ancla.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == marca);
            if (punto == null) { Debug.LogError($"[M4] No encontré la marca '{marca}' en AnclaCocina."); return; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, punto);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var marcaCocina = punto.GetComponent<PuntoCocina>();
            if (marcaCocina != null) marcaCocina.mostrarEnJuego = false;
        }

        // =============================================================================================
        // Modelos y piezas
        // =============================================================================================

        // Copia el FBX de Uli a Cocina/Modelos/Uli si falta y lo importa sin cámaras ni luces
        static string Importar(string archivo)
        {
            string destino = $"{CarpetaUli}/{archivo}";
            if (!File.Exists(destino))
            {
                string origen = Path.GetFullPath(Path.Combine(Application.dataPath, ExportsUli + archivo));
                if (!File.Exists(origen)) { Debug.LogError($"[M4] No encontré {origen}."); return null; }
                File.Copy(origen, destino);
                AssetDatabase.ImportAsset(destino);
            }
            if (AssetImporter.GetAtPath(destino) is ModelImporter imp &&
                (imp.importCameras || imp.importLights || imp.animationType != ModelImporterAnimationType.None))
            {
                imp.importCameras = false;
                imp.importLights = false;
                imp.animationType = ModelImporterAnimationType.None;
                imp.SaveAndReimport();
            }
            return destino;
        }

        // Saca un hijo del FBX como prefab propio (con su malla y material), en el origen
        static GameObject Pieza(string fbx, string hijo, string nombre)
        {
            if (fbx == null) return null;
            var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (modelo == null) return null;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var t = Hijo(inst.transform, hijo);
            if (t == null)
            {
                Debug.LogWarning($"[M4] {fbx} no trae '{hijo}'.");
                Object.DestroyImmediate(inst);
                return null;
            }
            // Si el FBX tiene un solo objeto, Unity lo hace la raíz: entonces t es la misma instancia
            bool esRaiz = t == inst.transform;
            t.SetParent(null, true);
            t.position = Vector3.zero;
            t.name = nombre;
            var prefab = PrefabUtility.SaveAsPrefabAsset(t.gameObject, $"{CarpetaPiezas}/{nombre}.prefab");
            Object.DestroyImmediate(t.gameObject);
            if (!esRaiz && inst != null) Object.DestroyImmediate(inst);
            return prefab;
        }

        // Dónde van las 3 bases sobre un tramo: Punto_base_1..3 relativo al centro de ARNm_cinta_codon
        static Vector3[] PuntosBase(string fbx)
        {
            var modelo = fbx != null ? AssetDatabase.LoadAssetAtPath<GameObject>(fbx) : null;
            if (modelo == null) return null;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
            var cinta = Hijo(inst.transform, "ARNm_cinta_codon");
            var puntos = new[] { 1, 2, 3 }.Select(i => Hijo(inst.transform, $"Punto_base_{i}")).ToArray();
            Vector3[] r = null;
            if (cinta != null && puntos.All(p => p != null))
            {
                r = puntos.Select(p => p.position - cinta.position).ToArray();
                Debug.Log($"[M4] Bases sobre el tramo: {string.Join(", ", r.Select(v => v.ToString("F3")))}; tramo de {Limites(cinta.gameObject).size.x:F3} m.");
            }
            else Debug.LogWarning("[M4] arnm-bases.fbx no trae los Punto_base_1..3: uso la separación de 0.034 m.");
            Object.DestroyImmediate(inst);
            return r;
        }

        // =============================================================================================
        // Ayudantes
        // =============================================================================================

        static Transform Hijo(Transform raiz, string nombre) =>
            raiz.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == nombre);

        static Bounds Limites(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static GameObject Primitiva(string nombre, PrimitiveType tipo, Transform padre, Material material)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nombre;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(padre, false);
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        // Texto 3D de cara al jugador (la raíz del prefab tiene su +Z hacia el jugador)
        static TextMeshPro Texto(string nombre, Transform padre, string texto, float tamano, Vector2 caja, Vector3 posicion)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = posicion;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = ColorTexto;
            tmp.rectTransform.sizeDelta = caja;
            return tmp;
        }

        // Material URP Lit guardado como asset (Cocina_M4_<nombre>.mat)
        static Material Material(string nombre, Color color, bool transparente)
        {
            nombre = nombre.Replace(' ', '_'); // sin espacios en los nombres de archivo
            string ruta = $"{CarpetaMateriales}/Cocina_M4_{nombre}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, ruta);
            }
            m.SetColor("_BaseColor", color);
            if (transparente) PuntoCocina.Transparente(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        // Copia transparente de un material del ribosoma (no se toca el FBX ni los materiales de la fase A)
        static Material TransparenteDe(Material original)
        {
            var color = new Color(0.6f, 0.5f, 0.75f);
            if (original != null && original.HasProperty("_BaseColor")) color = original.GetColor("_BaseColor");
            else if (original != null && original.HasProperty("_Color")) color = original.GetColor("_Color");
            color.a = OpacidadRibosoma;
            return Material("Ribo " + (original != null ? original.name : "sin material"), color, true);
        }
    }
}
