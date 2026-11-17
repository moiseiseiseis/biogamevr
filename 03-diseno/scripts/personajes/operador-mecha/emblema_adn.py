"""Emblema del parche del Operador del Mecha: doble hélice de ADN en un disco.
Uso: python3 emblema_adn.py <carpeta_salida>  (escribe emblema-adn_512.png y emblema-adn_256.png)"""
import math, sys
from PIL import Image, ImageDraw, ImageFilter
OUT = sys.argv[1]
S = 2048                       # se dibuja grande y se reduce (antialias)
C = S / 2
def hx(h): h = h.lstrip('#'); return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))
NAVY, NAVY2 = hx('#1f2b3a'), hx('#2a3a4d')
ORANGE, CHAR = hx('#ce773c'), hx('#3d3c3b')
CYAN, LIGHT, GRAY = hx('#5fd0c4'), hx('#dcd6cc'), hx('#847d75')
im = Image.new('RGBA', (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
R = S * 0.48
d.ellipse([C - R, C - R, C + R, C + R], fill=CHAR)                       # borde exterior (bordado)
r1 = R * 0.93; d.ellipse([C - r1, C - r1, C + r1, C + r1], fill=ORANGE)  # anillo naranja
r2 = R * 0.84; d.ellipse([C - r2, C - r2, C + r2, C + r2], fill=NAVY)    # campo
r3 = R * 0.60; d.ellipse([C - r3, C - r3, C + r3, C + r3], fill=NAVY2)   # halo interior
# puntadas del borde
for k in range(72):
    a = 2 * math.pi * k / 72; rr = R * 0.965
    x, y = C + rr * math.cos(a), C + rr * math.sin(a)
    d.ellipse([x - S*0.004, y - S*0.004, x + S*0.004, y + S*0.004], fill=GRAY)
# doble hélice en diagonal
ang = math.radians(-40); ca, sa = math.cos(ang), math.sin(ang)
L, A, turns = R * 1.30, R * 0.20, 2.0
def P(t, ph):
    u = (t - 0.5) * L; v = A * math.sin(2 * math.pi * turns * t + ph)
    return (C + u * ca - v * sa, C + u * sa + v * ca), math.cos(2 * math.pi * turns * t + ph)
N = 400; W = S * 0.030
segs = []
for ph, col in ((0.0, CYAN), (math.pi, LIGHT)):
    for i in range(N):
        (p0, z0), (p1, z1) = P(i / N, ph), P((i + 1) / N, ph)
        segs.append(((z0 + z1) / 2, 'seg', p0, p1, col))
for k in range(1, 15):                                                  # peldaños (pares de bases)
    t = k / 15; (pa, za), (pb, zb) = P(t, 0.0), P(t, math.pi)
    segs.append((-1.5, 'rung', pa, pb, GRAY if k % 2 else ORANGE))
segs.sort(key=lambda s: s[0])                                            # lo de atrás primero
for z, kind, p0, p1, col in segs:
    if kind == 'rung':
        d.line([p0, p1], fill=col, width=int(S * 0.012))
    else:
        shade = 0.55 + 0.45 * (z + 1) / 2
        c = tuple(int(x * shade) for x in col)
        w = W * (0.75 + 0.25 * (z + 1) / 2)
        d.line([p0, p1], fill=c, width=int(w)); d.ellipse([p1[0]-w/2, p1[1]-w/2, p1[0]+w/2, p1[1]+w/2], fill=c)
# recorte al campo para que la hélice no pise el anillo
mask = Image.new('L', (S, S), 0); ImageDraw.Draw(mask).ellipse([C - r2, C - r2, C + r2, C + r2], fill=255)
base = Image.new('RGBA', (S, S), (0, 0, 0, 0)); bd = ImageDraw.Draw(base)
bd.ellipse([C - R, C - R, C + R, C + R], fill=CHAR); bd.ellipse([C - r1, C - r1, C + r1, C + r1], fill=ORANGE)
for k in range(72):
    a = 2 * math.pi * k / 72; rr = R * 0.965
    x, y = C + rr * math.cos(a), C + rr * math.sin(a)
    bd.ellipse([x - S*0.004, y - S*0.004, x + S*0.004, y + S*0.004], fill=GRAY)
base.paste(im, (0, 0), mask)
for size in (512, 256):
    base.resize((size, size), Image.LANCZOS).save(f'{OUT}/emblema-adn_{size}.png')
prev = Image.new('RGB', (1100, 560), (196, 200, 206)); e = base.resize((512, 512), Image.LANCZOS)
prev.paste(e, (24, 24), e); small = base.resize((128, 128), Image.LANCZOS); prev.paste(small, (640, 216), small)
small2 = base.resize((64, 64), Image.LANCZOS); prev.paste(small2, (850, 248), small2)
prev.save(f'{OUT}/emblema-adn_preview.png')
print('ok')
