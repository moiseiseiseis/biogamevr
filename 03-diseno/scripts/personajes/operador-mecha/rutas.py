"""Rutas de la construcción del Operador del Mecha.

Variables de entorno:
  OPM_VRM       VRM exportado de VRoid Studio (operador-mecha.vrm, copia local de Chris).
                Por omisión, 03-diseno/local/operador-mecha.vrm.
  OPM_TRABAJO   carpeta de archivos intermedios (por omisión, una carpeta temporal).
  OPM_PYTHON    Python 3 con Pillow para dibujar el emblema y el panel (por omisión, «python3» del PATH).
"""
import os, shutil, tempfile
AQUI = os.path.dirname(os.path.abspath(__file__))
DISENO = os.path.abspath(os.path.join(AQUI, '..', '..', '..'))          # 03-diseno/
NEO_SCRIPTS = os.path.join(AQUI, '..', 'neo-investigador')               # herramientas.py y render_vistas.py
VRM = os.environ.get('OPM_VRM', os.path.join(DISENO, 'local', 'operador-mecha.vrm'))
W = os.environ.get('OPM_TRABAJO', os.path.join(tempfile.gettempdir(), 'operador-mecha'))
FUENTES = os.path.join(DISENO, 'fuentes')
EXPORTS = os.path.join(DISENO, 'exports')
TEXTURAS = os.path.join(EXPORTS, 'texturas', 'operador-mecha')
VISTAS = os.path.join(DISENO, 'vistas', 'personajes', 'operador-mecha')
VALIDACION = os.path.join(DISENO, 'validacion', 'personajes')
for d in (W, os.path.join(W, 'renders')):
    os.makedirs(d, exist_ok=True)
PYTHON = os.environ.get('OPM_PYTHON') or shutil.which('python3') or 'python3'
