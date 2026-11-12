"""04 · Escala a 1,68 m, guardado, exportaciones, escena en la camilla y vistas."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
OUT = os.environ['PAC_TRABAJO']; os.environ.setdefault('NEO_TRABAJO', OUT)
import render_vistas as rend
ALTURA = 1.68
bpy.ops.wm.open_mainfile(filepath=OUT + '/02b_pelo.blend')
O = bpy.data.objects; rig = O['Paciente_Rig']; sc = bpy.context.scene
# colección de soporte vital (mascarilla) para poder ocultarla
sv = collection('Paciente_Soporte_Vital')
for o in list(O):
    if o.name.startswith(('PAC_Mascarilla', 'PAC_Filtro', 'PAC_Valvula')):
        for c in list(o.users_collection): c.objects.unlink(o)
        sv.objects.link(o)
# escala uniforme
zmax = max((o.matrix_world @ v.co).z for o in O if o.type == 'MESH' for v in o.data.vertices)
f = ALTURA / zmax
bpy.ops.object.select_all(action='DESELECT')
kids = [o for o in O if o.parent == rig] + [rig]
for o in kids: o.hide_set(False); o.select_set(True)
rig.scale = (f, f, f); bpy.context.view_layer.update()
bpy.context.view_layer.objects.active = rig
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True, properties=False)
for o in O:
    if o.type == 'MESH':
        for m in o.modifiers:
            if m.type == 'SOLIDIFY': m.thickness *= f
            if m.type == 'BEVEL': m.width *= f
for o in O:
    if 'Collider' in o.name or o.type == 'EMPTY': o.hide_render = True
bpy.context.view_layer.update()
zmax2 = max((o.matrix_world @ v.co).z for o in O if o.type == 'MESH' and o.visible_get() for v in o.data.vertices)
print('ALTURA', round(zmax, 3), '->', round(zmax2, 3))
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/paciente.blend', compress=True)
# ---- exportaciones (sin el suavizado de presentación) ----
def tris(skip_sub):
    for o in O:
        for m in o.modifiers:
            if m.type == 'SUBSURF': m.show_viewport = not skip_sub
    dg = bpy.context.evaluated_depsgraph_get(); st = {}
    for o in O:
        if o.type == 'MESH':
            ev = o.evaluated_get(dg); me = ev.to_mesh(); me.calc_loop_triangles(); st[o.name] = len(me.loop_triangles); ev.to_mesh_clear()
    return st
full = tris(False); game = tris(True)
bpy.ops.object.select_all(action='DESELECT')
char = [o for o in O if o.type in ('MESH', 'ARMATURE')]
for o in char: o.select_set(True)
bpy.ops.export_scene.fbx(filepath=OUT + '/paciente.fbx', use_selection=True, object_types={'ARMATURE', 'MESH'},
    use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z', axis_up='Y', bake_space_transform=False, path_mode='COPY', embed_textures=True, mesh_smooth_type='FACE')
bpy.ops.export_scene.gltf(filepath=OUT + '/paciente.glb', use_selection=True, export_apply=True, export_animations=False, export_morph=True)
tris(False)
import json
rep = {'altura_m': round(zmax2, 3), 'tri_blend_presentacion': sum(full.values()), 'tri_exportacion': sum(game.values()),
       'huesos': len(rig.data.bones), 'mallas': len([o for o in O if o.type == 'MESH']),
       'por_malla_exportacion': {k: v for k, v in sorted(game.items(), key=lambda x: -x[1])}}
# ---- vistas de pie (sin mascarilla, como en la lámina) ----
for o in sv.objects: o.hide_render = True
for o in O:
    if o.name.startswith('PAC_Calcetin'): o.hide_render = True
P = 'pac'
rend.setup_and_render(P, views=('front', 'side', 'back', 'tq'), engine='BLENDER_EEVEE', res=(1000, 1500), samples=32, focus=(0, 0, 0.85), ortho=1.95)
w = sc.world.node_tree.nodes['Background']
rend.setup_and_render(P + '-rostro', views=('front', 'tq'), engine='BLENDER_EEVEE', res=(1100, 1100), focus=(0, 0, 1.43), ortho=0.5, samples=32)
for o in sv.objects: o.hide_render = False
rend.setup_and_render(P + '-mascarilla', views=('front', 'tq'), engine='BLENDER_EEVEE', res=(1100, 1100), focus=(0, 0, 1.43), ortho=0.5, samples=32)
# pose relajada (prueba de deformación)
pb = rig.pose.bones
def R(n, axis, deg):
    b = pb[n]; b.rotation_mode = 'QUATERNION'
    q = mathutils.Quaternion(axis, math.radians(deg)); mw = rig.data.bones[n].matrix_local.to_quaternion()
    b.rotation_quaternion = mw.inverted() @ q @ mw
for s, lab in ((1, 'L'), (-1, 'R')):
    R('J_Bip_%s_UpperArm' % lab, (0, 1, 0), 72 * s); R('J_Bip_%s_LowerArm' % lab, (0, 0, 1), 20 * s)
bpy.context.view_layer.update()
for o in sv.objects: o.hide_render = True
rend.setup_and_render(P + '-pose', views=('front', 'tq', 'back'), engine='BLENDER_EEVEE', res=(1000, 1500), samples=32, focus=(0, 0, 0.85), ortho=1.95)
for o in sv.objects: o.hide_render = False
json.dump(rep, open(OUT + '/paciente-geometria.json', 'w'), indent=1, ensure_ascii=False)

# ---- escena: paciente en la camilla ----
with bpy.data.libraries.load(OUT + '/camilla-soporte.blend') as (src, dst): dst.objects = ['Camilla_Soporte']
cam = dst.objects[0]; esc = collection('Escena_Camilla'); esc.objects.link(cam)
for o in O:
    if o.name.startswith('PAC_Zapatilla'): o.hide_render = True; o.hide_set(True)
    if o.name.startswith('PAC_Calcetin'): o.hide_render = False; o.hide_set(False)
for s, lab in ((1, 'L'), (-1, 'R')):
    R('J_Bip_%s_UpperArm' % lab, (0, 1, 0), 80 * s); R('J_Bip_%s_LowerArm' % lab, (0, 0, 1), 6 * s)
    R('J_Bip_%s_Hand' % lab, (1, 0, 0), 8)
    R('J_Bip_%s_UpperLeg' % lab, (0, 1, 0), -2 * s)
R('J_Bip_C_Head', (1, 0, 0), 6)
for k in O['VRoid_Cara'].data.shape_keys.key_blocks:
    k.value = 1.0 if k.name == 'Fcl_EYE_Close' else 0.0
rig.rotation_mode = 'XYZ'; rig.rotation_euler = (math.radians(-90), 0, 0)
bpy.context.view_layer.update()
dg = bpy.context.evaluated_depsgraph_get()
pts = [o.evaluated_get(dg).matrix_world @ v.co for o in O if o.type == 'MESH' and o.parent == rig and o.visible_get() and not o.name.startswith('PAC_Capucha')
       for v in o.evaluated_get(dg).to_mesh().vertices]
zmin = min(p.z for p in pts); yhead = max(p.y for p in pts)
rig.location = (0, 0.985 - yhead, 0.645 - zmin - 0.012)
bpy.context.view_layer.update()
# manguera del respirador hasta la cabecera
dg = bpy.context.evaluated_depsgraph_get(); va = O['PAC_Valvula'].evaluated_get(dg)
vc = sum((va.matrix_world @ v.co for v in va.data.vertices), V()) / len(va.data.vertices)
cu = bpy.data.curves.new('ESC_Manguera_Respirador', 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = 0.009; cu.bevel_resolution = 3
sp = cu.splines.new('NURBS'); pts_ = [vc, vc + V((-0.05, 0.0, 0.025)), V((-0.17, vc.y + 0.04, vc.z + 0.01)), V((-0.30, 0.90, 0.72)), V((-0.33, 1.12, 0.66))]
sp.points.add(len(pts_) - 1)
for pt, p in zip(sp.points, pts_): pt.co = (*p, 1)
sp.use_endpoint_u = True; sp.order_u = 3; sp.resolution_u = 16
cu.materials.append(bpy.data.materials.get('CAM_Tubo') or bpy.data.materials['PAC_Correa'])
esc.objects.link(bpy.data.objects.new('ESC_Manguera_Respirador', cu))
sc.name = 'Paciente_en_Camilla'
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/paciente-en-camilla.blend', compress=True)
rend.setup_and_render(P + '-camilla', views=('tq',), engine='BLENDER_EEVEE', res=(1800, 1100), samples=32, focus=(0, 0.1, 0.55), ortho=2.6)
camo = O['_R_cam']; d = V((0.75, -0.25, 0.62)).normalized()
for name, d, fo, orth in (('alto', (0.75, -0.25, 0.62), (0, 0.15, 0.55), 2.5), ('rostro', (0.55, -0.2, 0.8), (0, 0.78, 0.70), 0.75)):
    d = V(d).normalized(); camo.location = V(fo) + d * 6; camo.rotation_euler = (-d).to_track_quat('-Z', 'Y').to_euler()
    camo.data.ortho_scale = orth
    sc.render.resolution_x, sc.render.resolution_y = (1800, 1100) if name == 'alto' else (1200, 900)
    sc.render.filepath = f'{OUT}/{P}-camilla_{name}.png'; bpy.ops.render.render(write_still=True)
camo.location = V((6, 0.1, 0.62)); camo.rotation_euler = V((-1, 0, 0)).to_track_quat('-Z', 'Y').to_euler(); camo.data.ortho_scale = 2.5
sc.render.resolution_x, sc.render.resolution_y = (1800, 800); sc.render.filepath = f'{OUT}/{P}-camilla_lado.png'; bpy.ops.render.render(write_still=True)
print('OK04', rep['altura_m'], rep['tri_exportacion'], rep['huesos'])
