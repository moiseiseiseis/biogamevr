"""03 · Camilla de soporte vital (modelo independiente). Cabecera hacia +Y, suelo en Z = 0."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
NEO = os.environ['PAC_NEO']; OUT = os.environ['PAC_TRABAJO']
bpy.ops.wm.read_factory_settings(use_empty=True)
with bpy.data.libraries.load(NEO) as (src, dst): dst.materials = ['INV_Chaqueta_Gris']   # plantilla MToon
sc = bpy.context.scene; sc.name = 'Camilla_Soporte'
col = collection('Camilla_Soporte')
M = dict(
 casco=toon('CAM_Casco', '#8e939a', '#6c7178'),
 casco_claro=toon('CAM_Casco_Claro', '#aeb2b8', '#888c92'),
 oscuro=toon('CAM_Oscuro', '#3d4248', '#2a2e33'),
 grafito=toon('CAM_Grafito', '#52575e', '#3b3f45'),
 colchon=toon('CAM_Colchon', '#c9ccd0', '#a2a6ab'),
 almohada=toon('CAM_Almohada', '#d6d8db', '#b0b3b7'),
 naranja=toon('CAM_Luz_Naranja', '#f3843a', '#d0662a', emissive='#ff7a2a', strength=2.0),
 tubo=toon('CAM_Tubo', '#2f3338', '#202327'),
 metal=toon('CAM_Metal', '#9aa1a9', '#737a82'),
)
parts = []
def box(name, size, loc, mat, bevel=0.0, segs=3):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    ob = bpy.context.active_object; ob.name = ob.data.name = name; ob.scale = size
    bpy.ops.object.transform_apply(scale=True)
    for c in list(ob.users_collection): c.objects.unlink(ob)
    col.objects.link(ob); set_mats(ob, [mat])
    if bevel:
        b = ob.modifiers.new('Bisel', 'BEVEL'); b.width = bevel; b.segments = segs; b.limit_method = 'ANGLE'
    parts.append(ob); return ob
def cyl(name, r, d, loc, mat, rot=(0, 0, 0), verts=24, bevel=0.0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d, location=loc, rotation=rot)
    ob = bpy.context.active_object; ob.name = ob.data.name = name
    for c in list(ob.users_collection): c.objects.unlink(ob)
    col.objects.link(ob); set_mats(ob, [mat])
    if bevel:
        b = ob.modifiers.new('Bisel', 'BEVEL'); b.width = bevel; b.segments = 2; b.limit_method = 'ANGLE'
    for p in ob.data.polygons: p.use_smooth = True
    parts.append(ob); return ob
def tube(name, pts, r, mat, res=12):
    cu = bpy.data.curves.new(name, 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = r; cu.bevel_resolution = 3; cu.use_fill_caps = True
    sp = cu.splines.new('NURBS'); sp.points.add(len(pts) - 1)
    for pt, p in zip(sp.points, pts): pt.co = (*p, 1)
    sp.use_endpoint_u = True; sp.order_u = 3; sp.resolution_u = res
    ob = bpy.data.objects.new(name, cu); col.objects.link(ob)
    cu.materials.append(mat); parts.append(ob); return ob

L, Wd = 2.20, 0.82
# chasis y patas
box('CAM_Pedestal', (0.44, 1.30, 0.30), (0, 0.02, 0.23), M['oscuro'], 0.03)
for y in (-0.72, 0.74):
    box('CAM_Pata', (0.70, 0.30, 0.10), (0, y, 0.05), M['grafito'], 0.035)
    box('CAM_Pata_Tope', (0.74, 0.08, 0.035), (0, y, 0.0175), M['oscuro'], 0.012)
# plataforma
box('CAM_Plataforma', (Wd, L, 0.16), (0, 0, 0.47), M['casco'], 0.05, 4)
box('CAM_Bandeja', (Wd - 0.10, L - 0.12, 0.03), (0, 0, 0.56), M['casco_claro'], 0.012)
# franjas laterales con luces naranjas
for s in (1, -1):
    box('CAM_Franja', (0.02, L - 0.30, 0.05), (s * (Wd / 2 + 0.002), 0, 0.46), M['grafito'], 0.008)
    for y in (0.78, 0.05, -0.80):
        box('CAM_Luz', (0.012, 0.16, 0.016), (s * (Wd / 2 + 0.012), y, 0.46), M['naranja'], 0.004, 2)
    # topes de las esquinas
    for y in (L / 2 - 0.07, -L / 2 + 0.07):
        cyl('CAM_Tope', 0.05, 0.20, (s * (Wd / 2 - 0.04), y, 0.47), M['oscuro'], bevel=0.015)
# defensas laterales segmentadas con luces en las juntas
for s_ in (1, -1):
    x = s_ * (Wd / 2 - 0.05)
    ys = [(-0.98, -0.46), (-0.42, 0.10), (0.14, 0.62), (0.66, 1.00)]
    for a, b in ys:
        box('CAM_Defensa', (0.10, b - a, 0.10), (x, (a + b) / 2, 0.60), M['casco'], 0.032, 4)
    for a, b in ys[1:]:
        box('CAM_Junta_Luz', (0.05, 0.026, 0.012), (x, a - 0.02, 0.638), M['naranja'], 0.004, 2)
# paneles segmentados de la cubierta (ranuras)
for y in (-0.42, 0.18):
    box('CAM_Ranura', (Wd - 0.06, 0.012, 0.012), (0, y, 0.548), M['grafito'])
# colchón en tres segmentos y almohada
segs = ((-0.96, -0.44), (-0.42, 0.10), (0.12, 0.68))
for a, b in segs:
    box('CAM_Colchon', (0.60, b - a, 0.07), (0, (a + b) / 2, 0.61), M['colchon'], 0.028, 4)
box('CAM_Almohada', (0.46, 0.26, 0.09), (0, 0.84, 0.62), M['almohada'], 0.04, 5)
# cabecera y piecera
box('CAM_Cabecera', (Wd - 0.04, 0.12, 0.30), (0, L / 2 - 0.02, 0.56), M['casco'], 0.04, 4)
box('CAM_Cabecera_Luz', (0.34, 0.012, 0.02), (0, L / 2 + 0.042, 0.64), M['naranja'], 0.004, 2)
box('CAM_Cabecera_Panel', (0.50, 0.01, 0.10), (0, L / 2 + 0.042, 0.50), M['grafito'], 0.006)
box('CAM_Piecera', (Wd - 0.04, 0.10, 0.20), (0, -L / 2 + 0.03, 0.50), M['grafito'], 0.035, 4)
# barandillas tubulares en la mitad de la cabecera (como en la lámina)
for s in (1, -1):
    x = s * (Wd / 2 - 0.03)
    tube('CAM_Barandilla', [V((x, 0.20, 0.64)), V((x + s * 0.02, 0.30, 0.82)), V((x + s * 0.02, 0.80, 0.84)), V((x, 1.00, 0.80)), V((x * 0.55, 1.06, 0.80))], 0.018, M['tubo'])
    cyl('CAM_Anclaje', 0.03, 0.04, (x, 0.20, 0.655), M['metal'], bevel=0.008)
tube('CAM_Barandilla_Cabecera', [V((-(Wd / 2 - 0.03) * 0.55, 1.06, 0.80)), V((0, 1.08, 0.81)), V(((Wd / 2 - 0.03) * 0.55, 1.06, 0.80))], 0.018, M['tubo'])
# manguera y cable que bajan del lado izquierdo de la cabecera al pedestal
tube('CAM_Manguera', [V((0.40, 0.70, 0.50)), V((0.50, 0.55, 0.38)), V((0.47, 0.25, 0.30)), V((0.36, 0.05, 0.27)), V((0.22, -0.05, 0.25))], 0.016, M['tubo'])
tube('CAM_Cable', [V((0.40, 0.62, 0.48)), V((0.47, 0.42, 0.33)), V((0.44, 0.16, 0.22)), V((0.22, 0.10, 0.18))], 0.008, M['grafito'])
# unir en un solo objeto
bpy.ops.object.select_all(action='DESELECT')
for o in parts:
    o.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.convert(target='MESH')
bpy.ops.object.join()
cam = bpy.context.active_object; cam.name = cam.data.name = 'Camilla_Soporte'
bpy.ops.object.shade_auto_smooth(angle=math.radians(35))
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/camilla-soporte.blend', compress=True)
me = cam.data; me.calc_loop_triangles()
print('OK03', len(me.loop_triangles), [m.name for m in me.materials])
