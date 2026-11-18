using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // El código genético estándar: qué aminoácido pone cada codón, cuál es el codón "preferido"
    // de cada aminoácido (los señuelos de la corriente usan ese) y qué aminoácidos se parecen
    // (la sintetasa se equivoca entre parecidos).
    [CreateAssetMenu(menuName = "BioVR/Cocina/Código genético", fileName = "CodigoGenetico")]
    public class CodigoGenetico : ScriptableObject
    {
        [System.Serializable]
        public class Entrada
        {
            public AminoacidoData aminoacido;
            [Tooltip("El codón que usan los señuelos de la corriente (HTML: COD)")]
            public string preferido;
            [Tooltip("Todos los codones de este aminoácido, incluido el preferido")]
            public string[] codones;
        }

        [System.Serializable]
        public class Parecido
        {
            public AminoacidoData de;
            public AminoacidoData a;
        }

        public List<Entrada> aminoacidos = new List<Entrada>();
        public string[] codonesParo = { "UAA", "UAG", "UGA" };
        [Tooltip("Errores de la sintetasa: 'de' se confunde con 'a' (HTML: PAREC)")]
        public List<Parecido> parecidos = new List<Parecido>();

        Dictionary<string, AminoacidoData> porCodon;

        void OnEnable() => porCodon = null; // se reconstruye si cambian los datos

        void Indexar()
        {
            porCodon = new Dictionary<string, AminoacidoData>();
            foreach (var e in aminoacidos)
                if (e.aminoacido != null && e.codones != null)
                    foreach (var c in e.codones) porCodon[c] = e.aminoacido;
        }

        public bool EsParo(string codon) => System.Array.IndexOf(codonesParo, codon) >= 0;

        // El aminoácido que pone este codón, o null si es de paro o no existe.
        public AminoacidoData Traducir(string codon)
        {
            if (porCodon == null) Indexar();
            return codon != null && porCodon.TryGetValue(codon, out var aa) ? aa : null;
        }

        public AminoacidoData PorLetra(char letra)
        {
            foreach (var e in aminoacidos)
                if (e.aminoacido != null && e.aminoacido.letra == letra) return e.aminoacido;
            return null;
        }

        public string Preferido(AminoacidoData aa)
        {
            foreach (var e in aminoacidos)
                if (e.aminoacido == aa) return e.preferido;
            return null;
        }

        // El aminoácido equivocado que pone la sintetasa: su parecido o, si no tiene, otro al azar.
        public AminoacidoData Confundir(AminoacidoData aa, System.Random azar)
        {
            foreach (var p in parecidos)
                if (p.de == aa && p.a != null) return p.a;
            var otros = new List<AminoacidoData>();
            foreach (var e in aminoacidos)
                if (e.aminoacido != null && e.aminoacido != aa) otros.Add(e.aminoacido);
            return otros.Count > 0 ? otros[azar.Next(otros.Count)] : aa;
        }
    }
}
