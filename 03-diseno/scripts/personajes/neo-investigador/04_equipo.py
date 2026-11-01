import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh, math, numpy as np, sys
from herramientas import *
enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage2.blend')
body=bpy.data.objects['Body']; arm=bpy.data.objects['Armature']
col=bpy.data.collections['Investigador_Ropa']
S=SkinInfo(body); co=S.co; me=body.data
bvh0=body_bvh(body,0)
dom=np.array([S.gnames[i] for i in S.W.argmax(axis=1)])
F=[(p.index,co[list(p.vertices)].mean(axis=0),[dom[v] for v in p.vertices]) for p in me.polygons if p.material_index==0]
def mesh_bvh(ob):
    dg=bpy.context.evaluated_depsgraph_get()
    bm=bmesh.new(); bm.from_object(ob,dg); r=BVHTree.FromBMesh(bm); bm.free(); return r
M={m.name:m for m in bpy.data.materials}
Y=M['INV_Acento_Amarillo']; G=M['INV_Chaqueta_Grafito']
M['glove']=mtoon('INV_Guante_Negro', srgb('#1f2024'), srgb('#121215'))
M['glove_palm']=mtoon('INV_Guante_Palma', srgb('#34363b'), srgb('#222327'))
M['boot']=mtoon('INV_Bota_Clara', srgb('#d9d6ce'), srgb('#a9a69f'))
M['boot_dark']=mtoon('INV_Bota_Grafito', srgb('#3a3c41'), srgb('#25262a'))
M['belt']=mtoon('INV_Cinturon', srgb('#202125'), srgb('#131417'))
M['metal']=mtoon('INV_Metal', srgb('#a9adb3'), srgb('#6e7278'), toony=0.6)
M['pants_dark']=mtoon('INV_Pantalon_Sombra', srgb('#323c4d'), srgb('#212835'))
pants=bpy.data.objects['INV_Pantalon']; pbvh=mesh_bvh(pants)
jk=bpy.data.objects['INV_Chaqueta']

# ---------- GLOVES
for side,lab in ((1,'L'),(-1,'R')):
    P='J_Bip_%s_'%lab
    keep=[P+'Hand']+[P+f+'1' for f in ('Index','Middle','Ring','Little')]+[P+'Thumb1',P+'Thumb2']
    sel=[i for i,c,d in F if side*c[0]>0.57 and any(x in keep for x in d)]
    g=extract(body, sel, 'INV_Guante_'+lab, col)
    bm=bmesh.new(); bm.from_mesh(g.data); bm.normal_update()
    for v in bm.verts: v.co+=v.normal*0.0028
    smooth(bm,1,0.3,keep_boundary=False)
    push_out(bm,bvh0,lambda p:0.0025)
    cut(bm,V((side*0.585,0,0)),V((-side,0,0)))
    cut(bm,V((side*0.722,0,0)),V((side,0,0)),filt=lambda c:c.y>-0.012)   # fingers (thumb is in front)
    cut(bm,V((side*0.708,-0.035,1.43)),V((side*0.5,-0.85,0)).normalized(),filt=lambda c:c.y<-0.012)
    bmesh.ops.dissolve_degenerate(bm,edges=bm.edges,dist=0.0008)
    split(bm,V((side*0.605,0,0)),V((1,0,0)))
    bm.normal_update()
    for f in bm.faces:
        c=f.calc_center_median()
        f.material_index=0
        if f.normal.z<-0.55 and side*c.x>0.625: f.material_index=1   # palm (T-pose palm faces down)
        if side*c.x<0.605: f.material_index=2                         # wrist strap
    bm.to_mesh(g.data); bm.free()
    finish(g, arm, [M['glove'],M['glove_palm'],G])
    add_solidify(g, 0.0015, offset=-1)

# ---------- BOOTS
for side,lab in ((1,'L'),(-1,'R')):
    sel=[i for i,c,d in F if c[2]<0.37 and side*c[0]>0]
    b=extract(body, sel, 'INV_Bota_'+lab, col)
    bm=bmesh.new(); bm.from_mesh(b.data); bm.normal_update()
    lx=side*0.086
    for v in bm.verts:
        x,y,z=v.co
        o=0.019 + (0.010*(0.08-z)/0.08 if z<0.08 else 0) + (0.006 if y<-0.1 else 0)
        v.co+=v.normal*o
    smooth(bm,6,0.5,keep_boundary=False)
    push_out(bm,bvh0,lambda p:0.015)
    push_out(bm,pbvh,lambda p:0.006 if p.z>0.2 else None)
    for v in bm.verts:
        if v.co.z<0.0: v.co.z=0.0
    cut(bm,V((0,0,0.32)),V((0,0,1)))
    # fill the sole opening if any (bottom), then define zones
    bmesh.ops.dissolve_degenerate(bm,edges=bm.edges,dist=0.0008)
    split(bm,V((0,0,0.035)),V((0,0,1)))
    split(bm,V((0,-0.105,0)),V((0,1,0)),filt=lambda c:c.z<0.12)
    split(bm,V((0,0.065,0)),V((0,1,0)),filt=lambda c:c.z<0.16)
    split(bm,V((lx+0.022,0,0)),V((1,0,0)),filt=lambda c:c.y<-0.02 and c.z>0.1)
    split(bm,V((lx-0.022,0,0)),V((1,0,0)),filt=lambda c:c.y<-0.02 and c.z>0.1)
    split(bm,V((0,0,0.302)),V((0,0,1)))
    split(bm,V((0,0,0.12)),V((0,0,1)),filt=lambda c:c.y<-0.02)
    bm.normal_update()
    for f in bm.faces:
        c=f.calc_center_median()
        m=0
        if c.z<0.035: m=1
        elif c.y<-0.105 and c.z<0.12: m=1
        elif c.y>0.065 and c.z<0.16: m=1
        elif c.z>0.302: m=2
        elif c.y<-0.02 and abs(c.x-lx)<0.022 and c.z>0.12: m=2
        f.material_index=m
    bm.to_mesh(b.data); bm.free()
    finish(b, arm, [M['boot'],M['boot_dark'],Y])
    add_solidify(b, 0.003, offset=-1)

# ---------- helper: ring around an axis projected on a surface
def ring_on(bvh, center, z, n=64, off=0.004, inside=False):
    pts=[]
    for k in range(n):
        a=2*math.pi*k/n; d=V((math.cos(a),math.sin(a),0))
        o=V((center[0],center[1],z))
        if inside:
            h=bvh.ray_cast(o,d,0.3); hp=h[0]; extra=0.0035
        else:
            h=bvh.ray_cast(o+d*0.4,-d); hp=h[0]; extra=0
        pts.append((hp if hp else o+d*0.12)+d*(off+extra))
    return pts
def band(name, bvh, center, z0, z1, n=64, off=0.004, mats=None, inside=False):
    r0=ring_on(bvh,center,z0,n,off,inside); r1=ring_on(bvh,center,z1,n,off,inside)
    vs=[tuple(p) for p in r0+r1]; fs=[(k,(k+1)%n,n+(k+1)%n,n+k) for k in range(n)]
    m=bpy.data.meshes.new(name); m.from_pydata(vs,[],fs)
    o=bpy.data.objects.new(name,m); col.objects.link(o)
    bmb=bmesh.new(); bmb.from_mesh(m); bmb.normal_update()
    bmb.faces.ensure_lookup_table()
    f=bmb.faces[0]; c=f.calc_center_median()
    if f.normal.dot(V((c.x-center[0],c.y-center[1],0)))<0: bmesh.ops.reverse_faces(bmb,faces=bmb.faces)
    bmb.to_mesh(m); bmb.free()
    return o

# ---------- BELT
cyb=float(np.mean([v.co.y for v in pants.data.vertices if v.co.z>1.0]))
belt=band('INV_Cinturon', pbvh, (0,cyb), 0.988, 1.026, n=72, off=0.003)
transfer_weights(belt, body); finish(belt, arm, [M['belt']]); add_solidify(belt,0.004,offset=1)
# buckle (frame) at front
hit=pbvh.ray_cast(V((0,-0.5,1.007)),V((0,1,0)))
fy=hit[0].y-0.009
bk=bpy.data.meshes.new('INV_Hebilla'); bmk=bmesh.new()
def box(bmx, c, s):
    r=bmesh.ops.create_cube(bmx,size=1.0); 
    for v in r['verts']: v.co=V((c[0]+v.co.x*s[0],c[1]+v.co.y*s[1],c[2]+v.co.z*s[2]))
    return r
box(bmk,(0,fy,1.025),(0.052,0.008,0.007)); box(bmk,(0,fy,0.989),(0.052,0.008,0.007))
box(bmk,(-0.023,fy,1.007),(0.007,0.008,0.043)); box(bmk,(0.023,fy,1.007),(0.007,0.008,0.043))
box(bmk,(0.0,fy+0.002,1.007),(0.004,0.006,0.03))
bmesh.ops.bevel(bmk,geom=bmk.edges[:],offset=0.0012,segments=2,affect='EDGES')
bmk.to_mesh(bk); bmk.free()
buckle=bpy.data.objects.new('INV_Hebilla',bk); col.objects.link(buckle)
rigid_weight(buckle,'J_Bip_C_Hips'); finish(buckle, arm, [M['metal']])

# ---------- LEFT CARGO POCKET (+X outer thigh)
def surface_patch(name, bvh, axis_xy, a0, a1, z0, z1, off, nu=10, nv=10):
    vs=[];fs=[]
    for j in range(nv+1):
        z=z0+(z1-z0)*j/nv
        for i in range(nu+1):
            a=a0+(a1-a0)*i/nu; d=V((math.cos(a),math.sin(a),0))
            o=V((axis_xy[0],axis_xy[1],z)); h=bvh.ray_cast(o+d*0.4,-d)
            vs.append(tuple((h[0] if h[0] else o+d*0.1)+d*off))
    for j in range(nv):
        for i in range(nu):
            a=j*(nu+1)+i; fs.append((a,a+1,a+nu+2,a+nu+1))
    m=bpy.data.meshes.new(name); m.from_pydata(vs,[],fs); o=bpy.data.objects.new(name,m); col.objects.link(o)
    bmb=bmesh.new(); bmb.from_mesh(m); bmb.normal_update(); bmb.faces.ensure_lookup_table()
    f=bmb.faces[len(bmb.faces)//2]; c=f.calc_center_median()
    if f.normal.dot(V((c.x-axis_xy[0],c.y-axis_xy[1],0)))<0: bmesh.ops.reverse_faces(bmb,faces=bmb.faces)
    bmb.to_mesh(m); bmb.free(); return o
pk=surface_patch('INV_Bolsillo_Cargo_L', pbvh, (0.086,0.005), math.radians(-38), math.radians(34), 0.56, 0.74, 0.004)
transfer_weights(pk,body); finish(pk,arm,[M['INV_Pantalon_Azul']]); add_solidify(pk,0.012,offset=-1)
fl=surface_patch('INV_Bolsillo_Cargo_L_Tapa', pbvh, (0.086,0.005), math.radians(-41), math.radians(37), 0.715, 0.765, 0.0175, nv=3)
transfer_weights(fl,body); finish(fl,arm,[M['pants_dark']]); add_solidify(fl,0.004,offset=-1)
# second smaller pocket right calf? keep: knee pleat strips both legs (subtle)

# ---------- RIGHT THIGH POUCH (-X)
strap=band('INV_Funda_Correa_R', pbvh, (-0.086,0.005), 0.705, 0.73, n=48, off=0.003, inside=True)
transfer_weights(strap,body); finish(strap,arm,[M['belt']]); add_solidify(strap,0.003,offset=1)
strap2=band('INV_Funda_Correa_R2', pbvh, (-0.086,0.005), 0.835, 0.86, n=48, off=0.003, inside=True)
transfer_weights(strap2,body); finish(strap2,arm,[M['belt']]); add_solidify(strap2,0.003,offset=1)
hit=pbvh.ray_cast(V((-0.6,0.005,0.79)),V((1,0,0)))
px=hit[0].x-0.004
pm=bpy.data.meshes.new('INV_Funda_R'); bmp=bmesh.new()
box(bmp,(px-0.024,0.005,0.785),(0.044,0.10,0.15))
bmesh.ops.bevel(bmp,geom=bmp.edges[:],offset=0.008,segments=3,affect='EDGES')
r=box(bmp,(px-0.047,0.005,0.84),(0.006,0.104,0.045))   # flap
bmesh.ops.bevel(bmp,geom=list(set(e for v in r['verts'] for e in v.link_edges)),offset=0.002,segments=2,affect='EDGES')
r2=box(bmp,(px-0.051,0.005,0.83),(0.004,0.022,0.03))     # yellow tab
bmp.to_mesh(pm)
for p in pm.polygons: p.material_index=0
bmp.free()
pouch=bpy.data.objects.new('INV_Funda_R',pm); col.objects.link(pouch)
# tab material by position
for p in pm.polygons:
    c=p.center
    if c.x<px-0.0485 and abs(c.y-0.005)<0.012 and 0.814<c.z<0.846: p.material_index=1
transfer_weights(pouch,body)
finish(pouch,arm,[M['belt'],Y])
for p in pm.polygons: p.use_smooth=False
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage3.blend')
