# Comparación: escenario celular v1 (en el repo) → v2

- **v1:** `03-diseno/fuentes/escenario-celular.blend` y `exports/escenario-celular.fbx` (primera entrega).
- **v2:** `03-diseno/fuentes/escenario-celular-v2.blend` y `exports/escenario-celular-v2.fbx` (esta entrega).

Las cifras se midieron en los dos `.blend` con el mismo script (detalle en `escenario-celular-v2-informe.json`).

![Arriba: pista. Centro: retículo endoplásmico. Abajo: E. coli. Izquierda v1, derecha v2](comparacion-v1-v2.jpg)

## En números

| | v1 | v2 |
|---|---|---|
| Tamaño de la célula | 501 × 503 × 499 m (esfera) | 1126 × 1082 × 883 m (forma irregular) |
| Triángulos dibujados desde la pista (con LOD) | ~234k | ~295k (294k–303k en 5 puntos) |
| Triángulos con todo al máximo (LOD0) | 350k | 467k |
| Objetos | 528 | 5.885 (las redes van en bloques de 110 m para que tengan LOD) |
| `.blend` | 179,9 MB | 82,0 MB |
| `.fbx` | 10,3 MB | 28,8 MB |
| Texturas | 41 (12 de 4096 px) | 49 (máximo 2048 px) |

La v2 dibuja más triángulos que la v1 porque tiene mucho más contenido (ver abajo), pero gracias a los LOD por bloques se queda en ~300k.

## Organización: qué cambió

| Elemento | v1 | v2 | Por qué |
|---|---|---|---|
| Forma | Esfera perfecta de 500 m | Forma irregular cerrada, más grande | Más espacio para volar; una célula real no es una esfera perfecta |
| Retículo endoplásmico | Pista + 4 plataformas sueltas + 24 cintas generadas; RE liso en 5 redes **flotando sueltas** | **Una sola pieza**: terrazas rugosas que nacen de la envoltura nuclear y cinturón de RE liso **continuo** con el rugoso | En la célula real el RE es una sola membrana, continua con la nuclear (MBoC p. 669) |
| Golgi | **2** (uno junto a la pista y otro lejos) | **1**, junto al núcleo y al centrosoma; cis hacia el RE, trans hacia la membrana | En células animales es un solo complejo junto al núcleo y al centrosoma (MBoC p. 715) |
| Centrosoma | Modelo suelto, lejos del Golgi | Dentro del modelo del Golgi, pegado al núcleo | El centrosoma está junto al núcleo y el Golgi se agrupa a su alrededor (MBoC p. 715) |
| Microtúbulos | 5 modelos sueltos | 56 que salen del centrosoma en estrella hasta la membrana | Nacen en el centrosoma y llegan a la periferia (MBoC p. 930) |
| Kinesinas | — | 45 sobre los microtúbulos | Muestran el transporte de vesículas |
| Filamentos intermedios | 4 cuerdas sueltas | Canasta bajo el núcleo + red ramificada hasta la membrana | Rodean el núcleo y se extienden por toda la célula |
| Actina | 6 filamentos sueltos | Red continua bajo la membrana (corteza) | Mayor densidad justo bajo la membrana (MBoC p. 907) |
| Mitocondrias | 8 cápsulas | 70 cápsulas + 10 redes tubulares | Ocupan ~22 % del volumen (MBoC tabla 12-1); no todas son cápsulas |
| Lisosomas · peroxisomas | 10 · 4 repartidos | 12 junto al Golgi · 8 junto al RE liso | Posición según su función |
| Endosomas | — | 10 tempranos bajo la membrana, 4 tardíos junto al Golgi | Faltaban |
| Vesículas | 12 + 45 burbujas decorativas | 8 del Golgi + 24 COPII (RE→Golgi) + 70 de carga sobre microtúbulos + 40 burbujas | Ahora siguen una ruta (RE → Golgi → membrana) |
| Ribosomas libres | 8 polirribosomas | ~1100 en polisomas + 10 polirribosomas | El citoplasma real está lleno |
| E. coli | Libre en el citoplasma con 8 bacteriófagos | Dentro de un **fagosoma** con 2 lisosomas fusionándose; **sin bacteriófagos** | Una bacteria dentro de una célula solo puede estar fagocitada; los fagos solo infectan bacterias |
| Nucleosomas | 2 de 20 m en los poros | Quitados | Miden 11 nm y están dentro del núcleo |
| Virus | 14 | 4 | Menos protagonismo |

## Rendimiento: qué cambió

- **LOD en bloques:** las redes que cruzan toda la célula (filamentos, actina, microtúbulos, ribosomas libres) se partieron en bloques de 110 m con 3 niveles; lejos se quitan las hebras más cortas en lugar de deformarlas.
- **Piezas repetidas:** comparten malla; sus niveles lejanos bajan al 25 % y 8 % del nivel cercano.
- **Texturas:** de 4096 a 2048 px como máximo, y el `.blend` bajó de 180 MB a 82 MB.

## Qué se conservó de la v1

Estilo retro 80s con la misma paleta, membrana con textura de bicapa lipídica, núcleo, mitocondria, lisosoma, peroxisoma, vesícula, virus y polirribosoma (los mismos modelos de Meshy, ahora con texturas horneadas y LOD), escala de 32,5 m por µm, `Spawn_Nave` y `Zona_Aterrizaje`.
