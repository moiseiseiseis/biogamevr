import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, shutil, os, addon_utils, sys
from herramientas import enable
bpy.ops.wm.read_factory_settings(use_empty=True)
enable()
r=bpy.ops.import_scene.vrm(filepath=VRM)
print(r)
for o in bpy.data.objects:
    print(o.name,o.type,o.parent.name if o.parent else None, len(o.data.vertices) if o.type=='MESH' else '', [round(v,3) for v in o.dimensions])
arm=[o for o in bpy.data.objects if o.type=='ARMATURE'][0]
print([b.name for b in arm.data.bones])
for o in bpy.data.objects:
    if o.type=='MESH': print(o.name,[m.name for m in o.data.materials], len(o.data.shape_keys.key_blocks) if o.data.shape_keys else 0)
bpy.ops.wm.save_as_mainfile(filepath=W+'/base.blend')
