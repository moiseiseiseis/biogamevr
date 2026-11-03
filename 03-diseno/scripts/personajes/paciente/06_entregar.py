"""06 · Copia fuentes, exportaciones, validación y vistas a 03-diseno/ (se ejecuta con python3, no con Blender)."""
import os, sys, shutil, json, tempfile
from PIL import Image
AQUI = os.path.dirname(os.path.abspath(__file__)); DISENO = os.path.abspath(os.path.join(AQUI, '..', '..', '..'))
W = os.environ.get('PAC_TRABAJO', os.path.join(tempfile.gettempdir(), 'paciente'))
VISTAS = os.path.join(DISENO, 'vistas', 'personajes', 'paciente'); os.makedirs(VISTAS, exist_ok=True)
for f in ('paciente.blend', 'camilla-soporte.blend', 'paciente-en-camilla.blend'):
    shutil.copy2(os.path.join(W, f), os.path.join(DISENO, 'fuentes', f))
for f in ('paciente.fbx', 'paciente.glb', 'camilla-soporte.fbx', 'camilla-soporte.glb'):
    shutil.copy2(os.path.join(W, f), os.path.join(DISENO, 'exports', f))
for f in ('paciente-geometria.json', 'camilla-geometria.json'):
    shutil.copy2(os.path.join(W, f), os.path.join(DISENO, 'validacion', 'personajes', f))
def hoja(names, out):
    ims = [Image.open(os.path.join(W, n)).convert('RGB') for n in names]
    h = max(i.height for i in ims); c = Image.new('RGB', (sum(i.width for i in ims), h), (196, 199, 204)); x = 0
    for i in ims: c.paste(i, (x, 0)); x += i.width
    c.save(os.path.join(VISTAS, out), optimize=True)
nombres = {'front': 'frente', 'side': 'lateral', 'back': 'posterior', 'tq': 'tres-cuartos'}
for v, n in nombres.items():
    Image.open(os.path.join(W, f'pac_{v}.png')).convert('RGB').save(os.path.join(VISTAS, f'paciente-{n}.png'), optimize=True)
hoja([f'pac_{v}.png' for v in nombres], 'paciente-vistas.png')
hoja(['pac-rostro_front.png', 'pac-rostro_tq.png'], 'paciente-rostro.png')
hoja(['pac-mascarilla_front.png', 'pac-mascarilla_tq.png'], 'paciente-mascarilla.png')
hoja(['pac-pose_front.png', 'pac-pose_tq.png', 'pac-pose_back.png'], 'paciente-pose-relajada.png')
for n in ('alto', 'rostro', 'lado'):
    Image.open(os.path.join(W, f'pac-camilla_{n}.png')).convert('RGB').save(os.path.join(VISTAS, f'paciente-en-camilla-{n}.png'), optimize=True)
hoja(['cam_tq.png', 'cam_side.png', 'cam_front.png'], 'camilla-soporte-vistas.png')
Image.open(os.path.join(W, 'cam_top.png')).convert('RGB').save(os.path.join(VISTAS, 'camilla-soporte-planta.png'), optimize=True)
print('OK06', sorted(os.listdir(VISTAS)))
