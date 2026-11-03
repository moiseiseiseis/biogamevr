# Mo6 · Loop de la misión "Una proteína ausente"

Minijuego Expresión Genética · Desarrollo (Moisés) · Corte C2, 24 de octubre de 2026 · **Borrador para revisión**

> Se basa en la propuesta de la Minuta 4 (§ historia de ejemplo) y en el reporte de mutaciones de Genaro (G1). Hasta que Investigación apruebe el caso, la β-globina en eritroblasto es el **supuesto de trabajo**. En esta entrega solo se juega la **traducción**; lo demás del loop se documenta, pero no se programa.

## El loop en cinco pasos

| Paso | Qué pasa | Qué hace el jugador | En esta entrega |
| --- | --- | --- | --- |
| **1. Alerta** | La cabina avisa que la sangre del paciente transporta poco oxígeno: sus glóbulos rojos son pequeños y pálidos. Falta **β-globina**, una de las dos piezas de la hemoglobina. | Escucha o lee la alerta y acepta la misión. | Texto en el letrero de la escena |
| **2. Evidencia** | El mecha entra a un **eritroblasto** en la médula ósea (no a un glóbulo rojo maduro, que ya no tiene núcleo ni ribosomas). Ahí se pueden revisar tres cosas: cuánto ARN se fabrica, qué tan largo sale el ARNm y dónde se detiene el ribosoma. | Recoge evidencia en cada etapa. | Solo la evidencia del **ribosoma** (jugable) |
| **3. Descarte de etapas** | Cada evidencia descarta una o más etapas. Ver la tabla de abajo. | Compara lo sano con lo del paciente y descarta etapas. | En papel |
| **4. Diagnóstico** | El jugador dice en qué etapa está la falla. | Elige la etapa afectada en la cabina. | Fijo: **traducción** (paro prematuro) |
| **5. Cierre** | La cabina explica la causa en una frase (capa 1) y la relaciona con el paciente: sin β-globina sobran cadenas α, que dañan al eritroblasto, y hay anemia. | Lee el cierre. | Texto en el letrero |

## Qué evidencia descarta qué

Esto es lo que el jugador observa en cada caso. La columna "Paciente" de esta entrega es la de la **mutación 3**.

| Evidencia | Sano | Mutación 1: promotor (−28 A→G) | Mutación 2: splicing (IVS1-110 G→A) | Mutación 3: paro prematuro (codón 39 CAG→UAG) |
| --- | --- | --- | --- | --- |
| ¿Cuánto ARN se fabrica en el núcleo? | Normal | **Muy poco** | Normal | Normal |
| ¿El ARNm maduro tiene el largo correcto? | Sí | Sí (pero hay poco) | **No: sobra un trozo de intrón** | Sí |
| ¿Cuánto ARNm llega al citoplasma? | Normal | Poco | Sale, pero el ~90% está mal cortado; solo ~10% sirve | **Casi nada** (NMD; ver nota) |
| ¿Dónde se detiene el ribosoma? | En el paro normal | En el paro normal | Antes: el trozo extra cambia la lectura | **Muy pronto: en el codón 39** |
| **Etapa afectada** | — | Transcripción | Maduración del ARN | Traducción |

> **Nota de rigor (confirmada en PubMed el 25 oct; fuentes en `g1-reporte-mutaciones.md`):**
> - **Caso 3:** el paro tan temprano activa la vía NMD. En portadores casi no hay ARNm mutante, así que el jugador **no** debe ver "ARNm en cantidad normal". Lo que distingue al caso 3 es: **largo correcto, muy poco ARNm y paro prematuro en el ribosoma** (decisión D5).
> - **Caso 2:** el ARNm mal cortado sí sale al citoplasma (Busslinger et al., 1981). Lo que lo distingue es que **es más largo** de lo normal.

## Cómo se ve en la escena jugable (traducción)

1. **Ciclo de muestra:** el ribosoma traduce solo un ARNm **sano**: AUG (Met) → GGC (Gly) → UUU (Phe) → UAA. En el UAA **entra solo** el factor de liberación y se suelta la cadena de 3 aminoácidos.
2. **Ciclo guiado:** el jugador atrae, rota y coloca el ARNt cuyo anticodón corresponde a cada codón. Con el ARNt correcto aparece el aminoácido en la cadena. Las pistas están activadas.
3. **Ciclo del paciente, sin ayuda (P0, decisión D2):** la secuencia didáctica del paciente es **AUG–GGC–UAG**. Después de dos codones aparece UAG antes de lo esperado. Ningún ARNt encaja, tampoco el distractor de Gln (decisión D3). El jugador lo **identifica como paro** y el factor de liberación **entra solo** (decisión D1). La cadena sale más corta que en el ciclo sano.

**Los 4 ARNt de la escena (decisión D3):**

| ARNt | Anticodón (3′ → 5′) | Codón que lee | Uso |
| --- | --- | --- | --- |
| Met (iniciador) | UAC | AUG | Inicio |
| Gly | CCG | GGC | Elongación |
| Phe | AAA | UUU | Elongación (solo en el ciclo sano) |
| **Gln** | **GUC** | CAG | **Distractor:** leería el codón 39 sano; en el paciente (UAG) no encaja |
4. El jugador compara las dos cadenas y diagnostica: **la falla está en la traducción, por un paro prematuro**.

> **Escala didáctica:** la β-globina real tiene 146 aminoácidos y su ARNm no empieza con AUG–GGC–UUU. La secuencia de 4 codones y el paro "en el tercer codón" en vez del 39 son simplificaciones, y la escena debe decirlo con el aviso de escala didáctica (A1).

## Decisiones que necesita este loop

- **Q1–Q3 de la Minuta 4:** quién habla en la cabina y con qué tono, para escribir la alerta y el cierre (G4 y M4).
- ~~¿Qué hace el jugador en el codón de paro?~~ **Decidido (D1, 25 oct):** el jugador señala el codón como paro y el factor de liberación entra solo.
- **¿Dónde se elige el diagnóstico?** Propongo tres botones en la cabina (Transcripción, Maduración y Traducción), con el mismo botón de la cabina actual.
