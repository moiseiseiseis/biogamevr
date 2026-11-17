"""10 · Reimporta FBX y GLB y compara mallas, huesos, triángulos, vértices sin peso, altura y Z mínima."""
import os, sys, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
enable()
OUT = os.path.join(W, 'out')
ref = json.load(open(os.path.join(OUT, 'operador-mecha-geometria.json')))
def vaciar():
    for o in list(bpy.data.objects): bpy.data.objects.remove(o)
    for c in list(bpy.data.collections): bpy.data.collections.remove(c)
    bpy.data.orphans_purge(do_recursive=True)
def medir(ruta):
    vaciar()
    if ruta.endswith('.fbx'): bpy.ops.import_scene.fbx(filepath=ruta)
    else: bpy.ops.import_scene.gltf(filepath=ruta)
    dg = bpy.context.evaluated_depsgraph_get()
    mallas = [o for o in bpy.data.objects if o.type == 'MESH' and o.data.vertices and not (o.name.startswith('Icosphere'))]
    arm = [o for o in bpy.data.objects if o.type == 'ARMATURE']
    tri = 0; sinp = 0; zs = []; mats = set(); tex = set(); sk = 0
    for o in mallas:
        ev = o.evaluated_get(dg); me = ev.to_mesh(); me.calc_loop_triangles(); tri += len(me.loop_triangles)
        zs += [(ev.matrix_world @ v.co).z for v in me.vertices]; ev.to_mesh_clear()
        if arm: sinp += sum(1 for v in o.data.vertices if not any(g.weight > 1e-4 for g in v.groups))
        for m in o.data.materials:
            if m: mats.add(m.name)
            if m and m.node_tree:
                for n in m.node_tree.nodes:
                    if n.type == 'TEX_IMAGE' and n.image: tex.add(n.image.name)
        if o.data.shape_keys: sk = max(sk, len(o.data.shape_keys.key_blocks) - 1)
    return {'mallas': len(mallas), 'huesos': len(arm[0].data.bones) if arm else 0, 'triangulos': tri,
            'vertices_sin_peso': sinp if arm else None, 'z_min': round(min(zs), 4), 'altura': round(max(zs), 3),
            'materiales': len(mats), 'texturas': len(tex), 'expresiones': sk, 'bytes': os.path.getsize(ruta)}
rep = {'referencia': {k: ref[k] for k in ('triangulos_exportacion', 'huesos', 'materiales', 'expresiones', 'altura_m')}}
for f in ('operador-mecha.fbx', 'operador-mecha.glb', 'operador-mecha-panel.fbx', 'operador-mecha-panel.glb'):
    rep[f] = medir(os.path.join(OUT, f))
pj = rep['operador-mecha.fbx']; pg = rep['operador-mecha.glb']
rep['coinciden'] = {
    'triangulos': pj['triangulos'] == pg['triangulos'] == ref['triangulos_exportacion'],
    'huesos': pj['huesos'] == pg['huesos'] == ref['huesos'],
    'mallas': pj['mallas'] == pg['mallas'] == 3,
    'sin_vertices_sin_peso': pj['vertices_sin_peso'] == 0 and pg['vertices_sin_peso'] == 0,
    'apoya_en_z0': abs(pj['z_min']) < 0.002 and abs(pg['z_min']) < 0.002,
    'materiales': pj['materiales'] == pg['materiales'] == 3}
json.dump(rep, open(os.path.join(OUT, 'operador-mecha-verificacion.json'), 'w'), indent=1, ensure_ascii=False)
print('OK10', json.dumps(rep, ensure_ascii=False))
