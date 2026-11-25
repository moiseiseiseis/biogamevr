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

        [Header("Legibilidad (prueba del 23-nov)")]
        [Tooltip("Los letreros del ARNt siempre de pie y de frente al jugador, aunque el ARNt esté acostado o girado")]
        [SerializeField] bool letrerosDeFrente = true;
        [SerializeField] float escalaLetreros = 1.4f;
        [Tooltip("El aminoácido cargado flota, late y brilla (en el HTML es una bolita llamativa)")]
        [SerializeField] float escalaAminoacido = 1.5f;
        [SerializeField] float flotarAmplitud = 0.012f;

        Transform camara;
        Vector3 posAminoacido, escAminoacido, escAnticodon, escTextoAa, posAnticodon;
        bool medidas;

        void Update()
        {
            if (datos == null) return;

            ActualizarEscala();
            ActualizarAnticodonSiCambio();
            ActualizarCargaSiCambio();
            ActualizarAlerta();
        }

        void LateUpdate()
        {
            if (datos == null) return;
            if (!medidas)
            {
                medidas = true;
                if (contenedorAminoacido != null) { posAminoacido = contenedorAminoacido.transform.localPosition; escAminoacido = contenedorAminoacido.transform.localScale; }
                if (textoAnticodon != null) { escAnticodon = textoAnticodon.transform.localScale; posAnticodon = textoAnticodon.transform.localPosition; }
                if (textoAminoacido != null) escTextoAa = textoAminoacido.transform.localScale;
            }

            // Aminoácido cargado: flota y late
            if (contenedorAminoacido != null && contenedorAminoacido.activeSelf)
            {
                float t = Time.time;
                var c = contenedorAminoacido.transform;
                c.localPosition = posAminoacido + Vector3.up * (Mathf.Sin(t * 3f) * flotarAmplitud / Mathf.Max(0.01f, transform.lossyScale.y));
                c.localScale = escAminoacido * escalaAminoacido * (1f + 0.08f * Mathf.Sin(t * 6f));
            }

            // En la bandeja de salida (usado) no se muestra el anticodón: ya no importa cuál es (prueba del 23-nov)
            if (textoAnticodon != null)
            {
                bool ver = !datos.Usado;
                if (textoAnticodon.gameObject.activeSelf != ver) textoAnticodon.gameObject.SetActive(ver);
            }

            if (!letrerosDeFrente) return;
            if (camara == null) camara = Camera.main != null ? Camera.main.transform : null;
            if (camara == null) return;
            // Flotando, el anticodón va arriba del ARNt (no a media figura, donde chocaba con el del vecino)
            if (textoAnticodon != null)
            {
                if (datos.Lugar == LugarARNt.Corriente)
                    textoAnticodon.transform.position = transform.position + Vector3.up * alturaLetreroFlotando * transform.lossyScale.y;
                else
                    textoAnticodon.transform.localPosition = posAnticodon;
            }
            DeFrente(textoAnticodon, escAnticodon);
            DeFrente(textoAminoacido, escTextoAa);
        }

        // TextMeshPro se lee de frente cuando su +Z se aleja del que lo mira; "arriba" siempre es el del mundo
        void DeFrente(TextMeshPro texto, Vector3 escalaBase)
        {
            if (texto == null || !texto.gameObject.activeInHierarchy) return;
            var t = texto.transform;
            var hacia = t.position - camara.position;
            hacia.y *= 0.5f; // inclinación suave: que no se acueste al mirar desde arriba
            if (hacia.sqrMagnitude < 1e-6f) return;
            t.rotation = Quaternion.LookRotation(hacia, Vector3.up);
            t.localScale = escalaBase * escalaLetreros;
        }

        [Header("Escalas desde la prueba del 23-nov")]
        [Tooltip("Mientras flota en la corriente o espera en la bandeja: más chico para que no se encimen")]
        [SerializeField] float escalaFlotando = 0.7f;
        [Tooltip("Letrero del anticodón arriba del ARNt mientras flota (m sobre su centro, a escala 1)")]
        [SerializeField] float alturaLetreroFlotando = 0.13f;

        void ActualizarEscala()
        {
            // En la mano y volando conserva el tamaño de antes (escalaCorriente); flotando y en la bandeja, más chico
            float objetivo = (datos.Lugar == LugarARNt.Sintetasa ||
                              datos.Lugar == LugarARNt.Ribosoma ||
                              datos.Lugar == LugarARNt.Llave)
                              ? escalaEstacion
                              : (datos.Lugar == LugarARNt.Corriente || datos.Lugar == LugarARNt.Salida) ? escalaFlotando
                              : escalaCorriente;

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
                if (matAminoacido != null)
                {
                    matAminoacido.color = datos.Carga.color;
                    // Que brille con su propio color (URP Lit); en el visor la esfera café se perdía
                    if (matAminoacido.HasProperty("_EmissionColor"))
                    {
                        matAminoacido.EnableKeyword("_EMISSION");
                        matAminoacido.SetColor("_EmissionColor", datos.Carga.color * 1.6f);
                    }
                }
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

