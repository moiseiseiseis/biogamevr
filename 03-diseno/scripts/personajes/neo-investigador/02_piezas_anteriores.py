import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh
bpy.ops.wm.open_mainfile(filepath=ANTERIOR)
sc=bpy.data.scenes['Investigador_Blockout']; bpy.context.window.scene=sc if bpy.context.window else None
dg=bpy.context.evaluated_depsgraph_get()
groups={
 'A_Mochila': lambda o: o.name.startswith('PACK_') and 'Tirante' not in o.name,
 'A_Credencial': lambda o: o.name.startswith('ACC_Credencial'),
 'A_Gafas': lambda o: o.name.startswith('GLASS_'),
 'A_Tableta': lambda o: o.name in [x.name for x in bpy.data.collections['TABLET'].all_objects],
 'A_Parche': lambda o: o.name.startswith('ARM_Parche_Tejido') and o.name.endswith('_R') ,
}
out={}
for g,f in groups.items():
    obs=[o for o in sc.objects if o.type=='MESH' and f(o)]
    bm=bmesh.new(); mats=[]
    for o in obs:
        ev=o.evaluated_get(dg); m=bpy.data.meshes.new_from_object(ev)
        m.transform(o.matrix_world)
        base=len(mats)
        for mt in m.materials: mats.append(mt)
        for p in m.polygons: p.material_index+=base
        bm.from_mesh(m)
    me=bpy.data.meshes.new(g); bm.to_mesh(me); bm.free()
    for mt in mats: me.materials.append(mt)
    ob=bpy.data.objects.new(g,me); out[g]=ob
    print(g,len(obs),len(me.vertices),[round(x,3) for x in ob.dimensions] if False else '')
# new file with just these
for o in list(bpy.data.objects):
    if o.name not in out: bpy.data.objects.remove(o)
sc0=bpy.data.scenes.new('A'); 
for o in out.values(): sc0.collection.objects.link(o)
for s in list(bpy.data.scenes):
    if s!=sc0: bpy.data.scenes.remove(s)
import mathutils
for o in out.values():
    cs=[v.co for v in o.data.vertices]
    print(o.name,[round(min(c[k] for c in cs),3) for k in range(3)],[round(max(c[k] for c in cs),3) for k in range(3)],
      [ (m.name, tuple(round(x,3) for x in (m.node_tree.nodes['Principled BSDF'].inputs[0].default_value[:3] if m and m.node_tree and 'Principled BSDF' in m.node_tree.nodes else (0,0,0)))) for m in o.data.materials])
bpy.ops.wm.save_as_mainfile(filepath=W+'/assets.blend')
