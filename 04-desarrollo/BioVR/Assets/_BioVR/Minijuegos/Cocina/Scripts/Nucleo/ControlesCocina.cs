namespace BioVR.Minijuegos.Cocina
{
    // Umbrales comunes de los controles VR (especificación 1.4). Todos los módulos usan estos mismos
    // números para que "aventar", "sacudir" y "girar" se sientan igual en toda la Cocina.
    // Son el punto de partida: se ajustan después de probar en el Quest 2 (Marcos).
    public static class ControlesCocina
    {
        // ---- Aventar (M2, M4, M5) ----
        // Soltar el grip con la mano a más de esta velocidad lanza el ARNt al destino que apunta.
        // Prueba del 23-nov: con 1.2 m/s y 25° casi nunca salía; se bajó la velocidad y se abrió el ángulo.
        public const float VelocidadLanzar = 0.8f;          // m/s
        // Ángulo máximo entre la dirección del lanzamiento y el destino para que "le atine".
        public const float AnguloAtinar = 35f;               // grados
        // Un ARNt usado solo tiene un destino (el embudo del regreso al citoplasma): más margen.
        public const float AnguloAtinarUsado = 60f;          // grados

        // ---- Soltar en una estación (sockets) ----
        // Distancia a la que una estación (sintetasa, ribosoma de mesa, salida) acepta el ARNt.
        public const float RadioEstacion = 0.12f;            // m

        // ---- Sacudir (M3) ----
        // Cuenta como sacudida si la mano cambia de dirección de lado a lado este número de veces
        // dentro de la ventana de tiempo, cada vez a más de la velocidad mínima.
        public const int CambiosSacudida = 3;
        public const float VentanaSacudida = 0.6f;          // s
        public const float VelocidadSacudida = 0.8f;        // m/s

        // ---- Girar el ARNt como llave (M4) ----
        // Encaja al quedar a menos de este ángulo del derecho, o si pasa por el derecho al girar.
        public const float ToleranciaLlave = 17f;            // grados (HTML: 0.3 rad)
        public const float GiroMaximoLlave = 137f;           // grados (HTML: 2.4 rad)

        // ---- Manivela de la sintetasa (M3) ----
        // La mano debe estar a esta distancia del eje para que el giro cuente.
        public const float RadioMinimoManivela = 0.04f;      // m

        // ---- Tutorial (M8) ----
        // Botón A / X o gatillo sobre "Entendido" cierran la ventana.
        public const float EsperaMinimaTutorial = 0.5f;      // s antes de poder cerrarla (evita cerrarla sin leer)
    }
}
