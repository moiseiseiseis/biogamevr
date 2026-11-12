# Entrega del Neo Investigador — School Redemption Game

1 de noviembre de 2026 · Blender 5.2.2 LTS · rama `Modelado-Diseño`

Nueva versión de El Investigador: conserva la cara, el cuerpo y el esqueleto de un avatar de VRoid Studio, viste el equipo de la lámina de Diseño y queda **listo para animar**: está riggeado y pesado, pero no trae animaciones. Las versiones anteriores ([`investigador.blend`](../../fuentes/investigador.blend) e [`investigador-ligero.blend`](../../fuentes/investigador-ligero.blend)) no se modifican.

## Abrir el modelo

- [Fuente editable](../../fuentes/neo-investigador.blend). Las colecciones son `Investigador_Ropa_y_Equipo` (prendas y accesorios) e `Investigador_Props` (la tableta). El cuerpo, la cara y el cabello de VRoid se llaman `VRoid_Cuerpo`, `VRoid_Cara` y `VRoid_Cabello`, y el esqueleto `Neo_Investigador_Rig`.
- Unidades métricas: 1 unidad = 1 m, frente hacia -Y, arriba +Z, plantas en Z = 0, pose T.
- Los materiales son MToon (VRM), los mismos que usa VRoid. Para editarlos se recomienda el complemento VRM Add-on for Blender. Sin él, el archivo se abre y se ve igual, pero las propiedades MToon no son editables.
- Las prendas tienen un modificador `Suavizado` (subdivisión 1) solo para presentación. Las exportaciones se generan sin él.

## Archivos de intercambio

| Archivo | Contenido | Triángulos |
| --- | --- | ---: |
| [neo-investigador.fbx](../../exports/neo-investigador.fbx) | Personaje con esqueleto, pesos y expresiones | 55.551 |
| [neo-investigador.glb](../../exports/neo-investigador.glb) | Lo mismo en glTF binario | 55.551 |
| [neo-investigador-tableta.fbx](../../exports/neo-investigador-tableta.fbx) | Tableta como objeto independiente, sin pesos | 2.050 |

Ejes del FBX: Forward -Z y Up Y, con escala aplicada (*FBX All*). **No se activó *Apply Transform***: el manual de Blender la marca como experimental y conocida por fallar con armaduras. Sí se usó en la tableta, que no tiene armadura. Las texturas van incrustadas.

En la copia local de Chris existe también un `.vrm` para UniVRM. No se sube porque `.gitattributes` no envía los `.vrm` por Git LFS.

## Vistas

- [Vistas en pose T](../../vistas/personajes/neo-investigador/neo-investigador-vistas.png)
- [Pose relajada](../../vistas/personajes/neo-investigador/neo-investigador-pose-relajada.png)
- [Rostro](../../vistas/personajes/neo-investigador/neo-investigador-rostro.png)
- Vistas sueltas: [frente](../../vistas/personajes/neo-investigador/neo-investigador-frente.png), [lateral](../../vistas/personajes/neo-investigador/neo-investigador-lateral.png), [posterior](../../vistas/personajes/neo-investigador/neo-investigador-posterior.png) y [tres cuartos](../../vistas/personajes/neo-investigador/neo-investigador-tres-cuartos.png)

## Bitácora y validación

- [Bitácora en Word](bitacora-neo-investigador.docx), [PDF](bitacora-neo-investigador.pdf) y [Markdown](bitacora-neo-investigador.md)
- [Reimportación de las exportaciones](../../validacion/personajes/neo-investigador-verificacion.json)
- [Geometría por malla](../../validacion/personajes/neo-investigador-geometria.json)

## Reproducir

Los scripts están en `scripts/personajes/neo-investigador/` y se ejecutan en orden, del `01` al `15`, con Blender 5.2.2 LTS y el complemento **VRM Add-on for Blender** instalado. Necesitan dos archivos:

- el VRM exportado de VRoid Studio, que Chris conserva en su copia local (variable `NEO_VRM`);
- el `investigador.blend` del que se reutilizan piezas (variable `NEO_ANTERIOR`; por omisión, `fuentes/investigador.blend`).

```bash
cd 03-diseno/scripts/personajes/neo-investigador
export NEO_VRM=/ruta/al/investigador-vroid.vrm
for s in 01_importar_vrm 02_piezas_anteriores 03_prendas 04_equipo 05_accesorios \
         06_cuerpo_y_colores 07_sombreado 08a_pesos_origen 08b_pesos_transferir \
         08c_funda_rigida 09_lente 10_base_final 11_limpieza 12_texturas \
         13_exportar 14_verificar 15_renders; do
  blender --python-exit-code 1 -b --factory-startup --python "$s.py" || break
done
```

Los archivos intermedios se guardan en una carpeta temporal (variable `NEO_TRABAJO`). `13_exportar` escribe en `fuentes/` y `exports/`, `14_verificar` en `validacion/personajes/` y `15_renders` en `vistas/personajes/neo-investigador/`.

En una ejecución desde cero, en una carpeta limpia, los pasos `01` a `14` reprodujeron el modelo con las mismas mallas, huesos y pesos. La chaqueta salió con 20 triángulos más (55.571 frente a 55.551), porque el orden en que se aplican los cortes por plano puede variar entre ejecuciones. Si en macOS `blender` no está en el PATH, usa `/Applications/Blender.app/Contents/MacOS/Blender`.

## Descargar desde GitHub

Los `.blend`, `.fbx`, `.glb`, las imágenes, los PDF y los `.docx` van por Git LFS:

```bash
git lfs install
git clone --branch Modelado-Diseño https://github.com/moiseiseiseis/biogamevr.git
cd biogamevr
git lfs pull
```
