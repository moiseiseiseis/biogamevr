"""02b · Cabello rizado: ondula los mechones de VRoid y añade rizos en espiral."""
import os, sys, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
import numpy as np
from mathutils import noise
OUT = os.environ['PAC_TRABAJO']
bpy.ops.wm.open_mainfile(filepath=OUT + '/02_ropa.blend')
O = bpy.data.objects; rig = O['Paciente_Rig']; hair = O['VRoid_Cabello']
HC = V((0, 0.012, 1.66))   # centro aproximado de la cabeza
# 1) ondular los mechones: desplazamiento por ruido a lo largo de la normal, mayor hacia las puntas
bm = bm_of(hair); bm.normal_update()
for v in bm.verts:
    c = v.co; ends = min(1.0, max(0.0, (1.74 - c.z) / 0.16))
    n = noise.noise(c * 38.0) ; m = noise.noise(c * 17.0 + V((3, 1, 2)))
    v.co = c + v.normal * (0.0045 * n * (0.4 + ends)) + (c - HC).normalized() * (0.003 + 0.004 * m) * ends
bm_back(bm, hair)
# 2) color de los rizos: mediana de la textura del cabello
hm = hair.data.materials[0]; img = None
for nd in hm.node_tree.nodes:
    if nd.bl_idname == 'ShaderNodeTexImage' and nd.name.startswith('Mtoon1BaseColorTexture') and nd.image: img = nd.image
px = np.empty(img.size[0] * img.size[1] * 4, dtype=np.float32); img.pixels.foreach_get(px); px = px.reshape(-1, 4)
med = np.percentile(px[px[:, 3] > 0.5, :3], 30, axis=0)
lit_f = hm.node_tree.nodes['Mtoon1Material.Mtoon1Output'].inputs['Lit Color'].default_value
lin = lambda x: ((x + 0.055) / 1.055) ** 2.4 if x > 0.04045 else x / 12.92   # la textura está en sRGB
col = [float(lin(med[i]) * lit_f[i]) for i in range(3)]
mat = toon('PAC_Rizos', '#000000')
ou = mat.node_tree.nodes['Mtoon1Material.Mtoon1Output']
ou.inputs['Lit Color'].default_value = (*col, 1); ou.inputs['Shade Color'].default_value = (*[c * 0.55 for c in col], 1)
mat.node_tree.nodes['Mtoon1Material.CompatibleShader'].inputs['Base Color'].default_value = (*col, 1); mat.diffuse_color = (*col, 1)
# 3) puntos de anclaje sobre la superficie exterior del cabello
rnd = random.Random(7)
cand = []
for v in hair.data.vertices:
    c = v.co; out = (c - HC).normalized()
    if v.normal.dot(out) < 0.35: continue
    face = c.y < -0.035 and c.z < 1.685 and abs(c.x) < 0.075        # no tapar ojos ni cara
    if face or c.z < 1.55: continue
    cand.append((c.copy(), v.normal.copy()))
rnd.shuffle(cand)
pts = []
for c, n in cand:
    if all((c - p).length > 0.023 for p, _ in pts): pts.append((c, n))
    if len(pts) >= 75: break
# 4) rizos en espiral (tubo helicoidal que se afina)
SIDES, STEPS = 7, 14
verts = []; faces = []
for c, n in pts:
    out = (c - HC).normalized()
    down = V((0, 0, -1))
    w = 0.8 if c.z < 1.72 else 0.6            # los de arriba salen hacia fuera, los laterales caen
    axis = (out * (1 - w) + down * w + n * 0.3).normalized()
    t1 = axis.orthogonal().normalized(); t2 = axis.cross(t1)
    R = rnd.uniform(0.008, 0.012); L = rnd.uniform(0.026, 0.042); turns = rnd.uniform(0.9, 1.35); ph = rnd.uniform(0, 6.28)
    base = c - out * 0.007
    ring0 = len(verts)
    for k in range(STEPS + 1):
        t = k / STEPS; a = ph + t * turns * 6.283
        center = base + axis * (L * t) + (t1 * math.cos(a) + t2 * math.sin(a)) * R * min(1, t * 3)
        tang = (axis * L + (t1 * -math.sin(a) + t2 * math.cos(a)) * R * turns * 6.283).normalized()
        b1 = tang.orthogonal().normalized(); b2 = tang.cross(b1)
        rr = 0.0068 * (1 - 0.6 * t ** 1.5)
        for sd in range(SIDES):
            q = sd / SIDES * 6.283
            verts.append(center + (b1 * math.cos(q) + b2 * math.sin(q)) * rr)
    for k in range(STEPS):
        for sd in range(SIDES):
            a_ = ring0 + k * SIDES + sd; b_ = ring0 + k * SIDES + (sd + 1) % SIDES
            faces.append((a_, b_, b_ + SIDES, a_ + SIDES))
    tip = ring0 + STEPS * SIDES
    faces.append(tuple(tip + sd for sd in range(SIDES))[::-1])
me = bpy.data.meshes.new('PAC_Rizos'); me.from_pydata([v[:] for v in verts], [], faces)
rz = new_obj('PAC_Rizos', O['VRoid_Cabello'].users_collection[0], me)
set_mats(rz, [mat])
for p in rz.data.polygons: p.use_smooth = True
rigid_weight(rz, 'J_Bip_C_Head'); armature_child(rz, rig)
me.calc_loop_triangles()
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/02b_pelo.blend')
print('OK02b rizos', len(pts), 'tri', len(me.loop_triangles), 'color', [round(x, 3) for x in col])
