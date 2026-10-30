"""School Redemption Game — Explorador Celular, phase 01.

Reproducible Blender 4.5+/5.x script. No external packages required.
Run in Blender's Text Editor, or: blender -b --python crear_explorador.py
Creates its own scene. Output defaults to the package fuentes/ folder.
Optional CLI after --: --output-dir PATH --render
"""
import bpy
import math
import json
import sys
sys.dont_write_bytecode=True
import os
from pathlib import Path
from mathutils import Vector

ARGS = sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
DATA=ROOT/'validacion'/'personajes'
DATA.mkdir(parents=True,exist_ok=True)
DEFAULT_DIR=ROOT/'fuentes'
OUT = Path(ARGS[ARGS.index('--output-dir')+1]) if '--output-dir' in ARGS else DEFAULT_DIR
OUT.mkdir(parents=True, exist_ok=True)
WORK = ROOT/'vistas'/'personajes'/'explorador'
WORK.mkdir(parents=True, exist_ok=True)
startup_scene = bpy.context.scene
factory_scene = (bpy.app.background and not bpy.data.filepath and set(o.name for o in startup_scene.objects)=={'Cube','Camera','Light'})
SCENE_NAME = 'Explorador_Celular_Blockout'
if bpy.data.scenes.get(SCENE_NAME):
    i = 2
    while bpy.data.scenes.get(f'{SCENE_NAME}_{i:02d}'):
        i += 1
    SCENE_NAME += f'_{i:02d}'
scene = bpy.data.scenes.new(SCENE_NAME)
if bpy.context.window:
    bpy.context.window.scene = scene
if factory_scene:
    for o in list(startup_scene.objects):
        bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.scenes.remove(startup_scene)
    for c in list(bpy.data.collections):
        if c.users==0:
            bpy.data.collections.remove(c)
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1.0
scene.unit_settings.length_unit = 'METERS'
scene['project'] = 'School Redemption Game'
scene['phase'] = '01 — Blockout; sin UV, rig ni animacion'
scene['orientation'] = 'Frente -Y; derecha +X; arriba +Z; planta Z=0'
scene['reference_priority'] = 'Lamina Explorador Celular. Investigador y personaje femenino son referencias de otro personaje.'
scene['version'] = '0.2 — correccion de silueta y validacion ortografica'

COL = {}
for name in ['BODY','HELMET','BACKPACK','SUIT','GLOVES','BOOTS','ACCESSORIES','REFERENCES']:
    c = bpy.data.collections.new(name)
    scene.collection.children.link(c)
    COL[name] = c
PRES = bpy.data.collections.new('PRESENTATION')
scene.collection.children.link(PRES)
COL['PRESENTATION'] = PRES

def mat(name, rgb, rough=.6, metallic=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*rgb,1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*rgb,1)
    p.inputs['Roughness'].default_value = rough
    p.inputs['Metallic'].default_value = metallic
    return m

M = {
    'ivory':mat('MAT_01_Ceramica_marfil',(.79,.77,.68),.53),
    'slate':mat('MAT_02_Tejido_azul_pizarra',(.080,.125,.170),.86),
    'sage':mat('MAT_03_Soporte_verde_gris',(.150,.215,.170),.65),
    'orange':mat('MAT_04_Rescate_naranja',(.72,.205,.085),.58),
    'dark':mat('MAT_05_Grafito_flexible',(.045,.068,.077),.75),
    'visor':mat('MAT_06_Visor_azul_profundo',(.012,.027,.037),.30,.12),
    'metal':mat('MAT_07_Metal_mate',(.34,.40,.39),.4,.4),
    'screen':mat('MAT_08_Pantalla_analisis',(.035,.11,.14),.32),
    'cyan':mat('MAT_09_Lectura_cian',(.25,.66,.69),.5),
}

def link(obj, col, material=None, smooth=False):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    COL[col].objects.link(obj)
    if material:
        obj.data.materials.append(M[material])
    if obj.type == 'MESH':
        for p in obj.data.polygons:
            p.use_smooth = smooth
    if col not in ['REFERENCES','PRESENTATION']:
        obj.parent = root
        obj['stage'] = 'Blockout editable'
    return obj

root = bpy.data.objects.new('CTRL_Explorador_Origen',None)
COL['BODY'].objects.link(root)
root.empty_display_type = 'PLAIN_AXES'
root.empty_display_size = .15
root['height_target_m'] = 1.70
root['future_rig'] = 'Geometria bilateral separada; sin rig ni pesos. Crear pose A definitiva antes de retopologia.'

def mesh(name, verts, faces, col, material, smooth=False):
    data = bpy.data.meshes.new(name+'_Mesh')
    data.from_pydata(verts,[],faces)
    data.update()
    obj = bpy.data.objects.new(name,data)
    COL[col].objects.link(obj)
    return link(obj,col,material,smooth)

def bevel(obj, width=.006, segments=2):
    mod = obj.modifiers.new('Bisel_editable','BEVEL')
    mod.width=width
    mod.segments=segments
    return obj

def box(name, loc, size, col, material, radius=.006):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    obj=bpy.context.object
    obj.name=name
    obj.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    link(obj,col,material)
    if radius:
        bevel(obj,radius,2)
    return obj

def ellipsoid(name, loc, radii, col, material, seg=16, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,radius=1,location=loc)
    o=bpy.context.object
    o.name=name
    o.scale=radii
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return link(o,col,material,True)

def cyl(name, loc, radius, depth, col, material, axis=(0,0,1), vertices=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc)
    o=bpy.context.object
    o.name=name
    o.rotation_mode='QUATERNION'
    o.rotation_quaternion=Vector((0,0,1)).rotation_difference(Vector(axis))
    link(o,col,material,True)
    bevel(o,.002,1)
    return o

def torus(name, loc, major, minor, col, material, axis=(0,0,1), seg=24):
    bpy.ops.mesh.primitive_torus_add(major_segments=seg,minor_segments=6,location=loc,major_radius=major,minor_radius=minor)
    o=bpy.context.object
    o.name=name
    o.rotation_mode='QUATERNION'
    o.rotation_quaternion=Vector((0,0,1)).rotation_difference(Vector(axis))
    return link(o,col,material,True)

def tube(name, points, radius, col, material, sides=8):
    verts=[]
    for i,p in enumerate(points):
        tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(i-1,0)])
        tangent.normalize()
        ref=Vector((0,1,0)) if abs(tangent.y)<.8 else Vector((1,0,0))
        u=tangent.cross(ref).normalized()
        v=tangent.cross(u).normalized()
        for j in range(sides):
            a=2*math.pi*j/sides
            verts.append(Vector(p)+radius*(math.cos(a)*u+math.sin(a)*v))
    faces=[]
    for i in range(len(points)-1):
        for j in range(sides):
            faces.append((i*sides+j,i*sides+(j+1)%sides,(i+1)*sides+(j+1)%sides,(i+1)*sides+j))
    faces.extend([tuple(reversed(range(sides))),tuple((len(points)-1)*sides+j for j in range(sides))])
    return mesh(name,verts,faces,col,material,True)

def loft(name, rings, col, material, n=12, power=2.3):
    # rings: (x,y,z, half-width, half-depth), editable quad loops.
    verts=[]
    for x,y,z,rx,ry in rings:
        for j in range(n):
            a=2*math.pi*j/n
            ca,sa=math.cos(a),math.sin(a)
            verts.append((x+rx*math.copysign(abs(ca)**(2/power),ca),y+ry*math.copysign(abs(sa)**(2/power),sa),z))
    faces=[]
    for i in range(len(rings)-1):
        for j in range(n):
            faces.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
    faces.extend([tuple(reversed(range(n))),tuple((len(rings)-1)*n+j for j in range(n))])
    return mesh(name,verts,faces,col,material,True)

def ribbon(name, points, width, col, material):
    verts=[]
    for p in points:
        verts.extend([(p[0]-width/2,p[1],p[2]),(p[0]+width/2,p[1],p[2])])
    faces=[(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(points)-1)]
    o=mesh(name,verts,faces,col,material)
    m=o.modifiers.new('Espesor_editable','SOLIDIFY');m.thickness=.006
    bevel(o,.002,1)
    return o

def plate(name, x,y,z,w,h,depth,col,material,cut=.14):
    outline=[(-.5+cut,-.5),(.5-cut,-.5),(.5,-.5+cut),(.5,.5-cut),(.5-cut,.5),(-.5+cut,.5),(-.5,.5-cut),(-.5,-.5+cut)]
    verts=[(x+a*w,y+d,z+b*h) for d in [-depth/2,depth/2] for a,b in outline]
    faces=[tuple(reversed(range(8))),tuple(range(8,16))]
    faces.extend([(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)])
    return bevel(mesh(name,verts,[tuple(reversed(f)) for f in faces],col,material),.002,1)

# Internal structure, intentionally simple and separate from the suit.
loft('BODY_Torso_Base',[(0,0,.97,.12,.072),(0,0,1.12,.15,.084),(0,0,1.27,.17,.08),(0,0,1.32,.075,.064)],'BODY','dark')
loft('BODY_Pelvis_Base',[(0,0,.79,.12,.072),(0,0,.91,.153,.084),(0,0,1.00,.13,.08)],'BODY','slate')
cyl('BODY_Cuello_Sellado',(0,0,1.331),.083,.09,'BODY','dark')
ellipsoid('BODY_Cabeza_Proxy',(0,0,1.50),(.115,.108,.14),'BODY','dark')

# Suit: shaped volumes, broad shoulders, tucked waist and baggy legs.
loft('SUIT_Torso_Marfil',[(0,.004,.99,.132,.085),(0,.002,1.025,.149,.096),(0,0,1.105,.155,.097),(0,0,1.20,.174,.102),(0,.006,1.273,.191,.093),(0,.006,1.305,.146,.083),(0,0,1.327,.083,.068)],'SUIT','ivory',16,2.8)
plate('SUIT_Panel_Abdominal',0,-.100,1.076,.128,.166,.013,'SUIT','slate',.09)
loft('SUIT_Cadera_Pantalon',[(0,0,.80,.131,.080),(0,.003,.87,.160,.095),(0,0,.965,.151,.093),(0,0,.99,.133,.082)],'SUIT','slate',16,2.6)
loft('SUIT_Cinturon',[(0,0,.967,.154,.099),(0,0,1.002,.149,.096)],'ACCESSORIES','dark',16,3.0)
box('ACC_Hebilla_Cinturon',(0,-.105,.986),(.064,.021,.042),'ACCESSORIES','metal',.004)
box('ACC_Hebilla_Interior',(0,-.119,.986),(.039,.004,.023),'ACCESSORIES','dark',.002)

for s,tag in [(-1,'R'),(1,'L')]:
    rings=[(s*.108,.008,.218,.052,.057),(s*.116,.004,.254,.065,.067),(s*.123,.007,.28,.078,.075),(s*.117,.012,.343,.073,.077),(s*.116,.005,.43,.079,.080),(s*.112,-.004,.50,.077,.079),(s*.109,.002,.55,.077,.085),(s*.108,.008,.64,.086,.091),(s*.102,.005,.75,.09,.091),(s*.089,.006,.83,.082,.086),(s*.082,.004,.876,.080,.080)]
    pants=loft('SUIT_Pantalon_'+tag,rings,'SUIT','slate',12,2.5)
    # Sage textile patch shares the leg surface; no extra shin armor.
    pants.data.materials.append(M['sage'])
    for ring in range(1,5):
        for j in [7,8,9,10]:
            pants.data.polygons[ring*12+j].material_index=1
    plate('SUIT_Rodillera_'+tag,s*.113,-.081,.516,.123,.117,.023,'SUIT','sage',.17)
    plate('SUIT_Rodillera_Centro_'+tag,s*.113,-.096,.52,.081,.080,.006,'SUIT','metal',.22)
    for dx in [-.044,.044]:
        cyl('SUIT_Rodillera_Fijacion_'+tag+('_a' if dx<0 else '_b'),(s*.113+dx,-.097,.549),.005,.005,'SUIT','dark',axis=(0,1,0),vertices=8)
    # Front orange harness continues down the outside of the thighs.
    ribbon('ACC_Cinta_Muslo_'+tag,[(s*.142,-.080,.954),(s*.157,-.086,.88),(s*.168,-.080,.785),(s*.169,-.068,.70)],.017,'ACCESSORIES','orange')
    box('ACC_Pasador_Muslo_'+tag,(s*.169,-.080,.704),(.029,.018,.039),'ACCESSORIES','metal',.004)
    box('ACC_Bolsa_Cadera_'+tag,(s*.182,-.012,.918),(.050,.083,.105),'ACCESSORIES','orange',.012)
    box('ACC_Bolsa_Tapa_'+tag,(s*.185,-.045,.949),(.052,.024,.032),'ACCESSORIES','sage',.005)
    # Arms slightly separated for silhouette and a future pose A.
    shoulder=(s*.199,0,1.257)
    elbow=(s*.262,-.006,1.032)
    wrist=(s*.314,-.014,.835)
    ellipsoid('BODY_Hombro_'+tag,shoulder,(.068,.068,.067),'BODY','dark',12,6)
    loft('SUIT_Manga_Superior_'+tag,[(s*.266,-.003,1.028,.064,.066),(s*.257,0,1.075,.076,.075),(s*.234,.003,1.17,.073,.078),(s*.208,.004,1.247,.075,.077),(s*.194,.004,1.285,.057,.065)],'SUIT','ivory',12,2.1)
    ellipsoid('BODY_Codo_'+tag,elbow,(.056,.056,.046),'BODY','dark',12,6)
    loft('SUIT_Manga_Inferior_'+tag,[(s*.306,-.010,.856,.053,.057),(s*.303,-.008,.886,.069,.068),(s*.289,-.008,.927,.073,.072),(s*.276,-.007,.984,.063,.066),(s*.265,-.006,1.037,.063,.065)],'SUIT','ivory',12,2.3)
    cuff=cyl('SUIT_Puno_Sellado_'+tag,(s*.311,-.014,.845),.059,.058,'SUIT','sage',axis=(s*.23,0,-.97),vertices=16)
    cuff.scale.y=.94
    # Shoulder patch: readable circular inset on outside.
    axis=(s*.95,-.08,.20)
    cyl('SUIT_Parche_Hombro_Naranja_'+tag,(s*.277,-.005,1.222),.046,.010,'SUIT','orange',axis,20)
    cyl('SUIT_Parche_Hombro_Marfil_'+tag,(s*.284,-.005,1.223),.035,.012,'SUIT','ivory',axis,20)
    cyl('SUIT_Parche_Hombro_Centro_'+tag,(s*.291,-.006,1.224),.025,.013,'SUIT','slate',axis,20)
    # Gloves have palm, thumb and four readable fingers; no generic mitten.
    cyl('GLOVE_Muneca_'+tag,(s*.318,-.014,.807),.040,.044,'GLOVES','dark',axis=(s*.23,0,-.97),vertices=12)
    palm=ellipsoid('GLOVE_Palma_'+tag,(s*.327,-.015,.769),(.044,.033,.052),'GLOVES','dark',12,6)
    for j in range(4):
        fx=s*(.301+j*.018)
        top=.749-abs(j-1.5)*.003
        length=[.046,.058,.055,.043][j]
        tube('GLOVE_Dedo_'+str(j+1)+'_'+tag,[(fx,-.022,top),(fx+s*.005,-.027,top-length*.52),(fx+s*.002,-.022,top-length)],.009,'GLOVES','dark',6)
        box('GLOVE_Nudillo_'+str(j+1)+'_'+tag,(fx,-.045,top+.005),(.011,.009,.017),'GLOVES','orange',.003)
    tube('GLOVE_Pulgar_'+tag,[(s*.293,-.013,.783),(s*.281,-.027,.763),(s*.283,-.038,.741)],.014,'GLOVES','dark',8)
    cyl('GLOVE_Sensor_Aro_'+tag,(s*.328,-.046,.783),.026,.010,'GLOVES','orange',(0,-1,0),20)
    cyl('GLOVE_Sensor_Centro_'+tag,(s*.328,-.053,.783),.019,.005,'GLOVES','ivory',(0,-1,0),20)
    cyl('GLOVE_Sensor_Lente_'+tag,(s*.328,-.057,.783),.014,.004,'GLOVES','visor',(0,-1,0),20)
    # Boots: broad toe, flat soles exactly at Z=0, raised heel and ankle.
    bx=s*.118
    loft('BOOT_Suela_'+tag,[(bx,-.045,0,.078,.142),(bx,-.045,.018,.083,.148),(bx,-.045,.047,.084,.148),(bx,-.043,.057,.077,.137)],'BOOTS','dark',12,3.8)
    loft('BOOT_Carcasa_'+tag,[(bx,-.044,.047,.079,.138),(bx,-.044,.080,.078,.135),(bx,-.039,.114,.074,.125),(bx,-.006,.147,.064,.080),(bx,.011,.194,.058,.062),(bx,.013,.224,.051,.054)],'BOOTS','sage',12,3.1)
    loft('BOOT_Puntera_Marfil_'+tag,[(bx,-.132,.047,.076,.049),(bx,-.134,.076,.075,.048),(bx,-.128,.105,.065,.042)],'BOOTS','ivory',12,3.6)
    box('BOOT_Proteccion_Talon_'+tag,(bx,.078,.071),(.134,.044,.084),'BOOTS','ivory',.016)
    box('BOOT_Lengua_'+tag,(bx,-.048,.173),(.080,.034,.10),'BOOTS','slate',.012)
    box('BOOT_Correa_Tobillo_'+tag,(bx,-.065,.205),(.102,.022,.026),'BOOTS','orange',.006)
    strap=box('BOOT_Correa_Empeine_'+tag,(bx,-.087,.129),(.122,.035,.025),'BOOTS','orange',.006)
    strap.rotation_euler.x=math.radians(20)
    box('BOOT_Cierre_Empeine_'+tag,(bx,-.112,.132),(.041,.018,.023),'BOOTS','metal',.004)
    cyl('BOOT_Bisagra_'+tag,(bx+s*.071,.007,.137),.024,.012,'BOOTS','ivory',(s,0,0),16)
    # Orange load straps from the breast over the shoulders to the backpack.
    ribbon('ACC_Tirante_Hombro_'+tag,[(s*.125,-.097,1.112),(s*.145,-.113,1.209),(s*.159,-.087,1.281),(s*.157,-.032,1.309),(s*.154,.045,1.306),(s*.150,.118,1.252)],.034,'ACCESSORIES','orange')
    box('ACC_Cierre_Tirante_'+tag,(s*.142,-.119,1.204),(.037,.022,.051),'ACCESSORIES','dark',.006)
    box('ACC_Cierre_Tirante_Interior_'+tag,(s*.142,-.132,1.205),(.022,.005,.023),'ACCESSORIES','metal',.003)

# Central breast instrument, chamfered like the reference, with triangular mark.
plate('ACC_Placa_Pectoral_Base',0,-.111,1.202,.184,.168,.024,'ACCESSORIES','sage',.12)
plate('ACC_Placa_Pectoral_Marfil',0,-.128,1.204,.167,.149,.015,'ACCESSORIES','ivory',.12)
triangle=[(-.027,-.139,1.178),(.027,-.139,1.178),(0,-.139,1.234),(-.013,-.140,1.187),(.013,-.140,1.187),(0,-.140,1.215)]
mesh('ACC_Emblema_Triangulo',triangle,[(0,1,4,3),(1,2,5,4),(2,0,3,5)],'ACCESSORIES','orange')
for x in [-.065,.065]:
    for z in [1.151,1.258]:
        cyl('ACC_Placa_Fijacion_'+str(x)+'_'+str(z),(x,-.139,z),.0055,.004,'ACCESSORIES','metal',(0,-1,0),8)

# Collar: seal, broad green ring, front orange identification inset.
cyl('HELMET_Junta_Cuello',(0,.002,1.336),.098,.030,'HELMET','dark',vertices=32)
loft('HELMET_Collar_Soporte',[(0,0,1.302,.111,.096),(0,0,1.310,.127,.111),(0,0,1.345,.126,.111),(0,0,1.357,.110,.098)],'HELMET','sage',24,2.3)
plate('HELMET_Collar_Placa',0,-.110,1.330,.068,.029,.010,'HELMET','ivory',.12)
plate('HELMET_Collar_Naranja',0,-.117,1.330,.033,.012,.004,'HELMET','orange',.12)

# Helmet shell with an actual opening. Superellipse aperture follows curved surface.
HX,HY,HZ = .190,.176,.184
HC = Vector((0,.008,1.504))
N=40
OPEN_A,OPEN_B=.143,.145
def aperture(t):
    c,s=math.cos(t),math.sin(t)
    return OPEN_A*math.copysign(abs(c)**.66,c),OPEN_B*math.copysign(abs(s)**.78,s)
def front_y(x,z):
    return HC.y-HY*math.sqrt(max(.01,1-(x/HX)**2-(z/HZ)**2))
verts=[]
# Open boundary first; then spherical rings moving back to rear pole.
for j in range(N):
    x,z=aperture(2*math.pi*j/N)
    verts.append((x,front_y(x,z),HC.z+z))
for phi in [1.39,1.64,1.91,2.20,2.49,2.77,3.02]:
    for j in range(N):
        a=2*math.pi*j/N
        verts.append((HX*math.sin(phi)*math.cos(a),HC.y-HY*math.cos(phi),HC.z+HZ*math.sin(phi)*math.sin(a)))
faces=[]
for i in range(7):
    for j in range(N):
        faces.append((i*N+j,i*N+(j+1)%N,(i+1)*N+(j+1)%N,(i+1)*N+j))
verts.append((0,HC.y+HY,HC.z));end=len(verts)-1
for j in range(N):faces.append((7*N+j,7*N+(j+1)%N,end))
shell=mesh('HELMET_Calota_Con_Abertura',verts,[tuple(reversed(f)) for f in faces],'HELMET','ivory',True)
sol=shell.modifiers.new('Espesor_Calota_editable','SOLIDIFY');sol.thickness=.004;sol.offset=-1

# Curved dark glazing with tailored opening and an independently editable rim.
def visor_surface(scale=1,offset=0):
    vs=[]
    for r in [0,.25,.5,.75,1]:
        for j in range(N):
            x,z=aperture(2*math.pi*j/N);x*=r*scale;z*=r*scale
            vs.append((x,front_y(x,z)-.004+offset,HC.z+z))
    fs=[]
    for k in range(4):
        for j in range(N):fs.append((k*N+j,k*N+(j+1)%N,(k+1)*N+(j+1)%N,(k+1)*N+j))
    return vs,fs
vs,fs=visor_surface()
# Remove coincident center ring and create a clean triangle fan.
vs=vs[N:]+[(0,HC.y-HY-.004,HC.z)]
fs=[]
for j in range(N):fs.append((len(vs)-1,j,(j+1)%N))
for k in range(3):
    for j in range(N):fs.append(((k+1)*N+j,(k+1)*N+(j+1)%N,k*N+(j+1)%N,k*N+j))
mesh('HELMET_Visor_Curvo',vs,fs,'HELMET','visor',True)
rim=[]
for j in range(N+1):
    x,z=aperture(2*math.pi*j/N);rim.append((x,front_y(x,z)-.003,HC.z+z))
tube('HELMET_Junta_Perimetral_Visor',rim,.006,'HELMET','dark',6)
for s,tag in [(-1,'R'),(1,'L')]:
    cyl('HELMET_Comunicador_Base_'+tag,(s*.184,.008,1.501),.071,.034,'HELMET','sage',(s,0,0),28)
    cyl('HELMET_Comunicador_Aro_'+tag,(s*.206,.008,1.501),.055,.018,'HELMET','metal',(s,0,0),28)
    cyl('HELMET_Comunicador_Centro_'+tag,(s*.218,.008,1.501),.041,.016,'HELMET','dark',(s,0,0),24)
    cyl('HELMET_Comunicador_Lente_'+tag,(s*.227,.008,1.501),.026,.007,'HELMET','slate',(s,0,0),24)
    # small shell latches, visibly framing the glass
    latch=box('HELMET_Cierre_Inferior_'+tag,(s*.106,-.082,1.370),(.027,.024,.022),'HELMET','sage',.006)
    latch.rotation_euler.y=s*math.radians(18)
box('HELMET_Puente_Frente',(0,-.079,1.665),(.115,.042,.018),'HELMET','sage',.006)
box('HELMET_Marca_Superior',(0,-.018,1.686),(.044,.065,.006),'HELMET','orange',.002)
box('HELMET_Placa_Posterior',(0,.178,1.456),(.151,.020,.040),'HELMET','sage',.009)
box('HELMET_Antena_Base',(-.198,.052,1.552),(.033,.037,.046),'HELMET','sage',.006)
tube('HELMET_Antena', [(-.198,.056,1.57),(-.203,.056,1.685)], .005,'HELMET','dark',8)
ellipsoid('HELMET_Antena_Punta',(-.203,.056,1.693),(.006,.006,.007),'HELMET','sage',8,4)

# Life support pack: independent canister, rear circular port, orange retaining strips.
box('PACK_Base_Rigida',(0,.152,1.105),(.282,.126,.406),'BACKPACK','dark',.035)
box('PACK_Carcasa_Verde',(0,.195,1.111),(.287,.166,.397),'BACKPACK','sage',.032)
plate('PACK_Panel_Posterior',0,.286,1.118,.239,.334,.015,'BACKPACK','slate',.12)
plate('PACK_Panel_Centro',0,.297,1.119,.213,.299,.009,'BACKPACK','sage',.12)
box('PACK_Base_Inferior',(0,.213,.922),(.249,.153,.053),'BACKPACK','slate',.018)
for s,tag in [(-1,'R'),(1,'L')]:
    box('PACK_Cartucho_Lateral_'+tag,(s*.158,.188,1.111),(.046,.128,.333),'BACKPACK','slate',.018)
    for z in [.982,1.235]:
        box('PACK_Abrazadera_'+tag+'_'+str(z),(s*.157,.195,z),(.052,.139,.025),'BACKPACK','orange',.004)
    ribbon('PACK_Cinta_Posterior_'+tag,[(s*.105,.285,.968),(s*.119,.293,1.017),(s*.119,.295,1.215),(s*.102,.272,1.285)],.016,'BACKPACK','orange')
    box('PACK_Pasador_'+tag,(s*.119,.308,1.040),(.028,.013,.042),'BACKPACK','metal',.004)
    tube('PACK_Asa_Soporte_'+tag,[(s*.079,.150,1.302),(s*.068,.159,1.337),(s*.035,.164,1.339)],.011,'BACKPACK','sage',8)
cyl('PACK_Puerto_Circular_Base',(0,.307,1.174),.066,.020,'BACKPACK','dark',(0,1,0),28)
cyl('PACK_Puerto_Circular_Aro',(0,.321,1.174),.051,.012,'BACKPACK','ivory',(0,1,0),28)
cyl('PACK_Puerto_Circular_Centro',(0,.330,1.174),.037,.009,'BACKPACK','dark',(0,1,0),24)
torus('PACK_Puerto_Interior',(0,.336,1.174),.029,.004,'BACKPACK','metal',(0,1,0),24)
box('PACK_Luz_Estado',(0,.306,1.276),(.066,.010,.012),'BACKPACK','ivory',.003)
for i in range(3):
    box('PACK_Ventilacion_'+str(i),(0,.309,1.027+i*.014),(.077,.009,.005),'BACKPACK','dark',.001)
tube('PACK_Manguera_Cuello',[(.125,.184,1.286),(.155,.119,1.327),(.130,.060,1.347),(.103,.042,1.341)],.015,'BACKPACK','dark',8)

# Analysis wrist instrument, mounted on front of left forearm.
box('ACC_Brazalete_Correa',( .304,-.014,.902),(.129,.125,.041),'ACCESSORIES','orange',.008)
device=plate('ACC_Brazalete_Carcasa',.304,-.087,.922,.101,.105,.027,'ACCESSORIES','dark',.14)
plate('ACC_Brazalete_Pantalla',.304,-.104,.928,.079,.070,.006,'ACCESSORIES','screen',.07)
box('ACC_Brazalete_Boton',(.304,-.107,.886),(.026,.008,.010),'ACCESSORIES','ivory',.002)
tube('ACC_Brazalete_Traza',[(.274,-.109,.926),(.283,-.109,.926),(.290,-.109,.943),(.297,-.109,.912),(.305,-.109,.936),(.315,-.109,.926),(.333,-.109,.926)],.0013,'ACCESSORIES','cyan',5)

# Referencias locales opcionales; la entrega publica se genera sin imagenes empaquetadas.
REF_DIR = Path(ARGS[ARGS.index('--references-dir')+1]) if '--references-dir' in ARGS else None
ref_names = [
 ('REF_01_Explorador_Celular','explorador-celular-concepto.png'),
 ('REF_02_Investigador_Otro_Personaje','investigador-referencia-inicial.png'),
 ('REF_03_Investigadora_Estilo_Sencillo','investigadora-estilo.png')]
for i,(name,filename) in enumerate(ref_names):
    candidates = [REF_DIR/filename] if REF_DIR else []
    path=next((p for p in candidates if p.exists()),None)
    if path:
        img=bpy.data.images.load(str(path),check_existing=True)
        img.pack()
        o=bpy.data.objects.new(name,None);o.empty_display_type='IMAGE';o.data=img
        COL['REFERENCES'].objects.link(o)
        o.empty_display_size=1.7;o.location=(2.0+i*1.9,.65,.85);o.rotation_euler=(math.pi/2,0,0)
        o.color[3]=.75;o.hide_render=True
        o['purpose']='Referencia visual; no contiene instrucciones de ejecucion'
COL['REFERENCES'].hide_viewport=True
COL['REFERENCES'].hide_render=True

# Presentation only; excluded from character dimensions and triangle counts.
floor_mat=mat('MAT_Studio',(.024,.039,.050),.9)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.002))
floor=bpy.context.object;floor.name='STUDIO_Suelo_Z0';link(floor,'PRESENTATION');floor.data.materials.append(floor_mat)
def camera(name,loc,target,ortho):
    data=bpy.data.cameras.new(name);o=bpy.data.objects.new(name,data);PRES.objects.link(o)
    o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO';data.ortho_scale=ortho;data.lens=70;data.clip_start=.01;data.clip_end=200
    return o
cams={
 'front':camera('CAM_01_Frontal',(0,-5,.85),(0,0,.85),1.94),
 'side':camera('CAM_02_Lateral',(5,0,.85),(0,0,.85),1.94),
 'back':camera('CAM_03_Posterior',(0,5,.85),(0,0,.85),1.94),
 'hero':camera('CAM_04_Tres_Cuartos',(3.15,-5.3,2.65),(0,0,.84),2.00),
}
def area(name,loc,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);PRES.objects.link(o);o.location=loc
    o.rotation_euler=(Vector((0,0,.9))-o.location).to_track_quat('-Z','Y').to_euler()
area('LIGHT_Key',(-2,-3,4),350,3,(1,.88,.74))
area('LIGHT_Fill',(2,-2,2.5),230,2.5,(.69,.84,1))
area('LIGHT_Rim',(0,2.6,3.5),450,2.5,(.78,.9,1))
world=bpy.data.worlds.new('WORLD_Studio');scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.045,.065,.085,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.5
scene.render.engine='CYCLES'
scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=760;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.view_settings.view_transform='AgX'
scene.render.image_settings.color_mode='RGBA'
scene.camera=cams['hero']
for o in PRES.objects:
    o.hide_set(True)

# Metadata for future joint placement, stored as data, not an armature/animation.
root['joints_m'] = json.dumps({'ankle_L':[.118,.008,.196],'knee_L':[.112,0,.51],'hip_L':[.086,0,.84], 'shoulder_L':[.199,0,1.257],'elbow_L':[.262,-.006,1.032],'wrist_L':[.314,-.014,.835], 'mirror_R':'invert X','neck':[0,0,1.337]},ensure_ascii=False)
scene['limits']='Blockout segmentado: intersecciones intencionales, sin retopologia de deformacion, sin UV, rig, LOD o presupuesto VR validado.'

# Count both base meshes and evaluated meshes (bevel/solidify), never studio or images.
bpy.context.view_layer.update()
characters=[o for c in COL.values() if c.name not in [COL['PRESENTATION'].name,COL['REFERENCES'].name] for o in c.objects if o.type=='MESH']
dg=bpy.context.evaluated_depsgraph_get()
stats=[];coords=[]
for o in characters:
    o.data.calc_loop_triangles()
    e=o.evaluated_get(dg);me=e.to_mesh();me.calc_loop_triangles()
    bb=[e.matrix_world@Vector(v) for v in e.bound_box];coords+=bb
    stats.append({'object':o.name,'collection':o.users_collection[0].name,'base_vertices':len(o.data.vertices),'base_triangles':len(o.data.loop_triangles),'evaluated_triangles':len(me.loop_triangles),'modifiers':[m.type for m in o.modifiers]})
    e.to_mesh_clear()
mins=[min(p[i] for p in coords) for i in range(3)];maxs=[max(p[i] for p in coords) for i in range(3)]
report={'project':'School Redemption Game','character':'Explorador Celular','phase':'Blockout 01','blender':bpy.app.version_string,'units':'METRIC, 1 Blender unit = 1 meter','orientation':'front -Y, up +Z','bounding_min_m':mins,'bounding_max_m':maxs,'dimensions_m':[maxs[i]-mins[i] for i in range(3)],'mesh_objects':len(characters),'base_triangles':sum(s['base_triangles'] for s in stats),'evaluated_triangles':sum(s['evaluated_triangles'] for s in stats),'base_vertices':sum(s['base_vertices'] for s in stats),'collections':{},'objects':stats}
for name in ['BODY','HELMET','BACKPACK','SUIT','GLOVES','BOOTS','ACCESSORIES','REFERENCES']:
    report['collections'][name]={'objects':len(COL[name].objects),'base_triangles':sum(s['base_triangles'] for s in stats if s['collection']==COL[name].name),'evaluated_triangles':sum(s['evaluated_triangles'] for s in stats if s['collection']==COL[name].name)}
report['checks']={'feet_z0':abs(mins[2])<1e-5,'height_1_70m':abs(maxs[2]-1.70)<.002,'centered_x':abs(maxs[0]+mins[0])<.005,'reference_policy_ok':all(o.data and o.data.packed_file for o in COL['REFERENCES'].objects) if REF_DIR else len(COL['REFERENCES'].objects)==0,'no_armature':not any(o.type=='ARMATURE' for o in scene.objects),'no_animation':not any(o.animation_data for o in scene.objects)}
(DATA/'explorador-celular-informe.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf-8')

# Save with friendly material preview and a three-quarter view, character selected.
for o in bpy.context.selected_objects:o.select_set(False)
root.select_set(True);bpy.context.view_layer.objects.active=root
for screen in bpy.data.screens:
    for a in screen.areas:
        if a.type=='VIEW_3D':
            a.spaces.active.region_3d.view_rotation=cams['hero'].rotation_euler.to_quaternion()
            a.spaces.active.region_3d.view_distance=2.8
            a.spaces.active.region_3d.view_location=(0,0,.85)
            a.spaces.active.region_3d.view_perspective='ORTHO'
            a.spaces.active.shading.type='MATERIAL'
            a.spaces.active.overlay.show_floor=True
            a.spaces.active.overlay.show_relationship_lines=False
readme=bpy.data.texts.new('LEEME_Blockout')
readme.write('EXPLORADOR CELULAR / SCHOOL REDEMPTION GAME\nFase 01: blockout editable. Frente -Y, arriba +Z.\n8 colecciones de personaje/referencias + PRESENTATION (solo estudio).\nEntrega publica sin referencias. Uso privado opcional con --references-dir.\nCamaras CAM_01/02/03 para revisar frente, lado y espalda.\nColores planos de identificacion; sin texturas, UV, rig ni animaciones.\nSeleccionar CTRL_Explorador_Origen para mover el conjunto.\nPiezas independientes; modificadores de bisel y espesor conservados.\nLa topologia actual no se considera final para deformacion o VR.\n')
try:
    script_text=bpy.data.texts.new('crear_explorador.py');script_text.write(Path(__file__).read_text(encoding='utf-8'))
except (NameError,OSError):pass
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'explorador-celular.blend'),compress=True)
print('BLOCKOUT_REPORT',json.dumps({k:report[k] for k in ['dimensions_m','mesh_objects','base_triangles','evaluated_triangles','checks']}))
if '--render' in ARGS:
    for view,cam in cams.items():
        scene.camera=cam;scene.render.filepath=str(WORK/f'preview_{view}.png')
        bpy.ops.render.render(write_still=True)
    scene.camera=cams['hero']
print('BLOCKOUT_COMPLETE',str(OUT/'explorador-celular.blend'))
