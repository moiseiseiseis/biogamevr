"""Validate the saved Investigator .blend; run inside Blender, not system Python.
blender -b fuentes/investigador.blend --python scripts/personajes/investigador/validar_investigador.py
Writes validacion/personajes/investigador-verificacion.json and exits on failure.
"""
import bpy
import bmesh
import math
import json
from pathlib import Path

out=Path(bpy.data.filepath).parent.parent/'validacion'/'personajes'
report=json.loads((out/'investigador-informe.json').read_text(encoding='utf8'))
checks={}
scene_names={'reading':'Investigador_Blockout','a_pose':'Investigador_Pose_A'}
for key,name in scene_names.items():
    s=bpy.data.scenes[name];bpy.context.window.scene=s;bpy.context.view_layer.update()
    assert s.unit_settings.system=='METRIC' and s.unit_settings.scale_length==1
    dg=bpy.context.evaluated_depsgraph_get()
    obs=[o for o in s.objects if o.type=='MESH' and all(c.name!='PRESENTATION' for c in o.users_collection)]
    base=evaluated=0;coords=[]
    for o in obs:
        assert o.parent and o.parent.name=='CTRL_Investigador_Origen',o.name
        assert len(o.users_collection)==1,o.name
        o.data.calc_loop_triangles();base+=len(o.data.loop_triangles)
        e=o.evaluated_get(dg);me=e.to_mesh();me.calc_loop_triangles();evaluated+=len(me.loop_triangles)
        coords.extend(e.matrix_world@v.co for v in me.vertices)
        assert all(math.isfinite(x) for v in me.vertices for x in v.co),o.name
        e.to_mesh_clear()
    lo=[min(p[i] for p in coords) for i in range(3)];hi=[max(p[i] for p in coords) for i in range(3)]
    assert abs(lo[2])<1e-5,(key,lo)
    assert abs(hi[2]-1.78)<.004,(key,hi)
    assert len(obs)==report[key]['mesh_objects'],(key,len(obs))
    assert base==report[key]['base_triangles'] and evaluated==report[key]['evaluated_triangles']
    assert not any(o.type=='ARMATURE' or o.animation_data for o in s.objects)
    coat=s.objects['JACKET_Chaqueta_Sisas_Continuas'+('' if key=='reading' else '_A')]
    bm=bmesh.new();bm.from_mesh(coat.data)
    assert not any(not v.link_faces for v in bm.verts),'Vertice suelto en chaqueta'
    assert not any(len(e.link_faces)>2 for e in bm.edges),'Borde con mas de2caras en chaqueta'
    pending=set(bm.verts);components=0
    while pending:
        components+=1;stack=[pending.pop()]
        while stack:
            v=stack.pop()
            for edge in v.link_edges:
                other=edge.other_vert(v)
                if other in pending:pending.remove(other);stack.append(other)
    assert components==1,('Chaqueta desconectada',components)
    bm.free()
    ev=coat.evaluated_get(dg);me=ev.to_mesh();bm=bmesh.new();bm.from_mesh(me)
    nonmanifold=sum(not e.is_manifold for e in bm.edges)
    assert nonmanifold==0,('Espesor de chaqueta no cerrado',nonmanifold)
    bm.free();ev.to_mesh_clear()
    checks[key]={'scene':name,'mesh_objects':len(obs),'base_triangles':base,'evaluated_triangles':evaluated,'min_m':lo,'max_m':hi,'dimensions_m':[hi[i]-lo[i] for i in range(3)],'finite_vertices':True,'feet_z0':True,'height_1_78m':True,'no_rig_or_animation':True,'coat_connected_components':components,'coat_evaluated_nonmanifold_edges':nonmanifold}
read=bpy.data.scenes['Investigador_Blockout'];technical=bpy.data.scenes['Investigador_Pose_A']
assert read.collection.children.get('ARMS_LECTURA') and read.collection.children.get('TABLET')
assert technical.collection.children.get('ARMS_A') and not technical.collection.children.get('TABLET')
assert not technical.collection.children.get('ARMS_LECTURA')
for name in ['HEAD','HAIR','GLASSES','JACKET','PANTS','BOOTS','BACKPACK','ACCESSORIES']:
    assert read.collection.children[name]==technical.collection.children[name]
refs=read.collection.children['REFERENCES']
assert len(refs.objects)==1 and refs.objects[0].data.packed_file
assert bpy.data.objects['CTRL_Investigador_Origen'].location.length<1e-6
checks.update({'result':'PASS','file_reopened':True,'packed_reference':True,'pose_variants_separated':True,'static_geometry_shared':True,'editable_modifiers':sum(len(o.modifiers) for o in read.objects),'blender':bpy.app.version_string})
(out/'investigador-verificacion.json').write_text(json.dumps(checks,indent=2,ensure_ascii=False),encoding='utf8')
print('INVESTIGADOR_VALIDATION_PASS',json.dumps(checks))
