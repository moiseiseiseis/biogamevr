namespace BioVR.Minijuegos.Cocina
{
    // Un codón del ARNm de un turno, ya traducido.
    // ControlCocina arma la lista al iniciar el turno: el codón 0 es AUG (Met) y el último es el de paro.
    public readonly struct Codon
    {
        public readonly string tripleta;              // "AGC", leído 5′→3′
        public readonly AminoacidoData aminoacido;    // null en el codón de paro
        public readonly bool esParo;
        public readonly bool esMutacion;              // el codón 20 del turno 3 (GGC en lugar de AGC)

        public Codon(string tripleta, AminoacidoData aminoacido, bool esParo, bool esMutacion)
        {
            this.tripleta = tripleta;
            this.aminoacido = aminoacido;
            this.esParo = esParo;
            this.esMutacion = esMutacion;
        }

        // El anticodón que encaja, base por base y en el mismo orden que el codón, para mostrarlo
        // alineado justo debajo. Por eso se lee 3′→5′: debajo de 5′-AUG-3′ queda 3′-UAC-5′.
        public string Anticodon => Complemento(tripleta);

        // "3′ UAC 5′": así se rotula en todos los letreros (regla 1.5 de la especificación).
        public string AnticodonRotulado => $"3′ {Anticodon} 5′";

        // "5′ AUG 3′"
        public string CodonRotulado => $"5′ {tripleta} 3′";

        // Complemento de ARN: A↔U, G↔C.
        public static string Complemento(string bases)
        {
            if (bases == null) return null;
            var r = new char[bases.Length];
            for (int i = 0; i < bases.Length; i++)
                r[i] = bases[i] switch { 'A' => 'U', 'U' => 'A', 'G' => 'C', 'C' => 'G', _ => '?' };
            return new string(r);
        }

        // Cuántas bases son distintas entre dos codones (los señuelos difieren en 2 o más).
        public static int Diferencias(string a, string b)
        {
            int n = 0;
            for (int i = 0; i < 3; i++) if (a[i] != b[i]) n++;
            return n;
        }

        public override string ToString() => esParo ? $"{tripleta} (paro)" : $"{tripleta} ({aminoacido})";
    }
}
