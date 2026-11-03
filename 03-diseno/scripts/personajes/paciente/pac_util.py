"""Utilidades del Paciente (sin VRM Add-on: los materiales MToon se recolorean en sus nodos).

Variables de entorno:
  PAC_VROID_BLEND  investigador-vroid.blend (entrega del 29 de octubre, copia local de Chris). Obligatoria para 01 y 02:
                   de su camisa sin recortar se reconstruye la piel de antebrazos y cuello, y la camiseta.
  PAC_NEO          neo-investigador.blend (por omisión, fuentes/neo-investigador.blend).
  PAC_TRABAJO      carpeta de archivos intermedios (por omisión, una carpeta temporal).
"""
import os, sys
AQUI = os.path.dirname(os.path.abspath(__file__))
DISENO = os.path.abspath(os.path.join(AQUI, '..', '..', '..'))          # 03-diseno/
REPO_SCRIPTS = os.path.join(AQUI, '..', 'neo-investigador')            # herramientas.py y render_vistas.py
import tempfile
os.environ.setdefault('PAC_NEO', os.path.join(DISENO, 'fuentes', 'neo-investigador.blend'))
os.environ.setdefault('PAC_TRABAJO', os.path.join(tempfile.gettempdir(), 'paciente'))
os.makedirs(os.environ['PAC_TRABAJO'], exist_ok=True)
sys.path.insert(0, REPO_SCRIPTS)
import bpy, bmesh, math, mathutils
from mathutils import Vector as V
from herramientas import srgb, cut, smooth, push_out, transfer_weights, rigid_weight, add_solidify, boundary_loops, split, finish

def toon(name, hexcol, shade=None, template='INV_Chaqueta_Gris', emissive=None, strength=1.0, toony=None):
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials[template].copy(); m.name = name
    lit = srgb(hexcol); sh = srgb(shade) if shade else tuple(c*0.62 for c in lit)
    nt = m.node_tree
    out = nt.nodes['Mtoon1Material.Mtoon1Output']
    out.inputs['Lit Color'].default_value = (*lit, 1)
    out.inputs['Shade Color'].default_value = (*sh, 1)
    if toony is not None: out.inputs['Shading Toony'].default_value = toony
    if emissive:
        out.inputs['Emissive Factor'].default_value = (*srgb(emissive), 1)
        out.inputs['Emissive Strength'].default_value = strength
        nt.nodes['Mtoon1Material.GltfEmissive'].inputs['Color'].default_value = (*srgb(emissive), 1)
        nt.nodes['Mtoon1Material.GltfEmissive'].inputs['Strength'].default_value = strength
    nt.nodes['Mtoon1Material.CompatibleShader'].inputs['Base Color'].default_value = (*lit, 1)
    m.diffuse_color = (*lit, 1)
    return m

def opaque_copy(src_name, name):
    """Copia de un material MToon con textura cuya transparencia se ignora."""
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials[src_name].copy(); m.name = name
    nt = m.node_tree
    for l in list(nt.links):
        if l.from_socket.name == 'Alpha' and l.from_node.bl_idname == 'ShaderNodeTexImage':
            nt.links.remove(l)
    for l in list(nt.links):
        if l.to_node.name == 'Mtoon1Material.Mtoon1Output' and l.to_socket.name == 'Lit Color Texture Alpha':
            nt.links.remove(l)
    nt.nodes['Mtoon1Material.Mtoon1Output'].inputs['Lit Color Texture Alpha'].default_value = 1.0
    nt.nodes['Mtoon1Material.AlphaClip.Input'].inputs[0].default_value = 1.0
    # las zonas transparentes de la textura de VRoid tienen color rosado: se rellenan con el tono de piel
    import numpy as np
    for nd in nt.nodes:
        if nd.bl_idname == 'ShaderNodeTexImage' and nd.image and nd.image.name == '_10':
            im = bpy.data.images.get('PAC_Piel_Tex')
            if im is None:
                src = nd.image; w, h = src.size
                px = np.empty(w*h*4, dtype=np.float32); src.pixels.foreach_get(px); px = px.reshape(-1, 4)
                op = px[:, 3] > 0.5
                med = np.median(px[op, :3], axis=0)
                px[~op, :3] = med; px[:, 3] = 1.0
                im = bpy.data.images.new('PAC_Piel_Tex', w, h, alpha=True)
                im.colorspace_settings.name = src.colorspace_settings.name
                im.pixels.foreach_set(px.ravel()); im.pack()
            nd.image = im
        if nd.bl_idname == 'ShaderNodeTexImage' and nd.name == 'Mtoon1NormalTexture.Image' and nd.image:
            nd.image = bpy.data.images['Shader_NoneNormal']   # el mapa de normales de VRoid no cubre estas zonas
    return m

def bm_of(ob):
    bm = bmesh.new(); bm.from_mesh(ob.data); return bm

def bm_back(bm, ob):
    bm.to_mesh(ob.data); bm.free(); ob.data.update()

def keep_faces(ob, pred):
    bm = bm_of(ob)
    bmesh.ops.delete(bm, geom=[f for f in bm.faces if not pred(f.calc_center_median())], context='FACES')
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
    bm_back(bm, ob)

def offset_normals(ob, d, pred=None):
    bm = bm_of(ob)
    bm.normal_update()
    for v in bm.verts:
        if pred is None or pred(v.co):
            dd = d(v.co) if callable(d) else d
            v.co += v.normal * dd
    bm_back(bm, ob)

def set_mats(ob, mats):
    ob.data.materials.clear()
    for m in mats: ob.data.materials.append(m)

def armature_child(ob, rig):
    ob.parent = rig
    for m in [m for m in ob.modifiers if m.type == 'ARMATURE']: ob.modifiers.remove(m)
    md = ob.modifiers.new('Armature', 'ARMATURE'); md.object = rig
    return md

def new_obj(name, coll, me=None):
    me = me or bpy.data.meshes.new(name)
    ob = bpy.data.objects.new(name, me); coll.objects.link(ob); return ob

def collection(name, parent=None):
    c = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    p = parent or bpy.context.scene.collection
    if c.name not in [x.name for x in p.children]: p.children.link(c)
    return c

def copy_weights(ob, src, k=4):
    """Pesos del vértice más cercano de src (promedio de k vecinos, en coordenadas de reposo)."""
    import mathutils.kdtree
    sv = src.data.vertices; kd = mathutils.kdtree.KDTree(len(sv))
    for v in sv: kd.insert(v.co, v.index)
    kd.balance()
    names = [g.name for g in src.vertex_groups]
    ob.vertex_groups.clear()
    acc = {}
    for v in ob.data.vertices:
        tot = {}; wsum = 0
        for co, i, d in kd.find_n(v.co, k):
            w = 1.0 / (d + 1e-4); wsum += w
            for g in sv[i].groups:
                tot[g.group] = tot.get(g.group, 0) + g.weight * w
        acc[v.index] = {gi: x / wsum for gi, x in tot.items() if x / wsum > 0.01}
    for vi, d in acc.items():
        top = sorted(d.items(), key=lambda t: -t[1])[:4]; s_ = sum(x for _, x in top) or 1
        for gi, x in top:
            vg = ob.vertex_groups.get(names[gi]) or ob.vertex_groups.new(name=names[gi])
            vg.add([vi], x / s_, 'REPLACE')
