"""06 · Prueba de pose: brazos abajo ~65°, codos ~55°, una rodilla flexionada y el torso girado."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '05_pesos.blend'))
rig = bpy.data.objects['Operador_Mecha_Rig']; pb = rig.pose.bones
def R(n, axis, deg):
    b = pb[n]; b.rotation_mode = 'QUATERNION'
    q = mathutils.Quaternion(axis, math.radians(deg)); mw = rig.data.bones[n].matrix_local.to_quaternion()
    b.rotation_quaternion = mw.inverted() @ q @ mw
for s, lab in ((1, 'L'), (-1, 'R')):
    R('J_Bip_%s_UpperArm' % lab, (0, 1, 0), 65 * s)
    R('J_Bip_%s_LowerArm' % lab, (0, 0, 1), 55 * s)
R('J_Bip_L_UpperLeg', (1, 0, 0), -35); R('J_Bip_L_LowerLeg', (1, 0, 0), 55)
R('J_Bip_C_Spine', (0, 0, 1), 18); R('J_Bip_C_Head', (0, 0, 1), -10)
bpy.context.view_layer.update()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '06_pose.blend'))
print('OK06')
