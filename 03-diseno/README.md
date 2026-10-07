# Diseño
Modelado 3D a partir de los sketches y storyboards de Game Design.

- `fuentes/` — archivos `.blend` editables
- `exports/` — `.fbx` / `.glb` listos; Desarrollo los importa a `04-desarrollo/BioVR/Assets/Modelos/`

## Primera entrega (prelim. vie 9, final sáb 10 oct)
| Modelo | Archivo fuente | Export | Detalle |
| --- | --- | --- | --- |
| Cabina interior | `fuentes/submarino-cabina.blend` | `exports/submarino-cabina.fbx` | Alto (lo ve el jugador en VR) |
| Exterior del submarino | `fuentes/submarino-exterior.blend` | `exports/submarino-exterior.fbx` | Bajo |
| Escenario celular básico | `fuentes/escenario-celular.blend` | `exports/escenario-celular.fbx` | Bajo/medio |

## Pipeline Blender → Unity (acuerdo para no romper nada)
- **Formato:** FBX
- **Unidades:** metros, escala 1 = 1 m. Aplica transformaciones (`Ctrl+A → All Transforms`) antes de exportar
- **Ejes:** Blender es Z-arriba, Unity Y-arriba. En el exportador FBX: *Forward* = `-Z Forward`, *Up* = `Y Up`, *Apply Transform*
- **Polígonos (guía para visor standalone):** cabina ≤ 50k tris, exterior ≤ 20k, escenario ≤ 30k
- **Nombres:** minúsculas, sin acentos ni espacios, con guiones: `submarino-cabina.fbx`
- **Objetos interactivos** (botones, palancas) como objetos **separados** y con nombre claro (`boton-reduccion`), para que Desarrollo pueda programarlos
- **Modelos de internet:** registra autor y licencia en [`/CREDITOS.md`](../CREDITOS.md) antes de subirlos

## Versiones
- Blender: _por definir (todos la misma)_
