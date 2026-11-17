"""Textura del panel holográfico del Operador: interfaz cian sobre transparente (1024 px).
Uso: python3 panel_holografico_tex.py <carpeta_salida>  (escribe panel-holografico_1024.png)"""
import math, sys
from PIL import Image, ImageDraw
OUT = sys.argv[1]; S = 2048
im = Image.new('RGBA', (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
CY = (120, 240, 225)
def c(a): return CY + (a,)
def panel(x0, y0, x1, y1, main):
    d.rectangle([x0, y0, x1, y1], fill=(18, 92, 96, 70))                       # velo teal oscuro
    w = int(S * 0.006); d.rectangle([x0, y0, x1, y1], outline=c(235), width=w)  # marco
    k = (x1 - x0) * 0.07                                                        # esquinas reforzadas
    for (px, py, sx, sy) in ((x0, y0, 1, 1), (x1, y0, -1, 1), (x0, y1, 1, -1), (x1, y1, -1, -1)):
        d.line([(px, py + sy * k), (px, py), (px + sx * k, py)], fill=c(255), width=w * 3)
    gx = (x1 - x0) / 16
    for i in range(1, 16):                                                      # rejilla tenue
        d.line([(x0 + i * gx, y0), (x0 + i * gx, y1)], fill=(40, 150, 150, 60), width=2)
    for j in range(1, int((y1 - y0) / gx)):
        d.line([(x0, y0 + j * gx), (x1, y0 + j * gx)], fill=(40, 150, 150, 60), width=2)
    if main:
        cx, cy, r = x0 + (x1 - x0) * 0.36, (y0 + y1) / 2, (y1 - y0) * 0.30     # medidor circular
        d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=c(220), width=w * 2)
        r2 = r * 0.72; d.ellipse([cx - r2, cy - r2, cx + r2, cy + r2], outline=c(150), width=w)
        for a0 in range(-90, 200, 30):
            d.arc([cx - r * 1.15, cy - r * 1.15, cx + r * 1.15, cy + r * 1.15], a0, a0 + 18, fill=c(240), width=w * 3)
        d.ellipse([cx - r * .18, cy - r * .18, cx + r * .18, cy + r * .18], fill=c(200))
        d.line([(cx, cy), (cx + r * .9 * math.cos(-0.7), cy + r * .9 * math.sin(-0.7))], fill=c(255), width=w * 2)
        bx = x0 + (x1 - x0) * 0.66                                              # barras
        for i, v in enumerate((0.85, 0.55, 0.7, 0.35, 0.9)):
            yy = y0 + (y1 - y0) * (0.22 + i * 0.13)
            d.rectangle([bx, yy, bx + (x1 - x0) * 0.26, yy + (y1 - y0) * 0.05], outline=c(160), width=w)
            d.rectangle([bx, yy, bx + (x1 - x0) * 0.26 * v, yy + (y1 - y0) * 0.05], fill=c(200))
        d.line([(x0 + (x1-x0)*0.06, y0 + (y1-y0)*0.1), (x0 + (x1-x0)*0.5, y0 + (y1-y0)*0.1)], fill=c(230), width=w * 2)
    else:
        pts = [(x0 + (x1 - x0) * (0.08 + 0.84 * t / 40), (y0 + y1) / 2 + (y1 - y0) * 0.22 * math.sin(t * 0.7) * math.exp(-((t - 20) / 12) ** 2)) for t in range(41)]
        d.line(pts, fill=c(245), width=w * 2)                                   # trazo de señal
        d.line([(x0 + (x1-x0)*0.08, y0 + (y1-y0)*0.18), (x0 + (x1-x0)*0.45, y0 + (y1-y0)*0.18)], fill=c(230), width=w * 2)
panel(S * 0.02, S * 0.02, S * 0.98, S * 0.64, True)        # panel grande: UV v 0.36–0.98
panel(S * 0.02, S * 0.68, S * 0.48, S * 0.98, False)       # panel pequeño: UV v 0.02–0.32
im.resize((1024, 1024), Image.LANCZOS).save(OUT + '/panel-holografico_1024.png')
print('ok')
