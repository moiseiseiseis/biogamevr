"""02 · Ropa y accesorios del Paciente."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
from mathutils.bvhtree import BVHTree
VROID = os.environ['PAC_VROID_BLEND']; OUT = os.environ['PAC_TRABAJO']
bpy.ops.wm.open_mainfile(filepath=OUT + '/01_base.blend')
rig = bpy.data.objects['Paciente_Rig']
ropa = collection('Paciente_Ropa'); acc = collection('Paciente_Accesorios')
O = bpy.data.objects
M = dict(
 camiseta=toon('PAC_Camiseta', '#dcd8d0', '#b5b0a8'),
 chaqueta=toon('PAC_Chaqueta', '#5e665c', '#434a42'),
 chaq_osc=toon('PAC_Chaqueta_Oscura', '#4b5249', '#363b35'),
 forro=toon('PAC_Forro', '#3b403b', '#2b2f2b'),
 claro=toon('PAC_Gris_Claro', '#c9ccc5', '#a5a8a1'),
 pantalon=toon('PAC_Pantalon', '#cfc9be', '#aaa499'),
 pant_sombra=toon('PAC_Pantalon_Sombra', '#b8b2a7', '#958f85'),
 correa=toon('PAC_Correa', '#2b2d30', '#1b1c1e'),
 metal=toon('PAC_Metal', '#9097a0', '#6a7077'),
 blanco=toon('PAC_Blanco_Medico', '#eef0f0', '#c8ccd0'),
 azul=toon('PAC_Azul_Medico', '#5aa6e0', '#3d7cad', emissive='#3f9be0', strength=0.6),
 zapatilla=toon('PAC_Zapatilla', '#eae9e5', '#c4c2bc'),
 suela=toon('PAC_Suela', '#bcbbb6', '#97958f'),
 calcetin=toon('PAC_Calcetin', '#f2f1ed', '#cfccc6'),
 mascara=toon('PAC_Mascarilla', '#8c9095', '#686c71'),
 mascara_osc=toon('PAC_Mascarilla_Oscura', '#4b4f54', '#35383c'),
)
def remap(ob, table, default):
    mats = [table.get(m.name, default) if m else default for m in ob.data.materials]
    for i, m in enumerate(mats): ob.data.materials[i] = m
def nonarm_mods(ob, keep=('ARMATURE',)):
    for m in [m for m in ob.modifiers if m.type not in keep]: ob.modifiers.remove(m)
def move(ob, coll):
    for c in list(ob.users_collection): c.objects.unlink(ob)
    coll.objects.link(ob)
def bvh_of(ob):
    return BVHTree.FromObject(ob, bpy.context.evaluated_depsgraph_get(), deform=False, cage=True) if False else BVHTree.FromPolygons([v.co.copy() for v in ob.data.vertices], [tuple(p.vertices) for p in ob.data.polygons])

# ---------- camiseta ----------
with bpy.data.libraries.load(VROID) as (src, dst): dst.objects = ['INV_Camisa']
tee = dst.objects[0]; tee.name = 'PAC_Camiseta'; tee.data = tee.data.copy(); tee.data.name = 'PAC_Camiseta'; tee.parent = None
for o in list(O):
    if o is not tee and not o.users_scene: O.remove(o)
ropa.objects.link(tee)
keep_faces(tee, lambda c: abs(c.x) < 0.30 and c.z < 1.462 + (0.012 if c.y > 0 else 0.0) - (0.01 if (c.y < 0 and abs(c.x) < 0.05) else 0))
def tee_off(co):
    z = co.z
    if z < 1.06: return 0.0135
    if z < 1.20: return 0.0135 - (z - 1.06) / 0.14 * 0.0045
    return 0.009
offset_normals(tee, tee_off)
bm = bm_of(tee); smooth(bm, iters=3, fac=0.4); bm_back(bm, tee)
set_mats(tee, [M['camiseta']]); armature_child(tee, rig); add_solidify(tee, 0.002, offset=-1.0)
for p in tee.data.polygons: p.use_smooth = True

# ---------- chaqueta ----------
ch = O['INV_Chaqueta']; ch.name = 'PAC_Chaqueta'; ch.data.name = 'PAC_Chaqueta'; move(ch, ropa)
SLEEVE = 0.405
keep_faces(ch, lambda c: abs(c.x) < SLEEVE)
remap(ch, {'INV_Chaqueta_Gris': M['chaqueta'], 'INV_Chaqueta_Grafito': M['chaqueta'], 'INV_Acento_Amarillo': M['chaq_osc'], 'INV_Forro_Oscuro': M['forro']}, M['chaqueta'])
for s in (1, -1):   # puño remangado
    pu = ch.copy(); pu.data = ch.data.copy(); pu.name = pu.data.name = 'PAC_Chaqueta_Puno_' + ('L' if s > 0 else 'R'); ropa.objects.link(pu)
    keep_faces(pu, lambda c, s=s: c.x * s > SLEEVE - 0.035)
    offset_normals(pu, 0.006)
    set_mats(pu, [M['chaq_osc']]); nonarm_mods(pu); add_solidify(pu, 0.004, offset=1.0)
cu = O['INV_Chaqueta_Cuello']; cu.name = cu.data.name = 'PAC_Chaqueta_Cuello'; move(cu, ropa)
remap(cu, {'INV_Chaqueta_Gris': M['chaqueta'], 'INV_Acento_Amarillo': M['claro']}, M['chaqueta'])

# ---------- capucha caída sobre la espalda ----------
dg = bpy.context.evaluated_depsgraph_get()
chb = BVHTree.FromObject(ch, dg)
def on_back(x, z, lift):
    hit = chb.ray_cast(V((x, 0.6, z)), V((0, -1, 0)))
    if hit[0] is None: return None
    return hit[0] + V((0, lift, 0))
NU, NV = 16, 14
verts = []; faces = []; uv_t = []
for j in range(NV + 1):
    t = j / NV; z = 1.505 - t * 0.30
    half = 0.165 * (1 - t ** 1.8) + 0.02 * (1 - t)
    for i in range(NU + 1):
        u = i / NU * 2 - 1
        lift = 0.006 + 0.045 * (1 - t) ** 2 * (1 - 0.6 * u * u) + 0.010 * (1 - u * u)
        p = on_back(u * max(half, 0.004), z, lift)
        verts.append(p if p else V((u * half, 0.16, z))); uv_t.append((u, t))
for j in range(NV):
    for i in range(NU):
        a = j * (NU + 1) + i; faces.append((a, a + 1, a + NU + 2, a + NU + 1))
me = bpy.data.meshes.new('PAC_Capucha'); me.from_pydata([v[:] for v in verts], [], faces)
cap = new_obj('PAC_Capucha', ropa, me)
set_mats(cap, [M['chaqueta'], M['claro'], M['chaq_osc']])
bm = bm_of(cap); bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
if sum(f.normal.y for f in bm.faces) < 0: bmesh.ops.reverse_faces(bm, faces=bm.faces)
bm.faces.ensure_lookup_table()
for f in bm.faces:
    j, i = divmod(f.index, NU); u = (i + 0.5) / NU * 2 - 1; t = (j + 0.5) / NV
    if abs(u) > 0.86 or t > 0.9: f.material_index = 2          # borde de la capucha
    else: f.material_index = 0
bm_back(bm, cap)
copy_weights(cap, ch); armature_child(cap, rig)
add_solidify(cap, 0.010, offset=-1.0)
for p in cap.data.polygons: p.use_smooth = True
# emblema triangular oscuro en el centro del panel
def tri_ring(name, center, normal, r_out, r_in, mat, coll, bone):
    n = normal.normalized(); up = V((0, 0, 1)); side = up.cross(n).normalized(); up = n.cross(side).normalized()
    pts_o = []; pts_i = []
    for k in range(3):
        a = math.radians(90 + 120 * k + (180 if n.z == 0 and False else 0))
        dirv = side * math.cos(a) + up * math.sin(a)
        pts_o.append(center + dirv * r_out); pts_i.append(center + dirv * r_in)
    vs = pts_o + pts_i; fs = [(k, (k + 1) % 3, 3 + (k + 1) % 3, 3 + k) for k in range(3)]
    me = bpy.data.meshes.new(name); me.from_pydata([v[:] for v in vs], [], fs)
    ob = new_obj(name, coll, me); set_mats(ob, [mat]); add_solidify(ob, 0.0015, offset=1.0)
    rigid_weight(ob, bone); armature_child(ob, rig); return ob
capb = BVHTree.FromObject(cap, bpy.context.evaluated_depsgraph_get())
def proj_back(x, z, lift):
    h = capb.ray_cast(V((x, 0.6, z)), V((0, -1, 0)))
    return (h[0] + V((0, lift, 0))) if h[0] else V((x, 0.16, z))
A, Bp, C = V((-0.105, 0, 1.478)), V((0.105, 0, 1.478)), V((0, 0, 1.262))
R_ = 10; vs = []; idx = {}
for r in range(R_ + 1):
    for k in range(r + 1):
        a = 1 - r / R_; b = (r - k) / R_; c = k / R_
        p = C * a + A * b + Bp * c
        idx[(r, k)] = len(vs); vs.append(proj_back(p.x, p.z, 0.0018))
fs = []
for r in range(R_):
    for k in range(r + 1):
        fs.append((idx[(r, k)], idx[(r + 1, k)], idx[(r + 1, k + 1)]))
        if k < r: fs.append((idx[(r, k)], idx[(r + 1, k + 1)], idx[(r, k + 1)]))
me = bpy.data.meshes.new('PAC_Panel_Capucha'); me.from_pydata([v[:] for v in vs], [], fs)
pan = new_obj('PAC_Panel_Capucha', ropa, me); set_mats(pan, [M['claro']])
for p in pan.data.polygons: p.use_smooth = True
copy_weights(pan, cap); armature_child(pan, rig)
c0 = proj_back(0, 1.405, 0.0032)
tri = tri_ring('PAC_Emblema_Espalda', c0, V((0, 1, 0)), 0.042, 0.028, M['chaq_osc'], ropa, 'J_Bip_C_UpperChest')
# triángulo en el pecho de la camiseta (lado izquierdo del personaje)
tb = bvh_of(tee); hit = tb.ray_cast(V((0.0, -0.6, 1.375)), V((0, 1, 0)))
tri_ring('PAC_Logo_Camiseta', hit[0] + V((0, -0.0025, 0)), V((0, -1, 0)), 0.019, 0.013, M['correa'], ropa, 'J_Bip_C_UpperChest')

# ---------- pantalón cargo ancho ----------
pa = O['INV_Pantalon']; pa.name = pa.data.name = 'PAC_Pantalon'; move(pa, ropa)
def leg_axis(x, z):
    s = 1 if x > 0 else -1
    if z > 0.588: t = (0.982 - z) / 0.394; y = 0.001 + t * 0.008
    else: t = (0.588 - z) / 0.462; y = 0.009 + t * 0.031
    return V((0.086 * s, y, z))
def extra(z):
    if z > 0.95: return 0.0
    if z > 0.80: return 0.010 * (0.95 - z) / 0.15
    if z > 0.30: return 0.010 + 0.028 * (0.80 - z) / 0.50
    return 0.038 + 0.010 * min(1, (0.30 - z) / 0.20)
def baggy(co):
    a = leg_axis(co.x, co.z); d = V((co.x - a.x, co.y - a.y, 0))
    if d.length < 1e-6: return co
    e = extra(co.z)
    inward = (d.x * (1 if co.x > 0 else -1)) < 0
    if inward and co.z > 0.70: e *= max(0.15, (co.z - 0.70) and 0.15)
    return co + d.normalized() * e
bm = bm_of(pa)
# alargar las perneras hasta el tobillo
for loop in boundary_loops(bm):
    vs = list({v for e in loop for v in e.verts})
    if max(v.co.z for v in vs) > 0.30: continue
    edges = loop
    for step in range(3):
        r = bmesh.ops.extrude_edge_only(bm, edges=edges)
        nv = [g for g in r['geom'] if isinstance(g, bmesh.types.BMVert)]
        for v in nv: v.co.z -= 0.05
        edges = [g for g in r['geom'] if isinstance(g, bmesh.types.BMEdge) and g.is_boundary]
for v in bm.verts:
    if v.co.z < 0.95: v.co = baggy(v.co)
smooth(bm, iters=4, fac=0.35, mask=lambda v: v.co.z < 0.9)
bm_back(bm, pa)
remap(pa, {}, M['pantalon'])
for p in pa.data.polygons: p.use_smooth = True
SRCB = {part: O['INV_Bolsillo_Cargo_L' + part] for part in ('', '_Tapa')}
for s, lab in ((-1, 'R'), (1, 'L')):
    for part, mat in (('', M['pantalon']), ('_Tapa', M['pant_sombra'])):
        src = SRCB[part]
        ob = src if s > 0 else src.copy()
        if s < 0:
            ob.data = src.data.copy(); ropa.objects.link(ob)
            for v in ob.data.vertices: v.co.x *= -1
            ob.data.flip_normals()
            for vg in ob.vertex_groups:
                if '_L_' in vg.name: vg.name = vg.name.replace('_L_', '_X_')
            for vg in ob.vertex_groups:
                if '_R_' in vg.name: vg.name = vg.name.replace('_R_', '_L_')
            for vg in ob.vertex_groups:
                if '_X_' in vg.name: vg.name = vg.name.replace('_X_', '_R_')
        ob.name = ob.data.name = f'PAC_Bolsillo_Cargo_{lab}{part}'
        move(ob, ropa)
        for v in ob.data.vertices: v.co = baggy(v.co)
        set_mats(ob, [mat])
# correas que cuelgan del cinturón por la parte delantera del muslo
pb = bvh_of(pa)
def strap(name, x0, z0, x1, z1, w=0.022):
    rows = []
    for k in range(13):
        t = k / 12; x = x0 + (x1 - x0) * t; z = z0 + (z1 - z0) * t
        loc, nor, _, _ = pb.find_nearest(V((x, -0.2, z)))
        hit = pb.ray_cast(V((x, -0.6, z)), V((0, 1, 0)))
        p = hit[0] if hit[0] else loc; n = hit[1] if hit[0] else nor
        side = V((0, 0, 1)).cross(n).normalized()
        rows.append((p + n * 0.004 - side * w / 2, p + n * 0.004 + side * w / 2))
    vs = [c for r in rows for c in r]; fs = [(2 * k, 2 * k + 1, 2 * k + 3, 2 * k + 2) for k in range(len(rows) - 1)]
    me = bpy.data.meshes.new(name); me.from_pydata([v[:] for v in vs], [], fs)
    ob = new_obj(name, ropa, me); set_mats(ob, [M['correa']]); add_solidify(ob, 0.003, offset=1.0)
    copy_weights(ob, pa); armature_child(ob, rig); return ob, rows
for s, lab in ((1, 'L'), (-1, 'R')):
    ob, rows = strap('PAC_Correa_' + lab, 0.105 * s, 1.0, 0.125 * s, 0.60)
    # hebilla
    a, b = rows[5]; c = (a + b) / 2; n = (b - a).normalized()
    bpy.ops.mesh.primitive_cube_add(size=1, location=c + V((0, -0.006, 0)))
    hb = bpy.context.active_object; hb.name = hb.data.name = 'PAC_Hebilla_' + lab; hb.scale = (0.03, 0.008, 0.026)
    bpy.ops.object.transform_apply(scale=True)
    for col in list(hb.users_collection): col.objects.unlink(hb)
    ropa.objects.link(hb); set_mats(hb, [M['metal']])
    bv = hb.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.002; bv.segments = 2
    copy_weights(hb, pa); armature_child(hb, rig)

# ---------- zapatillas y calcetines ----------
for lab in 'LR':
    bo = O['INV_Bota_' + lab]
    so = bo.copy(); so.data = bo.data.copy(); so.name = so.data.name = 'PAC_Calcetin_' + lab; ropa.objects.link(so)
    bo.name = bo.data.name = 'PAC_Zapatilla_' + lab; move(bo, ropa)
    keep_faces(bo, lambda c: c.z < 0.118)
    remap(bo, {'INV_Bota_Clara': M['zapatilla'], 'INV_Bota_Grafito': M['suela'], 'INV_Acento_Amarillo': M['suela']}, M['zapatilla'])
    keep_faces(so, lambda c: c.z < 0.17)
    offset_normals(so, -0.009)
    bm = bm_of(so); smooth(bm, iters=6, fac=0.5); bm_back(bm, so)
    set_mats(so, [M['calcetin']]); nonarm_mods(so); add_solidify(so, 0.002, offset=1.0)
    for p in so.data.polygons: p.use_smooth = True

# ---------- accesorios ----------
skin = O['PAC_Piel_Extra']
def ring_at(name, center, axis, radius, height, mat, coll, bone=None, segs=32, src_w=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=segs, radius=radius, depth=height, location=center, end_fill_type='NOTHING')
    ob = bpy.context.active_object; ob.name = ob.data.name = name
    ob.rotation_euler = V((0, 0, 1)).rotation_difference(axis).to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for col in list(ob.users_collection): col.objects.unlink(ob)
    coll.objects.link(ob); set_mats(ob, [mat])
    add_solidify(ob, 0.003, offset=1.0)
    bv = ob.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.0012; bv.segments = 2
    for p in ob.data.polygons: p.use_smooth = True
    if bone: rigid_weight(ob, bone)
    else: copy_weights(ob, src_w)
    armature_child(ob, rig); return ob
def radius_around(ob, center, axis, band):
    a = axis.normalized(); rs = []
    for v in ob.data.vertices:
        d = v.co - center; h = d.dot(a)
        if abs(h) < band: rs.append((d - a * h).length)
    rs.sort(); return rs[int(len(rs) * 0.9)] if rs else 0.05
# collarín con sensor
nc = V((0, 0.032, 1.505))
rn = radius_around(O['VRoid_Cuerpo'], nc, V((0, 0, 1)), 0.01)
col = ring_at('PAC_Collarin', nc, V((0, 0.12, 1)).normalized(), rn + 0.004, 0.034, M['blanco'], acc, 'J_Bip_C_Neck')
bpy.ops.mesh.primitive_cube_add(size=1, location=nc + V((rn + 0.011, -0.006, 0)))
se = bpy.context.active_object; se.name = se.data.name = 'PAC_Sensor_Cuello'; se.scale = (0.012, 0.028, 0.02)
bpy.ops.object.transform_apply(scale=True)
for c_ in list(se.users_collection): c_.objects.unlink(se)
acc.objects.link(se); set_mats(se, [M['correa'], M['azul']])
bm = bm_of(se)
for f in bm.faces:
    if f.normal.x > 0.9: f.material_index = 1
bm_back(bm, se)
bv = se.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.003; bv.segments = 3
rigid_weight(se, 'J_Bip_C_Neck'); armature_child(se, rig)
# pulseras en las muñecas (cubren la unión entre mano y antebrazo)
for s, lab, mat in ((1, 'L', M['correa']), (-1, 'R', M['blanco'])):
    c = V((0.612 * s, 0.029, 1.432)); r = radius_around(skin, c, V((1, 0, 0)), 0.006)
    ring_at('PAC_Pulsera_' + lab, c, V((1, 0, 0)), r + 0.003, 0.026, mat, acc, None, src_w=skin)
# puerto vascular: antebrazo derecho, cara palmar (abajo en pose T), cerca del codo
sb = bvh_of(skin)
hit = sb.ray_cast(V((-0.455, 0.029, 1.20)), V((0, 0, 1)))
pp, pn = hit[0], hit[1]
def patch(name, center, n, w, h, lift, mat):
    n = n.normalized(); ax = V((1, 0, 0)); ay = n.cross(ax).normalized(); ax = ay.cross(n).normalized()
    vs = []; fs = []; N = 6
    for j in range(N + 1):
        for i in range(N + 1):
            p = center + ax * ((i / N - 0.5) * w) + ay * ((j / N - 0.5) * h)
            hh = sb.find_nearest(p); q = hh[0] + hh[1] * lift
            vs.append(q)
    for j in range(N):
        for i in range(N):
            a = j * (N + 1) + i; fs.append((a, a + 1, a + N + 2, a + N + 1))
    me = bpy.data.meshes.new(name); me.from_pydata([v[:] for v in vs], [], fs)
    ob = new_obj(name, acc, me); set_mats(ob, [mat]); add_solidify(ob, 0.0012, offset=1.0)
    for p in ob.data.polygons: p.use_smooth = True
    rigid_weight(ob, 'J_Bip_R_LowerArm'); armature_child(ob, rig); return ob
patch('PAC_Puerto_Parche', pp, pn, 0.05, 0.036, 0.0015, M['blanco'])
bpy.ops.mesh.primitive_cube_add(size=1, location=pp + pn.normalized() * 0.006)
dv = bpy.context.active_object; dv.name = dv.data.name = 'PAC_Puerto_Vascular'; dv.scale = (0.022, 0.016, 0.007)
dv.rotation_euler = V((0, 0, 1)).rotation_difference(pn).to_euler()
bpy.ops.object.transform_apply(rotation=True, scale=True)
for c_ in list(dv.users_collection): c_.objects.unlink(dv)
acc.objects.link(dv); set_mats(dv, [M['metal'], M['azul']])
bm = bm_of(dv)
for f in bm.faces:
    if f.normal.dot(pn) > 0.9: f.material_index = 1
bm_back(bm, dv)
bv = dv.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.0025; bv.segments = 3
rigid_weight(dv, 'J_Bip_R_LowerArm'); armature_child(dv, rig)
# mascarilla respiratoria (a partir de la zona de nariz y boca de la cara)
fa = O['VRoid_Cara']
fb = bvh_of(fa)
NU, NV = 18, 14; vs = []; fs = []
X0, X1, Z0, Z1 = -0.056, 0.056, 1.532, 1.627
for j in range(NV + 1):
    for i in range(NU + 1):
        u = i / NU * 2 - 1; w = j / NV * 2 - 1
        # contorno redondeado (superelipse), más estrecho arriba (puente de la nariz)
        top = (w + 1) / 2
        x = u * (X1 - 0.026 * top ** 2) * (1 - abs(w) ** 6) ** 0.17
        z = Z0 + (Z1 - Z0) * (w + 1) / 2
        h = fb.ray_cast(V((x, -0.5, z)), V((0, 1, 0)))
        if h[0] is None:
            h = fb.find_nearest(V((x, -0.08, z)))
        bulge = 0.008 + 0.010 * (1 - u * u) * (1 - (w * 0.9) ** 2)
        vs.append(h[0] + V((0, -bulge, 0)))
for j in range(NV):
    for i in range(NU):
        a_ = j * (NU + 1) + i; fs.append((a_, a_ + 1, a_ + NU + 2, a_ + NU + 1))
me = bpy.data.meshes.new('PAC_Mascarilla'); me.from_pydata([v[:] for v in vs], [], fs)
mk = new_obj('PAC_Mascarilla', acc, me)
bm = bm_of(mk); smooth(bm, iters=4, fac=0.45, keep_boundary=False)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
if sum(f.normal.y for f in bm.faces) > 0: bmesh.ops.reverse_faces(bm, faces=bm.faces)
bm_back(bm, mk)
set_mats(mk, [M['mascara']]); add_solidify(mk, 0.004, offset=-1.0)
bv = mk.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.0015; bv.segments = 2
for p in mk.data.polygons: p.use_smooth = True
rigid_weight(mk, 'J_Bip_C_Head'); armature_child(mk, rig)
mb = bvh_of(mk)
def cyl(name, loc, direction, r, d, mat, segs=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=segs, radius=r, depth=d, location=loc)
    ob = bpy.context.active_object; ob.name = ob.data.name = name
    ob.rotation_euler = V((0, 0, 1)).rotation_difference(direction).to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for c_ in list(ob.users_collection): c_.objects.unlink(ob)
    acc.objects.link(ob); set_mats(ob, [mat])
    bv = ob.modifiers.new('Bisel', 'BEVEL'); bv.width = min(0.002, r * 0.2); bv.segments = 2
    rigid_weight(ob, 'J_Bip_C_Head'); armature_child(ob, rig); return ob
for s, lab in ((1, 'L'), (-1, 'R')):
    h = mb.ray_cast(V((0.038 * s, -0.5, 1.572)), V((0, 1, 0)))
    if h[0]:
        dirv = (h[1] + V((0.5 * s, 0, -0.15))).normalized()
        cyl('PAC_Filtro_' + lab, h[0] + dirv * 0.011, dirv, 0.0175, 0.016, M['mascara_osc'])
        cyl('PAC_Filtro_Tapa_' + lab, h[0] + dirv * 0.020, dirv, 0.012, 0.004, M['mascara'])
h = mb.ray_cast(V((0, -0.5, 1.567)), V((0, 1, 0)))
if h[0]:
    cyl('PAC_Valvula', h[0] + V((0, -0.008, -0.002)), V((0, -1, -0.2)).normalized(), 0.011, 0.014, M['mascara_osc'])
# cinta de la mascarilla alrededor de la cabeza
hc = V((0, 0.012, 1.598)); rh = radius_around(fa, hc, V((0, -0.35, 1)).normalized(), 0.006)
cin = ring_at('PAC_Mascarilla_Cinta', hc, V((0, -0.35, 1)).normalized(), rh + 0.006, 0.008, M['mascara_osc'], acc, 'J_Bip_C_Head', segs=48)
keep_faces(cin, lambda c: c.y > -0.045)
# ---------- limpieza ----------
for o in list(O):
    if o.name.startswith('INV_'): O.remove(o)
for n in ('Investigador_Ropa_y_Equipo', 'Investigador_Props'):
    c = bpy.data.collections.get(n)
    if c: bpy.data.collections.remove(c)
# ojos de un azul más claro (modificación genética)
iris = bpy.data.materials['N00_000_00_EyeIris_00_EYE (Instance)']
ou = iris.node_tree.nodes['Mtoon1Material.Mtoon1Output']
ou.inputs['Lit Color'].default_value = (0.78, 0.93, 1.25, 1); ou.inputs['Shade Color'].default_value = (0.70, 0.85, 1.15, 1)
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/02_ropa.blend')
print('OK02', sorted(o.name for o in O if o.type == 'MESH'))
