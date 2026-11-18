using System.Collections.Generic;
using UnityEngine;

public class HologramaCocina : MonoBehaviour
{
    public LectorCoordenadas lector;
    public GameObject cuentaPrefab;

    [Header("Ajustes del Holograma")]
    public float escalaProteina = 0.012f;      // Tamaño ideal (~34 x 22 x 40 cm) para la mesa
    public float alturaSobreProyector = 0.25f; // Flota 25 cm arriba del centro del cilindro
    public float tamanoEsfera = 0.02f;         // Esferas redondas de 2 cm
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
            Debug.LogError("Falta asignar el Lector o el Cuenta_Prefab en HologramaCocina.");
            return;
        }

        List<DatoAminoacido> datos = lector.LeerDatos();
        if (datos.Count == 0) return;

        // Se crea en escala global (1,1,1) para no heredar la escala aplastada del cilindro
        contenedorHolograma = new GameObject("Proteina_Holograma");
        contenedorHolograma.transform.position = transform.position + Vector3.up * alturaSobreProyector;

        // Calcular el centro para que la proteína gire sobre su propio eje
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