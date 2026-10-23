# Minuta 4 — Cartas sobre la mesa

Oct 23, 2026 · @Moisés

El juego tiene varias piezas que funcionan o están bien pensadas, pero todavía no cuentan una sola historia. Esta minuta no decide nada. Pone en la mesa lo que **existe hoy** (jugable, en papel o como idea), dónde **no amarra** y qué preguntas necesita responder el equipo de **storytelling/guion** para destrabar a los demás.

## 1. La premisa que acordamos (7 de octubre)

Es lo único de historia que está escrito y aceptado por todos (Minuta inicial y Minuta 2):

1. **Vehículo:** un mecha submarino con tripulación humana, brazos y extensiones (p. ej. pinzas), que se reduce de escala.
2. **Objetivo:** mantener la homeostasis del cuerpo humano.
3. **Conflicto:** mecanismos, estructuras, procesos u órganos están "desapareciendo". **El porqué nunca se definió.**
4. **Mecánica central:** el jugador **sustituye** lo que falta.
5. **Minijuegos:** cada sustitución es un minijuego.

Además: el mecha tiene tecnología para **evadir el sistema inmune**, y la reducción se apoya en ciencia ficción ("Protocolo Matrioska", documento de Investigación).

## 2. Lo que tenemos tangible

Leyenda: 🟢 jugable en el visor · 🟡 jugable en PC, fuera del juego · 📄 en papel · 💭 idea sin documento.

### 2.1 El recorrido base (Desarrollo · 🟢 en `main`, validado el 14 de octubre)

| Momento | Qué pasa hoy | Qué es real y qué es placeholder |
| --- | --- | --- |
| **Hangar** | El jugador ve el mecha (Nave Seraphim) y al explorador | Modelos de Diseño; el cuarto es de cubos |
| **Abordaje** | Fundido a negro y aparece en la cabina | Sin animación de abordaje |
| **Viaje** | Tres "Saltos Matrioska" desde la cabina: **vaso sanguíneo → capilar → citoplasma**, con túnel, destello y pantalla de escala | Vaso y capilar son **placeholders** (primitivas con flujo y latido); no hay modelos de Diseño |
| **Llegada** | El mecha llega a un espacio celular | Modelo `escenario-celular` de Diseño (~347 k triángulos) |
| **Fin** | Botón Iniciar → vuelve al hangar | — |

También funciona: cabina física (botones, palanca, pantalla de escala), cabina holográfica en el citoplasma, caminar dentro de la cabina y salir al hangar por la escotilla, menú de pausa (con altura del asiento y confirmación) y un tutorial básico de controles.

**Lo que no tiene:** ningún objetivo. El jugador viaja, llega y se acaba. **No hay misión, ni conflicto visible, ni minijuego.**

### 2.2 BioMecha — cañón molecular (aporte de Diseño · 🟡 proyecto aparte, fuera del repo)

- **Mecánica:** un cañón al estilo *gravity gun*: **atraer, sostener, lanzar y empujar** objetos con física, con asistencia de puntería.
- **Nivel "Célula atacada":** una célula animal con virus pegados. Al jalar un virus se estira y se desprende; con 8 arrancados la célula "sana".
- **Nivel "Torrente":** dentro de un vaso, llevar átomos a un receptor esquivando leucocitos (que solo se pueden empujar), con corriente sanguínea.
- Ambiente "carnoso" (cueva orgánica con tentáculos, venas, pústulas).
- **Estado:** funciona bien en PC. Para el Quest hay que adaptarlo (análisis completo en [`04-desarrollo/docs/2026-10-22-analisis-aporte-biomecha.md`](../../04-desarrollo/docs/2026-10-22-analisis-aporte-biomecha.md)).

### 2.3 Minijuego Expresión Genética (Investigación + Desarrollo · 📄, arranca hoy)

- **Informe conceptual v1.0** (sin aprobación de implementación) y **plan de trabajo** del 23 de octubre, con cortes C1–C5 y entrega a más tardar el miércoles.
- **Idea rectora:** el jugador **diagnostica** por qué una célula no produce bien una proteína. **No sustituye a las enzimas.**
- **Etapas:** transcripción → maduración del ARN → exportación nuclear → traducción. En esta entrega solo se construye la **traducción** (escena del ribosoma, secuencia AUG–GGC–UUU–UAA). Lo demás queda en papel.
- **Caso propuesto (por aprobar con Investigación):** β-globina en un **eritroblasto**, con **β-talasemia** como alteración.
- **Verbos del jugador:** atraer, sujetar, rotar y seleccionar ARNt.
- **Misión en papel:** "Una proteína ausente": alerta → evidencia → descarte de etapas → diagnóstico → cierre.

### 2.4 Lo demás que existe en papel o como idea

| Pieza | Estado | Dónde |
| --- | --- | --- |
| Protocolo Matrioska: tabla de escalas (vasos 1–5 mm → capilar ~1 µm → citoplasma 200–500 nm → orgánulo → molecular) | 📄 | `01-investigacion/mecha-reduccion-escala-v2.pdf` |
| Cuaderno **GDD #1** (36 páginas): hangar, exploración celular, bocetos de cabina, estructuras moleculares, vistas VR | 📄 | Lo cita el informe de Expresión Genética. **No está en el repo** |
| 5 minijuegos propuestos el 7 oct: ATP, **síntesis de proteínas**, ADN, citoesqueleto, mitosis | 💭 por validar | `02-game-design/README.md` |
| Minijuego de Game Design (arrastrar objetos con colisiones) | 🟡 en su proyecto | Guía para subirlo en la rama `game-design/minijuego-prototipo` |
| **Minijuegos transitorios** (uno en el torrente sanguíneo) | 💭 | Esta minuta, §4 |
| Pokédex biológico | 💭 sin decidir cómo se organiza | Minuta inicial |
| Guía de estilo (colores, animación, música) | 📄 **vacía** | `02-game-design/guia-de-estilo.md` |
| Storyboards y sketches | No están en el repo (carpetas vacías) | `02-game-design/` |

## 3. Dónde no amarra el rompecabezas

Son las tensiones que vemos al poner todo junto. Ninguna es culpa de un equipo: aparecen porque cada pieza se hizo por separado.

| # | Tensión | Pieza A | Pieza B |
| --- | --- | --- | --- |
| T1 | **¿Sustituir o diagnosticar?** | Premisa: el jugador *sustituye* lo que falta | Expresión Genética: el jugador *no sustituye* a las enzimas, diagnostica |
| T2 | **¿Por qué falla el cuerpo?** | Premisa: los mecanismos "desaparecen" (sin causa) | Expresión Genética: una **mutación** (β-talasemia). BioMecha: un **virus** |
| T3 | **¿Hay enfermedades?** | Pendiente desde el 7 oct | Las dos propuestas nuevas ya las usan (virus, talasemia) |
| T4 | **¿A qué célula llegamos?** | Hoy: un "espacio celular" genérico, al que se llega por vaso → capilar | El caso propuesto es un **eritroblasto**, que vive en la **médula ósea**, no en un vaso cualquiera. El glóbulo rojo maduro no tiene núcleo |
| T5 | **¿Cuántas escalas?** | El recorrido tiene 3 saltos y termina en el citoplasma (200–500 nm) | El ribosoma está en la escala **molecular** (decenas de nm): haría falta un salto más. La tabla de Matrioska ya la contempla |
| T6 | **¿Quién hace la acción y desde dónde?** | El jugador está sentado en la cabina; la regla anti-mareo dice que la cabina no se mueve | BioMecha: cañón en primera persona, sin cabina. Expresión Genética: manipular ARNt "con las manos". No está dicho si es el brazo del mecha, el explorador con traje o un holograma |
| T7 | **Dos mecánicas parecidas, dos nombres** | BioMecha: atraer / sostener / lanzar | Expresión Genética: atraer / sujetar / rotar / seleccionar. **Es casi el mismo verbo**: se podría construir una sola herramienta |
| T8 | **¿Cuál es la misión principal?** | BioMecha: limpiar virus de una célula | Expresión Genética: diagnosticar una proteína ausente. Las dos pasan "en la célula de llegada" |
| T9 | **El nombre del juego** | Repo y README: **BioGameVR** | Informe conceptual: **School Redemption Game** |
| T10 | **Lo que dice la cabina** | Hoy solo muestra escala y controles | Expresión Genética pide "textos de cabina" que guíen la misión: no hay voz ni personaje que los diga |

## 4. Minijuegos transitorios: el torrente sanguíneo

**La idea:** además del minijuego principal, pequeños retos *durante el viaje*, entre un salto y otro, para que el trayecto no sea solo mirar.

**Lo que ya tenemos para eso:**
- El **vaso sanguíneo ya existe en Unity** (salto 1): pared del vaso, glóbulos rojos y leucocitos que fluyen hacia el jugador, con latido. Son placeholders, pero el espacio, la zona y la transición están hechos.
- El nivel **"Torrente" de BioMecha** es, literalmente, un minijuego de torrente sanguíneo: corriente que arrastra objetos, leucocitos que bloquean el paso y una meta.
- La Minuta 2 ya dice que el mecha **evade el sistema inmune**: hay un gancho narrativo listo.

**Restricciones que hay que respetar:**
- El jugador va **sentado**. Lo que se mueve es el exterior, nunca la cabina ni la cámara (anti-mareo).
- Debe ser **corto** (1–2 minutos) y no puede bloquear el avance: si falla, el viaje sigue.
- Rendimiento del Quest 2: pocas luces, sin transparencias, pocos objetos con física a la vez.
- Tiene que tener sentido biológico (regla de rigor del plan de Expresión Genética).

**Opciones para discutir** (no decididas):

| Opción | Qué hace el jugador | Gancho biológico | Reutiliza |
| --- | --- | --- | --- |
| A. Evadir la vigilancia inmune | Desviar o esquivar leucocitos que se acercan al mecha | Los leucocitos patrullan la sangre; el mecha es un cuerpo extraño | Leucocitos "solo empujables" de BioMecha + vaso existente |
| B. Abrirse paso en la corriente | Apartar glóbulos rojos para avanzar contra el flujo | Flujo sanguíneo, densidad de eritrocitos | Corriente de `Grabbable` + `FlujoPlaceholder` |
| C. Recolectar en el camino | Atrapar algo que la misión principal necesita (evidencia, muestra) | Conecta el viaje con el diagnóstico de Expresión Genética | Cañón molecular |
| D. Navegación hacia el destino | Elegir la ruta correcta hacia la médula ósea | Resuelve T4: por qué el mecha va a un eritroblasto | Pantalla de escala / cabina |

## 5. Un hilo posible (solo para provocar la discusión)

Esto **no es una propuesta cerrada**, solo muestra que las piezas *pueden* encajar:

> La cabina detecta que la sangre del paciente transporta poco oxígeno (**alerta**). El mecha entra al torrente sanguíneo, donde debe **evadir a los leucocitos** (*transitorio, vaso*). Se reduce hasta los capilares de la **médula ósea** y entra a un **eritroblasto** (*llegada*). Un último salto lo lleva a la escala molecular, frente al ribosoma, para **diagnosticar** por qué la β-globina no se produce bien (*minijuego principal, Expresión Genética*). Las herramientas del mecha son el **cañón molecular** de BioMecha, adaptado a "atraer y sujetar".

Lo que este hilo resuelve: T2, T3, T4, T5 y T7. Lo que deja abierto: T1 (sustituir vs. diagnosticar), T6, T8 (qué pasa con la célula con virus) y T9.

## 6. Lo que necesitamos de storytelling/guion

El cuello de botella está aquí: Desarrollo e Investigación ya avanzan, pero sin estas respuestas cada equipo inventa su propia historia. Las ordenamos por qué tan urgente es para lo que se programa esta semana.

| # | Pregunta | Destraba | Urgencia |
| --- | --- | --- | --- |
| Q1 | ¿El jugador **sustituye** o **diagnostica**? (o ¿diagnostica y luego sustituye?) | T1, el loop de misión (Mo6, sábado 24) | 🔴 esta semana |
| Q2 | ¿Por qué falla el cuerpo? (¿mutación, infección, una causa común a todo el juego?) | T2, T3 | 🔴 esta semana |
| Q3 | ¿Quién habla en la cabina? (IA de la nave, tripulación, centro de control) y con qué tono | T10, textos de cabina (G4) y tutorial de conceptos (M4) | 🔴 esta semana |
| Q4 | ¿La célula de llegada es **una sola** para todo el juego o cambia por misión? | T4, T8 | 🟠 próxima semana |
| Q5 | ¿El jugador actúa desde la cabina, con el brazo del mecha, o sale con el traje? | T6, adaptación de BioMecha | 🟠 próxima semana |
| Q6 | ¿Qué lugar tiene la misión de los virus (BioMecha)? ¿Otra misión, un transitorio o se fusiona? | T8 | 🟠 próxima semana |
| Q7 | ¿Cuántos minijuegos transitorios y en qué escalas? | §4 | 🟡 cuando se cierre lo anterior |
| Q8 | Nombre del juego: ¿BioGameVR o School Redemption Game? | T9, cartel de la feria | 🟡 antes del cartel |

## 7. Acuerdos y siguientes pasos (por confirmar en la reunión)

- [ ] Presentar esta minuta a todos los equipos y repartir las preguntas Q1–Q8
- [ ] Storytelling/guion responde Q1–Q3 antes del corte C2 (sábado 24), porque el loop de misión depende de ellas
- [ ] Subir el cuaderno **GDD #1** al repo (`02-game-design/`) para que todos trabajen con la misma referencia
- [ ] Investigación revisa el caso propuesto (β-globina en eritroblasto) y si la ruta vaso → médula ósea es correcta
- [ ] Game Design elige una opción de transitorio para el torrente (§4) y dibuja un storyboard de una página
- [ ] Desarrollo no integra BioMecha a `main` hasta que se responda Q5 y Q6
