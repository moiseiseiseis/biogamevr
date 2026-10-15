# BioGameVR

Videojuego de realidad virtual para una feria de ciencias. Su tema son las estructuras biológicas y la electrofisiología.

El jugador pilota un **mecha submarino** que se reduce de escala hasta entrar en el cuerpo humano: pasa del hangar a los vasos sanguíneos, luego a los capilares y por último al interior de una célula. Su misión es **mantener la homeostasis**. Hay mecanismos, estructuras y órganos que están "desapareciendo", y el jugador debe sustituirlos. Cada sustitución es un minijuego.

## Probarlo

| Quiero… | Ve a |
| --- | --- |
| Abrir el proyecto en Unity y darle *Play* | [`04-desarrollo/README.md`](04-desarrollo/README.md): versión de Unity, paquetes y simulador sin visor |
| Instalar el juego en un visor Meta Quest | [`05-implementacion/guia-visor-vr.md`](05-implementacion/guia-visor-vr.md) |
| Saber cómo se juega | El tutorial dentro del juego. Los controles también están en el menú de pausa |

## Cómo está organizado

Cada equipo tiene una carpeta y trabaja dentro de ella.

| Carpeta | Equipo | Qué hay ahí |
| --- | --- | --- |
| [`01-investigacion/`](01-investigacion/) | Investigación | La biología detrás del juego, explicada para que se pueda visualizar: fichas, escalas y referencias |
| [`02-game-design/`](02-game-design/) | Game Design | Guía de estilo, sketches, storyboards y cartel para la feria |
| [`03-diseno/`](03-diseno/) | Diseño | Modelos 3D: las fuentes de Blender en `fuentes/` y los exports para Unity en `exports/` |
| [`04-desarrollo/`](04-desarrollo/) | Desarrollo | El proyecto de Unity (`BioVR/`), su estructura y las reglas para programar |
| [`05-implementacion/`](05-implementacion/) | Implementación | Uso del visor, plan de pruebas y reportes de experiencia de usuario |
| [`06-management/`](06-management/) | Management | Equipos, roadmap con fechas y reportes al coordinador |
| [`docs/`](docs/) | Todos | Minutas, decisiones y documentos generales |

Las fechas y las entregas están en [`06-management/roadmap.md`](06-management/roadmap.md), y lo que se acordó en cada reunión, en [`docs/minutas/`](docs/minutas/). Este README no cambia con cada entrega.

## Cómo trabajamos

```
Investigación ─┐
               ├─► Diseño ─► Desarrollo ─► Implementación
Game Design ───┘
          (Management coordina a todos)
```

- El trabajo se reparte en **issues** de GitHub, con la etiqueta del equipo que corresponde.
- **Nadie sube directo a `main`.** Cada tarea va en su rama (`diseno/ribosoma`, `rol3/botones`) y entra con un Pull Request.
- Los archivos pesados (`.blend`, `.fbx`, imágenes y audio) van por **Git LFS**.
- Todo recurso externo (modelos, texturas, sonidos) se registra en [`CREDITOS.md`](CREDITOS.md) **antes** de subirlo.

El paso a paso para clonar, crear ramas y nombrar archivos y commits está en [`CONTRIBUTING.md`](CONTRIBUTING.md).

## Tecnología

Unity con C#, URP, OpenXR y XR Interaction Toolkit, para visores **Meta Quest** (Android). Los modelos se hacen en Blender. Las versiones exactas que todos deben usar están en [`04-desarrollo/README.md`](04-desarrollo/README.md).
