using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    public class LectorCoordenadas : MonoBehaviour
    {
        [Header("Datos de Formas")]
        [Tooltip("CSV de AlphaFold con 37 residuos en Å (coordenadas_normales.csv)")]
        public TextAsset csvNormal;

        [Tooltip("ScriptableObject con las 37 posiciones de la variante S20G (FormaS20G.asset)")]
        public FormaProteinaData formaS20G;

        [Header("Medidas de Especificación M7")]
        [Tooltip("Radio máximo de la proteína plegada en metros (0.19 m según §8.2)")]
        public float radioMaximo = 0.19f;

        /// <summary>
        /// Devuelve 38 posiciones centradas y escaladas a radioMaximo (índice 0 = Met, 1..37 = amilina).
        /// </summary>
        public List<Vector3> ObtenerPosicionesNormal()
        {
            List<Vector3> residuos = LeerCSV(csvNormal);
            List<Vector3> conMet = AgregarMetInicial(residuos, 3.8f);
            return CentrarYEscalar(conMet, radioMaximo);
        }

        /// <summary>
        /// Devuelve 38 posiciones centradas y escaladas desde el asset intercambiado (S20G o fibra 8AZ4).
        /// </summary>
        public List<Vector3> ObtenerPosicionesMutante(FormaProteinaData forma = null)
        {
            FormaProteinaData origen = forma != null ? forma : formaS20G;
            if (origen == null || origen.posiciones == null || origen.posiciones.Count == 0)
            {
                Debug.LogError("[M7] Falta asignar FormaS20G.asset en LectorCoordenadas.");
                return new List<Vector3>();
            }

            List<Vector3> residuos = new List<Vector3>(origen.posiciones);
            float pasoEnlace = residuos.Count >= 2 ? Vector3.Distance(residuos[0], residuos[1]) : 3.8f;
            List<Vector3> conMet = AgregarMetInicial(residuos, pasoEnlace);
            return CentrarYEscalar(conMet, radioMaximo);
        }

        /// <summary>
        /// Para el Turno 1 (<= 5 cuentas): línea recta separada 3.6 Å, centrada y escalada.
        /// </summary>
        public List<Vector3> ObtenerPosicionesLineales(int totalCuentas)
        {
            List<Vector3> puntos = new List<Vector3>(totalCuentas);
            for (int i = 0; i < totalCuentas; i++)
            {
                puntos.Add(new Vector3(i * 3.6f, 0f, 0f));
            }
            return CentrarYEscalar(puntos, radioMaximo * 0.65f);
        }

        private List<Vector3> LeerCSV(TextAsset csv)
        {
            List<Vector3> lista = new List<Vector3>(37);
            if (csv == null)
            {
                Debug.LogError("[M7] No se asignó coordenadas_normales.csv en LectorCoordenadas.");
                return lista;
            }

            string[] lineas = csv.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] cols = lineas[i].Split(',');
                if (cols.Length < 5) continue;

                if (float.TryParse(cols[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(cols[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(cols[4].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    lista.Add(new Vector3(x, y, z));
                }
            }
            return lista;
        }

        // Agrega la Met inicial antes de K1 en la dirección de K1 -> K2 invertida (§8.3)
        private List<Vector3> AgregarMetInicial(List<Vector3> residuos, float distancia)
        {
            List<Vector3> completa = new List<Vector3>(residuos.Count + 1);
            if (residuos.Count >= 2)
            {
                Vector3 dirInvertida = (residuos[0] - residuos[1]).normalized;
                completa.Add(residuos[0] + dirInvertida * distancia);
            }
            else if (residuos.Count == 1)
            {
                completa.Add(residuos[0] + Vector3.left * distancia);
            }

            completa.AddRange(residuos);
            return completa;
        }

        // Resta el centroide y escala para que la esfera más lejana quede a radioTarget (§8.3)
        private List<Vector3> CentrarYEscalar(List<Vector3> puntos, float radioTarget)
        {
            if (puntos == null || puntos.Count == 0) return new List<Vector3>();

            Vector3 centro = Vector3.zero;
            for (int i = 0; i < puntos.Count; i++) centro += puntos[i];
            centro /= puntos.Count;

            float maxDist = 0f;
            for (int i = 0; i < puntos.Count; i++)
            {
                puntos[i] -= centro;
                float d = puntos[i].magnitude;
                if (d > maxDist) maxDist = d;
            }

            float escala = maxDist > 0.0001f ? (radioTarget / maxDist) : 1f;
            for (int i = 0; i < puntos.Count; i++)
            {
                puntos[i] *= escala;
            }

            return puntos;
        }
    }
}