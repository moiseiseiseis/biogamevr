import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh, math, numpy as np, sys
from herramientas import *
enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage3.blend')
body=bpy.data.objects['Body']; arm=bpy.data.objects['Armature']; face=bpy.data.objects['Face']
col=bpy.data.collections['Investigador_Ropa']
def mesh_bvh(obs):
    dg=bpy.context.evaluated_depsgraph_get(); bm=bmesh.new()
    for ob in obs:
        m=bpy.data.meshes.new_from_object(ob.evaluated_get(dg)); bm.from_mesh(m)
    r=BVHTree.FromBMesh(bm); bm.free(); return r
jk=bpy.data.objects['INV_Chaqueta']; cl=bpy.data.objects['INV_Chaqueta_Cuello']
# armature modifiers off for evaluation = rest pose anyway
jb=mesh_bvh([jk])
M={m.name:m for m in bpy.data.materials}
with bpy.data.libraries.load(W+'/assets.blend') as (src,dst):
    dst.objects=[n for n in src.objects]
A={o.name:o for o in dst.objects}
for o in A.values(): col.objects.link(o)
# material mapping to MToon palette
pal={'MAT_Glasses_Graphite':mtoon('INV_Grafito_Duro',srgb('#2c2e33'),srgb('#1b1c20')),
     'MAT_Technical_Charcoal':M['INV_Chaqueta_Grafito'],
     'MAT_07_Metal_mate':bpy.data.materials['INV_Metal'],
     'MAT_Mission_Ochre':M['INV_Acento_Amarillo'],
     'MAT_Jacket_Porcelain':M['INV_Chaqueta_Gris'],
     'MAT_Jacket_Shadow':mtoon('INV_Gris_Sombra',srgb('#8d908f'),srgb('#6c6f6f')),
     'MAT_Lens_SubtleBlue':mtoon('INV_Lente_Azul',srgb('#9fc3d6'),srgb('#7fa3b6'),alpha=0.18),
     'MAT_08_Pantalla_analisis':mtoon('INV_Pantalla',srgb('#10222e'),srgb('#0b1820')),
     'MAT_09_Lectura_cian':mtoon('INV_Pantalla_Cian',srgb('#6fd3e8'),srgb('#4fb0c4'))}
try:
    pal['MAT_09_Lectura_cian'].vrm_addon_extension.mtoon1.emissive_factor=srgb('#3aa9c2')
except Exception as e: print('emissive',e)
for o in A.values():
    for i,m in enumerate(o.data.materials):
        o.data.materials[i]=pal.get(m.name.split('.')[0], m)
    for p in o.data.polygons: p.use_smooth=False
def move(o,d):
    o.data.transform(mathutils.Matrix.Translation(d))
def bbox(o):
    cs=np.array([v.co[:] for v in o.data.vertices]); return cs.min(0),cs.max(0)

# --- BACKPACK
pk=A['A_Mochila']; pk.name='INV_Mochila'; pk.data.name='INV_Mochila'
h=jb.ray_cast(V((0,0.6,1.30)),V((0,-1,0)))
yb=h[0].y
mn,mx=bbox(pk)
# contact: lowest y among verts near center column
cy_=min(v.co.y for v in pk.data.vertices if abs(v.co.x)<0.05 and 1.25<v.co.z<1.35)
move(pk,V((0,yb+0.006-cy_,-0.02)))
rigid_weight(pk,'J_Bip_C_Chest'); finish(pk,arm,[],smooth_shade=False)

# --- STRAPS (ribbons projected on jacket/collar)
sb=mesh_bvh([jk,cl])
def ribbon(name, ctrl, width=0.04, off=0.005, n=60):
    # catmull-rom through ctrl
    P=[V(c) for c in ctrl]; pts=[]
    for i in range(len(P)-1):
        p0=P[max(i-1,0)]; p1=P[i]; p2=P[i+1]; p3=P[min(i+2,len(P)-1)]
        for k in range(n//(len(P)-1)):
            t=k/(n//(len(P)-1))
            pts.append(0.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t**3))
    pts.append(P[-1])
    S=[];N=[]
    for p in pts:
        loc,nor,_,_=sb.find_nearest(p); S.append(loc+nor*off); N.append(nor)
    # smooth path
    for _ in range(3):
        S=[S[0]]+[(S[i-1]+S[i]*2+S[i+1])/4 for i in range(1,len(S)-1)]+[S[-1]]
    vs=[];fs=[]
    for i,(p,nr) in enumerate(zip(S,N)):
        t=(S[min(i+1,len(S)-1)]-S[max(i-1,0)]).normalized()
        b=t.cross(nr).normalized()
        vs+= [tuple(p-b*width/2), tuple(p+b*width/2)]
    for i in range(len(S)-1):
        fs.append((2*i,2*i+1,2*i+3,2*i+2))
    m=bpy.data.meshes.new(name); m.from_pydata(vs,[],fs); o=bpy.data.objects.new(name,m); col.objects.link(o)
    bmr=bmesh.new(); bmr.from_mesh(m); bmr.normal_update(); bmr.faces.ensure_lookup_table()
    f=bmr.faces[len(bmr.faces)//2]; loc,nor,_,_=sb.find_nearest(f.calc_center_median())
    if f.normal.dot(nor)<0: bmesh.ops.reverse_faces(bmr,faces=bmr.faces)
    bmr.to_mesh(m); bmr.free()
    return o
for s,lab in ((1,'L'),(-1,'R')):
    ctrl=[(s*0.09,yb+0.02,1.40),(s*0.10,0.09,1.50),(s*0.112,0.01,1.545),(s*0.118,-0.08,1.50),(s*0.122,-0.13,1.40),
          (s*0.125,-0.135,1.29),(s*0.15,-0.09,1.20),(s*0.17,0.0,1.16),(s*0.14,yb+0.03,1.15)]
    st=ribbon('INV_Mochila_Tirante_'+lab, ctrl)
    transfer_weights(st,body); finish(st,arm,[M['INV_Chaqueta_Grafito']]); add_solidify(st,0.005,offset=1)

# --- BADGE
bd=A['A_Credencial']; bd.name='INV_Credencial'; bd.data.name='INV_Credencial'
mn,mx=bbox(bd); cx=(mn[0]+mx[0])/2; cz=(mn[2]+mx[2])/2
h=jb.ray_cast(V((cx,-0.6,cz)),V((0,1,0)))
move(bd,V((0,(h[0].y-0.003)-mx[1],0)))
rigid_weight(bd,'J_Bip_C_UpperChest'); finish(bd,arm,[],smooth_shade=False)

# --- ARM PATCH (right arm, top surface in T-pose)
pt=A['A_Parche']; pt.name='INV_Parche_Brazo_R'; pt.data.name='INV_Parche_Brazo_R'
mn,mx=bbox(pt); c=V(((mn+mx)/2).tolist())
pt.data.transform(mathutils.Matrix.Translation(-c))
pt.data.transform(mathutils.Matrix.Rotation(math.radians(90),4,'Y'))
h=jb.ray_cast(V((-0.27,0.03,1.8)),V((0,0,-1)))
mn,mx=bbox(pt)
move(pt,V((-0.27,h[0].y if False else 0.03,h[0].z-mn[2]-0.001)))
rigid_weight(pt,'J_Bip_R_UpperArm'); finish(pt,arm,[],smooth_shade=False)

# --- GLASSES (rectangular, fitted to VRoid head)
gl=A['A_Gafas']; gl.name='INV_Gafas'; gl.data.name='INV_Gafas'
fm=face.data
iris=[i for i,m in enumerate(fm.materials) if 'EyeIris' in m.name][0]
ivs=set(v for p in fm.polygons if p.material_index==iris for v in p.vertices)
ic=np.array([fm.vertices[v].co[:] for v in ivs])
eyez=float(ic[:,2].mean()); eyey=float(ic[:,1].min())
print('eyes', ic.min(0), ic.max(0))
mn,mx=bbox(gl)
s=0.094/0.097
gl.data.transform(mathutils.Matrix.Diagonal((s,s,s,1)))
mn,mx=bbox(gl)
lens=[v.co for v in gl.data.vertices if v.co.y<mn[1]+0.012]
lz=(min(c.z for c in lens)+max(c.z for c in lens))/2
move(gl,V((0,(eyey-0.024)-mn[1],eyez-lz)))
rigid_weight(gl,'J_Bip_C_Head'); finish(gl,arm,[],smooth_shade=False)

# --- TABLET prop (separate, not skinned)
tb=A['A_Tableta']; tb.name='INV_Tableta_Prop'; tb.data.name='INV_Tableta_Prop'
props=bpy.data.collections.new('Investigador_Props'); bpy.context.scene.collection.children.link(props)
col.objects.unlink(tb); props.objects.link(tb)
mn,mx=bbox(tb); c=V(((mn+mx)/2).tolist())
tb.data.transform(mathutils.Matrix.Translation(-c))
tb.location=(0.9,0,1.0)

bpy.ops.wm.save_as_mainfile(filepath=W+'/stage4.blend')
