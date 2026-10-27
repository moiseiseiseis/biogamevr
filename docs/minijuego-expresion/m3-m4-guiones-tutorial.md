# Guiones del tutorial · Expresión Genética

Minijuego Expresión Genética · School Redemption Game · Entregables M3 (controles) y M4 (conceptos) · Autor: Marcos

Versión revisada por Desarrollo (Moisés, Mo9) contra las fichas de Genaro (G2), 27 de octubre de 2026. Los cambios están en `revision-m3-m4-g2.md`; el guion original es de Marcos. Todos los términos son los del glosario.

## Reglas de escritura

- Cada paso sigue la secuencia **mostrar → hacer → confirmar**.
- Ningún texto en pantalla pasa de 2 líneas (unos 100 caracteres como máximo).
- Un solo cuadro de texto a la vez, con letra legible y sin mover la cámara. Ningún paso exige caminar.
- Regla de rigor: el jugador no une aminoácidos a mano, no corta el ARN y no busca un ARNt de paro.
- Elegir el ARNt que encaja es una simplificación didáctica. En la escena queda una etiqueta fija: “Escala didáctica”.

## M3 · Tutorial de controles

Objetos neutros (cubos), sin biología. Son 5 pasos.

| Paso | Qué ve | Qué dice la cabina | Qué hace el jugador | Confirmación |
| --- | --- | --- | --- | --- |
| 1. Apuntar | Un cubo neutro flota frente al jugador; la mano muestra un rayo | “Apunta con tu mano al cubo. Se va a iluminar.” | Apuntar al cubo | El cubo se ilumina y suena un clic suave |
| 2. Agarrar | El cubo iluminado | “Mantén presionado el botón de agarre para acercarlo.” | Mantener el botón de agarre | El cubo llega a la mano y se queda sujeto; el control vibra un poco |
| 3. Rotar | El cubo en la mano, con una cara marcada | “Gira tu muñeca. Busca la cara marcada.” | Rotar el cubo hasta ver la cara marcada | La cara marcada se resalta |
| 4. Soltar | Una base luminosa frente al jugador | “Suelta el botón sobre la base para dejar el cubo.” | Soltar el cubo sobre la base | El cubo se posa en la base y brilla |
| 5. Seleccionar | Tres cubos sobre la mesa, uno con un marcador | “Con el gatillo, elige el cubo con marcador.” | Apuntar y presionar el gatillo sobre el cubo correcto | El cubo elegido se resalta y suena un acierto. Si no era: “Casi. Prueba otro.” |

**Cierre:** “Listo. Ya sabes usar tus manos.” Pasa al tutorial de conceptos.

### Qué revisar en la prueba T1 (APK de cubos)

- ¿Se agarra, rota y suelta sin trabarse?
- ¿Se lee el texto desde la posición del jugador?
- ¿Se distinguen el rayo y el resaltado del cubo?
- ¿Se entiende cada paso sin explicarlo?

## M4 · Tutorial de conceptos

Secuencia de la escena: AUG – GGC – UUU – UAA. El ribosoma trabaja un ciclo: el jugador hace uno **guiado** (paso 4) y uno **sin ayuda** (paso 5). Son 6 pasos.

| Paso | Qué ve | Qué dice la cabina | Qué hace el jugador | Qué desbloquea |
| --- | --- | --- | --- | --- |
| 1. Ribosoma y ARNm | El ribosoma y la cinta de ARNm con sus codones legibles; etiqueta “Escala didáctica” | Al llegar: “Este es un ribosoma. Lee la copia de la receta (ARNm) de tres en tres letras.” Al acertar: “Muy bien. La receta se lee en orden, de un extremo al otro.” | Seleccionar el ribosoma y luego el ARNm | Se resalta el primer codón |
| 2. Codón | AUG resaltado en el sitio de lectura del ribosoma | Al llegar: “Tres letras forman un codón. Este es AUG: la señal de inicio.” Al acertar: “Casi cada codón pide un aminoácido; algunos son señales de paro.” | Seleccionar el codón AUG | Aparecen 4 ARNt flotando cerca |
| 3. ARNt y anticodón | 4 ARNt con el anticodón visible, cada uno con su aminoácido | Al llegar: “Cada ARNt trae un aminoácido. Su anticodón encaja con el codón que le toca.” Al inspeccionar: “Una enzima ya le puso su aminoácido. Tú no lo unes.” Al terminar: “En la célula los ARNt llegan al azar; aquí tú los acercas.” y luego “Ahora busca el que encaja con AUG.” | Atraer, sujetar, rotar e inspeccionar los 4 ARNt (sin errores posibles) | Se puede acoplar un ARNt |
| 4. Inicio (guiado) | El ARNt correcto parpadea suave; el sitio de lectura está en AUG | Al llegar: “Acerca el ARNt que encaja con AUG.” Al acertar: “¡Encaja! Es el ARNt de Met, el iniciador.” Si no encaja: “No encaja. Mira su anticodón y prueba otro.” | Acoplar el ARNt de Met en AUG | El ribosoma avanza a GGC |
| 5. Elongación (sin ayuda) | El sitio de lectura en GGC; sin pistas; los ARNt disponibles | Al llegar: “Ahora tú: busca el ARNt que encaja con GGC.” Al acertar: “El ribosoma une Gly a Met con un enlace peptídico.” Si no encaja: “No encaja. Mira su anticodón y prueba otro.” | Acoplar el ARNt de Gly en GGC | La cadena crece y el ribosoma sigue |
| 6. Terminación | El ribosoma avanza solo por UUU (se agrega Phe) hasta UAA, resaltado según la hoja de convenciones | En UUU: “El ribosoma sigue solo: UUU pide Phe.” En UAA: “UAA es una señal de paro. Ningún ARNt encaja aquí.” Al acertar: “Entra un factor de liberación y la cadena se suelta.” | Seleccionar el codón UAA | La cadena Met – Gly – Phe se suelta y termina el tutorial |

**Cierre:** “Así se lee una receta y se arma una cadena de aminoácidos.” Pasa a la misión.

## Decisiones por confirmar

- ~~**Botones:**~~ Resuelto por D6: el grip agarra y el gatillo selecciona o señala un codón, igual que en la mesa de prueba (Mo2).
- **UUU automático:** en el paso 6 propongo que el ribosoma pase solo por UUU para no alargar el tutorial. Confirmar con Iker (I4) y Moisés.
- **Pistas:** propongo una pista suave si pasan 10 segundos sin acción. El informe pide que las pistas sean opcionales.
- ~~**Revisión científica:**~~ Hecha (Mo9, 27 oct): se agregó la línea de difusión al paso 3; el texto del paso 5 queda igual. Ver `revision-m3-m4-g2.md`.
- **Legibilidad:** los textos están contados para no pasar de 2 líneas, pero hay que confirmarlo en el Quest durante T1 y T2.
