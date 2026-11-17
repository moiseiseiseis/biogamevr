"""12 · Copia fuente, exportaciones, texturas, validación y vistas a 03-diseno/ (se ejecuta con python3 + Pillow).
Las vistas PNG se optimizan sin pérdida con oxipng y se comprueba que los píxeles no cambian."""
import os, sys, shutil, subprocess, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
from PIL import Image, ImageChops
OUT = os.path.join(W, 'out'); VW = os.path.join(W, 'vistas')
for d in (FUENTES, EXPORTS, TEXTURAS, VISTAS, VALIDACION): os.makedirs(d, exist_ok=True)
def copiar(src, dst):
    shutil.copyfile(src, dst); os.chmod(dst, 0o644)              # sin bit de ejecución y sin metadatos de macOS
copiar(os.path.join(OUT, 'operador-mecha.blend'), os.path.join(FUENTES, 'operador-mecha.blend'))
for f in ('operador-mecha.fbx', 'operador-mecha.glb', 'operador-mecha-panel.fbx', 'operador-mecha-panel.glb'):
    copiar(os.path.join(OUT, f), os.path.join(EXPORTS, f))
for f in sorted(os.listdir(OUT)):
    if f.endswith('.png'): copiar(os.path.join(OUT, f), os.path.join(TEXTURAS, f))
copiar(os.path.join(OUT, 'operador-mecha-verificacion.json'), os.path.join(VALIDACION, 'operador-mecha-verificacion.json'))
copiar(os.path.join(OUT, 'operador-mecha-geometria.json'), os.path.join(VALIDACION, 'operador-mecha-geometria.json'))
rep = {}
for f in sorted(os.listdir(VW)):
    dst = os.path.join(VISTAS, f); copiar(os.path.join(VW, f), dst)
    antes = os.path.getsize(dst); a = Image.open(dst).convert('RGBA'); a.load()
    subprocess.run(['oxipng', '-o', '4', '--strip', 'safe', '-q', dst], check=True)
    b = Image.open(dst).convert('RGBA'); igual = ImageChops.difference(a, b).getbbox() is None
    assert igual, dst
    rep[f] = (antes, os.path.getsize(dst))
print('OK12 vistas', {k: f'{v[0] // 1024}→{v[1] // 1024} KB' for k, v in rep.items()})
