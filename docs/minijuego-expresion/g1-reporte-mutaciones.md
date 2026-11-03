# G1 · Contexto patológico y mutaciones de la β-talasemia

Minijuego Expresión Genética · Entregable G1 (final) · 25 de octubre de 2026

**Autor:** Genaro (reporte original del 23 de octubre). **Correcciones de rigor:** Moisés, a partir de la revisión `revision-g1-reporte-mutaciones.md` y de las fuentes de PubMed de la §5.

> Es la base de las fichas en tres capas (G2) y de la tabla concepto → mecánica de Iker (I1 e I2). Las decisiones de juego que se mencionan aquí están en `decisiones.md`.

## 1. Contexto clínico

- Las talasemias están entre los trastornos hereditarios recesivos más comunes del mundo. Producen poca o ninguna hemoglobina (Hb) y anemia crónica de gravedad variable.
- La β-talasemia es frecuente en el Mediterráneo, Medio Oriente, Asia Central, India, el sur de China, el norte de África y Sudamérica. Hay más de 350 mutaciones registradas en la base de datos IthaGenes.
- El único tratamiento curativo es el trasplante alogénico de células madre hematopoyéticas, y es difícil encontrar donantes compatibles.
- **Es un defecto de cantidad, no de forma.** La β-globina que se produce es normal, pero hay poca (β⁺) o nada (β⁰). Es distinto de la anemia falciforme, en la que la proteína cambia de forma.

### La hemoglobina y el eritroblasto

- La hemoglobina del adulto (HbA) tiene cuatro cadenas: dos de α-globina y dos de β-globina, cada una con un grupo hemo. La producción de las dos cadenas tiene que estar equilibrada.
- **El escenario es un eritroblasto de la médula ósea.** El glóbulo rojo maduro pierde el núcleo y los ribosomas al diferenciarse, así que la transcripción y la traducción no pueden ocurrir ahí.

### Qué pasa cuando falta β-globina

1. **Sobran cadenas α**, que quedan libres y son inestables.
2. **Se precipitan** dentro de la célula y forman cuerpos de inclusión.
3. **Eritropoyesis ineficaz:** muchos eritroblastos mueren en la médula. Los glóbulos rojos que llegan a la sangre son pequeños (microcíticos) y pálidos (hipocrómicos), y hay anemia.

## 2. Las tres mutaciones del juego

| | Mutación 1 | Mutación 2 | Mutación 3 |
| --- | --- | --- | --- |
| Nombre | −28 A→G | IVS1-110 G→A | Codón 39 CAG→UAG |
| Dónde | Promotor (caja TATA) | Intrón 1 | Exón 2 |
| Etapa que falla | **Transcripción** | **Maduración (splicing)** | **Traducción** |
| Tipo | β⁺ | β⁺ | β⁰ |
| ¿Se juega en esta entrega? | No (en papel) | No (en papel) | **Sí** |

### Mutación 1: falla en la transcripción (−28 A→G)

- **Qué pasa:** la mutación está en la caja TATA del promotor del gen *HBB*, 28 pares de bases antes del inicio de la transcripción. Cambia una A por una G.
- En la caja TATA se ensambla la maquinaria de transcripción (la proteína TBP y el factor TFIID). Con el cambio se une peor, y la ARN polimerasa II inicia la transcripción con mucha menos frecuencia. Se produce **poco pre-ARNm** (β⁺).
- **Evidencia para el jugador (en papel):** al medir cuánto ARN sale del gen, el contador se queda muy bajo. Si casi no hay ARN, la falla está en la transcripción y se descartan el splicing y la traducción.

### Mutación 2: falla en el splicing (IVS1-110 G→A)

- **Qué pasa:** el gen *HBB* tiene 3 exones y 2 intrones. La mutación está en la **posición 110 del intrón 1, contada desde su inicio**. El intrón 1 mide 130 nt, así que queda cerca del final.
- La mutación crea un **sitio receptor de splicing falso**, parecido al sitio normal del final del intrón. **Cerca del 90%** del pre-ARNm se corta en el sitio falso y un trozo del intrón se queda en el ARNm.
  - La literatura reporta que el trozo retenido mide **19 nt**. *Pendiente: confirmar la cifra en el texto completo de Busslinger et al. (1981) o en la ficha de IthaGenes. Los resúmenes de PubMed no la traen.*
- Ese ARNm **sí sale al citoplasma**, pero el trozo extra cambia el marco de lectura: daría una cadena truncada de unos **35 aminoácidos**. El 10% restante se corta bien y produce β-globina normal; por eso es β⁺.
- **Evidencia para el jugador (en papel):** hay ARN (se descarta la transcripción), pero al comparar su largo con el de un ARNm sano, el del paciente es más largo. La falla está en la maduración.

### Mutación 3: paro prematuro en la traducción (codón 39 CAG→UAG)

- **Qué pasa:** el codón 39 sano es **CAG** (glutamina, Gln). Un cambio de una base lo convierte en **UAG**, un codón de paro.
- **El ARNm casi no se acumula.** Un paro tan temprano activa la vía de degradación **NMD** (*nonsense-mediated decay*): en portadores casi no se encuentra ARNm mutante. Es β⁰.
- El poco ARNm que llega a traducirse avanza bien hasta el codón 38. En el 39, en lugar de un ARNt con glutamina, entra un **factor de liberación**, que suelta una cadena de solo **38 aminoácidos**, inútil.
- **Evidencia para el jugador:** el ARNm tiene el largo correcto (se descarta la maduración), hay muy poco y el ribosoma se detiene mucho antes de lo esperado. La falla está en la traducción.

> **Corrección importante respecto al reporte original:** en el caso 3 el jugador **no** debe ver "ARNm en cantidad normal". Lo que lo distingue es: largo correcto, muy poco ARNm y paro prematuro en el ribosoma.

## 3. Cómo se juega la mutación 3 (escena del ribosoma)

Es la escena jugable en el Quest 2. Usa la **secuencia didáctica** acordada (ver la §4):

- **Ciclo sano:** AUG (Met) → GGC (Gly) → UUU (Phe) → **UAA** (paro).
- **Ciclo del paciente:** AUG (Met) → GGC (Gly) → **UAG** (paro prematuro).

Qué hace el jugador:

1. **Atrae** un ARNt con el rayo y el **grip**, lo **sujeta** y lo **rota** para ver su anticodón.
2. Lo **suelta en el sitio A** del ribosoma. Si su anticodón es complementario al codón, encaja y su aminoácido se une a la cadena. Si no, se rechaza.
3. En el codón de paro, **ningún ARNt encaja**, tampoco el de Gln (anticodón 3′-GUC-5′), que leería el CAG sano. El jugador **identifica** el codón como paro y el **factor de liberación entra solo**. La cadena se suelta: 3 aminoácidos en el ciclo sano y solo 2 en el del paciente.
4. Compara las dos cadenas y diagnostica: **falla en la traducción por un paro prematuro**.

## 4. Tabla de rigor: concepto → lo que se ve → lo que hace el jugador

| Concepto | Lo que se ve en VR | Lo que hace el jugador | Lo que el jugador **no** hace |
| --- | --- | --- | --- |
| Especificidad de tejido | El escenario es el citoplasma de un **eritroblasto** | — | Jugar en un glóbulo rojo maduro o en otra célula |
| Complementariedad codón-anticodón | El codón expuesto en el sitio A y los ARNt cerca | Atraer, sujetar y rotar el ARNt con el **grip** y soltarlo en el sitio A | Unir aminoácidos con las manos |
| Codón de paro (UAA sano, UAG paciente) | Ningún ARNt encaja; entra el factor de liberación | **Identificar** el codón como paro | Buscar un "ARNt de STOP" o **colocar el factor de liberación** (entra solo) |
| Escala didáctica | Aviso en la escena | Leerlo | — |

> **Escala didáctica:** la β-globina real tiene 146 aminoácidos y su ARNm no empieza con AUG–GGC–UUU. La secuencia de 4 codones y el paro en el tercer codón (en vez del 39) son **simplificaciones** para el juego, y la escena lo dice.

## 5. Por qué la β-talasemia

1. **Es real y común:** afecta a cientos de miles de personas. Un cambio de una sola letra cambia la vida de un paciente (anemia, transfusiones).
2. **Enseña que los genes dicen "cuánto", no solo "cómo":** el promotor y el splicing importan tanto como la secuencia de aminoácidos.
3. **Lo que se lleva el jugador:**
   - el ADN pasa por varias etapas antes de convertirse en proteína;
   - se diagnostica juntando evidencia y descartando;
   - en la célula, las piezas se reconocen por su forma y afinidad, no las une nadie a mano.

## 6. Referencias

**Generales (del reporte original):**

1. Jaing, T. H., et al. (2021). Molecular genetics of β-thalassemia: A narrative review. *Medicine*, 100(45), e27522. https://doi.org/10.1097/MD.0000000000027522
2. Zakaria, N. A., et al. (2022). Genetic manipulation strategies for β-thalassemia: A review. *Frontiers in Pediatrics*, 10, 901605. https://doi.org/10.3389/fped.2022.901605

**Mutación 2 (IVS1-110):**

3. Busslinger, M., Moschonas, N., & Flavell, R. A. (1981). β⁺ thalassemia: aberrant splicing results from a single point mutation in an intron. *Cell*, 27(2 Pt 1), 289–298. https://doi.org/10.1016/0092-8674(81)90412-8
4. Spritz, R. A., et al. (1981). Base substitution in an intervening sequence of a β⁺-thalassemic human globin gene. *PNAS*, 78(4), 2455–2459. https://doi.org/10.1073/pnas.78.4.2455

**Mutación 3 (codón 39):**

5. Baserga, S. J., & Benz, E. J. Jr. (1992). Beta-globin nonsense mutation: deficient accumulation of mRNA occurs despite normal cytoplasmic stability. *PNAS*, 89(7), 2935–2939. https://doi.org/10.1073/pnas.89.7.2935
6. Hall, G. W., & Thein, S. (1994). Nonsense codon mutations in the terminal exon of the β-globin gene are not associated with a reduction in β-mRNA accumulation. *Blood*, 83(8), 2031–2037. PMID 8161774.
7. Inácio, A., et al. (2004). Nonsense mutations in close proximity to the initiation codon fail to trigger full nonsense-mediated mRNA decay. *J Biol Chem*, 279(31), 32170–32180. https://doi.org/10.1074/jbc.M405024200

**Pendiente:** una fuente específica para −28 A→G (por ejemplo, su ficha en IthaGenes).
