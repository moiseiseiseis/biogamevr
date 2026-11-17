"""11 · Vistas EEVEE sobre fondo gris azulado claro: pose T (frente, lateral, posterior, tres cuartos),
rostro, pose relajada y prop del panel. Los PNG quedan en OPM_TRABAJO; las hojas las compone componer_vistas.py."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
import render_vistas as rend
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, 'out', 'operador-mecha.blend'))
O = bpy.data.objects; sc = bpy.context.scene; rig = O['Operador_Mecha_Rig']
panel = O['OPM_Panel_Holografico']; panel.hide_render = True
for o in O:
    if o.type == 'EMPTY' or 'Collider' in o.name: o.hide_render = True
FONDO = (0.62, 0.66, 0.72, 1)
def fondo():
    sc.world.node_tree.nodes['Background'].inputs[0].default_value = FONDO
rend.setup_and_render('opm', views=('front',), engine='BLENDER_EEVEE', res=(1000, 1500), samples=32, focus=(0, 0, 0.82), ortho=1.85)
fondo()
rend.setup_and_render('opm', views=('front', 'side', 'back', 'tq'), engine='BLENDER_EEVEE', res=(1000, 1500), samples=32, focus=(0, 0, 0.82), ortho=1.85)
fondo()
rend.setup_and_render('opm-rostro', views=('front', 'tq'), engine='BLENDER_EEVEE', res=(1100, 1100), focus=(0, 0, 1.44), ortho=0.48, samples=32)
fondo()
pb = rig.pose.bones
def R(n, axis, deg):
    b = pb[n]; b.rotation_mode = 'QUATERNION'
    q = mathutils.Quaternion(axis, math.radians(deg)); mw = rig.data.bones[n].matrix_local.to_quaternion()
    b.rotation_quaternion = mw.inverted() @ q @ mw
for s, lab in ((1, 'L'), (-1, 'R')):
    R('J_Bip_%s_UpperArm' % lab, (0, 1, 0), 70 * s); R('J_Bip_%s_LowerArm' % lab, (0, 0, 1), 20 * s)
R('J_Bip_C_Head', (1, 0, 0), 5)
bpy.context.view_layer.update()
rend.setup_and_render('opm-pose', views=('front', 'tq', 'back'), engine='BLENDER_EEVEE', res=(1000, 1500), samples=32, focus=(0, 0, 0.82), ortho=1.85)
fondo()
# prop del panel: solo, centrado, sobre fondo oscuro para que se lea el holograma
for o in O: o.hide_render = True
panel.hide_render = False
sc.world.node_tree.nodes['Background'].inputs[0].default_value = (0.05, 0.06, 0.08, 1)
rend.setup_and_render('opm-panel', views=('tq',), engine='BLENDER_EEVEE', res=(1100, 800), focus=(0, 0, 0), ortho=0.48, samples=32)
print('OK11', sorted(f for f in os.listdir(W) if f.startswith('opm')))   # render_vistas escribe en W (rutas del Operador)
