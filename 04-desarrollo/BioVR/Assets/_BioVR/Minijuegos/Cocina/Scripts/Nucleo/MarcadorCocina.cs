using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Puntos, contadores y tiempo de un turno. Lo cambia solo ControlCocina; M8 lo lee para el
    // tablero y la pantalla final (las 7 cifras de la especificación 10.5).
    [System.Serializable]
    public class MarcadorCocina
    {
        public int puntos;
        public int racha;                  // aciertos seguidos
        public float tiempo;               // segundos, solo en Armando y Plegando, sin contar pausas

        public int aciertos;               // aminoácidos puestos
        public int errores;                // ARNt que no encajaban + aminoácidos equivocados que entraron
        public int reciclados;             // ARNt usados devueltos al citoplasma
        public int arreglos;               // errores de la sintetasa arreglados (sacudidas)
        public int golpes;                 // horquillas desenrolladas
        public int aminoacidosMalos;       // aminoácidos equivocados que entraron (HTML: protErr)
        public int cadenasTerminadas;      // HTML: hechas
        public int erroresBusca;           // toques en cuentas equivocadas
        public bool usoChaperona;

        // 3 estrellas si tardó hasta estrellas.x segundos, 2 hasta estrellas.y, si no 1.
        public int Estrellas(Vector2 limites) => tiempo <= limites.x ? 3 : tiempo <= limites.y ? 2 : 1;

        public void Sumar(int cuantos) => puntos = Mathf.Max(0, puntos + cuantos);

        public string Reloj => $"{(int)(tiempo / 60)}:{(int)(tiempo % 60):00}";
    }
}
