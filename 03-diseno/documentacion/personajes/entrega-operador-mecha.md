# Entrega del Operador del Mecha — School Redemption Game

17 de noviembre de 2026 · Blender 5.2.2 LTS · rama `diseno/operador-mecha`

Personaje nuevo a partir de un avatar de VRoid Studio y de la lámina «Mecha Operator» de Game Design (`02-game-design/sketches/personajes/08-operador-mecha/`). Está riggeado y pesado, **listo para animar**, sin animaciones. Se preparó para Meta Quest: 20.562 triángulos, 3 materiales y texturas de 2048 px como máximo.

## Abrir el modelo

- [Fuente editable](../../fuentes/operador-mecha.blend). Colecciones `Operador_Ropa` (prendas), `Operador_Equipo` (botas, guantes, arnés, diadema, parche) y `Operador_Props` (panel holográfico). La cara, el cabello y la piel de VRoid se llaman `VRoid_Cara`, `VRoid_Cabello` y `VRoid_Cuerpo`, y el esqueleto `Operador_Mecha_Rig`.
- Unidades métricas: 1 unidad = 1 m, frente hacia -Y, arriba +Z, plantas en Z = 0, pose T. El cuerpo mide 1,61 m; con la diadema, 1,626 m.
- Las piezas están separadas y sus modificadores (grosor, bisel) siguen editables. Los materiales son MToon, editables con el complemento VRM Add-on for Blender.

## Archivos de intercambio

| Archivo | Contenido | Triángulos |
| --- | --- | ---: |
| [operador-mecha.fbx](../../exports/operador-mecha.fbx) | Personaje con esqueleto, pesos y 57 expresiones | 20.562 |
| [operador-mecha.glb](../../exports/operador-mecha.glb) | Lo mismo en glTF binario | 20.562 |
| [operador-mecha-panel.fbx](../../exports/operador-mecha-panel.fbx) y [.glb](../../exports/operador-mecha-panel.glb) | Panel holográfico cian, sin pesos y centrado en el origen | 4 |
| [texturas/operador-mecha/](../../exports/texturas/operador-mecha/) | Texturas PNG | — |

El personaje se exporta en 3 mallas, una por material: `Operador_Cara` (con las expresiones), `Operador_Cabello_Piel` y `Operador_Ropa_Equipo`. Tiene 59 huesos.

Ejes del FBX: Forward -Z y Up Y, con escala aplicada (*FBX All*). **No se activó *Apply Transform***: el manual de Blender la marca como experimental y conocida por fallar con armaduras. Sí se usó en el panel, que no tiene armadura. Las texturas van incrustadas en el FBX y también se entregan sueltas.

## Materiales en Unity (URP, sin UniVRM)

| Material | Textura de color | Configuración recomendada |
| --- | --- | --- |
| OPM_Cara | `operador-mecha_cara.png` | URP Lit o Simple Lit, *Alpha Clipping* 0,5, dos caras |
| OPM_Cabello_Piel | `operador-mecha_cuerpo.png` | URP Lit o Simple Lit, *Alpha Clipping* 0,5, dos caras |
| OPM_Ropa_Equipo | `operador-mecha_cuerpo.png` | URP Lit o Simple Lit, opaco; emisión con `operador-mecha_emision.png` |
| Panel holográfico | `operador-mecha_panel.png` | URP Unlit, transparente, dos caras |

Las texturas de color tienen sombras planas y se ven bien con un material URP simple. Para el aspecto toon del Neo Investigador se recomienda el paquete oficial **Unity Toon Shader** para URP, usando las texturas `*_sombra.png` como color de sombra. Falta confirmar con Desarrollo que ese paquete funcione en su versión de Unity; si no, URP Simple Lit es la opción de respaldo.

## Vistas

- [Vistas en pose T](../../vistas/personajes/operador-mecha/operador-mecha-vistas.png)
- [Pose relajada](../../vistas/personajes/operador-mecha/operador-mecha-pose-relajada.png)
- [Rostro](../../vistas/personajes/operador-mecha/operador-mecha-rostro.png)
- [Panel holográfico](../../vistas/personajes/operador-mecha/operador-mecha-panel.png)
- Vistas sueltas: [frente](../../vistas/personajes/operador-mecha/operador-mecha-frente.png), [lateral](../../vistas/personajes/operador-mecha/operador-mecha-lateral.png), [posterior](../../vistas/personajes/operador-mecha/operador-mecha-posterior.png) y [tres cuartos](../../vistas/personajes/operador-mecha/operador-mecha-tres-cuartos.png)

## Bitácora y validación

- [Bitácora en Word](bitacora-operador-mecha.docx), [PDF](bitacora-operador-mecha.pdf) y [Markdown](bitacora-operador-mecha.md)
- [Reimportación de las exportaciones](../../validacion/personajes/operador-mecha-verificacion.json)
- [Geometría y texturas](../../validacion/personajes/operador-mecha-geometria.json)

## Reproducir

Los scripts están en `scripts/personajes/operador-mecha/` y reutilizan `herramientas.py` y `render_vistas.py` del Neo Investigador. Necesitan Blender 5.2.2 LTS con el complemento **VRM Add-on for Blender**, `oxipng` y un Python 3 con Pillow. El VRM exportado de VRoid Studio lo conserva Chris en su copia local (variable `OPM_VRM`).

```bash
cd 03-diseno/scripts/personajes/operador-mecha
export OPM_VRM=/ruta/a/operador-mecha.vrm
export OPM_TRABAJO="${TMPDIR:-/tmp}/operador-mecha"
B=/Applications/Blender.app/Contents/MacOS/Blender
for s in 01_importar_vrm 02_separar 03_prendas 04_equipo 05_pesos 06_prueba_pose \
         07_optimizar 08_materiales 09_exportar 10_verificar 11_vistas; do
  $B -b --python-exit-code 1 --python "$s.py" || break
done
python3 componer_vistas.py "$OPM_TRABAJO" "$OPM_TRABAJO/vistas"
python3 12_entregar.py
```

Los archivos intermedios se guardan en una carpeta temporal (variable `OPM_TRABAJO`). `12_entregar.py` copia la fuente, las exportaciones, las texturas, la validación y las vistas a `03-diseno/`. Una reconstrucción desde cero en una carpeta vacía reprodujo el modelo con los mismos triángulos por malla, huesos, materiales y expresiones.

## Descargar desde GitHub

Los `.blend`, `.fbx`, `.glb`, las imágenes, los PDF y los `.docx` van por Git LFS:

```bash
git lfs install
git clone https://github.com/moiseiseiseis/biogamevr.git
cd biogamevr
git lfs pull
```
