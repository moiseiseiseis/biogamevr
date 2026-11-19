using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // SOLO PARA LA ESCENA DE PRUEBA DE M4. El ARNt de verdad (modelo, piezas del anticodón, alerta "!")
    // es de M2 (Genaro). Este solo muestra lo mínimo para probar los ribosomas: el anticodón 3′…5′,
    // el aminoácido que trae (color y abreviatura) y un aro gris cuando va vacío.
    [RequireComponent(typeof(ARNtCocina))]
    public class ARNtPruebaVisual : MonoBehaviour
    {
        [SerializeField] TextMeshPro anticodon;
        [SerializeField] TextMeshPro etiquetaAminoacido;
        [SerializeField] Renderer aminoacido;
        [SerializeField] GameObject aroVacio;
        [SerializeField] Color colorEquivocado = new Color(0.9f, 0.15f, 0.15f);

        ARNtCocina arnt;
        MaterialPropertyBlock bloque;
        AminoacidoData cargaMostrada;
        bool primera = true;

        void Awake()
        {
            arnt = GetComponent<ARNtCocina>();
            bloque = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (anticodon != null && anticodon.text != arnt.AnticodonRotulado) anticodon.text = arnt.AnticodonRotulado;
            if (!primera && cargaMostrada == arnt.Carga) return;
            primera = false;
            cargaMostrada = arnt.Carga;

            bool cargado = arnt.Cargado;
            if (aminoacido != null)
            {
                aminoacido.gameObject.SetActive(cargado);
                if (cargado)
                {
                    aminoacido.GetPropertyBlock(bloque);
                    bloque.SetColor("_BaseColor", arnt.Carga.color);
                    aminoacido.SetPropertyBlock(bloque);
                }
            }
            if (aroVacio != null) aroVacio.SetActive(!cargado);
            if (etiquetaAminoacido != null)
            {
                etiquetaAminoacido.gameObject.SetActive(cargado);
                if (cargado)
                {
                    etiquetaAminoacido.text = arnt.Equivocado ? $"{arnt.Carga.abreviatura} !" : arnt.Carga.abreviatura;
                    etiquetaAminoacido.color = arnt.Equivocado ? colorEquivocado : Color.white;
                }
            }
        }
    }
}
