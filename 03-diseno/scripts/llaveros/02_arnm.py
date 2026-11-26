"""02 · Llavero con volumen del ARNm de la fase A: AUG – GGC – UUU – UAG.

Uso: blender -b --python 02_arnm.py

Sigue la idea de `arnm_corto_fase_a.blend` (esqueleto con una base por nucleótido y
los codones separados), pero construido en milímetros para imprimir: un esqueleto
cilíndrico vertical (5′ arriba, 3′ abajo) y una base redondeada por nucleótido con su
letra en relieve por delante y por detrás. Los tramos de inicio (AUG) y de paro (UAG)
llevan un collar más grueso con un triángulo y un octágono. Todo se une en un sólido
cerrado. Fuente tipográfica: variable LLAV_FUENTE (por omisión Arial Black de macOS).
"""
import bpy, bmesh, math, os, sys
from mathutils import Vector, Matrix
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import W
from medidas import ARGOLLA_R, ARGOLLA_TUBO, ARGOLLA_HUNDIDA, VOXEL

FUENTE = os.environ.get('LLAV_FUENTE', '/System/Library/Fonts/Supplemental/Arial Black.ttf')
CODONES = ['AUG', 'GGC', 'UUU', 'UAG']
R_ESQ = 3.2                    # radio del esqueleto
BASE_LARGO, BASE_ALTO, BASE_FONDO = 11.5, 3.9, 5.6
HUECO_BASE, HUECO_CODON, EXTREMO = 0.7, 2.4, 4.2
R_COLLAR = 4.3
LETRA, RELIEVE_LETRA = 3.4, 0.8
ENGROSAR_LETRA = 0.18           # ensancha cada trazo 0,36 mm (trazos de 0,9 mm o más)

for o in list(bpy.data.objects):
    bpy.data.objects.remove(o)
sc = bpy.context.scene
us = sc.unit_settings; us.system, us.scale_length, us.length_unit = 'METRIC', 0.001, 'MILLIMETERS'
fuente = bpy.data.fonts.load(FUENTE)
piezas = []


def malla(nombre, bm):
    me = bpy.data.meshes.new(nombre); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(nombre, me); sc.collection.objects.link(o); piezas.append(o)
    return o


def caja(nombre, x0, x1, z0, z1, fondo, bisel):
    bm = bmesh.new(); bmesh.ops.create_cube(bm, size=1)
    bmesh.ops.scale(bm, vec=(x1 - x0, fondo, z1 - z0), verts=bm.verts)
    bmesh.ops.translate(bm, vec=((x0 + x1) / 2, 0, (z0 + z1) / 2), verts=bm.verts)
    o = malla(nombre, bm)
    b = o.modifiers.new('bisel', 'BEVEL'); b.width = bisel; b.segments = 4; b.limit_method = 'NONE'
    return o


def cilindro(nombre, r, z0, z1, x=0.0):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=48, radius1=r, radius2=r, depth=z1 - z0)
    bmesh.ops.translate(bm, vec=(x, 0, (z0 + z1) / 2), verts=bm.verts)
    o = malla(nombre, bm)
    b = o.modifiers.new('bisel', 'BEVEL'); b.width = min(r * 0.45, 1.2); b.segments = 5
    b.limit_method = 'ANGLE'; b.angle_limit = math.radians(60)
    return o


def texto(nombre, cadena, x, z, cara, alto=LETRA, prof=RELIEVE_LETRA, y_sup=None):
    cu = bpy.data.curves.new(nombre, 'FONT'); cu.body = cadena; cu.font = fuente
    cu.size = alto * 1.38; cu.offset = ENGROSAR_LETRA; cu.align_x = 'CENTER'; cu.align_y = 'CENTER'; cu.extrude = prof / 2 + 0.3
    o = bpy.data.objects.new(nombre, cu); sc.collection.objects.link(o)
    # el texto se escribe en XY; se levanta al plano XZ mirando a -Y (frente) o a +Y (detrás)
    giro = Matrix.Rotation(math.pi / 2, 4, 'X') if cara < 0 else \
        Matrix.Rotation(math.pi, 4, 'Z') @ Matrix.Rotation(math.pi / 2, 4, 'X')
    o.matrix_world = Matrix.Translation((x, cara * y_sup, z)) @ giro
    bpy.context.view_layer.update()
    me = bpy.data.meshes.new_from_object(o.evaluated_get(bpy.context.evaluated_depsgraph_get()))
    me.transform(o.matrix_world); bpy.data.objects.remove(o)
    ob = bpy.data.objects.new(nombre, me); sc.collection.objects.link(ob); piezas.append(ob)


largo = 2 * EXTREMO + 12 * BASE_ALTO + 8 * HUECO_BASE + 3 * HUECO_CODON
cilindro('Esqueleto', R_ESQ, 0, largo)
z = largo - EXTREMO
for codon in CODONES:
    z_ini = z
    for k, letra in enumerate(codon):
        z_b = z - BASE_ALTO
        caja(f'Base_{codon}_{k}', R_ESQ * 0.5, R_ESQ + BASE_LARGO, z_b, z, BASE_FONDO, 1.1)
        cx = R_ESQ + BASE_LARGO * 0.55
        for cara in (-1, 1):
            texto(f'Letra_{codon}_{k}_{cara}', letra, cx, z_b + BASE_ALTO / 2, cara,
                  y_sup=BASE_FONDO / 2 + RELIEVE_LETRA / 2 - 0.3)
        z = z_b - (HUECO_BASE if k < 2 else 0)
    if codon in ('AUG', 'UAG'):
        cilindro(f'Collar_{codon}', R_COLLAR, z + 0.2, z_ini - 0.2)
        zc = (z + z_ini) / 2
        bm = bmesh.new()
        lados, rr = (3, 2.0) if codon == 'AUG' else (8, 1.9)
        bmesh.ops.create_circle(bm, cap_ends=True, segments=lados, radius=rr)
        bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0),
                         matrix=Matrix.Rotation(-math.pi / 2 if lados == 3 else math.pi / 8, 3, 'Z'))   # el triángulo apunta hacia 3′
        cara = bmesh.ops.extrude_face_region(bm, geom=bm.faces[:])
        bmesh.ops.translate(bm, vec=(0, 0, 1.4), verts=[v for v in cara['geom'] if isinstance(v, bmesh.types.BMVert)])
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
        # en el frente del collar, apuntando hacia la cámara (-Y)
        bmesh.ops.transform(bm, verts=bm.verts, matrix=Matrix.Translation((0, -R_COLLAR + 0.8, zc)) @
                            Matrix.Rotation(math.pi / 2, 4, 'X'))
        malla(f'Signo_{codon}', bm)
    z -= HUECO_CODON
texto('Extremo_5', '5′', 0, largo - EXTREMO / 2 - 0.5, -1, alto=3.0, prof=0.9, y_sup=R_ESQ + 0.1)
texto('Extremo_3', '3′', 0, EXTREMO / 2 + 0.7, -1, alto=3.0, prof=0.9, y_sup=R_ESQ + 0.1)

# argolla en el extremo 5′
bm = bmesh.new()
centro = Vector((0, 0, largo + ARGOLLA_R - ARGOLLA_HUNDIDA))
seg, lados = 64, 16
vs = []
for i in range(seg):
    a = 2 * math.pi * i / seg
    c = Vector((math.cos(a) * ARGOLLA_R, 0, math.sin(a) * ARGOLLA_R)); rad = Vector((math.cos(a), 0, math.sin(a)))
    vs.append([bm.verts.new(centro + c + rad * (math.cos(2 * math.pi * k / lados) * ARGOLLA_TUBO) +
                            Vector((0, math.sin(2 * math.pi * k / lados) * ARGOLLA_TUBO, 0))) for k in range(lados)])
for i in range(seg):
    for k in range(lados):
        bm.faces.new((vs[i][k], vs[(i + 1) % seg][k], vs[(i + 1) % seg][(k + 1) % lados], vs[i][(k + 1) % lados]))
malla('Argolla', bm)

# un solo sólido
for o in bpy.data.objects:
    o.select_set(o in piezas)
bpy.context.view_layer.objects.active = piezas[0]
bpy.ops.object.convert(target='MESH')          # aplica los biseles
bpy.ops.object.join()
obj = bpy.context.active_object; obj.name = obj.data.name = 'Llavero_Arnm_Aug_Uag'
rm = obj.modifiers.new('unir', 'REMESH'); rm.mode = 'VOXEL'; rm.voxel_size = VOXEL * 0.8
bpy.ops.object.modifier_apply(modifier=rm.name)
d = obj.modifiers.new('aligerar', 'DECIMATE'); d.ratio = min(1.0, 250_000 / max(1, len(obj.data.polygons) * 2))
bpy.ops.object.modifier_apply(modifier=d.name)
bb = [Vector(c) for c in obj.bound_box]
off = Vector(((bb[0].x + bb[6].x) / 2, (bb[0].y + bb[6].y) / 2, bb[0].z))
obj.data.transform(Matrix.Translation(-off))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, 'arnm-aug-uag.blend'), compress=True)
bpy.ops.wm.stl_export(filepath=os.path.join(W, 'llavero-arnm-aug-uag_bruto.stl'), export_selected_objects=True,
                      global_scale=1.0, apply_modifiers=True, ascii_format=False)
ext = obj.dimensions
print('OK02 arnm mm %.1f x %.1f x %.1f' % tuple(ext), 'caras', len(obj.data.polygons))
