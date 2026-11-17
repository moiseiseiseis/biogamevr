"""01 · Importa el VRM del Operador del Mecha y nombra sus mallas."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
sys.path.insert(0, NEO_SCRIPTS)
import bpy
from herramientas import enable
enable()
for o in list(bpy.data.objects): bpy.data.objects.remove(o)          # escena vacía (sin cubo, cámara ni luz)
r = bpy.ops.import_scene.vrm(filepath=VRM)
assert r == {'FINISHED'}, r
sc = bpy.context.scene; sc.name = 'Operador_Mecha'
arm = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
arm.name = arm.data.name = 'Operador_Mecha_Rig'
for old, new in (('Body', 'VRoid_Cuerpo'), ('Face', 'VRoid_Cara')):
    o = bpy.data.objects[old]; o.name = o.data.name = new
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '01_importado.blend'))
print('OK01', [(o.name, o.type) for o in bpy.data.objects if o.type in ('MESH', 'ARMATURE')])
