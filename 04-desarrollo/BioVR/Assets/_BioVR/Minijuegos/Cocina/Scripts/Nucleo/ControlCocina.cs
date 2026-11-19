using System;
using System.Collections.Generic;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // El cerebro de la Cocina: guarda el estado del turno, aplica las reglas, suma los puntos y lanza
    // los eventos. Hay uno solo en la escena. Los módulos le PREGUNTAN (por ejemplo, ¿entra este ARNt?)
    // y le AVISAN (por ejemplo, el jugador sacudió un ARNt); nunca deciden por su cuenta ni suman puntos.
    //
    // Cómo se lee este archivo:
    //   1. Datos y estado          → qué guarda
    //   2. Turno y tiempo          → Iniciar, Paso (el "bucle" del HTML), pausas y tareas con retraso
    //   3. Lo que pregunta M2      → Necesarios, SegundosEspera
    //   4. Sintetasa (M3)          → PuedeEntrarASintetasa, TerminarCarga, Sacudir
    //   5. Ribosomas (M4)          → IntentarColocarEnRibosoma, Aceptar, TocarFin
    //   6. Salida y reciclaje (M5) → Agarrar, Reciclar
    //   7. Imprevistos (M6)        → horquilla y codón raro
    //   8. Fase B (M7 y M10)       → TocarCuenta, UsarChaperona, UsarProteasoma
    //   9. Final                   → Terminar
    //
    // Los nombres entre paréntesis (HTML: …) son las funciones del prototipo de Uli que hacen lo mismo.
    public class ControlCocina : MonoBehaviour
    {
        // ================= 1. Datos y estado =================

        public static ControlCocina Instancia { get; private set; }

        [Header("Datos")]
        [SerializeField] ConfigCocina config;
        [SerializeField] CodigoGenetico codigo;
        [SerializeField] TextosCocina textos;
        [SerializeField] TurnoData[] turnos;

        [Header("Arranque")]
        [Tooltip("Turno con el que empieza la escena (0 = turno 1). -1 = no empieza solo; alguien llama a IniciarTurno")]
        [SerializeField] int turnoInicial = 0;
        [Tooltip("Semilla del azar. 0 = distinta cada vez. Otro número = siempre los mismos errores de la sintetasa (para probar)")]
        [SerializeField] int semilla = 0;

        public ConfigCocina Config => config;
        public CodigoGenetico Codigo => codigo;
        public TextosCocina Textos => textos;
        public IReadOnlyList<TurnoData> Turnos => turnos;

        public TurnoData Turno { get; private set; }
        public int NumeroTurno { get; private set; } = -1;
        public FaseCocina Fase { get; private set; } = FaseCocina.Fin;
        public bool Pausado { get; private set; }
        public MarcadorCocina Marcador { get; private set; } = new MarcadorCocina();

        readonly List<Codon> codones = new List<Codon>();
        public IReadOnlyList<Codon> Codones => codones;

        readonly List<RibosomaEstado> ribosomas = new List<RibosomaEstado>();
        public IReadOnlyList<RibosomaEstado> Ribosomas => ribosomas;
        public RibosomaEstado RibosomaMesa => ribosomas.Count > 0 ? ribosomas[0] : null;

        readonly List<ARNtCocina> salida = new List<ARNtCocina>();
        public IReadOnlyList<ARNtCocina> Salida => salida;
        public bool SalidaLlena => Turno != null && salida.Count >= Turno.capacidadSalida;

        // El ARNt que el jugador está girando en el ribosoma de mesa (HTML: st.llave). null = ninguno.
        public ARNtCocina ARNtEnLlave { get; private set; }

        public bool HorquillaActiva { get; private set; }
        public int GolpesHorquilla { get; private set; }
        public float SegundosRaro { get; private set; }
        public bool CodonRaroActivo => SegundosRaro > 0f;
        // Segundos desde que algún ribosoma avanzó (M2 hace brillar los ARNt que sirven a los config.esperaPista s).
        public float SegundosEspera { get; private set; }
        // En Busca, después de varios errores, la cuenta correcta parpadea (M7).
        public bool PistaCuenta { get; private set; }

        System.Random azar;

        // Tareas con retraso (HTML: luego). Avanzan con Paso, así que se detienen en pausa.
        class Tarea { public float faltan; public Action accion; public int turno; }
        readonly List<Tarea> tareas = new List<Tarea>();

        void Awake()
        {
            if (Instancia != null && Instancia != this)
                Debug.LogWarning("[Cocina] Hay dos ControlCocina en la escena; se usa el último.", this);
            Instancia = this;
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        void Start()
        {
            if (turnoInicial >= 0) IniciarTurno(turnoInicial);
        }

        void Update()
        {
            if (!Pausado) Paso(Time.deltaTime);
        }

        // ================= 2. Turno y tiempo =================

        // Para la prueba del núcleo y para armar escenas por código.
        public void Configurar(ConfigCocina config, CodigoGenetico codigo, TextosCocina textos, TurnoData[] turnos, int semilla)
        {
            this.config = config;
            this.codigo = codigo;
            this.textos = textos;
            this.turnos = turnos;
            this.semilla = semilla;
        }

        public void IniciarTurno(int numero)
        {
            if (turnos == null || numero < 0 || numero >= turnos.Length || turnos[numero] == null)
            {
                Debug.LogError($"[Cocina] No existe el turno {numero}. Revisa la lista de turnos de ControlCocina.", this);
                return;
            }
            NumeroTurno = numero;
            Iniciar(turnos[numero]);
        }

        public bool HaySiguienteTurno => turnos != null && NumeroTurno + 1 < turnos.Length;
        public void SiguienteTurno() { if (HaySiguienteTurno) IniciarTurno(NumeroTurno + 1); }
        public void RepetirTurno() { if (Turno != null) Iniciar(Turno); }

        // (HTML: iniciar)
        public void Iniciar(TurnoData turno)
        {
            if (config == null || codigo == null || turno == null)
            {
                Debug.LogError("[Cocina] Faltan datos: ControlCocina necesita ConfigCocina, CodigoGenetico y el turno.", this);
                return;
            }
            Turno = turno;
            azar = semilla != 0 ? new System.Random(semilla) : new System.Random();
            Marcador = new MarcadorCocina();
            tareas.Clear();
            salida.Clear();
            ARNtEnLlave = null;
            HorquillaActiva = false;
            GolpesHorquilla = 0;
            SegundosRaro = 0f;
            SegundosEspera = 0f;
            PistaCuenta = false;
            Pausado = false;

            ArmarCodones(turno);
            ribosomas.Clear();
            for (int i = 0; i < turno.ribosomas.Length; i++)
                ribosomas.Add(new RibosomaEstado(i, turno.ribosomas[i]));

            Fase = FaseCocina.Armando;
            EventosCocina.TurnoIniciado(turno);
            EventosCocina.FaseCambiada(Fase);
            foreach (var r in ribosomas) EventosCocina.CodonCambiado(r);
            if (!string.IsNullOrEmpty(turno.claveIntro)) Decir(turno.claveIntro);
        }

        // Traduce los codones del turno. Avisa si no coinciden con la secuencia (por si alguien edita el asset).
        void ArmarCodones(TurnoData turno)
        {
            codones.Clear();
            int mut = turno.CodonMutacion;
            for (int i = 0; i < turno.codones.Length; i++)
            {
                string t = turno.codones[i].Trim().ToUpperInvariant();
                bool paro = codigo.EsParo(t);
                var aa = paro ? null : codigo.Traducir(t);
                codones.Add(new Codon(t, aa, paro, i == mut));

                if (i > 0 && !paro && i - 1 < turno.secuencia.Length && i != mut)
                {
                    char esperado = turno.secuencia[i - 1];
                    if (aa == null || aa.letra != esperado)
                        Debug.LogWarning($"[Cocina] {turno.name}: el codón {i} ({t}) no da {esperado}. Revisa el asset.", turno);
                }
            }
            if (codones.Count == 0 || !codones[codones.Count - 1].esParo)
                Debug.LogError($"[Cocina] {turno.name}: el último codón debe ser de paro.", turno);
        }

        // Avanza el juego dt segundos (HTML: bucle). Lo llama Update; la prueba del núcleo lo llama a mano.
        public void Paso(float dt)
        {
            if (Turno == null) return;

            if (Fase == FaseCocina.Armando || Fase == FaseCocina.Plegando) Marcador.tiempo += dt;
            SegundosEspera += dt;

            if (SegundosRaro > 0f)
            {
                SegundosRaro -= dt;
                if (SegundosRaro <= 0f) { SegundosRaro = 0f; EventosCocina.CodonRaro(false); }
            }

            // Avance automático en los codones que no pone el jugador (HTML: r.auto = .13)
            if (Fase == FaseCocina.Armando)
            {
                foreach (var r in ribosomas)
                {
                    if (r.listo || r.indice >= codones.Count) continue;
                    var c = codones[r.indice];
                    if (r.EsManual(r.indice, c.esParo)) continue;
                    // La horquilla frena al ribosoma de mesa también en los avances automáticos
                    if (!r.Lejano && HorquillaActiva) continue;
                    r.relojAutomatico -= dt;
                    if (r.relojAutomatico <= 0f)
                    {
                        r.relojAutomatico = config.avanceAutomatico;
                        Avanzar(r, c.aminoacido);
                    }
                }
            }

            // Tareas con retraso. Se copian porque una tarea puede agregar otra.
            if (tareas.Count > 0)
            {
                var vencidas = new List<Tarea>();
                foreach (var t in tareas)
                {
                    t.faltan -= dt;
                    if (t.faltan <= 0f) vencidas.Add(t);
                }
                foreach (var t in vencidas)
                {
                    tareas.Remove(t);
                    t.accion();
                }
            }
        }

        // La usa el tutorial (M8): mientras hay una ventana abierta, el juego se detiene.
        public void PonerPausa(bool pausar)
        {
            if (Pausado == pausar) return;
            Pausado = pausar;
            EventosCocina.Pausa(pausar);
        }

        void Luego(float segundos, Action accion) => tareas.Add(new Tarea { faltan = segundos, accion = accion });

        void Decir(string clave) => EventosCocina.Mensaje(clave);

        // Los módulos piden aquí los sonidos que son solo de animación (el tic de la llave, la manivela, el vuelo),
        // porque solo ControlCocina lanza eventos. Los sonidos de las reglas (acierto, error, pop…) ya los pone M1.
        // Las claves están en la especificación 10.6; M8 los reproduce si hay clip.
        public void Sonar(string clave) => EventosCocina.Sonido(clave);

        void Rechazar(string clave, string sonido = null)
        {
            if (sonido != null) Sonar(sonido);
            EventosCocina.Rechazo(clave);
            Decir(clave);
        }

        void CambiarFase(FaseCocina nueva)
        {
            Fase = nueva;
            EventosCocina.FaseCambiada(nueva);
        }

        // ================= 3. Lo que pregunta la corriente (M2) =================

        // El codón que este ribosoma pide ahora, o null si no pide nada (listo, avanzando solo o en el paro).
        public Codon? PideAhora(RibosomaEstado r)
        {
            if (r == null || r.listo || r.indice >= codones.Count) return null;
            var c = codones[r.indice];
            if (c.esParo || !r.EsManual(r.indice, false)) return null;
            return c;
        }

        // ¿Este ribosoma está en el codón de paro, esperando que el jugador toque FIN?
        public bool EnParo(RibosomaEstado r) => r != null && !r.listo && r.indice < codones.Count && codones[r.indice].esParo;

        // Los siguientes codones manuales de cada ribosoma (HTML: necesarios). M2 trae ARNt para estos.
        // Puede repetir codón si los dos ribosomas piden el mismo.
        public List<Codon> Necesarios()
        {
            var lista = new List<Codon>();
            foreach (var r in ribosomas)
            {
                if (r.listo) continue;
                int n = 0;
                for (int i = r.indice; i < codones.Count && n < config.codonesAdelantados; i++)
                {
                    if (codones[i].esParo) break;
                    if (r.EsManual(i, false)) { lista.Add(codones[i]); n++; }
                }
            }
            return lista;
        }

        // Un señuelo para la corriente: un codón preferido que difiere en 2+ bases del que se pide.
        public string Senuelo()
        {
            var nec = Necesarios();
            string referencia = nec.Count > 0 ? nec[0].tripleta : "AUG";
            var opciones = new List<string>();
            foreach (var e in codigo.aminoacidos)
                if (!string.IsNullOrEmpty(e.preferido) && Codon.Diferencias(e.preferido, referencia) >= config.diferenciaSenuelo)
                    opciones.Add(e.preferido);
            return opciones.Count > 0 ? opciones[azar.Next(opciones.Count)] : referencia;
        }

        // Máximo de ARNt en la corriente ahora mismo.
        public int MaximoEnCorriente => CodonRaroActivo ? config.maxEnCorrienteRaro : config.maxEnCorriente;

        // ================= 4. Sintetasa (M3) =================

        // ¿Se puede meter este ARNt en la sintetasa? (HTML: colocar con 'sint'). Si no, clave del rechazo.
        // Que la sintetasa tenga ranura libre lo revisa M3.
        public bool PuedeEntrarASintetasa(ARNtCocina t, out string clave)
        {
            clave = null;
            if (Fase != FaseCocina.Armando) clave = "rechazo_no_pide";
            else if (t.Equivocado) clave = "rechazo_sacudir_primero";
            else if (t.Cargado) clave = "rechazo_ya_cargado";
            if (clave != null) { Rechazar(clave); return false; }
            t.PonerLugar(LugarARNt.Sintetasa);
            Sonar("clic");
            return true;
        }

        // La barra de carga llegó al 100 % (HTML: manivela). M1 decide qué aminoácido pone:
        // con probabilidad errorSintetasa (una sola vez por ARNt) pone uno parecido equivocado.
        // forzarError es solo para la prueba del núcleo.
        public void TerminarCarga(ARNtCocina t, bool? forzarError = null)
        {
            if (t.Cargado) return;
            var aa = t.AminoacidoPropio;
            bool falla = forzarError ?? (!t.yaFalloSintetasa && azar.NextDouble() < Turno.errorSintetasa);
            if (falla)
            {
                aa = codigo.Confundir(aa, azar);
                t.yaFalloSintetasa = true;
            }
            t.PonerCarga(aa);
            bool equivocado = t.Equivocado;
            if (equivocado) { Sonar("malo"); Decir("sintetasa_error"); }
            else Sonar("cargado");
            EventosCocina.ARNtCargado(t, equivocado);
        }

        // El jugador sacudió un ARNt cargado (HTML: sacudir). Le quita el aminoácido.
        public void Sacudir(ARNtCocina t)
        {
            if (!t.Cargado) return;
            bool eraEquivocado = t.Equivocado;
            t.PonerCarga(null);
            Sonar("sacude");
            if (eraEquivocado)
            {
                Marcador.arreglos++;
                Marcador.Sumar(config.puntosSacudida);
                Decir("sacudido_equivocado");
            }
            else Decir("sacudido_bueno");
            EventosCocina.ARNtSacudido(t, eraEquivocado);
        }

        // ================= 5. Ribosomas (M4) =================

        // El jugador soltó (o aventó) un ARNt en un ribosoma (HTML: colocar con 'riboA'/'riboB').
        // Revisa en el orden de la especificación 2.5 y devuelve si entró.
        public ResultadoColocacion IntentarColocarEnRibosoma(ARNtCocina t, RibosomaEstado r, out string clave)
        {
            clave = null;
            string sonido = null;
            if (Fase != FaseCocina.Armando || r == null) clave = "rechazo_no_pide";
            // 1. horquilla
            else if (!r.Lejano && HorquillaActiva) clave = "rechazo_horquilla";
            // 2. listo, en el paro o sin pedir
            else if (r.listo || r.indice >= codones.Count) clave = "rechazo_no_pide";
            else if (codones[r.indice].esParo) clave = "rechazo_fin";
            else if (!r.EsManual(r.indice, false)) clave = "rechazo_no_pide";
            // 3. ya hay uno girando en la mesa
            else if (!r.Lejano && ARNtEnLlave != null) clave = "rechazo_llave_ocupada";
            // 4. vacío
            else if (!t.Cargado) { clave = "rechazo_sin_carga"; sonido = "error"; }
            // 5. no encaja: es el único rechazo que cuenta como error
            else if (t.CodonQueLee != codones[r.indice].tripleta)
            {
                clave = "rechazo_no_encaja"; sonido = "error";
                Marcador.errores++;
                Marcador.racha = 0;
            }
            // 6. salida llena
            else if (SalidaLlena) { clave = "rechazo_salida_llena"; sonido = "error"; }

            if (clave != null) { Rechazar(clave, sonido); return ResultadoColocacion.Rechazado; }

            // 7. entra
            if (r.Lejano)
            {
                Aceptar(t, r);
                return ResultadoColocacion.Aceptado;
            }
            ARNtEnLlave = t;
            t.PonerLugar(LugarARNt.Llave);
            Sonar("clic");
            Decir("llave_girar");
            return ResultadoColocacion.Llave;
        }

        // El ARNt entra al ribosoma (HTML: aceptar). M4 la llama cuando el ARNt de la mesa hace clic
        // al girarlo; para el lejano la llama IntentarColocarEnRibosoma.
        public void Aceptar(ARNtCocina t, RibosomaEstado r)
        {
            if (Fase != FaseCocina.Armando || r == null || r.listo) return;
            if (ARNtEnLlave == t) ARNtEnLlave = null;
            t.PonerLugar(LugarARNt.Ribosoma);

            if (t.Equivocado)
            {
                // El ribosoma solo revisa que codón y anticodón encajen; no puede ver el aminoácido.
                r.malos.Add(r.indice);
                Marcador.aminoacidosMalos++;
                Marcador.errores++;
                Marcador.racha = 0;
                Marcador.Sumar(config.puntosMaloEntro);
                Sonar("malo");
                Decir("malo_entro");
                Avanzar(r, t.Carga);
                EventosCocina.AminoacidoAceptado(r, false);
                Luego(config.retrasoSalidaMalo, () => MandarASalida(t));
                return;
            }

            Marcador.aciertos++;
            Marcador.racha++;
            Marcador.Sumar(config.puntosAcierto + Mathf.Min(Marcador.racha, config.rachaMaxima) * config.puntosPorRacha);
            if (!r.Lejano) Sonar("llave"); // el clic de la llave al encajar
            Sonar("acierto");
            Decir(r.Lejano ? "acierto_lejano" : "acierto_mesa");
            Avanzar(r, t.Carga);
            EventosCocina.AminoacidoAceptado(r, true);
            Luego(config.retrasoSalida, () => MandarASalida(t));
        }

        // (HTML: avanzar)
        void Avanzar(RibosomaEstado r, AminoacidoData aa)
        {
            r.cadena.Add(aa);
            r.indice++;
            SegundosEspera = 0f;
            EventosCocina.CodonCambiado(r);
        }

        // El jugador tocó el botón FIN de un ribosoma (HTML: tocarFin).
        public void TocarFin(RibosomaEstado r)
        {
            if (Fase != FaseCocina.Armando || !EnParo(r)) return;
            r.listo = true;
            r.indice++;
            Marcador.cadenasTerminadas++;
            Sonar("escala");
            EventosCocina.CodonCambiado(r);
            EventosCocina.CadenaTerminada(r);

            bool todos = true;
            foreach (var x in ribosomas) if (!x.listo) todos = false;
            if (!todos) { Decir("cadena_lista_sigue"); return; }

            // Se soltó la última cadena: M2, M3 y M5 esconden lo suyo al ver la fase Plegando
            salida.Clear();
            ARNtEnLlave = null;
            if (HorquillaActiva) { HorquillaActiva = false; EventosCocina.Horquilla(false); }
            if (CodonRaroActivo) { SegundosRaro = 0f; EventosCocina.CodonRaro(false); }
            CambiarFase(FaseCocina.Plegando);
            Decir("plegando");

            Luego(config.esperaPlegado, () =>
            {
                if (!Turno.TieneMutacion)
                {
                    Sonar("pliega");
                    Decir(Turno.ribosomas.Length > 1 ? "final_pedido" : "final_practica");
                    Luego(config.esperaFinal, Terminar);
                }
                else
                {
                    Sonar("torcida");
                    CambiarFase(FaseCocina.Busca);
                    Decir("busca");
                }
            });
        }

        // ================= 6. Salida y reciclaje (M5) =================

        // (HTML: el luego() de aceptar) El ARNt usado, ya vacío, pasa a la salida.
        void MandarASalida(ARNtCocina t)
        {
            if (Fase != FaseCocina.Armando || t == null) return;
            t.PonerCarga(null);
            t.PonerUsado(true);
            t.PonerLugar(LugarARNt.Salida);
            salida.Add(t);
            EventosCocina.ARNtASalida(t);
        }

        // El jugador agarró un ARNt (de la corriente, la sintetasa o la salida). M2, M3 o M5 lo avisan
        // para que M1 lo saque de donde estaba. Devuelve falso si no se puede agarrar ahora.
        public bool Agarrar(ARNtCocina t)
        {
            if (Fase != FaseCocina.Armando) return false;
            if (t.Lugar == LugarARNt.Vuelo || t.Lugar == LugarARNt.Ribosoma || t.Lugar == LugarARNt.Llave) return false;
            salida.Remove(t);
            t.PonerLugar(LugarARNt.Mano);
            Sonar("clic");
            return true;
        }

        // El ARNt va volando a un destino (lo aventaron). Al llegar, M2/M5 llaman a la función del destino.
        public void EnVuelo(ARNtCocina t) => t.PonerLugar(LugarARNt.Vuelo);

        // El jugador lo soltó sin destino o no entró: regresa a la corriente (HTML: devolver).
        public void DevolverACorriente(ARNtCocina t)
        {
            if (ARNtEnLlave == t) ARNtEnLlave = null;
            salida.Remove(t);
            t.PonerLugar(LugarARNt.Corriente);
            Sonar("cae");
        }

        // Llegó al regreso al citoplasma (HTML: colocar con 'recicla'). Si era usado, da puntos.
        public void Reciclar(ARNtCocina t)
        {
            bool eraUsado = t.Usado;
            salida.Remove(t);
            t.PonerCarga(null);
            t.PonerUsado(false);
            t.PonerLugar(LugarARNt.Corriente);
            Sonar("recicla");
            if (eraUsado)
            {
                Marcador.reciclados++;
                Marcador.Sumar(config.puntosReciclado);
                Decir("reciclado");
            }
            EventosCocina.Reciclado(t);
        }

        // ================= 7. Imprevistos (M6) =================
        // M6 decide CUÁNDO (con primerImprevisto e intervaloImprevisto); M1 guarda el estado y avisa.

        public bool PuedeHaberImprevisto => Fase == FaseCocina.Armando && Turno != null && Turno.imprevistos && !Pausado;

        // Aparece la horquilla en el ARNm del ribosoma de mesa. Falso si no se puede (ya hay una o la mesa terminó).
        public bool ActivarHorquilla()
        {
            var mesa = RibosomaMesa;
            if (!PuedeHaberImprevisto || HorquillaActiva || mesa == null || mesa.Lejano || mesa.listo) return false;
            HorquillaActiva = true;
            GolpesHorquilla = 0;
            Sonar("alarma");
            Decir("horquilla");
            EventosCocina.Horquilla(true);
            return true;
        }

        // El jugador golpeó la horquilla (HTML: golpear). A los golpesHorquilla se deshace. Devuelve 0 a 1 (qué tan abierta).
        public float GolpearHorquilla()
        {
            if (!HorquillaActiva) return 1f;
            GolpesHorquilla++;
            Sonar("golpe");
            if (GolpesHorquilla < config.golpesHorquilla) return (float)GolpesHorquilla / config.golpesHorquilla;
            HorquillaActiva = false;
            Marcador.golpes++;
            Marcador.Sumar(config.puntosHorquilla);
            Sonar("pop");
            Decir("horquilla_abierta");
            EventosCocina.Horquilla(false);
            return 1f;
        }

        // Empieza el codón raro: por duracionRaro segundos caben más ARNt en la corriente.
        public bool ActivarCodonRaro()
        {
            if (!PuedeHaberImprevisto) return false;
            bool yaEstaba = CodonRaroActivo;
            SegundosRaro = config.duracionRaro;
            if (!yaEstaba)
            {
                Sonar("alarma");
                Decir("raro");
                EventosCocina.CodonRaro(true);
            }
            return true;
        }

        // ================= 8. Fase B (M7 y M10) =================

        // El jugador tocó una cuenta del holograma (HTML: clicEsfera). i = número del residuo de la amilina
        // (1 a 37), que es el mismo que el número de codón. La S20G es TocarCuenta(20).
        public void TocarCuenta(int i)
        {
            if (Fase != FaseCocina.Busca) return;
            if (i == Turno.CodonMutacion)
            {
                Marcador.Sumar(config.puntosCuenta);
                PistaCuenta = false;
                Sonar("acierto");
                CambiarFase(FaseCocina.Decide);
                Decir("cuenta_encontrada");
                EventosCocina.CuentaEncontrada();
                return;
            }
            Marcador.erroresBusca++;
            Sonar("error");
            if (Marcador.erroresBusca >= config.erroresBuscaParaPista)
            {
                PistaCuenta = true;
                Decir("busca_pista");
            }
            else Decir("busca_error");
            EventosCocina.ErrorBusca(Marcador.erroresBusca);
        }

        // (HTML: clicChap) Una sola vez por turno.
        public bool UsarChaperona()
        {
            if (Fase != FaseCocina.Decide || Marcador.usoChaperona) return false;
            Marcador.usoChaperona = true;
            Sonar("chaperona");
            Decir("chaperona");
            EventosCocina.Chaperona();
            return true;
        }

        // (HTML: clicProt)
        public bool UsarProteasoma()
        {
            if (Fase != FaseCocina.Decide) return false;
            CambiarFase(FaseCocina.Final);
            Marcador.Sumar(Marcador.usoChaperona ? config.puntosProteasomaConChaperona : config.puntosProteasoma);
            Sonar("tritura");
            Decir("proteasoma");
            EventosCocina.Proteasoma();
            Luego(config.esperaProteasoma, Terminar);
            return true;
        }

        // ================= 9. Final =================

        // (HTML: terminar) M8 muestra la pantalla final al recibir OnTerminado.
        public void Terminar()
        {
            if (Fase == FaseCocina.Fin) return;
            CambiarFase(FaseCocina.Fin);
            Sonar("estrella");
            EventosCocina.Terminado(Marcador);
        }

        public int Estrellas => Turno != null ? Marcador.Estrellas(Turno.estrellas) : 0;

        // Texto de una clave (atajo para M8 y los letreros).
        public string Texto(string clave) => textos != null ? textos.Texto(clave) : $"[{clave}]";
    }
}
