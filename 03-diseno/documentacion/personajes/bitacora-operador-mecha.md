# Bitácora del Operador del Mecha

School Redemption Game

Trabajo registrado el 17 de noviembre de 2026

El Operador del Mecha controla el Mecha a distancia desde el laboratorio y es el protagonista de la narrativa actualizada. El modelo parte de un avatar nuevo de VRoid Studio creado por Chris Rojas a partir de la lámina «Mecha Operator» de Game Design, y viste la ropa y el equipo de esa lámina. Queda **listo para animar**: está riggeado y todas las piezas tienen pesos sobre el esqueleto. No incluye animaciones.

A diferencia del Neo Investigador, este modelo se preparó desde el principio para Meta Quest: 20.562 triángulos, 3 materiales y texturas de 2048 px como máximo.

## Alcance del trabajo

Se creó el avatar en VRoid Studio y se exportó en VRM con las reducciones de la propia aplicación. En Blender se separaron la piel, el cabello y la ropa de VRoid; se construyeron prendas nuevas sobre el cuerpo, se añadió el equipo de la lámina y se transfirieron los pesos. Después se revisó la deformación con una pose de prueba, se optimizó el modelo para el visor y se exportó en FBX y GLB. El panel holográfico cian se entrega como objeto aparte.

## Base del personaje

El avatar se hizo en VRoid Studio 2.16 con la base femenina: un set de cara de expresión serena, un peinado bob a la altura de la barbilla con flequillo, ojos grises, piel morena y 161 cm de estatura. Los colores se tomaron de la lámina:

| Elemento | Color |
| --- | --- |
| Iris | #7E8287 |
| Cabello, cejas y pestañas | #2A2B2E |
| Piel (base, sombra y luz) | #D8A893, #B9806C y #E8C2AF |

La exportación VRM 1.0 usó estas opciones: *Delete Transparent Meshes* desactivada, para conservar la piel bajo la ropa; *Face*, *Body* y *Outfit* al 100; *Hair* al 82 y *Hair Smoothness* al 31; 2 materiales con atlas de 4096 px; y la reducción de huesos del cabello al máximo. Con el cabello al 100 el bob se deshacía en mechones sueltos, por eso se dejó en 82. El VRM resultante tiene 25.652 triángulos, 2 materiales y 109 huesos.

Con 2 materiales VRoid une cuerpo, cabello y ropa en una sola malla. Se separaron según la zona del atlas que ocupa cada isla de UV, y la ropa de VRoid se eliminó.

## Prendas

- **Camiseta negra** de manga larga y cuello alto, a 3,5 mm de la piel.
- **Túnica verde oliva** con mangas cortas y holgadas sobre el codo, cuello alzado con una pequeña abertura al frente, caída recta hasta medio muslo y aberturas laterales. Va a 12,5 mm de la piel y a 16 mm del pantalón en la cadera.
- **Pantalón cargo gris carbón**, holgado, más amplio hacia la pantorrilla y recogido en un puño sobre el tobillo. Va a 11 mm de la piel.

| Pieza | Color |
| --- | --- |
| Túnica | #7A7864 |
| Camiseta y botas | #2B2824 |
| Pantalón | #47443F |
| Puño del pantalón | #3D3C3B |

Las partes altas de las prendas salen de la malla del cuerpo, desplazada hacia fuera. La base de VRoid con *Body* al 100 tiene pocos polígonos, y el desplazamiento por las normales plegaba caras al revés; como el sombreado no dibuja la cara trasera, se veían huecos. Por eso las mangas, las perneras y el faldón se construyeron como anillos a lo largo del eje de cada hueso: cada vértice conserva su ángulo y su radio se mide sobre la piel con rayos, más la separación de la capa. Los muslos de VRoid se tocan, así que cada pernera es un tubo propio que arranca dentro de la cadera.

Los cortes de dobladillos, mangas y límites de color se hicieron con planos para que quedaran rectos. La piel que queda oculta bajo la ropa se eliminó, conservando un anillo de caras en cada borde.

## Equipo

- **Botas** negras con franja y tirador naranja, puntera gris y suela clara de 3 cm.
- **Guantes hápticos** grises de dedos completos, con muñequera clara.
- **Cinturón ancho** con hebilla gris y placa naranja.
- **Arnés**: tirantes sobre los hombros con ajustadores naranja y grises, y cinta de pecho con un panel cian.
- **Correas** que bajan del cinturón por los muslos y **bolsillos cargo** en los costados.
- **Diadema** con auriculares claros, luz naranja y micrófono de brazo.
- **Parche redondo** en el brazo izquierdo con un emblema de ADN dibujado para esta entrega: doble hélice cian y gris sobre un disco azul marino con anillo naranja.

Las cintas del arnés se proyectan sobre la superficie de la ropa y se separan hacia su normal. Las piezas rígidas tienen pocas caras y bisel.

## Rig y pesos

Se conserva el esqueleto humanoide de VRoid. Los pesos se transfirieron con un método propio: cada vértice toma el triángulo más cercano de la piel completa e interpola sus pesos; después se suavizan, se limitan a cuatro influencias por vértice y se normalizan. El faldón de la túnica recibe un suavizado adicional para que no se separe entre las piernas. Las piezas rígidas, como la diadema, el parche, las hebillas, el panel del pecho y las suelas, siguen a un solo hueso. Ningún vértice quedó sin peso.

La deformación se revisó con los brazos bajados unos 65°, los codos a 55°, una rodilla flexionada y el torso girado. Todas las piezas siguieron al esqueleto.

![Pose relajada](../../vistas/personajes/operador-mecha/operador-mecha-pose-relajada.png)

*Figura 1. Pose relajada para revisar la deformación de la ropa y el equipo.*

## Optimización para Meta Quest

| Elemento | Antes | Después |
| --- | ---: | ---: |
| Triángulos del personaje | 36.866 | 20.562 |
| Triángulos del cabello | 14.540 | 4.362 |
| Materiales | 15 | 3 |
| Huesos | 109 | 59 |
| Textura más grande | 4096 px | 2048 px |

El cabello se redujo al 30 %. Se comparó con el original al 50, 38 y 30 % de frente y de espalda, y la diferencia no se aprecia. También se eliminaron las partes de la camiseta y del pantalón que la túnica cubre por completo, y los huesos sin pesos en ninguna malla, comprobando antes que ningún vértice dependiera de ellos. Solo se quitaron cadenas terminales, por lo que la jerarquía humanoide se conserva.

Los 3 materiales son:

- **OPM_Cara**: el atlas de la cara de VRoid reducido de 4096 a 2048 px.
- **OPM_Cabello_Piel**: un atlas nuevo de 2048 px con los mechones a resolución original en la mitad superior y la piel reducida a la mitad.
- **OPM_Ropa_Equipo**: comparte esa textura. La ropa y el equipo usan una paleta de colores planos y el emblema en el cuadrante libre, con un mapa de emisión de 512 px para el cian.

Las texturas se optimizaron sin pérdida con oxipng y se comprobó que sus píxeles son idénticos.

## Exportaciones y verificación

| Comprobación | FBX | GLB |
| --- | ---: | ---: |
| Mallas | 3 | 3 |
| Huesos | 59 | 59 |
| Triángulos | 20.562 | 20.562 |
| Expresiones faciales | 57 | 57 |
| Vértices sin peso | 0 | 0 |
| Materiales | 3 | 3 |
| Altura | 1,626 m | 1,626 m |
| Z mínima | 0 | 0 |

La altura incluye la diadema; el cuerpo mide 1,61 m. El FBX se exportó con Forward -Z, Up Y y *FBX All*, sin *Apply Transform*, porque el manual de Blender la marca como experimental y conocida por fallar con armaduras. El GLB se exportó con los modificadores aplicados. Ambos se reimportaron en Blender y coinciden entre sí y con el modelo. Los scripts reprodujeron el modelo al reconstruirlo desde cero en una carpeta vacía.

![Vistas](../../vistas/personajes/operador-mecha/operador-mecha-vistas.png)

*Figura 2. Vistas frontal, lateral, posterior y de tres cuartos en pose T.*

![Rostro](../../vistas/personajes/operador-mecha/operador-mecha-rostro.png)

*Figura 3. Rostro con la diadema, el micrófono y el cuello alto.*

![Panel holográfico](../../vistas/personajes/operador-mecha/operador-mecha-panel.png)

*Figura 4. Panel holográfico cian, entregado como objeto aparte.*

## Pendientes y límites

- Bajo los ojos se ve una sombra gris que viene de la textura de la cara de VRoid, igual que en el Neo Investigador. Puede corregirse en el maquillaje de la cara en VRoid Studio y volver a exportar; los scripts rehacen el modelo con el VRM nuevo.
- El cabello es más ordenado que en la lámina, donde se ve más despeinado y con la raya de lado.
- El cuello alto negro casi no se ve dentro del cuello de la túnica.
- El sombreado de la túnica muestra algunas irregularidades.
- Falta probarlo en Unity y en el visor. No tiene animaciones ni niveles de detalle (LOD), y el panel holográfico aún no está sujeto a la mano.
