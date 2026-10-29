import bpy, json, math
from pathlib import Path
from mathutils import Vector
ROOT=Path(bpy.data.filepath).parent.parent
DATA=ROOT/'validacion'/'personajes'
s=bpy.context.scene
required=['BODY','HELMET','BACKPACK','SUIT','GLOVES','BOOTS','ACCESSORIES','REFERENCES']
cs={n:s.collection.children.get(n) for n in required}
assert all(cs.values()),cs
assert s.unit_settings.system=='METRIC' and s.unit_settings.scale_length==1
assert bpy.data.objects['CTRL_Explorador_Origen'].location.length<1e-6
objects=[o for n,c in cs.items() if n!='REFERENCES' for o in c.objects if o.type=='MESH']
dg=bpy.context.evaluated_depsgraph_get()
base=evaluated=0;verts=[];invalid=[]
for o in objects:
    assert o.parent.name=='CTRL_Explorador_Origen',o.name
    assert len(o.users_collection)==1,o.name
    o.data.calc_loop_triangles();base+=len(o.data.loop_triangles)
    e=o.evaluated_get(dg);me=e.to_mesh();me.calc_loop_triangles();evaluated+=len(me.loop_triangles)
    verts += [e.matrix_world@v.co for v in me.vertices]
    if any(not math.isfinite(x) for v in me.vertices for x in v.co): invalid.append(o.name)
    e.to_mesh_clear()
lo=[min(p[i] for p in verts) for i in range(3)]
hi=[max(p[i] for p in verts) for i in range(3)]
assert abs(lo[2])<1e-6,(lo,hi)
assert abs(hi[2]-1.7)<.001,(lo,hi)
assert abs(lo[0]+hi[0])<.001
assert not invalid,invalid
assert not any(o.type=='ARMATURE' or o.animation_data for o in s.objects)
assert len(cs['REFERENCES'].objects)==3
assert all(o.data.packed_file for o in cs['REFERENCES'].objects)
report=json.loads((DATA/'explorador-celular-informe.json').read_text())
assert base==report['base_triangles'] and evaluated==report['evaluated_triangles']
assert len(objects)==report['mesh_objects']
assert len(bpy.data.scenes)==1,len(bpy.data.scenes)
checks={'file_reopened':True,'only_one_scene':True,'collections':required,'mesh_objects':len(objects),'base_triangles':base,'evaluated_triangles':evaluated,'min_m':lo,'max_m':hi,'editable_modifiers':sum(len(o.modifiers) for o in objects),'packed_references':3,'result':'PASS'}
(DATA/'explorador-celular-verificacion.json').write_text(json.dumps(checks,indent=2),encoding='utf8')
print('VERIFICACION_PASS',json.dumps(checks))
