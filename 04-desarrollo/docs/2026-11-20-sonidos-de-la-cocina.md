# Sonidos de la Cocina

Qué audio va en cada clave de sonido de la Cocina. Sirve para llenar `SonidosCocina.asset` (M8, especificación 10.6).

Los audios salen de `09_Listo_para_Unity`, en la carpeta de Drive del equipo de Sonido (`B_Sonora_BioVR`). Todos están **Aprobados** en la plantilla de banda sonora. Están en `Minijuegos/Cocina/Audio/` (SFX, MUS, AMB y UI). Al importarlos, `Editor/AjustesAudioCocina.cs` les aplica la configuración del Quest:

- **Música y ambiente:** Vorbis en streaming.
- **Efectos e interfaz:** Vorbis, descomprimidos al cargar.

## Clave → audio

Las claves las lanza el núcleo (`ControlCocina.Sonar` → `EventosCocina.OnSonido`), salvo `tic`, que lanza la sintetasa (M3), y `lanzar`, que lanza el vuelo (M2).

| Clave | Cuándo suena | Audio |
|---|---|---|
| `clic` | Agarras un ARNt, entra a la sintetasa o entra al sitio A | `SFX/SFX-MIN-02_tomar-arnt_v01` |
| `tic` | Manivela de la sintetasa, cada 1/8 de vuelta | `SFX/SFX-MIN-03_manivela_v01` (ver la nota 1) |
| `cargado` | La sintetasa terminó de cargar bien | `SFX/SFX-MIN-07_siguiente-codon_v01` |
| `malo` | La sintetasa puso un aminoácido equivocado | `SFX/SFX-MIN-06_seleccion-incorrecta_v01` |
| `error` | Un rechazo que cuenta como error | `SFX/SFX-MIN-06_seleccion-incorrecta_v01` |
| `sacude` | Sacudes un ARNt cargado | `SFX/SFX-MIN-04_sacudir_v01` (CC BY 3.0: va en los créditos) |
| `llave` | El ARNt encaja al girarlo en el ribosoma de mesa | `SFX/SFX-MIN-05_acople-clic_v01` |
| `acierto` | Se une un aminoácido, o encuentras la cuenta 20 | `SFX/SFX-MIN-08_aminoacido-incorporado_v01` |
| `escala` | Tocas FIN y la cadena sale al holograma | `SFX/SFX-MIN-01_aparece-modelo_v01` |
| `pliega` | La proteína normal terminó de doblarse | `SFX/SFX-MIN-09_modelo-repliega_v01` |
| `estrella` | Final del turno | `MUS/MUS-MIN-05_ejercicio-completado_v01` |
| `alarma` | Aparece la horquilla o el codón raro | `UI/UI-ALR-04_aviso-critico_v01` (ver la nota 2) |
| `cae` | El ARNt regresa a la corriente | `UI/UI-NAV-03_cancelar_v01` |
| `lanzar`, `recicla`, `golpe`, `pop`, `torcida`, `chaperona`, `tritura` | (especificación 10.6) | **Sin audio todavía**: hay que pedirlos a Sonido |

Fuera de las claves:

- **Música de fondo:** `MUS/MUS-MIN-01_traduccion-loop_v01`. Es un loop de 24 s a 120 BPM y suena toda la Cocina.
- **Ambiente:** `AMB/AMB-CEL-01_interior-celular-loop_v01`, unos 6 dB más bajo que en la llegada a la célula (fila AMB-MIN-01 de la plantilla).
- `UI/UI-NAV-02_confirmar_v01`: para los botones de los paneles de M8 (Entendido, Siguiente turno, Repetir).

## Pendientes

1. **`tic`:** el audio de la manivela son 3 clics de matraca y la plantilla dice que suena **una vez por vuelta**. La sintetasa pide `tic` 8 veces por vuelta. Hay dos opciones: que el reproductor lo suene solo cada 3 o 4 `tic`, o pedirle a Sonido un clic suelto.
2. **`alarma`:** UI-ALR-04 son 4 pitidos de "falla o peligro". Para un imprevisto puede sonar demasiado fuerte; probarlo en el visor.
3. **Créditos de Jesús:** la música, el error, confirmar, cancelar y la alarma dicen "procedencia por confirmar" en la plantilla (¿generados con IA? ¿con qué herramienta y licencia?). Están anotados así en `CREDITOS.md`. Hay que confirmarlo antes de la feria.
