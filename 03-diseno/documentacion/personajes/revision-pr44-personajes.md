# Revisión de personajes para el PR 44

30 de octubre de 2026 · School Redemption Game es un nombre de trabajo, todavía no definitivo.

Se atendieron las observaciones del PR 44 sobre integración, procedencia, organización y peso del Investigador. La entrega incorpora una variante estática ligera y mantiene las fuentes originales de mayor detalle para edición. Los avances y las instrucciones se concentran en `03-diseno/documentacion/personajes/`; las fuentes y las exportaciones conservan las carpetas acordadas por Diseño.

## Decisiones de integración

**Investigador e Investigadora.** Son personajes distintos. Pueden coexistir como personajes secundarios o de fondo del hangar; durante la integración puede utilizarse uno de ellos. Esta decisión coincide con la [respuesta de Chris en el PR 44](https://github.com/moiseiseiseis/biogamevr/pull/44#issuecomment-6509476579) y con la [aclaración de Diego en el PR 43](https://github.com/moiseiseiseis/biogamevr/pull/43#issuecomment-6508230419). Esta entrega no modifica los archivos de Diego.

**Explorador Celular.** Se propone como reemplazo visual de `explorador_caricatura` para acercarse al diseño de referencia. Desarrollo puede importar `exports/explorador-celular.fbx`, revisar escala, materiales y posición, y sustituir el modelo visual conservando la configuración del objeto de juego. Si la integración genera conflictos, el modelo anterior queda disponible como alternativa. La sustitución en escenas o prefabs de Unity todavía no se ha realizado en este PR.

**Nombre del proyecto.** School Redemption Game se mantiene como nombre de trabajo. La entrega no decide el nombre final del juego.

## Organización y referencias

Las fuentes `.blend` permanecen en `03-diseno/fuentes/` y los FBX/GLB en `03-diseno/exports/`. Se reubicó la guía detallada en `documentacion/personajes/entrega-personajes.md`, junto a la bitácora y este informe. Vistas e informes técnicos permanecen en `vistas/personajes/` y `validacion/personajes/`.

Las cuatro referencias se conservaron en la copia local y se eliminaron del árbol actual del PR, tanto como PNG independientes como dentro de los `.blend`. Los constructores funcionan sin imágenes externas; admiten una carpeta opcional de referencias para uso local. La procedencia conocida y la declaración pendiente se registran en [CREDITOS.md](../../../CREDITOS.md). Las vistas incluidas en el repositorio son renders de las mallas desarrolladas.

| Referencia retirada de la publicación | Conservación |
| --- | --- |
| explorador-celular-concepto.png | Copia local |
| investigador-concepto.png | Copia local |
| investigador-referencia-inicial.png | Copia local |
| investigadora-estilo.png | Copia local |

La retirada afecta la versión actual del PR; no reescribe commits anteriores. La copia local conserva las referencias para poder completar su procedencia posteriormente.

Blender **5.2.2 LTS** queda registrado en la sección Versiones del README como versión comprobada para esta entrega. No se presupone un acuerdo de versión de todo el equipo. Esta revisión continúa en `Chris_Desing` para conservar el PR 44; las próximas tareas seguirán `diseno/<tarea>`.

## Reducción del Investigador

| Variante | Triángulos originales | Triángulos ligeros | Reducción | Mallas exportadas | FBX original → ligero |
| --- | ---: | ---: | ---: | ---: | ---: |
| Brazos separados | 39.476 | 24.214 | 38.66 % | 247 → 14 | 1199.6 → 687.1 KiB (42.7 % menos) |
| Lectura con tableta | 42.510 | 27.060 | 36.34 % | 286 → 15 | 1338.0 → 771.6 KiB (42.3 % menos) |

Los conteos se obtienen de la geometría evaluada y de la reimportación de los archivos, sin cámaras, luces ni suelo. Las fuentes Blender conservan 247 piezas en la escena de brazos separados y 286 en la de lectura; la agrupación en 14/15 mallas se hace solo en las copias de exportación. Cabeza, gafas, cabello, mochila, botas, manos y tableta conservan grupos identificables.

La reducción se concentra en el rostro, las orejas, los biseles y la chaqueta. Ojos, párpados, labios, gafas y mechones conservan su geometría característica. En la chaqueta se usa disolución planar de 3 grados con límites por material; se conserva el cuello para evitar que cambien los paneles y ribetes. Las fuentes originales permanecen disponibles y la variante ligera mantiene modificadores editables.

La distancia muestreada entre las superficies original y reducida es de **0,23 mm en el rostro** y hasta **1,61 mm en la chaqueta**. Se midió en ambos sentidos, desde vértices a la superficie contraria; no constituye una cota continua de toda la superficie. Las chaquetas reducidas mantienen cero bordes no manifold.

Las variantes ligeras usan 66 y 72 asignaciones de material en total, frente a 248 y 287 en las exportaciones originales. Este conteo y el número de mallas no equivalen a una medición de draw calls: el resultado real depende de los materiales, las pasadas y el agrupamiento dentro de Unity.

## Qué se aligera y qué queda pendiente

Comprimir un ZIP o un `.blend` reduce almacenamiento y transferencia, pero por sí solo no reduce los triángulos que dibuja el juego. Aquí se redujeron realmente los triángulos y las piezas de las exportaciones. Además, se guardaron las fuentes con compresión y se retiraron las imágenes empaquetadas: el Explorador pasó de 5,26 MB a aproximadamente 0,25 MB y el Investigador original de 2,15 MB a 0,47 MB, sin cambiar su geometría original.

Se eligió una reducción conservadora de alrededor del 36–39 % para conservar cara, hombros y separaciones de color. Una variante más agresiva necesita retopología específica, simplificación de detalles secundarios y, posiblemente, un atlas de materiales. Las variantes actuales son estáticas: todavía faltan rig, pesos, animaciones, UV y texturas finales, configuración de LOD y pruebas en Unity/Quest 2. No se afirma una mejora de FPS ni una cantidad segura de personajes simultáneos sin medirla en el dispositivo.

## Comprobaciones y archivos

- Las tres fuentes se reabrieron y validaron: unidades métricas, altura, pies en Z = 0, inventarios, referencias excluidas y ausencia de animaciones.
- Las diez exportaciones FBX/GLB se reimportaron: conteos de mallas y triángulos, dimensiones, materiales y apoyo correctos.
- Se compararon frente, lateral, posterior, rostro y hombros con las mismas cámaras e iluminación. También se inspeccionó un GLB ligero reimportado.
- Ambos scripts de construcción se ejecutaron desde una carpeta independiente sin imágenes de referencia y reprodujeron los conteos originales. La bitácora actualizada se revisó mediante renderizado de sus seis páginas.

[Fuente ligera](../../fuentes/investigador-ligero.blend) · [Informe geométrico](../../validacion/personajes/investigador-optimizacion.json) · [Exportaciones verificadas](../../validacion/personajes/exportaciones-verificacion.json) · [Guía de uso](entrega-personajes.md)

![Comparación de vistas originales y ligeras](../../vistas/personajes/investigador/revision-ligera/comparacion-vistas.png)

![Comparación de rostro y hombros](../../vistas/personajes/investigador/revision-ligera/comparacion-detalles.png)

El método de simplificación usa los modos Collapse y Planar del [modificador Decimate de Blender](https://docs.blender.org/manual/en/latest/modeling/modifiers/generate/decimate.html). Los resultados y límites anteriores proceden de las mediciones de esta entrega.
