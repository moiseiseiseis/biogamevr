"""Exportaciones estaticas de los personajes. Ejecutar dentro de Blender.
blender -b --python scripts/personajes/exportar_personajes.py
Conserva fuentes editables; exporta copias evaluadas con transformaciones aplicadas.
"""
import bpy,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent.parents[1]
DEST=ROOT/'exports';DEST.mkdir(parents=True,exist_ok=True)
DATA=ROOT/'validacion'/'personajes';DATA.mkdir(parents=True,exist_ok=True)
summary=[]

def export_scene(source_scene,name):
    bpy.context.window.scene=source_scene;bpy.context.view_layer.update()
    dg=bpy.context.evaluated_depsgraph_get()
    originals=[o for o in source_scene.objects if o.type=='MESH' and all(c.name!='PRESENTATION' for c in o.users_collection)]
    snapshot=[]
    for o in originals:
        e=o.evaluated_get(dg)
        m=bpy.data.meshes.new_from_object(e,preserve_all_data_layers=True,depsgraph=dg)
        m.transform(e.matrix_world);m.update();m.calc_loop_triangles()
        snapshot.append((o.name.lower(),m))
    temp=bpy.data.scenes.new('EXPORT_'+name);temp.unit_settings.system='METRIC';temp.unit_settings.scale_length=1
    bpy.context.window.scene=temp
    anchor=bpy.data.objects.new(name,None);temp.collection.objects.link(anchor);anchor.select_set(True)
    coords=[];tris=0
    for label,m in snapshot:
        o=bpy.data.objects.new(label,m);temp.collection.objects.link(o);o.parent=anchor;o.select_set(True)
        coords.extend(v.co.copy() for v in m.vertices);tris+=len(m.loop_triangles)
    bpy.context.view_layer.objects.active=anchor
    fbx=DEST/(name+'.fbx');glb=DEST/(name+'.glb')
    bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH','EMPTY'},use_mesh_modifiers=False,global_scale=1.0,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=True,bake_anim=False,add_leaf_bones=False,path_mode='STRIP',embed_textures=False)
    bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,use_active_scene=True,export_apply=False,export_yup=True,export_materials='EXPORT',export_cameras=False,export_lights=False,export_extras=False,export_animations=False,export_skins=False)
    lo=[min(v[i] for v in coords) for i in range(3)];hi=[max(v[i] for v in coords) for i in range(3)]
    row={'asset':name,'scene':source_scene.name,'mesh_objects':len(snapshot),'evaluated_triangles':tris,'min_m':lo,'max_m':hi,'dimensions_m':[hi[i]-lo[i] for i in range(3)],'static':True,'transforms_applied':True,'fbx_axes':{'forward':'-Z','up':'Y'},'files':[{'path':str(p.relative_to(ROOT)),'bytes':p.stat().st_size} for p in [fbx,glb]]}
    summary.append(row)
    for o in list(temp.objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.scenes.remove(temp)
    bpy.context.window.scene=source_scene

bpy.ops.wm.open_mainfile(filepath=str(ROOT/'fuentes'/'explorador-celular.blend'))
export_scene(bpy.context.scene,'explorador-celular')
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'fuentes'/'investigador.blend'))
export_scene(bpy.data.scenes['Investigador_Pose_A'],'investigador')
export_scene(bpy.data.scenes['Investigador_Blockout'],'investigador-lectura')
(DATA/'exportaciones.json').write_text(json.dumps({'blender':bpy.app.version_string,'assets':summary},indent=2,ensure_ascii=False),encoding='utf8')
print('EXPORTACIONES_COMPLETAS',json.dumps(summary))
