using System.Collections.Generic;

namespace BioVR.Minijuegos.Cocina
{
    // Cómo va un ribosoma en el turno. Lo crea y lo cambia solo ControlCocina; los demás lo leen.
    public class RibosomaEstado
    {
        public readonly int numero;                  // 0 = el primero del turno (el de mesa), 1 = el segundo
        public readonly TurnoData.Ribosoma definicion;
        public bool Lejano => definicion.lejano;

        public int indice;                           // codón que está leyendo ahora
        public readonly List<AminoacidoData> cadena = new List<AminoacidoData>(); // cadena naciente, empieza con la Met
        public readonly HashSet<int> malos = new HashSet<int>(); // codones donde entró un aminoácido equivocado (cuentas rojas)
        public bool listo;                           // ya se tocó FIN y soltó su cadena
        internal float relojAutomatico;              // tiempo para el siguiente avance automático

        public RibosomaEstado(int numero, TurnoData.Ribosoma definicion)
        {
            this.numero = numero;
            this.definicion = definicion;
        }

        // ¿El jugador pone el ARNt de este codón? El codón de paro siempre es del jugador (toca FIN).
        public bool EsManual(int codon, bool esParo)
        {
            if (esParo || definicion.todoManual) return true;
            return codon >= definicion.manualDesde && codon <= definicion.manualHasta;
        }

        public override string ToString() => $"{(Lejano ? "Ribosoma lejano" : "Ribosoma de mesa")} (codón {indice})";
    }
}
