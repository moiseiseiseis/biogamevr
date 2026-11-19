using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class HologramaCocina : MonoBehaviour
    {
        public LectorCoordenadas lector;
        public GameObject cuentaPrefab;

        [Header("Ajustes del Holograma")]
        public float escalaProteina = 0.012f;
        public float alturaSobreProyector = 0.25f;
        public float tamanoEsfera = 0.02f;
        public float velocidadRotacion = 25f;

        private GameObject contenedorHolograma;

        void Start()
        {
            GenerarHolograma();
        }

        void Update()
        {
            if (contenedorHolograma != null)
            {
                contenedorHolograma.transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
            }
        }

        public void GenerarHolograma()
        {
            if (lector == null || cuentaPrefab == null)
            {
                Debug.LogError("Falta asignar el Lector o el Cocina_Cuenta en HologramaCocina.");
                return;
            }

            List<DatoAminoacido> datos = lector.LeerDatos();
            if (datos.Count == 0) return;

            contenedorHolograma = new GameObject("Proteina_Holograma");
            contenedorHolograma.transform.position = transform.position + Vector3.up * alturaSobreProyector;

            Vector3 centro = Vector3.zero;
            foreach (var d in datos) centro += d.posicion;
            centro /= datos.Count;

            foreach (var d in datos)
            {
                Vector3 posCentrada = (d.posicion - centro) * escalaProteina;
                GameObject nuevaCuenta = Instantiate(cuentaPrefab, contenedorHolograma.transform);
                nuevaCuenta.transform.localPosition = posCentrada;
                nuevaCuenta.transform.localScale = Vector3.one * tamanoEsfera;

                CuentaHolograma scriptCuenta = nuevaCuenta.GetComponent<CuentaHolograma>();
                if (scriptCuenta != null)
                {
                    scriptCuenta.Configurar(d.numero, d.nombre);
                }
            }

            Debug.Log($"Holograma generado con {datos.Count} aminoácidos.");
        }
    }
}

