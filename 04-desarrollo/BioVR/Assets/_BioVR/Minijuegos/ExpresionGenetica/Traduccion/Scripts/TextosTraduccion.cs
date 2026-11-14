using UnityEngine;

namespace BioVR.Minijuegos.ExpresionGenetica
{
    // Textos de la cabina para la secuencia de traducción. Están en un asset para que Genaro (G4) y Marcos (M4)
    // los cambien sin tocar código y sin que se pierdan al volver a armar la escena.
    // Ciclo guiado: guion M4 revisado por Moisés (Mo9, 27 oct). Ciclo del paciente y los marcados [PROV]: provisionales de Iker.
    // Regla de M4: ningún texto pasa de 2 líneas (~100 caracteres). {0}, {1}: se sustituyen en el juego.
    [CreateAssetMenu(fileName = "TextosTraduccion", menuName = "BioVR/Expresión Genética/Textos de traducción")]
    public class TextosTraduccion : ScriptableObject
    {
        [Header("Ciclo guiado (M4)")]
        [TextArea(1, 3)] public string paso1Llegar = "Este es un ribosoma. Lee la copia de la receta (ARNm) de tres en tres letras.";
        [TextArea(1, 3)] public string paso1Ribosoma = "Ahora señala la cinta de ARNm."; // [PROV]
        [TextArea(1, 3)] public string paso1Acertar = "Muy bien. La receta se lee en orden, de un extremo al otro.";
        [TextArea(1, 3)] public string paso2Llegar = "Tres letras forman un codón. Este es AUG: la señal de inicio.";
        [TextArea(1, 3)] public string paso2Acertar = "Casi cada codón pide un aminoácido; algunos son señales de paro.";
        [TextArea(1, 3)] public string paso3Llegar = "Cada ARNt trae un aminoácido. Su anticodón encaja con el codón que le toca.";
        [TextArea(1, 3)] public string paso3Inspeccionar = "Una enzima ya le puso su aminoácido. Tú no lo unes.";
        [TextArea(1, 3)] public string paso3Terminar1 = "En la célula los ARNt llegan al azar; aquí tú los acercas.";
        [TextArea(1, 3)] public string paso3Terminar2 = "Ahora busca el que encaja con AUG.";
        [TextArea(1, 3)] public string paso4Llegar = "Acerca el ARNt que encaja con AUG.";
        [TextArea(1, 3)] public string paso4Acertar = "¡Encaja! Es el ARNt de Met, el iniciador.";
        [TextArea(1, 3)] public string paso5Llegar = "Ahora tú: busca el ARNt que encaja con GGC.";
        [TextArea(1, 3)] public string paso5Acertar = "El ribosoma une Gly a Met con un enlace peptídico.";
        [TextArea(1, 3)] public string paso6Automatico = "El ribosoma sigue solo: UUU pide Phe.";
        [TextArea(1, 3)] public string paso6Paro = "UAA es una señal de paro. Ningún ARNt encaja aquí.";
        [TextArea(1, 3)] public string liberacion = "Entra un factor de liberación y la cadena se suelta.";
        [TextArea(1, 3)] public string cierreGuiado = "Así se lee una receta y se arma una cadena de aminoácidos.";

        [Header("Errores y avisos")]
        [TextArea(1, 3)] public string noEncaja = "No encaja. Mira su anticodón y prueba otro.";
        [TextArea(1, 3)] public string esperaInspeccion = "Primero inspecciona los 4 ARNt."; // [PROV]
        [TextArea(1, 3)] public string paroSinARNt = "Ningún ARNt encaja en este codón. ¿Qué tipo de codón es?"; // [PROV]
        [TextArea(1, 3)] public string continuar = "Toca Continuar para leer un ARNm que trae un error."; // [PROV]

        // Sin diagnóstico clínico (plan de la directora, 9 nov). "diagnostico" conserva el nombre para no romper el asset.
        [Header("Ciclo con error (Mo6, D2) · [PROV]")]
        [TextArea(1, 3)] public string alertaPaciente = "Alerta: esta célula arma una proteína que sale mal. Traduce su ARNm y mira qué pasa.";
        [TextArea(1, 3)] public string pacienteBuscar = "Busca el ARNt que encaja con {0}.";
        [TextArea(1, 3)] public string pacienteAcierto = "Encaja: se suma {0} a la cadena.";
        [TextArea(1, 3)] public string pacienteParo = "El ribosoma llegó a {0}. Si ningún ARNt encaja, señálalo como paro.";
        [TextArea(1, 3)] public string cierrePaciente = "Esta cadena tiene {0} aminoácidos; la normal tenía {1}. El paro llegó antes.";
        [TextArea(1, 3)] public string diagnostico = "Ahí está la falla: un cambio en el ARNm cambió la cadena de aminoácidos.";
    }
}
