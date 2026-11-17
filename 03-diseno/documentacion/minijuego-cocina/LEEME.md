# Minijuego "Cocina de proteínas": modelos y prototipo

Prototipo jugable (abre sin internet con doble clic): `cocina-de-proteinas-3d.html`.
Proteína: amilina (IAPP, UniProt P10997), mutación S20G. Codones reales del ARNm NM_000415.3, codón de paro UAG.

## Modelos de moléculas (estructuras reales del PDB)
| Modelo | Fuente | Export | PDB | Tris |
|---|---|---|---|---|
| Ribosoma 80S (60S + 40S separadas) | `fuentes/minijuego-cocina/ribosoma-80s.blend` | `exports/minijuego-cocina/ribosoma-80s.fbx` | 4V6X | ~4,200 |
| ARNt | `arnt.blend` | `arnt-1ehz.fbx` | 1EHZ | ~1,200 |
| Estación de sintetasa (2 enzimas + manivela) | `sintetasa.blend` | `estacion-sintetasa-1qtq.fbx` | 1QTQ | ~2,540 |
| Chaperona GroEL-GroES (tapa separada) | `chaperona.blend` | `chaperona-1aon.fbx` | 1AON | ~1,990 |
| Proteasoma 20S | `proteasoma.blend` | `proteasoma-1ryp.fbx` | 1RYP | ~2,500 |
| Bases del ARNm (A/U/G/C tipo rompecabezas) | `arnm-bases.blend` | `arnm-bases.fbx` | a mano | 47–156 c/u |
| Horquilla del ARNm (blend shape "abierta") | `horquilla-arnm.blend` | `horquilla-arnm.fbx` | a mano | ~750 |
| Aminoácido y cuenta de la cadena | `aminoacido.blend` | `aminoacido.fbx` | a mano | ~200 |
| Escena de vista previa (todo junto) | `escena-minijuego-preview.blend` | — | — | ~33k |

Todos los exports son `.fbx`. Escala en metros, -Z Forward, Y Up. Los modelos tienen vacíos `Punto_*` (sitios A/P/E, anticodón, aminoácido, entradas) para que Desarrollo enganche la lógica.

## Utilería (`exports/minijuego-cocina/props/`, FBX)
Convertida de GLB a FBX en Blender (metros, -Z Forward, Y Up, texturas embebidas en el `.fbx`).

| Archivo | Origen | Tris |
|---|---|---|
| `mesa-curva.fbx` | Blender a mano (arco de 150°, alto 0.78 m) | 1,064 |
| `regreso-citoplasma.fbx` (antes "recicladora") | Blender a mano | 1,260 |
| `base-sintetasa.fbx` | Blender a mano | 460 |
| `bandeja-salida.fbx` | Meshy (text-to-3D) | 1,513 |
| `proyector-holograma.fbx` | Meshy (text-to-3D) | 1,330 |
| `tablero-pedido.fbx` | Meshy (text-to-3D) | 1,488 |

## Fondo 360° (`exports/minijuego-cocina/fondo/`)
`fondo-celula-360-a.jpg` y `-b.jpg` (4096×2048, equirectangular): render en Eevee del escenario de la célula v14, cámara sobre el RER. En el prototipo se mezclan despacio las dos tomas. Script: `scripts/minijuego-cocina/render-fondo-360.py`. En Unity: Skybox/Panoramic.

## Pendiente / notas
- Toda la escena junta pasa el límite de 30k tris (sobre todo por los 9 ARNt); para Quest usar menos ARNt a la vez o LOD.
- La "recicladora" se llama ahora "Regreso al citoplasma" (minuta de Investigación, punto 10): lanzar el ARNt es una metáfora.
