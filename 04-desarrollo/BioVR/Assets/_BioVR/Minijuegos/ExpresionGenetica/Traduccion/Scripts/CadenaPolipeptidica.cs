using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // La cadena de aminoácidos que va armando el ribosoma (no el jugador: ficha 7).
    // Cada aminoácido nuevo llega desde su ARNt al extremo de la cadena, junto al sitio de lectura,
    // y los anteriores se recorren hacia la izquierda. Al terminar (factor de liberación) se suelta,
    // sube un poco y ya se puede sujetar y rotar para revisarla. Se entrega sin plegar (regla 4).
    public class CadenaPolipeptidica : MonoBehaviour
    {
        [SerializeField] float separacion = 0.06f;
        [SerializeField] float duracion = 0.45f;
        [SerializeField] float alturaAlSoltarse = 0.15f;

        readonly List<Transform> piezas = new List<Transform>();
        Vector3 posicionInicial;
        Quaternion rotacionInicial;
        XRGrabInteractable agarre;
        BoxCollider caja;

        public int Cantidad => piezas.Count;

        void Awake()
        {
            posicionInicial = transform.position;
            rotacionInicial = transform.rotation;
            agarre = GetComponent<XRGrabInteractable>();
            caja = GetComponent<BoxCollider>();
        }

        public IEnumerator Agregar(Transform aminoacido)
        {
            aminoacido.SetParent(transform, true);
            piezas.Add(aminoacido);
            var desde = new Vector3[piezas.Count];
            var hasta = new Vector3[piezas.Count];
            for (int i = 0; i < piezas.Count; i++)
            {
                desde[i] = piezas[i].localPosition;
                hasta[i] = new Vector3(-separacion * (piezas.Count - 1 - i), 0f, 0f); // la más nueva queda en 0
            }
            for (float t = 0f; t < 1f; t += Time.deltaTime / duracion)
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                for (int i = 0; i < piezas.Count; i++) piezas[i].localPosition = Vector3.Lerp(desde[i], hasta[i], k);
                yield return null;
            }
            for (int i = 0; i < piezas.Count; i++) piezas[i].localPosition = hasta[i];
        }

        public IEnumerator Liberar()
        {
            var desde = transform.position;
            var hasta = desde + Vector3.up * alturaAlSoltarse;
            for (float t = 0f; t < 1f; t += Time.deltaTime / (duracion * 2f))
            {
                transform.position = Vector3.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            transform.position = hasta;

            // Ya se puede sujetar: la caja cubre todos los aminoácidos
            if (caja != null && piezas.Count > 0)
            {
                var limites = new Bounds(piezas[0].position, Vector3.zero);
                foreach (var p in piezas)
                    foreach (var r in p.GetComponentsInChildren<Renderer>()) limites.Encapsulate(r.bounds);
                caja.center = transform.InverseTransformPoint(limites.center);
                caja.size = limites.size + Vector3.one * 0.01f;
                caja.enabled = true;
            }
            if (agarre != null) agarre.enabled = true;
        }

        // Para volver a empezar. Cada ARNt recupera su aminoácido con ARNt.Recargar().
        public void Limpiar()
        {
            if (agarre != null)
            {
                if (agarre.isSelected) agarre.interactionManager.CancelInteractableSelection((IXRSelectInteractable)agarre);
                agarre.enabled = false;
            }
            if (caja != null) caja.enabled = false;
            piezas.Clear();
            transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        }
    }
}
