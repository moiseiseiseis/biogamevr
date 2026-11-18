using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Un turno (pedido) de la Cocina. Tres assets: Turno1, Turno2 y Turno3 en Cocina/Datos/.
    // Los valores salen del HTML de Uli (NIVELES) con las decisiones del equipo (paro UAG, S20G en el codón 20).
    [CreateAssetMenu(menuName = "BioVR/Cocina/Turno", fileName = "Turno")]
    public class TurnoData : ScriptableObject
    {
        [System.Serializable]
        public class Ribosoma
        {
            [Tooltip("Falso = ribosoma de la mesa (se encaja y se gira). Verdadero = lejano (se le avientan los ARNt)")]
            public bool lejano;
            [Tooltip("Si está marcado, el jugador pone todos los ARNt (turno 1)")]
            public bool todoManual;
            [Tooltip("Primer y último codón que pone el jugador (índices de codón; el 0 es AUG). Los demás los avanza el ribosoma solo")]
            public int manualDesde;
            public int manualHasta;
        }

        [Tooltip("Título del turno, por ejemplo 'Turno 2 · Pedido de amilina'")]
        public string nombre;
        [Tooltip("Clave del mensaje de bienvenida en TextosCocina")]
        public string claveIntro;

        [Tooltip("Aminoácidos de la proteína, sin la Met inicial (una letra cada uno)")]
        public string secuencia;
        [Tooltip("Todos los codones del ARNm: AUG, uno por aminoácido y el de paro")]
        public string[] codones;
        [Tooltip("Índice en la secuencia del aminoácido cambiado (base 0). -1 = sin mutación. S20G = 19, que es el codón 20")]
        public int mutacion = -1;

        public Ribosoma[] ribosomas;

        [Tooltip("Cuántos ARNt usados caben en la salida")]
        public int capacidadSalida = 2;
        [Tooltip("Probabilidad (0 a 1) de que la sintetasa ponga el aminoácido equivocado")]
        [Range(0f, 1f)] public float errorSintetasa;
        [Tooltip("Si hay horquilla y codón raro (M6)")]
        public bool imprevistos;
        [Tooltip("Si la mesa gira (M6, solo si da tiempo)")]
        public bool mesaGira;
        [Tooltip("Segundos para 3 estrellas (x) y para 2 estrellas (y)")]
        public Vector2 estrellas = new Vector2(75, 140);
        [Tooltip("Ventanas del tutorial que se encolan al iniciar (claves de M8)")]
        public string[] tutoriales;

        public bool TieneMutacion => mutacion >= 0;
        // El codón de la mutación: la secuencia empieza en el codón 1 porque el 0 es la Met.
        public int CodonMutacion => mutacion >= 0 ? mutacion + 1 : -1;
    }
}
