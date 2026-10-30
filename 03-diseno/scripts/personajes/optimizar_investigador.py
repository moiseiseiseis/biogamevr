"""Crea una variante estatica ligera conservando las fuentes y los modificadores.
blender -b --factory-startup --python scripts/personajes/optimizar_investigador.py
La geometria facial secundaria, gafas, cabello y manos sin biseles se conservan.
"""
import bpy,bmesh,json,math
from pathlib import Path
from mathutils.bvhtree import BVHTree

ROOT=Path(__file__).resolve().parent.parents[1]
DATA=ROOT/'validacion'/'personajes';DATA.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'fuentes'/'investigador.blend'
TARGET=ROOT/'fuentes'/'investigador-ligero.blend'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
SCENES=['Investigador_Pose_A','Investigador_Blockout']

def character_objects(scene):
    return [o for o in scene.objects if o.type=='MESH' and all(c.name!='PRESENTATION' for c in o.users_collection)]

def evaluated_mesh(o):
    dg=bpy.context.evaluated_depsgraph_get();e=o.evaluated_get(dg)
    m=bpy.data.meshes.new_from_object(e,preserve_all_data_layers=True,depsgraph=dg)
    m.transform(e.matrix_world);m.calc_loop_triangles();return m

def inventory(scene):
    bpy.context.window.scene=scene;bpy.context.view_layer.update()
    coords=[];triangles=0
    for o in character_objects(scene):
        m=evaluated_mesh(o);triangles+=len(m.loop_triangles);coords.extend(v.co.copy() for v in m.vertices);bpy.data.meshes.remove(m)
    lo=[min(v[i] for v in coords) for i in range(3)];hi=[max(v[i] for v in coords) for i in range(3)]
    assert all(math.isfinite(x) for v in coords for x in v)
    return {'mesh_objects':len(character_objects(scene)),'triangles':triangles,'min_m':lo,'max_m':hi,'dimensions_m':[hi[i]-lo[i] for i in range(3)]}

before={name:inventory(bpy.data.scenes[name]) for name in SCENES}
processed=set();changes=[]
for name in SCENES:
    scene=bpy.data.scenes[name];bpy.context.window.scene=scene;bpy.context.view_layer.update()
    for obj in character_objects(scene):
        if obj.name in processed:continue
        processed.add(obj.name)
        original=evaluated_mesh(obj);count=len(original.loop_triangles);ratio=1.0
        if obj.name.startswith('HEAD_Rostro_'):ratio=.30
        elif 'Chaqueta_Sisas' in obj.name:ratio=.30
        elif obj.name.startswith('JACKET_Cuello_Alto_'):ratio=1.0
        elif obj.name.startswith('HEAD_Oreja_Pabellon_'):ratio=.50
        elif any(m.type=='BEVEL' for m in obj.modifiers) and count>=80:ratio=.50
        if ratio==1:
            bpy.data.meshes.remove(original);continue
        mod=obj.modifiers.new('LOD_Reduccion_Selectiva','DECIMATE');mod.ratio=ratio;mod.use_collapse_triangulate=True
        if 'Chaqueta_Sisas' in obj.name or len(obj.data.materials)>1:
            mod.decimate_type='DISSOLVE';mod.angle_limit=math.radians(3 if 'Chaqueta_Sisas' in obj.name else 1)
            mod.delimit={'MATERIAL','NORMAL'}
            obj.modifiers.new('LOD_Triangulacion_Estable','TRIANGULATE')
        obj.data.update();obj.update_tag()
        bpy.context.view_layer.update();reduced=evaluated_mesh(obj)
        row={'object':obj.name,'method':mod.decimate_type,'ratio':ratio if mod.decimate_type=='COLLAPSE' else None,'angle_limit_degrees':math.degrees(mod.angle_limit) if mod.decimate_type=='DISSOLVE' else None,'triangles_before':count,'triangles_after':len(reduced.loop_triangles)}
        if obj.name.startswith('HEAD_Rostro_') or 'Chaqueta_Sisas' in obj.name:
            trees=[BVHTree.FromPolygons([v.co for v in m.vertices],[list(t.vertices) for t in m.loop_triangles],all_triangles=True) for m in (original,reduced)]
            distances=[trees[1-i].find_nearest(v.co)[3] for i,m in enumerate((original,reduced)) for v in m.vertices]
            row['sampled_bidirectional_max_distance_m']=max(distances)
            assert max(distances)<(.0005 if obj.name.startswith('HEAD_Rostro_') else .003),row
            bm=bmesh.new();bm.from_mesh(reduced);row['nonmanifold_edges']=sum(not e.is_manifold for e in bm.edges);bm.free()
            assert row['nonmanifold_edges']==0,row
        changes.append(row);bpy.data.meshes.remove(original);bpy.data.meshes.remove(reduced)
    scene['asset_variant']='Ligera estatica para personajes secundarios; reduccion selectiva editable.'
    scene['source_asset']='investigador.blend'

after={name:inventory(bpy.data.scenes[name]) for name in SCENES}
for name,row in after.items():
    row['reduction_percent']=round(100*(1-row['triangles']/before[name]['triangles']),2)
    assert row['triangles']<before[name]['triangles']*.70
    assert abs(row['min_m'][2])<1e-5 and abs(row['max_m'][2]-1.78)<.001
    assert max(abs(row['dimensions_m'][i]-before[name]['dimensions_m'][i]) for i in range(3))<.003
assert not any(i.source=='FILE' for i in bpy.data.images)
text=bpy.data.texts.get(Path(__file__).name) or bpy.data.texts.new(Path(__file__).name);text.clear();text.write(Path(__file__).read_text());text.filepath=''
bpy.context.window.scene=bpy.data.scenes['Investigador_Pose_A'];bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(TARGET),compress=True)
report={'blender':bpy.app.version_string,'source':'fuentes/investigador.blend','output':'fuentes/investigador-ligero.blend','before':before,'after':after,'changes':changes,'source_preserved':True,'editable_reduction_modifiers':len(changes),'tested_in_unity':False,'surface_check':'Distancia bidireccional de vertices a superficies; no es una cota continua de Hausdorff.','result':'PASS'}
(DATA/'investigador-optimizacion.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
print('OPTIMIZACION_PASS',json.dumps(after))
