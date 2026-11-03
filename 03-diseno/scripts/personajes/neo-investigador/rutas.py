"""Rutas de la construcción del Neo Investigador.

Variables de entorno opcionales:
  NEO_VRM       VRM exportado de VRoid Studio (se conserva en la copia local de Chris).
  NEO_ANTERIOR  investigador.blend del que se reutilizan mochila, credencial, gafas, tableta y parche.
  NEO_TRABAJO   carpeta de archivos intermedios (por omisión, una carpeta temporal).
"""
import os, tempfile
AQUI = os.path.dirname(os.path.abspath(__file__))
DISENO = os.path.abspath(os.path.join(AQUI, '..', '..', '..'))          # 03-diseno/
VRM = os.environ.get('NEO_VRM', os.path.join(DISENO, 'local', 'investigador-vroid.vrm'))
ANTERIOR = os.environ.get('NEO_ANTERIOR', os.path.join(DISENO, 'fuentes', 'investigador.blend'))
W = os.environ.get('NEO_TRABAJO', os.path.join(tempfile.gettempdir(), 'neo-investigador'))
FUENTES = os.path.join(DISENO, 'fuentes')
EXPORTS = os.path.join(DISENO, 'exports')
VISTAS = os.path.join(DISENO, 'vistas', 'personajes', 'neo-investigador')
VALIDACION = os.path.join(DISENO, 'validacion', 'personajes')
for d in (W, os.path.join(W, 'out'), os.path.join(W, 'out2'), os.path.join(W, 'renders')):
    os.makedirs(d, exist_ok=True)
