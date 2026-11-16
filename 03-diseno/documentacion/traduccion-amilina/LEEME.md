# Traducción de la amilina

Piezas moleculares para la síntesis de la amilina: el **ribosoma 80S**, el **ARNt**, el **ARNm** y la **amilina**, más las piezas de la **fase A** (mesa de traducción), que son cuatro ARNt con su aminoácido y un ARNm corto de 4 codones. Todo parte de estructuras o secuencias reales (PDB, NCBI, UniProt), simplificadas a pocos polígonos con caras facetadas.

Esta entrega agrega solo las piezas de la fase A (`arnt_fase_a` y `arnm_corto_fase_a`). Los modelos base ya están fusionados y **no se modificaron**.

## Archivos

| En el repo | Qué es |
|---|---|
| `03-diseno/fuentes/arnt_fase_a.blend` | **Nuevo.** Los 4 ARNt de la fase A: Met, Gly, Phe y el distractor Gln |
| `03-diseno/exports/arnt_fase_a.fbx` | **Nuevo.** Los mismos 4 ARNt, solo mallas |
| `03-diseno/fuentes/arnm_corto_fase_a.blend` | **Nuevo.** ARNm de 4 codones: AUG–GGC–UUU–UAG |
| `03-diseno/exports/arnm_corto_fase_a.fbx` | **Nuevo.** El mismo ARNm, con un vacío padre y un objeto por codón |
| `amilina_modelo`, `arnt_modelo`, `arnm_modelo`, `ribosoma_80S_4ug0` (`.blend` y `.fbx`) | Modelos base, ya en el repo |

## Escala: 1 u = 1 Å

- Ribosoma 80S: ~29 nm (290 u). ARNt: 71 Å del anticodón al extremo CCA. ARNm completo: ~1.800 Å (90 codones). Amilina: ~26 Å.
- En Unity, tal como se importaron los modelos base, queda **1 Å = 1 cm**: el ARNm largo mide ~18 m y de `Sitio_P` a `Sitio_A` hay ~26 cm.
- FBX: −Z Forward, Y Up, **con** *Apply Transform* (Bake Space Transform), igual que los modelos base. Sin luces ni cámaras.

## Objetos

### ARNt de la fase A (`arnt_fase_a`)

Cada ARNt es un objeto raíz (el esqueleto) con tres hijos. Todos comparten la geometría de PDB 1EHZ; solo cambian el aminoácido y las bases del anticodón.

| Objeto | Qué es |
|---|---|
| `ARNt_Met`, `ARNt_Gly`, `ARNt_Phe`, `ARNt_Gln` | Esqueleto (tubo por los fósforos). Es la raíz de cada ARNt |
| `ARNt_XXX_Bases` | Las 73 bases que no son anticodón, coloreadas por nucleótido |
| `ARNt_XXX_Anticodon_YYY` | Las 3 bases del anticodón (posiciones 34–36), como objeto aparte para poder cambiarlas |
| `ARNt_XXX_Aminoacido` | Esfera del aminoácido en el extremo 3′ (CCA) |

| ARNt | Anticodón (5′→3′) | Lee el codón | Color del aminoácido |
|---|---|---|---|
| Met | CAU | AUG (inicio) | Gris `#8C95A6` |
| Gly | GCC | GGC | Blanco `#FFFFFF` |
| Phe | AAA | UUU | Rosa `#FF3EA5` |
| Gln (distractor) | CUG | CAG (no está en la mesa) | Azul hielo `#A8F0FF` |

- **Orientación:** anticodón abajo y extremo CCA arriba, como dentro del ribosoma, con el codo hacia el frente abierto de las ranuras.
- **Origen:** a la mitad entre el anticodón y el CCA, que es la misma altura que los marcadores `Sitio_E/P/A` del ribosoma.
- En el `.blend` están en fila cada 90 u solo para verlos juntos; en Unity se colocan en los marcadores.

### ARNm corto (`arnm_corto_fase_a`)

| Objeto | Qué es |
|---|---|
| `ARNm_Corto` | Vacío padre. Su origen es el centro de `Codon_1_AUG` |
| `Codon_1_AUG`, `Codon_2_GGC`, `Codon_3_UUU`, `Codon_4_UAG` | Un objeto por codón, con el origen en su centro y separados **26 u** (lo mismo que de `Sitio_P` a `Sitio_A`). Cada codón mide 18 u (3 nucleótidos de 6 u) y deja 8 u de hueco con el siguiente |

- Las bases apuntan hacia arriba (+Z en Blender), hacia el anticodón del ARNt.
- Esqueleto blanco en AUG y UAG (inicio y paro), oscuro en GGC y claro en UUU, con los mismos materiales que el ARNm largo.

## Triángulos

| Parte | Tris |
|---|---|
| Cada ARNt de la fase A (esqueleto 1.808 + bases 876 + anticodón 36 + aminoácido 20) | 2.740 |
| Los 4 ARNt de la fase A | 10.960 |
| ARNm corto (4 codones de 80) | 320 |
| *Referencia:* ribosoma 80S / ARNt base / ARNm largo / amilina | 4.279 / 2.740 / 6.488 / 1.876 |

## Unity

1. **ARNt en el ribosoma:** colocar cada ARNt en su marcador (`Sitio_E`, `Sitio_P` o `Sitio_A`) sin rotarlo, con el ribosoma también sin rotar. El ARNt mide 71 Å de alto y las ranuras 76 Å.
2. **ARNm corto:** poner `ARNm_Corto` sobre la línea que va de `ARNm_5prima` a `ARNm_3prima`, con `Codon_1_AUG` justo debajo de `Sitio_P` (misma posición a lo largo de esa línea). Así `Codon_2_GGC` queda debajo de `Sitio_A`. Para translocar, se mueve `ARNm_Corto` un paso de 26 cm hacia el lado 5′ (hacia `Sitio_E`).
3. **Ancho:** cada ARNt mide ~30 Å de ancho y las ranuras 18 Å. Con dos ARNt en sitios vecinos (P y A) se tocan un poco; si estorba, se pueden escalar al 85 %.
4. **Cambiar el anticodón:** basta con sustituir el objeto `ARNt_XXX_Anticodon_YYY`; el resto del ARNt es igual en las cuatro variantes.
5. **Materiales:** emisión suave sobre fondo oscuro; la emisión hay que rehacerla en Shader Graph. Ningún material ni objeto trae sufijo `.001` (se revisó dentro de los dos FBX).

## Pendiente de confirmar

| Tema | Qué pasa | Qué se hizo |
|---|---|---|
| Distractor “Gln (GUC)” | El anticodón 5′-GUC-3′ lee el codón GAC, que codifica **ácido aspártico (Asp)**, no glutamina. La glutamina usa los anticodones CUG o UUG (leen CAG y CAA) | Se hizo **Gln con CUG**. Si se prefiere **Asp con GUC**, solo cambia el objeto del anticodón (y el color del aminoácido). En los dos casos sigue siendo distractor: ni GAC ni CAG están en AUG–GGC–UUU–UAG |
| Anticodón de Phe | 1EHZ trae **GAA**, que lee UUC; la mesa usa UUU | La variante de Phe usa **AAA** |
| Color de la metionina | La paleta de 15 colores salió de la amilina, que no tiene metionina | Gris `#8C95A6`, el mismo gris que las hojas de referencia dan a Met/Pro, distinto del blanco de la glicina |
| Largo del ARNm completo | La nota de revisión dice ~69 codones | Son **90**: 89 del precursor más el de paro (270 nt, NM_000415.3, 119..388) |

## De dónde salen las medidas

| Elemento | Dato | Fuente |
|---|---|---|
| ARNt | Forma de L, 76 nt; anticodón en 34–36 y CCA en 74–76 | PDB **1EHZ** (ARNt-Phe de levadura, rayos X, 1,93 Å; Shi y Moore, *RNA*, 2000). Se usa solo la forma |
| ARNm | Región codificante de 270 nt; AUG en la posición 119 y UAG en la 386–388 | NCBI **NM_000415.3** (gen IAPP, variante 1) |
| Codones de la mesa | AUG = Met (inicio), GGC = Gly, UUU = Phe, UAG = paro | Código genético estándar |
| Ribosoma | Ribosoma humano 80S; sitios E–P–A y túnel ubicados con el centro de decodificación (A1824–A1825 del ARNr 18S) y las proteínas L23A, L26 y L35 | PDB **4UG0** (cryo-EM, 3,6 Å; Khatter et al., *Nature*, 2015) |
| Amilina | 37 aa, puente disulfuro Cys2–Cys7 | PDB **2L86** (RMN); UniProt **P10997** |
| Colores | Paleta por aminoácido y por nucleótido | Bitácora de modelado molecular (9–15 nov) y *Hojas de referencia BioGameVR* |
