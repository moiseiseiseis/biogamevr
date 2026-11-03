> **Nota de la subida al repo (02/11/2026).** Este documento usa los nombres de la carpeta de trabajo. En el repo: `sala_reduccion_nave_v4.blend` → `03-diseno/fuentes/sala-reduccion.blend`; `sala_reduccion_nave_v4.fbx` → `03-diseno/exports/sala-reduccion.fbx`; `meshy/texturas/` (las versiones finales) → `03-diseno/exports/texturas/sala-reduccion/`; `sala_reduccion_nave_v4_informe.json` → `sala-reduccion-informe.json`. Solo se sube la variante **nave v4**; las secciones de bloqueo, columna y Meshy anteriores quedan como historia del diseño. Los scripts, renders y video están en la carpeta de trabajo de Ulises y no se subieron al repo. Ver `LEEME_ZIP.md` para la tabla completa.
# Sala de Reducción: bloqueo (v1)

Un bloqueo con formas simples y colores planos, a escala real: **1 unidad = 1 m, Z arriba**. No usé créditos de Meshy. Todo sale de `scripts/sala_reduccion_bloqueo.py`, que es procedural y se puede volver a ejecutar:

```
blender -b --factory-startup --python scripts/sala_reduccion_bloqueo.py -- all      # construye, exporta, comprueba y renderiza
blender -b --factory-startup --python scripts/sala_reduccion_bloqueo.py -- build    # solo .blend, .fbx, .glb e informe
blender -b sala_reduccion/sala_reduccion_bloqueo.blend --python scripts/sala_reduccion_bloqueo.py -- render
```

Las medidas están al principio del script (`HALF`, `TECHO`, `HC`, `R_INT/R_MED/R_EXT`, `SEC`, etc.). Las cifras de este documento salen de `sala_reduccion_informe.json`, que el script vuelve a escribir en cada ejecución.

## Archivos

| Archivo | Qué es |
|---|---|
| `sala_reduccion_bloqueo.blend` | Escena completa: colecciones, nave de referencia, luces, 4 cámaras y la animación de los anillos |
| `sala_reduccion_bloqueo.fbx` | Exportado en metros, −Z Forward, Y Up. Lleva 44 objetos (mallas y empties) y las 3 tomas de los anillos. **No incluye la nave, las referencias de escala, las luces ni las cámaras** |
| `sala_reduccion_bloqueo.glb` | El mismo contenido en glTF binario, con 3 animaciones |
| `sala_reduccion_informe.json` | Medidas, escala de la nave, tris por colección y comprobaciones de colisión |
| `renders/` y la raíz de esta carpeta | 4 renders Eevee de 1600×900. Hay una copia de los 4 en ambos sitios |

## Ejes de la sala

- **−Y**: entrada desde el hangar. `Spawn_Jugador` está en (0, −14.5, 0) y mira hacia +Y.
- **+Y**: muro con la ventana de observación elevada y, detrás, la sala de control.
- **−X** (izquierda al entrar): consolas, escalera y balcón, del que sale la pasarela.
- **+X** (derecha): tanques de gas, cajas de herramientas y la puerta al **Área del paciente**.

## Medidas

| Elemento | Medida |
|---|---|
| Sala | Planta interior de 32 × 32 m, techo a 20 m, muros de 0.5 m. Pilares metálicos de 0.6 m cada 4 m, fajas a 6 m y a 19.6 m, nervaduras de techo de 0.3 m de canto, franja ámbar ASTRA a 3 m |
| Puerta del hangar (−Y) | 14 m de ancho × 10 m de alto, con marco de peligro. Las hojas están corridas detrás del muro |
| Ventana de observación (+Y) | 16 × 2.7 m. El alféizar está a 11.9 m y su bisel es ámbar |
| Sala de control | Detrás del muro +Y: 33 × 6.5 m y 4.5 m de alto, con el piso a 10.98 m, a la misma altura que el balcón |
| Plataforma de anclaje | **Circular**, Ø 14 m y 1 m de alto (de −0.7 a +0.3 m, semiempotrada). Tiene anillo de luz ámbar en el borde, 4 garras de anclaje en los ejes X/Y y una compuerta central de recogida de Ø 2.4 m |
| Giroscopio | Centro en (0, 0, **10.1**). Anillos de **Ø 18 / 16 / 14 m** (medidos en la línea media) con sección de 0.8 m. Holgura de 0.2 m entre anillos |
| Pedestal en U | Patas de 0.8 × 1.8 m con la cara interior a 9.6 m del centro, ejes de pivote de Ø 0.64 y la base de la U bajo el suelo |
| Brazos robóticos | 2 en rieles de techo (x = ±12.5 m, a 19.5 m de altura), uno con pinza y otro con láser. 2 de piso en las diagonales libres (−8.6, 7.4) y (8.6, −7.4), también con pinza y láser |
| Escalera | Un tramo junto al muro −X: 55 peldaños de 0.20 × 0.25 m y 1.4 m de ancho |
| Balcón lateral | x de −15.4 a −12.5, y de −1.2 a 16 (llega a la puerta de la sala de control). Cubierta a 10.98 m |
| Pasarela | En y = 0.84, con la cubierta a 10.98 m (la cubierta del platillo, junto a la cúpula). El tramo fijo va de x = −12.5 a −9.8 y el tramo extensible de −9.8 a −3.86 |
| Consolas | 5 en x = −11.8, de y = −11.5 a −3.5, con pantallas emisivas cian y ámbar que miran al operador |
| Panel de advertencia | 11 × 4.2 m en el muro +Y, debajo de la ventana: «PELIGRO / CAMPO DE REDUCCIÓN» |
| Tanques y cajas | 6 cilindros de Ø 1.3 m (2.4 y 3.2 m de alto) junto al muro +X y 4 cajas |
| Salida al paciente | Un tubo bajo la compuerta de la plataforma lleva a un elevador de vidrio en (8, −3). De ahí, un riel en y = −3 llega a la puerta de 2.6 × 2.6 m del muro +X, rotulada «ÁREA DEL PACIENTE». La **cápsula/vial** mide 0.3 m (Ø 0.16) y va sobre un carro |
| Figura humana | 1.70 m, en (−3, −11, 0), dentro de `Ref_Escala` |

## Nave de referencia (colección `Ref_Nave`)

- **Fuente:** append de `nave_eritro/modelo/eritro_rig.blend` (armature `Eritro_Rig` y 19 mallas, 19.859 tris) en pose **Desplegado**. No traje `Piel_Camuflaje`, que en esta pose está escondida dentro del platillo. El rig funcionó bien, así que no hizo falta el GLB.
- **Escala aplicada: 2.83 m por µm** (escala del objeto `Eritro_Rig`). Pediste 12/3.4 = 3.53, pero con esa escala la nave no cabe dentro del anillo interior al girar. 2.83 es la escala máxima que deja 0.5 m de holgura: la esfera que envuelve la nave mide 2.155 µm de radio, y (6.6 − 0.5) / 2.155 = 2.83.
- **Medidas resultantes:** 9.52 m de ancho (patas), 9.6 m de largo sin flagelo y **11.9 m con flagelo**, 3.35 m de alto.
- **Orientación:** el morro mira a +Y, hacia la ventana de control, y el flagelo a −Y, hacia la entrada.
- **Posición:** el centro de su esfera envolvente coincide con el centro del giroscopio, a 10.1 m. La nave **levita** sobre la plataforma, sostenida por el anclaje magnético. Puse un haz translúcido, `Haz_Levitacion_FX`, para que se entienda. Ver «Decisiones» más abajo.
- `Punto_Ojos_Piloto` está en (0, 0.84, 11.57), dentro de la cúpula y 0.15 m bajo su techo, y mira hacia +Y. En los materiales de la nave activé el backface culling (solo en esta copia), para poder ver hacia fuera desde la cabina.

## Holgura entre la nave y el anillo interior

- Hueco libre del anillo interior: radio de 7.0 − 0.4 = **6.6 m**.
- Vértice de la nave más alejado del centro: **6.10 m**. Holgura de **0.50 m en cualquier orientación**, así que la nave no toca aunque los tres anillos giren.
- Prueba con BVH de los anillos contra la nave en 30 cuadros (0–232, cada 8): **0 intersecciones**. Anillo contra anillo: **0**.
- La caja envolvente (9.52 × 11.9 × 3.35 m, semidiagonal de 7.8 m) no sirve como prueba, porque sus esquinas están vacías. La medida válida es la de los vértices.

## Holgura de cada elemento respecto al barrido del giroscopio (esfera de 9.4 m)

Techo (nervaduras): 0.20 · plataforma: 0.30 · pasarela fija: 0.42 · garras: 0.47 · suelo: 0.70 · brazos robóticos: de 1.0 a 1.4 · tubo de recogida: 1.4 · el resto, más de 2.4 m. La cara interior de las patas del pedestal está a 9.6 m (0.2 m). **Ningún brazo choca con los anillos al girar.**

**Excepción: `Pasarela_Extensible`.** Entra 5.5 m en el barrido, como es inevitable si tiene que llegar a la cabina. En reposo (cuadro 0) no toca ningún anillo ni la nave (0 intersecciones), pero **tiene que retraerse antes de que gire el giroscopio**. Es un objeto aparte, listo para animarlo en Unity (deslizar en −X unos 6 m).

## Animación de prueba

Cada anillo da una vuelta sobre su propio eje en los cuadros 0–240, con interpolación lineal y un modificador Cycles para que se repita. El exterior gira en X local (pivotes en la U), el medio en Y local en sentido contrario y el interior en Z local. Están anidados como un gimbal (`Giroscopio_Centro` → `Anillo_Exterior` → `Anillo_Medio` → `Anillo_Interior`) y los pivotes son hijos del anillo padre. Todo el conjunto está girado 45° en Z. En el FBX salen 3 tomas y en el GLB, 3 animaciones. En Unity activa Loop Time.

## Materiales

Planos, según la hoja conceptual: `Azul_Industrial`, `Gris_Metal`, `Gris_Oscuro`, `Naranja_Oxido`, `Amarillo_Advertencia` y `Franjas_Peligro`. Los acentos son `Ambar_ASTRA`, y las pantallas, `Pantalla_Ambar` y `Pantalla_Cian`. Hay además `Rojo_Alarma`, `Vidrio` y `Suelo_Rejilla` (una rejilla de 1 m, solo en el shader).

**Neón de los anillos:** franjas discontinuas `Neon_Magenta` (anillos exterior e interior) y `Neon_Cian` (anillo medio), en las caras interior y exterior. **Se apagan** con la propiedad de escena `neon_intensidad`: 1 = encendido y 0 = apagado (un driver controla la intensidad de emisión). Las rayas de peligro y la rejilla del suelo son procedurales, así que en el FBX llegan como color plano y en Unity habrá que usar una textura o un decal.

## Triángulos (presupuesto de Quest 3: menos de 60k sin la nave)

| Colección | Tris |
|---|---|
| Sala | 2.136 |
| Plataforma_Anclaje | 1.664 |
| Giroscopio | 5.100 |
| Brazos_Roboticos | 2.336 |
| Pasarela_Balcon | 984 |
| Consolas_Equipo | 1.896 |
| Salida_Paciente | 765 |
| **Total del bloqueo** | **14.881** (25 % del presupuesto) |
| Ref_Nave (aparte) | 19.859 |
| Ref_Escala (aparte, no se exporta) | 420 |

## Renders (Eevee, 1600×900)

1. `sala_reduccion_01_general_entrada.png`: vista general desde la entrada.
2. `sala_reduccion_02_vista_piloto.png`: lo que ve el jugador desde `Punto_Ojos_Piloto`, en el cuadro 0 con los anillos en reposo.
3. `sala_reduccion_03_planta_escala.png`: planta cenital ortográfica, sin techo, con una barra de 10 m y la rejilla de 1 m. +Y queda a la derecha.
4. `sala_reduccion_04_corte_lateral.png`: corte ortográfico en x = 0.3, mirando hacia −X, con la figura de 1.70 m y una regla de 10 m.

## Variante con COLUMNA telescópica (`sala_reduccion_columna.*`)

Sale del mismo script con `-- all --variante columna`, y el video con `-- video --variante columna`. No toca la versión flotante.

| Archivo | Qué es |
|---|---|
| `sala_reduccion_columna.blend`, `.fbx` y `.glb` | La escena con la columna y la acción **`Demo_Columna`** (cuadros 0–600, a 30 fps; una sola acción con un slot por objeto). Se exportan 57 objetos |
| `sala_reduccion_columna_informe.json` | Cifras y comprobaciones |
| `renders/columna_01_abajo_embarque.png` … `columna_05_vista_piloto_abajo.png` | 1: abajo, en el embarque · 2: arriba, en los anillos · 3: girando, con la columna recogida · 4: corte lateral con la figura de 1.70 m · 5: lo que ve el piloto estando abajo |
| `renders/columna_demo.mp4` | La secuencia completa, a 960×540 |

**Diseño**
- **Columna de 4 tramos telescópicos** (Ø 2.6 / 2.2 / 1.8 / 1.4 m, de 3.2 m cada uno) dentro de una camisa fija y un foso bajo la plataforma (hasta −3.6 m). **Recorrido: 8.84 m**: la cuna está a 0.32 m recogida, a 1.21 m bajo la nave en el embarque y a 9.16 m arriba.
- **Cuna de Ø 3.2 m.** La pedí de Ø 4.5, pero las patas traseras de la nave pasan bajo la panza a partir de r ≈ 2 m y la atravesaban. Ø 3.2 es el máximo que no toca nada. Lleva 4 cerrojos magnéticos que abrazan su borde y se abren en abanico (70°).
- **Abajo:** la nave apoya las patas sobre la plataforma (centro a 2.15 m) y la cubierta del platillo queda a 3.04 m. Una **rampa de embarque** sube desde el suelo por −X (20.8°, y = 0.84) hasta la cabina. Su tramo superior de 4.6 m se recoge 4.2 m hacia atrás antes de subir.
- **Arriba:** la nave queda en el centro de los anillos (10.1 m). Los cerrojos se abren, se enciende el haz magnético, que la sostiene, y **la columna baja del todo antes del giro**. Elegí esta opción porque una columna levantada atravesaría la esfera de giro (9.4 m). Recogida, la cuna queda a 0.37 m del barrido.
- **Anillos en reposo HORIZONTALES** (los 3 coplanares a 10.1 m). Con el reposo vertical de la versión flotante, la parte de abajo de los anillos y un pivote quedan justo sobre el eje central y bloquean la columna. En horizontal, la nave sube por el hueco del anillo interior (r libre 6.6 m; la nave mide 6.1 m de radio).
- La pasarela alta queda recogida y no se usa: el embarque es abajo. Las garras de la plataforma giran 45° para dejar paso a la rampa, y la compuerta de recogida se desplaza a (3, −2.6) porque el centro lo ocupa la columna.

**Secuencia `Demo_Columna`** (cuadros): 0–30 embarque · 30–50 la rampa se retrae · 50–130 la columna sube con la nave · 130–145 se abren los cerrojos y se enciende el haz · 145–205 la columna baja · 210–450 los anillos giran (una vuelta cada uno) · 455–515 la columna sube · 515–530 se cierran los cerrojos y se apaga el haz · 530–590 la columna baja con la nave · 590–600 vuelve la rampa. El último cuadro es igual al primero, así que funciona en loop.

**Comprobación (BVH, cuadros 0–600 cada 5, 121 en total):** 0 intersecciones entre anillos y nave, anillos y columna, anillos y rampa, anillos y pasarela alta, nave y columna, nave y rampa, nave y plataforma (garras incluidas) y nave y pasarela alta. Durante el giro, la nave mantiene 0.50 m de holgura con el anillo interior.

**Triángulos:** columna +1.420, rampa +264, plataforma +156 (brocal y compuerta nueva). **Total del bloqueo: 16.721** (28 % del presupuesto).

**Puntos:** `Soporte_Nave` es un empty animado. En Unity, pon la nave como hija de este empty. `Punto_Ojos_Piloto` también es hijo suyo y viaja con la nave.

**Pendiente:** la cuna queda unos 0.2 m bajo la panza plana, porque se ajustó a la parte más baja en r < 1.85 m. Se puede pegar más si se recorta su radio a 1.5 m.

## Variante con modelos de Meshy, lista para Quest (`sala_reduccion_columna_meshy.*`)

Se genera con `blender -b --factory-startup --python scripts/sala_reduccion_meshy.py`. El script parte de `sala_reduccion_columna.blend`, coloca los modelos ya descargados en `meshy/` (no vuelve a gastar créditos), aplica texturas y atlas, comprueba que nada choque en `Demo_Columna`, exporta y renderiza. Antes de cada pasada hay un respaldo: `sala_reduccion_columna_meshy_respaldo_v1…v6.blend`.

**Modelos de Meshy en la escena** (copias de una sola malla donde se repiten):

| Modelo | Copias | Tris c/u | Nota |
|---|---|---|---|
| Consola de control | 1 (sustituye la 3.ª del bloqueo) | 1.770 | + 3 planos de pantalla |
| Cabezal de pinza de 3 dedos | 4 | 2.534 (piso) / **1.200 LOD (techo)** | |
| Tanque de gas | 9 (4 de 2.5–3 m) | **999 (LOD)** | |
| Cápsula sobre carro | 1 | 2.419 | vidrio separado, azul industrial, 2 anillos ámbar |
| Garra de anclaje | 4 | 974 | |
| Caja de herramientas | 4 | 761 | |
| Reflector de techo | 8 | 415 | cada uno con un spot (solo Blender) |
| Bandeja de cables | 3 | 269 | |
| Baliza de alarma | 4 | 255 | el domo es un objeto hijo animado |

**Optimización para Quest**
- **Triángulos:** 54.336 → **46.988** (78 % del presupuesto de 60k, sin la nave). La reducción viene de los tanques (de 1.519 a 999) y de los 2 cabezales del techo (de 2.534 a 1.200). No se volvió a hornear la normal de los LOD; de lejos no se nota.
- **Atlas Meshy (2048):** las texturas de los 9 modelos están en un solo material, `Atlas_Meshy`, con 4 mapas en `meshy/texturas/`:
  - `atlas_meshy_color.jpg`;
  - `atlas_meshy_orm.jpg` (R = 1, G = rugosidad, B = metálico, compatible con glTF);
  - `atlas_meshy_normal.png`;
  - `atlas_meshy_emision.jpg`.
  - Las baldosas tienen 8 px de margen. Consola, cápsula y cabezal usan 1024 cada uno; los otros 6 modelos, unos 341×512.
- **Atlas de pantallas (1024):** las 4 imágenes están en `atlas_pantallas.jpg`, con un solo material, `Pantallas_Atlas`.
- **Texturas repetibles:** muro, piso y franjas siguen aparte con Repeat. El color y la rugosidad están en JPG 1024 y las normales en PNG 512.
- **Materiales:** de 37 a **26** en la escena (21 en el GLB).
- **Peso:** el FBX baja de 52 a **11,5 MB** y el GLB de 64 a **14,7 MB**. Las versiones anteriores se guardaron como `*_v5.fbx/.glb`.

**Unity (Quest 3)**
1. Importa el FBX o el GLB con Scale Factor 1 (las medidas están en metros).
2. El material `Atlas_Meshy` va en URP/Lit:
   - Base Map: `atlas_meshy_color`.
   - Normal: `atlas_meshy_normal`.
   - Emission: `atlas_meshy_emision`.
   - Rugosidad y metálico: el ORM está en el formato de glTF (G y B). Para URP crea un mapa Metallic/Smoothness: R = metálico, A = 1 − rugosidad. Si usas glTFast con el GLB, no hace falta.
3. **Animación:** el GLB trae una sola animación, `Demo_Columna` (cuadros 0–600, a 30 fps), que incluye el giro de los 4 domos de baliza. El FBX trae una toma por objeto. Activa Loop.
4. **Balizas:** el giro del domo (`Baliza_Alarma_0X_Domo`) viene en la animación. La **emisión no viaja** en el FBX ni en el GLB, así que se enciende con un script mientras giran los anillos (cuadros 210–450 de la demo):

```csharp
// En cada baliza: arrastra el Renderer del domo. Llama SetAlarma(true/false) al iniciar y terminar el giro.
public class BalizaAlarma : MonoBehaviour {
    public Renderer domo; public Color color = new Color(1f, 0.04f, 0f); public float intensidad = 2f;
    public Light luz;   // opcional: luz puntual roja
    public void SetAlarma(bool on) {
        var m = domo.material; m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", on ? color * intensidad : Color.black);
        if (luz) luz.enabled = on;
    }
}
```

5. **Luces:** no se exportan. En Blender hay 6 luces de área, 8 spots de reflector y 4 luces de baliza; en Unity conviene poner pocas luces en tiempo real y hornear el resto.

## Brazo de captura y vial (v7)

El brazo de piso que tenía el láser se cambió por un **brazo articulado**, `Brazo_Captura`. Su base está en (6.0, −2.2), en el borde de la plataforma, entre la cuna de la columna y la cápsula. Recoge el **vial** con la nave reducida y lo deposita en la cápsula. A 3 µm la nave no se ve, así que la garra toma un vial de 0.25 m (vidrio, tapas metálicas y un punto ámbar dentro).

**Jerarquía** (objetos padre-hijo, sin armature; cada pivote está en su articulación y gira en X local, salvo la base, que gira en Z):

```
Brazo_Captura_Pedestal (fijo)
Brazo_Captura_Base      rot Z  (giratoria)
 └ _Hombro              rot X  (brazo, 3.6 m)
    └ _Codo             rot X  (antebrazo, 3.4 m)
       └ _Muneca        rot X
          └ _Cabezal    (cabezal Meshy de 3 dedos, en el atlas)
             └ _Dedo_1..3  rot X = bisagra (abre/cierra)
```

**Secuencia completa v8** (0–1190 cuadros, 30 fps = **39,7 s**). Los marcadores de la línea de tiempo de Blender llevan estos mismos nombres.

| Cuadros | Qué pasa |
|---|---|
| 0–30 | Embarque abajo; la rampa está puesta |
| 30–50 | La rampa se retira |
| 50–130 | La columna sube la nave hasta el centro de los anillos (10.1 m) |
| 130–145 | Se abren los cerrojos y se enciende el haz, que sostiene la nave |
| 145–205 | La columna baja al foso |
| 210–450 | Los anillos giran (balizas encendidas) y **la nave se reduce poco a poco (220–440)**, escalada en el centro de los anillos, hasta unos 6 cm. Desde el 400 aparece un **punto ámbar** (`Punto_Nave_Reducida`) que queda flotando en el haz. Al 450 los anillos vuelven al reposo horizontal |
| 455–495 | La columna sube del foso a la altura de trabajo |
| 450–680 | El brazo toma el **vial de la mesa** (`Mesa_Vial`, en (8.6, 1.0)), lo lleva y lo deja en la cuna de la columna. **505: el vial pasa a la pinza**; **605: el vial pasa a la cuna** |
| 700–780 | La columna sube con el vial hasta debajo del punto |
| 780–785 | El haz se apaga |
| 785–805 | La nave reducida y el punto **caen dentro del vial**; **805: nave/punto, padre = vial** |
| 810–870 | La columna baja con el vial |
| 880–1120 | El brazo toma el vial de la cuna (**935: padre = pinza**), lo deposita en la cápsula (**1055: padre = cápsula**) y vuelve al reposo |
| 1120–1180 | La cápsula avanza por el riel hasta la puerta del Área del paciente |
| 1180–1190 | La rampa vuelve a su sitio |

**Reducción por escala:** ahora viene animada en Blender, en la escala de `Soporte_Nave` (1 → 0.006, cuadros 220–440). La nave no va en el FBX/GLB de la sala, así que en Unity se rehace igual: pon la nave como hija de `Soporte_Nave` y anima su `localScale` entre los marcadores «Reduccion_inicio» y «Reduccion_fin», o usa la escala ya animada de `Soporte_Nave`.

**Cambios de padre** (en Blender las posiciones están horneadas; en Unity se recomiendan eventos de animación):

| Cuadro | Objeto | Nuevo padre |
|---|---|---|
| 505 | Vial | Punta de la pinza |
| 605 | Vial | Cuna de la columna (`Columna_Cuna`) |
| 805 | Nave reducida y `Punto_Nave_Reducida` | Vial |
| 935 | Vial | Punta de la pinza |
| 1055 | Vial | Interior de la cápsula |

En Blender, el vial se movió por posición horneada para que funcione igual en FBX y GLB. **En Unity**, lo más limpio es cambiar su padre con eventos de animación:

```csharp
// Eventos en el clip: 505 y 935 -> APinza(); 605 -> ACuna(); 805 -> NaveAlVial(); 1055 -> ACapsula()
public class CapturaVial : MonoBehaviour {
    public Transform vial, puntaPinza, cuna, interiorCapsula, nave, punto;
    public void APinza()     { vial.SetParent(puntaPinza, true); }
    public void ACuna()      { vial.SetParent(cuna, true); }
    public void NaveAlVial() { nave.SetParent(vial, true); punto.SetParent(vial, true); }
    public void ACapsula()   { vial.SetParent(interiorCapsula, true); }
}
```
**Animation Rigging (opcional):** si quieres que el brazo siga el vial donde esté, en vez de usar la toma horneada, añade un *Two Bone IK Constraint* con Root = `_Hombro`, Mid = `_Codo`, Tip = `_Muneca` y un objetivo en el vial. La base se orienta con un *Multi-Aim Constraint* (solo eje Y). Los dedos se animan aparte.

**Comprobación** (cuadros 0–1190, cada 5): el brazo no toca los anillos, la nave, la columna, la cápsula, la mesa ni el resto del equipo. La columna no toca los anillos en ningún momento (sube cuando ya están en reposo horizontal). La nave no toca la columna antes de caer, y la mesa no toca nada. Durante el giro, el brazo queda a 0.62 m del barrido.

**Triángulos:** 47.302 en la escena (sin la nave).

## Variante con la NAVE v4 (Eritro v4 + cabina retro 80s, `sala_reduccion_nave_v4.*`)

Es un archivo nuevo: no se tocó `sala_reduccion_columna_meshy.*`. Se genera así, sin créditos:

```
blender -b --factory-startup --python scripts/sala_reduccion_bloqueo.py -- build --variante columna --nave v4
blender -b --factory-startup --python scripts/sala_reduccion_meshy.py -- --nave v4
```

- **Nave:** append de `nave_eritro/cabina/eritro_con_cabina_v2.blend` (`NAVE_Hangar` + `Eritro_Rig` en pose Desplegado + `CABINA_Interior`, sin la piel ni los exploradores). La escala es la del hangar, **8 m/µm**, con el mismo offset de cabina. La nave abierta mide **26,9 × 34,7 × 9,7 m** (con el flagelo) y su radio envolvente es de **17,75 m**. `Punto_Ojos_Piloto` está en los ojos del piloto de la cabina nueva y mira hacia +Y (el morro).
- **Cómo se amplió la sala:** se construye con la misma lógica a escala 8/K y luego todo cuelga de un empty, **`Escala_Sala`, con escala K = 2,7653**. K sale de que la nave quepa girando con 0,5 m de holgura: (8 × R + 0,5) / 6,6.
  - **Escalan ×K:** sala, giroscopio, pedestal, plataforma, columna, haz, rampa, balcón y pasarela, puertas, rieles, brazos y la estación de captura (brazo, mesa, vial y cápsula).
  - **Tamaño real** (escala 1/K dentro de la raíz, colocados en las posiciones ampliadas): consolas, tanques, cajas, bandejas, balizas, reflectores y la figura de 1,70 m.
  - Las luces se multiplicaron por K².

| Medida | Antes | Nave v4 |
|---|---|---|
| Planta interior | 32 × 32 m | **88,5 × 88,5 m** |
| Techo | 20 m | **55,3 m** |
| Centro de los anillos | 10,1 m | **27,9 m** |
| Anillos (línea media) | Ø 18 / 16 / 14 m | **Ø 49,8 / 44,2 / 38,7 m**, sección de 2,2 m |
| Radio libre del anillo interior | 6,6 m | **18,25 m** (la nave mide 17,75: **0,50 m de holgura** girando) |
| Plataforma | Ø 14 m | **Ø 38,7 m** |
| Recorrido de la columna | 8,8 m | **24,5 m** |

- **Garras de anclaje:** la pata delantera de la v4 chocaba con una garra, así que el script busca automáticamente un giro del conjunto. Quedan a **24°/114°/204°/294°**, sin tocar las patas, la rampa ni la base del brazo de captura.
- **Secuencia:** la misma v8 (0–1190, 39,7 s), con todas las posiciones ampliadas.
- **Comprobación** (cada 5 cuadros): 0 choques entre anillos, nave, columna, garras, brazo de captura, mesa, cápsula, props y estructura.
- **Triángulos:** 47.414 sin la nave. La nave v4 con cabina suma unos 22k aparte.
- **Unity:** importa la raíz tal cual (Scale Factor 1). `Escala_Sala` trae la escala K, y los props a tamaño real ya llevan 1/K. La nave va aparte, a escala ×8 (como en el hangar), como hija de `Soporte_Nave`.
- **Estación de captura a tamaño humano (v4b):** la mesa mide 1,0 × 0,7 × 0,9 m, el vial 0,25 m (Ø 0,1) y la cápsula con su carro 1,35 m, sobre un **riel nuevo a su medida** (trocha de 0,5 m). El **brazo sigue siendo grande** (×K, para alcanzar la cuna a 19 m y la cápsula), pero su **pinza de 3 dedos es pequeña**, del mismo tamaño que en la sala anterior (1,24 m), proporcional al vial. En la cuna de la columna hay un **soporte pequeño centrado** (`Cuna_Soporte_Vial`, Ø 0,4 m) donde se apoya el vial. La nave se reduce hasta unos 6 cm para caber en él, y el punto ámbar mide 8 cm. Las posiciones se crean ya a 1/K, así que la secuencia y las comprobaciones son las mismas: 0 choques.
- **Tamaños comparados con la sala anterior** (caja envolvente en metros):
  - **Ningún prop quedó más pequeño.** Consola, pantallas, tanques, cajas, bandejas, balizas, reflectores, figura de 1,70 m, mesa, vial, cápsula y pinza del brazo de captura miden exactamente lo mismo.
  - Lo estructural y lo que sirve a la nave grande creció ×2,77: garras de anclaje, cerrojos y cuna de la columna, haz, pantalla y panel del muro, letreros, puertas, escalera, balcón y cabezales de los brazos grandes.
  - Error corregido: en las consolas del bloqueo, el botón rojo de alarma quedaba flotando a 3,2 m.
- **Elementos para personas, a tamaño humano (v4b):** el script base tiene un factor `HUM = 1/K` y la función `hum()`. Sin `--nave v4`, todo queda como siempre.

  | Elemento | Ampliado ×K (antes) | Ahora |
  |---|---|---|
  | Escalera al balcón | 55 peldaños de 0,55 m de alto | **Elevador de personas** (torre junto al muro −X, cabina de 2 × 2 m con barandal de 1,05 m y rellano al balcón; recorrido de 27,9 m). Con escalones de 0,18 m harían falta 155 |
  | Barandal del balcón | 1,52 m, postes cada 5,5 m | **1,05 m** con travesaño medio, postes cada 1,5 m |
  | Barandales de las pasarelas | 1,38–1,52 m | **1,05 m** |
  | Barandales de la rampa de embarque | 1,52 m, postes cada 3,3 m | **1,05 m**, postes cada 1,5 m (la rampa sigue siendo ancha) |
  | Puerta del Área del paciente | vano de 7,2 × 7,2 m | **2,0 × 2,5 m**, con un marco de peligro de 0,15 m (deja pasar la cápsula de 1,35 m sobre su riel) |
  | Letrero «ÁREA DEL PACIENTE» | letras de 1,4 m, placa de 12,7 m | letras de **0,3 m**, placa de **3,0 × 0,5 m** sobre la puerta |
  | Puerta de la sala de control (balcón) | 6,1 × 7,2 m | **1,2 × 2,3 m** |
  | Portón del hangar | 38,7 × 27,7 m | igual (es para la nave) |

  Renders: `sala_nv4_elevador_balcon`, `sala_nv4_elevador_arriba` y `sala_nv4_puerta_paciente` (`_nv4b`), con figuras de 1,70 m.
- **Relleno de la sala (v4c, colección `Relleno_Sala`):** todo va a tamaño real y cada pieza se prueba contra la estructura, los props y lo ya agregado. Si choca, no se pone (no se rechazó ninguna). Tampoco toca nada que se mueva en la secuencia: lo comprobé cada 10 cuadros.

  | Qué | Dónde |
  |---|---|
  | 6 consolas (LOD de 700 tris) con sus pantallas | 4 bajo el balcón, mirando a la sala; 2 junto al muro de entrada, a la derecha |
  | 10 tanques (LOD de 300 tris, de 1,8 a 3 m) | Banco de 6 contra el muro +Y, con colector y bajadas de tubería; banco de 4 al fondo, bajo el balcón |
  | 8 cajas (LOD de 250 tris), algunas apiladas | Junto a los tanques, en la esquina de entrada derecha y bajo el balcón |
  | 4 bandejas de cables | De las estaciones nuevas hacia la plataforma |
  | 3 racks / estanterías | Muro de entrada y fondo bajo el balcón |
  | Barandal de seguridad de 1,05 m | Alrededor de la plataforma, a 1 m del borde. Tiene huecos en la rampa y en la zona del brazo de captura, la mesa y la cápsula |
  | Pasillos con franjas en el piso | De la entrada a la plataforma y frente a las estaciones |
  | 2 monitores de pared (4 × 2,25 m) | Con las imágenes del atlas de pantallas, en los muros −X y +X |

  - **Brazo de captura más delgado:** la sección de base, brazo, antebrazo y muñeca bajó al 55 %, con el mismo largo y alcance. La pinza pequeña queda igual y la secuencia funciona sin choques.
  - **Triángulos:** **59.786** sin la nave, justo por debajo de 60k. El relleno suma 11.568.
  - **Sin agregar:** reflectores y balizas extra (no cabían en el presupuesto, y a 55 m de altura casi no se ven).
  - **Error corregido:** al copiar modelos importados de glTF, el giro no se aplicaba porque usan cuaterniones, así que las consolas quedaban de espaldas. Ahora las copias se pasan a Euler.
  - Renders `_nv4c`: `sala_nv4_zona_estaciones`, `sala_nv4_zona_tanques`, `sala_nv4_zona_entrada`, `sala_nv4_brazo_delgado`, más la vista general, la cabina y el resto.
- **Renders** (`renders/sala_nv4_*_nv4.png` y `_nv4b.png`): general en el embarque, general durante la reducción, vista desde la cabina nueva (embarque y con los anillos), embarque a la altura de una persona y corte lateral con la figura de 1,70 m. Video: `renders/secuencia_nave_v4.mp4`.

## Decisiones y problemas

1. **La nave levita.** Con anillos de Ø 18 m girando en los tres ejes, la esfera que barren mide 9.4 m de radio. Una plataforma de Ø 14 m junto a la nave siempre quedaría dentro de esa esfera, así que no es posible apoyar la nave sobre la plataforma *y* además centrarla en los anillos. Con un techo de 20 m, el centro tiene que estar a 10.1 m. La plataforma queda abajo, como emisor de anclaje magnético. Si quieres que la nave se apoye en la plataforma, hay dos opciones: que la plataforma suba sobre una columna telescópica y baje antes de girar, o que los anillos solo giren parcialmente.
2. **Escala de la nave:** 2.83 en lugar de 3.53, por la holgura (ver arriba). Si prefieres la nave más grande, tienes que agrandar los anillos o el techo: con 3.53, el anillo interior necesita unos 16.6 m de Ø libre.
3. **El conjunto del giroscopio está girado 45°.** Así el eje de los pivotes y los anillos verticales en reposo no cruzan la pasarela, que entra por −X.
4. **La pasarela extensible** tiene que retraerse antes de girar (ver arriba).
5. Las holguras del techo, de la plataforma y del pedestal son de 0.2–0.3 m: bastan para el bloqueo, pero son justas para la versión final.
6. En el render del piloto, la cúpula de la nave es una pieza maciza. La cámara está dentro, con backface culling, y por eso abajo se ve el casco del platillo.
