import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,bmesh,sys,json
from herramientas import enable; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/out/investigador-vroid.blend')
arm=bpy.data.objects['Investigador_Rig']
rm=[b.name for b in arm.data.bones if 'CoatSkirt' in b.name or b.name.startswith('J_Opt_C_Glasses')]
# safety: verify zero weights
for o in bpy.data.objects:
    if o.type!='MESH': continue
    for n in rm:
        vg=o.vertex_groups.get(n)
        if vg:
            assert not any(g.group==vg.index and g.weight>0 for v in o.data.vertices for g in v.groups), (o.name,n)
            o.vertex_groups.remove(vg)
sb=arm.data.vrm_addon_extension.spring_bone1
for i in reversed(range(len(sb.springs))):
    s=sb.springs[i]
    if any(j.node.bone_name in rm for j in s.joints): sb.springs.remove(i)
bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode='EDIT')
eb=arm.data.edit_bones
for n in rm:
    if n in eb: eb.remove(eb[n])
bpy.ops.object.mode_set(mode='OBJECT')
print('removed bones',len(rm),'bones now',len(arm.data.bones),'springs',len(sb.springs))
# ---- hidden geometry
def trim(name, pred):
    o=bpy.data.objects[name]; bm=bmesh.new(); bm.from_mesh(o.data)
    n0=len(bm.faces)
    dele=[f for f in bm.faces if pred(f.calc_center_median())]
    bmesh.ops.delete(bm,geom=dele,context='FACES')
    bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')
    bm.to_mesh(o.data); print(name,n0,'->',len(bm.faces)); bm.free()
# shirt: under jacket sleeves, back half below collar, inside pants
trim('INV_Camisa', lambda c: (0.16<abs(c.x)<0.44 and c.z>1.30) or (c.y>0.0 and c.z<1.44 and abs(c.x)<0.22) or c.z<0.99)
# pants: inside boots
trim('INV_Pantalon', lambda c: c.z<0.27)
# purge
for _ in range(3): bpy.ops.outliner.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)
print('images',[(i.name,i.size[:],bool(i.packed_file), i.users) for i in bpy.data.images])
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage8.blend', compress=True)
