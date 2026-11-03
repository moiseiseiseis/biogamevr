"""05 · Exportación y vistas de la camilla."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
OUT = os.environ['PAC_TRABAJO']; os.environ.setdefault('NEO_TRABAJO', OUT)
import render_vistas as rend, json
bpy.ops.wm.open_mainfile(filepath=OUT + '/camilla-soporte.blend')
cam = bpy.data.objects['Camilla_Soporte']
bpy.ops.object.select_all(action='DESELECT'); cam.select_set(True)
bpy.ops.export_scene.fbx(filepath=OUT + '/camilla-soporte.fbx', use_selection=True, object_types={'MESH'}, use_mesh_modifiers=True,
    axis_forward='-Z', axis_up='Y', bake_space_transform=True, path_mode='COPY', embed_textures=True, mesh_smooth_type='FACE')
bpy.ops.export_scene.gltf(filepath=OUT + '/camilla-soporte.glb', use_selection=True, export_apply=True)
me = cam.data; me.calc_loop_triangles()
bb = [cam.matrix_world @ v.co for v in me.vertices]
rep = {'triangulos': len(me.loop_triangles), 'materiales': [m.name for m in me.materials],
       'dimensiones_m': [round(max(p[i] for p in bb) - min(p[i] for p in bb), 3) for i in range(3)]}
json.dump(rep, open(OUT + '/camilla-geometria.json', 'w'), indent=1, ensure_ascii=False)
rend.setup_and_render('cam', views=('tq', 'side', 'front'), engine='BLENDER_EEVEE', res=(1400, 1000), samples=32, focus=(0, 0, 0.45), ortho=2.5)
c = bpy.data.objects['_R_cam']; d = V((0, -0.001, 1)); c.location = V((0, 0, 0.4)) + d * 6; c.rotation_euler = (-d).to_track_quat('-Z', 'Y').to_euler()
c.data.ortho_scale = 2.4; bpy.context.scene.render.resolution_x, bpy.context.scene.render.resolution_y = (1000, 1400)
bpy.context.scene.render.filepath = OUT + '/cam_top.png'; bpy.ops.render.render(write_still=True)
print('OK05', rep)
