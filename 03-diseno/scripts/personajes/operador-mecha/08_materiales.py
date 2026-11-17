"""08 · Materiales y texturas: de 15 materiales a 3.
  OPM_Cara          atlas de la cara reducido de 4096 a 2048 (mismas UV), recorte alfa.
  OPM_Cabello_Piel  atlas nuevo de 2048: mechones y casquete arriba (resolución original), piel abajo a la izquierda (½).
  OPM_Ropa_Equipo   comparte esa textura: paleta de colores planos y emblema en el cuadrante libre; opaco, con mapa de emisión de 512.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
import numpy as np
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '07_optimizado.blend'))
O = bpy.data.objects
TEX = os.path.join(W, 'texturas'); os.makedirs(TEX, exist_ok=True)

def px(img):
    a = np.empty(img.size[0] * img.size[1] * 4, dtype=np.float32); img.pixels.foreach_get(a)
    return a.reshape(img.size[1], img.size[0], 4)            # filas de abajo arriba
def imagen(nombre, arr, archivo):
    h, w, _ = arr.shape
    im = bpy.data.images.get(nombre) or bpy.data.images.new(nombre, w, h, alpha=True)
    if tuple(im.size) != (w, h): im.scale(w, h)
    im.pixels.foreach_set(arr.astype(np.float32).ravel())
    im.filepath_raw = os.path.join(TEX, archivo); im.file_format = 'PNG'; im.save(); im.pack()
    return im
def mitad(a):                                                 # reduce a la mitad promediando 2×2
    return a.reshape(a.shape[0] // 2, 2, a.shape[1] // 2, 2, 4).mean(axis=(1, 3))

mc = bpy.data.materials['N00_000_00_FaceMouth_00_FACE (Instance)'].vrm_addon_extension.mtoon1
mb = bpy.data.materials['N00_000_00_Body_00_SKIN (Instance)'].vrm_addon_extension.mtoon1
cara_c = px(mc.pbr_metallic_roughness.base_color_texture.index.source)
cara_s = px(mc.extensions.vrmc_materials_mtoon.shade_multiply_texture.index.source)
cuer_c = px(mb.pbr_metallic_roughness.base_color_texture.index.source)
cuer_s = px(mb.extensions.vrmc_materials_mtoon.shade_multiply_texture.index.source)
assert cara_c.shape[0] == 4096 and cuer_c.shape[0] == 4096
img_cara = imagen('OPM_Cara_Color', mitad(cara_c), 'operador-mecha_cara.png')
img_cara_s = imagen('OPM_Cara_Sombra', mitad(cara_s), 'operador-mecha_cara_sombra.png')

# ---------- paleta de ropa y equipo (cuadrante inferior derecho: u 0,5–1, v 0–0,5) ----------
COLORES = {'OPM_Tunica': '#7a7864', 'OPM_Negro_Tela': '#2b2824', 'OPM_Pantalon': '#47443f', 'OPM_Pantalon_Puno': '#3d3c3b',
           'OPM_Correa': '#211e19', 'OPM_Naranja': '#ce773c', 'OPM_Gris': '#847d75', 'OPM_Gris_Claro': '#c8c3be',
           'OPM_Bota': '#2b2824', 'OPM_Guante': '#8a857e', 'OPM_Cian': '#5fd0c4'}
EMISIVOS = {'OPM_Cian': '#5fd0c4'}
celdas = {}
for i, n in enumerate(COLORES):                                # cuadrícula 4×4 de celdas de 128 px en la mitad izquierda del cuadrante
    cx, cy = i % 4, i // 4
    celdas[n] = (0.5 + (cx + 0.5) * 128 / 2048, (cy + 0.5) * 128 / 2048)
def hex2(h): h = h.lstrip('#'); return [int(h[k:k + 2], 16) / 255 for k in (0, 2, 4)]

def armar(color_src, es_sombra=False):
    A = np.zeros((2048, 2048, 4), dtype=np.float32); A[..., 3] = 1.0
    A[1024:2048, 0:2048] = color_src[3072:4096, 1024:3072]     # casquete y mechones, 1:1
    A[0:1024, 0:1024] = mitad(color_src[0:2048, 0:2048])        # piel, a la mitad
    for i, (n, h) in enumerate(COLORES.items()):
        cx, cy = i % 4, i // 4; c = np.array(hex2(h)) * (0.72 if es_sombra else 1.0)
        A[cy * 128:(cy + 1) * 128, 1024 + cx * 128:1024 + (cx + 1) * 128, :3] = c
    return A
A = armar(cuer_c); As = armar(cuer_s, es_sombra=True)
# emblema de ADN en la mitad derecha del cuadrante (512 × 512, arriba)
emb = bpy.data.images['OPM_Emblema_ADN']; E = px(emb)
if E.shape[0] != 512: emb.scale(512, 512); E = px(emb)
for B in (A, As):
    fondo = B[512:1024, 1536:2048, :3]
    B[512:1024, 1536:2048, :3] = E[..., :3] * E[..., 3:4] + np.array(hex2('#211e19')) * (1 - E[..., 3:4])
img_cuerpo = imagen('OPM_Cuerpo_Color', A, 'operador-mecha_cuerpo.png')
img_cuerpo_s = imagen('OPM_Cuerpo_Sombra', As, 'operador-mecha_cuerpo_sombra.png')
Em = np.zeros((512, 512, 4), dtype=np.float32); Em[..., 3] = 1.0
for n, h in EMISIVOS.items():
    u, v = celdas[n]; x0, y0 = int((u * 2048 - 64) / 4), int((v * 2048 - 64) / 4)
    Em[y0:y0 + 32, x0:x0 + 32, :3] = hex2(h)
img_em = imagen('OPM_Ropa_Emision', Em, 'operador-mecha_emision.png')

# ---------- materiales finales ----------
def mat(nombre, color, sombra, alfa, doble, factor_sombra=(1, 1, 1), emision=None):
    m = bpy.data.materials.get(nombre) or bpy.data.materials.new(nombre)
    e = m.vrm_addon_extension.mtoon1; e.enabled = True
    e.pbr_metallic_roughness.base_color_factor = (1, 1, 1, 1)
    e.pbr_metallic_roughness.base_color_texture.index.source = color
    mt = e.extensions.vrmc_materials_mtoon
    mt.shade_multiply_texture.index.source = sombra; mt.shade_color_factor = factor_sombra
    mt.shading_toony_factor = 0.9; mt.shading_shift_factor = -0.05
    e.alpha_mode = alfa; e.alpha_cutoff = 0.5; e.double_sided = doble
    if emision:
        e.emissive_texture.index.source = emision; e.emissive_factor = (1, 1, 1)
    return m
M_cara = mat('OPM_Cara', img_cara, img_cara_s, 'MASK', True)
M_pelo = mat('OPM_Cabello_Piel', img_cuerpo, img_cuerpo_s, 'MASK', True)
M_ropa = mat('OPM_Ropa_Equipo', img_cuerpo, img_cuerpo_s, 'OPAQUE', False, emision=img_em)

def solo(o, m):
    o.data.materials.clear(); o.data.materials.append(m)
    for p in o.data.polygons: p.material_index = 0
# cara: mismas UV
solo(O['VRoid_Cara'], M_cara)
# cabello y piel: se remapean las UV al atlas nuevo (la piel queda igual)
for n in ('VRoid_Cabello', 'VRoid_Cuerpo'):
    o = O[n]; uv = o.data.uv_layers.active.data
    for p in o.data.polygons:
        cu = sum(uv[i].uv.x for i in p.loop_indices) / p.loop_total; cv = sum(uv[i].uv.y for i in p.loop_indices) / p.loop_total
        for i in p.loop_indices:
            u, v = uv[i].uv
            if cv >= 0.75: uv[i].uv = ((u - 0.25) / 0.5, 0.5 + (v - 0.75) / 0.25 * 0.5)
    solo(o, M_pelo)
# ropa y equipo: cada cara a la celda de su color; el parche al emblema
faltan = set()
for o in [o for o in O if o.type == 'MESH' and o.name.startswith('OPM_')]:
    me = o.data
    if not me.uv_layers: me.uv_layers.new(name='UVMap')
    uv = me.uv_layers.active.data
    if o.name == 'OPM_Parche_Brazo':
        for i in range(len(uv)):
            u, v = uv[i].uv; uv[i].uv = (0.75 + u * 0.25, 0.25 + v * 0.25)
    else:
        for p in me.polygons:
            n = me.materials[p.material_index].name if me.materials else ''
            if n not in celdas: faltan.add(n); n = 'OPM_Gris'
            for i in p.loop_indices: uv[i].uv = celdas[n]
    solo(o, M_ropa)
# sin contorno MToon (Unity no usa MToon) y fuera los materiales y texturas que ya no usa nadie
for o in O:
    for m in [m for m in o.modifiers if m.type == 'NODES' and 'Outline' in m.name]: o.modifiers.remove(m)
for ng in [g for g in bpy.data.node_groups if 'Outline' in g.name and g.users == 0]: bpy.data.node_groups.remove(ng)
for m in [m for m in bpy.data.materials if m.name not in ('OPM_Cara', 'OPM_Cabello_Piel', 'OPM_Ropa_Equipo')]:
    bpy.data.materials.remove(m)
bpy.data.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)
usados = sorted({m.name for o in O if o.type == 'MESH' for m in o.data.materials if m})
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '08_materiales.blend'))
print('OK08 materiales', usados, 'sin celda:', faltan)
print('TEXTURAS', [(i.name, tuple(i.size)) for i in bpy.data.images if i.size[0] > 8])
