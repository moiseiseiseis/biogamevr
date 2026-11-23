using System;
using System.Collections;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina
{
    // Tutorial guiado de la Cocina (después de la prueba de Marcos del 21-nov: con ventanas de "Entendido" no supo
    // cargar el aminoácido, ni vaciar la bandeja, ni qué hacer con la chaperona y el proteasoma).
    //
    // En vez de explicar todo al principio, dice en cada momento UNA cosa: qué hacer, con qué botón y dónde, con la
    // flecha de MarcaTutorial encima del objeto. Avanza solo cuando el jugador lo hace.
    //
    // El paso no es una lista fija: se calcula con el estado de la mesa (Etapa). Así, si el jugador se adelanta o se
    // regresa (suelta el ARNt, lo vuelve a agarrar), la guía lo sigue sin trabarse.
    //   - Pasos de información (bienvenida, reglas nuevas): pausan el juego y se cierran con A / X o Entendido.
    //   - Pasos de acción: no pausan; desaparecen al cumplirse.
    //   - Interrupciones (error de la sintetasa, horquilla, bandeja llena, codón raro): se muestran encima del paso
    //     actual hasta resolverse.
    //   - Los rechazos de ControlCocina ("¡No encaja!") salen en rojo abajo del paso. El panel de mensajes se apaga
    //     mientras hay guía, para que no haya dos letreros hablando al mismo tiempo.
    //
    // Usa la ventana de TutorialCocina (TutorialCocina.MostrarGuia). Los textos están aquí, por turno.
    // Izquierda y derecha son las de la especificación 1.2: corriente y sintetasa a la izquierda; bandeja, embudo,
    // holograma y proteasoma a la derecha.
    public class GuiaCocina : MonoBehaviour
    {
        const string Grip = "<b>GRIP</b> (botón de tu dedo medio)";
        const string Gatillo = "<b>GATILLO</b> (botón de tu dedo índice)";

        // La guía se recalcula 10 veces por segundo (busca ARNt en la escena; no hace falta en cada cuadro)
        static readonly WaitForSeconds Tic = new WaitForSeconds(0.1f);

        TutorialCocina ventana;
        Coroutine guion;
        bool entendido, esperandoEntendido;
        string rechazo; float rechazoHasta;
        int reciclados;

        // Paso de acción actual (lo pone el guion; Update lo muestra)
        string titulo, cuerpo, etiqueta;
        Func<Transform> objetivo;

        // Interrupción actual (tiene prioridad sobre el paso)
        string iTitulo, iCuerpo; Func<Transform> iObjetivo; Func<bool> iResuelta;

        string mostrado; // para no reescribir la ventana en cada cuadro
        GameObject panelMensajes;

        ControlCocina C => ControlCocina.Instancia;

        public void Usar(TutorialCocina v) => ventana = v;

        void OnEnable()
        {
            EventosCocina.OnTurnoIniciado += AlIniciarTurno;
            EventosCocina.OnRechazo += AlRechazar;
            EventosCocina.OnARNtCargado += AlCargar;
            EventosCocina.OnHorquilla += AlHorquilla;
            EventosCocina.OnCodonRaro += AlCodonRaro;
            EventosCocina.OnReciclado += AlReciclar;
            EventosCocina.OnMensaje += AlMensaje;
        }

        void OnDisable()
        {
            EventosCocina.OnTurnoIniciado -= AlIniciarTurno;
            EventosCocina.OnRechazo -= AlRechazar;
            EventosCocina.OnARNtCargado -= AlCargar;
            EventosCocina.OnHorquilla -= AlHorquilla;
            EventosCocina.OnCodonRaro -= AlCodonRaro;
            EventosCocina.OnReciclado -= AlReciclar;
            EventosCocina.OnMensaje -= AlMensaje;
        }

        void Start()
        {
            var pm = FindFirstObjectByType<PanelMensajes>(FindObjectsInactive.Include);
            if (pm != null) { panelMensajes = pm.gameObject; panelMensajes.SetActive(false); }
            // Si el turno ya empezó antes que la guía (orden de Start), arrancar su guion
            if (C != null && C.Turno != null && guion == null) AlIniciarTurno(C.Turno);
        }

        // Botón Entendido o A / X (los manda TutorialCocina)
        public void Entendido() { if (esperandoEntendido) entendido = true; }

        void AlIniciarTurno(TurnoData _)
        {
            if (guion != null) StopCoroutine(guion);
            Limpiar();
            iResuelta = null;
            reciclados = 0;
            int n = C != null ? C.NumeroTurno : 0;
            guion = StartCoroutine(n <= 0 ? Turno1() : n == 1 ? Turno2y3(false) : Turno2y3(true));
        }

        // ======================= Guiones =======================

        IEnumerator Turno1()
        {
            yield return Info("Bienvenido a la Cocina de proteínas",
                "Vas a fabricar una proteína como lo hace un ribosoma. Te voy a decir qué hacer, paso por paso.\n" +
                $"• {Grip}: agarrar y soltar.\n• {Gatillo} + rayo: tocar botones.\n• <b>A</b> o <b>X</b>: continuar.   • <b>B</b> o <b>Y</b>: acomodar la mesa.",
                null, "Aprieta A o X para empezar");
            yield return Info("El mensaje",
                $"Frente a ti está el <b>ribosoma</b>. Sobre él pasa el <b>ARNm</b>: el mensaje. El codón que se ve <b>más grande</b> es el que pide: ahora pide <b>{CodonDe(C.RibosomaMesa)}</b>. " +
                "Le vas a traer el ARNt que encaja con él.", Ribosoma(false), "Aprieta A o X");

            for (int vuelta = 0; vuelta < 3; vuelta++)
            {
                var mesa = C.RibosomaMesa;
                int indice = mesa.indice;
                // Guiar este aminoácido hasta que el ribosoma avance
                while (mesa.indice == indice && !C.EnParo(mesa))
                {
                    GuiarAminoacido(mesa, vuelta == 0 ? 2 : vuelta == 1 ? 1 : 0, $"Aminoácido {vuelta + 1} de 3");
                    yield return Tic;
                }
                // En el primero se enseña a vaciar la bandeja (en el turno 1 caben 2: al tercero ya no cabría)
                if (vuelta == 0)
                {
                    int antes = reciclados;
                    while (reciclados == antes)
                    {
                        GuiarReciclar(detallado: true, "Último paso del ciclo");
                        yield return Tic;
                    }
                    yield return Info("¡Eso es todo el ciclo!",
                        "1. Agarra el ARNt que encaja.  2. Cárgalo en la sintetasa.  3. Llévalo al ribosoma y gíralo.  4. Avienta el usado al embudo.\n" +
                        "Ahora repítelo con el siguiente codón. Te ayudo menos cada vez.", null, "Aprieta A o X");
                }
            }

            yield return GuiarFin(C.RibosomaMesa);
            yield return GuiarFinal();
        }

        IEnumerator Turno2y3(bool turno3)
        {
            if (!turno3)
            {
                yield return Info("Turno 2 · Pedido de amilina",
                    "Ahora hay <b>dos ribosomas</b> leyendo el mismo mensaje: el de tu mesa y uno <b>lejano</b> (al frente, sobre el anillo magenta). " +
                    "Los dos avanzan solos y se detienen en un tramo de <b>5 codones</b> que te toca poner a ti.", Ribosoma(true), "Aprieta A o X");
                yield return Info("Reglas nuevas",
                    "• Al ribosoma lejano no lo alcanzas: carga su ARNt y <b>aviéntaselo</b>.\n• La sintetasa a veces se equivoca: si el ARNt sale con <b>!</b>, sacúdelo.\n" +
                    "• El ARNm puede doblarse en una <b>horquilla</b>: golpéala.\nTe aviso cuando pase cada cosa.", null, "Aprieta A o X");
            }
            else
            {
                yield return Info("Turno 3 · Un mensaje con algo raro",
                    "Otro pedido de 2 amilinas. Ahora <b>la mesa gira</b>: la sintetasa y la bandeja se mueven despacio por el borde. " +
                    "El mensaje trae un cambio: arma las cadenas como siempre y al final las revisamos.", null, "Aprieta A o X");
            }

            bool lejanoEnsenado = false;
            while (C.Fase == FaseCocina.Armando)
            {
                RibosomaEstado quien = null;
                foreach (var r in C.Ribosomas)
                    if (!r.listo && (C.PideAhora(r) != null || C.EnParo(r))) { quien = r; break; }

                if (C.SalidaLlena || (quien == null && C.Salida.Count > 0))
                    GuiarReciclar(detallado: reciclados == 0, quien == null ? "Mientras los ribosomas avanzan" : "La bandeja está llena");
                else if (quien != null && C.EnParo(quien))
                    PonerPaso("Toca FIN", TextoFin(quien), () => BotonFinDe(quien), "El ribosoma terminó");
                else if (quien != null)
                {
                    int detalle = quien.Lejano ? (lejanoEnsenado ? 0 : 2) : 0;
                    GuiarAminoacido(quien, detalle, quien.Lejano ? "Ribosoma lejano" : "Ribosoma de mesa");
                }
                else
                {
                    string sig = SiguienteManual();
                    PonerPaso("Los ribosomas avanzan solos",
                        sig != null ? $"Prepárate: el siguiente codón que te toca es <b>{sig}</b>. Puedes ir cargando su ARNt en la sintetasa." :
                                      "Espera un momento a que lleguen a tu tramo.",
                        null, "Espera");
                }
                // Se da por enseñado el lejano cuando aceptó su primer ARNt
                foreach (var r in C.Ribosomas) if (r.Lejano && r.indice > r.definicion.manualDesde) lejanoEnsenado = true;
                yield return Tic;
            }

            if (turno3)
            {
                yield return Esperar("La proteína se está doblando", "Mira el <b>holograma</b>, a tu derecha: la cadena se dobla sola.",
                                     Holograma, () => C.Fase != FaseCocina.Plegando);
                if (C.Fase == FaseCocina.Busca)
                {
                    yield return Esperar("Busca el error",
                        $"Se dobló diferente. En el <b>holograma</b> cada esfera es un aminoácido y <b>una es distinta</b>. Apúntale con el rayo y aprieta el {Gatillo}. " +
                        "Si fallas dos veces, la correcta empieza a parpadear.", Holograma, () => C.Fase != FaseCocina.Busca, "Fase final");
                    yield return Esperar("¡La encontraste! Ahora decide",
                        "Es la mutación <b>S20G</b>: donde iba serina quedó glicina. Esta amilina forma fibras tóxicas (los depósitos rosas). " +
                        $"Mándala a degradar: apunta al <b>PROTEASOMA</b> (el barril, a tu derecha) y aprieta el {Gatillo}.",
                        Proteasoma, () => C.Fase != FaseCocina.Decide, "Fase final");
                }
            }
            yield return GuiarFinal();
        }

        // ======================= Pasos =======================

        // Etapa del ARNt que sirve para el codón que pide este ribosoma:
        //   0 nadie lo tiene · 1 en la mano, vacío · 2 en la sintetasa, vacío · 3 en la sintetasa, cargado
        //   4 en la mano, cargado · 5 girando en la llave (solo el de mesa)
        int Etapa(RibosomaEstado r) => Etapa(r, out _);

        int Etapa(RibosomaEstado r, out ARNtCocina cual)
        {
            cual = null;
            var pide = C.PideAhora(r);
            if (pide == null) return 0;
            string codon = pide.Value.tripleta;
            int mejor = 0;
            foreach (var t in FindObjectsByType<ARNtCocina>(FindObjectsSortMode.None))
            {
                if (t.CodonQueLee != codon || t.Usado) continue;
                int e = 0;
                if (t == C.ARNtEnLlave && !r.Lejano) e = 5;
                else if (t.Lugar == LugarARNt.Mano || t.Lugar == LugarARNt.Vuelo) e = t.Cargado ? 4 : 1;
                else if (t.Lugar == LugarARNt.Sintetasa) e = t.Cargado ? 3 : 2;
                if (e > mejor) { mejor = e; cual = t; }
            }
            return mejor;
        }

        // detalle: 2 = todo explicado, 1 = medio, 0 = corto
        void GuiarAminoacido(RibosomaEstado r, int detalle, string etiquetaPaso)
        {
            int etapa = Etapa(r, out var arnt);
            var pide = C.PideAhora(r);
            string codon = pide != null ? pide.Value.tripleta : "";
            string aa = pide != null && pide.Value.aminoacido != null ? pide.Value.aminoacido.abreviatura : "";
            string anticodon = Codon.Complemento(codon);

            switch (etapa)
            {
                case 0:
                    var enMano = EnLaMano();
                    string aviso = enMano != null && !enMano.Usado && enMano.CodonQueLee != codon
                        ? $"\n<color=#E05050>Ese ARNt ({enMano.Anticodon}) no encaja. Suéltalo y agarra el que señala la flecha.</color>" : "";
                    PonerPaso("Agarra el ARNt que encaja",
                        detalle == 2
                            ? $"El ribosoma pide <b>{codon}</b>. En la <b>corriente</b> (el tubo a tu izquierda) busca el ARNt con las piezas <b>{anticodon}</b> abajo: " +
                              $"A encaja con U, G con C. La flecha te lo señala. Acércale la mano y aprieta el {Grip}.{aviso}"
                            : detalle == 1
                                ? $"Pide <b>{codon}</b>: en la corriente (izquierda) agarra con el GRIP el ARNt <b>{anticodon}</b> que señala la flecha.{aviso}"
                                : $"Pide <b>{codon}</b> → ARNt <b>{anticodon}</b>, en la corriente.{aviso}",
                        () => ARNtEnCorriente(codon), etiquetaPaso);
                    break;
                case 1:
                    PonerPaso("Llévalo a la sintetasa",
                        detalle > 0
                            ? "Los ARNt de la corriente vienen <b>vacíos</b>. Llévalo a la <b>sintetasa</b> (la máquina con manivela, a tu izquierda) y <b>suelta el GRIP</b> encima de una de sus ranuras."
                            : "Suéltalo en la sintetasa (izquierda).",
                        Sintetasa, etiquetaPaso);
                    break;
                case 2:
                    PonerPaso("Gira la manivela",
                        detalle > 0
                            ? $"Agarra la <b>manivela</b> de la sintetasa con el {Grip} y dale vueltas, como si batieras, hasta llenar la <b>barra verde</b> (unas 2 vueltas)."
                            : "Dale vueltas a la manivela hasta llenar la barra verde.",
                        Manivela, etiquetaPaso);
                    break;
                case 3:
                    PonerPaso("Saca el ARNt cargado",
                        detalle > 0 ? $"¡Ya tiene su aminoácido ({aa})! Agárralo de la sintetasa con el {Grip}." : $"Ya trae {aa}: sácalo de la sintetasa.",
                        () => arnt != null ? arnt.transform : Sintetasa(), etiquetaPaso);
                    break;
                case 4:
                    if (r.Lejano)
                        PonerPaso("Aviéntalo al ribosoma lejano",
                            detalle > 0
                                ? "El ribosoma lejano no se alcanza. <b>Apunta con el brazo</b> hacia él (al frente, sobre el anillo magenta), muévelo rápido y <b>suelta el GRIP</b> a medio movimiento, como si lanzaras una pelota."
                                : "Aviéntaselo al ribosoma lejano.",
                            () => Ribosoma(true)(), etiquetaPaso);
                    else
                        PonerPaso("Llévalo al ribosoma",
                            detalle > 0 ? "Llévalo al <b>ribosoma</b>, frente a ti, y <b>suelta el GRIP</b> justo encima del codón que brilla." : "Suéltalo sobre el ribosoma.",
                            () => Ribosoma(false)(), etiquetaPaso);
                    break;
                case 5:
                    PonerPaso("Gíralo como una llave",
                        detalle > 0
                            ? $"Entró chueco. <b>Agárralo otra vez</b> con el {Grip} y <b>gira la muñeca</b>, como cuando abres una puerta con llave, hasta que haga <b>clic</b>."
                            : "Agárralo y gira la muñeca hasta el clic.",
                        () => C.ARNtEnLlave != null ? C.ARNtEnLlave.transform : Ribosoma(false)(), etiquetaPaso);
                    break;
            }
        }

        void GuiarReciclar(bool detallado, string etiquetaPaso)
        {
            var enMano = EnLaMano();
            if (enMano != null && enMano.Usado)
                PonerPaso("Aviéntalo al embudo naranja",
                    detallado
                        ? "Ahora <b>aviéntalo al embudo</b> con aro naranja (lejos, a tu derecha): mueve el brazo rápido hacia él y <b>suelta el GRIP</b> a medio movimiento. " +
                          $"También puedes <b>apuntarle con el rayo y apretar el GATILLO</b>. Vuelve a la corriente y ganas +50."
                        : "Aviéntalo al embudo naranja (o apúntale y aprieta el gatillo).",
                    Embudo, etiquetaPaso);
            else
                PonerPaso("Recoge el ARNt usado",
                    detallado
                        ? $"Al dejar su aminoácido, el ARNt salió <b>vacío</b> a la <b>bandeja</b> (a tu derecha). Si la bandeja se llena, el ribosoma se detiene. Agárralo con el {Grip}."
                        : "Agarra el ARNt usado de la bandeja (derecha).",
                    UsadoEnBandeja, etiquetaPaso);
        }

        IEnumerator GuiarFin(RibosomaEstado r)
        {
            while (!r.listo)
            {
                PonerPaso("Toca FIN", TextoFin(r), () => BotonFinDe(r), "Fin de la cadena");
                yield return Tic;
            }
        }

        string TextoFin(RibosomaEstado r) =>
            $"El {(r.Lejano ? "ribosoma lejano" : "ribosoma")} llegó al <b>codón de paro</b> (el bloque rojo). Apunta con el rayo al <b>cilindro rojo FIN</b>, junto al ribosoma, y aprieta el {Gatillo}. Así sueltas la cadena.";

        IEnumerator GuiarFinal()
        {
            while (C.Fase != FaseCocina.Fin)
            {
                PonerPaso("Mira el holograma", "Tu cadena se está doblando en el <b>holograma</b>, a tu derecha.", Holograma, "");
                yield return Tic;
            }
            bool hay = C.HaySiguienteTurno;
            while (true)
            {
                PonerPaso(hay ? "¡Turno completado!" : "¡Pedido completado!",
                    hay ? $"Revisa tus puntos y estrellas. Para seguir, apunta al botón <b>SIGUIENTE</b> de la pantalla y aprieta el {Gatillo}."
                        : $"Terminaste la Cocina. Apunta a <b>REPETIR</b> para jugar otra vez o a <b>MENÚ</b> para salir, y aprieta el {Gatillo}.",
                    PantallaFinal, "");
                yield return Tic; // termina cuando empieza otro turno (AlIniciarTurno detiene este guion)
            }
        }

        // Paso de información: pausa el juego y espera A / X o Entendido
        IEnumerator Info(string t, string c, Func<Transform> o, string pie)
        {
            C?.PonerPausa(true);
            entendido = false; esperandoEntendido = true;
            titulo = t; cuerpo = c; objetivo = o; etiqueta = pie;
            Refrescar(true);
            yield return new WaitForSecondsRealtime(0.5f); // que no se cierre con el mismo botón que lo abrió
            while (!entendido) yield return null;
            esperandoEntendido = false;
            C?.Sonar("clic");
            C?.PonerPausa(false);
            Limpiar();
        }

        // Paso de acción que espera una condición
        IEnumerator Esperar(string t, string c, Func<Transform> o, Func<bool> listo, string pie = "")
        {
            while (!listo())
            {
                PonerPaso(t, c, o, pie);
                yield return Tic;
            }
        }

        void PonerPaso(string t, string c, Func<Transform> o, string pie)
        {
            titulo = t; cuerpo = c; objetivo = o; etiqueta = pie;
        }

        void Limpiar() { titulo = cuerpo = etiqueta = null; objetivo = null; mostrado = null; ventana?.OcultarGuia(); }

        // ======================= Interrupciones y rechazos =======================

        void AlCargar(ARNtCocina t, bool equivocado)
        {
            if (!equivocado) return;
            Interrumpir("¡La sintetasa se equivocó!",
                $"Ese ARNt trae un aminoácido equivocado (tiene <b>!</b>). Sácalo de la sintetasa con el {Grip} y <b>sacúdelo</b> de lado a lado, rápido, hasta que lo suelte. Luego vuelve a cargarlo.",
                () => t != null ? t.transform : null, () => t == null || !t.Equivocado);
        }

        void AlHorquilla(bool aparece)
        {
            if (!aparece) return;
            Interrumpir("¡Horquilla en el ARNm!",
                "El mensaje se dobló en un lazo y el ribosoma no puede avanzar. <b>Golpéala 3 veces</b> con la mano para desenrollarla.",
                Horquilla, () => !C.HorquillaActiva);
        }

        void AlCodonRaro(bool activo)
        {
            if (!activo) return;
            Interrumpir("¡Codón raro!",
                "El ARNt que encaja casi no existe en la célula. Por unos segundos pasan <b>más ARNt</b> por la corriente: búscalo bien.",
                null, () => !C.CodonRaroActivo);
        }

        void AlRechazar(string clave)
        {
            rechazo = C != null ? C.Texto(clave) : clave;
            rechazoHasta = Time.unscaledTime + 4f;
            mostrado = null;
        }

        void AlReciclar(ARNtCocina _) => reciclados++;

        // Con el panel de mensajes apagado, los avisos importantes salen en la ventana de la guía
        void AlMensaje(string clave)
        {
            if (clave == "malo_entro" || clave == "busca_error") AlRechazar(clave);
        }

        void Interrumpir(string t, string c, Func<Transform> o, Func<bool> resuelta)
        {
            iTitulo = t; iCuerpo = c; iObjetivo = o; iResuelta = resuelta;
            C?.Sonar("pop");
            mostrado = null;
        }

        // ======================= Mostrar =======================

        void Update()
        {
            if (ventana == null || C == null) return;
            if (iResuelta != null && iResuelta()) { iResuelta = null; mostrado = null; }
            if (rechazo != null && Time.unscaledTime > rechazoHasta) { rechazo = null; mostrado = null; }
            Refrescar(esperandoEntendido);
        }

        void Refrescar(bool conBoton)
        {
            if (ventana == null) return;
            bool interrupcion = iResuelta != null && !esperandoEntendido;
            string t = interrupcion ? iTitulo : titulo;
            if (string.IsNullOrEmpty(t)) { if (mostrado != null) { mostrado = null; ventana.OcultarGuia(); } return; }
            string c = interrupcion ? iCuerpo : cuerpo;
            if (rechazo != null && !conBoton) c += $"\n<color=#E05050><b>{rechazo}</b></color>";
            string pie = interrupcion ? "Arréglalo para seguir" : etiqueta;
            var o = interrupcion ? iObjetivo : objetivo;
            var destino = o != null ? o() : null;

            string clave = t + c + pie + conBoton + (destino != null ? destino.GetInstanceID() : 0);
            if (clave == mostrado) return;
            mostrado = clave;
            ventana.MostrarGuia(t, c, pie, conBoton, destino);
        }

        // ======================= Dónde está cada cosa =======================

        static ARNtCocina EnLaMano()
        {
            foreach (var t in FindObjectsByType<ARNtCocina>(FindObjectsSortMode.None))
                if (t.Lugar == LugarARNt.Mano) return t;
            return null;
        }

        Transform ARNtEnCorriente(string codon)
        {
            foreach (var t in FindObjectsByType<ARNtCocina>(FindObjectsSortMode.None))
                if (t.CodonQueLee == codon && !t.Usado && t.Lugar == LugarARNt.Corriente) return t.transform;
            var corriente = FindFirstObjectByType<CorrienteARNt>();
            return corriente != null ? corriente.transform : null;
        }

        Transform UsadoEnBandeja()
        {
            foreach (var t in C.Salida) if (t != null) return t.transform;
            var b = FindFirstObjectByType<BandejaSalida>();
            return b != null ? b.transform : null;
        }

        Func<Transform> Ribosoma(bool lejano) => () =>
        {
            foreach (var r in RibosomaCocina.Todos)
                if (r != null && r.lejano == lejano) return r.SitioA != null ? r.SitioA : r.transform;
            return null;
        };

        Transform BotonFinDe(RibosomaEstado estado)
        {
            foreach (var b in FindObjectsByType<BotonFin>(FindObjectsSortMode.None))
            {
                var rc = b.GetComponentInParent<RibosomaCocina>();
                if (rc == null || rc.Estado == estado) return b.transform;
            }
            return Ribosoma(estado.Lejano)();
        }

        static Transform Sintetasa() { var s = FindFirstObjectByType<EstacionSintetasa>(); return s != null ? s.transform : null; }
        static Transform Manivela() { var m = FindFirstObjectByType<Manivela>(); return m != null ? m.transform : Sintetasa(); }
        static Transform Embudo() { foreach (var r in RegresoCitoplasma.Todos) if (r != null) return r.Boca != null ? r.Boca : r.transform; return null; }
        static Transform Holograma() { var h = FindFirstObjectByType<HologramaCocina>(); return h != null ? h.transform : null; }
        static Transform Proteasoma() { var p = FindFirstObjectByType<Proteasoma26S>(); return p != null ? p.transform : null; }
        static Transform Horquilla() { var h = FindFirstObjectByType<HorquillaARNm>(); return h != null ? h.transform : null; }
        static Transform PantallaFinal() { var p = FindFirstObjectByType<PantallaFinal>(); return p != null ? p.transform : null; }

        string CodonDe(RibosomaEstado r) { var p = C.PideAhora(r); return p != null ? p.Value.tripleta : "?"; }

        string SiguienteManual()
        {
            var lista = C.Necesarios();
            return lista.Count > 0 ? lista[0].tripleta : null;
        }
    }
}
