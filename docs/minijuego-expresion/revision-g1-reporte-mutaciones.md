# Revisión del reporte de mutaciones (G1)

Minijuego Expresión Genética · Revisión de rigor (Moisés) · 24 de octubre de 2026

> **Estado (25 oct): aplicada.** Las correcciones están en el G1 final, `g1-reporte-mutaciones.md`, que hizo Desarrollo con crédito a Genaro (decisión D4). Los puntos 1 y 3 quedaron confirmados en PubMed. El punto 2 (19 nt) sigue pendiente de confirmar en el texto completo. El punto 4 se decidió en D1.

Reporte revisado: *Reporte de Contexto Patológico y Viabilidad de Mutaciones: β-Talasemia en el Minijuego VR* (Genaro, 23 de octubre).

**En general está muy bien:** el caso está bien elegido, la justificación del eritroblasto es correcta y las tres mutaciones cubren las tres etapas, como pedía G1. Antes de usarlo en las fichas (G2) y en la tabla de Iker (I2) hay que corregir o confirmar estos puntos.

## Para corregir

| # | Dónde | Qué dice | Lo que creo que es correcto | Por qué importa |
| --- | --- | --- | --- | --- |
| 1 | Mutación 3, "Lógica del minijuego" | La transcripción y el splicing salen bien, así que el ARN "existe en cantidad normal" | Con un paro prematuro en el codón 39, el ARNm **se acumula menos** (vía NMD). Tu propia referencia 3 (Baserga y Benz, 1992) lo dice en el título: *deficient accumulation of mRNA* | Si el jugador ve "ARN normal" en el caso 3, la evidencia es falsa. Lo que distingue al caso 3 es que **el largo del ARNm es correcto y el ribosoma se detiene pronto** |
| 2 | Mutación 2 | Se quedan pegados "109 nucleótidos basura" | Hasta donde sé (hay que confirmarlo en IthaGenes), IVS1-110 G→A crea un sitio receptor **19 nucleótidos antes** del sitio normal, y esos **19 nt** del intrón se quedan en el ARNm | Es un dato que se puede verificar y que va a quedar en las fichas |
| 3 | Mutación 2 | La mutación está "exactamente en el nucleótido 110 antes de que termine dicho intrón" | Es la posición **110 del intrón 1**, contando desde su inicio (el intrón 1 tiene 130 nt; queda a unos 20 nt del final) | Misma razón |
| 4 | Mutación 3, "Visualización" | El jugador **toma e inserta** el factor de liberación | El plan dice que el jugador **identifica** el UAA/UAG y que el factor de liberación **entra solo**. Si el jugador lo coloca, está haciendo el trabajo de una proteína | Regla rectora: el jugador diagnostica, no sustituye. Se decide en la tabla de Iker (I2) |
| 5 | Matriz, fila codón–anticodón | Agarrar el ARNt "con el *trigger*" | En el proyecto se agarra con el **grip** (el gatillo es para botones y menús) | Que el guion del tutorial de Marcos (M3) use el mismo botón |

## Para confirmar

- **Fuentes por mutación:** G1 pide una fuente verificable **por cada mutación**. Las referencias 1 y 2 son revisiones generales. Lo ideal sería una fuente específica para −28 A→G y otra para IVS1-110 (por ejemplo, su ficha en IthaGenes), y que cada mutación diga cuál es la suya.
- **β⁺ o β⁰:** conviene anotar el tipo de cada una: −28 → β⁺; IVS1-110 → β⁺ (todavía sale algo de ARNm bien cortado); codón 39 → β⁰.
- **Secuencia didáctica:** en el juego, el paro del paciente va a aparecer en el tercer codón (AUG–GGC–UAG) y no en el 39. Propongo que tu ficha lo diga como simplificación, para que nadie lo tome como la secuencia real de la β-globina.

## Siguiente paso

Si estás de acuerdo con los puntos 1 a 3, corrígelos en el reporte y súbelo a `docs/minijuego-expresion/` como G1 final. El punto 4 lo vemos con Iker en la tabla (I2).
