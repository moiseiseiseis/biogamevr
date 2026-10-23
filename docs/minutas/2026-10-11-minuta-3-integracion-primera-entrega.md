# Minuta 3: Integración de la primera entrega

Oct 10 y 11, 2026 · @Moisés · Equipo de Desarrollo

Sesión de integración del prototipo jugable. Se revisó y corrigió el código de cada rol, se armó la escena principal, se hicieron los entornos y efectos que faltaban, se integraron los modelos de Diseño y se generó el primer APK para el visor. **La entrega se aplazó un poco** para hacerlo bien; el APK ya está listo para probarse.

## Resultado

El juego ya se puede jugar de principio a fin en `00-EscenaInicial`:

**Hangar → Abordaje → Vasos sanguíneos → Capilar → Citoplasma → Llegada**

- El jugador va **sentado en la cabina de Diseño**, sin caminar ni teletransportarse, y puede **recentrar** la vista con B/Y.
- **Controles de la cabina:**
  - botón **Iniciar inmersión**: Hangar → Abordaje → Viaje;
  - botón **Salto Matrioska** y **palanca**: los tres saltos de escala y la llegada;
  - **pantalla** que muestra el nivel actual (p. ej. "Vasos sanguíneos · mecha ~1-5 mm").
- **Cada salto de escala** muestra un túnel de partículas, cierra la viñeta de comodidad, da un destello blanco y cambia de entorno. La cabina nunca se mueve, solo el exterior, para no marear.
- **Entornos:**
  - Hangar con la **nave Seraphim** y el **traje del explorador** (Diseño).
  - Vaso sanguíneo con glóbulos que fluyen con los latidos (placeholder).
  - Capilar con proteínas (placeholder).
  - **Escenario celular** de Diseño, con moléculas que tiemblan por los "empujones" del citoplasma.
- **En la escala celular, la cabina se vuelve una reconstrucción holográfica:** las paredes y el cristal desaparecen y lo demás se ve cian tenue (Protocolo Matrioska).
- **APK generado** para Quest, Quest 2, Quest Pro, Quest 3 y Quest 3S. Marcos lo va a probar en su Quest 2.

## Acuerdos y decisiones

- **Flujo de trabajo:** el equipo solo **crea su rama desde `main`, programa y manda su PR**. Moisés (rol 1) revisa, corrige, fusiona, mantiene `main` al día e integra todo en la escena principal.
- **Roles de Desarrollo** (registrados en `06-management/equipos.md`):

| Rol | Responsable |
| --- | --- |
| 1. Líder técnico e integración | Moisés |
| 2. Rig VR y cámara | Genaro |
| 3. Interacción de cabina | Iker |
| 4. Secuencia y flujo | Marcos |
| 5. Transición de escala y efectos | Ascencio (lo programa Moisés mientras Ascencio no tenga computadora) |
| 6. Entornos, assets y build | Moisés |

- **Una sola escena con zonas.** El `GameManager` lleva el índice de escala (0 Hangar, 1 Vasos, 2 Capilar, 3 Citoplasma) y los demás sistemas lo escuchan.
- **Función de cada control:** Iniciar inmersión antes del viaje; Salto Matrioska y palanca durante el viaje. Espacio y las teclas 1 a 4 siguen sirviendo en el editor para probar.
- **El explorador es el traje que usará la tripulación.** Se queda en el hangar.
- **Diseño sube sus modelos descomprimidos:** `.blend` a `03-diseno/fuentes/` y `.fbx` a `03-diseno/exports/`, por rama y PR. Las reglas están en `03-diseno/README.md`; los `.zip` están bloqueados.
- **El APK lo hizo Moisés** en lugar de Marcos, porque ya tenía integrado todo lo de Diseño.

## Lo que se integró (PRs fusionados)

| PR | Qué | Autor original |
| --- | --- | --- |
| #19 | Integración de roles 2 a 5 con correcciones y escena principal jugable | Genaro, Iker, Marcos, Ascencio |
| #20 | PDF del reporte de Investigación en `01-investigacion/` | Investigación |
| #21 | Entornos placeholder: hangar, vaso, capilar y espacio celular | Moisés (rol 6) |
| #22 | Efecto de salto Matrioska: túnel de partículas y destello | Moisés (rol 5) |
| #23 | Empujones moleculares y cabina holográfica | Moisés por Ascencio (rol 5) |
| #29 | Integrantes de Desarrollo y cómo subir los modelos de Diseño | Moisés |
| #30 | Palanca de la cabina con correcciones | Iker |
| #31 | Pantalla de escala, botones con nombre, viñeta y recentrar | Iker y Genaro |
| #32 | Modelos de Diseño: cabina, nave Seraphim, escenario celular y explorador | Ulises (Diseño) |
| #33 | Perfil de mandos Meta Quest Touch Plus (Quest 3) | Moisés |

**Issues cerrados:** #9 (proyecto y cabina), #10 (secuencia), #11 (pantalla y botones), #24 (viñeta y recentrar), #25 (palanca) y #27 (modelos de Diseño).

## Problemas encontrados y cómo se resolvieron

**En el código del equipo:**
- **Rol 2 (Genaro):**
  - Borró el ajuste de URP de Android y dejó marcas de conflicto de git en un `.meta`. Se restauraron.
  - Subió por error una carpeta `biogamevr/` con logs y temporales de un proyecto viejo. Se quitó y se bloqueó en `.gitignore`.
  - La viñeta no se cerraba: había que pedírsela al controlador de XRI.
  - El recentrar usaba una acción del simulador que en el visor no hace nada. Ahora usa B/Y reales.
- **Rol 3 (Iker):**
  - El botón tenía el componente de la mano en lugar del de botón, así que no se podía presionar.
  - La palanca con física salía volando. Se rehízo sin física: solo responde al movimiento hacia adelante y hacia atrás, hasta 45°.
  - Sus escenas de prueba no tenían manos (XR Origin) ni piso.
  - Subió unos 300 archivos de ejemplo de TextMesh Pro que no se usan; se quitaron.
- **Rol 4 (Marcos):** al entrar al viaje no se veía ninguna zona, y el fundido no tenía su propio quad. Falta `AbordajeTrigger` (#26).
- **Rol 5 (Ascencio):** los nombres de los scripts terminaban en un **carácter invisible** (copiados de un chat) y Unity no los compilaba.

**En los modelos de Diseño** (se corrigió al importar, sin tocar los archivos de Ulises):
- **Texturas no encontradas:** el FBX las buscaba en la carpeta de la computadora de Ulises, así que la membrana y varios orgánulos salían blancos. Ahora se asignan por nombre de archivo.
- **Emisión saturada:** Blender exportó la emisión hasta ×10 y todo se veía blanco. Se moderó.
- **Cabina holográfica:** las capas cian de la cabina de Diseño tapaban la vista del escenario celular. Ahora paredes y cristal desaparecen en esa escala.

**En git:**
- Se borró por error la rama de Iker con su palanca, y se recuperó desde el historial de GitHub. **Desde entonces, antes de borrar una rama se comprueba que no tenga commits nuevos.**

**En la laptop de Moisés:**
- Aparecen **puntitos blancos** en el Game view de Unity. No es del proyecto: es el driver de la Intel Iris Xe (de julio de 2024). Se está actualizando. En el visor no debería pasar, pero hay que confirmarlo.

## Herramientas nuevas en Unity (menú BioVR)

| Menú | Para qué |
| --- | --- |
| Build APK | Genera el APK para el visor (`Builds/BioVR.apk`) |
| Armar escena inicial | Armó la escena principal con el trabajo de cada rol (ya se usó) |
| Integrar controles de cabina | Pone y conecta botones, palanca, pantalla, asiento, viñeta y recentrar |
| Generar entornos placeholder | Entornos con primitivas. **No sobrescribe** los que ya tienen modelos de Diseño |
| Agregar efecto de salto / Agregar cabina holográfica | Efectos del rol 5 |
| Poner modelos de Diseño | Coloca la cabina, la nave, el explorador y el escenario celular |
| Revisar modelos de Diseño / Revisar materiales del escenario celular | Tamaño, triángulos, materiales y texturas de los FBX |

## Pendientes

**Desarrollo:**
- [ ] **Probar el APK en el Quest 2** (Marcos, issue #28). Revisar:
  - si va fluido en el escenario celular, que tiene ~347 mil triángulos de cerca;
  - si alguien se marea;
  - si se ven puntitos blancos;
  - si los controles se alcanzan.
- [ ] **`AbordajeTrigger`** (Marcos, issue #26): subirlo o descartarlo.
- [ ] Corregir lo que salga de la prueba en el visor.
- [ ] Grabar un video del recorrido en el simulador para la presentación (Win + Alt + R), después de actualizar el driver.
- [ ] Opcional: dar función a los controles decorativos de la cabina de Diseño (selector y tapa de escala, interruptor de luces, escotilla, pantallas laterales).
- [ ] Opcional: que el traje desaparezca al pasar a Abordaje, o usar sus guantes como manos.

**Otros equipos:**
- [ ] **Diseño (Ulises):** registrar en `CREDITOS.md` los modelos y texturas de **Meshy** del escenario celular (lisosoma; viole, green, polyr, smoot, rer_landing_platform).
- [ ] **Diseño:** ¿habrá modelos para el vaso sanguíneo, el capilar y el cuarto del hangar? Si no, se quedan los placeholders.
- [ ] **Diseño:** cerrar los issues #6, #7 y #8 (modelos ya entregados).
- [ ] **Investigación:** cerrar el issue #1 (el reporte ya está en el repo) y confirmar los valores provisionales de los 4 niveles de escala (inercia, viscosidad, empujones).
- [ ] **Game Design:** confirmar los nombres y funciones de los controles, y los colores.
- [ ] **Management:** completar `06-management/equipos.md` con los demás equipos. Recordar a chrisroiz, JupiterVega y andresflores2076-netizen que acepten la invitación al repo.

**Todos:**
- [ ] Leer el PR donde se integró su trabajo para poder **explicar su código** en la presentación: Genaro e Iker en #19, #30 y #31; Marcos en #19; Ascencio en #19 y #23.

## Documentos de esta sesión

Las bitácoras son notas personales del rol 1 y no están en el repo. El resto está en [`04-desarrollo/docs/`](../../04-desarrollo/docs/):
- *Bitácora de sesión — 2026-10-10 — Revisión de ramas*: revisión detallada del código de cada rama.
- *Bitácora de sesión — 2026-10-10 — Correcciones e integración*: bitácora técnica, paso a paso, del 10 y 11 de octubre.
- *Equipo de Desarrollo — Lo que falta para la primera entrega (10 oct)*: tareas del equipo con sus issues.
