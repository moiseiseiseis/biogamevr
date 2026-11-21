using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    // M5 · La bandeja de salida (especificación 6.2).
    //
    //   - Tiene tantos lugares como Turno.capacidadSalida, en fila y separados 0.085 m.
    //   - En OnARNtASalida el ARNt usado (ya vacío) vuela a su lugar y se acuesta (−0.5 rad).
    //   - De ahí se puede agarrar: M5 avisa a M1 con ControlCocina.Agarrar y el lugar queda libre.
    //   - Cuando M1 dice que la salida está llena (SalidaLlena), el letrero parpadea en rojo.
    //   - En el turno 3 la mesa gira: los lugares se calculan cada cuadro desde la posición actual de la bandeja.
    //   - En la fase Plegando la bandeja desaparece.
    //
    // M5 no suma puntos ni decide nada: solo coloca, avisa y muestra (las reglas son de M1).
    public class BandejaSalida : MonoBehaviour
    {
        [SerializeField] GameObject visual;                 // modelo y letrero; se esconde en Plegando
        [SerializeField] Transform centro;                  // centro de la fila de lugares, sobre la bandeja
        [SerializeField] TextMeshPro letrero;
        [SerializeField] float separacion = 0.085f;
        [SerializeField] float acostado = -0.5f;            // radianes, alrededor del eje X de la bandeja
        [SerializeField] float duracionVuelo = 0.35f;
        [SerializeField] float alturaVuelo = 0.08f;
        [SerializeField] Color colorLlena = new Color(1f, 0.25f, 0.25f);
        [SerializeField] float parpadeosPorSegundo = 2.5f;

        [Header("Textos (TextosCocina, con respaldo)")]
        [SerializeField] string claveLetrero = "salida_letrero";
        [SerializeField] string respaldoLetrero = "SALIDA · usados (máx. {0})";

        class Lugar
        {
            public ARNtCocina arnt;
            public float vuelo;                 // 0 → 1 mientras vuela a su lugar
            public Vector3 desde;
            public Quaternion giroDesde;
            public UnityAction<SelectEnterEventArgs> alAgarrar;
        }

        readonly List<Lugar> lugares = new List<Lugar>();
        Color colorLetrero = Color.white;

        static ControlCocina Control => ControlCocina.Instancia;

        public int Capacidad => lugares.Count;
        public int Ocupados { get { int n = 0; foreach (var l in lugares) if (l.arnt != null) n++; return n; } }

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnARNtASalida += AlLlegar;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            if (letrero != null) colorLetrero = letrero.color;
            if (Control != null && Control.Turno != null) AlIniciarTurno(Control.Turno);
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnARNtASalida -= AlLlegar;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            Vaciar();
        }

        void AlIniciarTurno(TurnoData turno)
        {
            Vaciar();
            lugares.Clear();
            int n = Mathf.Max(1, turno.capacidadSalida);
            for (int i = 0; i < n; i++) lugares.Add(new Lugar());
            if (visual != null) visual.SetActive(true);
            if (letrero != null)
            {
                string formato = Control != null && Control.Textos != null && Control.Textos.Tiene(claveLetrero)
                    ? Control.Texto(claveLetrero) : respaldoLetrero;
                letrero.text = string.Format(formato, n);
                letrero.color = colorLetrero;
            }
        }

        void AlCambiarFase(FaseCocina fase)
        {
            if (fase == FaseCocina.Armando) return;
            Vaciar();
            if (fase == FaseCocina.Plegando && visual != null) visual.SetActive(false);
        }

        // M1 mandó un ARNt usado a la salida
        void AlLlegar(ARNtCocina t)
        {
            if (t == null) return;
            var lugar = lugares.Find(l => l.arnt == null);
            if (lugar == null)
            {
                // Puede pasar si dos ribosomas aceptan casi a la vez con la salida casi llena: se agrega un lugar extra
                lugar = new Lugar();
                lugares.Add(lugar);
            }
            lugar.arnt = t;
            lugar.vuelo = 0f;
            lugar.desde = t.transform.position;
            lugar.giroDesde = t.transform.rotation;

            PuenteCorriente.Soltar(t);
            PuenteCorriente.PermitirAgarre(t, true);
            var agarre = t.GetComponent<XRGrabInteractable>();
            if (agarre != null)
            {
                var l = lugar;
                l.alAgarrar = args => AlAgarrar(l, args);
                agarre.selectEntered.AddListener(l.alAgarrar);
            }
        }

        void AlAgarrar(Lugar l, SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor || l.arnt == null) return;
            if (l.arnt.Lugar == LugarARNt.Salida) Control.Agarrar(l.arnt);
            Liberar(l);
        }

        void Update()
        {
            bool llena = Control != null && Control.Fase == FaseCocina.Armando && Control.SalidaLlena;
            if (letrero != null)
            {
                letrero.color = llena
                    ? Color.Lerp(colorLetrero, colorLlena, 0.5f + 0.5f * Mathf.Sin(Time.time * parpadeosPorSegundo * 2f * Mathf.PI))
                    : colorLetrero;
            }

            for (int i = 0; i < lugares.Count; i++)
            {
                var l = lugares[i];
                if (l.arnt == null) continue;
                // Ya no está en la salida (lo recicló, lo agarró otro módulo o se limpió el turno): se libera
                if (l.arnt.Lugar != LugarARNt.Salida) { Liberar(l); continue; }

                Posicion(i, out var destino, out var giro);
                if (l.vuelo < 1f)
                {
                    l.vuelo = Mathf.Min(1f, l.vuelo + Time.deltaTime / Mathf.Max(0.01f, duracionVuelo));
                    float k = Mathf.SmoothStep(0f, 1f, l.vuelo);
                    var p = Vector3.Lerp(l.desde, destino, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * alturaVuelo;
                    l.arnt.transform.SetPositionAndRotation(p, Quaternion.Slerp(l.giroDesde, giro, k));
                }
                else
                {
                    l.arnt.transform.SetPositionAndRotation(destino, giro); // sigue a la bandeja si la mesa gira
                }
                var cuerpo = l.arnt.GetComponent<Rigidbody>();
                if (cuerpo != null && !cuerpo.isKinematic) { cuerpo.linearVelocity = Vector3.zero; cuerpo.angularVelocity = Vector3.zero; }
            }
        }

        // Lugar i de la fila, centrada en "centro" y a lo largo de su eje X; acostado alrededor de su eje X
        void Posicion(int i, out Vector3 posicion, out Quaternion giro)
        {
            var c = centro != null ? centro : transform;
            float x = (i - (lugares.Count - 1) * 0.5f) * separacion;
            posicion = c.TransformPoint(new Vector3(x, 0f, 0f));
            giro = c.rotation * Quaternion.Euler(acostado * Mathf.Rad2Deg, 0f, 0f);
        }

        void Liberar(Lugar l)
        {
            if (l.arnt != null && l.alAgarrar != null)
            {
                var agarre = l.arnt.GetComponent<XRGrabInteractable>();
                if (agarre != null) agarre.selectEntered.RemoveListener(l.alAgarrar);
            }
            l.arnt = null;
            l.alAgarrar = null;
            l.vuelo = 0f;
        }

        void Vaciar()
        {
            foreach (var l in lugares) Liberar(l);
        }

        // Para pruebas y para M2: el primer ARNt que está quieto en la bandeja
        public ARNtCocina Primero()
        {
            foreach (var l in lugares) if (l.arnt != null && l.vuelo >= 1f) return l.arnt;
            return null;
        }
    }
}
