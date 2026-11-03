import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh, math, numpy as np, sys
from herramientas import *
enable()
bpy.ops.wm.open_mainfile(filepath=W+'/base.blend')
body=bpy.data.objects['Body']; arm=bpy.data.objects['Armature']
S=SkinInfo(body); co=S.co; me=body.data
col=bpy.data.collections.new('Investigador_Ropa'); bpy.context.scene.collection.children.link(col)
bvh0=body_bvh(body,0)
dom=np.array([S.gnames[i] for i in S.W.argmax(axis=1)])
F=[(p.index,co[list(p.vertices)].mean(axis=0),[dom[v] for v in p.vertices]) for p in me.polygons if p.material_index==0]
M={}
M['shirt']=mtoon('INV_Camisa_Negra', srgb('#25272c'), srgb('#15161a'))
M['pants']=mtoon('INV_Pantalon_Azul', srgb('#3a4558'), srgb('#262e3c'))
M['jk']=mtoon('INV_Chaqueta_Gris', srgb('#cfcfca'), srgb('#9fa1a2'))
M['jk_dark']=mtoon('INV_Chaqueta_Grafito', srgb('#3c3f46'), srgb('#2a2c31'))
M['yellow']=mtoon('INV_Acento_Amarillo', srgb('#e2ad35'), srgb('#b7832a'))
M['lining']=mtoon('INV_Forro_Oscuro', srgb('#2e3035'), srgb('#1e2024'))

def mesh_bvh(ob):
    dg=bpy.context.evaluated_depsgraph_get()
    bm=bmesh.new(); bm.from_object(ob,dg); r=BVHTree.FromBMesh(bm); bm.free(); return r

# ---------- SHIRT
sel=[i for i,c,d in F if 0.94<c[2]<1.58 and abs(c[0])<0.63 and not all(x=='J_Bip_C_Head' for x in d)]
shirt=extract(body, sel, 'INV_Camisa', col)
bm=bmesh.new(); bm.from_mesh(shirt.data); bm.normal_update()
for v in bm.verts:
    neck = 1.0 if v.co.z>1.47 and abs(v.co.x)<0.09 else 0.0
    v.co += v.normal*(0.004 + 0.006*neck)
smooth(bm,2,0.4,keep_boundary=False)
push_out(bm,bvh0,lambda p:0.0035)
cut(bm,V((0,0,1.553)),V((0,0,1)))
cut(bm,V((0,0,0.965)),V((0,0,-1)))
cut(bm,V((0.605,0,0)),V((1,0,0)))
cut(bm,V((-0.605,0,0)),V((-1,0,0)))
bm.to_mesh(shirt.data); bm.free()
finish(shirt, arm, [M['shirt']]); add_solidify(shirt, 0.002, offset=-1)

# ---------- PANTS
sel=[i for i,c,d in F if 0.2<c[2]<1.06 and abs(c[0])<0.3]
pants=extract(body, sel, 'INV_Pantalon', col)
bm=bmesh.new(); bm.from_mesh(pants.data); bm.normal_update()
for v in bm.verts:
    x,y,z=v.co; n=v.normal
    inward = max(0.0, -math.copysign(1,x)*n.x) if z<0.93 else 0
    base = 0.028 if z<0.9 else 0.028+(0.010-0.028)*min(1,(z-0.9)/0.13)
    v.co += n*base*(1-0.8*inward)
smooth(bm,10,0.5,keep_boundary=False)
push_out(bm,bvh0,lambda p:0.011 if p.z<0.95 else 0.008)
cut(bm,V((0,0,1.035)),V((0,0,1)))
cut(bm,V((0,0,0.22)),V((0,0,-1)))
bm.to_mesh(pants.data); bm.free()
finish(pants, arm, [M['pants']]); add_solidify(pants, 0.003, offset=-1)

# ---------- JACKET
sel=[]
for i,c,d in F:
    x,y,z=c
    if all(n in('J_Bip_C_Head','J_Bip_C_Neck') for n in d) and z>1.5: continue
    if abs(x)<0.075 and z>1.5: continue
    if z<0.88 or z>1.56 or abs(x)>0.56: continue
    if abs(x)>0.2 and z<1.30: continue
    sel.append(i)
jk=extract(body, sel, 'INV_Chaqueta', col)
bm=bmesh.new(); bm.from_mesh(jk.data); bm.normal_update()
for v in bm.verts:
    v.co += v.normal*(0.018 if abs(v.co.x)<0.2 else 0.022)
smooth(bm,6,0.5,keep_boundary=False)
torso=[v for v in bm.verts if abs(v.co.x)<0.22]
ring=[v for v in torso if 1.22<v.co.z<1.30]
cy=float(np.mean([v.co.y for v in ring]))
def ang(v): return math.atan2(v.co.y-cy, v.co.x)
bins=48; rmax=np.zeros(bins)
for v in ring:
    b=int((ang(v)+math.pi)/(2*math.pi)*bins)%bins
    rmax[b]=max(rmax[b], math.hypot(v.co.x, v.co.y-cy))
for k in range(bins):
    if rmax[k]==0: rmax[k]=max(rmax[(k-1)%bins],rmax[(k+1)%bins])
for v in torso:
    z=v.co.z
    if z>=1.27: continue
    s=min(1.0,(1.27-z)/0.24)*0.92
    b=int((ang(v)+math.pi)/(2*math.pi)*bins)%bins
    r=math.hypot(v.co.x, v.co.y-cy); rt=rmax[b]*0.99
    if rt>r:
        k=(r+(rt-r)*s)/r; v.co.x*=k; v.co.y=cy+(v.co.y-cy)*k
smooth(bm,4,0.4,mask=lambda v:v.co.z<1.32 and abs(v.co.x)<0.22,keep_boundary=False)
sbvh=mesh_bvh(shirt); pbvh=mesh_bvh(pants)
for it in range(3):
    push_out(bm,bvh0,lambda p:0.016)
    push_out(bm,sbvh,lambda p:0.009)
    smooth(bm,3,0.45,mask=lambda v:v.co.z<1.12,keep_boundary=False)
push_out(bm,bvh0,lambda p:0.016); push_out(bm,sbvh,lambda p:0.009)
def radial_clear(bm, bvhs, zmax, gap):
    for v in bm.verts:
        if v.co.z>zmax or abs(v.co.x)>0.26: continue
        d=V((v.co.x, v.co.y-cy, 0)); r=d.length
        if r<1e-6: continue
        d/=r; o=V((0,cy,v.co.z))
        R=0
        for bv,g in bvhs:
            h=bv.ray_cast(o+d*0.5,-d)
            if h[0] is not None: R=max(R,(V((h[0].x,h[0].y-cy,0))).length+g)
        if r<R:
            v.co.x=d.x*R; v.co.y=cy+d.y*R
for it in range(3):
    radial_clear(bm,[(pbvh,0.013),(bvh0,0.02),(sbvh,0.012)],1.08,0)
    smooth(bm,2,0.35,mask=lambda v:v.co.z<1.08 and abs(v.co.x)<0.26,keep_boundary=False)
radial_clear(bm,[(pbvh,0.013),(bvh0,0.02),(sbvh,0.012)],1.08,0)
# clean cuts
cut(bm,V((0,0,0.925)),V((0,0,-1)))
cut(bm,V((0.50,0,0)),V((1,0,0)),filt=lambda c:c.x>0.3)
cut(bm,V((-0.50,0,0)),V((-1,0,0)),filt=lambda c:c.x<-0.3)
# open front: plane through (w1,1.47) and (w0,0.925)
w1,w0,z1,z0=0.032,0.092,1.50,0.925
d=V((w0-w1,0,z0-z1)).normalized(); nR=V((-d.z,0,d.x)); 
if nR.x>0: nR=-nR
cut(bm,V((w1,0,z1)),nR,filt=lambda c:c.y<-0.03 and c.x>-0.004)
nL=V((-nR.x,0,nR.z))
cut(bm,V((-w1,0,z1)),nL,filt=lambda c:c.y<-0.03 and c.x<0.004)
bm.normal_update()
# material zones with straight split lines
for sx in (1,-1):
    split(bm,V((0,0,1.452)),V((0,0,1)),filt=lambda c:abs(c.x)>0.07)
    split(bm,V((sx*0.37,0,0)),V((1,0,0)))
    split(bm,V((sx*0.40,0,0)),V((1,0,0)))
    split(bm,V((sx*0.085,0,0)),V((1,0,0)),filt=lambda c:c.z>1.40)
# trim band parallel to opening (2 cm)
off=0.02
split(bm,V((w1,0,z1))-nR*off,nR,filt=lambda c:c.y<-0.03 and c.x>0)
split(bm,V((-w1,0,z1))-nL*off,nL,filt=lambda c:c.y<-0.03 and c.x<0)
bmesh.ops.dissolve_degenerate(bm,edges=bm.edges,dist=0.0015)
bm.normal_update()
for f in bm.faces:
    c=f.calc_center_median(); a=abs(c.x)
    f.material_index=0
    if 0.085<a<0.37 and c.z>1.452: f.material_index=1
    if 0.37<a<0.40: f.material_index=1
    if c.y<-0.03 and c.z>0.925:
        if c.x>0 and (c-V((w1,0,z1))).dot(nR)>-off: f.material_index=2
        if c.x<0 and (c-V((-w1,0,z1))).dot(nL)>-off: f.material_index=2
bm.to_mesh(jk.data); bm.free()
finish(jk, arm, [M['jk'],M['jk_dark'],M['yellow'],M['lining'],M['lining'],M['yellow']])
add_solidify(jk, 0.004, offset=-1, mat_off=3, rim=2)


# ---------- COLLAR (clean parametric)
jbvh=mesh_bvh(jk)
cyN=0.025; n_u=40; n_v=6
gap0,gap1=math.radians(26),math.radians(40)
verts=[];faces=[]
for j in range(n_v+1):
    t=j/n_v
    gap=gap0+(gap1-gap0)*t
    a0=-math.pi/2+gap; a1=-math.pi/2+2*math.pi-gap
    for i in range(n_u+1):
        a=a0+(a1-a0)*i/n_u
        d=V((math.cos(a),math.sin(a),0))
        # base: project onto jacket from outside
        src=V((0,cyN,1.475))+d*0.25+V((0,0,0.05))
        hit=jbvh.ray_cast(src,(V((0,cyN,1.475))-src).normalized())
        base=(hit[0] if hit[0] is not None else V((0,cyN,1.475))+V((d.x*0.085,d.y*0.08,0)))
        base=base+d*0.004
        front=max(0.0,-d.y)  # 1 at front
        h=0.085-0.015*front
        p=base+V((0,0,h*t))+d*(0.016*t*t+0.004*t)
        verts.append(p)
for j in range(n_v):
    for i in range(n_u):
        a=j*(n_u+1)+i; faces.append((a,a+1,a+n_u+2,a+n_u+1))
cm=bpy.data.meshes.new('INV_Chaqueta_Cuello'); cm.from_pydata([tuple(v) for v in verts],[],faces)
cl=bpy.data.objects.new('INV_Chaqueta_Cuello',cm); col.objects.link(cl)
# outer light, front edge strips yellow
for p in cm.polygons:
    i=p.index%n_u
    p.material_index = 2 if (i<2 or i>=n_u-2) else 0
cm.flip_normals() if False else None
cm.update()
# ensure normals outward
bmc=bmesh.new(); bmc.from_mesh(cm); bmc.normal_update(); bmc.faces.ensure_lookup_table()
f0=bmc.faces[len(bmc.faces)//2]; c0=f0.calc_center_median()
if f0.normal.dot(V((c0.x,c0.y-cyN,0)))<0: bmesh.ops.reverse_faces(bmc,faces=bmc.faces)
bmc.to_mesh(cm); bmc.free()
transfer_weights(cl, body)
finish(cl, arm, [M['jk'],M['yellow'],M['yellow'],M['yellow'],M['yellow'],M['yellow']])
add_solidify(cl, 0.005, offset=-1, mat_off=1, rim=1)

# ---------- ROLLED CUFFS
def cuff(side):
    bmj=bmesh.new(); bmj.from_mesh(jk.data)
    pts=[v.co.copy() for v in bmj.verts if v.is_boundary and side*v.co.x>0.48]
    bmj.free()
    cyz=V((0,sum(p.y for p in pts)/len(pts),sum(p.z for p in pts)/len(pts)))
    rad=sum(math.hypot(p.y-cyz.y,p.z-cyz.z) for p in pts)/len(pts)
    # profile (dx along arm toward shoulder, dr radial) closed loop, material per segment
    prof=[(0.0,-0.002,2),(0.0,0.008,2),(0.006,0.013,0),(0.022,0.014,0),(0.040,0.012,0),(0.046,0.006,0),(0.046,-0.001,0),(0.03,-0.002,0)]
    n=40; vs=[]; fs=[]; mats=[]
    for k in range(n):
        a=2*math.pi*k/n
        for (dx,dr,_) in prof:
            r=rad+dr
            vs.append((side*(0.505-dx), cyz.y+r*math.cos(a), cyz.z+r*math.sin(a)))
    P=len(prof)
    for k in range(n):
        for j in range(P):
            a=k*P+j; b=k*P+(j+1)%P; c=((k+1)%n)*P+(j+1)%P; d=((k+1)%n)*P+j
            fs.append((a,b,c,d) if side>0 else (a,d,c,b)); mats.append(prof[j][2])
    m=bpy.data.meshes.new('cuff'); m.from_pydata(vs,[],fs)
    for p,mi in zip(m.polygons,mats): p.material_index=mi
    nm='INV_Chaqueta_Puno_'+('L' if side>0 else 'R')
    m.name=nm; o=bpy.data.objects.new(nm,m); col.objects.link(o)
    bmc=bmesh.new(); bmc.from_mesh(m); bmc.normal_update()
    out=sum((f.normal.dot((f.calc_center_median()-V((f.calc_center_median().x,cyz.y,cyz.z))).normalized()) for f in bmc.faces))
    if out<0: bmesh.ops.reverse_faces(bmc,faces=bmc.faces)
    bmc.to_mesh(m); bmc.free()
    transfer_weights(o, body)
    finish(o, arm, [M['jk'],M['jk_dark'],M['yellow']])
cuff(1); cuff(-1)
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage2.blend')
