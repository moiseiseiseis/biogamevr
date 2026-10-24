# Mo7 · Lista priorizada de interacciones

Minijuego Expresión Genética · Desarrollo (Moisés) · Cortes C2–C3, 24 de octubre de 2026 · **Borrador para revisión**

> **Criterio:** entra lo necesario para jugar la secuencia AUG–GGC–UUU–UAA de punta a punta en Quest 2 y no contradice la capa 3. Lo que no cabe antes del miércoles se corta y queda documentado. Cada interacción dice en qué se apoya ya en el proyecto, para estimar el esfuerzo.

## P0 · Sin esto no hay minijuego

| # | Interacción | Verbo del jugador | Feedback | Se apoya en | Quién |
| --- | --- | --- | --- | --- | --- |
| 1 | **Atraer un ARNt** que flota cerca del ribosoma | Apuntar con el rayo + grip | El ARNt vuela a la mano | Mesa de prueba (Mo2): `PiezaManipulable` | Moisés |
| 2 | **Sujetar y rotar** el ARNt para ver su anticodón | Mantener el grip + muñeca o stick | El anticodón queda a la vista | Mo2 | Moisés |
| 3 | **Colocar el ARNt** en el sitio A, frente al codón | Soltarlo en el sitio A | Correcto: encaja y aparece el aminoácido. Incorrecto: se rechaza y regresa | Mo2: `RanuraSeleccion` (clave = codón) | Moisés → Iker |
| 4 | **Avance de la secuencia**: el ribosoma corre un codón y la cadena crece | Ninguno (lo hace el ribosoma) | Animación simple del ARNm y de la cadena | Nuevo (I4) | Iker |
| 5 | **Codón de paro** (UAA en sano, UAG en paciente) | Identificarlo como paro (pendiente: ver Mo6) | Entra solo el factor de liberación y se suelta la cadena | Nuevo (I4) | Iker |
| 6 | **Textos de cabina** por paso: alerta, instrucción y cierre (máximo 2 líneas) | Leer | Letrero frente al jugador | `MesaPrueba` (letrero) | Genaro (G4) |
| 7 | **Tutorial de controles** con objetos neutros (5 pasos de M3) | Atraer, sujetar, rotar y soltar | Paso completado → siguiente | Mesa de prueba + `Tutorial` de BioVR | Moisés (Mo4) |

## P1 · Entra si da tiempo (C5)

| # | Interacción | Por qué no es P0 |
| --- | --- | --- |
| 8 | **Ciclo de muestra automático** (el ribosoma traduce solo un ARNm sano, sin el jugador) | Ayuda a entender (mostrar → hacer → confirmar), pero el ciclo guiado ya enseña lo mismo |
| 9 | **Pistas opcionales**: el anticodón correcto brilla después de 2 errores | Mejora la experiencia, pero no es necesaria para completar |
| 10 | **Diagnóstico con 3 botones** (Transcripción, Maduración y Traducción) | Cierra el loop de la misión; mientras no esté, el diagnóstico se dice en el texto |
| 11 | **Comparar las dos cadenas** (sana y del paciente) lado a lado | Es la evidencia del caso 3; se puede reemplazar por texto |
| 12 | **Aviso de escala didáctica** visible en la escena | Lo exige la regla de rigor; si no da tiempo, va como texto fijo en el letrero (eso sí es P0) |

## P2 · Se corta en esta entrega (queda en papel)

| # | Interacción | Motivo |
| --- | --- | --- |
| 13 | Medir la cantidad de ARN en el núcleo (evidencia del caso 1) | La transcripción queda en papel |
| 14 | Comparar el largo del ARNm sano y el del paciente (evidencia del caso 2) | La maduración queda en papel |
| 15 | Seguir el ARNm por el poro nuclear | La exportación queda en papel |
| 16 | Difusión simulada (los ARNt llegan y se van solos) | Es lo más fiel a la capa 3 ("el ribosoma no escoge"), pero complica atraerlos. Para esta entrega los ARNt flotan quietos y se aclara en el texto |
| 17 | Factores de elongación visibles | Rigor de capa 3, pero no cambia lo que hace el jugador. Se menciona en el glosario |
| 18 | Mover el mecha o la cabina dentro de la célula | Regla anti-mareo: el jugador va sentado |

## Reglas que ninguna interacción puede romper

- El jugador **no une aminoácidos a mano**: el aminoácido aparece cuando el ARNt correcto encaja.
- **No existe un "ARNt de STOP"**: en el codón de paro ningún ARNt debe encajar.
- El jugador **no corta el ARN** con tijeras (ni siquiera en papel).
- Lo que se exagera (escala, colores y ARNt quietos) se marca como didáctico.
