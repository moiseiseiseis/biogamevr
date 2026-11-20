using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M4 · La cadena naciente que sale por el túnel del ribosoma (especificación 5.5).
    // Hasta 8 cuentas colgando junto a la salida del túnel, la más nueva primero, meciéndose ±0.015 m.
    // Las que entraron con el aminoácido equivocado van en rojo; las demás con el color de Diego.
    // Este objeto va en el marcador Tunel_Salida del ribosoma; las cuentas se alinean sobre su +X.
    public class CadenaSaliente : MonoBehaviour
    {
        [SerializeField] GameObject cuenta;        // Aminoacido_cuenta de aminoacido.fbx (prefab del armador)
        [SerializeField] int maximo = 8;
        [SerializeField] float separacion = 0.026f;
        [SerializeField] float vaiven = 0.015f;
        [SerializeField] float escalaCuenta = 1f;
        [SerializeField] Color colorMalo = new Color(0.9f, 0.15f, 0.15f);

        readonly List<Transform> cuentas = new List<Transform>();
        MaterialPropertyBlock bloque;

        public void Agregar(AminoacidoData aa, bool correcto)
        {
            if (bloque == null) bloque = new MaterialPropertyBlock();
            GameObject go;
            if (cuenta != null) go = Instantiate(cuenta, transform);
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.022f;
            }
            go.name = aa != null ? $"Cuenta {aa.abreviatura}" : "Cuenta";
            go.transform.localScale *= escalaCuenta;
            foreach (var col in go.GetComponentsInChildren<Collider>()) Destroy(col);

            var color = correcto && aa != null ? aa.color : colorMalo;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.GetPropertyBlock(bloque);
                bloque.SetColor("_BaseColor", color);
                r.SetPropertyBlock(bloque);
            }

            cuentas.Insert(0, go.transform); // la más nueva primero
            while (cuentas.Count > maximo)
            {
                var vieja = cuentas[cuentas.Count - 1];
                cuentas.RemoveAt(cuentas.Count - 1);
                if (vieja != null) Destroy(vieja.gameObject);
            }
        }

        public void Limpiar()
        {
            foreach (var c in cuentas) if (c != null) Destroy(c.gameObject);
            cuentas.Clear();
        }

        void Update()
        {
            for (int j = 0; j < cuentas.Count; j++)
            {
                if (cuentas[j] == null) continue;
                float y = Mathf.Sin(Time.time * 2f + j * 0.7f) * vaiven;
                cuentas[j].localPosition = new Vector3(j * separacion, y, 0f);
            }
        }
    }
}
