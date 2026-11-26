"""01 · Convierte un modelo del juego en una figura de llavero con volumen, lista para imprimir.

Uso: blender -b --python 01_figura.py -- <operador-mecha|seraphim-mecha|astra>

Abre la fuente del juego sin modificarla y:
  1. coloca el modelo (pose, piezas que se quitan, orientación con la argolla arriba);
  2. lo escala en milímetros al tamaño del llavero;
  3. añade la argolla arriba al centro;
  4. reconstruye todo como un único sólido: reparte puntos sobre toda la superficie
     y los convierte en un campo de distancia con radio ENGROSAR. Así ninguna pieza
     queda más delgada que 2 × ENGROSAR, las capas de ropa se funden y la malla
     sale cerrada aunque el modelo original tenga superficies abiertas;
  5. quita las cavidades internas y aligera la malla.
Guarda <modelo>.blend y llavero-<modelo>_bruto.stl en la carpeta de trabajo.
"""
import bpy, bmesh, json, math, os, sys
from mathutils import Vector, Matrix, Quaternion
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import FUENTES, W
from medidas import *

MODELO = sys.argv[sys.argv.index('--') + 1]
FUENTE = {'operador-mecha': 'operador-mecha.blend', 'seraphim-mecha': 'seraphim-mecha.blend',
          'astra': 'astra-ia-de-viaje.blend'}[MODELO]
ALTO, ANCHO, ENG = TAMANO[MODELO]


def activar_vrm():
    import addon_utils
    for n in ('bl_ext.blender_org.vrm', 'io_scene_vrm'):
        try:
            addon_utils.enable(n, default_set=False); return
        except Exception:
            pass


activar_vrm()
bpy.ops.wm.open_mainfile(filepath=os.path.join(FUENTES, FUENTE))

# ---------------------------------------------------------------- 1. colocar
orient = Matrix.Identity(4)
if MODELO == 'operador-mecha':
    for o in bpy.data.objects:                      # el panel holográfico no va en el llavero
        if any(c.name == 'Operador_Props' for c in o.users_collection):
            o.hide_render = True
    rig = bpy.data.objects['Operador_Mecha_Rig']; pb = rig.pose.bones

    def girar(n, eje, grados):
        b = pb[n]; b.rotation_mode = 'QUATERNION'
        q = Quaternion(eje, math.radians(grados)); mw = rig.data.bones[n].matrix_local.to_quaternion()
        b.rotation_quaternion = mw.inverted() @ q @ mw
    for s, lado in ((1, 'L'), (-1, 'R')):           # brazos abajo, pegados a la cadera
        girar('J_Bip_%s_UpperArm' % lado, (0, 1, 0), 72 * s)
        girar('J_Bip_%s_LowerArm' % lado, (0, 1, 0), 4 * s)
    pb['J_Bip_C_Head'].scale = (CHIBI, CHIBI, CHIBI)  # proporción de juguete: cabeza más grande
elif MODELO == 'seraphim-mecha':
    bpy.data.objects['Cables'].hide_render = True    # cables finos: se romperían
    ala = bpy.data.objects['Panel_Luz']               # alas plegadas hacia atrás para estrechar la figura
    me = ala.data; mw = ala.matrix_world; mwi = mw.inverted()
    pts = [mw @ v.co for v in me.vertices]
    for lado in (1, -1):
        idx = [i for i, p in enumerate(pts) if p.x * lado > 0]
        raiz_x = min(abs(pts[i].x) for i in idx)
        raiz = [pts[i] for i in idx if abs(pts[i].x) < raiz_x + 0.6]
        piv = sum(raiz, Vector()) / len(raiz)
        rot = Matrix.Rotation(math.radians(ALAS) * lado, 4, 'Z')
        for i in idx:
            me.vertices[i].co = mwi @ (piv + rot @ (pts[i] - piv))
    me.update()
    sol = ala.modifiers.new('grosor_llavero', 'SOLIDIFY'); sol.thickness = GROSOR_ALAS; sol.offset = 0
    # la punta hacia arriba y el lomo hacia el frente
    orient = Matrix.Rotation(math.pi, 4, 'Z') @ Matrix.Rotation(-math.pi / 2, 4, 'X')
else:
    for n in ('ASTRA_Soporte_Brazo', 'ASTRA_Soporte_Cable', 'ASTRA_Conector', 'ASTRA_Conector_Placa'):
        bpy.data.objects[n].hide_render = True       # el brazo de soporte sería frágil; queda la esfera
bpy.context.view_layer.update()
dg = bpy.context.evaluated_depsgraph_get()

# ---------------------------------------------------------------- 2. una sola malla en milímetros
bm = bmesh.new()
for o in bpy.data.objects:
    if o.type != 'MESH' or o.hide_render or not o.visible_get():
        continue
    oe = o.evaluated_get(dg); m = oe.to_mesh()
    m.transform(orient @ oe.matrix_world)
    bm.from_mesh(m); oe.to_mesh_clear()
xs = [v.co.x for v in bm.verts]; ys = [v.co.y for v in bm.verts]; zs = [v.co.z for v in bm.verts]
esc = min(ALTO / (max(zs) - min(zs)), ANCHO / (max(xs) - min(xs)))
cx, cy = (max(xs) + min(xs)) / 2, (max(ys) + min(ys)) / 2
bmesh.ops.transform(bm, verts=bm.verts, matrix=Matrix.Scale(esc, 4) @ Matrix.Translation((-cx, -cy, -min(zs))))

# ---------------------------------------------------------------- 3. argolla
z_top = max(v.co.z for v in bm.verts)
alto_cerca = [v.co for v in bm.verts if v.co.z > z_top - 2.0]
ax = sum(p.x for p in alto_cerca) / len(alto_cerca); ay = sum(p.y for p in alto_cerca) / len(alto_cerca)
tubo = ARGOLLA_TUBO - ENG                            # el engrosado completa el grosor del aro
centro = Vector((ax, ay, z_top + ARGOLLA_R - ARGOLLA_HUNDIDA))
seg, lados = 64, 16
vs = []
for i in range(seg):
    a = 2 * math.pi * i / seg
    c = Vector((math.cos(a) * ARGOLLA_R, 0, math.sin(a) * ARGOLLA_R))
    rad = Vector((math.cos(a), 0, math.sin(a)))
    fila = []
    for k in range(lados):
        b = 2 * math.pi * k / lados
        p = c + rad * (math.cos(b) * tubo) + Vector((0, math.sin(b) * tubo, 0))
        fila.append(bm.verts.new(centro + p))
    vs.append(fila)
for i in range(seg):
    for k in range(lados):
        bm.faces.new((vs[i][k], vs[(i + 1) % seg][k], vs[(i + 1) % seg][(k + 1) % lados], vs[i][(k + 1) % lados]))

me = bpy.data.meshes.new(f'Llavero_{MODELO}_base'); bm.to_mesh(me); bm.free()
for o in list(bpy.data.objects):
    bpy.data.objects.remove(o)
for c in list(bpy.data.collections):
    bpy.data.collections.remove(c)
sc = bpy.context.scene
us = sc.unit_settings; us.system, us.scale_length, us.length_unit = 'METRIC', 0.001, 'MILLIMETERS'
obj = bpy.data.objects.new(f'Llavero_{MODELO.replace("-", "_").title()}', me); sc.collection.objects.link(obj)

# ---------------------------------------------------------------- 4. sólido único por campo de distancia
g = bpy.data.node_groups.new('Engrosar', 'GeometryNodeTree')
g.interface.new_socket('Geometry', in_out='INPUT', socket_type='NodeSocketGeometry')
g.interface.new_socket('Geometry', in_out='OUTPUT', socket_type='NodeSocketGeometry')
N = g.nodes; L = g.links
gi = N.new('NodeGroupInput'); go = N.new('NodeGroupOutput')
dp = N.new('GeometryNodeDistributePointsOnFaces'); dp.inputs['Density'].default_value = DENSIDAD
sdf = N.new('GeometryNodePointsToSDFGrid'); sdf.inputs['Radius'].default_value = ENG
sdf.inputs['Voxel Size'].default_value = VOXEL
suave = N.new('GeometryNodeSDFGridMean'); suave.inputs['Width'].default_value = 1
suave.inputs['Iterations'].default_value = SUAVIZAR
gm = N.new('GeometryNodeGridToMesh')
L.new(gi.outputs[0], dp.inputs['Mesh']); L.new(dp.outputs['Points'], sdf.inputs['Points'])
L.new(sdf.outputs[0], suave.inputs['Grid']); L.new(suave.outputs[0], gm.inputs['Grid']); L.new(gm.outputs[0], go.inputs[0])
md = obj.modifiers.new('engrosar', 'NODES'); md.node_group = g
bpy.context.view_layer.objects.active = obj; obj.select_set(True)
bpy.ops.object.modifier_apply(modifier=md.name)

# ---------------------------------------------------------------- 5. sin cavidades internas, más ligera
bm = bmesh.new(); bm.from_mesh(obj.data)
partes, vistos = [], set()
for f in bm.faces:
    if f.index in vistos:
        continue
    pila, grupo = [f], []
    vistos.add(f.index)
    while pila:
        x = pila.pop(); grupo.append(x)
        for e in x.edges:
            for y in e.link_faces:
                if y.index not in vistos:
                    vistos.add(y.index); pila.append(y)
    vol = sum(t.verts[0].co.dot(t.verts[1].co.cross(t.verts[2].co)) for t in grupo if len(t.verts) == 3) / 6
    vol += sum(sum(q.verts[0].co.dot(q.verts[i].co.cross(q.verts[i + 1].co)) for i in range(1, len(q.verts) - 1))
               for q in grupo if len(q.verts) > 3) / 6
    partes.append((vol, grupo))
borrar = [f for vol, grupo in partes if vol <= 0 or vol < 1.0 for f in grupo]   # cavidades y migajas sueltas
bmesh.ops.delete(bm, geom=borrar, context='FACES')
bm.to_mesh(obj.data); bm.free()
n0 = len(obj.data.polygons)
tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
if tris > TRIANGULOS:
    d = obj.modifiers.new('aligerar', 'DECIMATE'); d.ratio = TRIANGULOS / tris
    bpy.ops.object.modifier_apply(modifier=d.name)
z0 = min(v.co.z for v in obj.data.vertices)
for v in obj.data.vertices:
    v.co.z -= z0
obj.data.update()

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(W, f'{MODELO}.blend'), compress=True)
bpy.ops.wm.stl_export(filepath=os.path.join(W, f'llavero-{MODELO}_bruto.stl'), export_selected_objects=True,
                      global_scale=1.0, apply_modifiers=True, ascii_format=False)
bb = [Vector(c) for c in obj.bound_box]
ext = [max(v[i] for v in bb) - min(v[i] for v in bb) for i in range(3)]
print('OK01', MODELO, 'mm %.1f x %.1f x %.1f' % tuple(ext), 'partes', len(partes), 'borradas',
      sum(1 for v, _ in partes if v <= 0 or v < 1.0), 'caras', len(obj.data.polygons))
