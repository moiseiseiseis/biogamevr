> **Nota de la subida al repo (02/11/2026).** Este documento se escribió con los nombres de la carpeta de trabajo. En el repo: `eritro_rig.blend` → `03-diseno/fuentes/nave-eritro.blend` (sigue en 1 u = 1 µm, texturas empaquetadas); `eritro_rig.fbx` → `03-diseno/exports/nave-eritro-micras.fbx` (sin cambios); el **export nuevo `03-diseno/exports/nave-eritro.fbx` / `.glb` está en metros** (8 m/µm aplicado: escala 1 en Unity, no 8), con las mismas 6 tomas y los 41 huesos; lo genera `scripts/nave-eritro/exportar-nave-eritro-metros.py` y su comprobación está en `nave-eritro-metros-check.json`. `eritro_rig_textures_1024/` → `03-diseno/exports/texturas/nave-eritro/`. Renders y video → `03-diseno/vistas/nave-eritro/`. Los comandos de abajo usan rutas de la PC de Ulises (ver `03-diseno/scripts/nave-eritro/LEEME.md`).
# Eritro: rig y animaciones (v4: cúpula de vidrio ancha)

Base: `eritro_meshy_20k.blend` (no se ha modificado). Todo se genera con `scripts/eritro_rig.py`, de forma reproducible y sin créditos de Meshy:

```
blender -b nave_eritro/modelo/eritro_meshy_20k.blend --python scripts/eritro_rig.py -- build        # piezas + rig + motor + acciones + Demo_Completa (v3) -> eritro_rig.blend
blender -b nave_eritro/modelo/eritro_rig_v2.blend    --python scripts/eritro_rig.py -- motor        # solo la etapa v3: añade el motor al rig v2 -> eritro_rig.blend
blender -b nave_eritro/modelo/eritro_rig_v3.blend    --python scripts/eritro_rig.py -- cupula       # solo la etapa v4: cúpula de vidrio + piel más baja -> eritro_rig.blend
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- punta        # solo para un v4 anterior al arreglo: entierra el borde delantero del collar (ya aplicado)
blender -b --factory-startup --python scripts/eritro_hoja_final.py                                  # eritro_hoja_final.png
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- check        # tris, interpenetraciones y camuflaje -> eritro_rig_check.json
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- render       # rig_*.png + rig_estados.png
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- comparacion  # render_comparacion_globulo.png
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- video        # eritro_animacion.mp4 / .gif
blender -b nave_eritro/modelo/eritro_rig.blend      --python scripts/eritro_rig.py -- export       # eritro_rig.fbx / .glb + reimportación
```

Respaldos: la v1 (piel procedural de 7,8 µm) está en `eritro_rig_v1.blend`, `.fbx` y `.glb`. La v2 (sin motor, flagelo pegado al platillo) está en `eritro_rig_v2.blend`, `.fbx` y `.glb`. La v3 (cúpula de Meshy, opaca dentro del atlas) está en `eritro_rig_v3.blend`, `.fbx` y `.glb`, con su comprobación en `eritro_rig_v3_check.json`.

## Cúpula de vidrio (v4)

La cúpula de Meshy era una lente negra de 0,45 × 1,43 µm pintada en el atlas del platillo: opaca y demasiado plana para meter la cabina del repo a menos de 12 m/µm. En la v4 se sustituye por dos piezas nuevas, más anchas, para que la cabina quepa a **8 m/µm** en el hangar (ver `nave_eritro/cabina/LEEME_cabina.md`):

| Pieza | Qué es | Material | Tris |
|---|---|---|---|
| `Cupula_Vidrio` | Lágrima de **0,74 × 1,42 µm** (antes 0,45 × 1,43: +64 % de ancho), cumbre en z = 0,615 (antes 0,589). Planta y perfil de superelipse (techo más lleno, para que quepan las esquinas de la cabina), cumbre en (0, −0,08) | `Cupula_Vidrio`: azul marino casi negro, alpha 0,35, capa brillante, transparente | 352 |
| `Cupula_Marco` | Collar blanco que entierra el canto del vidrio, con un filo morado (anillo de energía nuevo), y el pozo interior azul marino: pared y piso a z = 0,165. Mide 0,99 × 1,65 µm | `Eritro_Nave` (colores del atlas) | 352 |

- Las dos son hijas del armature, con pesos rígidos al hueso `Cuerpo`.
- **Platillo:** se cortó limpio por el contorno de la cúpula + 0,098 (el corte queda bajo el borde del collar). Se borró lo que había dentro: la cúpula y el collar de Meshy, el anillo morado interior (la cúpula ancha lo cortaba; el filo morado del collar lo sustituye) y lo que quedaba en el pozo. También se borraron 117 caras del borde delantero que asomaban por encima del collar. El platillo pasa de 8.737 a 7.852 tris.
- **Punta delantera del collar (arreglo del 02/11/2026):** en la punta, el platillo cae antes que el contorno de la cúpula, y el borde exterior del collar (la banda marina que se hunde en el casco) asomaba por fuera del morro como una faceta suelta. `enterrar_borde_marco` mueve los vértices de ese borde que no quedan al menos 0,006 µm dentro del platillo: los acerca a la fila anterior y los baja lo justo. Se movieron 7 de 32 vértices (el de la punta pasa de y = −0,965 a −0,950). La topología no cambia: siguen siendo 352 tris. Ya va incluido en la etapa `cupula`. Lo blanco y arrugado que se sigue viendo bajo el morro es textura de Meshy en un pliegue del platillo, que es una sola isla sin piezas sueltas.
- **Piel plegada más baja:** el pozo ocupaba el sitio de la piel plegada (z 0,007–0,277), así que la malla de la piel bajó 0,125 µm. El hueso `Piel` la vuelve a subir mientras crece: `location.y = (escala.y − 1) · 10 · 0,125 / 9`, con las mismas claves que su curva de escala, en `Petalos_Cerrar`, `Demo_Completa` y `Desplegado`. Comprobado en el FBX reimportado: plegada, el centro queda en z = 0,017; en el cuadro final, en z = 0,142, con 7,5 µm de diámetro y 2,70 de grueso, igual que en la v3.
- **Comprobado (`check`):**
  - Repetido después del arreglo de la punta. Cúpula, collar, motor y flagelo contra todas las demás mallas (70 pares), en los 449 cuadros (todas las tomas y `Demo_Completa`): **0 interpenetraciones**. Se excluyen a propósito collar-platillo y vidrio-collar, que van incrustados.
  - Las interpenetraciones de las muestras (rótulas de cadera y hombro, y bisagras de los pétalos contra el platillo) son las mismas que en la v3.
  - En el cuadro 60 ningún vértice queda fuera de la piel. El margen mínimo baja de 0,06 a **0,044 µm**, por la cumbre algo más alta.

## Motor flagelar (v3)

En lugar de una turbina a chorro (no empuja nada a 3 µm), el flagelo sale de un **motor rotatorio** como el del flagelo bacteriano, con aire de turbina retro de los 80. Está en el borde trasero del platillo, con el eje hacia atrás (+Y; el frente es −Y). Ver `render_motor_flagelar.png`.

| Parte | Material | Tris |
|---|---|---|
| Carcasa: anillo corto incrustado en el borde, 12 lados, banda marina, labio trasero y tapa delantera enterrada | `Motor_Nacar` (blanco nacarado: blanco del atlas, capa y película fina) + `Motor_Marino` (marino del atlas) | 154 |
| Anillo emisivo en el borde interior de la boca | `Motor_Emisivo` (#B04BFF, emisión 0,8) | (en la carcasa) |
| Estator: anillo escalonado al fondo de la boca | `Motor_Metal` + `Motor_Marino` | 48 |
| Rotor: buje de metal, cono nácar y gancho (nariz marina de la que sale el flagelo) | | 40 |
| Rotor: 10 aspas finas con paso de 50° a 30° | `Motor_Metal` (metal oscuro) | 40 |
| **Total del motor** | | **282** |

- **Perfil bajo:** el eje está a z = 0,12 y el radio exterior es de 0,158 µm, así que llega hasta z = 0,28, muy por debajo de la cúpula (0,615 en la v4). El motor sobresale hasta y = 1,13.
- **Presupuesto:** la nave ya tenía 19.859 tris, así que bajo 20.000 solo cabían 141 tris nuevos. Para hacer sitio, borré las **154 caras del platillo que quedaban enteras dentro de la carcasa** (el muñón del flagelo-aguja de Meshy, que ya no se ve). No reduje nada visible. Por eso el motor tiene 282 tris y no los 1.200 previstos: es facetado y de 12 lados, a propósito.
- **Flagelo:** se movió (0, +0,13, −0,024) para que nazca dentro de la nariz del buje. `Flagelo_01` ahora es hijo de `Motor_Rotor`, así que el flagelo gira con el rotor. Combinado con la onda de `Flagelo_Nadar`, hace un movimiento de sacacorchos.
- **Comprobado (`check`):** el motor y el flagelo no atraviesan pétalos, patas ni brazos en ningún cuadro: 449 cuadros revisados (todos los de `Patas_Contraer`, `Brazos_Contraer`, `Petalos_Cerrar`, `Flagelo_Nadar` y `Demo_Completa`). Se excluyen a propósito motor-platillo (va incrustado) y motor-flagelo (el flagelo nace dentro del buje). En el cuadro 60 ningún vértice queda fuera de la piel, con un margen mínimo de 0,06 µm.

## Ver la animación

Abre `eritro_rig.blend` y pulsa **Espacio**. El archivo se guarda con la Action `Demo_Completa` asignada a `Eritro_Rig`, la línea de tiempo en 0–300, la vista 3D en la cámara `Cam` y el sombreado Material Preview con las luces de la escena.

Si prefieres no abrir Blender, mira `eritro_animacion.mp4` o `eritro_animacion.gif`.

## Archivos

| Archivo | Qué es |
|---|---|
| `eritro_rig.blend` | Piezas, armature `Eritro_Rig`, 6 Actions con fake user (5 separadas y `Demo_Completa`), cámara `Cam` con acercamiento y 3 luces de área |
| `eritro_rig.fbx` | Armature (41 huesos), 22 mallas (con `Motor_Flagelar`, `Cupula_Vidrio` y `Cupula_Marco`) y 6 tomas, sin bake_space_transform. Texturas de 1024 incrustadas (9,7 MB). Reexportado tras el arreglo de la punta y comprobado al reimportar: 6 tomas, piel y rotor iguales |
| `eritro_rig.glb` | Lo mismo en glTF binario, con 6 animaciones (9,2 MB) |
| `eritro_hoja_final.png` | Hoja resumen: estados de la animación, primer plano del motor, corte con la cabina y escala en el hangar con el explorador |
| `render_motor_flagelar.png` | Primer plano del motor flagelar |
| `eritro_animacion.mp4` | Demo en video (regenerado el 02/11/2026: el anterior se había cortado en 392 cuadros): H.264, 1280×720, 30 fps, 453 cuadros (15,1 s, 1,6 MB) y tres tomas con rótulos: tres cuartos de frente (10 s, toda `Demo_Completa`; se ve la cúpula de vidrio), **trasera** mientras nada (2 s; motor girando y cúpula) y lateral del cierre (3 s) |
| `eritro_animacion.gif` | La misma demo a 480×270 y 10 fps (153 cuadros, 0,5 MB) |
| `render_comparacion_globulo.png` | Nave cerrada contra el glóbulo del torrente, con la misma cámara y las mismas luces. 4 vistas y la diferencia ×8 |
| `eritro_rig_comparacion.json` | Las cifras de esa comparación |
| `eritro_rig_textures/` | Texturas de la nave a 2048, más las del glóbulo (`eritrocito_*.png`) |
| `eritro_rig_textures_1024/` | Las mismas a 1024 para Quest. Nave: `base_color`, `normal`, `metal_rugosidad` (glTF: G = rugosidad, B = metálico) y `unity_metallic_smoothness` (R = metálico, A = suavidad). Glóbulo: `eritrocito_base`, `eritrocito_normal`, `eritrocito_rough` y `eritrocito_unity_metallic_smoothness` |
| `rig_*.png`, `rig_estados.png` | Renders de los estados |
| `eritro_rig_log.json`, `eritro_rig_check.json`, `eritro_rig_export_check.json` | Registro de la construcción, comprobaciones y reimportación |

## Escala: 1 unidad = 1 µm

| Estado | Medida |
|---|---|
| Platillo | 2,0 µm de diámetro, 0,85 µm de alto con la cúpula (cumbre a z = 0,615). El motor sobresale atrás hasta y = 1,13 |
| Cúpula de vidrio | 0,74 × 1,42 µm; sube 0,29 µm sobre la cubierta |
| En el hangar (8 m/µm) | Nave abierta de 26,9 × 24,6 × 9,7 m (34,7 m de largo con el flagelo); cerrada, 60 m de diámetro. Ver `nave_eritro/cabina/LEEME_cabina.md` |
| Motor flagelar | 0,32 µm de diámetro, 0,33 µm de largo (la mitad está enterrada en el borde) |
| Nave abierta | 3,4 µm de ancho (patas), 3,1 µm de largo sin flagelo y 4,3 µm con flagelo, 1,2 µm de alto |
| Pétalos cerrados | 2,4 × 2,3 × 1,05 µm (radio máximo de 1,22 µm) |
| **Cerrada (glóbulo rojo)** | **7,5 µm de diámetro, 2,70 µm en el borde y 1,00 µm en el centro: exactamente el `Eritrocito_Proto` del torrente** |

## Camuflaje = el glóbulo de la escena

- **Malla y material:** `Piel_Camuflaje` es una copia (append de datos, no de objetos) de la malla `Eritrocito_Proto` de `torrente_sanguineo/torrente_bloqueo.blend`. Trae su material `Eritrocito_Meshy`: texturas de Meshy, rugosidad y metálico en G/B y emisión 0,35 con el color base. Tiene 352 tris. Las imágenes están empaquetadas en el .blend, igual que en la escena, y además se copiaron a `eritrocito_*.png`.
- **Escala:** igual que en la escena, donde las 401 instancias usan escala 1. La malla se guarda a ×0,1 y escondida dentro del platillo, centrada en z = 0,142. El hueso `Piel`, hijo de `Raiz`, la escala ×10 en el último cuadro.
- **Encaje:** la altura de la piel se calculó lanzando rayos verticales sobre la malla real del glóbulo, buscando el máximo margen. Con la piel centrada en z = 0,142, la nave cerrada cabe entera **sin encogerla**, con un margen mínimo de **0,06 µm**. Si en el futuro no cupiera, el script encoge la nave (hueso `Cuerpo`) entre los cuadros 44 y 50.
- **Comprobación (`check`):** en el cuadro 60 de `Petalos_Cerrar`, cada vértice de la nave se prueba con 4 rayos y **ninguno queda fuera de la piel**.
- **Comparación (`render_comparacion_globulo.png`):** se renderizó la nave cerrada y, con la misma cámara ortográfica y las mismas luces, el glóbulo del torrente en el mismo sitio, en las vistas tres cuartos, lateral, arriba y abajo. La diferencia es de **0 en 0–255 en todos los píxeles** (máximo 1/255 por ruido de muestreo en arriba y abajo) y la silueta es idéntica. En el estado final no hay forma de distinguirlos.

## Piezas y tris (presupuesto: 20.000 sin la piel)

| Pieza | Tris |
|---|---|
| Platillo (cuerpo y fundas de cadera y hombro; ya sin la cúpula de Meshy) | 7.852 |
| Cupula_Vidrio / Cupula_Marco | 352 / 352 |
| Petalo_01 … Petalo_08 | 394–477 cada uno (3.517 en total) |
| Pata_L1, L2, L3, R1, R2, R3 | 820–970 cada una (5.407 en total) |
| Brazo_Pinza (−X) / Brazo_Cuchilla (+X) | 754 / 702 |
| Flagelo (hélice negra) | 588 |
| Motor_Flagelar (carcasa, estator, rotor y aspas) | 282 |
| **Total de la nave** | **19.806** (v3: 19.987) |
| Piel_Camuflaje (aparte: la malla del glóbulo del torrente) | 352 |

**El modelo tiene 8 pétalos, no 10.** Los dos huecos de Meshy, delante (brazos) y detrás (flagelo), no tienen pétalo. Numeración: el 01 está delante a la izquierda (+X) y se sigue en sentido antihorario visto desde arriba. Patas: L = +X y R = −X, con 1 = delante y 3 = detrás.

## Huesos (41, armature `Eritro_Rig`)

- `Raiz` → `Cuerpo` (platillo y carcasa del motor) y `Raiz` → `Piel`.
- `Cuerpo` → `Motor_Rotor`: buje y aspas. Gira sobre su eje Y, que es el eje del motor.
- `Petalo_01..08`: la cabeza está en la bisagra de la base del pétalo.
- `Pata_XN_Cadera` → `_Rodilla` → `_Pie` (6 × 3).
- `Brazo_Pinza_Hombro` → `_Codo` → `_Mano` y lo mismo para `Brazo_Cuchilla_*`.
- `Motor_Rotor` → `Flagelo_01..05`: cadena con pesos suaves. El flagelo cuelga del rotor.
- `Piel`: escala la piel de camuflaje (×10 al cerrar).

## Actions / tomas (30 fps)

| Action | Cuadros | Qué hace |
|---|---|---|
| `Desplegado` | 0–1 | Pose de reposo: abierta |
| `Patas_Contraer` | 0→30 | Las 6 patas se pliegan bajo el platillo |
| `Brazos_Contraer` | 0→30 | Los brazos se recogen bajo el borde delantero |
| `Petalos_Cerrar` | 0→60 | 0–24: patas, brazos y flagelo se recogen, y el rotor frena de 15°/cuadro a 0 (media vuelta). Se detiene en el cuadro 24, antes de que los pétalos cierren (32–46). 24–46: los pétalos se separan, suben y se cierran. 47–50: la piel crece en altura. 50–60: la piel se extiende hasta el glóbulo de 7,5 µm |
| `Flagelo_Nadar` | 0→24 | Onda lateral en bucle. El rotor da una vuelta exacta (15°/cuadro), así que el bucle no salta |
| **`Demo_Completa`** | **0→300 (10 s)** | 0–60 nadar · 60–90 Patas_Contraer · 90–120 Brazos_Contraer · 120–138 el flagelo se recoge · 138–174 Petalos_Cerrar (24→60) · 174–204 cerrada · 204–264 Petalos_Cerrar al revés (60→0) · 264–300 nadar. **Rotor:** gira sin parar de 0 a 120, frena de 120 a 144, está quieto mientras la nave está cerrada, arranca de 240 a 264 y desde ahí gira otra vez a la velocidad de nado, sin salto |

`Demo_Completa` se hornea a partir de las Actions separadas, un cuadro por fotograma, y esas Actions no se tocan. Sirve para mirar la animación. En el juego, usa las tomas separadas.

## Unity (Quest 3)

1. Importa `eritro_rig.fbx`.
2. **Rig:** Animation Type = **Generic**, Avatar = Create From This Model, Root node = `Raiz`.
3. **Animation:** salen 6 tomas (`Eritro_Rig|…|Patas_Contraer`, etc.). Activa **Loop Time** solo en `Flagelo_Nadar`. `Petalos_Cerrar` termina en el estado de camuflaje; para abrir, reprodúcela con velocidad −1. `Demo_Completa` es solo de referencia.
4. **Model:** Scale Factor 1 (1 unidad = 1 µm, igual que el torrente). No marques *Bake Axis Conversion*: el FBX se exportó sin bake_space_transform.
5. **Materiales:**
   - `Eritro_Nave`: URP/Lit con `eritro_rig_textures_1024/base_color.png`, `normal.png` y `unity_metallic_smoothness.png` (Smoothness Source = Metallic Alpha).
   - `Flagelo_Negro`: casi negro, suavidad 0,7.
   - Motor (sin texturas, color plano): `Motor_Nacar` (blanco nacarado, suavidad ~0,8), `Motor_Marino` (marino del atlas, suavidad ~0,85, metálico 0,4), `Motor_Metal` (gris oscuro, metálico 1, suavidad ~0,7) y `Motor_Emisivo` (#B04BFF con Emission #B04BFF, intensidad ~0,8; con Bloom brilla sin quemarse). La película fina del nácar es solo de Blender; en URP basta con el blanco y una suavidad alta.
   - En Unity, `Petalos_Cerrar` reproducida a velocidad −1 (para abrir) hace girar el rotor hacia atrás mientras se despliega el flagelo (cuadros 24→0, unos 0,8 s). Si molesta, se puede añadir una toma `Petalos_Abrir` con el rotor hacia delante (en `Demo_Completa` ya va así).
   - `Cupula_Vidrio`: **URP/Lit, Surface Type = Transparent**, Blend = Alpha, **Render Face = Both** (desde la cabina se ve la cara de dentro). Base color casi negro azulado (#010206 aprox., lineal 0,004/0,008/0,022) con **alpha 0,35**, Metallic 0 y Smoothness ~0,95. Si se quiere más reflejo en los bordes, un shader con Fresnel; Blender usa una capa brillante (coat) que URP Lit no tiene.
   - `Cupula_Marco` usa `Eritro_Nave` (se puede unir con el platillo).
   - `Eritrocito_Meshy` (la piel): **usa el mismo material que los glóbulos del torrente en Unity**, para que la nave camuflada sea idéntica a los demás. Si hay que crearlo: `eritrocito_base` + `eritrocito_normal` + `eritrocito_unity_metallic_smoothness`, con emisión = base color × 0,35.
6. **Hangar (antes de la reducción), 1 µm = 8 m:** es la misma nave con **Transform Scale = 8** en la raíz y **`Piel_Camuflaje` apagada**. Así mide 26,9 × 24,6 × 9,7 m (34,7 m con el flagelo) y, cerrada, 60 m.
   - **Pivote:** 4,758 m sobre el suelo.
   - **Cabina:** hundida 0,97 m bajo la cubierta. Su offset respecto al pivote es Unity (0, +1,669, +0,32) m o, como hija de la raíz ×8, localPosition (0, 0,2086, 0,04) con localScale 1/8.
   - **Ojos:** Unity (0, +2,958, +0,62) m.
   - **Distancia a una cabina suelta:** unos 18 m si la nave mira hacia ella y unos 22 m si le da la espalda (estimado, no probado en Unity).
   - **`Cupula_Vidrio`:** URP Lit Transparent, Render Face Both.

   Detalles en `nave_eritro/cabina/LEEME_cabina.md`.
7. **Rendimiento:** son 22 SkinnedMeshRenderers (el motor añade uno con 4 materiales y la cúpula dos; el vidrio es la única malla transparente). Las mallas que usan `Eritro_Nave` se pueden unir en una sola, porque los pesos son rígidos por hueso.

## Correcciones de la v1 (siguen aplicadas)

1. Malla en sopa: fusioné 19.469 vértices duplicados. 2. Borré 87 caras de islas internas. 3. Centré el origen en el platillo. 4. Corté las piezas con planos y tapé los cortes en azul marino. 5. Pinté de marino los 1.426 restos rojos enterrados. 6. Pétalos en carmesí #B0122E, con rugosidad mínima de 0,38. 7. Flagelo nuevo en hélice negra. 8. Aplané la cuchilla. 9. Recalculé normales. 10. Decimado del 7 % para quedar en 19.859 tris.

## Pendiente (menor)

- En las rótulas de cadera y hombro hay entre 10 y 50 pares de triángulos solapados con la pata o el brazo plegados (unos 0,02 µm, bajo el platillo). La piel los tapa en modo camuflaje.
- En los renders, el rectángulo blanco que se ve "dentro" del pozo es el reflejo de la luz de área `Clave` en el vidrio de la cúpula. Desaparece al ocultar `Cupula_Vidrio`: no es geometría.
- Durante la transición (cuadros 47–55 de `Petalos_Cerrar`), la piel todavía es más pequeña que la nave y los pétalos asoman por los lados. Es intencionado: la piel crece desde dentro y envuelve. Desde el cuadro 60 no asoma nada.
