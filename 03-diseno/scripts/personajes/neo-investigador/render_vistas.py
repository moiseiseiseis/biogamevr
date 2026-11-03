import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, sys, math, mathutils
def setup_and_render(prefix, views=('front','side','back'), engine='CYCLES', res=(700,1000), samples=24, focus=None, ortho=2.0):
    sc=bpy.context.scene
    for o in list(bpy.data.objects):
        if o.name.startswith('_R_'): bpy.data.objects.remove(o)
    sc.render.engine=engine
    if engine=='CYCLES':
        sc.cycles.samples=samples; sc.cycles.device='CPU'; sc.cycles.use_denoising=True
    sc.render.resolution_x,sc.render.resolution_y=res
    sc.render.film_transparent=False
    w=bpy.data.worlds.get('W') or bpy.data.worlds.new('W'); sc.world=w; w.use_nodes=True
    w.node_tree.nodes['Background'].inputs[0].default_value=(0.55,0.57,0.6,1); w.node_tree.nodes['Background'].inputs[1].default_value=1.0
    sc.view_settings.view_transform='Standard'
    # sun
    ld=bpy.data.lights.new('_R_sun','SUN'); ld.energy=3.0
    l=bpy.data.objects.new('_R_sun',ld); sc.collection.objects.link(l); l.rotation_euler=(math.radians(50),0,math.radians(30))
    cd=bpy.data.cameras.new('_R_cam'); cd.type='ORTHO'; cd.ortho_scale=ortho
    cam=bpy.data.objects.new('_R_cam',cd); sc.collection.objects.link(cam); sc.camera=cam
    fz = focus if focus else (0,0,0.9)
    dirs={'front':(0,-1,0),'side':(1,0,0),'back':(0,1,0),'tq':(0.7,-0.7,0),'lside':(-1,0,0)}
    out=[]
    for v in views:
        d=mathutils.Vector(dirs[v]).normalized()
        cam.location=mathutils.Vector(fz)+d*6
        cam.rotation_euler=(-d).to_track_quat('-Z','Y').to_euler()
        l.rotation_euler=((-d).to_track_quat('-Z','Y').to_euler()); l.rotation_euler.x+=math.radians(25); l.rotation_euler.z+=math.radians(25)
        sc.render.filepath=f'{W}/{prefix}_{v}.png'
        bpy.ops.render.render(write_still=True); out.append(sc.render.filepath)
    return out
