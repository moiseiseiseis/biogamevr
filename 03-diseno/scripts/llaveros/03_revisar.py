"""03 · Reúne los cuatro llaveros, los comprueba, exporta los STL y renderiza las vistas.

Uso: blender -b --python 03_revisar.py

Comprueba en cada malla: que esté cerrada (ninguna arista abierta ni compartida por
más de dos caras), que tenga volumen positivo, que apoye en Z = 0 y que quepa en la
A1 mini (180 mm por lado). Mide el grosor mínimo lanzando rayos hacia dentro desde
cada vértice. Exporta:
  exports/llaveros/llavero-<nombre>.stl     milímetros, de pie (Z arriba)
  fuentes/llaveros.blend                    las cuatro piezas
  vistas/llaveros/*.png                     vistas de cada llavero y hoja conjunta
  validacion/llaveros/llaveros-verificacion.json
"""
import bpy, bmesh, json, math, os, sys
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import W, FUENTES, EXPORTS, VISTAS, VALIDACION

LLAVEROS = ['operador-mecha', 'seraphim-mecha', 'astra', 'arnm-aug-uag']
TITULO = {'operador-mecha': 'Operadora del Mecha', 'seraphim-mecha': 'Seraphim Mecha',
          'astra': 'ASTRA', 'arnm-aug-uag': 'ARNm AUG–GGC–UUU–UAG'}
for d in (EXPORTS, VISTAS, VALIDACION):
    os.makedirs(d, exist_ok=True)

bpy.ops.wm.read_homefile(use_empty=True)
sc = bpy.context.scene
us = sc.unit_settings; us.system, us.scale_length, us.length_unit = 'METRIC', 0.001, 'MILLIMETERS'
mat = bpy.data.materials.new('Llavero_PLA'); mat.use_nodes = True
bsdf = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
bsdf.inputs['Base Color'].default_value = (0.55, 0.60, 0.68, 1); bsdf.inputs['Roughness'].default_value = 0.45


def grosor_minimo(bm, muestras=20000):
    """Grosor de pared: rayo hacia dentro desde cada vértice hasta la cara opuesta (normal contraria).
    Devuelve el mínimo y el percentil 1 (las esquinas cóncavas no cuentan como pared)."""
    bvh = BVHTree.FromBMesh(bm)
    vs = list(bm.verts); paso = max(1, len(vs) // muestras); g = []
    for v in vs[::paso]:
        n = v.normal
        if n.length < 0.5:
            continue
        loc, nh, i, d = bvh.ray_cast(v.co - n * 1e-3, -n, 50)
        if loc is not None and nh.dot(n) < -0.7:
            g.append(d + 1e-3)
    g.sort()
    return g[0], g[len(g) // 100]


informe = {}
objetos = []
for nombre in LLAVEROS:
    bpy.ops.wm.stl_import(filepath=os.path.join(W, f'llavero-{nombre}_bruto.stl'))
    o = bpy.context.selected_objects[0]
    o.name = o.data.name = 'Llavero_' + nombre.replace('-', '_').title()
    o.data.materials.append(mat)
    bm = bmesh.new(); bm.from_mesh(o.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bm.to_mesh(o.data)
    bm.verts.ensure_lookup_table(); bm.normal_update()
    abiertas = sum(1 for e in bm.edges if not e.is_manifold)
    vol = bm.calc_volume(signed=True)
    gmin = grosor_minimo(bm)
    bm.free()
    for p in o.data.polygons:
        p.use_smooth = True
    ext = o.dimensions
    z0 = min((o.matrix_world @ v.co).z for v in o.data.vertices)
    informe[nombre] = {'titulo': TITULO[nombre], 'archivo': f'exports/llaveros/llavero-{nombre}.stl',
                       'ancho_mm': round(ext.x, 1), 'fondo_mm': round(ext.y, 1), 'alto_mm': round(ext.z, 1),
                       'triangulos': len(o.data.polygons), 'aristas_abiertas_o_repetidas': abiertas,
                       'volumen_cm3': round(vol / 1000, 2), 'z_min_mm': round(z0, 3),
                       'grosor_pared_min_mm': round(gmin[0], 2), 'grosor_pared_p1_mm': round(gmin[1], 2), 'cabe_en_a1_mini': max(ext) <= 180}
    for s in bpy.context.selected_objects:
        s.select_set(False)
    o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.wm.stl_export(filepath=os.path.join(EXPORTS, f'llavero-{nombre}.stl'), export_selected_objects=True,
                          global_scale=1.0, apply_modifiers=True, ascii_format=False)
    objetos.append(o)
    print('OK03', nombre, informe[nombre])
json.dump(informe, open(os.path.join(VALIDACION, 'llaveros-verificacion.json'), 'w', encoding='utf-8'),
          indent=2, ensure_ascii=False)

# ------------------------------------------------------------ escena y vistas
x = 0.0
for o in objetos:
    o.location.x = x - min(v.co.x for v in o.data.vertices)
    x += o.dimensions.x + 10
for o in objetos:
    o.location.x -= x / 2
w = bpy.data.worlds.new('fondo'); sc.world = w; w.use_nodes = True
bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND')
bg.inputs[0].default_value = (0.80, 0.83, 0.88, 1); bg.inputs[1].default_value = 0.35
for motor in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT'):
    try:
        sc.render.engine = motor; break
    except TypeError:
        pass
sc.view_settings.view_transform = 'Standard'
for nombre, energia, rot in (('clave', 2.2, (50, 0, -35)), ('relleno', 0.6, (60, 0, 140))):
    l = bpy.data.objects.new(nombre, bpy.data.lights.new(nombre, 'SUN')); sc.collection.objects.link(l)
    l.data.energy = energia; l.rotation_euler = [math.radians(a) for a in rot]
cd = bpy.data.cameras.new('cam'); cd.type = 'ORTHO'; cd.clip_end = 3000
cam = bpy.data.objects.new('cam', cd); sc.collection.objects.link(cam); sc.camera = cam
r = sc.render


def foto(objs, ruta, giro_z, px):
    for o in objetos:
        o.hide_render = o not in objs
    pts = [o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    c = sum(pts, Vector()) / len(pts)
    rot = Matrix.Rotation(math.radians(giro_z), 4, 'Z') @ Matrix.Rotation(math.radians(84), 4, 'X')
    loc = [rot.inverted() @ (p - c) for p in pts]
    a = max(p.x for p in loc) - min(p.x for p in loc); h = max(p.y for p in loc) - min(p.y for p in loc)
    cd.ortho_scale = max(a, h) * 1.15
    r.resolution_x = px if a >= h else max(400, int(px * a / h * 1.15))
    r.resolution_y = px if h > a else max(400, int(px * h / a * 1.15))
    cam.matrix_world = Matrix.Translation(c) @ rot @ Matrix.Translation((0, 0, 500))
    r.filepath = ruta
    bpy.ops.render.render(write_still=True)


foto(objetos, os.path.join(W, 'calentar.png'), 0, 200)   # el primer render de EEVEE sale vacío
for o, nombre in zip(objetos, LLAVEROS):
    for etiqueta, giro in (('frente', 0), ('tres-cuartos', 35), ('lateral', 90), ('posterior', 180)):
        foto([o], os.path.join(VISTAS, f'llavero-{nombre}-{etiqueta}.png'), giro, 1000)
foto(objetos, os.path.join(VISTAS, 'llaveros.png'), 20, 2400)
for o in objetos:
    o.hide_render = False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(FUENTES, 'llaveros.blend'), compress=True)
print('OK03 fin')
