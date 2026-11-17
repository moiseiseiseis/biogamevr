"""07 · Optimización: cabello reducido, capas ocultas bajo la túnica eliminadas y huesos sin uso retirados."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '05_pesos.blend'))
O = bpy.data.objects; rig = O['Operador_Mecha_Rig']
def tris():
    dg = bpy.context.evaluated_depsgraph_get(); t = {}
    for o in O:
        if o.type != 'MESH': continue
        ev = o.evaluated_get(dg); m = ev.to_mesh(); m.calc_loop_triangles(); t[o.name] = len(m.loop_triangles); ev.to_mesh_clear()
    return t
antes = sum(tris().values())
# 1. cabello: decimado al 30 % (se comparó visualmente con el original al 50, 38 y 30 %)
h = O['VRoid_Cabello']
dm = h.modifiers.new('Reducir', 'DECIMATE'); dm.ratio = 0.30; dm.use_collapse_triangulate = True
h.modifiers.move(h.modifiers.find('Reducir'), 0)
with bpy.context.temp_override(object=h, active_object=h, selected_objects=[h]):
    bpy.ops.object.modifier_apply(modifier='Reducir')
# 2. capas ocultas por la túnica: camiseta y pantalón (se conserva un anillo de caras en cada borde)
def ocultar_bajo(o, cubierta, alcance=0.06, pred=lambda c: True):
    bm = bm_of(o)
    cub = {v: pred(v.co) and cubierta.ray_cast(v.co + v.normal * 0.0005, v.normal, alcance)[0] is not None for v in bm.verts}
    cand = {f for f in bm.faces if all(cub[v] for v in f.verts)}
    borde = {f for f in cand if any(g not in cand for v in f.verts for g in v.link_faces)}
    bmesh.ops.delete(bm, geom=list(cand - borde), context='FACES')
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
    bm_back(bm, o); return len(cand - borde)
TUN = bvh_de(O['OPM_Tunica'], O['OPM_Tunica_Cuello'])
print('OCULTAS camiseta', ocultar_bajo(O['OPM_Camiseta_Negra'], TUN, pred=lambda c: c.z < 1.30))
print('OCULTAS pantalon', ocultar_bajo(O['OPM_Pantalon'], TUN, pred=lambda c: c.z > 0.74))
# 3. huesos sin peso en ninguna malla (sólo cadenas terminales: no se rompe la jerarquía humanoide)
usados = set()
for o in O:
    if o.type == 'MESH':
        idx = {g.index: g.name for g in o.vertex_groups}
        for v in o.data.vertices:
            for g in v.groups:
                if g.weight > 1e-4: usados.add(idx[g.group])
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode='EDIT')
eb = rig.data.edit_bones
def rama_sin_uso(b):
    return b.name not in usados and all(rama_sin_uso(c) for c in b.children)
humanoides = {b.name for b in eb if b.name.startswith('J_Bip_') or b.name == 'Root'}
quitar = [b.name for b in eb if b.name not in humanoides and rama_sin_uso(b)]
for n in quitar: eb.remove(eb[n])
bpy.ops.object.mode_set(mode='OBJECT')
for o in O:
    if o.type == 'MESH':
        for g in [g for g in o.vertex_groups if g.name in quitar]: o.vertex_groups.remove(g)
t = tris()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '07_optimizado.blend'))
print('HUESOS retirados', len(quitar), 'quedan', len(rig.data.bones))
print('OK07 triangulos', antes, '->', sum(t.values()))
for n, k in sorted(t.items(), key=lambda x: -x[1])[:12]: print('  ', k, n)
