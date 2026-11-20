
using BioVR.Minijuegos.ExpresionGenetica;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(XRSimpleInteractable))]
    [RequireComponent(typeof(Senalable))]
    public class CuentaHolograma : MonoBehaviour
    {
        public int IndiceResiduo { get; private set; } // 0 = Met, 1..37 = residuos de amilina
        public bool EsMutacion { get; private set; }

        private Senalable senalable;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;
        private GameObject etiquetaObj;
        private TextMeshPro textoEtiqueta;

        private float escalaBase = 0.94f;
        private float multiplicadorEscala = 1f;
        private bool parpadeando;
        private bool encontrada;

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        void Awake()
        {
            senalable = GetComponent<Senalable>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();

            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = false;
            if (col.radius <= 0f) col.radius = 0.5f;

            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.enableInstancing = true;
            }
        }

        void OnEnable()
        {
            if (senalable != null) senalable.OnSenalado += AlSerTocada;
        }

        void OnDisable()
        {
            if (senalable != null) senalable.OnSenalado -= AlSerTocada;
        }

        public void Inicializar(int indiceResiduo, Color color, float escala, bool esMutacion)
        {
            IndiceResiduo = indiceResiduo;
            EsMutacion = esMutacion;
            escalaBase = escala;
            multiplicadorEscala = 1f;
            parpadeando = false;
            encontrada = false;

            AplicarColor(color);
            PonerInteractuable(false);

            if (etiquetaObj != null) etiquetaObj.SetActive(false);
        }

        public void AplicarColor(Color color)
        {
            if (meshRenderer == null) return;
            if (propBlock == null) propBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorID, color);
            propBlock.SetColor(ColorID, color);
            meshRenderer.SetPropertyBlock(propBlock);
        }

        public void PonerInteractuable(bool activo)
        {
            // La Met (índice 0) nunca es seleccionable en la fase Busca
            if (senalable != null)
            {
                senalable.Habilitado = activo && IndiceResiduo > 0;
            }
        }

        public void ActivarParpadeoPista(bool activo)
        {
            parpadeando = activo && !encontrada;
            if (!parpadeando && !encontrada)
            {
                multiplicadorEscala = 1f;
            }
        }

        public void MarcarComoEncontrada()
        {
            encontrada = true;
            parpadeando = false;
            multiplicadorEscala = 1.6f; // Crece x1.6 al acertar (§8.5)
            PonerInteractuable(false);
            MostrarEtiquetaMutacion();
        }

        void Update()
        {
            if (parpadeando)
            {
                // Parpadeo de pista entre escala 1.0 y 1.5 tras 2 errores (§8.5)
                float pulso = (Mathf.Sin(Time.time * 7f) + 1f) * 0.5f;
                multiplicadorEscala = Mathf.Lerp(1f, 1.5f, pulso);
            }

            if (encontrada || parpadeando)
            {
                transform.localScale = Vector3.one * (escalaBase * multiplicadorEscala);
            }

            if (etiquetaObj != null && etiquetaObj.activeSelf && Camera.main != null)
            {
                etiquetaObj.transform.rotation = Quaternion.LookRotation(
                    etiquetaObj.transform.position - Camera.main.transform.position
                );
            }
        }

        private void AlSerTocada(Senalable _)
        {
            if (IndiceResiduo <= 0) return;

            if (ControlCocina.Instancia != null)
            {
                ControlCocina.Instancia.TocarCuenta(IndiceResiduo);
            }
            else
            {
                // Respaldo por si se prueba aislado sin ControlCocina
                HologramaCocina holo = GetComponentInParent<HologramaCocina>();
                if (holo != null) holo.NotificarToquePrueba(IndiceResiduo);
            }
        }

        private void MostrarEtiquetaMutacion()
        {
            if (etiquetaObj == null)
            {
                etiquetaObj = new GameObject("Etiqueta_S20G");
                etiquetaObj.transform.SetParent(transform, false);
                etiquetaObj.transform.localPosition = new Vector3(0f, 0.035f, 0f);

                textoEtiqueta = etiquetaObj.AddComponent<TextMeshPro>();
                textoEtiqueta.text = "Ser → Gly";
                textoEtiqueta.fontSize = 0.35f;
                textoEtiqueta.alignment = TextAlignmentOptions.Center;
                textoEtiqueta.color = new Color(0.3f, 1f, 0.85f, 1f);
                textoEtiqueta.rectTransform.sizeDelta = new Vector2(0.25f, 0.08f);
            }

            etiquetaObj.SetActive(true);
        }
    }
}