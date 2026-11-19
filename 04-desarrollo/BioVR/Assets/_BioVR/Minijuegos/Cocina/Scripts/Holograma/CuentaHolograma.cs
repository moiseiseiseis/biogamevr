
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class CuentaHolograma : MonoBehaviour
    {
        public int numeroResiduo;
        public string tipoAminoacido;

        public void Configurar(int numero, string aminoacido)
        {
            numeroResiduo = numero;
            tipoAminoacido = aminoacido;
            gameObject.name = $"Residuo_{numero}_{aminoacido}";
        }
    }
}