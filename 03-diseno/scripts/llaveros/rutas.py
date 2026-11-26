"""Rutas de los llaveros.

Variables de entorno:
  LLAV_TRABAJO  carpeta de archivos intermedios (por omisión, una carpeta temporal).
  LLAV_PYTHON   Python 3 con numpy, scipy y Pillow (por omisión, «python3» del PATH).
"""
import os, shutil, tempfile
AQUI = os.path.dirname(os.path.abspath(__file__))
DISENO = os.path.abspath(os.path.join(AQUI, '..', '..'))
FUENTES = os.path.join(DISENO, 'fuentes')
EXPORTS = os.path.join(DISENO, 'exports', 'llaveros')
VISTAS = os.path.join(DISENO, 'vistas', 'llaveros')
VALIDACION = os.path.join(DISENO, 'validacion', 'llaveros')
W = os.environ.get('LLAV_TRABAJO', os.path.join(tempfile.gettempdir(), 'llaveros'))
PYTHON = os.environ.get('LLAV_PYTHON') or shutil.which('python3') or 'python3'
for d in (W,):
    os.makedirs(d, exist_ok=True)
