using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Marca vacía de la escena base: dónde va cada estación (tabla 1.2 de la especificación).
    // Cada módulo pone su prefab como hijo de su marca (o copia su posición) en su escena de prueba.
    //
    // En el editor la dibuja DibujarPuntosCocina (Editor/), aunque AnclaCocina esté apagada.
    // En Play, si "mostrarEnJuego" está encendido, pone una esfera translúcida con su nombre para revisar
    // alturas y distancias en el simulador o el visor. Apágalo cuando tu prefab ya esté encima.
    public class PuntoCocina : MonoBehaviour
    {
        [TextArea(1, 4)] public string nota;
        public Color color = new Color(1f, 0.24f, 0.65f);
        public float radio = 0.05f;

        [Tooltip("Metros extra para subir el nombre, cuando dos marcas comparten lugar (salida y proteasoma).")]
        public float subirEtiqueta;

        [Tooltip("Se agrega al nombre en la etiqueta, p. ej. \"fase B\".")]
        public string detalle;

        public string Etiqueta => string.IsNullOrEmpty(detalle) ? name : $"{name} · {detalle}";
        public float AlturaEtiqueta => radio * 1.5f + subirEtiqueta;

        [Tooltip("En Play: esfera translúcida y nombre en esta marca.")]
        public bool mostrarEnJuego = true;

        [Tooltip("Material translúcido de las marcas (lo asigna el armador). Si falta, se crea uno en el editor.")]
        public Material material;

        Transform letrero;
        Camera camara;

        void Start()
        {
            if (!mostrarEnJuego) return;

            // Tamaño en metros sin importar la escala de la marca (algunas vienen ×0.75 o ×2.1)
            float s = Mathf.Max(transform.lossyScale.x, 0.0001f);

            var esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.name = "Marca (solo prueba)";
            Destroy(esfera.GetComponent<Collider>()); // que no estorbe a los rayos ni a los agarres
            esfera.transform.SetParent(transform, false);
            esfera.transform.localScale = Vector3.one * (radio * 2f / s);
            var r = esfera.GetComponent<Renderer>();
            var mat = material != null ? material : MaterialProvisional();
            if (mat != null) r.sharedMaterial = mat;
            var bloque = new MaterialPropertyBlock();
            var c = color; c.a = 0.45f;
            bloque.SetColor("_BaseColor", c);
            r.SetPropertyBlock(bloque);

            var go = new GameObject("Nombre (solo prueba)", typeof(TextMeshPro));
            letrero = go.transform;
            letrero.SetParent(transform, false);
            letrero.localPosition = Vector3.up * ((radio * 2.2f + subirEtiqueta) / s);
            letrero.localScale = Vector3.one / s;
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = Etiqueta;
            tmp.fontSize = 0.35f;
            tmp.alignment = TextAlignmentOptions.Bottom;
            tmp.color = color;
            tmp.rectTransform.sizeDelta = new Vector2(0.6f, 0.1f);
        }

        void LateUpdate()
        {
            if (letrero == null) return;
            if (camara == null) camara = Camera.main;
            if (camara == null) return;
            // El texto siempre de frente al jugador
            letrero.rotation = Quaternion.LookRotation(letrero.position - camara.transform.position, Vector3.up);
        }

        Material MaterialProvisional()
        {
#if UNITY_EDITOR
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            return shader != null ? Transparente(new Material(shader)) : null;
#else
            return null; // en el APK solo se usa el material asignado (Shader.Find no sirve si el shader se quitó)
#endif
        }

        // Unlit de URP en modo transparente (lo mismo que cambiar Surface Type a Transparent en el Inspector)
        public static Material Transparente(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return m;
        }
    }
}
