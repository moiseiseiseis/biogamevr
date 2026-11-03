import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,json
from herramientas import enable; enable()
out={}
for f,op in (('neo-investigador.fbx','fbx'),('neo-investigador.glb','gltf'),('neo-investigador.vrm','vrm'),('neo-investigador-tableta.fbx','fbx')):
    bpy.ops.wm.read_factory_settings(use_empty=True); enable()
    getattr(bpy.ops.import_scene,op)(filepath=os.path.join(EXPORTS if not f.endswith('.vrm') else W+'/out2',f))
    ms=[o for o in bpy.data.objects if o.type=='MESH' and o.name!='Icosphere']; ar=[o for o in bpy.data.objects if o.type=='ARMATURE']
    tri=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in ms)
    unw=sum(1 for o in ms for v in o.data.vertices if not v.groups) if ar else None
    zs=[(o.matrix_world@v.co).z for o in ms for v in o.data.vertices]
    out[f]={'mallas':len(ms),'armaduras':len(ar),'huesos':len(ar[0].data.bones) if ar else 0,'triangulos':tri,
            'vertices_sin_peso':unw,'altura_m':round(max(zs)-min(zs),3),'z_min':round(min(zs),4),
            'materiales':len(set(m.name for o in ms for m in o.data.materials if m))}
    print(f,out[f])
json.dump(out,open(os.path.join(VALIDACION,'neo-investigador-verificacion.json'),'w'),indent=1,ensure_ascii=False)
