# Bitácora de El Paciente

School Redemption Game

Trabajo registrado el 2 de noviembre de 2026

El Paciente es un humano modificado genéticamente para resistir y metabolizar el modelo molecular, y es la clave del proyecto. Su modelo parte del Neo Investigador: conserva el esqueleto, la cara y las expresiones del avatar de VRoid Studio creado por Chris Rojas, y viste la ropa y el equipo de soporte vital de la lámina de Diseño. Junto con el personaje se modeló la camilla de soporte vital como un modelo independiente. El personaje queda **listo para animar**: todas las piezas tienen pesos sobre el esqueleto. No incluye animaciones.

## Alcance del trabajo

Se partió del archivo del Neo Investigador. Se retiraron su ropa y su equipo, se reconstruyó la piel que quedaba oculta bajo las prendas y se vistió al personaje según la lámina de El Paciente. Después se le dio un cabello rizado, se escaló a la altura de la lámina y se revisó la deformación con poses de prueba.

La camilla se construyó por separado, con las mismas proporciones y colores de la lámina. Por último se preparó una escena de presentación con el paciente acostado sobre la camilla.

## Base del personaje

Del Neo Investigador se conservan el esqueleto humanoide de 111 huesos, la cara con sus 58 expresiones faciales, el cabello y las manos. El personaje se escaló de manera uniforme de 1,80 m a 1,68 m, la altura indicada en la lámina.

Los ojos se llevaron a un azul más claro y vivo, porque la lámina describe que tienen un tono inusual que refleja la modificación genética.

## Piel reconstruida

En el Neo Investigador se había retirado la piel que quedaba oculta bajo la chaqueta, la camisa y los guantes. El Paciente lleva las mangas remangadas y no usa guantes, así que había que recuperar los antebrazos, las manos y el cuello.

Esa piel se reconstruyó a partir de las prendas que se habían generado desde la malla del cuerpo: la camisa sin recortar de la entrega del 29 de octubre (`investigador-vroid.blend`) y los guantes. Ambas se devolvieron a la superficie de la piel deshaciendo su separación (3,5 mm la camisa y 3 mm los guantes). Como proceden del cuerpo de VRoid, conservan sus coordenadas de textura y sus pesos, y la textura de la piel encaja con la de la cara y los dedos. La unión entre la mano y el antebrazo queda cubierta por las pulseras.

La textura de la piel de VRoid es transparente en las zonas que cubría la ropa original. Para la piel reconstruida se usa una copia de esa textura en la que esas zonas se rellenaron con el tono de piel.

## Cabello rizado

El peinado de VRoid es corto y liso, mientras que la lámina muestra un cabello más alborotado. Para acercarlo, los mechones se ondularon con un desplazamiento suave, mayor hacia las puntas, y se añadieron 75 rizos en espiral sobre la parte superior, los lados y la nuca. Los rizos tienen el mismo tono azul oscuro del cabello y no cubren la cara ni los ojos.

## Prendas

- **Camiseta clara**, holgada y con un triángulo en el pecho. Cae por encima de la cintura del pantalón.
- **Chaqueta verde oliva abierta**, con mangas remangadas al codo, puños de un tono más oscuro y cuello alto con interior claro.
- **Capucha caída** sobre la espalda, con un panel claro triangular y el emblema del triángulo en el centro.
- **Pantalón cargo ancho**, más amplio hacia el tobillo, con bolsillos cargo en ambos muslos y dos correas negras con hebilla que cuelgan de la cintura.
- **Zapatillas claras** de caña baja, y **calcetines blancos** para la escena en la camilla.

Entre capas se mantuvo una separación mínima. La camiseta queda a 13,5 mm de la piel en la cintura, por encima del pantalón (11 mm) y por debajo de la chaqueta (16 mm), para que no la atraviese.

## Soporte vital y accesorios

- **Mascarilla respiratoria** que cubre nariz y boca, con dos filtros laterales, una válvula central y una cinta que rodea la cabeza. Está en su propia colección para poder ocultarla.
- **Collarín blanco con sensor** en el lado izquierdo del cuello.
- **Puerto vascular** en la cara interior del antebrazo derecho, cerca del codo: un parche blanco con un dispositivo y una luz azul.
- **Pulseras** en ambas muñecas: una negra en la izquierda y una blanca en la derecha.

## Rig y pesos

Se conserva el esqueleto del Neo Investigador sin cambios. Las prendas que proceden del cuerpo conservan sus pesos. Las piezas generadas aparte, como la capucha, las correas y las pulseras, toman los pesos de la superficie más cercana, limitados a cuatro influencias por vértice. Las piezas rígidas siguen a un solo hueso: la mascarilla y sus filtros a la cabeza, el collarín y el sensor al cuello, el puerto vascular al antebrazo y el logotipo al pecho.

La deformación se revisó con los brazos bajados y los codos flexionados, y con el personaje acostado en la camilla. Todas las piezas siguieron al esqueleto.

![Prueba de pose](../../vistas/personajes/paciente/paciente-pose-relajada.png)

*Figura 1. Pose relajada para revisar la deformación de las prendas y los accesorios.*

## Camilla de soporte vital

La camilla es un objeto independiente, sin esqueleto. Mide 0,93 × 2,27 × 0,85 m, tiene la cabecera hacia +Y y apoya en Z = 0. Se compone de:

- pedestal central con dos patas;
- plataforma con defensas laterales segmentadas y luces naranjas en las juntas y en los costados;
- colchón en tres tramos y almohada;
- cabecera con panel y franja de luz, y piecera oscura;
- barandillas tubulares en la mitad de la cabecera;
- manguera y cable que bajan al pedestal.

Usa el mismo estilo de sombreado que el personaje, con nueve materiales en tonos grises, grafito y naranja.

![Camilla](../../vistas/personajes/paciente/camilla-soporte-vistas.png)

*Figura 2. Camilla de soporte vital.*

## Escena en la camilla

Para presentar el personaje como en la lámina se preparó una escena aparte con el paciente acostado boca arriba sobre la camilla. Tiene los brazos a lo largo del cuerpo, los ojos cerrados, la mascarilla puesta y calcetines en lugar de zapatillas. Una manguera une la válvula de la mascarilla con la cabecera.

![Paciente en la camilla](../../vistas/personajes/paciente/paciente-en-camilla-alto.png)

*Figura 3. El Paciente en estado de soporte vital sobre la camilla.*

## Medidas y comprobaciones

| Elemento | Valor |
| --- | ---: |
| Altura del personaje | 1,68 m |
| Triángulos de la exportación del personaje | 77.817 |
| Triángulos en el `.blend` con suavizado de presentación | 139.473 |
| Mallas del personaje | 40 |
| Huesos | 111 |
| Expresiones faciales | 58 |
| Vértices sin peso | 0 |
| Triángulos de la camilla | 13.512 |
| Medidas de la camilla | 0,93 × 2,27 × 0,85 m |

Las exportaciones FBX y GLB del personaje y de la camilla se reimportaron en Blender. Coinciden en mallas, huesos y triángulos, el personaje no tiene vértices sin peso y ambos modelos apoyan en Z = 0.

![Vistas](../../vistas/personajes/paciente/paciente-vistas.png)

*Figura 4. Vistas frontal, lateral, posterior y de tres cuartos en pose T.*

![Rostro](../../vistas/personajes/paciente/paciente-rostro.png)

*Figura 5. Rostro con cabello rizado, cuello alto y collarín.*

![Rostro con mascarilla](../../vistas/personajes/paciente/paciente-mascarilla.png)

*Figura 6. Rostro con la mascarilla respiratoria.*

## Pendientes y límites

- Los rizos son geometría añadida sobre el peinado corto de VRoid. Un peinado rizado hecho en VRoid Studio se vería más limpio y tendría física de cabello.
- La piel se reconstruyó a partir de las prendas porque no se contaba con el avatar original de VRoid Studio. Con ese archivo podría recuperarse la piel completa.
- Los materiales se editaron sin el complemento VRM Add-on for Blender. Si se abre con él, conviene revisar que los colores se conserven. Tampoco se generó una exportación `.vrm`.
- Con 77.817 triángulos, el personaje es más pesado que el Neo Investigador (55.551). Los rizos aportan 15.075. Falta medir el rendimiento en el visor.
- No se ha probado en Unity. Faltan animaciones y niveles de detalle (LOD).
