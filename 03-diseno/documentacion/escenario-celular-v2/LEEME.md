# Escenario celular v2

Interior de una célula animal en interfase, organizado como una célula real: todo se ordena alrededor del núcleo y del centrosoma, y los microtúbulos funcionan como vías del centro a la periferia. Reemplaza al escenario celular de la primera entrega (`escenario-celular`), que sigue en el repo sin cambios. La comparación completa está en [`comparacion-v1-v2.md`](comparacion-v1-v2.md).

![v1 (izquierda) y v2 (derecha)](comparacion-v1-v2.jpg)

## Archivos

| Archivo | Qué es |
|---|---|
| `fuentes/escenario-celular-v2.blend` | Escena editable con texturas empaquetadas (82 MB). Incluye luces, niebla y cámaras de render, que no van en el FBX |
| `exports/escenario-celular-v2.fbx` | Escena completa con LOD0/LOD1/LOD2 por pieza (28,8 MB) |
| `exports/texturas/escenario-celular-v2/` | 49 texturas, máximo 2048 px. El FBX las busca en esta carpeta con ruta relativa |
| `documentacion/escenario-celular-v2/escenario-celular-v2-informe.json` | Medidas de la v1 y la v2 (triángulos, objetos, tamaños) |

## Medidas

- **Unidades:** metros. −Z Forward, Y Up. Escala biológica: **32,5 m por µm**.
- **Célula:** 1126 × 1082 × 883 m (≈ 35 × 33 × 27 µm), forma irregular, completamente cerrada.
- **Triángulos:** ~300.000 dibujados desde cualquier punto con los LOD activos (294k–303k en 5 puntos medidos). Sin LOD, todo al máximo, son 467k.
- **Gameplay:** `Spawn_Nave` sobre la pista de aterrizaje del retículo endoplásmico rugoso y `Zona_Aterrizaje` en la pista.

## Importar en Unity (Quest 3)

1. Copia `escenario-celular-v2.fbx` y la carpeta `texturas/escenario-celular-v2/` manteniendo la misma posición relativa (`exports/` y `exports/texturas/`). Scale Factor 1.
2. **LODGroup:** Unity crea uno por cada grupo `_LOD0/_LOD1/_LOD2`. Ajusta las transiciones (fracción de la altura de pantalla):

   | Piezas | LOD0 | LOD1 | LOD2 | Culled |
   |---|---|---|---|---|
   | Redes en bloques (objetos dentro de `*_Bloques`: telaraña de filamentos, actina, microtúbulos, ribosomas libres, proteínas) | > 45 % | > 18 % | > 8 % | < 8 % |
   | Organelos y demás piezas | > 15 % | > 5 % | > 1 % | < 1 % |

   Con los valores por defecto de Unity las redes se quedan con demasiado detalle y la escena pasa de los 300k.
3. **Texturas:** compresión ASTC; ya están a 2048 como máximo.
4. **Materiales:**
   - Piezas repetidas (mitocondrias, kinesinas, ribosomas, lisosomas, endosomas): GPU Instancing.
   - `M_Fagosoma`, `M_Burbuja` y `M_Lisosoma_Membrana`: Transparent (son las únicas con transparencia).
5. **Ambiente:** las luces y la niebla volumétrica no van en el FBX. Usa la niebla de Unity (violeta-azul) y un bloom ligero; la emisión de los materiales da el brillo neón.

## Qué contiene

| Elemento | Cantidad | Dónde |
|---|---|---|
| Núcleo con poros | 1 | Algo desplazado del centro |
| Retículo endoplásmico (rugoso + liso, una sola pieza) | 1 | Terrazas rugosas con ribosomas que nacen de la envoltura nuclear; cinturón tubular liso alrededor; pista de aterrizaje integrada |
| Golgi con centrosoma | 1 | Junto al núcleo; cara cis hacia el RE, trans hacia la membrana |
| Microtúbulos | 56 | Salen del centrosoma en estrella, rodean el núcleo y llegan a la membrana |
| Kinesinas | 45 | Caminan sobre los microtúbulos hacia la periferia |
| Filamentos intermedios | 4 + red | Canasta bajo el núcleo que se ramifica en una red hasta la membrana |
| Actina | 1 red | Corteza bajo la membrana con hebras hacia el citoplasma |
| Mitocondrias | 70 + 10 redes | Sobre los microtúbulos |
| Lisosomas · peroxisomas | 12 · 8 | Lisosomas junto al Golgi; peroxisomas junto al RE liso |
| Endosomas | 10 tempranos + 4 tardíos | Tempranos bajo la membrana; tardíos junto al Golgi |
| Vesículas | 8 del Golgi + 24 COPII + 70 de carga + 40 burbujas | Entre RE y Golgi, sobre los microtúbulos y en el citoplasma |
| Ribosomas libres | ~1100 (polisomas) + 10 polirribosomas | Citoplasma |
| E. coli en fagosoma | 1 + 2 lisosomas fusionándose | Parte baja de la célula |
| Virus | 4 | Repartidos |
| Membrana | 1 | Textura de bicapa lipídica |

## Fuentes biológicas

Alberts et al., *Molecular Biology of the Cell*, 6.ª ed. (caps. 12, 13 y 16); *Guyton y Hall, Tratado de fisiología médica*, 14.ª ed. (cap. 2); Goodsell, *The Machinery of Life*, 2.ª ed. (caps. 1 y 5); video *Biology: Cell Structure* de Nucleus Medical Media como referencia visual. La organización espacial tomó como referencia la célula «Celula» de k10tra (Sketchfab, CC BY 4.0), sin usar su geometría.
