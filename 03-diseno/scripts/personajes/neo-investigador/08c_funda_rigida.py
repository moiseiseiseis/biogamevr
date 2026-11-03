import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys
from herramientas import *; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage7.blend')
o=bpy.data.objects['INV_Funda_R']
for vg in list(o.vertex_groups): o.vertex_groups.remove(vg)
rigid_weight(o,'J_Bip_R_UpperLeg')
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage7.blend')
