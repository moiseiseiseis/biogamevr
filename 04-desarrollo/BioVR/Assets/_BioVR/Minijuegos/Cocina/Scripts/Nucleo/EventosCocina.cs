using System;

namespace BioVR.Minijuegos.Cocina
{
    // Todo lo que pasa en la Cocina se anuncia aquí. Los módulos se suscriben en OnEnable y se
    // desuscriben en OnDisable:
    //
    //     void OnEnable()  => EventosCocina.OnCodonCambiado += MoverARNm;
    //     void OnDisable() => EventosCocina.OnCodonCambiado -= MoverARNm;
    //
    // Solo ControlCocina los lanza. Los módulos no se hablan entre sí: escuchan estos eventos.
    public static class EventosCocina
    {
        public static event Action<TurnoData> OnTurnoIniciado;
        public static event Action<FaseCocina> OnFaseCambiada;
        // El ribosoma avanzó (también en los avances automáticos). Su cadena ya trae el aminoácido nuevo.
        public static event Action<RibosomaEstado> OnCodonCambiado;
        // El jugador puso un ARNt y entró. correcto = falso si traía el aminoácido equivocado.
        public static event Action<RibosomaEstado, bool> OnAminoacidoAceptado;
        // Algo no entró. La clave es de TextosCocina (por ejemplo "rechazo_no_encaja").
        public static event Action<string> OnRechazo;
        public static event Action<ARNtCocina, bool> OnARNtCargado;     // bool = equivocado
        public static event Action<ARNtCocina, bool> OnARNtSacudido;    // bool = era equivocado
        public static event Action<ARNtCocina> OnARNtASalida;
        public static event Action<ARNtCocina> OnReciclado;
        public static event Action<bool> OnHorquilla;                   // true = aparece, false = se deshizo
        public static event Action<bool> OnCodonRaro;                   // true = empieza, false = termina
        public static event Action<RibosomaEstado> OnCadenaTerminada;
        public static event Action OnCuentaEncontrada;
        public static event Action<int> OnErrorBusca;                   // errores al buscar la cuenta
        public static event Action OnChaperona;
        public static event Action OnProteasoma;
        public static event Action<string> OnMensaje;                   // clave de TextosCocina
        public static event Action<string> OnSonido;                    // clave del sonido
        public static event Action<MarcadorCocina> OnTerminado;
        public static event Action<bool> OnPausa;

        internal static void TurnoIniciado(TurnoData t) => OnTurnoIniciado?.Invoke(t);
        internal static void FaseCambiada(FaseCocina f) => OnFaseCambiada?.Invoke(f);
        internal static void CodonCambiado(RibosomaEstado r) => OnCodonCambiado?.Invoke(r);
        internal static void AminoacidoAceptado(RibosomaEstado r, bool correcto) => OnAminoacidoAceptado?.Invoke(r, correcto);
        internal static void Rechazo(string clave) => OnRechazo?.Invoke(clave);
        internal static void ARNtCargado(ARNtCocina t, bool equivocado) => OnARNtCargado?.Invoke(t, equivocado);
        internal static void ARNtSacudido(ARNtCocina t, bool eraEquivocado) => OnARNtSacudido?.Invoke(t, eraEquivocado);
        internal static void ARNtASalida(ARNtCocina t) => OnARNtASalida?.Invoke(t);
        internal static void Reciclado(ARNtCocina t) => OnReciclado?.Invoke(t);
        internal static void Horquilla(bool aparece) => OnHorquilla?.Invoke(aparece);
        internal static void CodonRaro(bool activo) => OnCodonRaro?.Invoke(activo);
        internal static void CadenaTerminada(RibosomaEstado r) => OnCadenaTerminada?.Invoke(r);
        internal static void CuentaEncontrada() => OnCuentaEncontrada?.Invoke();
        internal static void ErrorBusca(int errores) => OnErrorBusca?.Invoke(errores);
        internal static void Chaperona() => OnChaperona?.Invoke();
        internal static void Proteasoma() => OnProteasoma?.Invoke();
        internal static void Mensaje(string clave) => OnMensaje?.Invoke(clave);
        internal static void Sonido(string clave) => OnSonido?.Invoke(clave);
        internal static void Terminado(MarcadorCocina m) => OnTerminado?.Invoke(m);
        internal static void Pausa(bool pausado) => OnPausa?.Invoke(pausado);
    }
}
