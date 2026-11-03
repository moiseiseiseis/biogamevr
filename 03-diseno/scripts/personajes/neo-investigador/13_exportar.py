import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,json,os
from herramientas import enable; enable()
O=W+'/out2'
bpy.ops.wm.open_mainfile(filepath=W+'/stage9.blend')
arm=bpy.data.objects['Investigador_Rig']; arm.name='Neo_Investigador_Rig'; arm.data.name='Neo_Investigador_Rig'
bpy.context.scene.name='Neo_Investigador'
dg=bpy.context.evaluated_depsgraph_get()
def tris(skip_sub):
    st={}
    for o in bpy.data.objects:
        for md in o.modifiers:
            if md.type=='SUBSURF': md.show_viewport=not skip_sub
    dg=bpy.context.evaluated_depsgraph_get(); tot=0
    for o in bpy.data.objects:
        if o.type=='MESH':
            ev=o.evaluated_get(dg); m=ev.to_mesh(); m.calc_loop_triangles(); st[o.name]=len(m.loop_triangles); ev.to_mesh_clear()
    return st
full=tris(False)
bpy.ops.wm.save_as_mainfile(filepath=O+'/neo-investigador.blend', compress=True)
game=tris(True)
tb=bpy.data.objects['INV_Tableta_Prop']
char=[o for o in bpy.data.objects if o.type in('MESH','ARMATURE') and o!=tb]
bpy.ops.object.select_all(action='DESELECT')
for o in char: o.select_set(True)
bpy.ops.export_scene.fbx(filepath=O+'/neo-investigador.fbx', use_selection=True, object_types={'ARMATURE','MESH'},
    use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z', axis_up='Y', bake_space_transform=False, path_mode='COPY', embed_textures=True, mesh_smooth_type='FACE')
bpy.ops.export_scene.gltf(filepath=O+'/neo-investigador.glb', use_selection=True, export_apply=True, export_animations=False, export_morph=True)
bpy.ops.object.select_all(action='DESELECT'); tb.select_set(True)
tb.location=(0,0,0)   # la tableta se exporta centrada en el origen
bpy.ops.export_scene.fbx(filepath=O+'/neo-investigador-tableta.fbx', use_selection=True, object_types={'MESH'},
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, path_mode='COPY', embed_textures=True)
bpy.data.objects.remove(tb)
bpy.ops.export_scene.vrm(filepath=O+'/neo-investigador.vrm', armature_object_name='Neo_Investigador_Rig')
rep={'tri_blend_presentacion':sum(full.values()),'tri_exportacion':sum(v for k,v in game.items() if k!='INV_Tableta_Prop'),
     'tri_tableta':game['INV_Tableta_Prop'],'por_malla_exportacion':{k:v for k,v in sorted(game.items(),key=lambda x:-x[1])},
     'huesos':len(arm.data.bones),'resortes_vrm':len(arm.data.vrm_addon_extension.spring_bone1.springs)}
json.dump(rep,open(O+'/tri.json','w'),indent=1,ensure_ascii=False)
print(rep['tri_blend_presentacion'],rep['tri_exportacion'],rep['huesos'])

import shutil
for f in ('neo-investigador.blend',):
    shutil.copy2(O+'/'+f, os.path.join(FUENTES,f))
for f in ('neo-investigador.fbx','neo-investigador.glb','neo-investigador-tableta.fbx'):
    shutil.copy2(O+'/'+f, os.path.join(EXPORTS,f))
json.dump(rep,open(os.path.join(VALIDACION,'neo-investigador-geometria.json'),'w'),indent=1,ensure_ascii=False)
