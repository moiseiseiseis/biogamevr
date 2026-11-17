"""Compone las hojas de vistas del Operador (se ejecuta con python3 + Pillow).
Uso: python3 componer_vistas.py <carpeta_renders> <carpeta_vistas>"""
import os, sys
from PIL import Image
R, V = sys.argv[1], sys.argv[2]; os.makedirs(V, exist_ok=True)
def hoja(prefix, vistas, salida):
    ims = [Image.open(os.path.join(R, f'{prefix}_{v}.png')).convert('RGB') for v in vistas]
    c = Image.new('RGB', (sum(i.width for i in ims), ims[0].height)); x = 0
    for i in ims: c.paste(i, (x, 0)); x += i.width
    c.save(os.path.join(V, salida), optimize=True)
nombres = {'front': 'frente', 'side': 'lateral', 'back': 'posterior', 'tq': 'tres-cuartos'}
for v, n in nombres.items():
    Image.open(os.path.join(R, f'opm_{v}.png')).convert('RGB').save(os.path.join(V, f'operador-mecha-{n}.png'), optimize=True)
hoja('opm', ('front', 'side', 'back', 'tq'), 'operador-mecha-vistas.png')
hoja('opm-rostro', ('front', 'tq'), 'operador-mecha-rostro.png')
hoja('opm-pose', ('front', 'tq', 'back'), 'operador-mecha-pose-relajada.png')
Image.open(os.path.join(R, 'opm-panel_tq.png')).convert('RGB').save(os.path.join(V, 'operador-mecha-panel.png'), optimize=True)
print('OK', sorted(os.listdir(V)))
