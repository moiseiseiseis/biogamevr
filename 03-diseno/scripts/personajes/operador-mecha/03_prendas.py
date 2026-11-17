"""03 · Prendas: camiseta negra de cuello alto, túnica verde oliva y pantalón cargo gris carbón."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from opm_util import *
enable()
bpy.ops.wm.open_mainfile(filepath=os.path.join(W, '02_separado.blend'))
rig = bpy.data.objects['Operador_Mecha_Rig']; piel = bpy.data.objects['VRoid_Cuerpo']
ropa = coleccion('Operador_Ropa')
# colores tomados de la lámina (muestras de la paleta y de la figura)
M = dict(
    tunica=mtoon('OPM_Tunica', srgb('#7a7864'), shade=srgb('#5c5b4a')),
    negro=mtoon('OPM_Negro_Tela', srgb('#2b2824'), shade=srgb('#1b1916')),
    pantalon=mtoon('OPM_Pantalon', srgb('#47443f'), shade=srgb('#33312d')),
    puno=mtoon('OPM_Pantalon_Puno', srgb('#3d3c3b'), shade=srgb('#2a2928')),
)
# base: la piel con las costuras UV fusionadas (evita grietas en espalda y entrepierna)
base = copia_malla(piel, 'OPM_Base_Tmp', ropa)
bm = bm_of(base)
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0002)
bm_back(bm, base)

PIEL = bvh_de(base)
def prenda(nombre, recorte, desplaz, suaviza=2, minimo=None):
    o = copia_malla(base, nombre, ropa)
    bm = bm_of(o)
    recorte(bm)
    bm.normal_update()
    for v in bm.verts:
        d = desplaz(v.co) if callable(desplaz) else desplaz
        v.co += v.normal * d
    smooth(bm, iters=suaviza, fac=0.35)
    # el suavizado encoge la malla: empuje hacia fuera con distancia mínima a la piel
    empujar(bm, PIEL, minimo or (lambda c: desplaz(c) if callable(desplaz) else desplaz))
    print('INVERTIDAS', nombre, len(caras_invertidas(bm, PIEL)))
    bm_back(bm, o)
    return o

# ---------- camiseta negra de manga larga y cuello alto ----------
def rec_camiseta(bm):
    corte(bm, (0, 0, 1.372), (0, -0.22, 1))           # cuello alto, un poco más alto en la nuca
    corte(bm, (0, 0, 1.12), (0, 0, -1))               # termina bajo la túnica (no se ve más abajo)
    corte(bm, (0.20, 0, 0), (1, 0, 0)); corte(bm, (-0.20, 0, 0), (-1, 0, 0))     # las mangas se tornean después
    quitar_caras(bm, lambda c: abs(c.x) < 0.2 and c.z < 0.86)                    # restos de pierna
camiseta = prenda('OPM_Camiseta_Negra', rec_camiseta, lambda c: 0.0035 if c.z < 1.30 else 0.005)
bm = bm_of(camiseta)
for s_ in (1, -1):
    lazo = densificar_borde(bm, lambda e, s_=s_: all(v.co.x * s_ > 0.19 for v in e.verts), 0.012)
    eje_b = lambda t, s_=s_: (V((s_ * t, 0.025, 1.274)), V((s_, 0, 0)))
    pasos = [0.24, 0.28, 0.33, 0.38, 0.43, 0.48, 0.532]
    torno(bm, lazo, pasos, eje_b, lambda t, d, s_=s_: radio_desde_eje(PIEL, eje_b(t)[0], d, 0.03) + 0.0035)
smooth(bm, iters=1, fac=0.25)
empujar(bm, PIEL, 0.0035)
print('VOLTEADAS', 'OPM_Camiseta_Negra', orientar_por_superficie(bm, PIEL))
bm_back(bm, camiseta)
material(camiseta, [M['negro']])

# ---------- pantalón cargo holgado ----------
CUFF = 0.175
def rec_pantalon(bm):
    corte(bm, (0, 0, 0.955), (0, 0, 1))
    corte(bm, (0, 0, 0.80), (0, 0, -1))                                 # las perneras se tornean
    quitar_caras(bm, lambda c: abs(c.x) > 0.20 and c.z > 0.85)        # brazos y manos
pan = prenda('OPM_Pantalon', rec_pantalon, 0.011, suaviza=3)
bm = bm_of(pan)
def holgura(z):
    if z > 0.82: return 0.0
    if z > 0.52: return 0.004 + 0.020 * (0.82 - z) / 0.30
    if z > 0.26: return 0.024 + 0.014 * (0.52 - z) / 0.26          # se abomba sobre el puño
    return 0.038 * max(0, (z - CUFF) / (0.26 - CUFF)) + 0.004       # y se recoge en el puño
for s_ in (1, -1):
    eje_p = lambda t, s_=s_: (eje_pierna(0.077 * s_, t), V((0, 0, -1)))
    pasos = [0.835, 0.79, 0.74, 0.67, 0.60, 0.53, 0.46, 0.39, 0.32, 0.26, 0.215, CUFF]
    prev = {}
    def rad(t, d, s_=s_, prev=prev):
        k = (round(d.x, 2), round(d.y, 2))
        rs = radio_desde_eje(PIEL, eje_p(t)[0], d, -1)
        if not (0.02 < rs < 0.085): rs = prev.get(k, 0.05)
        prev[k] = rs
        e = holgura(t) * (0.35 if d.x * s_ < -0.3 else 1.0)       # lado interior: menos holgura
        if t > 0.80: e -= 0.004                                     # arranca dentro de la cadera
        return rs + 0.011 + e
    tubo(bm, pasos, eje_p, rad, lados=12)
empujar(bm, PIEL, lambda c: 0.011 if c.z > 0.79 else -1)
print('VOLTEADAS', 'OPM_Pantalon', orientar_hacia_fuera(bm, lambda c: (eje_pierna(c.x, min(c.z, 0.86)), V((0, 0, -1))) if c.z < 0.86 else (V((0, 0, c.z)), V((0, 0, 1)))))
bm_back(bm, pan); material(pan, [M['pantalon']])
PANT = bvh_de(pan)
# puños recogidos sobre la bota
for s in (1, -1):
    pu = copia_malla(base, 'OPM_Pantalon_Puno_' + ('L' if s > 0 else 'R'), ropa)
    bm = bm_of(pu)
    corte(bm, (0, 0, CUFF + 0.035), (0, 0, 1)); corte(bm, (0, 0, CUFF - 0.002), (0, 0, -1))
    quitar_caras(bm, lambda c: c.x * s < 0)
    bm.normal_update()
    for v in bm.verts: v.co += v.normal * 0.016
    bm_back(bm, pu); material(pu, [M['puno']]); add_solidify(pu, 0.004, offset=1.0)
# ---------- túnica verde oliva ----------
HEM, SLIT = 0.70, 0.83
def rec_tunica(bm):
    corte(bm, (0, 0, 1.322), (0, -0.30, 1))           # escote bajo el cuello alzado
    corte(bm, (0, 0, 1.10), (0, 0, -1))               # bajo el busto; el resto se tornea
    corte(bm, (0.30, 0, 0), (1, 0, 0)); corte(bm, (-0.30, 0, 0), (-1, 0, 0))     # manga corta sobre el codo
def des_tunica(c):
    d = 0.0125
    if abs(c.x) > 0.16: d += 0.010 * min(1, (abs(c.x) - 0.16) / 0.12)   # manga holgada
    return d
tun = prenda('OPM_Tunica', rec_tunica, des_tunica, suaviza=8)
bm = bm_of(tun)
EXT = bvh_de(base, pan)
lazo = densificar_borde(bm, lambda e: all(v.co.z < 1.105 for v in e.verts), 0.03)
print('LAZO_TUNICA', len(lazo))
eje_t = lambda t: (V((0, 0.0, t)), V((0, 0, -1)))
pasos = [1.06, 1.02, 0.98, 0.94, 0.90, 0.86, 0.82, 0.77, 0.73, HEM]
rprev = {}
def rad_t(t, d):
    k = (round(d.x, 3), round(d.y, 3))
    r = radio_exterior(EXT, eje_t(t)[0], d, defecto=rprev.get(k, 0.15)) + (0.016 if t < 1.0 else 0.0125)
    if k in rprev and t < 0.98: r = max(r, rprev[k] * 1.004)          # cae recto desde la cadera
    rprev[k] = r; return r
torno(bm, lazo, pasos, eje_t, rad_t, salto_max=0.02)
smooth(bm, iters=1, fac=0.25, mask=lambda v: v.co.z < 1.1)
empujar(bm, PIEL, 0.0125)
empujar(bm, PANT, lambda c: 0.014 if c.z < 1.0 else -1.0)
# parte alta (derivada de la piel): mirar lejos de la piel; faldón torneado: mirar lejos del eje del torso
volteadas = orientar_por_superficie(bm, PIEL, lambda c: c.z >= 1.09)
volteadas += orientar_hacia_fuera(bm, lambda c: (V((0, 0, c.z)), V((0, 0, 1))) if c.z < 1.09 else (c, V((0, 0, 1))))
print('VOLTEADAS', 'OPM_Tunica', volteadas)
# aberturas laterales: se retira una franja estrecha en cada costado bajo SLIT
quitar_caras(bm, lambda c: c.z < SLIT and abs(c.y - 0.004) < 0.012 and abs(c.x) > 0.08)
bm_back(bm, tun)
material(tun, [M['tunica']])
add_solidify(tun, 0.004, offset=1.0, use_rim=True).use_rim_only = True
# cuello alzado (mao) con una pequeña abertura al frente
cue = copia_malla(base, 'OPM_Tunica_Cuello', ropa)
bm = bm_of(cue)
corte(bm, (0, 0, 1.385), (0, -0.15, 1)); corte(bm, (0, 0, 1.312), (0, 0, -1))
quitar_caras(bm, lambda c: abs(c.x) > 0.075)
quitar_caras(bm, lambda c: c.y < -0.02 and abs(c.x) < 0.010)
bm.normal_update()
for v in bm.verts: v.co += v.normal * (0.012 + 0.006 * max(0, v.co.z - 1.312) / 0.07)
smooth(bm, iters=3, fac=0.35)
bm_back(bm, cue); material(cue, [M['tunica']]); add_solidify(cue, 0.004, offset=1.0)

# capas interiores contenidas bajo la túnica
TUN = bvh_de(tun)
for o, pred in ((camiseta, lambda c: c.z < 1.24 and abs(c.x) < 0.17), (pan, lambda c: c.z > HEM + 0.02)):
    bm = bm_of(o); print('CONTENIDOS', o.name, contener(bm, TUN, 0.006, 0.04, pred)); bm_back(bm, o)
bpy.data.objects.remove(base)
cubre = bvh_de(*[o for o in ropa.objects if o.type == 'MESH'])
bm = bm_of(piel)
cubierto = {}
for v in bm.verts:
    if (v.co.z > 1.33 and abs(v.co.x) < 0.075) or v.co.z > 1.42 or abs(v.co.x) > 0.53 or v.co.z < CUFF + 0.01:   # cuello, manos y pies aparte; los hombros sí
        cubierto[v] = False; continue
    hit = cubre.ray_cast(v.co + v.normal * 0.0005, v.normal, 0.08)
    cubierto[v] = hit[0] is not None
cand = {f for f in bm.faces if all(cubierto[v] for v in f.verts)}
borde = {f for f in cand if any(g not in cand for v in f.verts for g in v.link_faces)}
bmesh.ops.delete(bm, geom=list(cand - borde), context='FACES')
bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
bm_back(bm, piel)
print('PIEL_OCULTA_BORRADA', len(cand - borde), 'caras; anillo conservado', len(borde))
rep = {}
for o in ropa.objects:
    dg = bpy.context.evaluated_depsgraph_get(); ev = o.evaluated_get(dg); m = ev.to_mesh(); m.calc_loop_triangles(); rep[o.name] = len(m.loop_triangles); ev.to_mesh_clear()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, '03_prendas.blend'))
print('OK03', rep)
