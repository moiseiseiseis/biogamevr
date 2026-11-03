# Entrega de El Paciente y la camilla de soporte — School Redemption Game

2 de noviembre de 2026 · Blender 5.2.2 LTS · rama `Modelado-Diseño`

El Paciente se construyó a partir del [Neo Investigador](entrega-neo-investigador.md): conserva su esqueleto (111 huesos), su cara con las 58 expresiones y su cuerpo de VRoid, y viste la ropa de la lámina de Diseño. La camilla es un modelo aparte. Ninguno trae animaciones. El modelado se hizo con apoyo de Claude (IA), conforme al acuerdo del equipo.

![Vistas](../../vistas/personajes/paciente/paciente-vistas.png)

## Archivos

| Archivo | Contenido |
| --- | --- |
| [paciente.blend](../../fuentes/paciente.blend) | Personaje en pose T, 1,68 m. Colecciones `Paciente_Cuerpo`, `Paciente_Ropa`, `Paciente_Accesorios` y `Paciente_Soporte_Vital` (mascarilla, para ocultarla) |
| [camilla-soporte.blend](../../fuentes/camilla-soporte.blend) | Camilla como un solo objeto, `Camilla_Soporte` |
| [paciente-en-camilla.blend](../../fuentes/paciente-en-camilla.blend) | Escena de presentación: paciente acostado, ojos cerrados, mascarilla, calcetines y manguera del respirador |
| [paciente.fbx](../../exports/paciente.fbx) · [paciente.glb](../../exports/paciente.glb) | Personaje con esqueleto, pesos y expresiones. 77.817 triángulos, 0 vértices sin peso |
| [camilla-soporte.fbx](../../exports/camilla-soporte.fbx) · [camilla-soporte.glb](../../exports/camilla-soporte.glb) | Camilla, sin esqueleto. 13.512 triángulos, 0,93 × 2,27 × 0,85 m |

Mismos ejes y opciones de exportación que el Neo Investigador: frente hacia -Y, arriba +Z, plantas en Z = 0; FBX con Forward -Z, Up Y y *FBX All*. La camilla tiene la cabecera hacia +Y y el suelo en Z = 0. Las exportaciones van sin el suavizado de presentación.

## Qué se hizo

- **Base**: esqueleto, cara, cabello y manos del Neo Investigador, escalados de 1,80 m a 1,68 m. Los ojos se aclararon a un azul más vivo.
- **Piel reconstruida**: en el Neo Investigador la piel bajo la ropa estaba recortada. Los antebrazos, el cuello y las manos se reconstruyeron a partir de la camisa sin recortar de `investigador-vroid.blend` y de los guantes, devolviéndolos a la superficie de la piel. Conservan las UV y los pesos del cuerpo de VRoid.
- **Ropa**: camiseta clara holgada con triángulo en el pecho; chaqueta verde oliva abierta con mangas remangadas al codo, cuello alto, capucha caída con panel claro triangular y emblema en la espalda; pantalón cargo ancho con bolsillos en ambos muslos y correas con hebilla; zapatillas claras y calcetines.
- **Soporte vital**: mascarilla respiratoria con dos filtros y válvula; collarín blanco con sensor en el cuello; puerto vascular en el antebrazo derecho; pulseras en ambas muñecas.
- **Cabello rizado**: los mechones de VRoid se ondularon con ruido y se añadieron 75 rizos en espiral del mismo tono (15.075 triángulos).
- **Camilla**: pedestal con dos patas, plataforma, defensas laterales segmentadas con luces naranjas, colchón en tres tramos, almohada, barandillas tubulares en la cabecera, manguera y cable.

## Vistas

- [Rostro](../../vistas/personajes/paciente/paciente-rostro.png) y [con mascarilla](../../vistas/personajes/paciente/paciente-mascarilla.png)
- [Pose relajada](../../vistas/personajes/paciente/paciente-pose-relajada.png)
- [En la camilla](../../vistas/personajes/paciente/paciente-en-camilla-alto.png), [lateral](../../vistas/personajes/paciente/paciente-en-camilla-lado.png) y [rostro](../../vistas/personajes/paciente/paciente-en-camilla-rostro.png)
- [Camilla](../../vistas/personajes/paciente/camilla-soporte-vistas.png) y [planta](../../vistas/personajes/paciente/camilla-soporte-planta.png)

## Reproducir

Los scripts están en `scripts/personajes/paciente/` y reutilizan `herramientas.py` y `render_vistas.py` del Neo Investigador. No necesitan el complemento VRM: los materiales MToon se recolorean directamente en sus nodos.

```bash
cd 03-diseno/scripts/personajes/paciente
export PAC_VROID_BLEND=/ruta/a/investigador-vroid.blend   # entrega del 29 de octubre, copia local de Chris
B=/Applications/Blender.app/Contents/MacOS/Blender
for s in 01_base 02_ropa 02b_pelo_rizado 03_camilla 05_camilla_export 04_final; do
  $B -b --python-exit-code 1 --python "$s.py" || break
done
python3 06_entregar.py
```

## Pendientes y límites

- **Cabello**: los rizos son geometría añadida sobre el peinado corto de VRoid. Un peinado rizado hecho en VRoid Studio se vería más limpio y tendría física de cabello.
- **Materiales**: se editaron sin el complemento VRM. Si se abre con él, conviene revisar que los colores MToon se conserven.
- **Triángulos**: 77.817 en el personaje, más que el Neo Investigador (55.551). Los rizos aportan 15.075; falta medir el rendimiento en el visor.
- Faltan animaciones, la prueba en Unity y los niveles de detalle (LOD).
