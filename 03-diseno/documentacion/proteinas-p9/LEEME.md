# Proteínas de la fila P9

Modelos que Desarrollo pidió en la fila P9 de su tabla de decisiones: la **sintetasa**, las **chaperonas BiP y PDI**, el **proteasoma 26S**, la **amilina en horquilla β** y las **esferas de aminoácido** con la paleta completa. Todo parte de estructuras reales de PDB, simplificadas a pocos polígonos con caras facetadas, igual que el ribosoma.

Los ARNt de Met, Gly y Gln que también pedía la fila P9 ya están en `arnt_fase_a` (entrega anterior). En esta entrega **no se modificó ningún modelo existente**: todo es archivo nuevo.

## Archivos

| En el repo | Qué es |
|---|---|
| `03-diseno/fuentes/esferas_aminoacidos.blend` / `exports/esferas_aminoacidos.fbx` | Las 20 esferas de aminoácido con su color |
| `03-diseno/fuentes/sintetasa_modelo.blend` / `exports/sintetasa_modelo.fbx` | Sintetasa genérica (MetRS humana) con su esfera de metionina y marcadores para acoplar el ARNt |
| `03-diseno/fuentes/bip_chaperona.blend` / `exports/bip_chaperona.fbx` | Chaperona BiP en dos estados: `BiP_Abierta` y `BiP_Cerrada` |
| `03-diseno/fuentes/pdi_chaperona.blend` / `exports/pdi_chaperona.fbx` | Chaperona PDI: 4 dominios en herradura y 2 sitios activos |
| `03-diseno/fuentes/proteasoma_26S.blend` / `exports/proteasoma_26S.fbx` | Proteasoma 26S: núcleo 20S y dos tapas 19S |
| `03-diseno/fuentes/amilina_horquilla.blend` / `exports/amilina_horquilla.fbx` | Amilina en horquilla β: 37 esferas, una por residuo |

## Escala: 1 u = 1 Å

- Igual que los modelos moleculares anteriores: en Unity, tal como se importaron, queda **1 Å = 1 cm**.
- Tamaños: esfera de aminoácido 6,8 Å de diámetro · horquilla ~37 Å · PDI ~94 Å · sintetasa ~100 Å · BiP ~125 Å · proteasoma ~460 × 210 Å (el ribosoma mide ~290 Å).
- FBX: −Z Forward, Y Up, **con** *Apply Transform* (Bake Space Transform). Sin luces ni cámaras. Se revisó dentro de cada FBX que ningún objeto ni material trae sufijo `.001`.

## Paleta de aminoácidos (20 colores)

Un color por aminoácido. Los materiales se llaman `Mat_AA_<letra>_<nombre>` y son **los mismos** en la amilina, los ARNt, las esferas, la horquilla y el péptido de BiP.

| Letra | Color | Letra | Color | Letra | Color | Letra | Color |
|---|---|---|---|---|---|---|---|
| I | `#FF2E2E` | F | `#FF3EA5` | S | `#3DFF7A` | K | `#2F6BFF` |
| L | `#FF7A1A` | W | `#6A0DAD` | T | `#00C49A` | R | `#00E5FF` |
| V | `#FFB000` | Y | `#D64DFF` | N | `#B6FF3B` | H | `#9B5CFF` |
| A | `#E8C39E` | P | `#5C6273` | Q | `#A8F0FF` | D | `#B3001B` |
| M | `#8C95A6` | G | `#FFFFFF` | C | `#FFF200` | E | `#7A0F3D` |

D, E, P y W son nuevos (la amilina no los tiene); M ya estaba en los ARNt de la fase A.

## Objetos

### Esferas de aminoácido (`esferas_aminoacidos`)

- Vacío padre `Esferas_Aminoacidos` con 20 hijos `AA_<letra>_<nombre>` (por ejemplo `AA_K_Lisina`), cada uno con el origen al centro.
- Misma geometría que la esfera de los ARNt: 20 caras, 3,4 Å de radio. Sirven para la esfera del ARNt, el péptido que se arma y cualquier aminoácido suelto.

### Sintetasa (`sintetasa_modelo`)

| Objeto | Qué es |
|---|---|
| `Sintetasa` | Vacío padre, en el centro de la enzima |
| `Sintetasa_Nucleo_Catalitico` | Dominio con el sitio activo (violeta) |
| `Sintetasa_Dominio_CP` | Dominio de los dedos de zinc (lila) |
| `Sintetasa_Dominio_Anticodon` | Dominio que lee el anticodón (verde azulado) |
| `Sintetasa_Aminoacido_Met` | Esfera de metionina en la boca del sitio activo (`Mat_AA_M_Metionina`) |
| `ARNt_Anclaje` | Marcador: ahí va el origen de cualquier ARNt de `arnt_fase_a` |
| `Sitio_Activo`, `Sitio_Anticodon` | Marcadores: metionina del cristal y triptófano 725 |

### BiP (`bip_chaperona`)

Dos raíces, una por estado. La **base es la misma malla** en las dos, en la misma posición respecto a su raíz.

| Objeto | Qué es |
|---|---|
| `BiP_Abierta` / `BiP_Cerrada` | Vacíos raíz, en el centro de la base |
| `BiP_*_Base_ATPasa` | Base que une ATP (azul) |
| `BiP_*_Mandibula` | Mandíbula que atrapa el péptido (ámbar) |
| `BiP_*_Tapa` | Tapa (coral): pegada a la base en la abierta, cerrada sobre la mandíbula en la cerrada |
| `BiP_Cerrada_Conector` | Tubo que une la base con la mandíbula en el estado cerrado |
| `BiP_Cerrada_Sustrato_NRLLLTG` | Péptido atrapado, 7 esferas con la paleta |
| `BiP_*_Sitio_Nucleotido`, `BiP_*_Sitio_Sustrato` | Marcadores: ATP/ADP y donde va la proteína atrapada |

### PDI (`pdi_chaperona`)

| Objeto | Qué es |
|---|---|
| `PDI` | Vacío padre |
| `PDI_Dominio_a`, `PDI_Dominio_b`, `PDI_Dominio_b_prima`, `PDI_Dominio_a_prima` | Los 4 dominios en herradura (a y a′ azules, b y b′ lilas) |
| `PDI_Sitio_Activo_a`, `PDI_Sitio_Activo_a_prima` | Sitios activos CGHC en amarillo (`Mat_AA_C_Cisteina`) |

### Proteasoma 26S (`proteasoma_26S`)

| Objeto | Qué es |
|---|---|
| `Proteasoma_26S` | Vacío padre, en el centro del barril. El eje del barril es Z en Blender |
| `Proteasoma_20S_Anillo_Alfa_1/2` | Anillos α (azul acero) |
| `Proteasoma_20S_Anillo_Beta_1/2` | Anillos β, los que cortan (terracota) |
| `Proteasoma_19S_1/2_Anillo_ATPasa` | Motor que desdobla y empuja la proteína (arena) |
| `Proteasoma_19S_1/2_Tapa` | Tapa de la 19S (verde salvia) |
| `Proteasoma_Entrada_1/2` | Marcadores en el poro de cada anillo ATPasa, por donde entra la proteína |
| `Proteasoma_Camara_Central` | Marcador en el centro del barril |
| `Proteasoma_Sitio_Activo_b1/b2/b5_1/2` | Los 6 sitios que cortan (treonina 1 de β1, β2 y β5) |

### Amilina en horquilla (`amilina_horquilla`)

- Vacío padre `Amilina_Horquilla` con 37 esferas `Horquilla_NN_<letra>_<nombre>` (de `Horquilla_01_K_Lisina` a `Horquilla_37_Y_Tirosina`). Al ser objetos separados se puede animar el plegado residuo por residuo.
- Marcadores: `Horquilla_Hebra_1_12a18`, `Horquilla_Giro_19a21` y `Horquilla_Hebra_2_22a28`.

## Triángulos

| Modelo | Tris |
|---|---|
| Esferas de aminoácido (20 × 20) | 400 |
| Amilina en horquilla (37 × 20) | 740 |
| PDI | 1.560 |
| BiP, cada estado (+ 140 del péptido en la cerrada) | ~2.000 |
| Sintetasa | 2.554 |
| Proteasoma 26S | 4.798 |
| *Referencia:* ribosoma 80S / ARNt | 4.279 / 2.740 |

## Unity

1. **ARNt en la sintetasa:** con la sintetasa sin rotar, se pone el origen del ARNt en `ARNt_Anclaje` y con la misma rotación que la sintetasa; el extremo CCA llega al sitio activo y el anticodón al dominio verde. Si la sintetasa se rota, el ARNt se hace hijo de `ARNt_Anclaje` con rotación local 0.
2. **Cargar el aminoácido:** la esfera `Sintetasa_Aminoacido_Met` está a ~6 cm de la esfera del ARNt acoplado; se puede animar el paso de una a otra. Para otro aminoácido se cambia el material por el de `esferas_aminoacidos`.
3. **BiP abre y cierra:** se cambia `BiP_Abierta` por `BiP_Cerrada` en la misma posición y rotación; la base no se mueve. La proteína atrapada se coloca en `BiP_*_Sitio_Sustrato`.
4. **Proteasoma:** la proteína entra por `Proteasoma_Entrada_1` o `_2` y se degrada al pasar por `Proteasoma_Camara_Central`.
5. **Materiales:** emisión suave sobre fondo oscuro; la emisión hay que rehacerla en Shader Graph.

## Licencias didácticas

| Modelo | Qué se simplificó |
|---|---|
| Sintetasa | 5GOY no trae ARNt. El acoplamiento es aproximado sobre dos puntos reales (sitio activo y triptófano 725) y se abrió un surco quitando los átomos que chocaban |
| BiP cerrada | 5E85 no trae la base. Su mandíbula se alineó con la de 5E84 y se separó 18 Å, como en la BiP real tras gastar el ATP (en la realidad ese tramo es flexible) |
| PDI | Las esferas amarillas se sacaron a la superficie para que se vean: quedan a 29 Å una de otra; los sitios reales están a 40,3 Å |
| Proteasoma | Se ve cerrado por fuera porque la compuerta de los anillos α está cerrada en la estructura; la cámara interior existe |
| Horquilla | 5K5G solo trae los residuos 10–30; los extremos 1–9 y 31–37 se tomaron de 2L86 y se empalmaron |

## De dónde salen las medidas

| Elemento | Fuente |
|---|---|
| Sintetasa | PDB **5GOY** (MetRS citoplasmática humana con metionina, rayos X, 2,28 Å); UniProt **P56192** |
| BiP | PDB **5E84** (BiP humana completa con ATP, rayos X, 2,99 Å) y **5E85** (mandíbula y tapa con péptido); Yang et al., *Structure*, 2015; UniProt **P11021** |
| PDI | PDB **4EL1** (PDI humana oxidada, rayos X, 2,88 Å; Wang et al., *Antioxid Redox Signal*, 2013); UniProt **P07237** |
| Proteasoma | PDB **5GJR** (26S humano, cryo-EM, 3,5 Å; Huang et al., *Nat Struct Mol Biol*, 2016) |
| Horquilla | PDB **5K5G** (amilina en horquilla β, RMN; Mirecka et al., *Sci Rep*, 2016) y PDB **2L86** |
| Colores y formas | Bitácora de modelado molecular y *Hojas de referencia BioGameVR* |
