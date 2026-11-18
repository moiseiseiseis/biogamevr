namespace BioVR.Minijuegos.Cocina
{
    // Fases de un turno (HTML: st.fase).
    //   Armando  → el jugador arma las cadenas en los ribosomas
    //   Plegando → se soltó la última cadena; el holograma la dobla (2.4 s)
    //   Busca    → solo con mutación: el jugador toca la cuenta 20 en el holograma
    //   Decide   → chaperona o proteasoma
    //   Final    → el proteasoma la está degradando
    //   Fin      → pantalla final
    public enum FaseCocina { Armando, Plegando, Busca, Decide, Final, Fin }

    // Dónde está un ARNt en este momento (HTML: userData.lugar).
    public enum LugarARNt { Corriente, Mano, Sintetasa, Llave, Ribosoma, Salida, Vuelo }

    // Lo que responde ControlCocina.IntentarColocarEnRibosoma.
    public enum ResultadoColocacion
    {
        Rechazado, // no entró; la clave del rechazo dice por qué (M8 la muestra)
        Llave,     // ribosoma de mesa: el ARNt entró y ahora el jugador lo gira (M4 llama a Aceptar al encajar)
        Aceptado   // ribosoma lejano: entró directo
    }
}
