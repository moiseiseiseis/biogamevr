import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh, math, sys
from herramientas import *
enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage5.blend')
arm=bpy.data.objects['Armature']
gl=bpy.data.objects['INV_Gafas']; gl.visible_shadow=False
# softer toon terminator on cloth
for m in bpy.data.materials:
    if m.name.startswith('INV_') and m.name not in ('INV_Metal','INV_Lente_Azul','INV_Pantalla','INV_Pantalla_Cian'):
        mt=m.vrm_addon_extension.mtoon1.extensions.vrmc_materials_mtoon
        mt.shading_toony_factor=0.62; mt.shading_shift_factor=-0.15
# subdivision for soft cloth (before armature)
for n in ('INV_Chaqueta','INV_Pantalon','INV_Camisa','INV_Bota_L','INV_Bota_R','INV_Chaqueta_Cuello'):
    o=bpy.data.objects[n]
    s=o.modifiers.new('Suavizado','SUBSURF'); s.levels=1; s.render_levels=1; s.quality=2
    bpy.context.view_layer.objects.active=o
    with bpy.context.temp_override(object=o,active_object=o):
        bpy.ops.object.modifier_move_to_index(modifier='Suavizado', index=len(o.modifiers)-2)
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage6.blend')
