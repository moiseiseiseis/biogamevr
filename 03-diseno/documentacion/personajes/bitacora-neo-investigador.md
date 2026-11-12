# Bitácora del Neo Investigador

School Redemption Game

Trabajo registrado el 31 de octubre y el 1 de noviembre de 2026

El Neo Investigador es una nueva versión de El Investigador. Conserva la cara, el cuerpo y el esqueleto de un avatar creado por Chris Rojas en VRoid Studio, y le añade la vestimenta y el equipo de la lámina de Diseño. A diferencia de las entregas del 29 y 30 de octubre, el modelo queda **listo para animar**: todas las piezas tienen pesos sobre el esqueleto. No incluye animaciones.

## Alcance del trabajo

Se importó el avatar de VRoid Studio en formato VRM. Sobre la malla del cuerpo se construyeron prendas nuevas y se añadió el equipo del personaje. Algunas piezas se reutilizaron de `investigador.blend` y se ajustaron a la nueva cabeza y al nuevo cuerpo. Después se revisó la deformación con poses de prueba y se optimizó el archivo sin cambiar su aspecto.

El modelado se realizó con apoyo de Claude (IA), conforme al acuerdo del equipo: el modelado puede hacerse con IA, y solo los bocetos y diseños se hacen a mano.

## Base del personaje

La cara, el cabello, el cuerpo, las 58 expresiones faciales y el esqueleto humanoide proceden del avatar de VRoid Studio. Para acercarlo a la lámina, el cabello y las cejas se llevaron a un negro azulado, y el iris a un gris azulado. El cambio se hizo directamente sobre las texturas.

Se retiraron la ropa original de VRoid, sus gafas redondas y la piel que quedaba oculta bajo las prendas nuevas. Así la piel no atraviesa la ropa al moverse. Se conserva un anillo de piel en cada borde para que no se abran huecos.

## Prendas

Las prendas se generaron a partir de la propia malla del cuerpo, desplazada hacia fuera. Por eso se ajustan a las proporciones del avatar y heredan sus pesos. Los bordes se cortaron con planos para que los dobladillos, las mangas y los límites de color quedaran rectos.

- **Camisa negra de cuello alto**, bajo la chaqueta.
- **Chaqueta gris abierta**, con hombros y codos grafito, vivos amarillos en la abertura, cuello alto con interior amarillo y puños remangados. Cae recta desde el pecho, sin ceñirse a la cintura.
- **Pantalón cargo azul**, con bolsillo cargo en el muslo izquierdo.
- **Guantes sin dedos**, con palma reforzada y muñequera grafito.
- **Botas de dos tonos**, con puntera, talón y suela grafito, y lengüeta y borde superior amarillos.

Entre capas se fijó una separación mínima: camisa a 3,5 mm de la piel, chaqueta a 16 mm de la piel y a 9 mm de la camisa, pantalón a 11 mm. El bajo de la chaqueta se separa del pantalón de forma radial para evitar pliegues invertidos.

## Equipo y piezas reutilizadas

Se modelaron el cinturón con hebilla, la funda del muslo derecho con dos correas y los tirantes de la mochila. Los tirantes se proyectan sobre la chaqueta y el cuello.

De `investigador.blend` se reutilizaron la mochila con emblema, la credencial, el parche del brazo, las gafas rectangulares y la tableta. Las gafas se escalaron y centraron en los ojos del avatar. La tableta se entrega como objeto independiente, sin pesos, para que Desarrollo la sujete a la mano en Unity.

## Rig y pesos

El esqueleto es el humanoide de VRoid, con 111 huesos y 13 cadenas de resorte para el cabello. Se retiraron 25 huesos sin uso: 20 de la falda del abrigo original y 5 de sus gafas. Antes de borrarlos se comprobó que ninguna malla tenía pesos sobre ellos. También se quitaron las 6 cadenas de resorte del abrigo.

Las prendas derivadas del cuerpo conservan sus pesos. Las piezas generadas aparte reciben los pesos de la superficie más cercana del cuerpo, suavizados y limitados a cuatro influencias por vértice. Las piezas rígidas, como la mochila, la hebilla, la credencial, las gafas, el parche y la funda, siguen a un solo hueso.

La deformación se revisó con brazos bajados, codos y rodilla flexionados y torso y cabeza girados. Todas las piezas siguieron al esqueleto.

![Prueba de pose](../../vistas/personajes/neo-investigador/neo-investigador-pose-relajada.png)

*Figura 1. Pose relajada para revisar la deformación de las prendas y el equipo.*

## Optimización sin pérdida

- Las 21 texturas empaquetadas se recomprimieron sin pérdida. Pasaron de 6,04 MB a 3,26 MB y se comprobó que sus píxeles son idénticos.
- Se retiró la geometría que nunca se ve: la camisa bajo las mangas, la espalda y la cintura, y el pantalón dentro de las botas. La exportación bajó de 57.481 a 55.551 triángulos.
- Se purgaron los datos sin uso y el archivo se guardó con compresión.

El `.blend` pasó de 9,04 MB a 6,18 MB. El FBX bajó de 10,6 MB a 7,7 MB y el GLB de 9,4 MB a 7,6 MB.

## Medidas y comprobaciones

| Elemento | Valor |
| --- | ---: |
| Altura total | 1,80 m |
| Triángulos de la exportación | 55.551 |
| Triángulos en el `.blend` con suavizado de presentación | 129.753 |
| Triángulos de la tableta | 2.050 |
| Mallas del personaje | 26 |
| Huesos | 111 |
| Expresiones faciales | 58 |
| Vértices sin peso | 0 |

Los archivos FBX, GLB y VRM se reimportaron en Blender. Los tres coinciden en mallas, huesos y triángulos, no tienen vértices sin peso y apoyan los pies en Z = 0.

![Vistas](../../vistas/personajes/neo-investigador/neo-investigador-vistas.png)

*Figura 2. Vistas frontal, lateral, posterior y de tres cuartos en pose T.*

![Rostro](../../vistas/personajes/neo-investigador/neo-investigador-rostro.png)

*Figura 3. Rostro con gafas rectangulares, cuello alto y credencial.*

## Pendientes y límites

- Con 55.551 triángulos, el modelo puede resultar pesado para un visor standalone. El cabello de VRoid aporta unos 13.400 triángulos. Falta medir el rendimiento en el visor.
- Bajo los ojos se ve una sombra que viene de la cara de VRoid; su causa exacta no se ha confirmado.
- Las botas son más suaves y tienen menos suela que las de la lámina. Las mangas muestran algunas irregularidades en el sombreado toon.
- No se ha probado en Unity. Faltan animaciones, la sujeción de la tableta a la mano y los niveles de detalle (LOD).
