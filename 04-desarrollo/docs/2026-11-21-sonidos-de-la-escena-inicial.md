# Sonidos de la escena inicial

Qué audio suena en cada momento del hangar, el abordaje, el viaje y la llegada a la célula. Los audios salen de `09_Listo_para_Unity` (carpeta `B_Sonora_BioVR` del equipo de Sonido), con las entregas del 20 y el 21 de noviembre, y siguen la plantilla de banda sonora.

## Cómo está armado

- **Audios:** `Assets/_BioVR/Comun/Audio/` (MUS, AMB, SFX y UI). `AMB-CEL-01` y `UI-NAV-03` se toman de `Minijuegos/Cocina/Audio/` para no duplicarlos.
- **Ajustes para el Quest** (`Comun/Audio/Editor/AjustesAudioEscena.cs`, se aplican solos al importar):
  - música: Vorbis en streaming;
  - loops de ambiente y efectos: Vorbis comprimido en memoria, porque suenan varios a la vez;
  - efectos cortos e interfaz: Vorbis descomprimido al cargar;
  - los que suenan en 3D pasan a mono.
- **Reproductor:** `Comun/Audio/Scripts/SonidosEscena.cs`. Escucha los eventos que ya existen (GameManager, escotilla, palanca, botones de la consola, altura del asiento, menú de pausa y tutorial). No cambia ningún script de otro rol.
- **Armador:** menú **BioVR → Agregar sonidos a la escena inicial**. Pone el objeto `Sonidos` en `00-EscenaInicial` con todos los audios asignados. Para cambiar un volumen o un audio, se cambia en `Comun/Audio/Editor/AgregarSonidosEscena.cs` y se vuelve a correr.
- **Pausa:** el GameManager pausa todo el audio. Solo siguen sonando la música de pausa y la interfaz.

## Capas (loops que se cruzan con un fundido de 1.5 s)

Volumen en cada estado (0 = no suena). Escala: 1 vasos, 2 capilar, 3 citoplasma.

| Capa | Audio | Hangar | Abordaje | Viaje | Llegada | Escala |
|---|---|---|---|---|---|---|
| Zumbido de la nave | `AMB-GEN-02` | 0.35 | 0.35 | 0.35 | 0.35 | todas |
| Sistemas de la nave | `AMB-ABO-02` | 0.5 | 0.6 | 0.6 | 0.6 | todas |
| Hangar (apagado dentro de la cabina) | `AMB-HAN-01` | 0.7 | 0.3 | | | |
| Flujo sanguíneo | `AMB-VAS-01` | | | 0.6 | | 1 y 2 |
| Corriente | `SFX-ENT-03` | | | 0.5 | | 1 |
| Partículas del plasma | `SFX-ENT-02` | | | 0.7 | | 1 y 2 |
| Desplazamiento | `SFX-MEC-06` | | | 0.35 | 0.25 | todas |
| Antenas (estática) | `SFX-MEC-05` | | | 0.12 | 0.12 | todas |
| Interior celular | `AMB-CEL-01` | | | 0.6 | 0.6 | 3 |
| Música del viaje, tranquila | `MUS-VAS-01` | | | 0.4 | | 1 |
| Música del viaje, intensa | `MUS-VAS-03` | | | 0.4 | | 2 |
| Música en la célula | `MUS-MEN-01` | | | 0.35 | 0.35 | 3 |

## Momentos

| Cuándo | Audio |
|---|---|
| Al empezar en el hangar, y cada vez que se vuelve a él | `MUS-HAN-03` revelación del mecha |
| Entrar al abordaje | `MUS-ABO-01`, `SFX-CAB-01` cinturón (0.4 s después) y `SFX-CAB-02` pantallas (1.2 s después) |
| Primer salto (hangar → vasos) | `SFX-MEC-04` repliegue y `SFX-ESC-02` reducción |
| Salto 2 (1 µm), 3 (50 nm) y 4 (5 nm) | `SFX-ESC-04_reduccion-2-1um`, `3-50nm` y `4-5nm`. El de 5 nm ya está asignado; suena cuando exista ese salto. |
| Al terminar cada salto | `SFX-ESC-03` reducción completada |
| Llegar al citoplasma | `SFX-ENT-07` entrada a la célula |
| Cambio de estado sin salto (viaje → llegada, llegada → hangar) | `MUS-GEN-07`, las 4 transiciones en orden |
| Escotilla (salir y entrar) | `SFX-MEC-01` compuerta |
| Botones Iniciar inmersión y Salto Matrioska | `SFX-CAB-03`. Si el botón no aplica en ese momento, también `UI-FBK-04` no disponible. |
| Palanca al tope | `SFX-CAB-04` |
| Ajustar la altura del asiento | `SFX-CAB-06` matraca |
| Abrir y cerrar el menú de pausa | `UI-NAV-04` y `UI-NAV-03`; mientras está abierto, `MUS-GEN-02` |
| Pasar el rayo sobre un botón de panel / presionarlo | `UI-NAV-01` / `UI-INT-04` |
| Completar un paso del tutorial | `UI-FBK-01` acierto |

## Sonidos 3D

| Sonido | Audio | Dónde |
|---|---|---|
| Pantallas del hangar | `AMB-HAN-03` | En la torre del hangar (el hangar provisional no tiene pantallas) |
| Membrana | `AMB-CEL-02` burbujeo | En la membrana del espacio celular |
| Citoesqueleto | `AMB-CEL-02` crujidos | En el citoesqueleto (o en los microtúbulos o la actina) |
| Núcleo | `AMB-CEL-02` pulso | En el núcleo. **El espacio celular actual no tiene núcleo:** sonará cuando entre el escenario low poly (A4), si trae uno. |
| Herramientas del hangar | `SFX-INS-05` (4 martillos, 4 golpes de metal y la llave neumática) | Uno al azar cada 4 a 10 s, en un punto del hangar, durante el hangar y el abordaje |
| Glóbulos junto a la cabina | `SFX-ENT-01` | Cada 2.5 a 6 s, a 1.5–3.5 m de la cabina, en los vasos |
| Contacto con moléculas | `SFX-MEC-07` | Cada 8 a 18 s, a 1–2 m, en el citoplasma |

## Sin uso por ahora

No se importaron. Quedan en el Drive para cuando exista su momento en el juego.

- **Inserción vascular:** `MUS-INS-01`, `MUS-INS-02`, `AMB-INS-01`, `AMB-INS-02`, `SFX-INS-01` a `04` y `SFX-INS-05_aviso-altavoz`. En el juego no hay paciente ni catéter.
- **Sala de preparación y anillos:** `AMB-RED-01`, `AMB-RED-02`, `SFX-ESC-01`. Los anillos duran 19 s y el salto, 1.2 s.
- **Mecha:** `SFX-MEC-02` patas y `SFX-MEC-03` élitros.
- **Entrada a la célula:** `SFX-ENT-04` membrana, `SFX-ENT-05` aterrizaje y `SFX-ENT-08` endosoma. El juego entra a la célula con un salto.
- **Menú y generales:** `AMB-GEN-01` (no hay menú principal), `AMB-HAN-02` (no hay zona de plataforma), `SFX-CAB-05`, `MUS-GEN-04` fallo, `UI-ALR-02` advertencia, `UI-INT-01/02/03` (en la escena inicial no hay nada que agarrar) y `UI-FBK-03`.
- **Para la Cocina:** `MUS-MIN-02` poco tiempo. La Cocina no tiene reloj.
- **Para el flujo a la Cocina:** `MUS-GEN-01` carga. Se usará al hacer el salto a 5 nm.
- **Voz del prólogo:** `VOZ-NAR-01` (13 partes, 4.1 min). Esperando la decisión sobre el prólogo.

## Pendientes

1. Probar los volúmenes en el visor. Los graves (`AMB-ABO-02`, `SFX-ENT-02`) casi no se oyen en las bocinas del Quest.
2. Procedencia de los audios de Jesús y de los entregados el 21-nov (ver `CREDITOS.md`).
3. Al cambiar el espacio celular por el low poly (A4), revisar que la membrana, el citoesqueleto y el núcleo sigan encontrando su objeto. El armador busca por nombre; el registro dice cuáles no encontró.
