# Desarrollo: análisis del aporte BioMecha (cañón molecular)

Equipo de Desarrollo · Documento para decidir la integración · **jueves 22 de octubre de 2026**
Fuente: `BioMecha_Base` (aporte de un compañero de **Diseño**: carpeta `Scripts/` con 15 scripts, unas 2,700 líneas, y un `LEEME.txt`), comparado con `main` = `a8947f5` de `github.com/moiseiseiseis/biogamevr`.
Copia de trabajo para probarlo: proyecto aparte de Moisés (Unity 6000.3.26f1, plantilla 3D URP), **fuera del repo**.

> **Para qué es este documento.** Todavía no se integra nada a BioVR. Aquí se explica qué trae el aporte, qué se puede aprovechar, qué no funcionaría en el Quest y qué hay que decidir antes de llevarlo al juego.

---

## 1. Resumen en tres líneas

- Es un **prototipo jugable de escritorio** con una mecánica central muy buena: un **cañón molecular** al estilo de la *gravity gun* de Half-Life 2 (jalar, sostener, lanzar y empujar objetos con física).
- Trae dos niveles generados por código. El de **"Célula atacada"**, en el que hay que arrancar virus pegados a una célula, encaja casi perfecto con el **espacio celular de llegada** de BioVR.
- El **núcleo de la mecánica se puede reutilizar** casi tal cual. Lo que rodea al núcleo (generación del mundo, cámara, post-proceso, HUD, calidad visual) está hecho para PC y **no puede ir así al Quest**.

---

## 2. Qué contiene

### 2.1 Mecánica: el cañón molecular

| Acción | Escritorio | Equivalente natural en VR |
|---|---|---|
| **Jalar** (mantener): atrae el objeto apuntado y lo deja flotando enfrente | Clic derecho | Grip del mando |
| **Lanzar**: dispara lo que sostienes | Clic izquierdo | Gatillo |
| **Empujar**: si no sostienes nada, empuja lo que apuntas | Clic izquierdo | Gatillo |
| **Distancia** del objeto sostenido | Rueda del mouse | Stick arriba/abajo |

Detalles que lo hacen sentir bien: asistencia de puntería (`SphereCast`), rayo animado con ruido, luz en la boca del cañón, retroceso del modelo, resaltado del objeto apuntado y "se atoró contra la pared → se suelta".

### 2.2 Niveles

| Nivel | Objetivo | Piezas |
|---|---|---|
| **Torrente** (`NivelPlataforma`) | Llevar 8 átomos a un **receptor** al fondo | Átomos con electrones orbitando, glóbulos rojos que arrastra la corriente, **leucocitos** que solo se pueden empujar, corriente sanguínea global. Dos estilos de mapa: *Plataforma* (modelo de Meshy) o *Vaso* (tubo generado) |
| **Célula atacada** (`NivelCelula` + `Virus`) | Arrancar 8 de 12 virus | Célula animal translúcida (núcleo, mitocondrias, microvellosidades) que gira y "respira"; virus con espigas. Al jalar un virus ~0.9 s se estira, tiembla y se desprende. Al llegar a 8 la célula **sana**: se pone verdosa y los demás virus se desvanecen |

### 2.3 Ambiente y calidad visual

- **`EntornoCarnoso`**: cueva orgánica cerrada (paredes irregulares, textura de carne generada, tentáculos, venas, pústulas que respiran, esporas, luces rojas y verdes).
- **`Brillo`**: sliders de luz ambiente, intensidad de luces, niebla y exposición, para ajustar en vivo.
- **`CalidadVisual`**: sube la resolución interna, MSAA x4, SMAA, texturas al máximo y relieve fino en la cueva.
- **`PlataformaSetup`** (editor): menú *BioMecha → Configurar plataforma*, que prepara el modelo de Meshy (normal map, metálico/rugosidad para URP y un mapa de emisión). **El modelo (~100 MB) no viene en el zip.**

### 2.4 Mapa de scripts

| Script | Qué hace | ¿Reutilizable en BioVR? |
|---|---|---|
| `MolecularCannon` | Cañón: estados Idle/Pulling/Holding, rayo, lanzar y empujar. **No lee el teclado**: expone `SetGrab()`, `Fire()`, `AdjustHold()` y eventos `OnGrab`, `OnLaunch`, `OnFire` | ✅ **Sí**, es el corazón. Hay que limitar la capa del rayo |
| `Grabbable` | Reglas por objeto (se agarra o solo se empuja, peso al lanzar, corriente, temblor browniano, resaltado, absorción) | ✅ Sí |
| `Virus` | Virus pegado que se desprende al jalarlo | ✅ Sí (usa `Camera.main`; el XR Origin ya tiene `MainCamera`) |
| `NivelCelula` | Construye la célula y los virus y lleva el conteo | 🟡 La lógica sí; la construcción en Play conviene pasarla a prefab |
| `Receptor` | Meta que absorbe átomos | 🟡 Solo si se usa el nivel de átomos |
| `Procedural` (`Mats`) | Materiales y mallas por código | 🟡 Con cuidado (ver §3) |
| `NivelPlataforma` | Genera todo el nivel 1, el jugador y el post-proceso | ❌ No (ver §3) |
| `DesktopPlayer`, `GameInput` | Movimiento y controles de teclado y mouse | ❌ Solo para probar en PC (lo dice su autor) |
| `GameHUD`, `CellHUD` | HUD con IMGUI y panel de ajuste (TAB) | ❌ No se ve en VR |
| `EntornoCarnoso` | Cueva de carne | 🟡 Idea visual valiosa, demasiado pesada tal cual |
| `Brillo`, `CalidadVisual` | Ajuste de luz y nitidez | ❌ No para el Quest |
| `Editor/PlataformaSetup` | Prepara el modelo de Meshy | 🟡 Solo si se adopta ese modelo |

---

## 3. Por qué no puede entrar tal cual al Quest

| # | Problema | Dónde | Consecuencia en BioVR |
|---|---|---|---|
| P1 | Al arrancar **destruye todas las cámaras, luces y Volumes** de la escena y crea su propio jugador | `NivelPlataforma.Build()` | Borraría el XR Origin, la iluminación y el ScreenFader |
| P2 | **Modifica el URP Asset en vivo**: render scale 1.25, MSAA x4, SMAA, filtrado anisotrópico forzado | `CalidadVisual` | Caída fuerte de FPS en el Quest 2 y riesgo de dejar cambiado el asset del proyecto |
| P3 | Post-proceso (bloom, viñeta, ACES), membrana **transparente**, niebla exponencial, luces puntuales y spot | `NivelPlataforma`, `NivelCelula`, `Brillo`, `EntornoCarnoso` | Muy caro en GPU móvil; en VR la viñeta y el bloom además pueden molestar |
| P4 | Materiales con `Shader.Find("Universal Render Pipeline/Lit")` y `Sprites/Default` | `Procedural` | En el APK los shaders que ningún asset usa se quitan al compilar → objetos **rosas** |
| P5 | Mallas densas y texturas generadas en Play (célula de 40×64, cueva con sectores, 70 microvellosidades, 14 espigas × 12 virus = cientos de objetos) | `NivelCelula`, `EntornoCarnoso` | Muchas *draw calls* y un pico de carga al entrar al nivel |
| P6 | HUD con **IMGUI** (`OnGUI`) | `GameHUD`, `CellHUD` | No se dibuja en el visor. Hay que rehacerlo en world space con `UIVR` |
| P7 | El rayo apunta a **todas las capas** (`mask = ~0`) | `MolecularCannon` | Dentro de la cabina chocaría con el parabrisas o con el propio jugador |
| P8 | Clases **sin namespace** (`Grabbable`, `Receptor`, `Mats`…) | Todos | Posible choque de nombres. En BioVR todo va en `BioVR.*` |
| P9 | Tecla **R reinicia** el nivel y **Esc** suelta el mouse | `NivelPlataforma`, `DesktopPlayer` | Esc ya abre el menú de pausa en BioVR |
| P10 | Escalas en decenas de metros (célula de 6 m de radio, alcance de 18 m, plataforma de 64 m) | Varios | Hay que cuadrarlas con la tabla de escalas de Investigación ("Protocolo Matrioska") y con el tamaño del espacio celular de Diseño |
| P11 | Modelo de Meshy de ~100 MB | `PlataformaSetup` | Necesitaría **git-lfs**. Hay que revisar su **licencia** (términos de Meshy según el plan con que se generó) y cuántos polígonos tiene |

---

## 4. Cómo encaja con BioVR

**Flujo actual del juego:** Hangar → abordaje → viaje y reducción (vaso → capilar) → **llegada al espacio celular** → botón Iniciar → reinicio.
Hoy la llegada **no tiene objetivo jugable**: el jugador mira el espacio celular (modelo de Diseño con `EmpujonesMoleculares`) y reinicia.

**Lo que aporta BioMecha:** la **primera mecánica de juego real**. La misión "arranca los virus de la célula" le da un propósito al viaje y tiene sentido biológico (virus unidos a receptores de la membrana).

**Coincidencias con lo que ya existe:**

| En BioMecha | En BioVR | Nota |
|---|---|---|
| `Grabbable.GlobalFlow` (corriente sanguínea) | `FlujoPlaceholder` en `Entornos/Scripts` | Dos ideas de "corriente". Si un día se juega en el vaso, hay que unificarlas |
| `jitter` browniano de `Grabbable` | `EmpujonesMoleculares` | Mismo concepto (empujones moleculares) |
| Célula generada por código | `EspacioCelular.prefab` (Diseño) | Decidir si la célula con virus va **dentro** del espacio de Diseño o lo **reemplaza** |
| Minijuego de "arrastrar objetos" | Rama `game-design/minijuego-prototipo` (guía para subir el de Game Design) | Revisar que no se dupliquen esfuerzos entre Diseño y Game Design |

**Choque de controles en VR:**

| Botón | Uso actual en BioVR | Uso que pediría el cañón |
|---|---|---|
| Grip | Agarrar la palanca de la cabina | Jalar |
| Gatillo | Tocar botones / usar la UI con el rayo | Lanzar / empujar |
| Stick derecho | Girar por pasos | Distancia del objeto |
| B / Y | Volver al asiento | — |
| ☰ izquierdo | Menú de pausa | — |

Propuesta: el cañón solo se **activa en el espacio celular**. Al llegar se apaga el giro del stick derecho y la palanca ya no se usa, así el grip, el gatillo y el stick quedan libres para el cañón sin conflicto.

---

## 5. Decisiones pendientes

1. **¿Desde dónde se usa el cañón?**
   - **(a) Desde la cabina, sentado**, apuntando con el mando por el parabrisas: el cañón es un brazo del mecha. Es lo más coherente con la historia, aprovecha la cabina y el asiento, y **no marea**. *← Recomendado.*
   - **(b) Fuera del mecha**, con el traje del explorador, flotando en el citoplasma. Es más libre, pero pide locomoción 3D (subir y bajar), con más riesgo de mareo y más trabajo.
2. **¿Qué nivel se adapta?** Se recomienda **solo "Célula atacada"**. El de átomos y receptor queda para después.
3. **¿La célula va dentro del espacio celular de Diseño o lo reemplaza?** Esto lo decide Diseño junto con Game Design.
4. **¿Qué pasa del ambiente?** `EntornoCarnoso` es muy vistoso. Se puede pedir a Diseño un modelo ligero inspirado en él, en vez de generarlo en Play.
5. **¿Se adopta el modelo de Meshy?** Depende de la licencia, el peso y los polígonos.
6. **Crédito:** registrar al autor en `CREDITOS.md` y como coautor en los commits cuando se integre.

---

## 6. Plan de integración propuesto (por fases)

**Fase 0 — Probarlo aparte (hecho el 22 oct).** Proyecto de Unity aparte con los scripts tal cual, para jugarlo en PC y entender la mecánica. No toca el repo. **Probado el 23 oct: funciona bien.**

**Fase 1 — Núcleo aislado en BioVR** (rama `rol1/canon-molecular`, sin tocar `00-EscenaInicial`)
- Copiar `MolecularCannon`, `Grabbable`, `Virus` y `Mats` a `Assets/_BioVR/Minijuegos/Prototipos/CanonMolecular/`, con namespace `BioVR.Minijuegos`.
- Crear la capa **"Manipulable"** y limitar el rayo a ella (P7).
- Materiales como assets `.mat` en vez de `Shader.Find` (P4).
- Escena de prueba propia (`Escenas/Pruebas/CanonMolecular.unity`) con XR Origin y XR Device Simulator.

**Fase 2 — Adaptarlo a VR**
- `CanonVR`: traduce las acciones de XRI (grip, gatillo, stick) a `SetGrab`, `Fire` y `AdjustHold`. El cañón apunta desde el mando derecho.
- HUD de virus (n/8) en world space con `UIVR`, y vibración del mando al jalar, al desprender y al lanzar.
- Célula y virus como **prefab** armado por un menú *BioVR → Armar misión de virus*: célula opaca, menos microvellosidades, virus con espigas combinadas en una sola malla, sin post-proceso.
- **Medir en el visor**: APK de prueba con Marcos y un objetivo de 72 FPS estables.

**Fase 3 — Conectarlo al flujo**
- Al llegar al espacio celular: activar el cañón, desactivar el giro y la palanca, y lanzar la misión.
- Paso de tutorial nuevo (`Tutorial.Notificar("canon")`) que explique jalar y lanzar.
- Al completar: célula sana → mensaje → botón Iniciar / reinicio (lo que ya existe).
- Se puede usar una versión menor del juego para la siguiente entrega.

**Queda fuera por ahora:** `NivelPlataforma`, `CalidadVisual`, `Brillo`, `EntornoCarnoso`, `GameHUD`, `CellHUD`, el nivel de átomos y el modelo de Meshy.

---

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| Rendimiento en el Quest 2 (física de muchos objetos, luces) | Medir en la fase 2 antes de conectarlo al flujo. Pocas luces, sin transparencias, mallas combinadas |
| Mareo al jalar y lanzar objetos rápido frente a la cara | Límite de distancia mínima de sostén. El jugador sentado (opción a) ayuda |
| Controles confusos al cambiar el significado del grip y el gatillo | Indicación visual al activarse el cañón y un paso de tutorial |
| Duplicar trabajo con Game Design (minijuego de arrastrar objetos) | Hablarlo con Game Design antes de la fase 1 |
| Escalas que no cuadran con la tabla de Investigación | Revisar con Investigación el tamaño de virus y célula frente al mecha |
