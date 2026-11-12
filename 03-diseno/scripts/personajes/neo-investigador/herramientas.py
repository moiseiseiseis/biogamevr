import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, math, numpy as np, mathutils, mathutils.kdtree, addon_utils
from mathutils import Vector
from mathutils.bvhtree import BVHTree
V=Vector
def enable():
    """Activa el complemento VRM Add-on for Blender (como add-on o como extensión)."""
    for mod in ('io_scene_vrm', 'bl_ext.blender_org.vrm', 'bl_ext.user_default.vrm'):
        try:
            addon_utils.enable(mod, default_set=True)
            if hasattr(bpy.ops.import_scene, 'vrm'):
                return mod
        except Exception:
            pass
    raise RuntimeError('Instala "VRM Add-on for Blender" antes de ejecutar estos scripts')

def mtoon(name, col, shade=None, toony=0.9, shift=-0.05, outline=0.0008, alpha=None):
    m=bpy.data.materials.get(name)
    if m: return m
    m=bpy.data.materials.new(name)
    e=m.vrm_addon_extension.mtoon1; e.enabled=True
    e.pbr_metallic_roughness.base_color_factor=(*col, alpha if alpha is not None else 1.0)
    if shade is None: shade=tuple(c*0.72 for c in col)
    mt=e.extensions.vrmc_materials_mtoon
    mt.shade_color_factor=shade; mt.shading_toony_factor=toony; mt.shading_shift_factor=shift
    if alpha is not None:
        e.alpha_mode='BLEND'
    return m

def srgb(h):
    h=h.lstrip('#'); c=[int(h[i:i+2],16)/255 for i in (0,2,4)]
    return tuple(((x+0.055)/1.055)**2.4 if x>0.04045 else x/12.92 for x in c)

class SkinInfo:
    def __init__(self, body):
        self.obj=body; me=body.data
        self.me=me
        self.co=np.array([v.co[:] for v in me.vertices])
        self.no=np.array([v.normal[:] for v in me.vertices])
        self.gnames=[g.name for g in body.vertex_groups]
        W=np.zeros((len(me.vertices),len(self.gnames)),dtype=np.float32)
        for v in me.vertices:
            for g in v.groups: W[v.index,g.group]=g.weight
        self.W=W
        self.skin_faces=[p.index for p in me.polygons if p.material_index==0]
    def w(self, names):
        idx=[self.gnames.index(n) for n in names if n in self.gnames]
        return self.W[:,idx].sum(axis=1)

def body_bvh(body, mat_index=0):
    me=body.data
    verts=[v.co.copy() for v in me.vertices]
    polys=[tuple(p.vertices) for p in me.polygons if p.material_index==mat_index]
    return BVHTree.FromPolygons(verts, polys)

def extract(body, face_ids, name, coll=None):
    """Copy Body mesh with only the given faces (keeps vertex groups)."""
    me=body.data.copy(); me.name=name
    ob=bpy.data.objects.new(name, me)
    (coll or bpy.context.scene.collection).objects.link(ob)
    bm=bmesh.new(); bm.from_mesh(me)
    keep=set(face_ids)
    bm.faces.ensure_lookup_table()
    dele=[f for f in bm.faces if f.index not in keep]
    bmesh.ops.delete(bm, geom=dele, context='FACES')
    loose=[v for v in bm.verts if not v.link_faces]
    bmesh.ops.delete(bm, geom=loose, context='VERTS')
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0002)
    fill_small_holes(bm, 0.10)
    bm.to_mesh(me); bm.free()
    me.materials.clear()
    if me.shape_keys: ob.shape_key_clear()
    for md in list(ob.modifiers): ob.modifiers.remove(md)
    ob.parent=body.parent
    return ob

def smooth(bm, iters=3, fac=0.5, mask=None, keep_boundary=True):
    for _ in range(iters):
        new={}
        for v in bm.verts:
            if mask is not None and not mask(v): continue
            if keep_boundary and v.is_boundary: continue
            nb=[e.other_vert(v).co for e in v.link_edges]
            if not nb: continue
            avg=sum(nb,V())/len(nb)
            new[v]=v.co.lerp(avg,fac)
        for v,c in new.items(): v.co=c

def push_out(bm, bvh, dmin_fn, iters=4):
    """Ensure every vertex is at least dmin outside of bvh surface."""
    for _ in range(iters):
        moved=0
        for v in bm.verts:
            d=dmin_fn(v.co)
            if d is None: continue
            loc,nor,idx,dist=bvh.find_nearest(v.co)
            if loc is None: continue
            off=v.co-loc
            sd=off.dot(nor)
            if sd<d:
                v.co=loc+nor*d + (off-nor*sd)*0.0
                moved+=1
        if moved==0: break

def finish(ob, arm, mats, smooth_shade=True):
    me=ob.data
    for m in mats: me.materials.append(m)
    if smooth_shade:
        for p in me.polygons: p.use_smooth=True
    ob.parent=arm
    md=ob.modifiers.new('Armature','ARMATURE'); md.object=arm
    return ob

def add_solidify(ob, t, offset=1.0, mat_off=0, rim=0, use_rim=True):
    s=ob.modifiers.new('Grosor','SOLIDIFY'); s.thickness=t; s.offset=offset
    s.material_offset=mat_off; s.material_offset_rim=rim; s.use_rim=use_rim; s.use_even_offset=False; s.use_quality_normals=True
    # put before armature
    while ob.modifiers.find('Grosor')>0: bpy.context.view_layer.objects.active=ob; bpy.ops.object.modifier_move_up(modifier='Grosor')
    return s

def rigid_weight(ob, bone, w=1.0):
    vg=ob.vertex_groups.get(bone) or ob.vertex_groups.new(name=bone)
    vg.add(list(range(len(ob.data.vertices))), w, 'REPLACE')

def transfer_weights(ob, src):
    """Nearest-face interpolated weights from src (Body)."""
    for vg in list(ob.vertex_groups): ob.vertex_groups.remove(vg)
    for g in src.vertex_groups: ob.vertex_groups.new(name=g.name)
    dt=ob.modifiers.new('DT','DATA_TRANSFER'); dt.object=src
    dt.use_vert_data=True; dt.data_types_verts={'VGROUP_WEIGHTS'}; dt.vert_mapping='POLYINTERP_NEAREST'
    dt.layers_vgroup_select_src='ALL'; dt.layers_vgroup_select_dst='NAME'
    bpy.context.view_layer.objects.active=ob
    with bpy.context.temp_override(object=ob, active_object=ob):
        bpy.ops.object.modifier_move_to_index(modifier='DT', index=0)
        bpy.ops.object.modifier_apply(modifier='DT')
    # drop empty groups
    me=ob.data
    used=set(g.group for v in me.vertices for g in v.groups if g.weight>1e-4)
    for vg in list(ob.vertex_groups):
        if vg.index not in used: ob.vertex_groups.remove(vg)

def cut(bm, co, no, filt=None):
    geom=[f for f in bm.faces if (filt is None or filt(f.calc_center_median()))]
    if not geom: return
    edges=set(e for f in geom for e in f.edges); verts=set(v for f in geom for v in f.verts)
    bmesh.ops.bisect_plane(bm, geom=list(verts)+list(edges)+geom, dist=1e-6, plane_co=co, plane_no=no, clear_outer=True)
    bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')

def boundary_loops(bm):
    be=[e for e in bm.edges if e.is_boundary]
    seen=set(); loops=[]
    for e0 in be:
        if e0 in seen: continue
        loop=[e0]; seen.add(e0)
        # walk
        for start in (e0.verts[1],):
            v=start; prev=e0
            while True:
                nxt=[e for e in v.link_edges if e.is_boundary and e not in seen]
                if not nxt: break
                e=nxt[0]; seen.add(e); loop.append(e); v=e.other_vert(v)
        loops.append(loop)
    return loops

def faces_near_boundary(bm, pred_vert, dist):
    bv=[v.co.copy() for v in bm.verts if v.is_boundary and pred_vert(v)]
    if not bv: return []
    kd=mathutils.kdtree.KDTree(len(bv))
    for i,c in enumerate(bv): kd.insert(c,i)
    kd.balance()
    return [f for f in bm.faces if kd.find(f.calc_center_median())[2]<dist]

def split(bm, co, no, filt=None):
    geom=[f for f in bm.faces if (filt is None or filt(f.calc_center_median()))]
    if not geom: return
    edges=set(e for f in geom for e in f.edges); verts=set(v for f in geom for v in f.verts)
    bmesh.ops.bisect_plane(bm, geom=list(verts)+list(edges)+geom, dist=1e-6, plane_co=co, plane_no=no)

def fill_small_holes(bm, maxsize):
    for loop in boundary_loops(bm):
        vs=set(v for e in loop for v in e.verts)
        cs=[v.co for v in vs]
        size=max(max(c[k] for c in cs)-min(c[k] for c in cs) for k in range(3))
        if size<maxsize and len(loop)>=3:
            r=bmesh.ops.holes_fill(bm, edges=loop, sides=len(loop)+2)
            if r['faces']:
                bmesh.ops.triangulate(bm, faces=r['faces'], quad_method='BEAUTY', ngon_method='BEAUTY')
