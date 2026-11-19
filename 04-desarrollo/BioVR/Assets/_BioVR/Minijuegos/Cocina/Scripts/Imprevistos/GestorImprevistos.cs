using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M6 · Decide CUÁNDO pasa un imprevisto (especificación 7.2, HTML: evento).
    // El estado (si hay horquilla, cuántos golpes, cuánto falta del codón raro) lo guarda ControlCocina;
    // aquí solo se lleva el reloj y se elige cuál toca.
    //
    //   - Solo en turnos con "imprevistos" y en la fase Armando (ControlCocina.PuedeHaberImprevisto).
    //   - El primero a los primerImprevisto s (22); después cada intervaloImprevisto s al azar (20 a 32).
    //   - 50 % codón raro y 50 % horquilla. Si ya hay horquilla o la mesa ya terminó, toca codón raro.
    //
    // El reloj solo corre mientras se puede: en pausa (tutorial) o en otra fase se detiene.
    public class GestorImprevistos : MonoBehaviour
    {
        [Tooltip("0 = distinto cada vez. Otro número = siempre los mismos imprevistos (para probar)")]
        [SerializeField] int semilla = 0;

        public float Faltan { get; private set; }   // segundos para el siguiente imprevisto
        public int Ocurridos { get; private set; }

        System.Random azar;

        void Awake() => Reiniciar(ControlCocina.Instancia);

        void OnEnable() => EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        void OnDisable() => EventosCocina.OnTurnoIniciado -= AlIniciarTurno;

        void AlIniciarTurno(TurnoData t) => Reiniciar(ControlCocina.Instancia);

        // Al empezar cada turno el reloj vuelve a primerImprevisto. La prueba del núcleo pasa su
        // propio ControlCocina y una semilla fija.
        public void Reiniciar(ControlCocina c, int? semillaFija = null)
        {
            if (semillaFija.HasValue) semilla = semillaFija.Value;
            azar = semilla != 0 ? new System.Random(semilla) : new System.Random();
            Faltan = c != null && c.Config != null ? c.Config.primerImprevisto : 22f;
            Ocurridos = 0;
        }

        void Update()
        {
            var c = ControlCocina.Instancia;
            if (c != null) Avanzar(c, Time.deltaTime);
        }

        // Separado de Update para que la prueba del núcleo lo pueda llamar con su propio tiempo.
        public void Avanzar(ControlCocina c, float dt)
        {
            if (!c.PuedeHaberImprevisto) return;
            Faltan -= dt;
            if (Faltan > 0f) return;

            var cfg = c.Config;
            Faltan = Mathf.Lerp(cfg.intervaloImprevisto.x, cfg.intervaloImprevisto.y, (float)azar.NextDouble());
            Ocurridos++;

            var mesa = c.RibosomaMesa;
            bool puedeHorquilla = !c.HorquillaActiva && mesa != null && !mesa.Lejano && !mesa.listo;
            if (puedeHorquilla && azar.NextDouble() < 0.5) c.ActivarHorquilla();
            else c.ActivarCodonRaro();
        }
    }
}
