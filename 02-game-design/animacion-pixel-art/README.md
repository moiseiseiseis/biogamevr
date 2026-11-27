# Animación pixel art de CELLVERSE

Video en bucle para proyectar durante la presentación del proyecto. Cuenta la [narrativa del juego](../narrativa/historia-del-juego.md) paso a paso, en pixel art y con diálogos en cajas de texto, como en los RPG de bolsillo. Solo hay imagen: no muestra código y no tiene sonido.

| Archivo | Para qué |
| --- | --- |
| [`cellverse-animacion-pixel-art.mp4`](cellverse-animacion-pixel-art.mp4) | Video de 1920 × 1080 a 30 cuadros por segundo. Se pone en el reproductor con la repetición activada |
| [`index.html`](index.html) | La misma animación en el navegador. Funciona sin internet y ya se repite sola |

## Cómo proyectarlo

- **Video:** abre el `.mp4` en pantalla completa con la repetición activada (en VLC: *Reproducción → Repetir*; en QuickTime: *Visualización → Repetir*).
- **Navegador:** abre `index.html` con doble clic. Haz doble clic sobre la imagen o pulsa `F` para la pantalla completa. Otras teclas: `Espacio` pausa y `←` / `→` cambian de escena.

## Guion (unos 5 min 30 s por vuelta)

| # | Escena | Qué se ve |
| --- | --- | --- |
| 0 | Título | Logo de CELLVERSE con el hexágono, el nombre clave *School Redemption Game* y el Seraphim cruzando el cielo |
| 1 | Prólogo: la Tierra | El planeta gira y se va secando mientras la narración cuenta el cambio del clima |
| 2 | Ciudad bajo el domo | Las ciudades se protegen y nace el Proyecto CELLVERSE |
| 3 | La anomalía | En un monitor, cadenas de amilina se agrupan y salta la alerta; después aparece el plano del Mecha Seraphim |
| 4 | El hangar | La Operadora entra y habla con la Investigadora frente al Seraphim mientras los técnicos trabajan |
| 5 | Sala de enlace | La Operadora en la consola; ASTRA se presenta y carga la ruta sobre el cuerpo holográfico |
| 6 | Sala de reducción | Los anillos giran y el Mecha se reduce de 5 m a 3 µm |
| 7 | El paciente | Ulises en la camilla. El Mecha entra por el catéter de la vena mediana cubital y la pantalla se pone en blanco |
| 8 | Torrente sanguíneo | Eritrocitos, plaquetas y un leucocito. Se ven las fichas del registro biológico y la alerta por la corona de proteínas |
| 9 | Ruta por el cuerpo | Mapa holográfico de ASTRA: corazón, pulmones, aorta y páncreas. Segundo ajuste de escala, a 1 µm |
| 10 | Célula beta | Endocitosis, endosoma con el pH bajando y escape al citosol |
| 11 | Citosol | Ribosoma en el retículo, ARNm con el codón AUG, corriente de ARNt, sintetasa con manivela y la cadena de amilina que crece |
| 12 | La variante S20G | Comparación de las 37 posiciones, el codón AGC que pasa a GGC y fibras amiloides que crecen más rápido en la variante |
| 13 | Cierre | Registro guardado, primera etapa completada y la pregunta final |
| 14 | Continuará… | El Seraphim cruza las estrellas y el bucle vuelve al título |

## De dónde salen los personajes y los modelos

- **Operadora del Mecha:** hoja «Mecha Operator» de Game Design y modelo `operador-mecha` (cabello negro corto, diadema con micrófono, túnica verde olivo y arnés).
- **La Investigadora:** hoja de `sketches/personajes/02-investigadora` (bata clara, gafas oscuras, cabello rizado recogido y tableta).
- **ASTRA:** hoja de `sketches/personajes/01-ia-astra`. Se usan sus estados de reposo (ámbar), alerta (rojo) y análisis (rejilla), su paleta y la frase «No estás solo. Yo veo lo que tú no ves» (en la animación, «sola», porque habla con la Operadora).
- **Ulises, el paciente:** hoja de `sketches/personajes/04-paciente` y modelo `paciente-en-camilla`.
- **Mecha Seraphim:** modelo `seraphim-mecha` (casco blanco, visor negro, élitros redondos, placas superiores, cables, aleta-cuchilla, patas y brazos).
- **Logo:** modelo `pasarela-logo-cellverse` (hexágono rosa con borde amarillo).
- **Ciencia:** la secuencia de la amilina madura y su codón 20 (AGC) salen de `01-investigacion/proteina-amilina/secuencia_amilina_NM_000415.3.fasta`.

## Para cambiar algo

Todo está en `index.html`. El nombre del juego y el nombre clave son las constantes `GAME` y `CODENAME`, al principio del script. Los diálogos de cada escena están en `SCENES`, como pares `[personaje, texto]`. La duración de cada línea se calcula según su largo.

Para volver a sacar el video, sirve la carpeta con un servidor local que reciba los cuadros y abre `index.html?export=1&fps=30`. La página manda cuadros de 320 × 180 en RGBA a `/frames` y ffmpeg los escala ×6 con `flags=neighbor`, para que los pixeles queden nítidos.
