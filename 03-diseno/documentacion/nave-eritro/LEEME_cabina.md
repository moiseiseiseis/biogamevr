> **Nota de la subida al repo (02/11/2026).** En el repo: `eritro_con_cabina_v2.blend` → `03-diseno/fuentes/nave-eritro-con-cabina.blend` (texturas empaquetadas; incluye también las colecciones `EXPLORADOR` / `EXPLORADOR_CABINA`, que no se exportan). El FBX `nave_v4_con_cabina.fbx` → `03-diseno/exports/nave-eritro-con-cabina.fbx`, **reexportado con las escalas aplicadas** (antes las raíces traían `NAVE_Hangar` ×8 y `CABINA_Interior` ×0,01; ahora ambas ×1, mismas medidas en metros) y sin `Piel_Camuflaje` (comprobación en `nave-eritro-con-cabina-check.json`). La cabina retro y el explorador vienen de otras ramas del repo y **no se suben por separado**. Los scripts, renders y comandos de abajo están en la carpeta de trabajo de Ulises y no se subieron al repo.
# Cabina del repo dentro de la nave Eritro (hangar, antes de la reducción) — v2: 8 m/µm

En la v1 (cúpula de Meshy, 0,45 µm de ancho) la cabina solo cabía a **12 m/µm** (nave de 40 m). Se eligió ensanchar la cúpula: con la cúpula de vidrio nueva de la nave v4 (`Cupula_Vidrio`, 0,74 µm de ancho, ver `../modelo/LEEME_eritro_rig.md`), la cabina cabe a **8 m/µm** y la nave abierta mide **26,9 m** de ancho.

Todo se genera con `scripts/eritro_cabina.py`, sin créditos y sin tocar `eritro_rig.blend` (el script hace append de sus objetos en un archivo nuevo):

```
blender -b --factory-startup --python scripts/eritro_cabina.py -- medir    # barrido de escala -> eritro_cabina_barrido.json (elige la escala)
blender -b --factory-startup --python scripts/eritro_cabina.py -- build    # -> eritro_con_cabina.blend + eritro_cabina_medidas.json
blender -b nave_eritro/cabina/eritro_con_cabina.blend --python scripts/eritro_cabina.py -- render
```

Respaldos de la v1: `eritro_con_cabina_v1.blend` y `scripts/eritro_cabina_v1_backup.py.txt`.

## Origen de los modelos (repo en solo lectura)

No se modificó nada en `biogamevr`: solo leí el clon con `git show`/`ls-tree` y copié los objetos LFS hacia esta carpeta.

| Archivo aquí | De dónde sale |
|---|---|
| `submarino-cabina.fbx` | Rama `diseno/cabina` (commit `b461007`, "diseno: interior de cabina"). Objeto LFS `d5a4341d…`, **93.228 bytes** |
| `explorador_caricatura.fbx` | Rama `diseno/explorador`, objeto LFS `b1ce55b6…`, 134.844 bytes |
| `submarino-exterior.fbx` | Nave vieja (Seraphim), `03-diseno/exports/` de la rama `diseno/nave-exterior` |

> ⚠️ **La versión de la cabina no coincide con la pedida.** El oid `961d0a4d…` (203.852 bytes) **no está** en `.git/lfs/objects`, y ninguna rama local ni remota (`origin/diseno/cabina` = `b461007`) apunta a él. La única cabina del clon es la de 93 KB. Si existe una versión más nueva en GitHub, el clon no la ha bajado (no hice `fetch` para no tocar el repo). Las medidas de abajo son de la versión de 93 KB.

## 1. Cabina (metros; frente −Y, origen = centro del piso)

| Medida | Valor |
|---|---|
| Total (CAB_Estructura) | **4,20 de ancho × 4,80 de largo × 2,80 de alto** (z −0,10 … 2,70). Es una caja completa: mandan las esquinas del techo |
| ANCLA_Jugador (asiento) | (0, −0,30, 0,49) |
| Ojos del piloto | No hay marca: asiento + 0,80 m = **1,29 m** sobre el piso, en (0, −0,30, 1,29) |
| CAB_Cristal | 4,12 × 3,29 × 1,30 m: x ±2,06, y −2,39 … 0,90, z 0,90 … 2,20 (ventanal delantero y laterales; doble hoja) |
| INT_Escotilla | Puerta trasera de 1,10 × 1,90 m: x ±0,55, y 2,18 … 2,30, z 0 … 1,90 |
| Consola | Hasta 1,22 m de alto, de y −2,10 a −0,39 |
| Tris | 2.330 |

## 2. Cúpula nueva de la nave (eritro_rig.blend v4, 1 u = 1 µm)

| Medida | µm | Hangar (×8) |
|---|---|---|
| Vidrio (`Cupula_Vidrio`) | **0,74 × 1,42** (y −0,84 … 0,58), centro en x = 0 | 5,9 × 11,4 m |
| Cumbre | z = 0,615 | 2,3 m sobre la cubierta |
| Cubierta alrededor | z ≈ 0,33 (baja hasta 0,24 en la punta delantera) | — |
| Pozo interior (`Cupula_Marco`) | pared vertical bajo el contorno y piso a z = 0,165 | piso 1,3 m bajo la cubierta |
| Antes (Meshy) | 0,45 × 1,43, cumbre 0,589, lente muy plana | — |
| Tris de la nave (sin la piel) | **19.806** (platillo 7.852, vidrio 352, marco 352); la piel aparte, 352 | — |

La cumbre solo sube 0,026 µm, porque la piel cerrada deja 0,64 en el centro (el glóbulo es bicóncavo). Para dar sitio a la cabina, la cúpula se ensanchó y su sección es una superelipse: el techo queda alto también hacia los lados. La piel plegada se bajó 0,125 µm para dejar libre el pozo.

## 3. Escala para el hangar

**Resultado: 1 µm = 8 m** (factor 8). Es la escala objetivo; la mínima posible es 7,75.

Criterios del barrido (`eritro_cabina_barrido.json`):
- Holgura 3D ≥ 10 cm de la cabina (estructura y cristal) a cualquier superficie de la nave en pose `Desplegado`.
- Ningún vértice de la nave dentro de la caja de la cabina más la holgura.
- Ojos bajo el vidrio y viendo por él hacia −Y.
- Frente de la cabina hacia −Y, sin rotación.

Para cada escala se elige el piso más alto que cumple todo (mejor vista):

| m/µm | ¿Cabe? | Piso bajo la cubierta | Ojos sobre la cubierta | Ventanal (borde inferior) |
|---|---|---|---|---|
| ≤ 7,5 | no (las esquinas del techo tocan el vidrio) | — | — | — |
| 7,75 | sí, justo (rango vertical de 2 mm) | 1,08 m | +0,21 m | −0,18 m |
| **8,0** | **sí** | **0,97 m** | **+0,32 m** | **−0,07 m** |
| 8,5 | sí | 0,76 m | +0,53 m | +0,14 m |
| 9,0 | sí | 0,55 m | +0,74 m | +0,35 m |
| 10,0 | sí | 0,18 m | +1,11 m | +0,72 m |

### Colocación a 8 m/µm (`eritro_cabina_medidas.json`)

| | Valor |
|---|---|
| Origen de la cabina (centro del piso) | (0, −0,04, 0,2086) µm → **(0, −0,32, +1,669) m desde el pivote de la nave** |
| **Cabina hundida** | **sí: el piso queda 0,97 m bajo la cubierta** (en la v1, 0,60 m a 12 m/µm). El techo (2,70 m) queda 1,73 m por encima de la cubierta |
| Ojos | 2,958 m sobre el pivote, **0,32 m sobre la cubierta**, bajo el vidrio (la cumbre está 1,96 m más arriba) |
| Ventanal | de −0,07 a +1,23 m respecto de la cubierta |
| Frente del ventanal | y = −2,71 m; el vidrio sigue 4 m más hacia delante (hasta y = −6,72 m) |
| **Holgura mínima** | **10,1 cm en 3D** y 11,6 cm en vertical. 0 de los 2.244 puntos quedan fuera |
| Zona más justa | las esquinas superiores del techo, contra la curva lateral del vidrio |

**Visión del piloto (rayos desde los ojos):**
- Al frente se ve **a través del vidrio desde unos −5° hacia arriba**. Por debajo tapan el collar y la cubierta, y la propia consola ya tapa hasta unos −10°.
- 187 de 315 direcciones de una rejilla de 100° × 70° salen por la cúpula (v1: 151).
- En el render de la vista del piloto se ve el hangar de fondo, a través del ventanal y del vidrio tintado.

## 4. Tamaño de la nave en el hangar (8 m/µm)

| | Medida |
|---|---|
| Platillo | 16 m de diámetro |
| **Abierta** | **26,9 m de ancho** (patas) × **24,6 m de largo** sin flagelo (**34,7 m** con flagelo) × **9,7 m de alto** (de los pies a la cumbre del vidrio) |
| **Cerrada (glóbulo)** | **60 m de diámetro**, 21,6 m de grueso en el borde y 8 m en el centro |
| Cúpula | 5,9 × 11,4 m, 2,3 m sobre la cubierta |
| Cabina dentro | piso a 6,43 m sobre el suelo del hangar |

**Comparación:**
- **Explorador (1,73 m):** la nave abierta es unas 15,5 veces más ancha que su altura y le llega a 5,6 alturas de explorador. Ver `render_hangar_escala.png` y `render_corte_lateral.png`.
- **Nave vieja Seraphim (`submarino-exterior.fbx`):** casco de 6,2 × 6,1 × 5,7 m. La Eritro tiene un platillo de 16 m contra un casco de 6 m: unas 2,6 veces más grande en planta.

## 5. Valores para Unity

Ambos FBX usan el mismo pipeline (Forward −Z, Up Y), así que **Blender (x, y, z) → Unity (−x, z, −y)**. El frente de la nave (−Y de Blender) queda hacia **+Z de Unity**.

| | Valor |
|---|---|
| Escala de `eritro_rig.fbx` en el hangar | Scale Factor 1 (1 u = 1 µm, como en el torrente) y **Transform Scale = (8, 8, 8)** en el objeto raíz del hangar |
| Pivote de la nave sobre el suelo del hangar | **+4,758 m** (los pies, en el cuadro 0 de `Desplegado`, quedan en y = 0) |
| **Offset de la cabina respecto al pivote de la nave** (sin rotación entre ambas) | Blender (0, −0,32, +1,669) m → **Unity (0, +1,669, +0,32) m** |
| Si la cabina es hija de la raíz escalada ×8 | localPosition = **(0, 0,2086, 0,04)** y localScale = **1/8 (0,125)** |
| Si la cabina se queda donde está (C) | nave en **C − (0, 1,669, 0,32)**. El suelo del hangar queda 6,43 m bajo el piso de la cabina |
| Ojos (ANCLA_Jugador + 0,8 m) en espacio de la nave | **Unity (0, +2,958, +0,62) m** desde el pivote |
| `Cupula_Vidrio` | **URP/Lit Transparent**, alpha 0,35, **Render Face = Both** (desde la cabina se ve la cara de dentro) |
| `Piel_Camuflaje` | **Apagarla en el hangar** (renderer desactivado). Ya no toca la cabina (queda 35 cm bajo la losa del piso, dentro del platillo), pero no aporta nada y es una malla más |

**Distancia mínima a una cabina suelta en el hangar** (estimada con las cotas de la nave; no la probé en una escena de Unity). Desde su pivote, la nave abierta ocupa ±13,5 m de lado, 15,4 m hacia delante (pinzas) y 9,2 m hacia atrás (19,3 m con el flagelo). La cabina mide 4,2 × 4,8 m. Separación pivote–centro de la cabina, en línea:
- **≈ 18 m** si la nave mira hacia la cabina (15,4 + 2,4).
- **≈ 22 m** si le da la espalda, con el flagelo (19,3 + 2,4).
- ≈ 16 m si la cabina queda de lado (13,5 + 2,1).

Con la v1 (12 m/µm) hacían falta 25 y 31 m; la "nave a 17 m" del hangar actual sigue sin caber, pero por poco.

## 6. Otros hallazgos

- **Escotilla:** la puerta trasera arranca 0,97 m bajo la cubierta y da al **pozo de la cúpula**, que sigue 2,5 m más hacia atrás bajo el vidrio (hasta y = +4,64 m). Para salir hace falta una rampa o escalera en ese hueco hasta una compuerta en el vidrio o en el collar trasero.
- **Cerrada:** la cabina queda dentro del glóbulo (ningún vértice de la nave queda fuera de la piel; margen mínimo de 0,044 µm).
- El vidrio es una sola superficie sin grosor; si en VR se nota, se puede duplicar hacia dentro con las normales invertidas.
- **02/11/2026:** en `eritro_rig.blend` se enterró en el platillo el borde delantero de `Cupula_Marco`, que asomaba por el morro (7 vértices, misma topología; ver `../modelo/LEEME_eritro_rig.md`). Los renders y `eritro_con_cabina.blend` de esta carpeta se hicieron antes de ese arreglo. El cambio está fuera de la cabina y no se ve a esta escala, así que las medidas no cambian. Si se vuelve a ejecutar `build`, ya sale con el arreglo.

## 7. Cabina retro 80s + Explorador Celular (v2, 02/11/2026)

```
blender -b --factory-startup --python scripts/eritro_cabina.py -- medir_v2   # -> eritro_cabina_v2_barrido.json
blender -b --factory-startup --python scripts/eritro_cabina.py -- build_v2   # -> eritro_con_cabina_v2.blend + eritro_cabina_v2_medidas.json
blender -b nave_eritro/cabina/eritro_con_cabina_v2.blend --python scripts/eritro_cabina.py -- render_v2
```

Las etapas de la v1 (`medir`, `build`, `render`) siguen igual y no tocan nada de la v2. `eritro_con_cabina.blend` no se modificó.

### Origen (descargado de GitHub, sin tocar el clon `biogamevr`)

| Archivo en `repo_nuevo/` | Origen | Bytes | sha256 |
|---|---|---|---|
| `submarino-cabina_retro80.fbx` | `main`, commit `74a399b` (Diego Ponce), `03-diseno/exports/submarino-cabina.fbx` | 203.852 | `961d0a4d…71bc0d` ✔ (coincide) |
| `explorador-celular.fbx` | rama `Modelado-Diseño` (Chris Rojas) | 701.148 | `a1aa9833…945465` |
| `explorador-celular.glb` | ídem | 856.512 | `f879a162…a979051` |
| `explorador-celular-vistas.png` | ídem, `03-diseno/vistas/personajes/explorador/` (referencia) | 1.870.315 | — |

### Cabina nueva frente a la vieja (`b461007`)

**Tiene el mismo envolvente y la misma distribución.** Solo cambia el detalle: pasa de 2.330 a **12.782 tris**, con materiales nuevos (CRT verde/ámbar, luces, amarillo, mando).

| Medida (m, frente −Y, origen = centro del piso) | Vieja | Retro 80s |
|---|---|---|
| CAB_Estructura | 4,20 × 4,80 × 2,80 (z −0,10 … 2,70) | **idéntica** |
| ANCLA_Jugador | (0, −0,30, 0,49) | **idéntico** |
| Ojos (ancla + 0,80) | 1,29 m | 1,29 m |
| CAB_Cristal | x ±2,06, y −2,39 … 0,90, z 0,90 … 2,20 | **idéntico** |
| INT_Escotilla | 1,10 × 1,90 m, y 2,175 … 2,30 | 1,10 × 1,90 m, y 2,165 … 2,30 |
| Ojos de buey | — | se ven en los renders (paredes laterales), pero forman parte de la malla `CAB_Estructura`/`CAB_Mobiliario`, no son objetos aparte. No los medí por separado; quedan dentro del envolvente, así que no afectan al encaje |
| CAB_Consola | ±0,95, hasta 1,22 m | ±0,97, hasta 1,233 m |
| CAB_Asiento | ±0,43, respaldo hasta 1,49 | ±0,425, respaldo hasta 1,49 |
| INT_Palanca_Potencia | (−0,20, −0,95, 0,73) | igual posición (pomo hasta 0,906) |

**Encaje en la cúpula v4:** el barrido (`eritro_cabina_v2_barrido.json`) da **los mismos números que con la cabina vieja**. Cabe a **8 m/µm** con **10,1 cm de holgura en 3D** (11,6 cm en vertical; 0 de 11.112 puntos fuera). La escala mínima sigue siendo 7,75. **No hace falta ajuste:** ni escala nueva ni hundirla ni recortar el techo. La colocación es la misma: origen (0, −0,04, 0,2086) µm, piso 0,97 m bajo la cubierta y ojos 0,32 m sobre ella. **Los valores de Unity de la sección 5 no cambian.**

### Explorador Celular

| | Valor |
|---|---|
| Altura | **1,70 m** (la caricatura medía 1,73) · ancho 0,75 m (de guante a guante), fondo 0,53 m (con la mochila) |
| Pose | **De pie, brazos abajo junto al cuerpo** (no es T-pose ni está sentado). Mira hacia −Y; la mochila queda en +Y |
| **Rig** | **No tiene.** Son 157 mallas rígidas (19.684 tris), sin armature ni grupos de vértices; el FBX y el GLB son iguales en esto. Pantalón y mangas son una sola pieza cada uno, así que tampoco se puede doblar por piezas |
| Cotas | casco 1,30 … 1,70 m (0,46 × 0,36 m), centro del visor a 1,50 m, hombros a 1,26 m (0,53 m de ancho), rodillas a 0,52 m, base de la pelvis a 0,79 m, hombro → punta de los dedos 0,57 m |

**No se pudo sentar** porque no tiene rig. Como pide el encargo para ese caso, está **de pie dentro de la cabina**, en el primer sitio libre detrás del asiento: (−0,80, +0,40) m en espacio de la cabina, suelas 5 mm sobre el piso. Allí:
- Techo a **0,90 m** sobre el casco.
- Holgura mínima a la cabina: **11 cm** (contra `CAB_Estructura`). No atraviesa nada.
- Ojos de pie a ≈ 1,51 m.

**Estimación sentado** (sin posarlo: el apoyo es la base de la pelvis sobre el ancla, con el torso recto):

| | Valor | Veredicto |
|---|---|---|
| Ojos | **≈ 1,20 m**, frente a 1,29 m de la cabina (ancla + 0,80) | −8,6 cm: **dentro de ±10 cm, pero justo**. El casco es grande y el visor queda bajo (88 % de la altura). En Unity conviene medir la cámara con el modelo, no usar +0,80 fijo |
| Casco | tope ≈ 1,40 m; techo sobre el asiento a 2,60 m | **1,20 m de holgura** |
| Palanca | hombro derecho → pomo ≈ 0,63 m; brazo (hombro → dedos) 0,57 m | **No llega por ~6 cm** con la espalda en el respaldo: tiene que inclinarse un poco o hay que acercar la palanca o el asiento |
| Piernas (rodilla y pie estimados) | 10 cm de la consola en la rodilla, 29 cm a media espinilla, 8 cm del piso en el pie | No chocan (estimación) |

Para sentarlo de verdad hace falta un rig (Rigify o Mixamo, unir las mallas por región y pesarlas). No lo hice porque eso es modificar el modelo de Chris.

### Archivos v2

| Archivo | Qué es |
|---|---|
| `eritro_con_cabina_v2.blend` | Como `eritro_con_cabina.blend`, pero con la cabina retro 80s (`CABINA`) y dos copias del Explorador Celular: `EXPLORADOR` (de pie, 3 m delante de las pinzas, para escala) y `EXPLORADOR_CABINA` (de pie, dentro). `OJOS_Piloto` marca los ojos de la cabina (ancla + 0,80) |
| `render_v2_corte_lateral.png`, `render_v2_corte_tres_cuartos.png`, `render_v2_vista_piloto.png` | Igual que los de la v1. La vista del piloto sale de los ojos de la cabina (1,29 m), porque el explorador no se puede sentar; sus ojos sentado estarían ≈ 9 cm más abajo |
| `render_v2_explorador_en_cabina.png` | Primer plano por el corte: el explorador de pie detrás del asiento. Se pedía como `…_explorador_sentado.png`; cambié el nombre porque no está sentado |
| `render_v2_hoja.png` | Los 4 renders, el panel de cifras y la lámina de vistas del repo |
| `eritro_cabina_v2_medidas.json` / `eritro_cabina_v2_barrido.json` | Cifras de la v2 (incluye `sentado_estimado`) |

## Archivos

| Archivo | Qué es |
|---|---|
| `eritro_con_cabina.blend` | `NAVE_Hangar` (empty ×8, pivote a 4,758 m) con el rig en pose `Desplegado`, colecciones `CABINA` y `EXPLORADOR`, `OJOS_Piloto` (esfera roja), `Plano_Corte` y modificador `Corte` (GN) en cada malla (apagado en el visor), pared de hangar de referencia, luces y cámara |
| `render_corte_lateral.png` | Corte por el eje de la cúpula, ortográfico, con el explorador delante |
| `render_corte_tres_cuartos.png` | El mismo corte en tres cuartos |
| `render_vista_piloto.png` | Desde los ojos, mirando hacia −Y (FOV horizontal de ~97°) |
| `render_hangar_escala.png` | La nave abierta a escala real junto al explorador de 1,73 m |
| `render_cabina_hoja.png` | Los tres renders de la cabina y el panel de cifras |
| `eritro_cabina_medidas.json` / `eritro_cabina_barrido.json` | Cifras de la colocación y del barrido de escala (con la escala elegida) |
| `submarino-cabina.fbx`, `explorador_caricatura.fbx`, `submarino-exterior.fbx` | Copias de los modelos del repo |
