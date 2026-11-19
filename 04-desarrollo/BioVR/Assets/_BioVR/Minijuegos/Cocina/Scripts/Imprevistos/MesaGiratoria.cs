using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M6 · Mesa giratoria del turno 3 (especificación 7.2). Se enciende con el interruptor "mesaGira" del
    // TurnoData; si está apagado, el turno funciona igual pero nada se mueve.
    //
    // Mueve la sintetasa y la salida por el borde de la mesa, siempre mirando al jugador:
    //   sintetasa: ángulo = 31 + 16·sen(0.32·t)      (de +15° a +47°), radio 0.58
    //   salida:    ángulo = −30 + 14·sen(0.32·t + 2) (de −44° a −16°), radio 0.52
    //
    // Mueve las MARCAS de Cocina_Base ("Sintetasa" y "Salida"): M3 y M5 ponen su prefab como hijo de su marca,
    // así que se mueven con ella. La altura de cada una no cambia. Solo avanza en la fase Armando y sin pausa.
    public class MesaGiratoria : MonoBehaviour
    {
        [SerializeField] Transform sintetasa;
        [SerializeField] Transform salida;

        [Header("Sintetasa")]
        [SerializeField] float centroSintetasa = 31f;
        [SerializeField] float amplitudSintetasa = 16f;
        [SerializeField] float radioSintetasa = 0.58f;

        [Header("Salida")]
        [SerializeField] float centroSalida = -30f;
        [SerializeField] float amplitudSalida = 14f;
        [SerializeField] float radioSalida = 0.52f;
        [SerializeField] float desfaseSalida = 2f;

        [Header("Velocidad")]
        [SerializeField] float velocidad = 0.32f; // rad/s

        public bool Girando { get; private set; }

        float t;
        Pose inicioSintetasa, inicioSalida;

        void Awake()
        {
            if (sintetasa != null) inicioSintetasa = new Pose(sintetasa.localPosition, sintetasa.localRotation);
            if (salida != null) inicioSalida = new Pose(salida.localPosition, salida.localRotation);
        }

        void OnEnable() => EventosCocina.OnTurnoIniciado += AlIniciarTurno;
        void OnDisable() => EventosCocina.OnTurnoIniciado -= AlIniciarTurno;

        // Cada turno empieza con las dos en su lugar de Cocina_Base.
        void AlIniciarTurno(TurnoData turno)
        {
            t = 0f;
            Regresar(sintetasa, inicioSintetasa);
            Regresar(salida, inicioSalida);
        }

        void Update()
        {
            var c = ControlCocina.Instancia;
            Girando = c != null && c.Turno != null && c.Turno.mesaGira && c.Fase == FaseCocina.Armando && !c.Pausado;
            if (!Girando) return;

            t += Time.deltaTime;
            Mover(sintetasa, centroSintetasa + amplitudSintetasa * Mathf.Sin(velocidad * t), radioSintetasa);
            Mover(salida, centroSalida + amplitudSalida * Mathf.Sin(velocidad * t + desfaseSalida), radioSalida);
        }

        static void Mover(Transform x, float angulo, float radio)
        {
            if (x == null) return;
            PosicionMesa.Colocar(x, angulo, radio, x.localPosition.y);
        }

        static void Regresar(Transform x, Pose p)
        {
            if (x == null) return;
            x.localPosition = p.position;
            x.localRotation = p.rotation;
        }
    }
}
