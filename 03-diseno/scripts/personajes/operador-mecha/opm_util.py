"""Utilidades del Operador del Mecha (reutiliza herramientas.py del Neo Investigador)."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
sys.path.insert(0, NEO_SCRIPTS)
import bpy, bmesh, math, mathutils
from mathutils import Vector as V
from herramientas import enable, mtoon, srgb, smooth, add_solidify, rigid_weight, push_out
from mathutils.bvhtree import BVHTree

def bm_of(ob):
    bm = bmesh.new(); bm.from_mesh(ob.data); bm.normal_update(); return bm

def bm_back(bm, ob):
    bm.normal_update(); bm.to_mesh(ob.data); bm.free(); ob.data.update()

def corte(bm, co, no):
    """Corte recto con bisect_plane: elimina lo que queda del lado hacia donde apunta `no`."""
    geom = list(bm.verts) + list(bm.edges) + list(bm.faces)
    bmesh.ops.bisect_plane(bm, geom=geom, dist=1e-6, plane_co=V(co), plane_no=V(no).normalized(), clear_outer=True)
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
    bmesh.ops.dissolve_degenerate(bm, dist=1e-5, edges=bm.edges)

def quitar_caras(bm, pred):
    bmesh.ops.delete(bm, geom=[f for f in bm.faces if pred(f.calc_center_median())], context='FACES')
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')

def copia_malla(src, nombre, coll=None):
    o = src.copy(); o.data = src.data.copy(); o.name = o.data.name = nombre
    for c in (coll and [coll]) or src.users_collection: c.objects.link(o)
    for m in [m for m in o.modifiers if m.type != 'ARMATURE']: o.modifiers.remove(m)
    if o.data.shape_keys: o.shape_key_clear()
    return o

def coleccion(nombre):
    c = bpy.data.collections.get(nombre) or bpy.data.collections.new(nombre)
    if c.name not in bpy.context.scene.collection.children: bpy.context.scene.collection.children.link(c)
    return c

def mover(o, coll):
    for c in list(o.users_collection): c.objects.unlink(o)
    coll.objects.link(o)

def material(ob, mats):
    ob.data.materials.clear()
    for m in mats: ob.data.materials.append(m)
    for p in ob.data.polygons: p.use_smooth = True

def eje_pierna(x, z):
    """Punto del eje de la pierna (cadera 0,868 → rodilla 0,515 → tobillo 0,101) a la altura z."""
    s = 1 if x > 0 else -1
    if z > 0.515: t = (0.868 - z) / 0.353; y = 0.0 + t * 0.007
    else: t = (0.515 - z) / 0.414; y = 0.007 + t * 0.028
    return V((0.077 * s, y, z))

def bvh_de(*obs):
    """BVH en reposo de una o varias mallas (sin modificadores)."""
    vs, fs = [], []
    for o in obs:
        n = len(vs); vs += [v.co.copy() for v in o.data.vertices]
        fs += [tuple(n + i for i in p.vertices) for p in o.data.polygons]
    return BVHTree.FromPolygons(vs, fs)

def empujar(bm, bvh, dmin, iters=6, tope_total=0.02):
    """Distancia mínima a una superficie empujando cada vértice por SU normal (no al punto más cercano,
    que pliega caras al revés y deja huecos con MToon/URP, que no dibujan la cara trasera)."""
    acumulado = {}
    for _ in range(iters):
        bm.normal_update(); movidos = 0
        for v in bm.verts:
            d = dmin(v.co) if callable(dmin) else dmin
            if d is None or d < 0: continue
            loc, nor, _, _ = bvh.find_nearest(v.co)
            if loc is None: continue
            sd = (v.co - loc).dot(nor)
            if sd < -0.03: continue                      # superficie equivocada (otra pierna, otra capa)
            falta = min(d - sd, 0.015, tope_total - acumulado.get(v, 0.0))   # tope por pasada y en total
            if falta > 1e-5:
                v.co += v.normal * falta; movidos += 1; acumulado[v] = acumulado.get(v, 0.0) + falta
        if not movidos: break

def caras_invertidas(bm, bvh):
    """Caras cuya normal apunta hacia la superficie de referencia (hacia dentro)."""
    malas = []
    for f in bm.faces:
        c = f.calc_center_median(); loc, nor, _, _ = bvh.find_nearest(c)
        if loc is not None and f.normal.dot(c - loc) < 0: malas.append(f)
    return malas

def torno(bm, aristas, pasos, eje, radio, extremo_salida=False, salto_max=None):
    """Extruye anillos a lo largo de un eje (como un torno).
    aristas: lazo de borde inicial. pasos: lista de parámetros t. eje(t) -> (punto, dirección).
    radio(t, dir) -> distancia desde el eje. Cada vértice conserva su ángulo alrededor del eje,
    así que no se pliegan caras. Devuelve las aristas del último anillo."""
    def proyecta(co, t):
        P, Dd = eje(t); d = co - P; d -= Dd * d.dot(Dd)
        return d.normalized() if d.length > 1e-6 else None
    for t_prev, t in zip([None] + pasos[:-1], pasos):
        r = bmesh.ops.extrude_edge_only(bm, edges=aristas)
        nuevos = [g for g in r['geom'] if isinstance(g, bmesh.types.BMVert)]
        for v in nuevos:
            tp = t_prev if t_prev is not None else t
            d = proyecta(v.co, tp)
            if d is None: continue
            P, Dd = eje(t); v.co = P + d * radio(t, d)
        aristas = [g for g in r['geom'] if isinstance(g, bmesh.types.BMEdge) and g.is_boundary
                   and all(any(v is n for n in nuevos) for v in g.verts)]
        if salto_max:
            # filtro dentro del anillo: un vértice que sobresale de sus vecinos vuelve a su promedio
            P, Dd = eje(t)
            def rad(v):
                d = v.co - P; d -= Dd * d.dot(Dd); return d
            vec = {v: [e.other_vert(v) for e in v.link_edges if e in aristas] for v in nuevos}
            for v, nb in vec.items():
                if len(nb) < 2: continue
                rv = rad(v); rn = sum(rad(n).length for n in nb) / len(nb)
                if rv.length - rn > salto_max: v.co = P + rv.normalized() * rn + Dd * (v.co - P).dot(Dd)
    return aristas

def radio_desde_eje(bvh, P, d, defecto):
    """Distancia desde un punto del eje (dentro del miembro) hasta la superficie, hacia fuera."""
    hit = bvh.ray_cast(P, d, 0.5)
    return (hit[0] - P).length if hit[0] is not None else defecto

def radio_exterior(bvh, P, d, desde=0.45, defecto=0.0):
    """Distancia al punto más exterior de las superficies en esa dirección (rayo de fuera hacia dentro)."""
    o = P + d * desde; hit = bvh.ray_cast(o, -d, desde)
    return (hit[0] - P).length if hit[0] is not None else defecto

def bordes_bajo(bm, z):
    return [e for e in bm.edges if e.is_boundary and all(v.co.z < z for v in e.verts)]

def lazos_borde(bm):
    """Lazos de borde por conectividad: lista de listas de aristas."""
    from herramientas import boundary_loops
    return boundary_loops(bm)

def tubo(bm, pasos, eje, radio, lados=12, tapa_arriba=False):
    """Tubo nuevo de anillos alrededor de un eje: eje(t) -> (punto, dirección); radio(t, dir) -> r."""
    anillos = []
    for t in pasos:
        P, Dd = eje(t)
        a = V((1, 0, 0)) if abs(Dd.x) < 0.9 else V((0, 0, 1))
        u = (a - Dd * a.dot(Dd)).normalized(); w = Dd.cross(u)
        fila = []
        for k in range(lados):
            ang = 2 * math.pi * k / lados
            d = (u * math.cos(ang) + w * math.sin(ang)).normalized()
            fila.append(bm.verts.new(P + d * radio(t, d)))
        anillos.append(fila)
    for f0, f1 in zip(anillos[:-1], anillos[1:]):
        for k in range(lados):
            bm.faces.new((f0[k], f0[(k + 1) % lados], f1[(k + 1) % lados], f1[k]))
    return anillos

def orientar_hacia_fuera(bm, eje_de):
    """Da la vuelta a las caras cuya normal apunta hacia el eje local (eje_de(centro) -> (punto, dir))."""
    n = 0
    for f in bm.faces:
        c = f.calc_center_median(); P, Dd = eje_de(c)
        r = c - P; r -= Dd * r.dot(Dd)
        if r.length > 1e-4 and f.normal.dot(r) < 0:
            f.normal_flip(); n += 1
    bm.normal_update(); return n

def densificar_borde(bm, pred, largo_max=0.02):
    """Subdivide las aristas de borde que cumplen pred(arista) hasta que ninguna mida más de largo_max.
    Devuelve esas aristas de borde (se vuelven a elegir por posición tras cada subdivisión)."""
    for _ in range(6):
        largas = [e for e in bm.edges if e.is_boundary and pred(e) and e.calc_length() > largo_max]
        if not largas: break
        bmesh.ops.subdivide_edges(bm, edges=largas, cuts=1, use_grid_fill=False)
    return [e for e in bm.edges if e.is_boundary and pred(e)]

def contener(bm, bvh_exterior, margen=0.006, alcance=0.04, pred=None):
    """Mantiene una capa interior por debajo de la exterior: si un vértice queda a menos de `margen`
    por dentro (o fuera) de la superficie exterior cercana, se lleva a `margen` por dentro."""
    n = 0
    for v in bm.verts:
        if pred and not pred(v.co): continue
        loc, nor, _, dist = bvh_exterior.find_nearest(v.co)
        if loc is None or dist > alcance: continue
        if (v.co - loc).dot(nor) > -margen:
            v.co = loc - nor * margen; n += 1
    bm.normal_update(); return n

def orientar_por_superficie(bm, bvh, pred=lambda c: True, minimo=0.002):
    """Da la vuelta a las caras que miran hacia la superficie de referencia (la piel) en vez de alejarse."""
    n = 0
    for f in bm.faces:
        c = f.calc_center_median()
        if not pred(c): continue
        loc, nor, _, d = bvh.find_nearest(c)
        if loc is not None and d > minimo and f.normal.dot(c - loc) < 0:
            f.normal_flip(); n += 1
    bm.normal_update(); return n
