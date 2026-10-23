# Equipo de Desarrollo — Pendientes de la primera entrega

Oct 8, 2026 · @Moisés

## Resumen

El sábado 10 de octubre entregamos a Implementación un build que corre en el visor con la escena inicial completa: hangar, abordaje, viaje y llegada al espacio celular. El viernes en la noche todo debe estar integrado con placeholders; el sábado solo se sustituyen modelos, se corrige y se hace el build.

Investigación ya entregó su reporte (*Un mecha submarino a escala celular*, v2). Con eso ya tenemos la tabla de escalas, cómo se mueve el mecha en cada una y cómo luce el espacio celular de llegada. Esta lista ya lo incluye.

## Lo que ya está hecho

- [x] Proyecto Unity `04-desarrollo/BioVR` en el repo (rama `main`), con Unity **6000.3.26f1**, URP, OpenXR y XR Interaction Toolkit 3.6.1
- [x] OpenXR configurado para Meta Quest (perfil Oculus Touch)
- [x] XR Device Simulator activado: se puede probar sin visor
- [x] Carpetas por rol en `Assets/_BioVR/` y escena `00-EscenaInicial` con el XR Origin

**Para abrirlo:** instala Unity 6000.3.26f1 con Android Build Support (OpenJDK + SDK & NDK), clona el repo y en Unity Hub usa *Add project from disk* → `04-desarrollo/BioVR`. Si Unity Hub ofrece cambiar de versión, di que no. Los detalles están en `04-desarrollo/README.md`.

## Lo que dice Investigación y nos afecta

**El viaje son 4 niveles, cada uno unas 1000 veces más chico.** No es un encogimiento continuo, son saltos:

| # | Nivel | Tamaño del mecha | Cómo se mueve | Qué hay alrededor |
| --- | --- | --- | --- | --- |
| 0 | Hangar | Escala humana | Normal | Hangar, submarino por fuera |
| 1 | Vasos sanguíneos | ~1–5 mm | Con inercia; la corriente empuja con pulsos | Sangre: ~45% glóbulos rojos |
| 2 | Capilar / tejido | ~1 µm | Casi sin inercia; muy viscoso | Plasma con proteínas |
| 3 | Citoplasma (llegada) | ~200–500 nm | Sin inercia: al dejar de empujar se detiene. “Como moverse en miel”, con empujones constantes de moléculas | Espacio muy lleno de moléculas |

Los niveles de orgánulo (~50–100 nm) y molecular (~5–10 nm) quedan para después de la primera entrega.

**Espacio celular de llegada:** el mecha entra por debajo de la membrana.
- Pared de membrana con proteínas incrustadas (canales, bombas, receptores) y una malla de actina que la sostiene
- Interior muy lleno, no vacío: ribosomas, enzimas y proteínas por todas partes, siempre en movimiento
- Filamentos que cruzan la célula: actina (8 nm, delgados), filamentos intermedios (~10 nm) y microtúbulos (25 nm, gruesos)
- Una cinesina “caminando” sobre un microtúbulo y arrastrando una vesícula
- Al fondo: el Golgi (discos apilados) y mitocondrias
- Los colores son libres; Game Design y Diseño los eligen

## Decisiones tomadas

- **Protocolo Matrioska.** El mecha madre se separa en módulos más chicos, cada uno un mecha completo a la siguiente escala, y la tripulación salta de uno a otro por enlace neural. Para nosotros significa:
  - cada cambio de nivel es un **salto** (separación de módulos + destello o fundido), no un encogimiento continuo
  - desde la escala celular la cabina ya no es física: el jugador ve una **reconstrucción** (p. ej. una cabina con aspecto holográfico)
- **Realismo: el que funcione mejor en el juego.** Partimos de los datos de Investigación y exageramos lo necesario para que se entienda y no maree.
- **Célula de llegada: célula plasmática.**

Falta por decidir (roles 4 y 6): **¿una sola escena con zonas o una escena por momento?** Propuesta: una sola escena con zonas, sin tiempos de carga y con fundidos más fáciles.

## Pendientes por rol

Cada rol termina cuando cumple su criterio de terminado, no cuando “ya casi”.

### 1. Líder técnico e integración — Moisés

- [ ] Enviar hoy los requerimientos a Game Design (lista de controles de la cabina y storyboard), Diseño (reglas de exportación) e Implementación (modelo del visor)
- [ ] Asignar un nombre a cada rol y llenar `06-management/equipos.md`
- [ ] Crear y asignar un issue por rol en GitHub
- [ ] Pedirle a Investigación que suba su PDF a `01-investigacion/`
- [ ] Hacer hoy un APK de prueba vacío para detectar problemas de Android antes del sábado
- [ ] Revisar y fusionar los pull requests
- [ ] Sábado a mediodía: congelar `main` y armar el build integrado

**Terminado cuando:** todos abren el proyecto sin errores y el sábado existe un solo build con todo integrado.

### 2. Rig VR y cámara — Por asignar

- [ ] Cabina provisional con cubos: asiento, consola al frente y ventanas
- [ ] XR Origin con el origen en el asiento del piloto, modo sentado
- [ ] Quitar la locomoción libre (caminar y teletransporte): el submarino mueve al jugador, no el jugador
- [ ] Viñeta de comodidad durante el viaje (ya viene en el sample *Starter Assets*, carpeta `TunnelingVignette`)
- [ ] Botón para recentrar la vista
- [ ] Probar en el simulador que la cabeza y las manos se mueven y el jugador no sale de su lugar

**Terminado cuando:** en el simulador y en el visor el jugador ve la cabina desde el asiento, mueve la cabeza y las manos, y no se sale de su lugar.

### 3. Interacción de cabina — Por asignar

- [ ] Prefab de botón que se presiona con el dedo (poke) y dispara un `UnityEvent`
- [ ] Prefab de palanca que se agarra, con recorrido limitado, y dispara un evento al llegar al tope
- [ ] Pantalla en la cabina (Canvas en world space) que muestre la escala actual, p. ej. “Escala 1 : 1,000 · Vasos sanguíneos”
- [ ] Botones provisionales “Iniciar inmersión” y “Salto Matrioska” hasta que Game Design mande su lista
- [ ] Respuesta al presionar: cambio de color, vibración del control y sonido (mientras tanto, `Button Pop.wav` del sample)

**Terminado cuando:** al presionar “Salto Matrioska” en el visor, el evento llega a la secuencia y se nota la respuesta.

### 4. Secuencia y flujo — Por asignar

- [ ] `GameManager` con máquina de estados: Hangar → Abordaje → Viaje (3 saltos de escala) → Llegada
- [ ] Fundido a negro entre estados
- [ ] Conectar los eventos de los botones del rol 3 con los cambios de estado
- [ ] Atajo de depuración (teclas) para saltar a cualquier estado
- [ ] Decidir con el rol 6 si es una sola escena con zonas

**Terminado cuando:** se puede jugar de principio a fin sin tocar el editor y saltar a cualquier momento para probarlo.

### 5. Transición de escala y efectos — Por asignar

- [ ] Guardar los 4 niveles de la tabla como datos editables en el Inspector (nombre, tamaño, inercia, viscosidad, color del fluido, intensidad de empujones) para ajustarlos sin tocar código
- [ ] Efecto de cada salto Matrioska: los módulos del mecha se separan, destello o túnel de partículas y aparece la siguiente escala; nada de escalado continuo
- [ ] Al llegar a la escala celular, cambiar la cabina a su versión “reconstrucción” (material holográfico o semitransparente)
- [ ] La cabina se queda fija y lo que se mueve es el exterior
- [ ] Vasos sanguíneos: partículas rojas en forma de disco que fluyen con pulsos
- [ ] Citoplasma: partículas que se mueven lento y erráticamente. **Los empujones se aplican a lo de afuera, nunca a la cámara**, porque mover la cámara marea
- [ ] Revisar el checklist de mareo con Implementación

**Terminado cuando:** se entiende que el submarino se reduce en cada salto y nadie de Implementación reporta mareo fuerte.

### 6. Entornos, assets y build — Por asignar

- [ ] Hangar con placeholders: piso, paredes, luces y un submarino hecho de cápsulas
- [ ] Vaso sanguíneo con placeholders: túnel rojo y discos que fluyen
- [ ] Espacio celular con placeholders:
  - pared de membrana con cilindros incrustados (proteínas) y una malla detrás
  - cilindros gruesos (microtúbulos) y delgados (actina) cruzando el espacio
  - muchas esferas chicas (ribosomas)
  - al fondo, discos apilados (Golgi) y cápsulas (mitocondrias)
- [ ] Opcional: una cinesina caminando sobre un microtúbulo con una vesícula; es lo que más luce en la llegada
- [ ] Mandar a Diseño las reglas de exportación: FBX, 1 unidad = 1 metro, Y hacia arriba, pivote de la cabina en el asiento, controles como piezas separadas y ~100 mil triángulos en total para la escena
- [ ] Viernes: importar los FBX preliminares a `Assets/_BioVR/Modelos/` y hacer el primer APK en el visor
- [ ] Sábado: sustituir por los modelos finales y hacer el build final
- [ ] Llevar la lista de créditos en `CREDITOS.md`

**Terminado cuando:** los escenarios usan los modelos de Diseño y el APK se instala y abre en el visor.

## Calendario

| Día | Qué debe existir al final del día |
| --- | --- |
| Jue 8 oct | Roles asignados; requerimientos enviados; APK de prueba vacío |
| Vie 9 oct | Secuencia completa jugable con cubos; saltos de escala con efecto; los tres entornos con placeholders; FBX preliminares importados; primer APK en el visor |
| Sáb 10 oct | Modelos finales, errores corregidos y build entregado a Implementación. `main` se congela a mediodía |
| Dom 11 oct | Solo correcciones críticas del reporte de Implementación |
| Lun 12 oct | Presentación del prototipo |

Cada día, cada uno reporta en una línea: qué terminó, qué sigue y qué lo bloquea.

## Reglas de trabajo

1. Todos usan Unity **6000.3.26f1**.
2. Una rama por tarea (`rol3/boton-reduccion`) y pull request a `main`; nadie sube directo a `main`.
3. Cada quien trabaja solo en su carpeta de `Assets/_BioVR/` y en su escena de `Escenas/Pruebas/`. La escena principal solo la toca el rol 1.
4. Lo que vaya a la escena principal se entrega como prefab.
5. Siempre suban los `.meta` junto con su archivo.
6. Quien sube un script debe poder explicar qué hace.

## Para después de la primera entrega

El reporte de Investigación trae ideas que encajan con los minijuegos, pero no entran el sábado:
- Camuflaje con ácido siálico que se desgasta y hay que renovar, o el sistema del complemento ataca
- Pinzas de reconocimiento que solo agarran la molécula correcta, y pinza deslizante sobre el ADN
- Brazo que camina sobre microtúbulos como la cinesina
- Niveles de orgánulo y molecular
