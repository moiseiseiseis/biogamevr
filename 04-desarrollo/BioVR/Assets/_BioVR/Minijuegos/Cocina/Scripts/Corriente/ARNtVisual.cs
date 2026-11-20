using UnityEngine;
using TMPro;

namespace BioVR.Minijuegos.Cocina
{
    [RequireComponent(typeof(ARNtCocina))]
    public class ARNtVisual : MonoBehaviour
    {
        [Header("Escalas")]
        [SerializeField] float escalaCorriente = 0.9f;
        [SerializeField] float escalaEstacion = 0.75f;
        [SerializeField] float velocidadEscala = 10f;

        [Header("Anticodón (punta de abajo)")]
        [Tooltip("3 puntos separados 0.034 m para instanciar las bases volteadas")]
        [SerializeField] Transform[] puntosBase = new Transform[3];
        [SerializeField] GameObject prefabBaseA;
        [SerializeField] GameObject prefabBaseU;
        [SerializeField] GameObject prefabBaseG;
        [SerializeField] GameObject prefabBaseC;
        [SerializeField] TextMeshPro textoAnticodon;

        [Header("Aminoácido (punta de arriba, 0.178 m)")]
        [SerializeField] GameObject contenedorAminoacido;
        [SerializeField] Renderer rendererAminoacido;
        [SerializeField] TextMeshPro textoAminoacido;
        [SerializeField] GameObject aroVacio;

        [Header("Alertas y ayudas visuales")]
        [Tooltip("Ícono '!' blanco sobre fondo rojo")]
        [SerializeField] GameObject alertaEquivocado;
        [SerializeField] GameObject bordeRojoEtiqueta;
        [Tooltip("Esfera amarilla translúcida para pista (> 9s)")]
        [SerializeField] Renderer brilloPista;
        [Tooltip("Esfera verde azulado (0.04 m) con etiqueta eEF1A")]
        [SerializeField] GameObject factorEEF1A;

        ARNtCocina datos;
        AminoacidoData ultimaCarga;
        string ultimoAnticodon;
        bool ultimoEquivocado;
        Material matAminoacido;
        Material matBrillo;

        void Awake()
        {
            datos = GetComponent<ARNtCocina>();
            if (rendererAminoacido != null) matAminoacido = rendererAminoacido.material;
            if (brilloPista != null)
            {
                matBrillo = brilloPista.material;
                brilloPista.gameObject.SetActive(false);
            }
            if (factorEEF1A != null) factorEEF1A.SetActive(false);
        }

        void Update()
        {
            if (datos == null) return;

            ActualizarEscala();
            ActualizarAnticodonSiCambio();
            ActualizarCargaSiCambio();
            ActualizarAlerta();
        }

        void ActualizarEscala()
        {
            float objetivo = (datos.Lugar == LugarARNt.Sintetasa ||
                              datos.Lugar == LugarARNt.Ribosoma ||
                              datos.Lugar == LugarARNt.Llave)
                              ? escalaEstacion : escalaCorriente;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * objetivo, Time.deltaTime * velocidadEscala);
        }

        void ActualizarAnticodonSiCambio()
        {
            string ac = datos.Anticodon;
            if (string.IsNullOrEmpty(ac) || ac == ultimoAnticodon) return;
            ultimoAnticodon = ac;

            if (textoAnticodon != null)
                textoAnticodon.text = datos.AnticodonRotulado;

            for (int i = 0; i < 3 && i < ac.Length && i < puntosBase.Length; i++)
            {
                if (puntosBase[i] == null) continue;
                for (int c = puntosBase[i].childCount - 1; c >= 0; c--)
                    Destroy(puntosBase[i].GetChild(c).gameObject);

                GameObject prefab = ObtenerPrefabBase(ac[i]);
                if (prefab != null)
                {
                    var inst = Instantiate(prefab, puntosBase[i]);
                    inst.transform.localPosition = Vector3.zero;
                    inst.transform.localRotation = Quaternion.Euler(180f, 0f, 0f); // volteada hacia abajo
                }
            }
        }

        GameObject ObtenerPrefabBase(char baseChar)
        {
            switch (baseChar)
            {
                case 'A': return prefabBaseA;
                case 'U': return prefabBaseU;
                case 'G': return prefabBaseG;
                case 'C': return prefabBaseC;
                default: return null;
            }
        }

        void ActualizarCargaSiCambio()
        {
            if (datos.Carga == ultimaCarga && datos.Equivocado == ultimoEquivocado) return;
            ultimaCarga = datos.Carga;
            ultimoEquivocado = datos.Equivocado;

            bool cargado = datos.Cargado;
            if (contenedorAminoacido != null) contenedorAminoacido.SetActive(cargado);
            if (aroVacio != null) aroVacio.SetActive(!cargado);

            if (cargado)
            {
                if (matAminoacido != null) matAminoacido.color = datos.Carga.color;
                if (textoAminoacido != null) textoAminoacido.text = datos.Carga.abreviatura;
            }

            if (bordeRojoEtiqueta != null) bordeRojoEtiqueta.SetActive(datos.Equivocado);
            if (alertaEquivocado != null && !datos.Equivocado) alertaEquivocado.SetActive(false);
        }

        void ActualizarAlerta()
        {
            if (!datos.Equivocado || alertaEquivocado == null) return;
            bool parpadeo = Mathf.PingPong(Time.time * 4f, 1f) > 0.4f;
            alertaEquivocado.SetActive(parpadeo);
        }

        public void EstablecerBrilloPista(bool activo)
        {
            if (brilloPista == null) return;
            if (brilloPista.gameObject.activeSelf != activo)
                brilloPista.gameObject.SetActive(activo);

            if (activo && matBrillo != null)
            {
                float alfa = Mathf.Lerp(0.16f, 0.28f, (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f);
                Color c = matBrillo.color;
                c.a = alfa;
                matBrillo.color = c;
            }
        }

        public void MostrarFactorEEF1A(bool mostrar)
        {
            if (factorEEF1A != null) factorEEF1A.SetActive(mostrar);
        }
    }
}

