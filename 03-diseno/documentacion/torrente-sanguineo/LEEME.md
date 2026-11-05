# Torrente sanguíneo

Interior de una **arteria pequeña con una bifurcación en Y**, con glóbulos rojos, plaquetas y un glóbulo blanco, y la nave **Eritro** (v4, con rig y 6 animaciones) en el punto de salida. Es una escena de bloqueo: las proporciones son correctas, pero no es el arte final.

## Archivos

| En el repo | Qué es |
|---|---|
| `03-diseno/fuentes/torrente-sanguineo.blend` | Escena completa (Blender 5.2), texturas empaquetadas, con luces, niebla y cámaras de render |
| `03-diseno/exports/torrente-sanguineo.fbx` | Escena + rig de la Eritro con 6 tomas; texturas incrustadas |
| `03-diseno/exports/texturas/torrente-sanguineo/` | Mapas sueltos de la pared, el glóbulo rojo, la plaqueta y el glóbulo blanco (color, normal y rugosidad/metálico) |
| `03-diseno/exports/texturas/nave-eritro/` | Mapas de la nave (ya están en el repo con la nave Eritro; no se duplican aquí) |

## Escala: 1 u = 1 µm

- Vaso: 30 µm de diámetro y 150 µm de largo; dos ramas de 24 µm × 100 µm, a 60° entre sí.
- Glóbulo rojo: 7,5 µm. Plaqueta: 2 µm. Glóbulo blanco: ~12 µm.
- Eritro: 3,4 × 4,3 × 1,2 µm abierta, con flagelo. Va a la misma escala que en `fuentes/nave-eritro.blend`: no se reescaló.
- En Unity, con Scale Factor 1, queda **1 µm = 1 m** (vaso de 150 m, nave de 3,4 m). El hangar usa 8 m por µm; si se quiere la misma escala, se multiplica la raíz ×8.
- FBX: −Z Forward, Y Up, sin *Apply Transform* (no marcar *Bake Axis Conversion* en Unity).

## Objetos

| Objeto | Qué es |
|---|---|
| `Vaso_Sanguineo` | Pared lisa, extremos abiertos, **normales hacia dentro** (se ve desde dentro). 10.000 tris |
| `Eritrocito_###` (400), `Plaqueta_###` (15), `Leucocito_001` | Copias que comparten malla con `*_Proto` (colección oculta `Prototipos`). `Eritrocito_Escala` está junto a la nave |
| `Eritro_Rig` y sus 22 mallas | La nave, en `Spawn_Nave`, en pose `Desplegado` y con el frente hacia la bifurcación |
| `Spawn_Nave` | Punto de salida de la nave |
| `Flujo` | Flecha con la dirección de la sangre: del tronco a las ramas |
| `Ruta_Nave` | Curva del tronco a la rama derecha. Solo está en el `.blend`: las curvas no viajan en FBX |

Hay una franja sin glóbulos de 9 µm por el centro de la ruta, y una zona libre en los primeros 20 µm.

## Triángulos

| Parte | Tris |
|---|---|
| Vaso | 10.000 |
| 400 glóbulos rojos (352 c/u) | 140.800 |
| 15 plaquetas (120 c/u) | 1.800 |
| Glóbulo blanco | 320 |
| Nave Eritro (con la piel de camuflaje) | 20.142 |
| **Total dibujado** | **173.062** |
| **Geometría única** (vaso + 1 glóbulo + 1 plaqueta + 1 glóbulo blanco + nave) | **30.934** |

## Unity (Quest 3)

1. **GPU Instancing:** glóbulos, plaquetas y glóbulo blanco son copias de una sola malla por tipo. Activar *Enable GPU Instancing* en sus materiales, o dibujarlos con `Graphics.RenderMeshInstanced` a partir de las posiciones del FBX. Sin instancing serían 400 draw calls solo de glóbulos.
2. **Pared (`Pared_Vaso_Meshy`):** **Wrap Mode = Mirror** en sus tres texturas, porque se repiten en espejo a lo largo del tubo; con Repeat se ven costuras. Usar **Render Face = Both**. Emisión = base × 0,25.
3. **Células:** `Eritrocito_Meshy` (emisión = base × 0,35), `Plaqueta_Meshy` (× 0,6) y `Leucocito_Meshy` (× 0,15). Los mapas `*_rough` traen la rugosidad en G y el metálico en B (formato glTF): para URP/Lit hay que pasarlos a Metallic (R) + Smoothness (A).
4. **Nave:** igual que en `documentacion/nave-eritro/LEEME_eritro_rig.md`. Rig Generic, Root node = `Raiz`, Loop Time solo en `Flagelo_Nadar`, y `Cupula_Vidrio` Transparent con Render Face Both. La piel de camuflaje usa el mismo material que los glóbulos de la escena.
5. **Niebla y luces:** las de Blender son solo para los renders. En Unity, niebla lineal o exponencial y bloom ligero.

## De dónde salen las medidas

| Elemento | Dato | Fuente (página del PDF) |
|---|---|---|
| Glóbulo rojo | Disco bicóncavo de 7,5–7,8 µm, 2,5 µm en el borde y ≤ 1 µm en el centro, con el centro más pálido | Guyton p.420; Williams p.44 y p.492 |
| Plaqueta | Disco de 1–4 µm | Guyton p.457 |
| Glóbulo blanco | Neutrófilo de 10–14 µm que rueda pegado a la pared (marginación) | Williams p.45 y p.303; Guyton p.435 |
| Vaso | Arteriolas de 4–25 µm; el tronco de 30 µm es una arteria pequeña. Ramas de 24 µm por la ley de Murray (no está en los libros) | Guyton p.170 y p.184 |
| Pared | Una capa de células endoteliales planas | Guyton p.184; Alberts p.1388 |
| Densidad | ~18 % de hematocrito (la sangre real tiene 38–45 %); se rebajó para dejar la franja central libre | Guyton p.171; Goodsell p.98 |

Libros: Hall, *Guyton y Hall: Tratado de fisiología médica*, 14.ª ed.; Kaushansky et al., *Williams Hematology*; Alberts et al., *Molecular Biology of the Cell*, 6.ª ed.; Goodsell, *The Machinery of Life*, 2.ª ed.
