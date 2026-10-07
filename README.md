# BioGameVR

Videojuego de realidad virtual sobre estructuras biológicas y electrofisiología, para presentarse en una feria de ciencias.

El jugador controla un **mecha submarino** que se reduce a distintas escalas del cuerpo humano para **mantener la homeostasis**: mecanismos, estructuras y órganos están "desapareciendo" y el jugador debe sustituirlos. Cada sustitución es un minijuego.

## Entrega actual — lunes 12 oct 2026

**Escena inicial jugable en VR** (todo listo el **sábado 10 oct**):

1. **Hangar** — el submarino visto por fuera
2. **Abordaje** — la tripulación sube; el jugador queda en la cabina
3. **Viaje** — reducción de escala y entrada al cuerpo humano
4. **Llegada** — el submarino llega a un espacio celular *(fin de la entrega)*

Plan completo: [`docs/minutas/2026-10-07-minuta-2-primera-entrega.md`](docs/minutas/2026-10-07-minuta-2-primera-entrega.md) · Fechas: [`06-management/roadmap.md`](06-management/roadmap.md)

**Herramientas:** Unity (C#, OpenXR + XR Interaction Toolkit) y Blender. Recursos externos → [`CREDITOS.md`](CREDITOS.md).

## Equipos y carpetas

| Carpeta | Equipo | Objetivo |
| --- | --- | --- |
| [`01-investigacion/`](01-investigacion/) | Investigación | Entender las estructuras biológicas y comunicarlas de forma visualizable |
| [`02-game-design/`](02-game-design/) | Game Design | Sketches, storyboards y cartel para la feria |
| [`03-diseno/`](03-diseno/) | Diseño | Modelado 3D a partir de sketches y storyboards |
| [`04-desarrollo/`](04-desarrollo/) | Desarrollo | Programar objetos, funciones y entidades del juego |
| [`05-implementacion/`](05-implementacion/) | Implementación | Pruebas de UX y dominio del visor VR |
| [`06-management/`](06-management/) | Management | Coordinación, deadlines y reportes al coordinador |
| [`docs/`](docs/) | Todos | Minutas, GDD y decisiones |

## Flujo de trabajo

```
Investigación ─┐
               ├─► Diseño ─► Desarrollo ─► Implementación
Game Design ───┘
          (Management coordina a todos)
```

Cada estructura biológica es un **issue** con la plantilla *Estructura biológica* que avanza por el tablero de Projects:
`Investigación → Game Design → Diseño → Desarrollo → Pruebas → Listo`.

## Reglas básicas

1. Nadie sube directo a `main`: trabaja en una rama (`diseno/ribosoma`, `dev/minijuego-atp`) y abre un Pull Request.
2. Archivos pesados (`.blend`, `.fbx`, `.png`, audio) van por **Git LFS**. Instálalo antes de clonar: `git lfs install`.
3. Sube solo fuentes y exports finales, no cada versión intermedia.

## Minijuegos propuestos (por validar)

| Tema | Mecánica | Carpeta en Unity |
| --- | --- | --- |
| Energía y metabolismo | Guiar iones y fosforilar ADP → ATP | `Minijuegos/ATP` |
| Síntesis de proteínas | Emparejar anticodones (ARNt) con codones (ARNm) | `Minijuegos/Traduccion` |
| Información genética | Enrollar y liberar cadenas de ADN | `Minijuegos/ADN` |
| Estructura celular | Conectar actina y microtúbulos | `Minijuegos/Citoesqueleto` |
| Reproducción y regeneración | Coordinar el huso mitótico | `Minijuegos/Mitosis` |
