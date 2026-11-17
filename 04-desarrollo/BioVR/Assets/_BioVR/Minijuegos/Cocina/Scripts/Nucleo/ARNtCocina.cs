using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Los datos de un ARNt: qué codón lee, qué aminoácido le toca y qué trae cargado.
    // No dibuja nada: M2 lo pone en el prefab del ARNt y se encarga del modelo, el color y el movimiento.
    //
    // Quien crea el ARNt llama a Configurar(). Después, el lugar y la carga los cambia ControlCocina
    // (M2, M3, M4 y M5 le avisan con sus métodos; nadie más escribe "carga" o "lugar").
    public class ARNtCocina : MonoBehaviour
    {
        [SerializeField] string codon;
        [SerializeField] AminoacidoData aminoacidoPropio;
        [SerializeField] AminoacidoData carga;
        [SerializeField] LugarARNt lugar = LugarARNt.Corriente;
        [SerializeField] bool usado;

        // El codón que lee (5′→3′). Su anticodón es el complemento, alineado debajo.
        public string CodonQueLee => codon;
        public string Anticodon => Codon.Complemento(codon);
        public string AnticodonRotulado => $"3′ {Anticodon} 5′";

        // El aminoácido que le corresponde (el que pone la sintetasa si no se equivoca).
        public AminoacidoData AminoacidoPropio => aminoacidoPropio;
        // El que trae ahora. null = vacío.
        public AminoacidoData Carga => carga;
        public bool Cargado => carga != null;
        // Trae un aminoácido que no es el suyo (la sintetasa se equivocó): hay que sacudirlo.
        public bool Equivocado => carga != null && carga != aminoacidoPropio;
        // Ya pasó por un ribosoma; al devolverlo al citoplasma da puntos.
        public bool Usado => usado;
        public LugarARNt Lugar => lugar;

        // La sintetasa se equivoca máximo una vez con cada ARNt (HTML: st.yaFallo).
        internal bool yaFalloSintetasa;

        public void Configurar(string codonQueLee, AminoacidoData propio)
        {
            codon = codonQueLee;
            aminoacidoPropio = propio;
            carga = null;
            usado = false;
            lugar = LugarARNt.Corriente;
            yaFalloSintetasa = false;
        }

        // Solo ControlCocina (y la prueba del núcleo) cambian estos datos.
        internal void PonerCarga(AminoacidoData aa) => carga = aa;
        internal void PonerLugar(LugarARNt nuevo) => lugar = nuevo;
        internal void PonerUsado(bool valor) => usado = valor;

        public override string ToString() =>
            $"ARNt {Anticodon} ({(carga != null ? carga.abreviatura : "vacío")}{(Equivocado ? " equivocado" : "")})";
    }
}
