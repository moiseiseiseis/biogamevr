# Diseño
Modelado 3D a partir de los sketches y storyboards de Game Design.

- `fuentes/` — archivos `.blend` editables
- `exports/` — `.fbx` / `.glb` listos; Desarrollo los importa a `04-desarrollo/BioVR/Assets/_BioVR/Modelos/`

## Primera entrega (prelim. vie 9, final sáb 10 oct)
| Modelo | Archivo fuente | Export | Detalle |
| --- | --- | --- | --- |
| Cabina interior | `fuentes/submarino-cabina.blend` | `exports/submarino-cabina.fbx` | Alto (lo ve el jugador en VR) |
| Exterior del submarino | `fuentes/submarino-exterior.blend` | `exports/submarino-exterior.fbx` | Bajo |
| Escenario celular básico | `fuentes/escenario-celular.blend` | `exports/escenario-celular.fbx` | Bajo/medio |

## Cómo subir los modelos
1. **Descomprime** el `.zip`/`.rar`. **No subas el comprimido**: git no puede ver lo que trae adentro y está bloqueado en `.gitignore`.
2. Cada `.blend` va a `fuentes/` y su `.fbx` a `exports/`, con **el mismo nombre** (`submarino-cabina.blend` → `submarino-cabina.fbx`).
3. Si trae texturas, van en `exports/texturas/`. En Blender, antes de exportar, usa *File → External Data → Pack Resources* para que el `.blend` también las lleve.
4. No subas los `.blend1` (respaldos automáticos de Blender); ya se ignoran solos.
5. Rama `diseno/<modelo>` (por ejemplo `diseno/submarino-cabina`) y PR a `main`, igual que los demás equipos. Los `.blend`, `.fbx` y las imágenes se suben solos por Git LFS.
6. Si el modelo es de internet, anótalo en `/CREDITOS.md`.

Desarrollo (rol 6) copia los `.fbx` de `exports/` a Unity y sustituye los placeholders.

## Pipeline Blender → Unity (acuerdo para no romper nada)
- **Formato:** FBX
- **Unidades:** metros, escala 1 = 1 m. Aplica transformaciones (`Ctrl+A → All Transforms`) antes de exportar
- **Ejes:** Blender es Z-arriba, Unity Y-arriba. En el exportador FBX: *Forward* = `-Z Forward`, *Up* = `Y Up`, *Apply Transform*
- **Polígonos (guía para visor standalone):** cabina ≤ 50k tris, exterior ≤ 20k, escenario ≤ 30k
- **Nombres:** minúsculas, sin acentos ni espacios, con guiones: `submarino-cabina.fbx`
- **Objetos interactivos** (botones, palancas) como objetos **separados** y con nombre claro (`boton-reduccion`), para que Desarrollo pueda programarlos
- **Modelos de internet:** registra autor y licencia en [`/CREDITOS.md`](../CREDITOS.md) antes de subirlos

## Versiones
- Blender 5.2.2 LTS: versión comprobada para la entrega de personajes de Chris. La versión común de todo el equipo sigue pendiente de acuerdo.

## Personajes — entrega del 29 de octubre; revisión del 30 de octubre de 2026

Se incorporan el **Explorador Celular** y **El Investigador**, este último con revisión de cara y hombros y dos variantes estáticas (lectura y brazos separados). Fuentes editables en Blender 5.2.2 LTS, exportaciones FBX/GLB, vistas, scripts e informes se describen en la [guía de entrega](documentacion/personajes/entrega-personajes.md).

- [Explorador Celular](fuentes/explorador-celular.blend) y [El Investigador](fuentes/investigador.blend).
- [Bitácora en PDF](documentacion/personajes/bitacora-personajes.pdf), [Word](documentacion/personajes/bitacora-personajes.docx) y [Markdown](documentacion/personajes/bitacora-personajes.md).
- [Variante ligera del Investigador](fuentes/investigador-ligero.blend): 24.214 triángulos con brazos separados y 27.060 con tableta.
- [Revisión del PR 44 y decisiones de integración](documentacion/personajes/revision-pr44-personajes.md).
- [Comprobación de las diez exportaciones](validacion/personajes/exportaciones-verificacion.json).

Descargar los binarios con Git LFS. La entrega establece proporciones, materiales planos y geometría estática; rig, texturas finales e integración y rendimiento en Unity/VR quedan pendientes.

El Investigador y La Investigadora del PR 43 son personajes distintos para roles secundarios o de fondo. El Explorador Celular se propone como reemplazo de `explorador_caricatura`; Desarrollo mantiene el anterior si la integración presenta conflictos. Las fuentes siguen en `fuentes/`, los archivos para importar en `exports/` y los avances documentados en `documentacion/personajes/`. Para próximas tareas se usará `diseno/<tarea>`; esta revisión continúa en el PR 44 existente.

## Neo Investigador — entrega del 1 de noviembre de 2026

Nueva versión de El Investigador sobre un avatar de VRoid Studio: conserva su cara, cuerpo y esqueleto, viste el equipo de la lámina de Diseño y queda **riggeado y pesado, listo para animar** (sin animaciones). No reemplaza ni modifica `investigador.blend` ni `investigador-ligero.blend`.

- [Fuente editable](fuentes/neo-investigador.blend) y exportaciones [FBX](exports/neo-investigador.fbx), [GLB](exports/neo-investigador.glb) y [tableta](exports/neo-investigador-tableta.fbx): 55.551 triángulos, 111 huesos y 58 expresiones faciales.
- [Guía de entrega](documentacion/personajes/entrega-neo-investigador.md) y bitácora en [PDF](documentacion/personajes/bitacora-neo-investigador.pdf), [Word](documentacion/personajes/bitacora-neo-investigador.docx) y [Markdown](documentacion/personajes/bitacora-neo-investigador.md).
- [Vistas](vistas/personajes/neo-investigador/neo-investigador-vistas.png) y [verificación de exportaciones](validacion/personajes/neo-investigador-verificacion.json).

El FBX sale sin *Apply Transform*, porque esa opción falla con armaduras; el motivo está en la guía. Rendimiento en el visor, animaciones y sujeción de la tableta quedan pendientes.

## Operador del Mecha — entrega del 17 de noviembre de 2026

Personaje nuevo sobre un avatar de VRoid Studio, según la lámina «Mecha Operator» de Game Design (`02-game-design/sketches/personajes/08-operador-mecha/`). Viste la túnica, el arnés y el equipo de la lámina y queda **riggeado y pesado, listo para animar** (sin animaciones). Se preparó para Meta Quest.

- [Fuente editable](fuentes/operador-mecha.blend) y exportaciones [FBX](exports/operador-mecha.fbx), [GLB](exports/operador-mecha.glb) y [panel holográfico](exports/operador-mecha-panel.fbx): 20.562 triángulos, 3 materiales, texturas de 2048 px, 59 huesos y 57 expresiones faciales.
- [Guía de entrega](documentacion/personajes/entrega-operador-mecha.md) y bitácora en [PDF](documentacion/personajes/bitacora-operador-mecha.pdf), [Word](documentacion/personajes/bitacora-operador-mecha.docx) y [Markdown](documentacion/personajes/bitacora-operador-mecha.md).
- [Vistas](vistas/personajes/operador-mecha/operador-mecha-vistas.png) y [verificación de exportaciones](validacion/personajes/operador-mecha-verificacion.json).

El FBX sale sin *Apply Transform*, como el del Neo Investigador. La guía recomienda los materiales de Unity (URP) y falta confirmar con Desarrollo el shader toon. Quedan pendientes la prueba en el visor, las animaciones y la sujeción del panel a la mano.

## Llaveros — 25 de noviembre de 2026

Cuatro llaveros con volumen, tipo juguete, para imprimir en la Bambu Lab A1 mini: **Operadora del Mecha**, **Seraphim Mecha**, **ASTRA** y el **ARNm AUG–GGC–UUU–UAG** de la fase A. Miden entre 42 y 74 mm de alto, están en milímetros y llevan argolla con orificio de Ø 3,8 mm.

- [STL para imprimir](exports/llaveros/) y [fuente editable](fuentes/llaveros.blend).
- [Guía para el estudio de impresión](documentacion/llaveros/LEEME.md), [vistas](vistas/llaveros/llaveros.png) y [comprobación de las mallas](validacion/llaveros/llaveros-verificacion.json).

Se imprimen de pie con soportes de árbol. Todavía no se ha hecho una impresión de prueba.
