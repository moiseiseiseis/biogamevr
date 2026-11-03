import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,math,mathutils
from herramientas import enable; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/out2/neo-investigador.blend')
import render_vistas as rend
for o in bpy.data.objects:
    if o.type=='EMPTY' or o.name=='INV_Tableta_Prop': o.hide_render=True
sc=bpy.context.scene
P='renders/neo-investigador'
rend.setup_and_render(P,views=('front','side','back','tq'),engine='BLENDER_EEVEE',res=(1000,1500),samples=32)
w=sc.world.node_tree.nodes['Background']; w.inputs[0].default_value=(0.62,0.66,0.72,1)
rend.setup_and_render(P+'-rostro',views=('front','tq'),engine='BLENDER_EEVEE',res=(1100,1100),focus=(0,0,1.50),ortho=0.55,samples=32)
arm=bpy.data.objects['Neo_Investigador_Rig']; pb=arm.pose.bones
def R(n,axis,deg):
    b=pb[n]; b.rotation_mode='QUATERNION'
    q=mathutils.Quaternion(axis, math.radians(deg)); mw=arm.data.bones[n].matrix_local.to_quaternion()
    b.rotation_quaternion=mw.inverted()@q@mw
for s,lab in ((1,'L'),(-1,'R')):
    R('J_Bip_%s_UpperArm'%lab,(0,1,0),70*s); R('J_Bip_%s_LowerArm'%lab,(0,0,1),20*s)
R('J_Bip_C_Head',(1,0,0),5)
bpy.context.view_layer.update()
rend.setup_and_render(P+'-pose',views=('front','tq','back'),engine='BLENDER_EEVEE',res=(1000,1500),samples=32,ortho=2.0)
# composición y copia a vistas/personajes/neo-investigador
from PIL import Image
import shutil
os.makedirs(VISTAS, exist_ok=True)
nombres={'front':'frente','side':'lateral','back':'posterior','tq':'tres-cuartos'}
def hoja(prefix, views, out):
    ims=[Image.open(f'{W}/{prefix}_{v}.png') for v in views]
    c=Image.new('RGB',(sum(i.width for i in ims),ims[0].height)); x=0
    for i in ims: c.paste(i,(x,0)); x+=i.width
    c.save(out, optimize=True)
for v,n in nombres.items():
    Image.open(f'{W}/{P}_{v}.png').convert('RGB').save(os.path.join(VISTAS,f'neo-investigador-{n}.png'), optimize=True)
hoja(P,('front','side','back','tq'),os.path.join(VISTAS,'neo-investigador-vistas.png'))
hoja(P+'-rostro',('front','tq'),os.path.join(VISTAS,'neo-investigador-rostro.png'))
hoja(P+'-pose',('front','tq','back'),os.path.join(VISTAS,'neo-investigador-pose-relajada.png'))
