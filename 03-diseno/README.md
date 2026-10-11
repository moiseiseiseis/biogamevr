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
- Blender: _por definir (todos la misma)_
