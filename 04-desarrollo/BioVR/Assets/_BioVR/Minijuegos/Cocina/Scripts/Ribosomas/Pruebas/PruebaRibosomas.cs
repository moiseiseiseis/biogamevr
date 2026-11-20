using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BioVR.Minijuegos.Cocina
{
    // SOLO PARA Cocina_M4 (escena de prueba de Iker). Sustituye a la corriente (M2), la sintetasa (M3) y la
    // salida (M5) mientras no estén en main, para poder jugar los ribosomas de punta a punta:
    //
    //   - Un "estante" a la izquierda con ARNt YA CARGADOS (ControlCocina.TerminarCarga) para los codones que
    //     piden los ribosomas, más un señuelo. Se reponen solos.
    //   - Los ARNt que M1 manda a la salida se reciclan solos (ControlCocina.Reciclar) y desaparecen.
    //   - Si el jugador suelta un ARNt fuera del ribosoma, o el ribosoma lo rechaza, vuelve a su lugar.
    //
    // Teclas (editor): 1/2/3 = turno · M = ARNt con el aminoácido EQUIVOCADO · L = aventar al ribosoma lejano
    //                  P = poner en la mesa el ARNt que pide · K (mantener) = girar la llave · Enter = FIN de la mesa.
    // En el PR: decir que la prueba usa ARNt de prueba ya cargados (sin sintetasa).
    public class PruebaRibosomas : MonoBehaviour
    {
        [SerializeField] GameObject prefabARNt;
        [SerializeField] int maximo = 7;
        [Tooltip("Estante: de este ángulo hacia el frente, a este radio y altura (PosicionMesa)")]
        [SerializeField] float anguloInicio = 62f;
        [SerializeField] float pasoAngulo = 8f;
        [SerializeField] float radio = 0.55f;
        [SerializeField] float altura = 1.0f;
        [SerializeField] float intervalo = 0.5f;

        readonly List<ARNtCocina> arnts = new List<ARNtCocina>();
        readonly Dictionary<ARNtCocina, int> lugares = new Dictionary<ARNtCocina, int>();
        float reloj;

        static ControlCocina Control => ControlCocina.Instancia;

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnARNtASalida += AlSalir;
            EventosCocina.OnFaseCambiada += AlCambiarFase;
            PuenteCorriente.OnDevolver += AlDevolver;
            if (Control != null && Control.Turno != null) AlIniciarTurno(Control.Turno);
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnARNtASalida -= AlSalir;
            EventosCocina.OnFaseCambiada -= AlCambiarFase;
            PuenteCorriente.OnDevolver -= AlDevolver;
        }

        void AlIniciarTurno(TurnoData t)
        {
            Limpiar();
            reloj = 0f;
        }

        void AlCambiarFase(FaseCocina f)
        {
            if (f != FaseCocina.Armando) Limpiar();
        }

        void Update()
        {
            if (Control == null || Control.Fase != FaseCocina.Armando || Control.Pausado) return;

            reloj -= Time.deltaTime;
            if (reloj <= 0f)
            {
                reloj = intervalo;
                Rellenar();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var k = Keyboard.current;
            if (k == null) return;
            if (k.digit1Key.wasPressedThisFrame) Control.IniciarTurno(0);
            if (k.digit2Key.wasPressedThisFrame) Control.IniciarTurno(1);
            if (k.digit3Key.wasPressedThisFrame) Control.IniciarTurno(2);
            if (k.mKey.wasPressedThisFrame) CrearEquivocado();
            if (k.lKey.wasPressedThisFrame) AventarAlLejano();
            if (k.pKey.wasPressedThisFrame) PonerEnMesa();
            if (k.kKey.isPressed) { var m = Mesa(); if (m != null && m.Llave != null) m.Llave.GirarPrueba(90f * Time.deltaTime); }
            if (k.enterKey.wasPressedThisFrame) { var m = Mesa(); if (m != null) Control.TocarFin(m.Estado); }
#endif
        }

        // Un ARNt cargado para cada codón que piden los ribosomas, y un señuelo
        void Rellenar()
        {
            var necesarios = Control.Necesarios();
            var disponibles = new List<ARNtCocina>();
            foreach (var a in arnts)
                if (a != null && (a.Lugar == LugarARNt.Corriente || a.Lugar == LugarARNt.Mano)) disponibles.Add(a);

            foreach (var c in necesarios)
            {
                var hay = disponibles.Find(a => a.CodonQueLee == c.tripleta && !a.Equivocado);
                if (hay != null) { disponibles.Remove(hay); continue; }
                if (arnts.Count >= maximo) return;
                Crear(c.tripleta, equivocado: false);
                return; // uno por vuelta, como la corriente
            }

            bool haySenuelo = disponibles.Count > 0;
            if (!haySenuelo && arnts.Count < maximo) Crear(Control.Senuelo(), equivocado: false);
        }

        void CrearEquivocado()
        {
            var necesarios = Control.Necesarios();
            if (necesarios.Count == 0 || arnts.Count >= maximo + 2) return;
            Crear(necesarios[0].tripleta, equivocado: true);
        }

        ARNtCocina Crear(string codon, bool equivocado)
        {
            if (prefabARNt == null) { Debug.LogError("[M4 prueba] Falta el prefab del ARNt de prueba.", this); return null; }
            int lugar = LugarLibre();
            if (lugar < 0) return null;

            var go = Instantiate(prefabARNt, transform.parent);
            go.name = $"ARNt de prueba {codon}{(equivocado ? " (equivocado)" : "")}";
            var t = go.GetComponent<ARNtCocina>();
            t.Configurar(codon, Control.Codigo.Traducir(codon));
            Control.TerminarCarga(t, equivocado);

            arnts.Add(t);
            lugares[t] = lugar;
            Poner(t);

            var agarre = go.GetComponent<XRGrabInteractable>();
            if (agarre != null)
            {
                agarre.selectEntered.AddListener(args => AlAgarrar(t, args));
                agarre.selectExited.AddListener(args => AlSoltar(t, args));
            }
            return t;
        }

        int LugarLibre()
        {
            for (int i = 0; i < maximo + 2; i++)
                if (!lugares.ContainsValue(i)) return i;
            return -1;
        }

        void Poner(ARNtCocina t)
        {
            if (!lugares.TryGetValue(t, out int i)) return;
            t.transform.localPosition = PosicionMesa.En(anguloInicio - i * pasoAngulo, radio, altura);
            PosicionMesa.MirarAlJugador(t.transform);
            var cuerpo = t.GetComponent<Rigidbody>();
            if (cuerpo != null && !cuerpo.isKinematic) { cuerpo.linearVelocity = Vector3.zero; cuerpo.angularVelocity = Vector3.zero; }
        }

        void AlAgarrar(ARNtCocina t, SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor) return;
            if (t.Lugar == LugarARNt.Llave) return; // lo está girando; M1 no lo saca de la llave
            Control.Agarrar(t);
        }

        // Si lo soltó y nadie lo recibió (sigue "en la mano"), regresa al estante
        void AlSoltar(ARNtCocina t, SelectExitEventArgs args)
        {
            if (args.isCanceled || args.interactorObject is XRSocketInteractor) return;
            StartCoroutine(RevisarAlSoltar(t));
        }

        IEnumerator RevisarAlSoltar(ARNtCocina t)
        {
            yield return null; // deja que la zona del sitio A lo reciba primero
            if (t != null && t.Lugar == LugarARNt.Mano)
            {
                Control.DevolverACorriente(t);
                Poner(t);
            }
        }

        void AlDevolver(ARNtCocina t)
        {
            if (lugares.ContainsKey(t)) Poner(t);
        }

        // M5 todavía no existe: los usados se reciclan solos para que la salida no se llene
        void AlSalir(ARNtCocina t)
        {
            if (!arnts.Contains(t)) return;
            StartCoroutine(Reciclar(t));
        }

        IEnumerator Reciclar(ARNtCocina t)
        {
            yield return new WaitForSeconds(0.3f);
            if (t == null) yield break;
            Control.Reciclar(t);
            Quitar(t);
        }

        static RibosomaCocina Mesa()
        {
            foreach (var r in RibosomaCocina.Todos) if (!r.lejano && r.Activo) return r;
            return null;
        }

        // Atajo de prueba: como si el jugador soltara en el sitio A el ARNt que pide la mesa
        void PonerEnMesa()
        {
            var mesa = Mesa();
            if (mesa == null) return;
            if (Control.EnParo(mesa.Estado)) { Debug.Log("[M4 prueba] La mesa está en el codón de paro: toca FIN (o Enter)."); return; }
            var pide = Control.PideAhora(mesa.Estado);
            if (!pide.HasValue) return;
            // Si hay uno equivocado (tecla M) lo pone primero, para probar la cuenta roja
            ARNtCocina t = null;
            foreach (var a in arnts)
                if (a != null && a.Lugar == LugarARNt.Corriente && a.CodonQueLee == pide.Value.tripleta && (t == null || a.Equivocado)) t = a;
            if (t == null) { Debug.Log("[M4 prueba] Todavía no hay en el estante un ARNt para " + pide.Value.tripleta); return; }
            Control.Agarrar(t);
            mesa.Recibir(t);
        }

        // Sustituye al lanzamiento de M2: el ARNt que pide el lejano vuela hacia él
        void AventarAlLejano()
        {
            RibosomaCocina lejano = null;
            foreach (var r in RibosomaCocina.Todos) if (r.lejano && r.Activo) lejano = r;
            if (lejano == null) { Debug.Log("[M4 prueba] Este turno no tiene ribosoma lejano (prueba el turno 2 o 3)."); return; }
            var pide = Control.PideAhora(lejano.Estado);
            ARNtCocina t = null;
            foreach (var a in arnts)
                if (a != null && a.Lugar == LugarARNt.Corriente && (!pide.HasValue || a.CodonQueLee == pide.Value.tripleta)) { t = a; break; }
            if (t == null) { Debug.Log("[M4 prueba] No hay un ARNt para aventar al lejano."); return; }
            StartCoroutine(Volar(t, lejano));
        }

        IEnumerator Volar(ARNtCocina t, RibosomaCocina destino)
        {
            Control.EnVuelo(t);
            var desde = t.transform.position;
            float dur = Control.Config.vueloLejos;
            for (float k = 0f; k < 1f; k += Time.deltaTime / dur)
            {
                var p = Vector3.Lerp(desde, destino.SitioA.position, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * Control.Config.alturaArcoVuelo;
                t.transform.position = p;
                yield return null;
            }
            if (destino.Recibir(t) == ResultadoColocacion.Rechazado) Poner(t);
        }

        void Quitar(ARNtCocina t)
        {
            arnts.Remove(t);
            lugares.Remove(t);
            if (t != null) Destroy(t.gameObject);
        }

        void Limpiar()
        {
            foreach (var a in new List<ARNtCocina>(arnts)) Quitar(a);
        }
    }
}
