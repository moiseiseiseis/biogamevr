# Sala de enlace (cápsula)

Por un cambio en el guion, el piloto ya no va dentro del mecha: lo maneja **a distancia** desde una sala de enlace, “como una sala de VR”. Este modelo reemplaza a la cabina en ese papel. **La cabina anterior (`submarino-cabina`) no se modificó** y sigue en el repo.

Se mantiene el estilo retrofuturista 80s que se votó, con polígonos marcados tipo Puppet Combo.

## Archivos

| En el repo | Qué es |
|---|---|
| `03-diseno/fuentes/sala-enlace.blend` | Escena completa. La textura de las cámaras va empacada dentro. Trae el texto `LEEME_Sala_Enlace` y la animación de ejemplo `Casco_Bajar` |
| `03-diseno/exports/sala-enlace.fbx` | Solo mallas y empties: 19 objetos bajo `SALA_Enlace`. Sin luces, cámaras ni animación |
| `03-diseno/exports/texturas/sala-enlace/sala-enlace_camaras.png` | Imagen provisional de la pantalla envolvente (256 × 64 px, pixel art) |

- **FBX:** metros, escala 1, −Z Forward, Y Up, con *Apply Transform*; Smoothing = Face (sombreado plano); con Custom Properties.
- **Triángulos:** ~5,300 en total. El límite de la cabina es 50k y la meta que nos pusimos para el estilo Puppet Combo era ~7k.

## Por qué una cápsula

Se propusieron tres formas de hacer la sala:

| Opción | Idea | Decisión |
|---|---|---|
| **A · Cápsula de enlace** | Un capullo cerrado alrededor del asiento, con pantalla envolvente y un casco que baja del techo | **Elegida** |
| B · Centro de control | Sala grande con tarima para el piloto y mesas de técnicos | Descartada: el doble de trabajo y se siente vacía en VR |
| C · Sala de enlace en el cuarto de la cabina | El mismo cuarto de 4.2 × 4.8 m, con monitores en lugar del ventanal | Respaldo, por si la A no convence al probarla en el visor |

Referencias: las sillas de conexión de *Matrix*, la cápsula de conducción de *Pacific Rim*, la unidad de enlace de *Avatar*, el arnés de VR de *The Lawnmower Man*, los operadores de *Surrogates* y la sala de cómputo de *Alien: Isolation*. Solo se usaron como inspiración; no hay modelos externos.

## Medidas

Frente = −Y en Blender (en Unity, +Z). 1 unidad = 1 m.

| Elemento | Medida |
|---|---|
| Cápsula | 12 caras, Ø 3.0 m. Pared de 2.35 m y domo hasta 2.88 m. El eje está en (0, −0.3), bajo el asiento |
| Jugador | `ANCLA_Jugador` en (0, −0.3, 0.49), sentado. Ojos a ~1.27 m |
| Pantalla envolvente | Franja de 270° alrededor del frente, de radio 1.42 m y de 0.95 a 2.15 m de alto. La parte de atrás queda libre para la escotilla |
| Consola | Estante plano a 0.72 m con 3 monitores CRT iguales (50 × 44 cm), inclinados 22° hacia atrás y a 1 m de los ojos. Los laterales giran 30° hacia el piloto |
| Controles | Misma posición que en la cabina aprobada. Quedan a 59–72 cm del hombro |
| Casco de enlace | En reposo a 1.95 m sobre el asiento. Baja 62 cm, hasta 1.33 m |
| Escotilla | Puerta trasera de 0.78 × 1.84 m. Origen al pie, en (0, 1.10, 0) |

## Objetos

| Objeto | Qué es |
|---|---|
| `SALA_Enlace` | Empty raíz, en el origen |
| `CAB_Capsula` | Pared, domo y costillas. Normales hacia dentro |
| `CAB_Piso` | Piso, tarima del asiento y rejilla de la escotilla |
| `CAB_Consola` | Cuerpo de la consola, estante, carcasas de los 3 CRT y los paneles laterales |
| `CAB_Mobiliario` | Biseles y montantes de la pantalla, luces de techo, rejillas, panel de señal, carretes de cinta, cable principal y soporte del casco |
| `CAB_Asiento` | Asiento del piloto |
| `ANCLA_Jugador` | Empty donde se coloca al jugador |

### Interactivos

Las piezas `INT_` **no tienen la rotación aplicada**: su Z local apunta hacia fuera del panel. Los rangos son relativos a la pose de reposo y están en las Custom Properties de cada objeto, que el FBX conserva.

| Objeto | Movimiento | Rango |
|---|---|---|
| `INT_Palanca_Potencia` | Rotación en X local | 0 a 70° (0 apagado, 35 ralentí, 70 potencia) |
| `INT_Boton_Anclaje` | Traslación en Z local | 0 a −1.2 cm |
| `INT_Interruptor_Luces` | Rotación en X local | 0 a 50° |
| `INT_Selector_Escala` | Rotación en Z local | 0 a 120°, 5 pasos de 30° |
| `INT_Tapa_Escala` | Rotación en X local | 0 a 110° |
| `INT_Boton_Escala` | Traslación en Z local | 0 a −1.2 cm (solo con la tapa abierta) |
| `INT_Casco_Enlace` | Traslación en Z | 0 a −0.62 m. **Nuevo** |
| `INT_Pantalla_Envolvente` | Solo lectura | Video de las cámaras del mecha. **Nuevo** |
| `INT_Pantalla_Principal` | Táctil | Círculo de 16 lados, radio 18 cm. Textura sugerida 1024 × 1024 |
| `INT_Pantalla_Estado` / `INT_Pantalla_Sistemas` | Solo lectura | 39 × 25 cm. Textura sugerida 1024 × 660 |
| `INT_Escotilla` | Pendiente | Falta definir con Game Design cómo se abre |

## Decisiones

- **Mismos nombres `INT_` que la cabina.** Si Desarrollo ya programó la cabina, los scripts deberían funcionar igual. Lo único nuevo es el casco y la pantalla envolvente.
- **Casco interactivo.** Al bajarlo hasta la cabeza del jugador se puede hacer un fundido y empezar el enlace con el mecha. Si no da tiempo de programarlo, puede quedarse fijo arriba como decoración.
- **Pantalla envolvente.** En lugar de ventanas, el piloto ve el video de las cámaras del mecha. Es una sola malla con UV 0–1: u va de la izquierda a la derecha del piloto y v de abajo arriba. Se sugiere una textura de 2048 × 512 (un render texture panorámico). Mientras tanto lleva `sala-enlace_camaras.png`: verde fósforo con líneas de barrido, sin filtrado (Point/Closest).
- **Pantallas derechas.** En `submarino-cabina` las tres pantallas de la consola estaban **giradas 180°**: una imagen salía de cabeza. Aquí ya están corregidas. Si alguien usa la cabina vieja, hay que rotar ahí su textura.
- **Monitores emparejados.** Los tres CRT son iguales, apoyados en el mismo estante, a la misma altura y con la misma inclinación, y cada pantalla va centrada en su marco.
- **Estilo Puppet Combo.** Pocos lados por pieza, sombreado plano y colores planos por material. La única textura es la de la pantalla.

## Paleta

La misma de la cabina retrofuturista aprobada, más un color nuevo: el **cian de enlace**, solo para lo que tiene que ver con la conexión remota (el visor del casco, los medidores de señal, el cable principal y las tiras junto a la escotilla).

| Material | Color | Uso |
|---|---|---|
| `MAT_Cab_Pared` | `#626366` grafito | Paredes y carcasas |
| `MAT_Cab_Panel` | `#45474A` | Consola y paneles |
| `MAT_Cab_Piso` | `#383839` | Piso |
| `MAT_Cab_Metal` | `#AEAEA9` metal gastado | Molduras, tornillos, brazo del casco |
| `MAT_Cab_Asiento` | `#7C4540` cuero vino | Asiento |
| `MAT_Cab_Mando` | `#DAD1BC` beige 80s | Empuñadura y perillas |
| `MAT_Cab_Amarillo` | `#EDCC38` | Franjas de peligro |
| `MAT_Cab_Tapa` | `#DA3F38` | Tapa del botón de escala |
| `MAT_Cable` | `#1C1D1E` | Cables |
| `MAT_CRT_Vidrio` | `#193027` | Vidrio apagado de los CRT |
| `MAT_CRT_Verde` · emisivo | `#7CFFA9` | Pantallas principal y de estado |
| `MAT_CRT_Ambar` · emisivo | `#FFC459` | Pantalla de sistemas |
| `MAT_Luz_Verde` / `Ambar` / `Roja` / `Blanca` · emisivos | `#59F38A` / `#FFCB3F` / `#FA5A50` / `#FFF3DA` | Pilotos y lámparas |
| `MAT_Enlace_Cian` · emisivo | `#4FD8E8` | **Nuevo:** casco y señal |
| `MAT_Pantalla_Envolvente` · emisivo | textura | Pantalla envolvente |

## En Unity

1. Importar `sala-enlace.fbx` con *Scale Factor* 1. No hace falta rotar nada.
2. En los materiales emisivos (`MAT_CRT_*`, `MAT_Luz_*`, `MAT_Enlace_Cian`, `MAT_Pantalla_Envolvente`) hay que activar **Emission** y usar **Bloom** en el post-proceso para que brillen.
3. La textura de la pantalla está en `exports/texturas/sala-enlace/`. Si Unity no la conecta sola, se asigna a mano a `MAT_Pantalla_Envolvente`, con *Filter Mode* = **Point** para que se vea pixelada.
4. La cápsula es cerrada: el jugador entra por la escotilla trasera.

## Pendiente

| Tema | Estado |
|---|---|
| Cómo se abre la escotilla | Por definir con Game Design |
| Casco interactivo | Depende de Desarrollo; si no, queda como decoración |
| Video de las cámaras en la pantalla envolvente | Por definir con Desarrollo (render texture) |
| Probarla en el visor | Si la cápsula se siente demasiado cerrada, se pasa a la opción C |
