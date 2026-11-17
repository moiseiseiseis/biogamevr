"""04 · Equipo: botas, guantes hápticos, cinturón, arnés con panel cian, correas de muslo,
bolsillos cargo, diadema con micrófono y parche con el emblema de ADN."""
import os, sys, subprocess
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
from mathutils.geometry import convex_hull_2d
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '03_prendas.blend'))
rig = bpy.data.objects['Operador_Mecha_Rig']; piel = bpy.data.objects['VRoid_Cuerpo']
eq = coleccion('Operador_Equipo')
O = bpy.data.objects
tun, pan = O['OPM_Tunica'], O['OPM_Pantalon']
M = dict(
    correa=mtoon('OPM_Correa', srgb('#211e19'), shade=srgb('#141210')),
    naranja=mtoon('OPM_Naranja', srgb('#ce773c'), shade=srgb('#9c5428')),
    gris=mtoon('OPM_Gris', srgb('#847d75'), shade=srgb('#625c56')),
    gris_claro=mtoon('OPM_Gris_Claro', srgb('#c8c3be'), shade=srgb('#9d9893')),
    bota=mtoon('OPM_Bota', srgb('#2b2824'), shade=srgb('#1b1916')),
    guante=mtoon('OPM_Guante', srgb('#8a857e'), shade=srgb('#66625c')),
    cian=mtoon('OPM_Cian', srgb('#5fd0c4'), shade=srgb('#3f9e95')),
)
M['cian'].vrm_addon_extension.mtoon1.emissive_factor = srgb('#3fb8ac')

def nueva(nombre, mats):
    me = bpy.data.meshes.new(nombre); o = bpy.data.objects.new(nombre, me); eq.objects.link(o)
    for m in mats: me.materials.append(m)
    o.parent = rig; md = o.modifiers.new('Armature', 'ARMATURE'); md.object = rig
    return o

def suave(o):
    for p in o.data.polygons: p.use_smooth = True

def caja(nombre, centro, ejes, medidas, mat, bisel=0.002):
    """Caja orientada: ejes = (u, v, n) unitarios; medidas = (ancho, alto, grosor)."""
    o = nueva(nombre, [mat]); bm = bmesh.new()
    u, v, n = [V(e).normalized() for e in ejes]; c = V(centro)
    vs = [bm.verts.new(c + u * (sx * medidas[0] / 2) + v * (sy * medidas[1] / 2) + n * (sz * medidas[2] / 2))
          for sx in (-1, 1) for sy in (-1, 1) for sz in (-1, 1)]
    bmesh.ops.convex_hull(bm, input=vs)
    bm.to_mesh(o.data); bm.free()
    b = o.modifiers.new('Bisel', 'BEVEL'); b.width = bisel; b.segments = 1; b.limit_method = 'ANGLE'
    while o.modifiers.find('Bisel') > 0: o.modifiers.move(o.modifiers.find('Bisel'), 0)
    return o

def curva(puntos, n=24):
    """Catmull-Rom por los puntos de control."""
    P = [V(p) for p in puntos]; P = [P[0]] + P + [P[-1]]; out = []
    for i in range(1, len(P) - 2):
        p0, p1, p2, p3 = P[i - 1], P[i], P[i + 1], P[i + 2]
        for k in range(n):
            t = k / n
            out.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
    out.append(P[-2]); return out

def cinta(nombre, puntos, ancho, bvh, desfase, mat, grosor=0.003, n=4):
    """Cinta proyectada sobre una superficie (find_nearest) y separada hacia su normal."""
    o = nueva(nombre, [mat]); bm = bmesh.new()
    pts = curva(puntos, n); prev = None; filas = []
    for i, p in enumerate(pts):
        loc, nor, _, _ = bvh.find_nearest(p)
        q = loc + nor * desfase
        tg = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        b = tg.cross(nor).normalized()
        filas.append((bm.verts.new(q - b * ancho / 2), bm.verts.new(q + b * ancho / 2)))
    for (a0, a1), (b0, b1) in zip(filas[:-1], filas[1:]):
        bm.faces.new((a0, a1, b1, b0))
    bm.normal_update()
    # que la cinta mire hacia fuera (misma dirección que la normal de la superficie)
    for f in bm.faces:
        loc, nor, _, _ = bvh.find_nearest(f.calc_center_median())
        if f.normal.dot(nor) < 0: f.normal_flip()
    centros = [((a.co + b.co) / 2, (b.co - a.co).normalized()) for a, b in filas]
    bm.to_mesh(o.data); bm.free(); suave(o)
    add_solidify(o, grosor, offset=1.0)
    return o, centros

base = copia_malla(piel, 'OPM_Base_Tmp', eq); bm = bm_of(base)
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0002); bm_back(bm, base)
PIEL = bvh_de(base)

# ---------- botas: caña desde la piel del pie + suela clara gruesa ----------
for s, lab in ((1, 'L'), (-1, 'R')):
    b = copia_malla(base, 'OPM_Bota_' + lab, eq)
    bm = bm_of(b)
    corte(bm, (0, 0, 0.205), (0, 0, 1))
    quitar_caras(bm, lambda c: c.x * s < 0.01)
    bm.normal_update()
    for v in bm.verts: v.co += v.normal * 0.008
    smooth(bm, iters=3, fac=0.35)
    empujar(bm, PIEL, 0.008)
    for f in bm.faces:
        c = f.calc_center_median()
        f.material_index = 1 if 0.150 < c.z < 0.185 else (2 if (c.y < -0.085 and c.z < 0.07) else 0)   # franja naranja, puntera gris
    bm_back(bm, b)
    material(b, [M['bota'], M['naranja'], M['gris']])
    add_solidify(b, 0.003, offset=1.0, use_rim=True).use_rim_only = True
    # suela: contorno convexo del pie, algo más ancho, de 0 a 3 cm
    pie = [v.co for v in base.data.vertices if v.co.z < 0.06 and v.co.x * s > 0.01]
    xy = [V((c.x, c.y)) for c in pie]; hull = [xy[i] for i in convex_hull_2d(xy)]
    cen = sum(hull, V((0, 0))) / len(hull)
    so = nueva('OPM_Suela_' + lab, [M['gris_claro']]); bm = bmesh.new()
    abajo = [bm.verts.new((cen.x + (p.x - cen.x) * 1.16, cen.y + (p.y - cen.y) * 1.10, 0.0)) for p in hull]
    arriba = [bm.verts.new((cen.x + (p.x - cen.x) * 1.16, cen.y + (p.y - cen.y) * 1.10, 0.030)) for p in hull]
    n = len(hull)
    bm.faces.new(arriba); bm.faces.new(list(reversed(abajo)))
    for k in range(n): bm.faces.new((abajo[k], abajo[(k + 1) % n], arriba[(k + 1) % n], arriba[k]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(so.data); bm.free()
    bv = so.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.006; bv.segments = 1; bv.limit_method = 'ANGLE'
    so.modifiers.move(so.modifiers.find('Bisel'), 0)
    rigid_weight(so, 'J_Bip_%s_Foot' % lab)
    # lengüeta trasera naranja
    caja('OPM_Bota_Tirador_' + lab, (0.077 * s, 0.062, 0.19), ((1, 0, 0), (0, 0, 1), (0, 1, 0)), (0.022, 0.035, 0.006), M['naranja'])

# ---------- guantes hápticos con muñequera ----------
for s, lab in ((1, 'L'), (-1, 'R')):
    g = copia_malla(base, 'OPM_Guante_' + lab, eq)
    bm = bm_of(g)
    corte(bm, (0.505 * s, 0, 0), (-s, 0, 0))
    bm.normal_update()
    for v in bm.verts: v.co += v.normal * 0.0025
    smooth(bm, iters=1, fac=0.3)
    empujar(bm, PIEL, 0.0025)
    bm_back(bm, g); material(g, [M['guante']])
    dm = g.modifiers.new('Reducir', 'DECIMATE'); dm.ratio = 0.5; dm.use_collapse_triangulate = True
    g.modifiers.move(g.modifiers.find('Reducir'), 0)
    with bpy.context.temp_override(object=g, active_object=g, selected_objects=[g]):
        bpy.ops.object.modifier_apply(modifier='Reducir')
    mu = nueva('OPM_Muniequera_' + lab, [M['gris_claro'], M['naranja']]); bm = bmesh.new()
    eje = lambda t, s=s: (V((s * t, 0.025, 1.270)), V((s, 0, 0)))
    def rad(t, d): return radio_desde_eje(PIEL, eje(t)[0], d, 0.03) + 0.0075
    an = tubo(bm, [0.507, 0.522, 0.540], eje, rad, lados=14)
    for f in bm.faces:
        c = f.calc_center_median(); f.material_index = 1 if abs(c.x) > 0.531 and abs(c.x) < 0.535 else 0
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mu.data); bm.free(); suave(mu); add_solidify(mu, 0.004, offset=1.0)
    rigid_weight(mu, 'J_Bip_%s_Hand' % lab)

# ---------- cinturón ancho sobre la túnica ----------
TUN = bvh_de(tun)
ci = nueva('OPM_Cinturon', [M['correa']]); bm = bmesh.new()
eje_c = lambda t: (V((0, 0, t)), V((0, 0, -1)))
tubo(bm, [1.045, 1.020, 0.995], eje_c, lambda t, d: radio_exterior(TUN, eje_c(t)[0], d, defecto=0.14) + 0.004, lados=28)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
bm.to_mesh(ci.data); bm.free(); suave(ci); add_solidify(ci, 0.006, offset=1.0)
yfr = -radio_exterior(TUN, V((0, 0, 1.02)), V((0, -1, 0)), defecto=0.11) - 0.012
caja('OPM_Hebilla', (0, yfr, 1.020), ((1, 0, 0), (0, 0, 1), (0, -1, 0)), (0.060, 0.044, 0.010), M['gris'], 0.003)
caja('OPM_Hebilla_Placa', (0, yfr - 0.0055, 1.020), ((1, 0, 0), (0, 0, 1), (0, -1, 0)), (0.040, 0.024, 0.003), M['naranja'], 0.001)

# ---------- arnés: tirantes, cinta de pecho con panel cian ----------
EXT_T = bvh_de(tun, O['OPM_Tunica_Cuello'])
for s, lab in ((1, 'L'), (-1, 'R')):
    ruta = [(0.075 * s, -0.16, 1.00), (0.080 * s, -0.16, 1.12), (0.085 * s, -0.16, 1.24), (0.095 * s, -0.08, 1.35),
            (0.100 * s, 0.02, 1.38), (0.095 * s, 0.11, 1.31), (0.085 * s, 0.13, 1.18), (0.078 * s, 0.12, 1.06), (0.075 * s, 0.12, 1.00)]
    t_, filas = cinta('OPM_Tirante_' + lab, ruta, 0.032, EXT_T, 0.006, M['correa'], 0.003)
    # ajustadores naranja y gris en el tirante delantero
    for z0, mat, nom in ((1.27, M['naranja'], 'OPM_Ajuste_'), (1.10, M['gris'], 'OPM_Ajuste_Bajo_')):
        p, bdir = min(filas, key=lambda f: abs(f[0].z - z0) + (0 if f[0].y < 0 else 1))
        loc, nor, _, _ = EXT_T.find_nearest(p)
        caja(nom + lab, p + nor * 0.006, (bdir, nor.cross(bdir), nor), (0.040, 0.022, 0.008), mat, 0.002)
pecho = [(0.090, -0.17, 1.215), (0.03, -0.20, 1.215), (-0.03, -0.20, 1.215), (-0.090, -0.17, 1.215)]
cp, filas = cinta('OPM_Cinta_Pecho', pecho, 0.026, EXT_T, 0.010, M['correa'], 0.003)
p, bdir = min(filas, key=lambda f: abs(f[0].x))
loc, nor, _, _ = EXT_T.find_nearest(p)
tg = nor.cross(bdir).normalized()                                       # dirección a lo largo de la cinta
caja('OPM_Panel_Pecho', p + nor * 0.009, (tg, bdir, nor), (0.075, 0.032, 0.010), M['gris'], 0.003)
caja('OPM_Panel_Pecho_Cian', p + nor * 0.0145, (tg, bdir, nor), (0.062, 0.022, 0.002), M['cian'], 0.0008)

# ---------- correas de muslo y bolsillos cargo ----------
EXT_TP = bvh_de(tun, pan)
PANT = bvh_de(pan)
for s, lab in ((1, 'L'), (-1, 'R')):
    ruta = [(0.12 * s, -0.10, 1.00), (0.14 * s, -0.09, 0.88), (0.16 * s, -0.07, 0.76), (0.165 * s, -0.05, 0.66), (0.16 * s, -0.04, 0.58)]
    cinta('OPM_Correa_Muslo_' + lab, ruta, 0.024, EXT_TP, 0.006, M['correa'], 0.003)
    # bolsillo cargo en el costado del muslo (bajo el faldón)
    bo = nueva('OPM_Bolsillo_' + lab, [M['pantalon'] if 'pantalon' in M else bpy.data.materials['OPM_Pantalon']]); bm = bmesh.new()
    filas = []
    for j, z in enumerate((0.43, 0.48, 0.53, 0.58, 0.63)):
        fila = []
        for i, y in enumerate((-0.05, -0.025, 0.0, 0.025, 0.05)):
            hit = PANT.ray_cast(V((0.45 * s, y, z)), V((-s, 0, 0)), 0.5)
            q = hit[0] + hit[1] * 0.009 if hit[0] else V((0.16 * s, y, z))
            fila.append(bm.verts.new(q))
        filas.append(fila)
    for r0, r1 in zip(filas[:-1], filas[1:]):
        for k in range(4): bm.faces.new((r0[k], r0[k + 1], r1[k + 1], r1[k]))
    bm.normal_update()
    for f in bm.faces:
        if f.normal.x * s < 0: f.normal_flip()
    bm.to_mesh(bo.data); bm.free(); suave(bo)
    add_solidify(bo, 0.008, offset=-1.0, use_rim=True)
    tapa, _ = cinta('OPM_Bolsillo_Tapa_' + lab, [(0.2 * s, -0.055, 0.635), (0.2 * s, 0.0, 0.64), (0.2 * s, 0.055, 0.635)], 0.045, PANT, 0.018, bpy.data.materials['OPM_Pantalon_Puno'], 0.004)

# ---------- diadema con auriculares y micrófono ----------
XC, ZC = 0.133, 1.445
for s, lab in ((1, 'L'), (-1, 'R')):
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.030, depth=0.024, location=(XC * s, 0.004, ZC), rotation=(0, math.radians(90), 0))
    ec = bpy.context.active_object; ec.name = ec.data.name = 'OPM_Auricular_' + lab
    ec.scale = (1, 1.0, 1.25); bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for c in list(ec.users_collection): c.objects.unlink(ec)
    eq.objects.link(ec); material(ec, [M['gris_claro'], M['naranja'], M['correa']])
    for p in ec.data.polygons:
        if abs(p.normal.x) > 0.9 and p.normal.x * s < 0: p.material_index = 2      # almohadilla hacia la cabeza
    bv = ec.modifiers.new('Bisel', 'BEVEL'); bv.width = 0.006; bv.segments = 3; bv.limit_method = 'ANGLE'
    ec.parent = rig; md = ec.modifiers.new('Armature', 'ARMATURE'); md.object = rig
    caja('OPM_Auricular_Luz_' + lab, (XC * s + 0.0125 * s, -0.012, ZC - 0.012), ((0, 0, 1), (0, 1, 0), (s, 0, 0)), (0.020, 0.007, 0.002), M['naranja'], 0.0008)
# arco por encima del pelo
arco = nueva('OPM_Diadema', [M['correa']]); bm = bmesh.new(); filas = []
a, zc, b = XC - 0.002, ZC + 0.03, 1.612 + 0.006 - (ZC + 0.03)
for k in range(17):
    th = math.pi * k / 16; x = a * math.cos(th); z = zc + b * math.sin(th)
    filas.append((bm.verts.new((x, -0.012, z)), bm.verts.new((x, 0.012, z))))
for (a0, a1), (b0, b1) in zip(filas[:-1], filas[1:]): bm.faces.new((a0, a1, b1, b0))
bm.normal_update()
for f in bm.faces:
    c = f.calc_center_median()
    if f.normal.dot(V((c.x, 0, c.z - zc))) < 0: f.normal_flip()
bm.to_mesh(arco.data); bm.free(); suave(arco); add_solidify(arco, 0.008, offset=1.0)
# micrófono de brazo desde el auricular izquierdo hasta la comisura
cu = bpy.data.curves.new('OPM_Microfono', 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = 0.0025; cu.bevel_resolution = 1
sp = cu.splines.new('NURBS'); pts = [V((XC - 0.005, -0.020, ZC - 0.022)), V((0.10, -0.075, ZC - 0.040)), V((0.06, -0.100, 1.398)), V((0.028, -0.094, 1.392))]
sp.points.add(len(pts) - 1)
for pt, p in zip(sp.points, pts): pt.co = (*p, 1)
sp.use_endpoint_u = True; sp.order_u = 3; sp.resolution_u = 6
mo = bpy.data.objects.new('OPM_Microfono_Tmp', cu); eq.objects.link(mo); cu.materials.append(M['correa'])
bpy.context.view_layer.objects.active = mo; mo.select_set(True)
with bpy.context.temp_override(selected_editable_objects=[mo], active_object=mo, object=mo):
    bpy.ops.object.convert(target='MESH')
mo = O['OPM_Microfono_Tmp']; mo.name = mo.data.name = 'OPM_Microfono'; mo.parent = rig
md = mo.modifiers.new('Armature', 'ARMATURE'); md.object = rig
bpy.ops.mesh.primitive_uv_sphere_add(segments=10, ring_count=6, radius=0.006, location=(0.028, -0.094, 1.392))
mt = bpy.context.active_object; mt.name = mt.data.name = 'OPM_Microfono_Punta'; mt.scale = (1.3, 1, 1); bpy.ops.object.transform_apply(location=True, scale=True)
for c in list(mt.users_collection): c.objects.unlink(mt)
eq.objects.link(mt); material(mt, [M['gris']]); mt.parent = rig; md = mt.modifiers.new('Armature', 'ARMATURE'); md.object = rig
for n in ('OPM_Auricular_L', 'OPM_Auricular_R', 'OPM_Diadema', 'OPM_Microfono', 'OPM_Microfono_Punta', 'OPM_Auricular_Luz_L', 'OPM_Auricular_Luz_R'):
    rigid_weight(O[n], 'J_Bip_C_Head')

# ---------- parche redondo con el emblema de ADN (brazo izquierdo) ----------
subprocess.run([PYTHON, os.path.join(os.path.dirname(os.path.abspath(__file__)), 'emblema_adn.py'), W], check=True)
img = bpy.data.images.load(os.path.join(W, 'emblema-adn_512.png')); img.name = 'OPM_Emblema_ADN'; img.pack()
mp = mtoon('OPM_Parche', srgb('#ffffff'), shade=srgb('#d0d0d0'))
mp.vrm_addon_extension.mtoon1.pbr_metallic_roughness.base_color_texture.index.source = img
mp.vrm_addon_extension.mtoon1.extensions.vrmc_materials_mtoon.shade_multiply_texture.index.source = img
hit = TUN.ray_cast(V((0.205, 0.030, 1.60)), V((0, 0, -1)), 1.0)
c0, nrm = hit[0] + hit[1] * 0.004, hit[1].normalized()
u = V((1, 0, 0)); u = (u - nrm * u.dot(nrm)).normalized(); w = nrm.cross(u)
pa = nueva('OPM_Parche_Brazo', [mp]); bm = bmesh.new(); uvl = bm.loops.layers.uv.new('UVMap')
R, N = 0.036, 20
cv = bm.verts.new(c0); anillo = []
for k in range(N):
    ang = 2 * math.pi * k / N; p = c0 + (u * math.cos(ang) + w * math.sin(ang)) * R
    loc, nn, _, _ = TUN.find_nearest(p); anillo.append(bm.verts.new(loc + nn * 0.004))
for k in range(N):
    f = bm.faces.new((cv, anillo[k], anillo[(k + 1) % N]))
    for l in f.loops:
        d = l.vert.co - c0; l[uvl].uv = (0.5 + d.dot(u) / (2 * R) * 0.96, 0.5 + d.dot(w) / (2 * R) * 0.96)
bm.normal_update()
for f in bm.faces:
    if f.normal.dot(nrm) < 0: f.normal_flip()
bm.to_mesh(pa.data); bm.free(); suave(pa); add_solidify(pa, 0.002, offset=1.0)
rigid_weight(pa, 'J_Bip_L_UpperArm')

# ---------- piel oculta por guantes y botas ----------
bpy.data.objects.remove(base)
cubre = bvh_de(*[o for o in list(O) if o.type == 'MESH' and o.name.startswith('OPM_') and not o.name.startswith('OPM_Parche')])
bm = bm_of(piel)
cub = {v: (v.co.z < 1.33 or (abs(v.co.x) > 0.075 and v.co.z < 1.42)) and cubre.ray_cast(v.co + v.normal * 0.0005, v.normal, 0.08)[0] is not None for v in bm.verts}
cand = {f for f in bm.faces if all(cub[v] for v in f.verts)}
borde = {f for f in cand if any(g not in cand for v in f.verts for g in v.link_faces)}
bmesh.ops.delete(bm, geom=list(cand - borde), context='FACES')
bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
bm_back(bm, piel)
print('PIEL_OCULTA_BORRADA', len(cand - borde))
rep = {}
dg = bpy.context.evaluated_depsgraph_get()
for o in eq.objects:
    ev = o.evaluated_get(dg); m = ev.to_mesh(); m.calc_loop_triangles(); rep[o.name] = len(m.loop_triangles); ev.to_mesh_clear()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '04_equipo.blend'))
print('OK04', sum(rep.values()), rep)
