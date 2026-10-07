# Minuta 2 — Plan de la primera entrega

Oct 7, 2026 · @Moisés

La primera entrega es el Oct 12, 2026: la escena inicial del juego en VR, desde que la tripulación aborda el mecha submarino hasta que llega a un espacio celular. Todo debe estar listo a más tardar el sábado 10 de octubre.

## Acuerdos del chat

Todos los equipos avanzan primero con el mecha submarino, siguiendo la historia acordada en la minuta anterior.

- **Herramientas:** todo se programa en Unity y todo se modela en Blender.
- **El submarino:** mecha submarino con brazos y tripulación humana dentro, reducible a diferentes escalas. Tiene tecnología para evadir el sistema inmune y extensiones (p. ej. pinzas) para intervenir en una célula.
- **Enfoque científico:** la reducción de escala es prácticamente imposible con la física actual, tanto a escala gigante como micro. Apoyarse en la ciencia ficción la hace más creíble (Yibran).
- **Referencias de estilo:** Julio Verne, Pacific Rim, Evangelion (“un Eva bien pasado”, lanzas Longinus y Cassius) y el PRAWN de Subnautica.
- **Es VR:** se necesita una cabina interior detallada (paneles, botones, etc.) y una versión exterior con menos detalle.
- **Modelos 3D:** se hacen en Blender, ya sea desde cero (con un diseño más simple) o partiendo de modelos gratuitos de internet importados a Blender, dándole crédito a los autores.
- **Alcance de la primera entrega:** la escena inicial completa, desde el submarino hasta la llegada a un espacio celular (Inge Chris).
- **Game Design** se encarga de la escenografía, la colorimetría, la música y el estilo de animación (caricatura, animado o realista) y consulta sus decisiones con Diseño. Diseño puede apoyar con los sketches de escenarios.

## Producto de la primera entrega

El lunes se presenta un prototipo jugable hecho en Unity y probado en el visor de la escena inicial, en cuatro momentos, más los documentos que la respaldan.

**Escena inicial (jugable en VR)**

1. **Hangar / base:** el submarino visto por fuera.
2. **Abordaje:** la tripulación sube y el jugador queda en la cabina interior (paneles y botones).
3. **Viaje:** reducción de escala y entrada al cuerpo humano.
4. **Llegada:** el submarino llega a un espacio celular. Aquí termina la entrega.

**Entregables de apoyo**

- Documento de Investigación: cómo funciona el submarino y la reducción de escala.
- Sketches y storyboard de la escena, con la guía de estilo (colorimetría, tipo de animación y música).
- Modelos 3D en Blender: cabina interior y exterior del submarino, y un escenario celular básico.
- Lista de créditos de los modelos y recursos externos que se usen.
- Reporte breve de la prueba en el visor (Implementación).

## Requerimientos entre equipos

Investigación es el primer eslabón: si su documento se retrasa del jueves, se retrasan todos los demás.

| Equipo que necesita | De quién | Qué necesita | Para cuándo |
| --- | --- | --- | --- |
| Game Design | Investigación | Funcionamiento del submarino: materiales, construcción, evasión del sistema inmune, extensiones y comportamiento a cada escala | Jue 8 oct |
| Game Design | Investigación | Cómo luce un espacio celular para la escena de llegada (estructuras visibles, tamaños, colores reales) | Jue 8 oct |
| Diseño | Game Design | Sketches del exterior y la cabina, storyboard de la escena y guía de estilo | Vie 9 oct |
| Diseño | Investigación | Dimensiones y partes del submarino (brazos, pinzas, cabina para la tripulación) | Jue 8 oct |
| Desarrollo | Investigación | Reglas de comportamiento a cada escala, para programar la reducción | Jue 8 oct |
| Desarrollo | Game Design | Storyboard con la secuencia de la escena y qué elementos de la cabina son interactivos | Vie 9 oct |
| Desarrollo | Diseño | Modelos 3D de Blender exportados en FBX (cabina, exterior, escenario celular) | Vie 9 oct (versión preliminar) |
| Implementación | Desarrollo | Build de Unity que corra en el visor de VR | Sáb 10 oct |
| Management | Todos | Avance de cada equipo y bloqueos | Diario (mié–sáb) |
| Todos | Management | Deadlines confirmados y canal de entrega de archivos | Mié 7 oct |

## Plan de trabajo por equipo

Cada equipo puede empezar hoy con lo que no depende de nadie y conectar el resto conforme lleguen los insumos.

### Investigación — entrega jueves 8 oct

- [ ] Documento corto (2–3 páginas) sobre la reducción de escala: la física real y su límite, y una explicación de ciencia ficción que la haga creíble
- [ ] Materiales y construcción del mecha: casco, brazos, cabina para la tripulación
- [ ] Comportamiento a cada escala: qué cambia en movimiento, fluidos y presión (una tabla de escalas)
- [ ] Tecnología para evadir el sistema inmune y extensiones (pinzas) para intervenir en una célula
- [ ] Descripción visual del espacio celular de llegada

### Game Design — entrega viernes 9 oct

- [ ] Storyboard de la escena inicial en sus cuatro momentos (hangar, abordaje, viaje, llegada)
- [ ] Sketches del exterior del submarino y de la cabina interior (paneles y botones)
- [ ] Guía de estilo: colorimetría, tipo de animación (caricatura, animado o realista) y música
- [ ] Consultar las decisiones de estilo con Diseño
- [ ] Empezar el boceto del cartel para la feria

### Diseño — versión preliminar viernes 9, final sábado 10 oct

- [ ] Desde hoy: buscar modelos gratuitos (de preferencia en .blend o FBX) y referencias (Evangelion, Pacific Rim, PRAWN de Subnautica), anotando autor y licencia
- [ ] Modelar en Blender la cabina interior con buen detalle, porque es lo que el jugador ve en VR
- [ ] Modelar en Blender el exterior con menos detalle
- [ ] Modelar en Blender un escenario celular básico
- [ ] Exportar a FBX para Unity, con la escala aplicada y pocos polígonos para que corra fluido en VR
- [ ] Apoyar a Game Design con sketches de escenarios

### Desarrollo — entrega sábado 10 oct

- [ ] Desde hoy: crear el proyecto en Unity con soporte VR (p. ej. OpenXR y XR Interaction Toolkit) y la cámara en primera persona dentro de la cabina, usando modelos provisionales (cubos)
- [ ] Programar en C# la secuencia de la escena (abordaje, viaje, llegada) y la transición de escala
- [ ] Programar la interacción con paneles y botones de la cabina
- [ ] Importar los FBX de Diseño y sustituir los modelos provisionales
- [ ] Enviar a Game Design la lista de requerimientos de Desarrollo (pendiente de Moi)

### Implementación — sábado 10 oct

- [ ] Desde hoy: configurar el Oculus/visor para Unity (modo desarrollador y conexión por cable o build directo al visor) y probar una escena de ejemplo
- [ ] Preparar un checklist de experiencia de usuario: mareo, legibilidad de paneles y escala percibida
- [ ] Probar el build de Unity del sábado y reportar los problemas a Desarrollo

### Management — todos los días

- [ ] Confirmar hoy los integrantes de cada equipo y el canal donde se suben los archivos
- [ ] Revisión diaria de avances y bloqueos
- [ ] Recibir y validar el documento de Investigación el jueves
- [ ] Integrar el resumen para el coordinador el domingo 11 oct

## Cronograma

&#91;embedded content: cronograma por equipo · 7–12 oct\]

Cada barra va de hoy a la fecha límite del equipo; Diseño entrega una versión preliminar el viernes 9 para que Desarrollo pueda integrar.

## Pendientes y riesgos

- **Plazo muy corto:** Investigación tiene un día para entregar. Para no frenar a nadie, conviene que el jueves entregue una versión mínima y la complete después.
- **Requerimientos de Desarrollo:** Moi quedó de compartirlos; mientras tanto, Desarrollo trabaja con modelos provisionales.
- **Sketches de escenarios:** le tocan a Game Design, con apoyo de Diseño. Falta confirmar quién hace cada uno para no duplicar trabajo.
- **Versiones:** que todos usen la misma versión de Unity (con soporte XR) y de Blender, para que los archivos abran igual en todas las computadoras. Confirmar también qué visor exacto se usará y cuántos hay.
- **Paso de Blender a Unity:** acordar el formato (FBX), la escala y los ejes (Blender usa Z hacia arriba; Unity, Y), los nombres de archivo y un límite de polígonos.
- **Proyecto compartido:** definir cómo se comparte el proyecto de Unity (p. ej. Git con LFS o Unity Version Control) para no pisar el trabajo de otros.
- **Licencias:** revisar que los modelos gratuitos permitan su uso y llevar la lista de créditos.
- **Lugar y hora de la presentación del lunes:** por confirmar.
