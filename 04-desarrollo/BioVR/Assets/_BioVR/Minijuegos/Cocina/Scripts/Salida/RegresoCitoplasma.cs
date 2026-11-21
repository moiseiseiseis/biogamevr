using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // M5 · El regreso al citoplasma (especificación 6.2).
    //
    //   - Está lejos (1.6 m) y solo se llega AVENTANDO: el vuelo es de M2 (VueloARNt, 0.75 s con bamboleo).
    //     M5 se suscribe a VueloARNt.AlLlegarRegreso; la escena le da a VueloARNt la Boca como destino
    //     (destinoRegresoCitoplasma). Cualquier otro vuelo también puede llamar a Recibir(t) al llegar.
    //   - Recibir avisa a M1 (ControlCocina.Reciclar): si era usado, +50 y mensaje "reciclado"; en los dos
    //     casos el ARNt pierde la carga y vuelve vacío a la corriente (M2 lo pone al inicio de la corriente
    //     al escuchar OnReciclado). El sonido "recicla" lo lanza M1.
    //   - El aro naranja de la boca pulsa un momento al recibir.
    //
    // Destino para M2: RegresoCitoplasma.Todos (normalmente uno) y su Boca (hacia dónde apuntar y volar).
    public class RegresoCitoplasma : MonoBehaviour
    {
        public static readonly List<RegresoCitoplasma> Todos = new List<RegresoCitoplasma>();

        [SerializeField] Transform boca;                    // centro del aro: punto de llegada del vuelo
        [SerializeField] Transform aro;                     // aro naranja de 0.2 m de radio
        [SerializeField] TextMeshPro letrero;
        [SerializeField] float pulso = 0.25f;               // cuánto crece el aro al recibir
        [SerializeField] float duracionPulso = 0.35f;

        [Header("Textos (TextosCocina, con respaldo)")]
        [SerializeField] string claveLetrero = "regreso_letrero";
        [SerializeField] string respaldoLetrero = "REGRESO AL CITOPLASMA\n<size=60%>avienta aquí</size>";

        Vector3 escalaAro = Vector3.one;
        float relojPulso;
        VueloARNt vuelo;

        static ControlCocina Control => ControlCocina.Instancia;

        public Transform Boca => boca != null ? boca : transform;

        void OnEnable()
        {
            Todos.Add(this);
            if (aro != null) escalaAro = aro.localScale;
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            AlIniciarTurno(null);
        }

        void OnDisable()
        {
            Todos.Remove(this);
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
        }

        // En Start para que VueloARNt ya haya hecho su Awake
        void Start()
        {
            vuelo = VueloARNt.Instancia;
            if (vuelo != null) vuelo.AlLlegarRegreso += Recibir;
            else Debug.LogWarning("[M5] No hay VueloARNt (M2) en la escena: el regreso solo recibe vuelos de prueba.", this);
        }

        void OnDestroy()
        {
            if (vuelo != null) vuelo.AlLlegarRegreso -= Recibir;
        }

        void AlIniciarTurno(TurnoData _)
        {
            if (letrero == null) return;
            var textos = Control != null ? Control.Textos : null;
            letrero.text = textos != null && textos.Tiene(claveLetrero) ? textos.Texto(claveLetrero) : respaldoLetrero;
        }

        // Llegó un ARNt aventado (lo llama M2 al terminar el vuelo)
        public void Recibir(ARNtCocina t)
        {
            if (t == null || Control == null) return;
            PuenteCorriente.Soltar(t);
            Control.Reciclar(t);
            relojPulso = duracionPulso;
        }

        void Update()
        {
            if (aro == null) return;
            if (relojPulso > 0f)
            {
                relojPulso -= Time.deltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(1f - relojPulso / duracionPulso) * Mathf.PI);
                aro.localScale = escalaAro * (1f + pulso * k);
            }
            else aro.localScale = escalaAro;
        }
    }
}
