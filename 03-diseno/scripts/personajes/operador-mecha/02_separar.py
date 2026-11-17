"""02 · Separa piel, cabello y la ropa de VRoid según la zona del atlas que ocupa cada isla.

Con «Materials: 2» VRoid une cuerpo, cabello y ropa en una sola malla y un atlas de 4096.
Zonas del atlas (u, v): piel 0–0,5 × 0–0,5; playera 0–0,25 × 0,5–1; short 0,25–0,5 × 0,5–0,75;
casquete del cabello 0,25–0,5 × 0,75–1; mechones 0,5–0,75 × 0,75–1; calzado 0,75–0,875 × 0,75–0,875.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '01_importado.blend'))
src = bpy.data.objects['VRoid_Cuerpo']
def zona(u, v):
    if u < 0.5 and v < 0.5: return 'piel'
    if u < 0.25 and v >= 0.5: return 'ropa'
    if u < 0.5 and v < 0.75: return 'ropa'
    if u < 0.5: return 'cabello'
    if u < 0.75 and v >= 0.75: return 'cabello'
    return 'ropa'
bm = bmesh.new(); bm.from_mesh(src.data); bm.faces.ensure_lookup_table()
uvl = bm.loops.layers.uv.active
seen = set(); clase = {}
for f in bm.faces:
    if f.index in seen: continue
    stack = [f]; seen.add(f.index); isla = []
    while stack:
        g = stack.pop(); isla.append(g.index)
        for e in g.edges:
            for h in e.link_faces:
                if h.index not in seen: seen.add(h.index); stack.append(h)
    us = [l[uvl].uv for i in isla for l in bm.faces[i].loops]
    cu = sum(u.x for u in us) / len(us); cv = sum(u.y for u in us) / len(us)
    for i in isla: clase[i] = zona(cu, cv)
bm.free()
def copia(nombre, keep):
    o = src.copy(); o.data = src.data.copy(); o.name = o.data.name = nombre
    for c in src.users_collection: c.objects.link(o)
    b = bmesh.new(); b.from_mesh(o.data); b.faces.ensure_lookup_table()
    bmesh.ops.delete(b, geom=[f for f in b.faces if clase[f.index] != keep], context='FACES')
    bmesh.ops.delete(b, geom=[v for v in b.verts if not v.link_faces], context='VERTS')
    b.to_mesh(o.data); b.free(); return o
cab = copia('VRoid_Cabello', 'cabello')
piel = copia('VRoid_Cuerpo_Piel', 'piel')
bpy.data.objects.remove(src)
piel.name = piel.data.name = 'VRoid_Cuerpo'
rep = {}
for o in bpy.data.objects:
    if o.type == 'MESH':
        o.data.calc_loop_triangles(); rep[o.name] = len(o.data.loop_triangles)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '02_separado.blend'))
print('OK02', rep, 'ropa VRoid eliminada:', sum(1 for c in clase.values() if c == 'ropa'), 'caras')
