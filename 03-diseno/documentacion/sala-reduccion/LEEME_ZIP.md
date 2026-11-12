# Sala de reducción + nave Eritro v4: qué hay en el repo

Es la sala final, ampliada para la nave Eritro v4 con cabina retro (8 m/µm). Suma **59.786 tris sin la nave**. La secuencia `Demo_Columna` dura 0–1190 cuadros (30 fps). El detalle completo está en `LEEME.md` (en esta misma carpeta).

> Versión adaptada al repo del `LEEME_ZIP.md` de la entrega `Sala_Reduccion_NaveV4.zip`. Los archivos cambiaron de nombre y de carpeta para seguir `03-diseno/README.md`; la tabla de abajo da la equivalencia.

## Dónde está cada cosa

| En el repo | Antes (zip V4) | Qué es |
|---|---|---|
| `03-diseno/fuentes/sala-reduccion.blend` | `Blender/sala_reduccion_nave_v4.blend` | Escena completa, texturas empaquetadas |
| `03-diseno/fuentes/nave-eritro-con-cabina.blend` | `Blender/eritro_con_cabina_v2.blend` | Nave v4 + cabina retro + exploradores, texturas empaquetadas |
| `03-diseno/fuentes/nave-eritro.blend` | `nave_eritro/modelo/eritro_rig.blend` | Nave sola con rig y 6 acciones (**1 u = 1 µm**) |
| `03-diseno/exports/sala-reduccion.fbx` | `Unity/Sala/sala_reduccion_nave_v4.fbx` | Sala con animaciones y texturas incrustadas (sin cambios) |
| `03-diseno/exports/nave-eritro-con-cabina.fbx` | `Unity/Nave/nave_v4_con_cabina.fbx` | Nave + cabina en pose Desplegado, **reexportada con las escalas aplicadas** (raíces a escala 1) y sin `Piel_Camuflaje` |
| `03-diseno/exports/nave-eritro.fbx` | `Unity/Nave/eritro_rig.fbx` | Nave sola con 6 tomas, **ahora en metros** (8 m/µm ya aplicado) |
| `03-diseno/exports/texturas/sala-reduccion/` | `Unity/Sala/Texturas/` | Mapas sueltos de la sala |
| `03-diseno/exports/texturas/nave-eritro/` | `Unity/Nave/Texturas/` | Mapas sueltos de la nave (1024) |
| — | `Unity/Nave/submarino-cabina_retro80.fbx` | **No se sube**: es la cabina que ya está en el repo (va dentro de `nave-eritro-con-cabina`) |

## Importar en Unity (Quest 3)

1. **Sala:** arrastra `exports/sala-reduccion.fbx` con Scale Factor 1. Toda la sala cuelga de **`Escala_Sala` (escala 2,7653)**, y los props a tamaño real ya traen 1/K. No cambies esas escalas.
2. **Nave:** importa `exports/nave-eritro-con-cabina.fbx` y ponla como hija de **`Soporte_Nave`**. Así sube con la columna, flota y se reduce con la animación de la sala.
   - Si usas `exports/nave-eritro.fbx` (nave sola, **ya en metros**), deja la escala en 1 (antes había que poner 8). La cabina va en **(0, +1,669, +0,32) m** desde el pivote de la nave (Unity) y los ojos del piloto en (0, +2,958, +0,62) m.
3. **Cámara del jugador:** en `Punto_Ojos_Piloto`, dentro de la cabina.
4. **Animación:** clip `Demo_Columna`, con Loop. Los cuadros clave están en `LEEME.md` y en los marcadores de la línea de tiempo de Blender:
   - 0–205: subida y haz.
   - 210–450: giro y reducción.
   - 505: vial a la pinza. 605: vial a la cuna.
   - 785–805: la nave cae al vial.
   - 935 y 1055: vial a la pinza y a la cápsula.
   - 1120–1180: la cápsula sale.
5. **Eventos** (Desarrollo decide cómo engancharlos; el código de referencia está en `LEEME.md`):
   - Alarma: encendida en el 210 y apagada en el 450.
   - Vial: cambia de lugar en 505, 605, 805, 935 y 1055.
6. **Luces:** no van en el FBX. Pon las tuyas: área de techo, spots hacia la plataforma y luz roja en las balizas. Conviene hornear la mayoría.
7. **Materiales:**
   - `Atlas_Meshy` en URP/Lit. El ORM está en el formato de glTF (G = rugosidad, B = metálico), así que para URP crea Metallic/Smoothness.
   - `Cupula_Vidrio`: Transparent, Render Face Both.
