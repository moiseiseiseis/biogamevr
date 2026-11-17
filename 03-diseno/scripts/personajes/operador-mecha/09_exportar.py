"""09 · Prop del panel holográfico, fuente .blend, exportaciones FBX/GLB y texturas PNG sin pérdida.

FBX: Forward -Z, Up Y, apply_scale_options='FBX_SCALE_ALL' y SIN «Apply Transform»
(bake_space_transform=False): el manual de Blender la marca como experimental y conocida por fallar
con armaduras. El prop no tiene armadura y sí la usa. GLB: export_apply=True.
Para Unity se agrupan las piezas por material en 3 mallas: Operador_Cara (con expresiones),
Operador_Cabello_Piel y Operador_Ropa_Equipo. La fuente .blend conserva las piezas separadas y editables.
"""
import os, sys, subprocess, shutil, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
import numpy as np
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '08_materiales.blend'))
O = bpy.data.objects; rig = O['Operador_Mecha_Rig']; sc = bpy.context.scene
OUT = os.path.join(W, 'out'); os.makedirs(OUT, exist_ok=True)
TEXW = os.path.join(W, 'texturas')

# ---------- prop: panel holográfico cian (dos paneles en un objeto, centrado en el origen) ----------
subprocess.run([PYTHON, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'panel_holografico_tex.py'), TEXW], check=True)
img = bpy.data.images.load(os.path.join(TEXW, 'panel-holografico_1024.png')); img.name = 'OPM_Panel_Holografico'; img.pack()
mp = bpy.data.materials.new('OPM_Holograma'); e = mp.vrm_addon_extension.mtoon1; e.enabled = True
e.pbr_metallic_roughness.base_color_texture.index.source = img
e.extensions.vrmc_materials_mtoon.shade_multiply_texture.index.source = img
e.emissive_texture.index.source = img; e.emissive_factor = (1, 1, 1)
e.alpha_mode = 'BLEND'; e.double_sided = True
props = coleccion('Operador_Props')
bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
def quad(w, h, M, u0, u1, v0, v1):
    vs = [bm.verts.new(M @ V(p)) for p in ((-w/2, 0, -h/2), (w/2, 0, -h/2), (w/2, 0, h/2), (-w/2, 0, h/2))]
    f = bm.faces.new(vs)
    for l, uv in zip(f.loops, ((u0, v0), (u1, v0), (u1, v1), (u0, v1))): l[uvl].uv = uv
R_ = mathutils.Matrix.Rotation
quad(0.30, 0.30 * 0.62 / 0.96, R_(math.radians(-8), 4, 'Z') @ R_(math.radians(10), 4, 'X'), 0.02, 0.98, 0.36, 0.98)
quad(0.13, 0.13 * 0.30 / 0.46, mathutils.Matrix.Translation((0.15, -0.035, 0.155)) @ R_(math.radians(-18), 4, 'Z') @ R_(math.radians(6), 4, 'X'), 0.02, 0.48, 0.02, 0.32)
bm.normal_update()
for f in bm.faces:
    if f.normal.y > 0: f.normal_flip()                                     # miran a -Y, hacia quien los usa
cen = sum((v.co for v in bm.verts), V()) / len(bm.verts)
bb = [v.co.copy() for v in bm.verts]; mid = V([(max(p[i] for p in bb) + min(p[i] for p in bb)) / 2 for i in range(3)])
for v in bm.verts: v.co -= mid
pm = bpy.data.meshes.new('operador-mecha-panel'); bm.to_mesh(pm); bm.free(); pm.materials.append(mp)
panel = bpy.data.objects.new('OPM_Panel_Holografico', pm); props.objects.link(panel)

# ---------- fuente .blend ----------
for o in O:
    if o.type == 'EMPTY' or 'Collider' in o.name: o.hide_render = True
sc.name = 'Operador_Mecha'
def triangulos(objs):
    dg = bpy.context.evaluated_depsgraph_get(); t = {}
    for o in objs:
        ev = o.evaluated_get(dg); m = ev.to_mesh(); m.calc_loop_triangles(); t[o.name] = len(m.loop_triangles); ev.to_mesh_clear()
    return t
personaje = [o for o in O if o.type == 'MESH' and o.parent == rig]
tri_piezas = triangulos(personaje)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'operador-mecha.blend'), compress=True)

# ---------- copia para exportar: modificadores aplicados y 3 mallas, una por material ----------
def aplicada(o):
    c = o.copy(); c.data = o.data.copy(); sc.collection.objects.link(c)
    for m in [m for m in c.modifiers if m.type != 'ARMATURE']:
        with bpy.context.temp_override(object=c, active_object=c, selected_objects=[c]):
            bpy.ops.object.modifier_apply(modifier=m.name)
    return c
def unir(nombre, objs):
    cs = [aplicada(o) for o in objs]
    bpy.ops.object.select_all(action='DESELECT')
    for c in cs: c.select_set(True)
    bpy.context.view_layer.objects.active = cs[0]
    if len(cs) > 1:
        with bpy.context.temp_override(active_object=cs[0], selected_editable_objects=cs, object=cs[0]):
            bpy.ops.object.join()
    j = bpy.context.view_layer.objects.active if len(cs) > 1 else cs[0]
    j.name = j.data.name = nombre
    for m in [m for m in j.modifiers if m.type == 'ARMATURE']: m.object = rig
    return j
for o in personaje: o.hide_set(False)
cara = unir('Operador_Cara', [O['VRoid_Cara']])
pelo = unir('Operador_Cabello_Piel', [O['VRoid_Cabello'], O['VRoid_Cuerpo']])
ropa = unir('Operador_Ropa_Equipo', [o for o in personaje if o.name.startswith('OPM_')])
exp = [cara, pelo, ropa]
tri_exp = triangulos(exp)
bpy.ops.object.select_all(action='DESELECT')
for o in exp + [rig]: o.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, 'operador-mecha.fbx'), use_selection=True, object_types={'ARMATURE', 'MESH'},
    use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z', axis_up='Y', bake_space_transform=False, path_mode='COPY', embed_textures=True, mesh_smooth_type='FACE')
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, 'operador-mecha.glb'), use_selection=True, export_apply=True,
    export_animations=False, export_morph=True)
bpy.ops.object.select_all(action='DESELECT'); panel.select_set(True); bpy.context.view_layer.objects.active = panel
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, 'operador-mecha-panel.fbx'), use_selection=True, object_types={'MESH'},
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, path_mode='COPY', embed_textures=True)
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, 'operador-mecha-panel.glb'), use_selection=True, export_apply=True)

# ---------- texturas PNG: oxipng sin pérdida y comprobación de píxeles idénticos ----------
def pix(path):
    im = bpy.data.images.load(path, check_existing=False); a = np.empty(im.size[0] * im.size[1] * im.channels, dtype=np.float32)
    im.pixels.foreach_get(a); r = (tuple(im.size), im.channels, a); bpy.data.images.remove(im); return r
texs = {}
for n in ('operador-mecha_cara.png', 'operador-mecha_cara_sombra.png', 'operador-mecha_cuerpo.png', 'operador-mecha_cuerpo_sombra.png',
          'operador-mecha_emision.png', 'panel-holografico_1024.png'):
    src = os.path.join(TEXW, n); dst = os.path.join(OUT, n.replace('panel-holografico_1024', 'operador-mecha_panel'))
    shutil.copy2(src, dst); antes = os.path.getsize(dst); p0 = pix(dst)
    subprocess.run(['oxipng', '-o', '4', '--strip', 'safe', '-q', dst], check=True)
    p1 = pix(dst); igual = p0[0] == p1[0] and p0[1] == p1[1] and np.array_equal(p0[2], p1[2])
    texs[os.path.basename(dst)] = {'px': list(p0[0]), 'bytes_antes': antes, 'bytes_despues': os.path.getsize(dst), 'pixeles_identicos': bool(igual)}
    assert igual, dst
rep = {'altura_m': round(max((o.matrix_world @ v.co).z for o in exp for v in o.data.vertices), 3),
       'triangulos_exportacion': sum(tri_exp.values()), 'por_malla_exportacion': tri_exp,
       'triangulos_piezas_blend': sum(tri_piezas.values()), 'huesos': len(rig.data.bones),
       'materiales': sorted({m.name for o in exp for m in o.data.materials if m}),
       'expresiones': len(cara.data.shape_keys.key_blocks) - 1 if cara.data.shape_keys else 0,
       'triangulos_panel': len(panel.data.polygons) * 2, 'texturas': texs}
json.dump(rep, open(os.path.join(OUT, 'operador-mecha-geometria.json'), 'w'), indent=1, ensure_ascii=False)
print('OK09', json.dumps({k: v for k, v in rep.items() if k != 'texturas'}, ensure_ascii=False))
print('TEX', json.dumps(texs))
