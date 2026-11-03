import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy, bmesh, math, numpy as np, sys
from herramientas import *
enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage4.blend')
body=bpy.data.objects['Body']; arm=bpy.data.objects['Armature']; face=bpy.data.objects['Face']; hair=bpy.data.objects['Hair']
# ---- recolor textures
def imgs_of(mat):
    e=mat.vrm_addon_extension.mtoon1; out=[]
    for tex in (e.pbr_metallic_roughness.base_color_texture, e.extensions.vrmc_materials_mtoon.shade_multiply_texture):
        s=tex.index.source
        if s: out.append(s)
    return out
done=set()
def recolor(img, fn):
    if img.name in done: return
    done.add(img.name)
    w,h=img.size; px=np.array(img.pixels[:],dtype=np.float32).reshape(h,w,4)
    px[...,:3]=fn(px[...,:3]); img.pixels.foreach_set(px.ravel()); img.update(); img.pack()
    print('recolored',img.name,w,h)
def to_blueblack(rgb):
    lum=0.2126*rgb[...,0]+0.7152*rgb[...,1]+0.0722*rgb[...,2]
    l=np.clip(lum/ max(1e-3,np.percentile(lum,90)),0,1.2)
    tint=np.array([0.055,0.068,0.10]); hi=np.array([0.20,0.24,0.33])
    return tint+ (hi-tint)*l[...,None]**1.6
def to_greyblue(rgb):
    lum=0.2126*rgb[...,0]+0.7152*rgb[...,1]+0.0722*rgb[...,2]
    sat=rgb.max(-1)-rgb.min(-1)
    tgt=lum[...,None]*np.array([0.80,0.95,1.18])
    m=np.clip(sat*4,0,1)[...,None]   # only colored (iris) pixels
    return rgb*(1-m)+tgt*m
hair_mats=[m for m in hair.data.materials]+[m for m in body.data.materials if 'HairBack' in m.name]
for m in hair_mats:
    for i in imgs_of(m): recolor(i,to_blueblack)
for m in face.data.materials:
    if 'FaceBrow' in m.name:
        for i in imgs_of(m): recolor(i,to_blueblack)
    if 'EyeIris' in m.name:
        for i in imgs_of(m): recolor(i,to_greyblue)

# ---- body cleanup: remove VRoid clothes/glasses faces + skin hidden under garments
S=SkinInfo(body); co=S.co
dom=np.array([S.gnames[i] for i in S.W.argmax(axis=1)])
bm=bmesh.new(); bm.from_mesh(body.data); bm.faces.ensure_lookup_table()
def covered(f):
    c=f.calc_center_median(); x,y,z=c; a=abs(x)
    ds=[dom[v.index] for v in f.verts]
    if all(d=='J_Bip_C_Head' for d in ds): return False
    if z<0.30: return True                         # boots
    if 0.24<z<1.02 and a<0.3: return True          # pants
    if 0.98<z<1.535 and a<0.70: return True        # shirt / gloves (hand up to 0.70)
    return False
dele=[f for f in bm.faces if f.material_index in (1,2,3,5,6,7,8) or (f.material_index==0 and covered(f))]
# keep fingers beyond glove (x>0.70 handled), erode: keep faces bordering kept skin
keep_border=set()
skin_del=set(f for f in dele if f.material_index==0)
for f in skin_del:
    for e in f.edges:
        for lf in e.link_faces:
            if lf.material_index==0 and lf not in skin_del: keep_border.add(f)
dele=[f for f in dele if f not in keep_border]
bmesh.ops.delete(bm,geom=dele,context='FACES')
bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')
bm.to_mesh(body.data); bm.free()
# drop unused material slots + their outline modifiers
used=set(p.material_index for p in body.data.polygons)
names=[m.name for m in body.data.materials]
for md in list(body.modifiers):
    if md.type=='NODES' and any(n in md.name for i,n in enumerate(names) if i not in used):
        body.modifiers.remove(md)
for i in sorted([i for i in range(len(names)) if i not in used], reverse=True):
    body.data.materials.pop(index=i)
print('body verts',len(body.data.vertices),[m.name for m in body.data.materials],[m.name for m in body.modifiers])
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage5.blend')
