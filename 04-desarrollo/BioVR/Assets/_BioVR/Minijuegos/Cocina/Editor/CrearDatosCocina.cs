using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BioVR.Minijuegos.Cocina.Herramientas
{
    // Crea los datos de la Cocina en Cocina/Datos/ (menú BioVR → Cocina → Crear datos).
    //
    // Se puede correr las veces que sea: lo que YA EXISTE NO SE SOBRESCRIBE, para no borrar lo que
    // Investigación, Diseño o un compañero hayan cambiado a mano. Solo se agregan los assets y los textos que falten.
    // Para regenerar un asset desde cero, bórralo y vuelve a correr el menú.
    //
    // De dónde salen los datos:
    //   - Colores: paleta de Diego (03-diseno/documentacion/proteinas-p9/LEEME.md)
    //   - Codones de la amilina: NM_000415.3, residuos 34–70 del precursor (igual que el HTML de Uli, PR #65)
    //   - Turnos, tiempos, puntos y textos: HTML de Uli, con las decisiones del equipo (paro UAG, sin diabetes, controles VR)
    public static class CrearDatosCocina
    {
        public const string Raiz = "Assets/_BioVR/Minijuegos/Cocina";
        public const string Datos = Raiz + "/Datos";
        const string CarpetaAA = Datos + "/Aminoacidos";

        public const string RutaConfig = Datos + "/ConfigCocina.asset";
        public const string RutaCodigo = Datos + "/CodigoGenetico.asset";
        public const string RutaTextos = Datos + "/TextosCocina.asset";
        public static readonly string[] RutasTurnos = { Datos + "/Turno1.asset", Datos + "/Turno2.asset", Datos + "/Turno3.asset" };

        // Amilina madura (37 aminoácidos) y sus codones reales en NM_000415.3
        public const string Amilina = "KCNTATCATQRLANFLVHSSNNFGAILSSTNVGSNTY";
        public const string CodonesAmilina =
            "AAA UGC AAC ACU GCC ACA UGU GCA ACG CAG CGC CUG GCA AAU UUU UUA GUU CAU UCC AGC " +
            "AAC AAC UUU GGU GCC AUU CUC UCA UCU ACC AAC GUG GGA UCC AAU ACA UAU";
        public const int MutacionS20G = 19;      // índice en la secuencia (base 0) = codón 20
        public const string CodonS20G = "GGC";   // AGC → GGC (c.157A>G)

        // (letra, abreviatura, nombre, color de Diego, codón preferido, todos los codones)
        static readonly (char, string, string, string, string, string)[] Tabla =
        {
            ('A', "Ala", "alanina",          "#E8C39E", "GCC", "GCU GCC GCA GCG"),
            ('R', "Arg", "arginina",         "#00E5FF", "CGC", "CGU CGC CGA CGG AGA AGG"),
            ('N', "Asn", "asparagina",       "#B6FF3B", "AAC", "AAU AAC"),
            ('D', "Asp", "ácido aspártico",  "#B3001B", "GAC", "GAU GAC"),
            ('C', "Cys", "cisteína",         "#FFF200", "UGC", "UGU UGC"),
            ('Q', "Gln", "glutamina",        "#A8F0FF", "CAG", "CAA CAG"),
            ('E', "Glu", "ácido glutámico",  "#7A0F3D", "GAG", "GAA GAG"),
            ('G', "Gly", "glicina",          "#FFFFFF", "GGC", "GGU GGC GGA GGG"),
            ('H', "His", "histidina",        "#9B5CFF", "CAC", "CAU CAC"),
            ('I', "Ile", "isoleucina",       "#FF2E2E", "AUC", "AUU AUC AUA"),
            ('L', "Leu", "leucina",          "#FF7A1A", "CUG", "UUA UUG CUU CUC CUA CUG"),
            ('K', "Lys", "lisina",           "#2F6BFF", "AAG", "AAA AAG"),
            ('M', "Met", "metionina",        "#8C95A6", "AUG", "AUG"),
            ('F', "Phe", "fenilalanina",     "#FF3EA5", "UUU", "UUU UUC"),
            ('P', "Pro", "prolina",          "#5C6273", "CCC", "CCU CCC CCA CCG"),
            ('S', "Ser", "serina",           "#3DFF7A", "AGC", "UCU UCC UCA UCG AGU AGC"),
            ('T', "Thr", "treonina",         "#00C49A", "ACC", "ACU ACC ACA ACG"),
            ('W', "Trp", "triptófano",       "#6A0DAD", "UGG", "UGG"),
            ('Y', "Tyr", "tirosina",         "#D64DFF", "UAC", "UAU UAC"),
            ('V', "Val", "valina",           "#FFB000", "GUG", "GUU GUC GUA GUG"),
        };

        // Errores de la sintetasa entre aminoácidos parecidos (HTML: PAREC)
        static readonly string[] Parecidos = { "IV", "VI", "ST", "TS", "NQ", "QN", "KR", "RK", "LI", "FY", "YF", "AG", "GA", "CS", "HN", "ML" };

        // Mensajes que lanza ControlCocina. M8 agrega los del tutorial y la pantalla final.
        // Reglas: controles de VR (pinza, muñeca, aventar), nunca la diabetes, "depósitos amiloides".
        static readonly (string, string)[] Mensajes =
        {
            ("intro_turno1", "Pedido de práctica: una cadenita de 3 aminoácidos. Atrapa con la pinza un ARNt de la corriente que encaje con el codón del ribosoma."),
            ("intro_turno2", "La célula pide 2 amilinas. Dos ribosomas leen el mismo mensaje a la vez (un polirribosoma). Al lejano tienes que aventarle los ARNt."),
            ("intro_turno3", "Otro pedido de 2 amilinas. Atento: algo raro trae este mensaje."),

            ("rechazo_ya_cargado", "Ese ya tiene su aminoácido: llévalo al ribosoma que lo pide."),
            ("rechazo_sacudir_primero", "Primero sacúdelo para quitarle el aminoácido equivocado."),
            ("rechazo_horquilla", "La horquilla del ARNm no deja avanzar al ribosoma: golpéala 3 veces con la pinza para desenrollarla."),
            ("rechazo_fin", "Ese ribosoma ya llegó al final: toca su botón FIN."),
            ("rechazo_no_pide", "Ese ribosoma no está pidiendo nada ahora."),
            ("rechazo_llave_ocupada", "Primero termina de girar el que ya está en el ribosoma."),
            ("rechazo_sin_carga", "Le falta su aminoácido: primero cárgalo en la sintetasa."),
            ("rechazo_no_encaja", "¡No encaja! Compara las piezas de abajo del ARNt con el codón."),
            ("rechazo_salida_llena", "¡La salida está llena! El ribosoma no puede soltar más usados: avienta los usados al regreso al citoplasma."),

            ("llave_girar", "¡Entró! Ahora gíralo como llave: gira la muñeca hasta que haga clic."),
            ("acierto_mesa", "¡Clic! El aminoácido se sumó a la cadena."),
            ("acierto_lejano", "¡Le atinaste al ribosoma lejano!"),
            ("malo_entro", "¡Ojo! El ribosoma solo revisa que las piezas encajen, no qué aminoácido trae. Entró uno equivocado y la proteína lleva un error (la cuenta roja). Por eso hay que corregir en la sintetasa."),
            ("sintetasa_error", "¡Uy! La sintetasa le puso un aminoácido equivocado (tiene \"!\"). Sácalo y sacúdelo."),
            ("sacudido_equivocado", "¡Bien! Le quitaste el aminoácido equivocado. Ahora vuelve a cargarlo en la sintetasa."),
            ("sacudido_bueno", "Le quitaste su aminoácido. Vuelve a cargarlo en la sintetasa."),
            ("reciclado", "¡De regreso! El ARNt vuelve al citoplasma para recargarse y usarse otra vez."),

            ("cadena_lista_sigue", "¡Una amilina lista! El holograma la muestra doblándose. Sigue con el otro ribosoma."),
            ("plegando", "Se soltó la última cadena. Mira el holograma: se dobla sola."),
            ("final_practica", "¡Tu primera cadena! Las proteínas de verdad son mucho más largas y se doblan solas."),
            ("final_pedido", "¡Pedido listo! Las dos amilinas salieron con la secuencia correcta."),

            ("busca", "Se dobló distinto: esta versión se pega más fácil a otras amilinas. Una letra venía cambiada desde el ADN del gen, y el ARNm solo la copió. Toca en el holograma la cuenta que es diferente."),
            ("busca_error", "Esa no. Busca la cuenta que es diferente."),
            ("busca_pista", "Fíjate en la cuenta que parpadea."),
            ("cuenta_encontrada", "¡Esa! Ahí iba Ser (serina) y quedó Gly (glicina), porque el mensaje decía GGC en lugar de AGC. En laboratorio, la amilina S20G forma fibras amiloides más rápido que la normal. ¿Chaperona o proteasoma?"),
            ("chaperona", "La chaperona (BiP) abre, sujeta y suelta la cadena para darle otra oportunidad de doblarse, pero no siempre lo logra: el cambio viene del mensaje. Mándala al proteasoma."),
            ("proteasoma", "El proteasoma degradó la cadena marcada con ubiquitina y recicló sus aminoácidos."),

            ("horquilla", "¡El ARNm se dobló y formó una horquilla! El ribosoma no puede avanzar: golpéala 3 veces con la pinza para desenrollarla."),
            ("horquilla_abierta", "¡Desenrollada! El ribosoma puede seguir leyendo."),
            ("raro", "¡Codón raro! El ARNt que encaja con él casi no hay en la célula: búscalo bien en la corriente."),
        };

        [MenuItem("BioVR/Cocina/Crear datos")]
        public static void Crear()
        {
            Carpeta(Raiz);
            Carpeta(Datos);
            Carpeta(CarpetaAA);

            var aminoacidos = new Dictionary<char, AminoacidoData>();
            foreach (var (letra, abrev, nombre, color, _, _) in Tabla)
            {
                var aa = Cargar<AminoacidoData>($"{CarpetaAA}/AA_{letra}_{abrev}.asset", out bool nuevo);
                if (nuevo)
                {
                    aa.letra = letra;
                    aa.abreviatura = abrev;
                    aa.nombre = nombre;
                    ColorUtility.TryParseHtmlString(color, out aa.color);
                    EditorUtility.SetDirty(aa);
                }
                aminoacidos[letra] = aa;
            }

            var codigo = Cargar<CodigoGenetico>(RutaCodigo, out bool codigoNuevo);
            if (codigoNuevo)
            {
                foreach (var (letra, _, _, _, preferido, todos) in Tabla)
                    codigo.aminoacidos.Add(new CodigoGenetico.Entrada
                    {
                        aminoacido = aminoacidos[letra],
                        preferido = preferido,
                        codones = todos.Split(' ')
                    });
                foreach (var p in Parecidos)
                    codigo.parecidos.Add(new CodigoGenetico.Parecido { de = aminoacidos[p[0]], a = aminoacidos[p[1]] });
                EditorUtility.SetDirty(codigo);
            }

            Cargar<ConfigCocina>(RutaConfig, out _); // los valores iniciales ya vienen en la clase

            var textos = Cargar<TextosCocina>(RutaTextos, out _);
            int agregados = 0;
            foreach (var (clave, texto) in Mensajes)
                if (textos.AgregarSiFalta(clave, texto)) agregados++;
            if (agregados > 0) EditorUtility.SetDirty(textos);

            CrearTurnos();

            AssetDatabase.SaveAssets();
            Debug.Log($"[Cocina] Datos listos en {Datos} ({agregados} textos nuevos). Lo que ya existía no se tocó.");
        }

        static void CrearTurnos()
        {
            var amilina = CodonesAmilina.Split(' ');

            var t1 = Cargar<TurnoData>(RutasTurnos[0], out bool nuevo1);
            if (nuevo1)
            {
                t1.nombre = "Turno 1 · Aprende la cocina";
                t1.claveIntro = "intro_turno1";
                t1.secuencia = "GF";
                t1.codones = new[] { "AUG", "GGC", "UUU", "UAG" };
                t1.mutacion = -1;
                t1.ribosomas = new[] { new TurnoData.Ribosoma { lejano = false, todoManual = true } };
                t1.capacidadSalida = 2;
                t1.errorSintetasa = 0f;
                t1.imprevistos = false;
                t1.mesaGira = false;
                t1.estrellas = new Vector2(75, 140);
                t1.tutoriales = new[] { "bienvenida", "ribosoma", "piezas", "corriente" };
                EditorUtility.SetDirty(t1);
            }

            var t2 = Cargar<TurnoData>(RutasTurnos[1], out bool nuevo2);
            if (nuevo2)
            {
                t2.nombre = "Turno 2 · Pedido de amilina";
                t2.claveIntro = "intro_turno2";
                t2.secuencia = Amilina;
                t2.codones = CodonesTurno(amilina, -1);
                t2.mutacion = -1;
                t2.ribosomas = new[]
                {
                    new TurnoData.Ribosoma { lejano = false, manualDesde = 14, manualHasta = 18 },
                    new TurnoData.Ribosoma { lejano = true,  manualDesde = 19, manualHasta = 23 },
                };
                t2.capacidadSalida = 4;
                t2.errorSintetasa = 0.3f;
                t2.imprevistos = true;
                t2.mesaGira = false;
                t2.estrellas = new Vector2(170, 260);
                t2.tutoriales = new[] { "pedido", "poli" };
                EditorUtility.SetDirty(t2);
            }

            var t3 = Cargar<TurnoData>(RutasTurnos[2], out bool nuevo3);
            if (nuevo3)
            {
                t3.nombre = "Turno 3 · Mensaje con una letra cambiada";
                t3.claveIntro = "intro_turno3";
                t3.secuencia = Amilina;
                t3.codones = CodonesTurno(amilina, MutacionS20G);
                t3.mutacion = MutacionS20G;
                t3.ribosomas = new[]
                {
                    new TurnoData.Ribosoma { lejano = false, manualDesde = 16, manualHasta = 20 },
                    new TurnoData.Ribosoma { lejano = true,  manualDesde = 20, manualHasta = 24 },
                };
                t3.capacidadSalida = 4;
                t3.errorSintetasa = 0.3f;
                t3.imprevistos = true;
                t3.mesaGira = true;
                t3.estrellas = new Vector2(180, 280);
                t3.tutoriales = new[] { "mesa" };
                EditorUtility.SetDirty(t3);
            }
        }

        // AUG + los codones de la amilina (con GGC en la mutación, si hay) + UAG
        static string[] CodonesTurno(string[] amilina, int mutacion)
        {
            var lista = new List<string> { "AUG" };
            for (int i = 0; i < amilina.Length; i++) lista.Add(i == mutacion ? CodonS20G : amilina[i]);
            lista.Add("UAG");
            return lista.ToArray();
        }

        static T Cargar<T>(string ruta, out bool nuevo) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(ruta);
            nuevo = a == null;
            if (nuevo)
            {
                a = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(a, ruta);
            }
            return a;
        }

        public static void Carpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            var padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
            Carpeta(padre);
            AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
        }
    }
}
