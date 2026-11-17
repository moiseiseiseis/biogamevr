"""05 · Pesos: transferencia propia desde la piel completa (triángulo más cercano + interpolación),
suavizado, máximo 4 influencias y normalización. Las piezas rígidas siguen a un solo hueso.
(DataTransfer con modifier_apply falló en silencio en modo background en el Neo Investigador.)"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
from mathutils.interpolate import poly_3d_calc
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '04_equipo.blend'))
rig = bpy.data.objects['Operador_Mecha_Rig']; O = bpy.data.objects
deform = {b.name for b in rig.data.bones if b.use_deform}
# piel completa (antes de borrar la oculta) como fuente
antes = set(bpy.data.objects); antes_arm = set(bpy.data.armatures); antes_me = set(bpy.data.meshes)
with bpy.data.libraries.load(os.path.join(W, '02_separado.blend')) as (src, dst): dst.objects = ['VRoid_Cuerpo']
fuente = dst.objects[0]
bm = bmesh.new(); bm.from_mesh(fuente.data); bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0002)
bmesh.ops.triangulate(bm, faces=bm.faces)
dl = bm.verts.layers.deform.active; nombres = [g.name for g in fuente.vertex_groups]
bm.verts.ensure_lookup_table(); bm.faces.ensure_lookup_table()
tris = [[v.index for v in f.verts] for f in bm.faces]
cos = [v.co.copy() for v in bm.verts]
pesos = [{nombres[g]: w for g, w in v[dl].items() if nombres[g] in deform and w > 1e-4} for v in bm.verts]
from mathutils.bvhtree import BVHTree
BV = BVHTree.FromPolygons(cos, tris); bm.free()

def muestrear(co):
    loc, nor, fi, d = BV.find_nearest(co)
    t = tris[fi]; bc = poly_3d_calc([cos[i] for i in t], loc); acc = {}
    for i, b in zip(t, bc):
        for n, w in pesos[i].items(): acc[n] = acc.get(n, 0) + w * b
    return acc

def escribir(o, tabla):
    o.vertex_groups.clear()
    for vi, acc in enumerate(tabla):
        top = sorted(acc.items(), key=lambda t: -t[1])[:4]; s = sum(w for _, w in top) or 1
        for n, w in top:
            vg = o.vertex_groups.get(n) or o.vertex_groups.new(name=n); vg.add([vi], w / s, 'REPLACE')

def transferir(o, suavizado=2, extra=None):
    me = o.data
    tabla = [muestrear(o.matrix_world @ v.co) for v in me.vertices]
    vecinos = [[] for _ in me.vertices]
    for e in me.edges:
        a, b = e.vertices; vecinos[a].append(b); vecinos[b].append(a)
    def pasada(mask):
        nueva = []
        for i, acc in enumerate(tabla):
            if not mask(me.vertices[i].co) or not vecinos[i]: nueva.append(acc); continue
            m = dict(acc)
            for j in vecinos[i]:
                for n, w in tabla[j].items(): m[n] = m.get(n, 0) + w
            k = len(vecinos[i]) + 1; nueva.append({n: w / k for n, w in m.items()})
        return nueva
    for _ in range(suavizado): tabla = pasada(lambda c: True)
    if extra:
        mask, n = extra
        for _ in range(n): tabla = pasada(mask)
    escribir(o, tabla)

def rigido(o, hueso=None):
    if hueso is None:
        c = sum((o.matrix_world @ v.co for v in o.data.vertices), V()) / len(o.data.vertices)
        acc = muestrear(c); hueso = max(acc, key=acc.get)
        if hueso.startswith('J_Sec_') or 'Shoulder' in hueso: hueso = 'J_Bip_C_UpperChest'   # sin huesos secundarios ni de hombro
    o.vertex_groups.clear(); rigid_weight(o, hueso)
    return hueso

RIGIDOS = {'OPM_Auricular_L': 'J_Bip_C_Head', 'OPM_Auricular_R': 'J_Bip_C_Head', 'OPM_Auricular_Luz_L': 'J_Bip_C_Head',
           'OPM_Auricular_Luz_R': 'J_Bip_C_Head', 'OPM_Diadema': 'J_Bip_C_Head', 'OPM_Microfono': 'J_Bip_C_Head',
           'OPM_Microfono_Punta': 'J_Bip_C_Head', 'OPM_Parche_Brazo': 'J_Bip_L_UpperArm',
           'OPM_Suela_L': 'J_Bip_L_Foot', 'OPM_Suela_R': 'J_Bip_R_Foot',
           'OPM_Muniequera_L': 'J_Bip_L_Hand', 'OPM_Muniequera_R': 'J_Bip_R_Hand'}
rep = {}
for o in [o for o in O if o.type == 'MESH' and o.name.startswith('OPM_')]:
    if o.name in RIGIDOS: rep[o.name] = rigido(o, RIGIDOS[o.name])
    elif len(o.data.vertices) <= 16 or o.name.startswith(('OPM_Hebilla', 'OPM_Ajuste', 'OPM_Panel', 'OPM_Bota_Tirador')):
        rep[o.name] = rigido(o)                                              # cajas: un solo hueso
    elif o.name == 'OPM_Tunica':
        transferir(o, 2, (lambda c: c.z < 0.95, 25)); rep[o.name] = 'piel + suavizado del faldón'
    else:
        transferir(o, 2); rep[o.name] = 'piel'
    for m in o.modifiers:
        if m.type == 'ARMATURE': m.object = rig
# fuera todo lo que trajo la carga (la armadura VRM arrastra la cara y su esqueleto)
for o in [o for o in bpy.data.objects if o not in antes]: bpy.data.objects.remove(o)
for a in [a for a in bpy.data.armatures if a not in antes_arm and a.users == 0]: bpy.data.armatures.remove(a)
for m in [m for m in bpy.data.meshes if m not in antes_me and m.users == 0]: bpy.data.meshes.remove(m)
print('OBJETOS', len(bpy.data.objects), sorted(o.name for o in bpy.data.objects if '.0' in o.name))
# comprobación: vértices sin peso
sin = {}
for o in O:
    if o.type != 'MESH': continue
    n = sum(1 for v in o.data.vertices if not any(g.weight > 1e-4 for g in v.groups))
    if n: sin[o.name] = n
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '05_pesos.blend'))
print('RIGIDOS', {k: v for k, v in rep.items() if v not in ('piel', 'piel + suavizado del faldón')})
print('OK05 vertices_sin_peso', sum(sin.values()), sin)
