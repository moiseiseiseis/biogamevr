using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public struct DatoAminoacido
    {
        public int numero;
        public string nombre;
        public Vector3 posicion;
    }

    public class LectorCoordenadas : MonoBehaviour
    {
        public TextAsset archivoCSV;

        public List<DatoAminoacido> LeerDatos()
        {
            List<DatoAminoacido> lista = new List<DatoAminoacido>();

            if (archivoCSV == null)
            {
                Debug.LogError("Falta asignar el archivo CSV en el Inspector.");
                return lista;
            }

            string[] lineas = archivoCSV.text.Split('\n');

            for (int i = 1; i < lineas.Length; i++)
            {
                string linea = lineas[i].Trim();
                if (string.IsNullOrEmpty(linea)) continue;

                string[] columnas = linea.Split(',');
                if (columnas.Length < 5) continue;

                DatoAminoacido dato = new DatoAminoacido();
                dato.numero = int.Parse(columnas[0]);
                dato.nombre = columnas[1].Trim();

                float x = float.Parse(columnas[2], CultureInfo.InvariantCulture);
                float y = float.Parse(columnas[3], CultureInfo.InvariantCulture);
                float z = float.Parse(columnas[4], CultureInfo.InvariantCulture);

                dato.posicion = new Vector3(x, y, z);
                lista.Add(dato);
            }

            return lista;
        }
    }
}