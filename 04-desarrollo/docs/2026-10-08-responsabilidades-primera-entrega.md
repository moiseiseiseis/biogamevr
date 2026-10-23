# Equipo de Desarrollo — Responsabilidades para la primera entrega

Oct 7, 2026 · @Moisés

## Resumen

El sábado 10 de octubre Desarrollo entrega a Implementación un build de Unity que corre en el visor con la escena inicial completa: hangar, abordaje, viaje y llegada al espacio celular.

El trabajo se divide en 6 roles con una sola persona responsable cada uno, para que nadie edite lo mismo al mismo tiempo. Todos trabajan desde hoy con modelos provisionales (cubos y cápsulas) y sustituyen por los FBX de Diseño cuando lleguen el viernes 9.

Es un primer boceto: los nombres se asignan en la reunión del equipo y los roles se pueden fusionar o dividir.

## Roles

Cada rol es dueño de un conjunto de escenas, prefabs o scripts; esa propiedad es la que evita conflictos en Git.

| # | Rol | Responsable | Es dueño de | Entrega clave |
| --- | --- | --- | --- | --- |
| 1 | Líder técnico e integración | Por asignar | Repositorio, escena principal, estructura de carpetas | Proyecto base hoy; build integrado el sábado |
| 2 | Rig VR y cámara | Por asignar | XR Origin, controles, comodidad | Jugador sentado en la cabina con las manos funcionando |
| 3 | Interacción de cabina | Por asignar | Prefabs de paneles, botones y palancas | Botones que disparan eventos de la secuencia |
| 4 | Secuencia y flujo de la escena | Por asignar | GameManager, estados, transiciones con fundido | Los cuatro momentos encadenados de inicio a fin |
| 5 | Transición de escala y efectos | Por asignar | Efecto de reducción, partículas, post-procesado | Viaje creíble sin marear al jugador |
| 6 | Entornos, assets y build | Por asignar | Escenas del hangar y espacio celular, importación de FBX, build al visor | Escenarios con modelos reales y APK instalado en el visor |

Si alguien del equipo tiene laptop con GPU dedicada, conviene que tome el rol 2 o el 6, porque son los que más prueban en el visor.

## Tareas por rol

Cada rol termina cuando cumple su criterio de terminado, no cuando "ya casi".

### 1. Líder técnico e integración

- [ ] Crear el proyecto en Unity 6.3 LTS con la plantilla VR y fijar la versión exacta para todos
- [ ] Crear el repo en GitHub con `.gitignore` de Unity y Git LFS para `.fbx`, `.png`, `.wav`
- [ ] Crear la estructura de carpetas: `Assets/Scenes`, `Prefabs`, `Scripts`, `Models`, `Materials`, `Audio`
- [ ] Configurar MCP for Unity y documentar los pasos para el resto
- [ ] Enviar hoy la lista de requerimientos a Game Design (sección de abajo)
- [ ] Revisar y fusionar los pull requests; armar el build integrado el sábado

**Terminado cuando:** los 6 clonan el repo, abren el proyecto sin errores y el sábado existe un solo build con todo integrado.

### 2. Rig VR y cámara

- [ ] Configurar OpenXR y el XR Origin con XR Interaction Toolkit
- [ ] Colocar al jugador sentado en la cabina provisional (cubo), con el origen en el asiento del piloto
- [ ] Configurar controles y manos; probar con el XR Device Simulator
- [ ] Desactivar locomoción libre: el jugador no camina, el submarino lo mueve
- [ ] Agregar viñeta de comodidad para los momentos de movimiento

**Terminado cuando:** en el simulador y en el visor, el jugador ve la cabina desde el asiento, mueve la cabeza y las manos, y no se sale de su lugar.

### 3. Interacción de cabina

- [ ] Crear prefabs genéricos: botón (poke), palanca (grab con límite) y pantalla (Canvas en world space)
- [ ] Cada prefab dispara un evento de C# (`UnityEvent`) que otros scripts escuchan
- [ ] Crear los controles que pida el storyboard, por ejemplo "iniciar inmersión" y "activar reducción"
- [ ] Retroalimentación al presionar: sonido, cambio de color o vibración del control

**Terminado cuando:** al presionar el botón "activar reducción" en el visor, el evento llega a la secuencia y se nota la respuesta.

### 4. Secuencia y flujo de la escena

- [ ] Programar un `GameManager` con máquina de estados: Hangar → Abordaje → Viaje → Llegada
- [ ] Transiciones con fundido a negro entre momentos
- [ ] Decidir con el rol 6 si son escenas separadas o una sola escena con zonas
- [ ] Conectar los eventos de la cabina (rol 3) con los cambios de estado
- [ ] Agregar un atajo de depuración (tecla o botón) para saltar entre estados

**Terminado cuando:** se puede jugar de principio a fin sin tocar el editor y saltar a cualquier momento para probarlo.

### 5. Transición de escala y efectos

- [ ] Programar la reducción como cambio de nivel de escala, no como escalado continuo hasta el nanómetro
- [ ] Efecto visual del viaje: túnel de partículas, distorsión o destello, según la guía de estilo
- [ ] Usar las reglas de Investigación por escala para el comportamiento (fluidos, movimiento)
- [ ] Mantener la cabina fija y mover el exterior, para reducir el mareo
- [ ] Revisar con Implementación el checklist de mareo

**Terminado cuando:** el viaje dura lo acordado en el storyboard, se entiende que el submarino se reduce y nadie de Implementación reporta mareo fuerte.

### 6. Entornos, assets y build

- [ ] Armar el hangar y el espacio celular con placeholders e iluminación básica
- [ ] Acordar con Diseño el paso de Blender a Unity: FBX, 1 unidad = 1 metro, ejes (Blender Z arriba, Unity Y arriba), nombres
- [ ] Importar los FBX del viernes y sustituir los placeholders dentro de los prefabs
- [ ] Revisar polígonos y materiales para que corra fluido en el visor
- [ ] Configurar el build para Android/Quest y hacer el primer build de prueba antes del sábado
- [ ] Llevar la lista de créditos de modelos y recursos externos que se importen

**Terminado cuando:** las dos escenas usan los modelos de Diseño y el APK se instala y abre en el visor.

## Calendario

El viernes en la noche todo debe estar integrado con placeholders; el sábado solo se sustituyen modelos, se corrige y se hace el build.

| Día | Qué debe existir al final del día | Depende de |
| --- | --- | --- |
| Mié 7 oct | Repo creado, los 6 con el proyecto abierto; requerimientos enviados a Game Design | Rol 1 |
| Jue 8 oct | Jugador sentado en la cabina provisional; prefabs de botón y palanca; máquina de estados con los cuatro momentos vacíos | Reglas de escala de Investigación |
| Vie 9 oct | Secuencia completa jugable con cubos; primer efecto de reducción; primer build de prueba en el visor; FBX preliminares importados | Storyboard de Game Design; FBX de Diseño |
| Sáb 10 oct | Modelos finales sustituidos, errores corregidos, build entregado a Implementación | FBX finales de Diseño |
| Dom 11 oct | Correcciones del reporte de Implementación, solo si son críticas | Reporte de Implementación |
| Lun 12 oct | Presentación del prototipo | — |

Cada día, cada persona reporta a Management en una línea: qué terminó, qué sigue y qué la bloquea.

## Reglas de trabajo

1. Todos usan la misma versión exacta de Unity, la que fije el rol 1.
2. Una rama por tarea (`rol3/boton-reduccion`) y pull request hacia `main`; nadie sube directo a `main`.
3. Una persona por escena a la vez; se avisa en el chat antes de abrir una escena que no es tuya.
4. Todo lo que se pueda se trabaja como prefab; la escena solo los ensambla.
5. Commit antes de pedirle cambios grandes a Claude por MCP, y el MCP solo se usa sobre tus propias escenas y prefabs.
6. Por cada script que genere Claude, quien lo pide debe poder explicar qué hace antes de subirlo.
7. Los modelos se suben ya terminados por Git LFS, no cada versión intermedia, para cuidar la cuota de almacenamiento.

## Requerimientos de Desarrollo para otros equipos

Este es el pendiente de Moisés según la Minuta 2; se puede enviar hoy tal cual o ajustado.

| Para | Qué necesitamos | Formato | Para cuándo |
| --- | --- | --- | --- |
| Game Design | Secuencia de la escena en los cuatro momentos, con duración aproximada de cada uno | Storyboard con una viñeta por acción | Vie 9 oct |
| Game Design | Lista de controles interactivos de la cabina: nombre, qué hace y en qué momento se usa | Tabla simple | Jue 8 oct |
| Game Design | Qué ve el jugador durante el viaje de reducción y cuánto dura | Descripción o sketch | Vie 9 oct |
| Game Design | Guía de estilo: paleta de colores y música o sonidos de cada momento | Paleta en códigos de color; audios en `.wav` u `.ogg` | Vie 9 oct |
| Diseño | Cabina interior con controles como piezas separadas y pivote en el asiento del piloto | FBX, 1 unidad = 1 metro, Y arriba | Vie 9 oct (preliminar) |
| Diseño | Exterior del submarino de bajo detalle para el hangar | FBX | Vie 9 oct |
| Diseño | Escenario celular básico | FBX | Vie 9 oct |
| Diseño | Partes móviles separadas (brazos, pinzas, escotilla) si se van a animar | Objetos con nombre dentro del FBX | Sáb 10 oct |
| Investigación | Escalas del viaje: de qué tamaño a qué tamaño pasa el submarino y en cuántos pasos | Tabla de escalas | Jue 8 oct |
| Investigación | Comportamiento en cada escala: cómo se mueve y qué cambia en fluidos y presión | Tabla breve | Jue 8 oct |
| Implementación | Modelo exacto del visor, cuántos hay y si está en modo desarrollador | Mensaje en el chat | Jue 8 oct |

## Riesgos y decisiones pendientes

El riesgo mayor es llegar al sábado sin haber probado nunca en el visor; por eso el primer build es el viernes.

| Riesgo | Plan B |
| --- | --- |
| Los FBX llegan tarde o con problemas | Se presenta con placeholders bien iluminados; la mecánica es lo que se evalúa |
| No se sabe qué visor es | Se configura con OpenXR, que funciona con la mayoría; se confirma el jueves |
| El build no corre fluido | Bajar polígonos, sombras y partículas antes que recortar momentos de la escena |
| Conflictos en Git el sábado | Congelar `main` el sábado a mediodía; solo el rol 1 fusiona |
| El viaje marea | Acortar el viaje y reforzar la cabina fija como referencia visual |

- [ ] Asignar un nombre a cada rol
- [ ] Decidir si los cuatro momentos son escenas separadas o una sola escena
- [ ] Confirmar el modelo de visor y quién del equipo tiene laptop con GPU dedicada
- [ ] Elegir canal de comunicación del equipo de Desarrollo
