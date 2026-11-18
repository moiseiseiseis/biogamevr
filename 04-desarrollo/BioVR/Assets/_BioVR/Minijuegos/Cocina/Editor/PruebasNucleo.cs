using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Prueba de M1 sin visor ni escena (menú BioVR → Cocina → Probar núcleo).
    // Crea un ControlCocina temporal y juega por código, avanzando el tiempo a mano con Paso():
    //
    //   A. Datos: los 3 turnos cargan; el 3 trae GGC en el codón 20 y todos terminan en UAG
    //   B. Turno 1 completo: sin carga, uno que no encaja, uno equivocado que entra, salida llena,
    //      reciclar, sacudir, FIN y final. Revisa puntos, racha, contadores y fases
    //   C. Turno 2: avance automático hasta los codones manuales, Necesarios, señuelos y la horquilla
    //   D. Turno 3 completo con los dos ribosomas, la búsqueda de la cuenta 20, chaperona y proteasoma
    //
    // Escribe cada revisión en la consola con OK o FALLA y al final el total.
    public static class PruebasNucleo
    {
        static int fallas, revisadas;
        static readonly List<string> mensajes = new List<string>();
        static readonly List<FaseCocina> fases = new List<FaseCocina>();

        [MenuItem("BioVR/Cocina/Probar núcleo")]
        public static void Probar()
        {
            fallas = revisadas = 0;
            CrearDatosCocina.Crear();
            var config = AssetDatabase.LoadAssetAtPath<ConfigCocina>(CrearDatosCocina.RutaConfig);
            var codigo = AssetDatabase.LoadAssetAtPath<CodigoGenetico>(CrearDatosCocina.RutaCodigo);
            var textos = AssetDatabase.LoadAssetAtPath<TextosCocina>(CrearDatosCocina.RutaTextos);
            var turnos = CrearDatosCocina.RutasTurnos.Select(AssetDatabase.LoadAssetAtPath<TurnoData>).ToArray();

            var go = new GameObject("PruebaNucleo (temporal)") { hideFlags = HideFlags.HideAndDontSave };
            var basura = new List<GameObject> { go };
            EventosCocina.OnMensaje += AnotarMensaje;
            EventosCocina.OnFaseCambiada += AnotarFase;
            try
            {
                var control = go.AddComponent<ControlCocina>();
                control.Configurar(config, codigo, textos, turnos, semilla: 1234);

                ProbarDatos(control, codigo, textos, turnos);
                ProbarTurno1(control, codigo, basura);
                ProbarTurno2(control, codigo, basura);
                ProbarTurno3(control, codigo, basura);
            }
            catch (System.Exception e)
            {
                fallas++;
                Debug.LogException(e);
            }
            finally
            {
                EventosCocina.OnMensaje -= AnotarMensaje;
                EventosCocina.OnFaseCambiada -= AnotarFase;
                foreach (var g in basura) Object.DestroyImmediate(g);
            }

            if (fallas == 0) Debug.Log($"[Cocina] Prueba del núcleo: TODO BIEN ({revisadas} revisiones).");
            else Debug.LogError($"[Cocina] Prueba del núcleo: {fallas} de {revisadas} revisiones FALLARON. Busca 'FALLA' arriba.");
        }

        // ---------------- A. Datos ----------------
        static void ProbarDatos(ControlCocina c, CodigoGenetico codigo, TextosCocina textos, TurnoData[] turnos)
        {
            Seccion("A. Datos");
            Revisar(turnos.All(t => t != null), "Los 3 turnos existen");
            Revisar(codigo.aminoacidos.Count == 20, $"El código genético tiene 20 aminoácidos ({codigo.aminoacidos.Count})");
            Revisar(codigo.Traducir("AGC").letra == 'S' && codigo.Traducir("GGC").letra == 'G', "AGC = Ser y GGC = Gly");
            Revisar(codigo.EsParo("UAG") && codigo.Traducir("UAG") == null, "UAG es de paro");
            Revisar(new Codon("AUG", null, false, false).AnticodonRotulado == "3′ UAC 5′", "El anticodón de AUG se rotula 3′ UAC 5′");

            c.IniciarTurno(0);
            Revisar(string.Join(" ", c.Codones.Select(x => x.tripleta)) == "AUG GGC UUU UAG", "Turno 1: AUG GGC UUU UAG");

            c.IniciarTurno(1);
            Revisar(c.Codones.Count == 39, $"Turno 2: 39 codones (AUG + 37 + paro) ({c.Codones.Count})");
            Revisar(c.Codones[20].tripleta == "AGC" && !c.Codones[20].esMutacion, "Turno 2: el codón 20 es AGC (Ser)");
            Revisar(c.Codones.Skip(1).Take(37).Select(x => x.aminoacido.letra).SequenceEqual(CrearDatosCocina.Amilina),
                "Turno 2: los codones traducen la amilina madura KCNTATCATQRLANFLVHSSNNFGAILSSTNVGSNTY");

            c.IniciarTurno(2);
            Revisar(c.Codones[20].tripleta == "GGC" && c.Codones[20].esMutacion && c.Codones[20].aminoacido.letra == 'G',
                "Turno 3: el codón 20 es GGC (Gly) y está marcado como mutación");
            Revisar(c.Codones[c.Codones.Count - 1].tripleta == "UAG", "Turno 3: termina en UAG");
            Revisar(c.Turno.CodonMutacion == 20, "Turno 3: la mutación está en el codón 20");

            foreach (var clave in new[] { "rechazo_no_encaja", "malo_entro", "cuenta_encontrada", "chaperona", "proteasoma" })
                Revisar(textos.Tiene(clave), $"Existe el texto '{clave}'");
            Revisar(!textos.textos.Any(e => e.texto.ToLowerInvariant().Contains("diabetes")), "Ningún texto menciona la diabetes");
        }

        // ---------------- B. Turno 1 completo ----------------
        static void ProbarTurno1(ControlCocina c, CodigoGenetico codigo, List<GameObject> basura)
        {
            Seccion("B. Turno 1");
            c.IniciarTurno(0);
            var mesa = c.RibosomaMesa;
            var m = c.Marcador;

            var met = ARNt("AUG", codigo, basura);
            Colocar(c, met, mesa, ResultadoColocacion.Rechazado, "rechazo_sin_carga", "Un ARNt vacío no entra");

            c.TerminarCarga(met, forzarError: false);
            Revisar(met.Cargado && !met.Equivocado && met.Carga.letra == 'M', "La sintetasa carga Met en el ARNt de AUG");

            var val = ARNt("GUG", codigo, basura);
            c.TerminarCarga(val, forzarError: false);
            Colocar(c, val, mesa, ResultadoColocacion.Rechazado, "rechazo_no_encaja", "Val (GUG) no encaja en AUG");
            Revisar(m.errores == 1 && m.racha == 0, "No encajar cuenta como error y corta la racha");

            Colocar(c, met, mesa, ResultadoColocacion.Llave, null, "Met entra al ribosoma de mesa como llave");
            var otro = ARNt("AUG", codigo, basura);
            c.TerminarCarga(otro, forzarError: false);
            Colocar(c, otro, mesa, ResultadoColocacion.Rechazado, "rechazo_llave_ocupada", "No entra otro mientras uno gira");
            c.Aceptar(met, mesa);
            Revisar(m.puntos == 110 && m.racha == 1 && m.aciertos == 1, $"Met correcta: 110 puntos, racha 1 ({m.puntos}, {m.racha})");
            Revisar(mesa.indice == 1 && mesa.cadena.Count == 1, "El ribosoma avanzó al codón 1");
            c.Paso(1.5f);
            Revisar(c.Salida.Contains(met) && met.Usado && !met.Cargado, "A los 1.4 s el ARNt usado pasa vacío a la salida");

            var gly = ARNt("GGC", codigo, basura);
            c.TerminarCarga(gly, forzarError: true);
            Revisar(gly.Equivocado && gly.Carga.letra == 'A', $"Error forzado de la sintetasa: Gly se confunde con Ala ({gly.Carga})");
            Colocar(c, gly, mesa, ResultadoColocacion.Llave, null, "El ribosoma deja entrar el ARNt equivocado (solo revisa el anticodón)");
            c.Aceptar(gly, mesa);
            Revisar(m.puntos == 60 && m.aminoacidosMalos == 1 && m.errores == 2 && mesa.malos.Contains(1),
                $"Aminoácido equivocado: −50 (60), cuenta roja en el codón 1 ({m.puntos})");
            c.Paso(0.7f);
            Revisar(c.Salida.Count == 2 && c.SalidaLlena, "La salida del turno 1 se llena con 2");

            var phe = ARNt("UUU", codigo, basura);
            c.TerminarCarga(phe, forzarError: false);
            Colocar(c, phe, mesa, ResultadoColocacion.Rechazado, "rechazo_salida_llena", "Con la salida llena no entra");

            Revisar(c.Agarrar(met), "Se agarra un usado de la salida");
            c.Reciclar(met);
            Revisar(m.puntos == 110 && m.reciclados == 1 && c.Salida.Count == 1, $"Reciclar un usado: +50 (110) ({m.puntos})");

            var sac = ARNt("UUU", codigo, basura);
            c.TerminarCarga(sac, forzarError: true);
            Revisar(c.PuedeEntrarASintetasa(sac, out var claveSac) == false && claveSac == "rechazo_sacudir_primero",
                "Uno equivocado no vuelve a la sintetasa sin sacudirlo");
            c.Sacudir(sac);
            Revisar(!sac.Cargado && m.arreglos == 1 && m.puntos == 140, $"Sacudir el equivocado: +30 (140) ({m.puntos})");
            Revisar(c.PuedeEntrarASintetasa(sac, out _), "Ya sacudido, sí entra a la sintetasa");

            Colocar(c, phe, mesa, ResultadoColocacion.Llave, null, "Phe entra");
            c.Aceptar(phe, mesa);
            Revisar(m.puntos == 250 && m.racha == 1, $"Phe correcta: +110 (250) ({m.puntos})");
            Revisar(c.EnParo(mesa) && c.PideAhora(mesa) == null, "El ribosoma llegó a UAG y ya no pide ARNt");
            Colocar(c, phe, mesa, ResultadoColocacion.Rechazado, "rechazo_fin", "En el paro se pide tocar FIN");

            fases.Clear();
            c.TocarFin(mesa);
            Revisar(c.Fase == FaseCocina.Plegando && mesa.listo && m.cadenasTerminadas == 1, "FIN: cadena lista y fase Plegando");
            float t = m.tiempo;
            c.Paso(2.5f);
            Revisar(mensajes.LastOrDefault() == "final_practica", "A los 2.4 s, mensaje final del turno de práctica");
            c.Paso(3.1f);
            Revisar(c.Fase == FaseCocina.Fin, "A los 3 s más, fase Fin");
            Revisar(m.tiempo > t && c.Estrellas == 3, $"El reloj corrió y hay 3 estrellas ({m.Reloj})");

            c.IniciarTurno(0);
            c.PonerPausa(true);
            float antes = c.Marcador.tiempo;
            Revisar(c.Pausado, "La pausa del tutorial se activa");
            c.PonerPausa(false);
            Revisar(!c.Pausado && c.Marcador.tiempo == antes, "Y se quita");
        }

        // ---------------- C. Turno 2 ----------------
        static void ProbarTurno2(ControlCocina c, CodigoGenetico codigo, List<GameObject> basura)
        {
            Seccion("C. Turno 2");
            c.IniciarTurno(1);
            var mesa = c.Ribosomas[0];
            var lejano = c.Ribosomas[1];
            for (int i = 0; i < 60; i++) c.Paso(0.1f);
            Revisar(mesa.indice == 14, $"La mesa avanza sola hasta su primer codón manual, el 14 ({mesa.indice})");
            Revisar(lejano.indice == 19, $"El lejano avanza solo hasta el 19 ({lejano.indice})");
            Revisar(mesa.cadena.Count == 14 && mesa.cadena[0].letra == 'M', "La cadena trae la Met y los 13 automáticos");

            var nec = c.Necesarios().Select(x => x.tripleta).ToList();
            var esperado = new[] { c.Codones[14].tripleta, c.Codones[15].tripleta, c.Codones[19].tripleta, c.Codones[20].tripleta };
            Revisar(nec.SequenceEqual(esperado), $"Necesarios: 2 codones por ribosoma ({string.Join(" ", nec)})");

            string pide = c.Codones[14].tripleta;
            var senuelo = c.Senuelo();
            Revisar(Codon.Diferencias(senuelo, pide) >= 2, $"El señuelo {senuelo} difiere en 2+ bases de {pide}");

            Revisar(c.ActivarHorquilla(), "Aparece la horquilla en el ARNm de la mesa");
            var t = ARNt(pide, codigo, basura);
            c.TerminarCarga(t, forzarError: false);
            Colocar(c, t, mesa, ResultadoColocacion.Rechazado, "rechazo_horquilla", "Con horquilla la mesa no acepta");
            float puntos = c.Marcador.puntos;
            c.GolpearHorquilla(); c.GolpearHorquilla();
            Revisar(c.HorquillaActiva, "Con 2 golpes sigue cerrada");
            c.GolpearHorquilla();
            Revisar(!c.HorquillaActiva && c.Marcador.golpes == 1 && c.Marcador.puntos == puntos + 40, "Al tercer golpe se abre: +40");

            Revisar(c.ActivarCodonRaro() && c.MaximoEnCorriente == 9, "Codón raro: caben 9 ARNt");
            c.Paso(12.1f);
            Revisar(!c.CodonRaroActivo && c.MaximoEnCorriente == 5, "A los 12 s termina: vuelven a ser 5");

            var lej = ARNt(c.Codones[19].tripleta, codigo, basura);
            c.TerminarCarga(lej, forzarError: false);
            Colocar(c, lej, lejano, ResultadoColocacion.Aceptado, null, "Al lejano entra directo, sin girar");
            Revisar(lejano.indice == 20, "El lejano avanzó al 20");
        }

        // ---------------- D. Turno 3 completo ----------------
        static void ProbarTurno3(ControlCocina c, CodigoGenetico codigo, List<GameObject> basura)
        {
            Seccion("D. Turno 3");
            c.IniciarTurno(2);
            int vueltas = 0;
            while (!c.Ribosomas.All(r => c.EnParo(r)) && vueltas++ < 2000)
            {
                foreach (var r in c.Ribosomas)
                {
                    var pide = c.PideAhora(r);
                    if (pide == null) continue;
                    var t = ARNt(pide.Value.tripleta, codigo, basura);
                    c.TerminarCarga(t, forzarError: false);
                    var res = c.IntentarColocarEnRibosoma(t, r, out _);
                    if (res == ResultadoColocacion.Llave) c.Aceptar(t, r);
                }
                c.Paso(0.2f);
                foreach (var usado in c.Salida.ToList()) { c.Agarrar(usado); c.Reciclar(usado); }
            }
            Revisar(c.Ribosomas.All(r => c.EnParo(r)), $"Los dos ribosomas llegan al paro ({vueltas} vueltas)");
            Revisar(c.Ribosomas.All(r => r.cadena.Count == 38 && r.cadena[20].letra == 'G'),
                "Las dos cadenas tienen Met + 37 y Gly en la posición 20");

            c.TocarFin(c.Ribosomas[0]);
            Revisar(c.Fase == FaseCocina.Armando && mensajes.LastOrDefault() == "cadena_lista_sigue", "Con una cadena lista se sigue con la otra");
            c.TocarFin(c.Ribosomas[1]);
            c.Paso(2.5f);
            Revisar(c.Fase == FaseCocina.Busca, "Con mutación, después del plegado viene la búsqueda");

            float p = c.Marcador.puntos;
            c.TocarCuenta(5);
            Revisar(c.Fase == FaseCocina.Busca && !c.PistaCuenta, "Una cuenta equivocada: sigue buscando");
            c.TocarCuenta(7);
            Revisar(c.PistaCuenta, "A los 2 errores parpadea la cuenta correcta");
            c.TocarCuenta(20);
            Revisar(c.Fase == FaseCocina.Decide && c.Marcador.puntos == p + 100, "La cuenta 20: +100 y fase Decide");

            Revisar(c.UsarChaperona() && !c.UsarChaperona(), "La chaperona se usa una sola vez");
            p = c.Marcador.puntos;
            Revisar(c.UsarProteasoma() && c.Fase == FaseCocina.Final, "Proteasoma: fase Final");
            Revisar(c.Marcador.puntos == p + 150, "Después de la chaperona, el proteasoma da +150");
            c.Paso(3f);
            Revisar(c.Fase == FaseCocina.Fin, "A los 2.8 s, fase Fin");
            Revisar(fases.Contains(FaseCocina.Busca) && fases.Contains(FaseCocina.Decide), "Pasó por Busca y Decide");
            Debug.Log($"[Cocina] Turno 3: {c.Marcador.puntos} puntos, {c.Marcador.aciertos} aminoácidos puestos, {c.Marcador.reciclados} reciclados, {c.Estrellas} estrellas ({c.Marcador.Reloj}).");
        }

        // ---------------- ayudantes ----------------
        static ARNtCocina ARNt(string codon, CodigoGenetico codigo, List<GameObject> basura)
        {
            var go = new GameObject($"ARNt {codon}") { hideFlags = HideFlags.HideAndDontSave };
            basura.Add(go);
            var t = go.AddComponent<ARNtCocina>();
            t.Configurar(codon, codigo.Traducir(codon));
            return t;
        }

        static void Colocar(ControlCocina c, ARNtCocina t, RibosomaEstado r, ResultadoColocacion esperado, string claveEsperada, string que)
        {
            var res = c.IntentarColocarEnRibosoma(t, r, out var clave);
            Revisar(res == esperado && clave == claveEsperada, $"{que} → {res}{(clave != null ? $" ({clave})" : "")}");
        }

        static void Revisar(bool ok, string que)
        {
            revisadas++;
            if (ok) Debug.Log($"[Cocina]   OK     {que}");
            else { fallas++; Debug.LogError($"[Cocina]   FALLA  {que}"); }
        }

        static void Seccion(string nombre) => Debug.Log($"[Cocina] ---- {nombre} ----");
        static void AnotarMensaje(string clave) => mensajes.Add(clave);
        static void AnotarFase(FaseCocina f) => fases.Add(f);

        // Para correrla sin abrir el editor:
        // Unity.exe -batchmode -projectPath ... -executeMethod BioVR.Minijuegos.Cocina.Herramientas.PruebasNucleo.ProbarYSalir
        public static void ProbarYSalir()
        {
            Probar();
            EditorApplication.Exit(fallas == 0 ? 0 : 1);
        }
    }
}
