using BioVR.Secuencia;
using UnityEngine;

namespace BioVR.Entornos
{
    // Anillos de contención de la sala de reducción (sala-reduccion_lowpoly de Diseño, de Uli).
    // En el abordaje giran despacio y van acelerando, como si se cargaran (con SFX-ESC-01 y AMB-RED-02);
    // al empezar el primer salto (la reducción) giran rápido. Los gira este script, no la animación del FBX
    // (que se importa apagada: trae también balizas y el brazo de captura mezclados).
    public class AnillosReduccion : MonoBehaviour
    {
        [SerializeField] Transform[] anillos = new Transform[0];
        [Tooltip("Eje local de giro de cada anillo (el del modelo: exterior, medio, interior).")]
        [SerializeField] Vector3[] ejes = new Vector3[0];
        [SerializeField] float velocidadInicial = 10f;   // grados por segundo al entrar al abordaje
        [SerializeField] float velocidadCargada = 60f;   // a los "segundosCarga"
        [SerializeField] float segundosCarga = 15f;
        [SerializeField] float velocidadReduccion = 400f; // durante el salto

        GameManager gm;
        float velocidad, objetivo, reloj;
        bool cargando;

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnEstadoCambiado += AlCambiarEstado;
            gm.OnSaltoIniciado += AlIniciarSalto;
            AlCambiarEstado(gm.Estado);
        }

        void OnDestroy()
        {
            if (gm == null) return;
            gm.OnEstadoCambiado -= AlCambiarEstado;
            gm.OnSaltoIniciado -= AlIniciarSalto;
        }

        void AlCambiarEstado(GameState estado)
        {
            cargando = estado == GameState.Abordaje;
            reloj = 0f;
            velocidad = objetivo = cargando ? velocidadInicial : 0f;
        }

        void AlIniciarSalto(int salto)
        {
            if (salto == 1) { cargando = false; objetivo = velocidadReduccion; }
        }

        void Update()
        {
            if (cargando)
            {
                reloj += Time.deltaTime;
                objetivo = Mathf.Lerp(velocidadInicial, velocidadCargada, reloj / Mathf.Max(0.1f, segundosCarga));
            }
            velocidad = Mathf.MoveTowards(velocidad, objetivo, 300f * Time.deltaTime);
            for (int i = 0; i < anillos.Length; i++)
            {
                if (anillos[i] == null) continue;
                var eje = i < ejes.Length ? ejes[i] : Vector3.up;
                // Los anillos alternan el sentido para que se vea como giroscopio
                anillos[i].Rotate(eje, velocidad * (i % 2 == 0 ? 1f : -1.3f) * Time.deltaTime, Space.Self);
            }
        }
    }
}
