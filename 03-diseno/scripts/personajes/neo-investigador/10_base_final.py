import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,json,bmesh
from herramientas import *; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage7.blend')
arm=bpy.data.objects['Armature']; arm.name='Investigador_Rig'; arm.data.name='Investigador_Rig'
for n in ('Body','Face','Hair'):
    o=bpy.data.objects[n]; o.name='VRoid_'+{'Body':'Cuerpo','Face':'Cara','Hair':'Cabello'}[n]
col=bpy.data.collections['Investigador_Ropa']; col.name='Investigador_Ropa_y_Equipo'
for p in arm.pose.bones: p.rotation_quaternion=(1,0,0,0); p.rotation_euler=(0,0,0); p.location=(0,0,0); p.scale=(1,1,1)
try: bpy.ops.file.pack_all()
except Exception as e: print('pack',e)
bpy.context.scene.name='Investigador_VRoid'
for n,mods in (('INV_Camisa',('Grosor','Suavizado')),('INV_Pantalon',('Grosor',))):
    o=bpy.data.objects[n]
    for mn in mods:
        if o.modifiers.get(mn): o.modifiers.remove(o.modifiers[mn])
# validation
dg=bpy.context.evaluated_depsgraph_get()
rep={'meshes':[], 'bones':len(arm.data.bones)}
tot=0; totb=0
for o in bpy.data.objects:
    if o.type!='MESH': continue
    me=o.data; unw=sum(1 for v in me.vertices if not v.groups or sum(g.weight for g in v.groups)<0.01)
    ev=o.evaluated_get(dg); m=ev.to_mesh(); m.calc_loop_triangles(); t=len(m.loop_triangles); ev.to_mesh_clear()
    tb=sum(len(p.vertices)-2 for p in me.polygons)
    tot+=t; totb+=tb
    rep['meshes'].append({'nombre':o.name,'vertices':len(me.vertices),'tri_base':tb,'tri_evaluados':t,'sin_peso':unw,'padre':o.parent.name if o.parent else None})
rep['tri_total_evaluados']=tot; rep['tri_total_base']=totb
cs=[ (o.matrix_world@v.co).z for o in bpy.data.objects if o.type=='MESH' and o.parent==arm for v in o.data.vertices]
rep['altura_m']=round(max(cs)-min(cs),3); rep['z_min']=round(min(cs),4)
json.dump(rep,open(W+'/out/investigador-vroid-validacion.json','w'),indent=1,ensure_ascii=False)
print('tri',tot,totb,'h',rep['altura_m'],rep['z_min'],'unweighted',[m['nombre'] for m in rep['meshes'] if m['sin_peso'] and m['padre']])
bpy.ops.wm.save_as_mainfile(filepath=W+'/out/investigador-vroid.blend', compress=True)
# exports: game version without subdivision
for o in bpy.data.objects:
    for md in o.modifiers:
        if md.type=='SUBSURF': md.show_viewport=False; md.show_render=False
dg=bpy.context.evaluated_depsgraph_get(); tg=0
for o in bpy.data.objects:
    if o.type=='MESH' and o.name!='INV_Tableta_Prop':
        ev=o.evaluated_get(dg); m=ev.to_mesh(); m.calc_loop_triangles(); tg+=len(m.loop_triangles); ev.to_mesh_clear()
rep['tri_exportacion_juego']=tg; json.dump(rep,open(W+'/out/investigador-vroid-validacion.json','w'),indent=1,ensure_ascii=False)
print('tri game',tg)
# exports (character only, no prop for skinned files)
tb=bpy.data.objects['INV_Tableta_Prop']
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
    if o.type in ('MESH','ARMATURE') and o!=tb: o.select_set(True)
bpy.ops.export_scene.fbx(filepath=W+'/out/investigador-vroid.fbx', use_selection=True, object_types={'ARMATURE','MESH'},
    use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', embed_textures=True, mesh_smooth_type='FACE')
bpy.ops.export_scene.gltf(filepath=W+'/out/investigador-vroid.glb', use_selection=True, export_apply=True, export_animations=False, export_morph=True)
# tablet prop separately
bpy.ops.object.select_all(action='DESELECT'); tb.select_set(True)
bpy.ops.export_scene.fbx(filepath=W+'/out/tableta-prop.fbx', use_selection=True, object_types={'MESH'}, path_mode='COPY', embed_textures=True)
bpy.data.objects.remove(tb)
try:
    r=bpy.ops.export_scene.vrm(filepath=W+'/out/investigador-vroid.vrm', armature_object_name='Investigador_Rig')
    print('vrm',r)
except Exception as e: print('vrm fail',str(e)[:400])
