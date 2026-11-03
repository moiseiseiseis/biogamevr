import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys
from herramientas import *; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage7.blend')
m=bpy.data.materials['INV_Lente_Azul']; e=m.vrm_addon_extension.mtoon1
e.pbr_metallic_roughness.base_color_factor=(*srgb('#d8ecf6'),0.06)
e.extensions.vrmc_materials_mtoon.shade_color_factor=srgb('#d8ecf6')
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage7.blend')
