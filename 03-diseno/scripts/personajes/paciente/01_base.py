"""01 · Base del Paciente: esqueleto, cara y cabello del Neo Investigador + piel reconstruida."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pac_util import *
NEO = os.environ['PAC_NEO']            # fuentes/neo-investigador.blend
VROID = os.environ['PAC_VROID_BLEND']  # investigador-vroid.blend (camisa sin recortar)
OUT = os.environ['PAC_TRABAJO']
bpy.ops.wm.open_mainfile(filepath=NEO)
sc = bpy.context.scene; sc.name = 'Paciente'
rig = bpy.data.objects['Neo_Investigador_Rig']; rig.name = 'Paciente_Rig'; rig.data.name = 'Paciente_Rig'
# camisa completa del investigador-vroid: su superficie es la piel desplazada 3,5 mm
with bpy.data.libraries.load(VROID) as (src, dst): dst.objects = ['INV_Camisa']
cam = dst.objects[0]; cam.name = 'PAC_Piel_Extra'; cam.data = cam.data.copy()
cam.parent = None
for o in list(bpy.data.objects):   # lo que arrastró la carga (su esqueleto, etc.)
    if o is not cam and not o.users_scene: bpy.data.objects.remove(o)
coll = collection('Paciente_Cuerpo')
coll.objects.link(cam)
for o in list(coll.objects): pass
for c in (rig.users_collection[0],):
    if rig.name not in coll.objects: coll.objects.link(rig)
# cuerpo VRoid a la colección del cuerpo
for n in ('VRoid_Cuerpo', 'VRoid_Cara', 'VRoid_Cabello'):
    o = bpy.data.objects[n]
    for c in list(o.users_collection): c.objects.unlink(o)
    coll.objects.link(o)
for c in list(rig.users_collection):
    if c != coll: c.objects.unlink(rig)
# antebrazos (desde poco antes del codo) y cuello/esternón
ELB = 0.372
keep_faces(cam, lambda c: abs(c.x) > ELB - 0.05 or (c.z > 1.42 and abs(c.x) < 0.12))
offset_normals(cam, -0.0035)
# manos completas a partir de los guantes (malla de la mano desplazada)
hands = []
for s in 'LR':
    g = bpy.data.objects['INV_Guante_' + s]
    h = g.copy(); h.data = g.data.copy(); coll.objects.link(h)
    for m in [m for m in h.modifiers if m.type != 'ARMATURE']: h.modifiers.remove(m)
    offset_normals(h, -0.003)
    hands.append(h)
# unir en una sola malla de piel
for o in bpy.context.selected_objects: o.select_set(False)
for o in [cam] + hands: o.select_set(True)
bpy.context.view_layer.objects.active = cam
bpy.ops.object.join()
piel = opaque_copy('N00_000_00_Body_00_SKIN (Instance)', 'PAC_Piel')
set_mats(cam, [piel])
for p in cam.data.polygons: p.use_smooth = True; p.material_index = 0
armature_child(cam, rig)
with bpy.context.temp_override(object=cam, active_object=cam, selected_objects=[cam]):
    bpy.ops.mesh.customdata_custom_splitnormals_clear()
out = piel.node_tree.nodes['Mtoon1Material.Mtoon1Output']
out.inputs['Shade Color'].default_value = (*srgb('#f1c6bb'), 1)
bpy.ops.wm.save_as_mainfile(filepath=OUT + '/01_base.blend')
print('OK01', [o.name for o in bpy.data.objects if o.type in ('MESH', 'ARMATURE')])
