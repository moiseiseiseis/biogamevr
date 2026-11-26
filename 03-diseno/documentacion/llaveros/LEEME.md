# Llaveros de School Redemption Game

Cuatro llaveros con volumen, tipo juguete, en homenaje a modelos del juego. Están preparados para la **Bambu Lab A1 mini** (filamento, boquilla de 0,4 mm, volumen de 180 × 180 × 180 mm) del estudio de impresión.

![Los cuatro llaveros](../../vistas/llaveros/llaveros.png)

| Llavero | Archivo | Ancho × fondo × alto | Basado en |
|---|---|---|---|
| Operadora del Mecha | [llavero-operador-mecha.stl](../../exports/llaveros/llavero-operador-mecha.stl) | 21,7 × 13,9 × 68,7 mm | `fuentes/operador-mecha.blend` |
| Seraphim Mecha | [llavero-seraphim-mecha.stl](../../exports/llaveros/llavero-seraphim-mecha.stl) | 45,2 × 41,6 × 44,0 mm | `fuentes/seraphim-mecha.blend` |
| ASTRA | [llavero-astra.stl](../../exports/llaveros/llavero-astra.stl) | 37,0 × 32,7 × 42,0 mm | `fuentes/astra-ia-de-viaje.blend` |
| ARNm AUG–GGC–UUU–UAG | [llavero-arnm-aug-uag.stl](../../exports/llaveros/llavero-arnm-aug-uag.stl) | 19,2 × 9,2 × 74,1 mm | `fuentes/arnm_corto_fase_a.blend` |

Las cuatro piezas están juntas en [`fuentes/llaveros.blend`](../../fuentes/llaveros.blend). Los modelos del juego no se modificaron.

## Qué tiene cada uno

- **Operadora del Mecha:** versión chibi (cabeza al 130 %), de pie con los brazos abajo, con diadema y micrófono, arnés, cinturón, bolsillos y botas. El panel holográfico no se incluye.
- **Seraphim Mecha:** cuelga de la punta, con el lomo al frente. Las alas se plegaron 46° hacia atrás y se engrosaron, y se quitaron los cables de la espalda, que se romperían.
- **ASTRA:** la esfera con su lente de anillos y las placas de la carcasa. Se quitó el brazo de soporte, que sería frágil.
- **ARNm de la fase A:** esqueleto vertical de 5′ (arriba) a 3′ (abajo) y una base por nucleótido con su letra en relieve por delante y por detrás: AUG (metionina, inicio), GGC (glicina), UUU (fenilalanina) y UAG (paro). El collar de inicio lleva un triángulo y el de paro un octágono.

## Medidas para imprimir

- Unidades: **milímetros**. Al arrastrar el STL a Bambu Studio debe aparecer con las medidas de la tabla; si pregunta si convierte unidades, acepta y revisa con la herramienta Scale (S).
- Argolla arriba al centro: orificio de **Ø 3,8 mm** y aro de **2,6 mm** de grosor, para una argolla o cadena de llavero.
- Grosor: todas las piezas se reconstruyeron con un engrosado mínimo, así que casi ninguna pared baja de 1,1 mm (ver los límites abajo). La boquilla de 0,4 mm necesita al menos 0,8 mm.
- Cada STL es un solo sólido cerrado, sin aristas abiertas ni repetidas, apoyado en Z = 0. Comprobación en [`validacion/llaveros/llaveros-verificacion.json`](../../validacion/llaveros/llaveros-verificacion.json).

## Recomendaciones para Bambu Studio

- Impresora: Bambu Lab A1 mini, boquilla de 0,4 mm.
- Imprimir **de pie**, tal como vienen (la argolla arriba).
- **Soportes de árbol** (*tree*) activados: las figuras tienen voladizos (brazos, alas, bases del ARNm).
- Altura de capa de **0,12 mm** para que se vean los detalles; con 0,2 mm sale más rápido pero más liso.
- Un borde (*brim*) ayuda a la Operadora, que apoya solo en las botas.

## Límites

- Se imprimen en un solo color. Los ojos y la cara de la Operadora vienen de la textura del juego, así que en la pieza no se ven; se pueden pintar a mano o imprimir en varios colores con el AMS lite.
- Las alas del Seraphim son la parte más frágil.
- El apóstrofo del «3′» del ARNm mide unos 0,35 mm y puede no salir. Hay un punto de 0,48 mm en la Operadora (micrófono) y uno de 0,68 mm en el Seraphim.
- No se ha hecho una impresión de prueba.

## Reproducir

Los scripts están en [`scripts/llaveros/`](../../scripts/llaveros/). Necesitan Blender 5.2.2 LTS; para la Operadora, también el complemento **VRM Add-on for Blender**. Las medidas están en `medidas.py`.

```bash
cd 03-diseno/scripts/llaveros
export LLAV_TRABAJO="${TMPDIR:-/tmp}/llaveros"
B=/Applications/Blender.app/Contents/MacOS/Blender
for m in operador-mecha seraphim-mecha astra; do
  $B -b --python-exit-code 1 --python 01_figura.py -- $m || break
done
$B -b --python-exit-code 1 --python 02_arnm.py
$B -b --python-exit-code 1 --python 03_revisar.py
```

`01_figura.py` coloca cada modelo, lo escala en milímetros, le pone la argolla y lo reconstruye como un sólido único: reparte puntos por toda la superficie y los convierte en un campo de distancia con un radio mínimo, así las capas de ropa se funden y las piezas finas se engrosan. `02_arnm.py` construye el ARNm (la fuente tipográfica se elige con `LLAV_FUENTE`; por omisión, Arial Black de macOS). `03_revisar.py` comprueba las mallas, exporta los STL y renderiza las vistas.
