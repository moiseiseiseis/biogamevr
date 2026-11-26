"""Medidas de los llaveros (milímetros). Impresora: Bambu Lab A1 mini (FDM, boquilla de 0,4 mm,
volumen de 180 × 180 × 180 mm). Ningún detalle debe bajar de 0,8 mm (dos pasadas de boquilla)."""
# Figura sin argolla: alto, ancho máximo y engrosado (radio). Cada pieza queda con al menos 2 × engrosado.
TAMANO = {
    'operador-mecha': (62.0, 36.0, 0.55),
    'seraphim-mecha': (60.0, 44.0, 0.60),
    'astra': (36.0, 36.0, 0.50),
}
CHIBI = 1.30            # escala de la cabeza de la Operadora
ALAS = 46               # grados que se pliegan las alas del Seraphim
ARGOLLA_R = 3.2         # radio del aro (centro del tubo)
ARGOLLA_TUBO = 1.3      # radio del tubo del aro: orificio de Ø 3,8 mm, aro de 2,6 mm de grosor
ARGOLLA_HUNDIDA = 1.6   # cuánto entra el aro en la figura
DENSIDAD = 30.0         # puntos por mm² para reconstruir la superficie
VOXEL = 0.08            # resolución del campo de distancia
TRIANGULOS = 250_000    # máximo por llavero
SUAVIZAR = 2           # pasadas de suavizado del campo de distancia (quita la textura de los puntos)
GROSOR_ALAS = 0.16     # grosor extra de las alas del Seraphim (unidades del modelo, ≈ 0,9 mm)
