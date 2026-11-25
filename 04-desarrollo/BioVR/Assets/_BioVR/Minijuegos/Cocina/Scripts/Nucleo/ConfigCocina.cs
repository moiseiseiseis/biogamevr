using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Todos los tiempos y números de ajuste de la Cocina. Valores iniciales sacados del HTML de Uli.
    // Si algo se siente lento o rápido en el visor, se cambia aquí, no en el código.
    [CreateAssetMenu(menuName = "BioVR/Cocina/Configuración", fileName = "ConfigCocina")]
    public class ConfigCocina : ScriptableObject
    {
        [Header("Ribosomas (M1 y M4)")]
        [Tooltip("Segundos entre codón y codón cuando el ribosoma avanza solo (HTML: r.auto)")]
        public float avanceAutomatico = 0.13f;
        [Tooltip("Segundos hasta que el ARNt usado pasa a la salida")]
        public float retrasoSalida = 1.4f;
        [Tooltip("Lo mismo, cuando entró un aminoácido equivocado")]
        public float retrasoSalidaMalo = 0.6f;
        [Tooltip("Segundos entre la última cadena y la fase B (Busca) o el final")]
        public float esperaPlegado = 2.4f;
        [Tooltip("Segundos entre el mensaje final del turno sin mutación y la pantalla final")]
        public float esperaFinal = 3f;
        [Tooltip("Segundos entre usar el proteasoma y la pantalla final")]
        public float esperaProteasoma = 2.8f;

        [Header("Corriente de ARNt (M2)")]
        public float intervaloRelleno = 0.8f;
        public int arntIniciales = 5;
        public int maxEnCorriente = 5;
        [Tooltip("Máximo de ARNt mientras hay codón raro")]
        public int maxEnCorrienteRaro = 9;
        [Tooltip("Vueltas por segundo del recorrido de la corriente")]
        public float velocidadCorriente = 0.08f; // 24-nov: antes 0.055 (iba lenta); 0.08 ≈ 9 cm/s, 12.5 s por vuelta
        [Tooltip("Segundos sin avanzar antes de que brillen los ARNt que sirven")]
        public float esperaPista = 9f;
        [Tooltip("Cuántos codones manuales por ribosoma se adelantan en la corriente")]
        public int codonesAdelantados = 2;
        [Tooltip("Diferencia mínima de bases entre un señuelo y el codón que se pide")]
        public int diferenciaSenuelo = 2;

        [Header("Sintetasa (M3)")]
        public float vueltasManivela = 2f;
        public int ranurasSintetasa = 2;

        [Header("Imprevistos (M6)")]
        public float primerImprevisto = 22f;
        public Vector2 intervaloImprevisto = new Vector2(20f, 32f);
        [Tooltip("Segundos que dura el codón raro")]
        public float duracionRaro = 12f;
        [Tooltip("Golpes para deshacer la horquilla")]
        public int golpesHorquilla = 3;

        [Header("Vuelo de los ARNt aventados (M2 y M5)")]
        public float vueloCerca = 0.45f;
        public float vueloLejos = 0.75f;
        public float alturaArcoVuelo = 0.35f;

        [Header("Puntos")]
        public int puntosAcierto = 100;
        [Tooltip("Puntos extra por cada acierto seguido")]
        public int puntosPorRacha = 10;
        [Tooltip("Racha máxima que cuenta para el extra")]
        public int rachaMaxima = 10;
        public int puntosMaloEntro = -50;
        public int puntosReciclado = 50;
        public int puntosSacudida = 30;
        public int puntosHorquilla = 40;
        public int puntosCuenta = 100;
        public int puntosProteasoma = 250;
        public int puntosProteasomaConChaperona = 150;
        [Tooltip("Errores al buscar la cuenta antes de que parpadee la correcta")]
        public int erroresBuscaParaPista = 2;
    }
}
