using UnityEngine;
using UnityEngine.Rendering;

namespace BioVR.Minijuegos.Cocina
{
    public class DepositosAmiloides : MonoBehaviour
    {
        [Header("Referencia opcional")]
        [SerializeField] private HologramaCocina holograma;

        [Header("Material (lo asigna BioVR → Cocina → Armar fase B; en el APK Shader.Find puede fallar)")]
        [SerializeField] private Material material;

        [Header("Ajustes Visuales (§9.2)")]
        [SerializeField] private Vector3 desplazamiento = new Vector3(0f, 0.07f, 0.05f);
        [SerializeField] private Color colorRosa = new Color(1f, 0.32f, 0.72f, 0.16f);
        [SerializeField] private Color colorEmisionRosa = new Color(1f, 0.22f, 0.65f, 1f);
        [SerializeField] private float intensidadEmision = 0.85f;
        [SerializeField] private float velocidadRotacionY = 0.15f;

        private GameObject raizDepositos;
        private Material materialFantasma;

        void OnEnable()
        {
            EventosCocina.OnCuentaEncontrada += GenerarDepositos;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        }

        void OnDisable()
        {
            EventosCocina.OnCuentaEncontrada -= GenerarDepositos;
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        void OnDestroy()
        {
            LimpiarDepositos();
            if (materialFantasma != null) Destroy(materialFantasma);
        }

        private void AlIniciarTurno(TurnoData _)
        {
            LimpiarDepositos();
        }

        [ContextMenu("Probar Depósitos Amiloides")]
        public void GenerarDepositos()
        {
            LimpiarDepositos();

            if (holograma == null)
            {
                holograma = FindFirstObjectByType<HologramaCocina>();
            }

            if (holograma == null || holograma.ContenedorCadena == null || holograma.Cuentas == null)
            {
                Debug.LogWarning("[M10] No se encontró HologramaCocina o su cadena para generar depósitos.");
                return;
            }

            Transform contenedor = holograma.ContenedorCadena;
            Material mat = ObtenerOCrearMaterialRosa();

            // Se crea como hermano de ContenedorCadena (fuera de HologramaCocina.Cuentas)
            // para que el proteasoma solo se lleve la cadena original.
            raizDepositos = new GameObject("Depositos_Amiloides");
            raizDepositos.transform.SetParent(contenedor.parent, false);
            raizDepositos.transform.position = contenedor.position;
            raizDepositos.transform.rotation = contenedor.rotation;
            raizDepositos.transform.localScale = contenedor.localScale;

            CrearCopiaFantasma("Deposito_Amiloide_1", desplazamiento, contenedor, mat);
            CrearCopiaFantasma("Deposito_Amiloide_2", -desplazamiento, contenedor, mat);
        }

        private void CrearCopiaFantasma(string nombreCopia, Vector3 offsetLocal, Transform contenedor, Material mat)
        {
            GameObject grupo = new GameObject(nombreCopia);
            grupo.transform.SetParent(raizDepositos.transform, false);
            grupo.transform.localPosition = offsetLocal;
            grupo.transform.localRotation = Quaternion.identity;
            grupo.transform.localScale = Vector3.one;

            int index = 0;
            foreach (var cuenta in holograma.Cuentas)
            {
                if (cuenta == null || !cuenta.gameObject.activeSelf) continue;

                MeshFilter mfOrigen = cuenta.GetComponentInChildren<MeshFilter>();
                if (mfOrigen == null || mfOrigen.sharedMesh == null) continue;

                GameObject esfera = new GameObject($"Fantasma_{index:00}");
                esfera.transform.SetParent(grupo.transform, false);
                esfera.transform.localPosition = cuenta.transform.localPosition;
                esfera.transform.localRotation = cuenta.transform.localRotation;
                esfera.transform.localScale = cuenta.transform.localScale;

                MeshFilter mf = esfera.AddComponent<MeshFilter>();
                mf.sharedMesh = mfOrigen.sharedMesh;

                MeshRenderer mr = esfera.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;

                index++;
            }
        }

        void Update()
        {
            if (raizDepositos == null) return;
            if (ControlCocina.Instancia != null && ControlCocina.Instancia.Pausado) return;

            raizDepositos.transform.Rotate(Vector3.up, velocidadRotacionY * Mathf.Rad2Deg * Time.deltaTime, Space.Self);
        }

        public void LimpiarDepositos()
        {
            if (raizDepositos != null)
            {
                Destroy(raizDepositos);
                raizDepositos = null;
            }
        }

        private Material ObtenerOCrearMaterialRosa()
        {
            if (material != null) return material;
            if (materialFantasma != null) return materialFantasma;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");

            materialFantasma = new Material(shader);
            materialFantasma.name = "Mat_DepositoAmiloide_Rosa";

            // Configuración de transparencia en URP
            if (materialFantasma.HasProperty("_Surface"))
            {
                materialFantasma.SetFloat("_Surface", 1f); // Transparent
                materialFantasma.SetFloat("_Blend", 0f);   // Alpha
                materialFantasma.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                materialFantasma.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                materialFantasma.SetFloat("_ZWrite", 0f);
                materialFantasma.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                materialFantasma.renderQueue = (int)RenderQueue.Transparent;
            }

            if (materialFantasma.HasProperty("_BaseColor"))
            {
                materialFantasma.SetColor("_BaseColor", colorRosa);
            }
            if (materialFantasma.HasProperty("_Color"))
            {
                materialFantasma.SetColor("_Color", colorRosa);
            }

            // Emisión rosa
            materialFantasma.EnableKeyword("_EMISSION");
            materialFantasma.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            if (materialFantasma.HasProperty("_EmissionColor"))
            {
                materialFantasma.SetColor("_EmissionColor", colorEmisionRosa * intensidadEmision);
            }

            materialFantasma.enableInstancing = true;
            return materialFantasma;
        }
    }
}