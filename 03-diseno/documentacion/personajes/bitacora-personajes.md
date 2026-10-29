# Bitácora de desarrollo de personajes

School Redemption Game

Trabajo registrado el 28 de octubre de 2026
Entrega documental del 29 de octubre de 2026

Se desarrollaron dos personajes tridimensionales estilizados para el proyecto School Redemption Game: el Explorador Celular y El Investigador. La entrega conserva archivos editables de Blender, referencias visuales, materiales planos, vistas de revisión y scripts reproducibles en Python mediante bpy. Esta bitácora registra la construcción inicial, las correcciones y el estado técnico alcanzado.

## Alcance del trabajo

El Explorador Celular se resolvió como un blockout verificable con sus accesorios distintivos. El Investigador avanzó desde una primera construcción de cuerpo y equipo hasta una revisión específica de cara y hombros, identificada como versión 03. El objetivo de estas fases fue fijar silueta, proporciones, lectura de las prendas e identidad de cada personaje.

Los modelos se mantuvieron organizados por función y con nombres descriptivos. Se conservaron modificadores editables y piezas independientes. El trabajo no incluye todavía un rig, animaciones, texturizado final ni una validación de rendimiento dentro de un visor VR.

## Referencias y criterios comunes

Se revisaron las láminas conceptuales antes de construir la geometría. Las vistas frontal, lateral y posterior sirvieron para comparar la altura del personaje, el volumen del torso, la longitud de las extremidades y la posición de la mochila y los accesorios. Los detalles ampliados de casco, guantes, gafas y prendas guiaron las formas que debían reconocerse desde el blockout.

Se trabajó con unidades métricas, una unidad de Blender equivalente a un metro, el eje Z como vertical y el frente orientado hacia -Y. Las plantas se situaron en Z = 0 y el controlador de cada conjunto se colocó en el origen. Los colores planos se utilizaron para distinguir materiales y piezas durante la revisión.

## Método de construcción y revisión

La geometría se construyó con mallas de bucles, superficies de prendas, perfiles y volúmenes de baja densidad, apoyados por funciones de bpy. Se generaron cámaras ortográficas y vistas de tres cuartos para revisar las formas. Las iteraciones se evaluaron con renders y con una reapertura independiente de los archivos guardados, registrando medidas y triángulos.

## Desarrollo del Explorador Celular

### Construcción de la silueta

Se fijó una altura de 1,70 m, incluida la antena. Primero se definieron cabeza, torso, pelvis, brazos, piernas y botas. Después se ajustaron los volúmenes del traje: mangas amplias, pantalón holgado, cintura recogida y apoyo ancho de los pies.

### Elementos de identidad

Se modelaron el casco ovoide con abertura real, visor curvado oscuro, comunicadores concéntricos, antena y collar de soporte. La mochila incorporó un puerto circular posterior y abrazaderas. El conjunto se completó con placa pectoral y triángulo, tirantes naranjas, cinturón, bolsas laterales, rodilleras, guantes con sensores y brazalete de análisis.

### Ajustes durante la revisión

La inspección de frente, perfil y espalda permitió corregir normales de la calota y el visor, oscurecer el cristal, reforzar los acentos naranjas y ampliar la placa pectoral. Los refuerzos iniciales de espinilla se retiraron y se sustituyeron por áreas de color sobre el pantalón para acercar la lectura a la lámina.

![Referencia visual](../../vistas/personajes/explorador/explorador-celular-vistas.png)

*Figura 1. Blockout del Explorador Celular con vistas frontal, lateral y posterior a escala compartida.*

El archivo quedó dividido en BODY, HELMET, BACKPACK, SUIT, GLOVES, BOOTS, ACCESSORIES y REFERENCES. PRESENTATION agrupa únicamente cámaras, luces y suelo. Las tres imágenes de referencia quedaron empaquetadas en el archivo.

## Desarrollo de El Investigador

### Proporciones y prendas

Se tomó la lámina de El Investigador como referencia principal y se fijó una altura de 1,78 m. La silueta se planteó más esbelta que la del explorador, con cabeza descubierta, piernas largas y ropa de campo. Se construyó una chaqueta corta abierta con hombros oscuros, cuello alto, forro y ribetes amarillos, en lugar de una prenda larga.

Las mangas se remangaron para dejar los antebrazos expuestos. Se añadieron credencial colgante, parches circulares, tirantes de mochila, cinturón con hebilla y cintas para herramientas. El pantalón cargo incorporó bolsillos externos y pliegues grandes, y las botas combinaron piezas claras y grafito con lengüeta amarilla.

### Cabeza cabello y gafas

La primera cabeza incluyó mandíbula afinada, ojos almendrados gris azulados, boca neutra y gafas rectangulares con puente y patillas. El cabello se construyó con una base y mechones anchos superpuestos. El flequillo siguió un barrido diagonal; el volumen posterior y la nuca se ajustaron en varias pasadas para evitar una calota lisa y mechones demasiado paralelos.

Se redujo la apertura de los ojos para recuperar una expresión contenida y se llevó el cabello hacia un negro azulado. La camisa se elevó en el cuello para reducir la franja de piel expuesta y mejorar su relación con el cuello exterior de la chaqueta.

### Tableta manos y variantes

Se modeló una tableta de carcasa biselada y pantalla azul oscura. La interfaz se representó con geometría editable de paneles, barras, trazas y un esquema celular. Las manos se dispusieron junto a los bordes, con guantes sin dedos y puntas de piel visibles. Se ajustó la proyección de los dedos para mostrar contacto con el dispositivo.

Se prepararon dos escenas. Investigador_Blockout muestra la lectura con tableta. Investigador_Pose_A conserva una variante con los brazos separados aproximadamente 15 grados de la vertical para inspección y edición. Esta variante es una pose relajada de trabajo y no una pose A definitiva para rigging.

### Correcciones del cuerpo y el equipo

Los parches de color que parecían camuflaje en las piernas se sustituyeron por pliegues discretos. La cadera y las perneras se reconstruyeron como una malla continua para eliminar la unión visible entre tres volúmenes. También se corrigió la orientación del emblema posterior de la mochila y se revisó el amarillo mostaza de los acentos.

## Revisión de cara y hombros

La revisión de la versión anterior detectó una cara demasiado lisa y una unión abultada entre torso y mangas. La versión 03 incorporó una superficie facial con mejor distribución de vértices y una chaqueta con bucles compartidos en las sisas. La comparación utiliza los mismos encuadres para hacer visibles los cambios.

![Referencia visual](../../vistas/personajes/investigador/investigador-comparacion-v03.png)

*Figura 2. Comparación de la cara y de los hombros entre la versión anterior y la revisión 03.*

## Correcciones técnicas de la versión 03

La malla facial recibió más muestras en el frente y alrededor de nariz, ojos y boca. Se definieron puente, punta y alas nasales, cuencas, pómulos, mejillas, mandíbula y mentón. Los ojos se curvaron dentro de las cuencas y se añadieron párpados de piel, pliegue superior y lagrimales discretos.

Los labios se resolvieron como superficies suaves y se redujo su proyección de perfil. Las orejas se hicieron más pequeñas, con contorno irregular y pliegues ligeros. Los rasgos se ajustaron a la piel subdividida mediante una estructura BVH de consulta para evitar separaciones entre la cara y los detalles. La subdivisión facial se conservó en nivel 1.

En los hombros se suavizó la pendiente entre cuello, clavícula y manga. Se sustituyeron las tapas solapadas por una chaqueta continua con bucles de sisa compartidos. Las secciones de manga se transportaron a lo largo del brazo para evitar giros bruscos. El canesú recibió bordes continuos y los parches se proyectaron sobre la curvatura de la tela.

## Medidas y geometría verificadas

| Personaje o variante | Altura | Mallas | Triángulos base | Triángulos evaluados |
| --- | ---: | ---: | ---: | ---: |
| Explorador Celular | 1,70 m | 157 | 9.408 | 19.684 |
| Investigador con tableta | 1,78 m | 286 | 18.025 | 42.510 |
| Investigador con brazos separados | 1,78 m | 247 | 16.463 | 39.476 |

*Los triángulos evaluados incluyen biseles, subdivisión y espesores. Los conteos excluyen cámaras, luces, suelo y referencias. Cada variante del investigador se cuenta por separado.*

## Comprobaciones de los archivos

Los archivos guardados se reabrieron en Blender 5.2.2 LTS y se contrastaron con los informes de inventario. Se comprobaron unidades, altura, apoyo en Z = 0, vértices finitos, referencias empaquetadas, número de mallas y triángulos, y ausencia de armature y animaciones.

En las dos variantes del investigador, cada chaqueta forma un componente conectado. La validación no encontró vértices sueltos ni bordes con más de dos caras. Tras evaluar subdivisión y espesor, el resultado presentó cero bordes no manifold. Los registros de verificación de ambos personajes finalizaron con resultado PASS.

## Organización de la entrega

Las fuentes editables se entregan como fuentes/explorador-celular.blend y fuentes/investigador.blend. Las acompañan scripts de construcción y validación, referencias, vistas e informes de inventario. El investigador corresponde a la revisión 03 de cara y hombros. Las piezas y los modificadores se conservan en los archivos nativos.

Las exportaciones estáticas FBX y GLB se conservan en exports. investigador corresponde a la variante con brazos separados e investigador-lectura conserva la tableta. Estos archivos facilitan el intercambio de las poses, pero su existencia no sustituye la integración ni las pruebas dentro del motor de juego. El estudio y las referencias se separan de la geometría del personaje.

### Reproducción y consulta

Para reproducir los modelos se utiliza Blender con los archivos de scripts/personajes/explorador y scripts/personajes/investigador. El investigador utiliza también el módulo cabeza_investigador.py. Las imágenes se conservan en referencias/personajes. Las instrucciones de la entrega describen las rutas, los parámetros, las escenas y las cámaras de revisión.

Los scripts de validación generan registros JSON con medidas, conteos y comprobaciones. Estos informes permiten contrastar la geometría guardada con la documentación sin depender únicamente de una imagen del personaje.

### Control de versiones

La entrega se organiza para la rama Chris_Desing del repositorio moiseiseiseis/biogamevr. Las fuentes editables se ubican en 03-diseno/fuentes; las exportaciones, en 03-diseno/exports; y esta bitácora, en 03-diseno/documentacion/personajes. Los nombres y la estructura permiten revisar y actualizar cada personaje por separado.

## Trabajo pendiente

La siguiente fase comprende definir la pose A definitiva y preparar la topología de deformación del cuerpo y la cara. Después corresponde construir el rig, asignar pesos, preparar UV y texturas finales y producir las animaciones necesarias para el juego.

La integración debe completar materiales y escala dentro del motor, revisar colisiones, establecer niveles de detalle y comprobar el rendimiento en el dispositivo VR objetivo. Los presupuestos finales de geometría y materiales se definirán a partir de esas pruebas.

## Nota final
