"""El Investigador / School Redemption Game — editable stylized blockout.

Blender 5.2.2 tested. Run in Text Editor or blender -b --factory-startup
--python crear_investigador.py -- --render. Keep the complete package folder structure. No add-on or external Python packages required.
Creates two scenes (reading and relaxed A); preserves existing custom scenes.
"""
import bpy
import math
import json
import sys
sys.dont_write_bytecode=True
import importlib.util
import bmesh
from pathlib import Path
from mathutils import Vector, Matrix

ARGS=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
DATA=ROOT/'validacion'/'personajes'
DATA.mkdir(parents=True,exist_ok=True)
OUT=Path(ARGS[ARGS.index('--output-dir')+1]) if '--output-dir' in ARGS else ROOT/'fuentes'
OUT.mkdir(parents=True,exist_ok=True)
WORK=Path(ARGS[ARGS.index('--work-dir')+1]) if '--work-dir' in ARGS else ROOT/'vistas'/'personajes'/'investigador'
WORK.mkdir(parents=True,exist_ok=True)
old_scene=bpy.context.scene
factory=bpy.app.background and not bpy.data.filepath and set(o.name for o in old_scene.objects)=={'Camera','Cube','Light'}
scene=bpy.data.scenes.new('Investigador_Blockout')
bpy.context.window.scene=scene
if factory:
    for o in list(old_scene.objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.scenes.remove(old_scene)
    for c in list(bpy.data.collections):
        if c.users==0:bpy.data.collections.remove(c)
scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1;scene.unit_settings.length_unit='METERS'
scene['project']='School Redemption Game'
scene['phase']='Refinamiento 02 — cara y hombros con sisas continuas'
scene['orientation']='Frente -Y, arriba +Z. Pies Z=0; altura nominal1.78m.'
scene['version']='0.3 — rostro anatomico y chaqueta de hombros continuos'
COL={}
NAMES=['BODY','HEAD','HAIR','GLASSES','JACKET','PANTS','BOOTS','BACKPACK','ACCESSORIES','ARMS_LECTURA','TABLET','REFERENCES','PRESENTATION']
for name in NAMES:
    c=bpy.data.collections.new(name);scene.collection.children.link(c);COL[name]=c

# Shared modeling helpers follow. They create real, independently editable meshes.
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
    'ivory':mat('MAT_Jacket_Porcelain',(.63,.61,.56),.85),
    'slate':mat('MAT_Cargo_Blue',(.047,.067,.091),.9),
    'sage':mat('MAT_Jacket_Shadow',(.31,.33,.32),.85),
    'yellow':mat('MAT_Mission_Ochre',(.72,.34,.035),.78),
    'dark':mat('MAT_Technical_Charcoal',(.023,.029,.034),.86),
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

root = bpy.data.objects.new('CTRL_Investigador_Origen',None)
COL['BODY'].objects.link(root)
root.empty_display_type = 'PLAIN_AXES'
root.empty_display_size = .15
root['height_target_m'] = 1.78
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



# Additional palette for the human face, fabric and hair.
M.update({
 'skin':mat('MAT_Skin_Warm',(.64,.39,.24),.78),
 'skin_shadow':mat('MAT_Skin_Shadow',(.37,.19,.13),.85),
 'skin_light':mat('MAT_Skin_Light',(.75,.50,.33),.8),
 'hair':mat('MAT_Hair_Ink',(.004,.006,.009),.92),
 'hair_hi':mat('MAT_Hair_Plane',(.008,.012,.017),.9),
 'charcoal':mat('MAT_Glasses_Graphite',(.018,.024,.028),.55),
 'eye_white':mat('MAT_Eye_SoftWhite',(.79,.76,.67),.6),
 'iris':mat('MAT_Iris_Slate',(.074,.12,.155),.65),
 'pupil':mat('MAT_Pupil',(.006,.008,.010),.6),
 'lip':mat('MAT_Mouth',(.30,.13,.10),.9),
 'lens':mat('MAT_Lens_SubtleBlue',(.21,.35,.42),.18),
 'fold':mat('MAT_Cargo_Fold',(.032,.045,.061),.94),
 'cloth_light':mat('MAT_Cargo_LightPlane',(.066,.087,.111),.94),
 'boot_light':mat('MAT_Boot_WarmWhite',(.65,.64,.57),.75),
})
M['lens'].node_tree.nodes.get('Principled BSDF').inputs['Alpha'].default_value=.055
M['lens'].diffuse_color=(.21,.35,.42,.055)
M['lens'].node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.15
for key in ['hair','hair_hi']:
    M[key].node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.14
sp=M['screen'].node_tree.nodes.get('Principled BSDF')
sp.inputs['Base Color'].default_value=(.014,.038,.053,1)
sp.inputs['Roughness'].default_value=.7
sp.inputs['Specular IOR Level'].default_value=.12
M['screen'].diffuse_color=(.014,.038,.053,1)
M['cyan'].node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.18,.42,.50,1)

def solid(obj,thickness=.004):
    m=obj.modifiers.new('Espesor_editable','SOLIDIFY');m.thickness=thickness;m.offset=0
    return obj

def sweep(name,rows,col,material,n=12,power=2.1):
    # rows of (center, rx, ry); ring frame follows the center line.
    vs=[]
    for i,(p,rx,ry) in enumerate(rows):
        tangent=Vector(rows[min(i+1,len(rows)-1)][0])-Vector(rows[max(i-1,0)][0])
        tangent.normalize();ref=Vector((0,1,0))
        if abs(tangent.dot(ref))>.94:ref=Vector((0,0,1))
        u=tangent.cross(ref).normalized();v=tangent.cross(u).normalized()
        for j in range(n):
            a=j*math.tau/n;c=math.cos(a);s=math.sin(a)
            vs.append(Vector(p)+u*rx*math.copysign(abs(c)**(2/power),c)+v*ry*math.copysign(abs(s)**(2/power),s))
    fs=[]
    for i in range(len(rows)-1):
        for j in range(n):fs.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
    fs.extend([tuple(reversed(range(n))),tuple((len(rows)-1)*n+j for j in range(n))])
    return mesh(name,vs,fs,col,material,True)

def poly(name,points,col,material,thick=.003):
    o=mesh(name,points,[tuple(range(len(points)))],col,material)
    if thick:solid(o,thick)
    return o

def local_frame(normal):
    n=Vector(normal).normalized();v=Vector((0,0,1))
    v=(v-n*v.dot(n)).normalized();u=v.cross(n).normalized()
    return u,v,n

def badge(name,center,radius,normal,col):
    u,v,n=local_frame(normal);p=Vector(center)
    cyl(name+'_Base',p,radius,.005,col,'dark',normal,24)
    torus(name+'_Aro',p+n*.004,radius*.82,radius*.075,col,'yellow',normal,24)
    # Broken triangular arrow corresponding to the concept's scientific team mark.
    shape=[(-.40,-.31),(.04,.46),(.43,-.14),(.10,-.09),(-.19,-.25)]
    tube(name+'_Marca',[p+n*.006+u*a*radius+v*b*radius for a,b in shape],radius*.09,col,'ivory',5)

def panel_patch(name,points,col,material):
    return poly(name,points,col,material,.0015)

# Shirt, neck and underlying anatomy. No proxy head remains behind the face.
loft('BODY_Camisa_Torso',[(0,0,1.005,.126,.071),(0,0,1.13,.139,.076),(0,0,1.32,.163,.083),(0,0,1.397,.177,.076),(0,0,1.442,.067,.052)],'BODY','dark',16,2.6)
loft('BODY_Camisa_Cuello',[(0,.002,1.42,.054,.045),(0,.002,1.507,.046,.039),(0,.001,1.535,.041,.035)],'BODY','dark',16,2.4)
loft('BODY_Cuello',[(0,.004,1.472,.038,.033),(0,.003,1.531,.039,.033),(0,.006,1.565,.044,.037)],'BODY','skin',16,2.0)
tube('BODY_Camisa_Costura',[(0,-.078,1.08),(0,-.083,1.29),(0,-.049,1.423),(0,-.039,1.521)],.0011,'BODY','charcoal',5)
for s,tag in [(-1,'R'),(1,'L')]:
    poly('BODY_Camisa_Solapa_'+tag,[(s*.004,-.040,1.524),(s*.027,-.040,1.516),(s*.031,-.045,1.490),(s*.014,-.045,1.497)],'BODY','dark',.002)

# Open jacket surface with asymmetric large cloth folds in the ring shapes.
jacket_rows=[(.985,.148,.085,.32),(1.01,.154,.090,.31),(1.075,.148,.091,.32),(1.13,.147,.090,.32),(1.215,.160,.095,.34),(1.305,.174,.098,.35),(1.365,.185,.094,.35),(1.397,.181,.086,.36),(1.422,.155,.078,.38),(1.440,.112,.066,.39),(1.458,.077,.056,.40)]
JN=26;verts=[];front_edges=[[],[]]
for ri,(z,rx,ry,gap) in enumerate(jacket_rows):
    for j in range(JN+1):
        a=-math.pi/2+gap+(math.tau-2*gap)*j/JN
        c,sn=math.cos(a),math.sin(a)
        x=rx*math.copysign(abs(c)**.80,c);y=ry*math.copysign(abs(sn)**.80,sn)
        if ri in [2,3] and j not in [0,JN]:y+=.004*math.sin(j*2.3+ri)
        verts.append((x,y,z))
    front_edges[0].append(verts[ri*(JN+1)]);front_edges[1].append(verts[ri*(JN+1)+JN])
faces=[];jacket_materials=[]
for i in range(len(jacket_rows)-1):
    for j in range(JN):
        # Real armholes: these faces will be replaced by connected sleeve loops.
        if 5<=i<9 and (3<=j<9 or 17<=j<23):continue
        faces.append((i*(JN+1)+j,i*(JN+1)+j+1,(i+1)*(JN+1)+j+1,(i+1)*(JN+1)+j))
        jacket_materials.append(1 if i>=6 and (1<=j<12 or 14<=j<25) else 0)
jacket_template=(list(verts),list(faces),list(jacket_materials))
for side,tag in [(0,'L'),(1,'R')]:
    edge=front_edges[side]
    ribbon('JACKET_Ribete_Amarillo_'+tag,[(x,y-.003,z) for x,y,z in edge],.014,'JACKET','yellow')
    for idx in [1,4,6]:
        x,y,z=edge[idx]
        cyl('JACKET_Broche_'+tag+'_'+str(idx),(x,y-.009,z),.003,.004,'JACKET','metal',(0,-1,0),8)
# Tall standing collar with visible ochre lining and a split in front.
for s,tag in [(-1,'R'),(1,'L')]:
    pts=[(s*.034,-.066,1.429),(s*.069,-.034,1.441),(s*.064,.023,1.446),(s*.047,.050,1.447),(s*.047,.052,1.520),(s*.066,.019,1.527),(s*.080,-.047,1.511),(s*.044,-.075,1.489)]
    o=poly('JACKET_Cuello_Alto_'+tag,pts,'JACKET','ivory',.006);bevel(o,.002,1)
    lining=[(s*.041,-.074,1.435),(s*.069,-.048,1.447),(s*.074,-.046,1.506),(s*.046,-.072,1.484)]
    poly('JACKET_Cuello_Forro_'+tag,lining,'JACKET','yellow',.002)
    tube('JACKET_Cuello_Pespunte_'+tag,[pts[-1],pts[-2],pts[-3],pts[-4]],.0013,'JACKET','yellow',5)
tube('JACKET_Borde_Hem',[(x,y-.0005,z) for x,y,z in verts[:JN+1]],.002,'JACKET','sage',6)
plate('JACKET_Bolsillo_Pecho',-.117,-.095,1.281,.056,.071,.007,'JACKET','sage',.05)
box('JACKET_Bolsillo_Borde',(-.117,-.101,1.308),(.055,.006,.007),'JACKET','ivory',.001)
for s,tag in [(-1,'R'),(1,'L')]:
    plate('JACKET_Bolsillo_Bajo_'+tag,s*.119,-.086,1.052,.054,.081,.008,'JACKET','ivory',.07)
    box('JACKET_Bolsillo_Bajo_Borde_'+tag,(s*.119,-.092,1.081),(.053,.006,.007),'JACKET','sage',.001)
    panel_patch('JACKET_Pliegue_Cintura_'+tag,[(s*.145,-.061,1.13),(s*.142,-.077,1.105),(s*.082,-.091,1.114)],'JACKET','sage')
    tube('PACK_Tirante_'+tag,[(s*.112,-.095,1.291),(s*.135,-.099,1.37),(s*.150,-.054,1.418),(s*.150,.036,1.421),(s*.129,.105,1.368)],.010,'BACKPACK','dark',8)
    tube('PACK_Tirante_Pespunte_'+tag,[(s*.121,-.106,1.31),(s*.144,-.105,1.37),(s*.159,-.055,1.418),(s*.159,.035,1.421)],.0009,'BACKPACK','yellow',5)
    box('PACK_Tirante_Cierre_'+tag,(s*.124,-.113,1.328),(.025,.012,.034),'BACKPACK','metal',.003)

# Belt and continuous cargo trouser mesh. The branching crotch is sewn rather
# than made from an overlapping pelvis block and two intersecting leg tubes.
def trouser_half(sign,rows):
    n=16;vs=[];fs=[]
    for x,y,z,rx,ry in rows:
        for j in range(n):
            a=-math.pi/2+j*math.tau/n;c=math.cos(a);si=math.sin(a)
            vs.append((x+sign*rx*math.copysign(abs(c)**.8,c),y+ry*math.copysign(abs(si)**.8,si),z))
    # The shared inner arc forms a curved crotch seam at X=0; the outer arc
    # becomes one half of the hip circumference.
    trans=len(vs)
    for j in range(n):
        a=-math.pi/2+j*math.tau/n
        if j<=8:
            vs.append((sign*.148*math.cos(a),.081*math.sin(a),.915))
        else:
            vs.append((0,.081*math.sin(a),.915-.074*max(0,-math.cos(a))**.8))
    for i in range(len(rows)):
        a0=i*n;a1=(i+1)*n
        for j in range(n):fs.append((a0+j,a0+(j+1)%n,a1+(j+1)%n,a1+j))
    fs.append(tuple(reversed(range(n))))
    prev=[trans+j for j in range(9)]
    for z,rx,ry in [(.963,.142,.081),(1.022,.135,.079)]:
        indices=[]
        for j in range(9):
            a=-math.pi/2+j*math.pi/8
            indices.append(len(vs));vs.append((sign*rx*math.cos(a),ry*math.sin(a),z))
        for j in range(8):fs.append((prev[j],prev[j+1],indices[j+1],indices[j]))
        prev=indices
    fs.append(tuple(prev))
    return vs,fs
pants_vs=[];pants_fs=[]
loft('ACC_Cinturon',[(0,0,1.005,.137,.084),(0,0,1.042,.133,.081)],'ACCESSORIES','dark',20,2.8)
box('ACC_Hebilla_Exterior',(0,-.090,1.024),(.060,.017,.042),'ACCESSORIES','yellow',.003)
box('ACC_Hebilla_Centro',(0,-.102,1.024),(.040,.010,.030),'ACCESSORIES','charcoal',.002)
for s,tag in [(-1,'R'),(1,'L')]:
    rings=[(s*.105,.004,.198,.045,.049),(s*.107,.003,.228,.049,.054),(s*.111,.009,.246,.061,.062),(s*.107,.013,.278,.069,.068),(s*.119,.007,.316,.067,.071),(s*.122,.005,.38,.074,.073),(s*.116,-.007,.457,.069,.074),(s*.108,-.010,.527,.066,.075),(s*.108,-.002,.562,.073,.080),(s*.103,.010,.665,.079,.084),(s*.098,.005,.774,.082,.087),(s*.084,.004,.872,.078,.081),(s*.080,.003,.908,.073,.075),(s*.074,.003,.955,.070,.075),(s*.067,.002,1.015,.068,.073)]
    vpart,fpart=trouser_half(s,rings[:11]+[(s*.084,.004,.850,.078,.081)])
    start=len(pants_vs);pants_vs.extend(vpart);pants_fs.extend(tuple(start+j for j in f) for f in fpart)
    # Folds use the same textile material and a shallow modeled ridge.
    for idx,(cx,cy,cz,span) in enumerate([(s*.107,-.076,.55,.050),(s*.112,-.063,.305,.046),(s*.109,-.067,.79,.043)]):
        pts=[(cx-span,cy+.010,cz-.009),(cx,cy-.005,cz),(cx+span,cy+.011,cz+.018),(cx+span*.25,cy+.006,cz+.006)]
        mesh('PANTS_Pliegue_Modelado_'+str(idx)+'_'+tag,pts,[(0,1,3),(1,2,3)],'PANTS','slate')
    # External pockets are fabric shells with flaps and a second stacked pocket.
    box('PANTS_Cargo_Bolsa_'+tag,(s*.173,.003,.791),(.037,.108,.145),'PANTS','dark',.009)
    box('PANTS_Cargo_Tapa_'+tag,(s*.185,-.005,.845),(.042,.114,.039),'PANTS','charcoal',.004)
    box('PANTS_Cargo_Bolsa_Inferior_'+tag,(s*.165,-.012,.656),(.033,.102,.115),'PANTS','slate',.006)
    box('PANTS_Cargo_Bolsa_Inferior_Tapa_'+tag,(s*.177,-.018,.703),(.037,.106,.027),'PANTS','dark',.003)
    tube('PANTS_Costura_Exterior_'+tag,[(s*.154,.006,.32),(s*.186,.003,.65),(s*.177,.005,.875)],.0011,'PANTS','fold',5)
    tube('PANTS_Costura_Bolsillo_'+tag,[(s*.101,-.083,.921),(s*.151,-.075,.879),(s*.163,-.059,.85)],.0014,'PANTS','fold',5)
    ribbon('ACC_Cinta_Cinturon_'+tag,[(s*.139,-.053,1.034),(s*.157,-.063,.963),(s*.170,-.063,.876)],.017,'ACCESSORIES','yellow')
    box('ACC_Cinta_Pasador_'+tag,(s*.167,-.069,.889),(.027,.013,.038),'ACCESSORIES','metal',.003)
    # The ankle hems sit loose over the yellow-tongued work boots.
    panel_patch('PANTS_Pliegue_Tobillo_'+tag,[(s*.072,-.047,.244),(s*.156,-.049,.265),(s*.124,-.061,.285)],'PANTS','fold')
    # Boots, with shaped toe-box and alternating sole/upper panels.
    bx=s*.108;before=set(COL['BOOTS'].objects)
    loft('BOOT_Suela_'+tag,[(bx,-.041,0,.071,.134),(bx,-.043,.014,.075,.140),(bx,-.043,.039,.076,.141),(bx,-.042,.050,.071,.134)],'BOOTS','dark',14,3.5)
    loft('BOOT_Media_Suela_'+tag,[(bx,-.039,.039,.073,.136),(bx,-.039,.061,.073,.135)],'BOOTS','boot_light',14,3.5)
    loft('BOOT_Cuerpo_'+tag,[(bx,-.039,.056,.069,.130),(bx,-.038,.087,.068,.129),(bx,-.026,.112,.064,.112),(bx,.004,.15,.054,.067),(bx,.009,.202,.047,.051)],'BOOTS','boot_light',14,3)
    loft('BOOT_Puntera_Grafito_'+tag,[(bx,-.124,.052,.066,.051),(bx,-.125,.077,.065,.049),(bx,-.118,.100,.056,.043)],'BOOTS','dark',12,3.5)
    box('BOOT_Talon_'+tag,(bx,.070,.082),(.117,.041,.080),'BOOTS','charcoal',.009)
    loft('BOOT_Cuello_'+tag,[(bx,.010,.159,.051,.057),(bx,.010,.208,.051,.057),(bx,.010,.219,.044,.049)],'BOOTS','dark',14,2.6)
    tongue=box('BOOT_Lengueta_Amarilla_'+tag,(bx,-.045,.195),(.062,.027,.078),'BOOTS','yellow',.006)
    box('BOOT_Lengueta_Parche_'+tag,(bx,-.061,.213),(.027,.004,.012),'BOOTS','metal',.002)
    for i in range(3):
        y=-.106+i*.020;z=.113+i*.016
        bar=box('BOOT_Correa_'+str(i)+'_'+tag,(bx,y,z),(.091,.018,.012),'BOOTS','slate',.003)
        bar.rotation_euler.x=math.radians(26)
        for dx in [-.037,.037]:
            cyl('BOOT_Ojal_'+str(i)+'_'+str(dx)+'_'+tag,(bx+dx,y-.009,z),.004,.004,'BOOTS','yellow',(0,-1,0),8)
    box('BOOT_Refuerzo_Lateral_'+tag,(bx+s*.059,-.003,.115),(.016,.084,.042),'BOOTS','sage',.005)
    T=Matrix.Translation((bx,0,0))@Matrix.Rotation(s*math.radians(6),4,'Z')@Matrix.Translation((-bx,0,0))
    for part in set(COL['BOOTS'].objects)-before:part.matrix_world=T@part.matrix_world

pants=mesh('PANTS_Pantalon_Continuo',pants_vs,pants_fs,'PANTS','slate',True)
bm=bmesh.new();bm.from_mesh(pants.data)
bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
bm.to_mesh(pants.data);bm.free();pants.data.update()
pants['design']='Dos perneras y cadera cosidas en una malla continua, con entrepierna propia'

# Project identification card and clip, deliberately independent pieces.
box('ACC_Credencial_Clip',(.098,-.111,1.374),(.017,.014,.026),'ACCESSORIES','metal',.002)
tube('ACC_Credencial_Cinta',[(.098,-.108,1.374),(.099,-.117,1.350)],.0038,'ACCESSORIES','dark',6)
plate('ACC_Credencial_Funda',.099,-.118,1.310,.058,.089,.009,'ACCESSORIES','dark',.04)
plate('ACC_Credencial_Tarjeta',.099,-.124,1.310,.051,.081,.003,'ACCESSORIES','ivory',.02)
tri=[(.089,-.127,1.322),(.109,-.127,1.322),(.099,-.127,1.342)]
poly('ACC_Credencial_Logo',tri,'ACCESSORIES','charcoal',.001)
for i,w in enumerate([.030,.022,.026]):box('ACC_Credencial_Linea_'+str(i),(.099,-.127,1.307-i*.007),(w,.001,.0015),'ACCESSORIES','charcoal',.0001)
box('ACC_Credencial_Foto',(.087,-.127,1.281),(.009,.001,.011),'ACCESSORIES','sage',.0002)

# Compact black field pack — distinct from the explorer's large life-support unit.
box('PACK_Acolchado',(0,.108,1.274),(.253,.063,.295),'BACKPACK','charcoal',.026)
box('PACK_Carcasa',(0,.154,1.280),(.266,.111,.304),'BACKPACK','dark',.026)
plate('PACK_Tapa_Posterior',0,.216,1.282,.220,.252,.015,'BACKPACK','charcoal',.08)
box('PACK_Bolsillo_Inferior',(0,.230,1.182),(.194,.029,.053),'BACKPACK','dark',.008)
tube('PACK_Cremallera',[(-.097,.229,1.184),(-.104,.228,1.205),(-.104,.226,1.374),(-.085,.225,1.401),(.085,.225,1.401),(.104,.226,1.374),(.104,.228,1.205)],.0022,'BACKPACK','metal',6)
box('PACK_Cremallera_Tirador',(.100,.237,1.291),(.006,.005,.027),'BACKPACK','yellow',.001)
badge('PACK_Emblema',(0,.232,1.316),.044,(0,1,0),'BACKPACK')
tube('PACK_Asa',[(-.043,.127,1.424),(-.034,.127,1.449),(.034,.127,1.449),(.043,.127,1.424)],.008,'BACKPACK','dark',8)
for s,tag in [(-1,'R'),(1,'L')]:
    box('PACK_Bolsillo_Lateral_'+tag,(s*.143,.151,1.247),(.041,.086,.142),'BACKPACK','charcoal',.009)
    box('PACK_Presilla_Lateral_'+tag,(s*.165,.160,1.266),(.009,.066,.020),'BACKPACK','yellow',.002)

# Arm construction in two authoring poses. No bones or animation are introduced.
def armhole_indices(sign):
    # Both loops face outward, follow the quad-grid boundary, and share vertices
    # with the torso. Their top runs into the clavicle/trapezius region.
    a,b=(3,9) if sign>0 else (17,23)
    bottom,top=5,9
    return ([bottom*(JN+1)+j for j in range(a,b+1)]
            +[i*(JN+1)+b for i in range(bottom+1,top+1)]
            +[top*(JN+1)+j for j in range(b-1,a-1,-1)]
            +[i*(JN+1)+a for i in range(top-1,bottom,-1)])

def append_connected_sleeve(sign,E,F,end,gv,gf,gm):
    indices=armhole_indices(sign)
    center=sum((Vector(gv[i]) for i in indices),Vector())/len(indices)
    n0=Vector((sign,0,0));u0=Vector((0,-sign,0));v0=n0.cross(u0)
    angles=[]
    for idx in indices:
        d=Vector(gv[idx])-center;a=math.atan2(d.dot(v0),d.dot(u0))
        if angles:
            while a<=angles[-1]:a+=math.tau
        angles.append(a)
    upper=(E-Vector((sign*.191,0,1.405))).normalized()
    rows=[
      (Vector((sign*.192,-.001,1.371)),Vector((sign*.97,-.01,-.24)),.066,.060),
      (Vector((sign*.212,-.003,1.340)),Vector((sign*.48,-.03,-.88)),.063,.057),
      (Vector((sign*.215,-.010,1.291)),upper,.059,.057),
      (E-upper*.027,upper,.057,.055),
      (E+F*.014,(upper+F).normalized(),.052,.050),
      (end,F,.048,.047),
    ]
    prev=indices;previous_n=n0;previous_u=u0;seam=None
    for row,(p,normal,ru,rv) in enumerate(rows):
        normal.normalize()
        u=previous_n.rotation_difference(normal)@previous_u
        u=(u-normal*u.dot(normal)).normalized();v=normal.cross(u).normalized()
        nxt=[]
        for a in angles:
            nxt.append(len(gv));gv.append(tuple(p+u*ru*math.cos(a)+v*rv*math.sin(a)))
        for j in range(len(prev)):
            face=(prev[j],prev[(j+1)%len(prev)],nxt[(j+1)%len(prev)],nxt[j])
            gf.append(face)
            gm.append(1 if row<2 else 0)
        if row==1:seam=([Vector(gv[k]) for k in nxt],p.copy())
        prev=nxt;previous_n=normal;previous_u=u
    return seam

def yoke_stitch(coat,seam,tag,col,sign):
    points,center=seam
    start=max(range(len(points)),key=lambda i:points[i].y)
    points=points[start:]+points[:start]
    bpy.context.view_layer.update();ev=coat.evaluated_get(bpy.context.evaluated_depsgraph_get())
    path=[]
    for point in points:
        if point.y>-.015 or sign*point.x<.195:continue
        radial=(point-center).normalized()
        ok,p,no,idx=ev.ray_cast(point+radial*.05,-radial)
        if ok and (p-point).length<.014:path.append(p+no*.0015)
    if len(path)>2:tube('ARM_Costura_Canesu_'+tag,path,.0007,col,'sage',5)

def textile_badge(coat,sign,tag,col):
    # Project every patch vertex to the evaluated sleeve: no rigid floating disc.
    bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();ev=coat.evaluated_get(dg)
    out=Vector((sign*.97,-.23,.05)).normalized()
    guess=Vector((sign*.265,-.020,1.352))
    ok,hit,normal,idx=ev.ray_cast(guess+out*.14,-out)
    if not ok:raise RuntimeError('No se pudo apoyar el parche en la manga '+tag)
    u,v,n=local_frame(normal);radius=.0255
    def project(point,lift=.0015):
        ok,p,no,idx=ev.ray_cast(Vector(point)+out*.12,-out)
        if not ok:raise RuntimeError('Parche fuera de la superficie '+tag)
        return p+no*lift
    center=project(hit)
    disc=[center]+[project(hit+radius*(u*math.cos(j*math.tau/32)+v*math.sin(j*math.tau/32))) for j in range(32)]
    patch=mesh('ARM_Parche_Tejido_Base_'+tag,disc,[(0,j+1,(j+1)%32+1) for j in range(32)],col,'dark',True)
    ring=[]
    for r in [.78*radius,.91*radius]:
        ring.extend(project(hit+r*(u*math.cos(j*math.tau/32)+v*math.sin(j*math.tau/32)),.002) for j in range(32))
    mesh('ARM_Parche_Tejido_Aro_'+tag,ring,[(j,(j+1)%32,(j+1)%32+32,j+32) for j in range(32)],col,'yellow',True)
    shape=[(-.40,-.31),(.04,.46),(.43,-.14),(.10,-.09),(-.19,-.25)]
    tube('ARM_Parche_Tejido_Marca_'+tag,[project(hit+radius*(u*a+v*b),.0028) for a,b in shape],.0016,col,'ivory',6)
    tube('ARM_Parche_Tejido_Costura_'+tag,[project(hit+radius*(u*math.cos(j*math.tau/32)+v*math.sin(j*math.tau/32)),.002) for j in range(33)],.00045,col,'sage',4)
    patch['design']='Parche de tela conformado a la superficie evaluada de la manga'

def finish_coat(gv,gf,gm,col,suffix):
    coat=mesh('JACKET_Chaqueta_Sisas_Continuas'+suffix,gv,gf,col,'ivory',True)
    coat.data.materials.append(M['dark'])
    for p,material in zip(coat.data.polygons,gm):p.material_index=material
    bm=bmesh.new();bm.from_mesh(coat.data)
    loose=[v for v in bm.verts if not v.link_faces]
    if loose:bmesh.ops.delete(bm,geom=loose,context='VERTS')
    # Cut real diagonal yoke boundary edges instead of stepping material
    # assignments over the sleeve grid. Both sides follow the shoulder slope.
    for sign in [-1,1]:
        bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=.000001,plane_co=(0,0,1.410),plane_no=Vector((sign*.25,0,1)).normalized(),clear_inner=False,clear_outer=False)
    for face in bm.faces:
        center=face.calc_center_median()
        face.material_index=1 if center.z+.25*abs(center.x)>1.410 else 0
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(coat.data);bm.free();coat.data.update()
    # Hold the open front/hem shape without hardening the sewn shoulder loops.
    usage={}
    for p in coat.data.polygons:
        for edge in p.edge_keys:usage[edge]=usage.get(edge,0)+1
    crease=coat.data.attributes.new('crease_edge','FLOAT','EDGE')
    for edge in coat.data.edges:
        crease.data[edge.index].value=.85 if usage.get(tuple(sorted(edge.vertices)),0)==1 else 0
    sub=coat.modifiers.new('Suavizado_Prenda_Continuo','SUBSURF');sub.levels=1;sub.render_levels=1
    solid(coat,.003)
    coat['design']='Torso y copas de manga unidos por bucles de sisa; sin tapas solapadas'
    coat['armhole_boundary_vertices']=len(armhole_indices(1))
    return coat

def build_arms(col,reading):
    suffix='' if reading else '_A'
    gv,gf,gm=(list(x) for x in jacket_template)
    for s,tag in [(-1,'R'),(1,'L')]:
        S=Vector((s*.191,0,1.405));E=Vector((s*.229,-.025,1.171)) if reading else Vector((s*.255,0,1.171))
        W=Vector((s*.155,-.247,1.165)) if reading else Vector((s*.314,-.012,.950))
        U=(E-S).normalized();F=(W-E).normalized()
        end=E+F*.040
        append_connected_sleeve(s,E,F,end,gv,gf,gm)
        # Rolled hem, with a lip and inward dark shadow around the opening.
        sweep('ARM_Manga_Rollo_'+tag+suffix,[(end-F*.011,.051,.049),(end,.055,.052),(end+F*.013,.051,.048)],col,'sage',14,2.2)
        sweep('ARM_Manga_Rollo_Luz_'+tag+suffix,[(end-F*.003,.053,.050),(end+F*.007,.054,.051)],col,'ivory',14,2.2)
        sweep('ARM_Antebrazo_'+tag+suffix,[(end-F*.003,.038,.035),(E+F*.09,.037,.033),(W-F*.028,.027,.024),(W,.025,.022)],col,'skin',14,2)
        # Cloth fold planes follow the elbow instead of added armor.
        p=E-U*.054
        tube('ARM_Pliegue_Codo_'+tag+suffix,[p+Vector((s*.047,-.026,0)),p+Vector((s*.010,-.059,.005)),p+Vector((-s*.033,-.035,.013))],.0017,col,'sage',5)
        sweep('GLOVE_Correa_'+tag+suffix,[(W-F*.026,.030,.027),(W-F*.011,.031,.028),(W+F*.007,.029,.026)],col,'charcoal',12,2.5)
        if reading:
            # Palms sit behind the tablet; fingertips curl around its side bevel.
            palm=ellipsoid('GLOVE_Palma_'+tag+suffix,(s*.157,-.278,1.153),(.026,.037,.021),col,'dark',14,7)
            palm.rotation_euler.x=math.radians(-35)
            for j in range(4):
                z=1.121+j*.018;y=-.304+(z-1.15)*.700
                tipx=[.130,.121,.125,.134][j]
                p0=(s*.172,y+.012,z-.006);p1=(s*.145,y-.006,z);p2=(s*tipx,y-.011,z+.005)
                tube('GLOVE_Dedo_Base_'+str(j)+'_'+tag+suffix,[p0,p1],.009,col,'dark',7)
                tube('HAND_Dedo_Punta_'+str(j)+'_'+tag+suffix,[p1,p2],.0079,col,'skin',7)
                ellipsoid('HAND_Yema_'+str(j)+'_'+tag+suffix,p2,(.0068,.006,.0072),col,'skin',10,5)
                tube('HAND_Una_'+str(j)+'_'+tag+suffix,[(s*.134,y-.017,z+.007),(s*.124,y-.017,z+.009)],.004,col,'skin_light',5)
            tube('GLOVE_Pulgar_Base_'+tag+suffix,[(s*.145,-.263,1.174),(s*.136,-.260,1.195)],.011,col,'dark',8)
            tube('HAND_Pulgar_Punta_'+tag+suffix,[(s*.136,-.260,1.195),(s*.124,-.269,1.204)],.009,col,'skin',8)
            patch=box('GLOVE_Refuerzo_Dorso_'+tag+suffix,(s*.168,-.287,1.144),(.027,.035,.009),col,'charcoal',.004)
            patch.rotation_euler.x=math.radians(-35)
        else:
            P=W+F*.041
            ellipsoid('GLOVE_Palma_'+tag+suffix,P,(.034,.025,.043),col,'dark',14,7)
            for j in range(4):
                x=P.x+s*(j-1.5)*.014;z=P.z-.024
                tube('GLOVE_Dedo_Base_'+str(j)+'_'+tag+suffix,[(x,-.016,z),(x+s*.003,-.020,z-.020)],.0085,col,'dark',7)
                tube('HAND_Dedo_Punta_'+str(j)+'_'+tag+suffix,[(x+s*.003,-.020,z-.020),(x+s*.005,-.021,z-.040+abs(j-1.5)*.004)],.0076,col,'skin',7)
            tube('GLOVE_Pulgar_Base_'+tag+suffix,[(P.x-s*.025,-.009,P.z+.01),(P.x-s*.038,-.015,P.z-.012)],.011,col,'dark',8)
            tube('HAND_Pulgar_Punta_'+tag+suffix,[(P.x-s*.038,-.015,P.z-.012),(P.x-s*.040,-.025,P.z-.028)],.009,col,'skin',8)
    coat=finish_coat(gv,gf,gm,col,suffix)
    for s,tag in [(-1,'R'),(1,'L')]:textile_badge(coat,s,tag+suffix,col)
build_arms('ARMS_LECTURA',True)

# Detailed head module is delivered beside this script and packed as a Text block.
spec=importlib.util.spec_from_file_location('cabeza_investigador',HERE/'cabeza_investigador.py')
head_module=importlib.util.module_from_spec(spec);spec.loader.exec_module(head_module)
head_module.build_head({'mesh':mesh,'ellipsoid':ellipsoid,'tube':tube,'box':box})

# Glass tablet: editable screen and UI marks, not a texture or image replacement.
tab_center=Vector((0,-.295,1.168));tab_angle=math.radians(-35)
tab_T=Matrix.Translation(tab_center)@Matrix.Rotation(tab_angle,4,'X')
def tablet_local(obj):
    obj.matrix_world=tab_T@obj.matrix_world
    return obj
tablet_local(plate('TABLET_Marco',0,0,0,.293,.197,.016,'TABLET','charcoal',.075))
tablet_local(plate('TABLET_Cristal',0,-.011,.001,.268,.171,.005,'TABLET','screen',.035))
tablet_local(box('TABLET_Borde_Superior',(0,-.015,.078),(.229,.001,.003),'TABLET','metal',.0003))
tablet_local(box('TABLET_Camara',(.116,-.015,.081),(.006,.002,.006),'TABLET','dark',.001))
# Blueprint interface: a few legible graphs, small cell diagrams and headings.
for i,(x,z,w,h) in enumerate([(-.074,.009,.085,.108),(.028,.025,.087,.073),(.074,-.050,.080,.029)]):
    coords=[(x-w/2,-.016,z-h/2),(x+w/2,-.016,z-h/2),(x+w/2,-.016,z+h/2),(x-w/2,-.016,z+h/2),(x-w/2,-.016,z-h/2)]
    tablet_local(tube('TABLET_UI_Panel_'+str(i),coords,.0007,'TABLET','cyan',4))
for i in range(5):
    tablet_local(box('TABLET_UI_Barra_'+str(i),(-.107+i*.012,-.017,-.030+i*.002),(.005,.001,.025+i*.007),'TABLET','cyan',.0001))
for i in range(4):
    z=.052-i*.009
    tablet_local(box('TABLET_UI_Linea_'+str(i),(.031,-.017,z),(.058-(i%2)*.012,.001,.0015),'TABLET','cyan',.0001))
tablet_local(tube('TABLET_UI_Onda',[(.040,-.017,-.051),(.049,-.017,-.051),(.055,-.017,-.040),(.061,-.017,-.058),(.071,-.017,-.047),(.085,-.017,-.050),(.108,-.017,-.050)],.001,'TABLET','cyan',5))
for r in [.011,.021,.029]:
    coords=[(-.069+r*math.cos(j*math.tau/16),-.017,.022+r*.7*math.sin(j*math.tau/16)) for j in range(17)]
    tablet_local(tube('TABLET_UI_Celula_'+str(r),coords,.0007,'TABLET','cyan',4))
tablet_local(box('TABLET_UI_Encabezado',(-.062,-.017,.071),(.119,.001,.003),'TABLET','cyan',.0002))

# Packed reference and scene-only studio setup.
refpath=ROOT/'referencias'/'personajes'/'investigador-concepto.png'
if not refpath.exists():raise FileNotFoundError('Falta referencias/personajes/investigador-concepto.png en la entrega')
img=bpy.data.images.load(str(refpath),check_existing=True);img.pack()
ref=bpy.data.objects.new('REF_Investigador_Concepto',None);ref.empty_display_type='IMAGE';ref.data=img;COL['REFERENCES'].objects.link(ref)
ref.empty_display_size=1.78;ref.location=(2.0,.6,.89);ref.rotation_euler=(math.pi/2,0,0);ref.hide_render=True
COL['REFERENCES'].hide_viewport=True;COL['REFERENCES'].hide_render=True
studio=mat('MAT_Studio_Investigador',(.024,.033,.042),.95)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.002))
o=bpy.context.object;o.name='STUDIO_Suelo';link(o,'PRESENTATION');o.data.materials.append(studio)
def camera(name,loc,target,scale):
    d=bpy.data.cameras.new(name);o=bpy.data.objects.new(name,d);COL['PRESENTATION'].objects.link(o)
    o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=scale;d.clip_start=.01;d.clip_end=100
    return o
cams={
 'front':camera('CAM_Frente',(0,-5,.89),(0,0,.89),2.02),
 'side':camera('CAM_Lateral',(5,0,.89),(0,0,.89),2.02),
 'back':camera('CAM_Posterior',(0,5,.89),(0,0,.89),2.02),
 'hero':camera('CAM_Tres_Cuartos',(3.1,-6,2.65),(0,-.025,.91),2.02),
 'head':camera('CAM_Rostro',(1.45,-4,1.95),(0,-.015,1.633),.39),
 'head_front':camera('CAM_Rostro_Frontal',(0,-4,1.65),(0,0,1.65),.33),
 'head_side':camera('CAM_Rostro_Perfil',(4,0,1.645),(0,0,1.645),.36),
 'shoulders':camera('CAM_Hombros',(1.8,-4,2.1),(0,0,1.358),.75),
 'shoulders_front':camera('CAM_Hombros_Frente',(0,-4,1.36),(0,0,1.36),.72),
 'tablet':camera('CAM_Manos_Tableta',(1.4,-3.7,2.1),(0,-.20,1.17),.57),
}
def area(name,loc,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);COL['PRESENTATION'].objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,1.0))-o.location).to_track_quat('-Z','Y').to_euler()
area('LIGHT_Key',(-2.5,-4,4.8),420,3,(1,.88,.76))
area('LIGHT_Fill',(3,-3,3),230,3,(.70,.83,1))
area('LIGHT_Rim',(1,2.5,4),500,2.5,(.84,.90,1))
world=bpy.data.worlds.new('WORLD_Investigador');world.use_nodes=True;scene.world=world
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.055,.065,.08,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.45
def render_settings(sc):
    sc.world=world;sc.render.engine='CYCLES';sc.cycles.samples=48;sc.cycles.use_denoising=True
    sc.render.resolution_x=850;sc.render.resolution_y=1100;sc.render.resolution_percentage=100
    sc.render.image_settings.file_format='PNG';sc.render.image_settings.color_mode='RGBA';sc.view_settings.view_transform='AgX'
render_settings(scene);scene.camera=cams['hero']

# A relaxed pose variant in a separate scene shares the static body/head geometry.
technical=bpy.data.scenes.new('Investigador_Pose_A')
technical.unit_settings.system='METRIC';technical.unit_settings.scale_length=1;technical.unit_settings.length_unit='METERS'
for name,c in COL.items():
    if name not in ['ARMS_LECTURA','TABLET']:technical.collection.children.link(c)
ac=bpy.data.collections.new('ARMS_A');technical.collection.children.link(ac);COL['ARMS_A']=ac
bpy.context.window.scene=technical
build_arms('ARMS_A',False)
render_settings(technical);technical.camera=cams['front']
technical['purpose']='Variante geometrica editable de brazos separados; sin rig ni animaciones. Tableta en escena principal.'
for o in COL['PRESENTATION'].objects:o.hide_set(True)
for o in COL['GLASSES'].objects:
    if o.name.startswith('GLASS_Lente_'):o.hide_set(True)
bpy.context.window.scene=scene
for o in COL['PRESENTATION'].objects:o.hide_set(True)
for o in COL['GLASSES'].objects:
    if o.name.startswith('GLASS_Lente_'):o.hide_set(True)
root['joints_m']=json.dumps({'shoulder_L':[.191,0,1.405],'elbow_A_L':[.255,0,1.171],'wrist_A_L':[.314,-.012,.950],'elbow_reading_L':[.229,-.025,1.171],'wrist_reading_L':[.155,-.247,1.165],'hip_L':[.084,0,.872],'knee_L':[.108,0,.55],'ankle_L':[.108,0,.20],'neck':[0,0,1.50],'R':'invert X'})
scene['limits']='Blockout con materiales planos. Sin texturizado, rig, animacion ni retopologia final. Piezas segmentadas e intersecciones de montaje intencionales.'

# Actual evaluated vertex bounds, plus per-object triangle inventory for both poses.
def statistics(sc):
    bpy.context.window.scene=sc;bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get()
    obs=[o for o in sc.objects if o.type=='MESH' and not any(c==COL['PRESENTATION'] for c in o.users_collection)]
    coords=[];stats=[]
    for o in obs:
        o.data.calc_loop_triangles();e=o.evaluated_get(dg);me=e.to_mesh();me.calc_loop_triangles()
        coords.extend(e.matrix_world@v.co for v in me.vertices)
        stats.append({'name':o.name,'collection':o.users_collection[0].name,'vertices':len(o.data.vertices),'base_triangles':len(o.data.loop_triangles),'evaluated_triangles':len(me.loop_triangles),'modifiers':[m.type for m in o.modifiers]})
        e.to_mesh_clear()
    lo=[min(p[i] for p in coords) for i in range(3)];hi=[max(p[i] for p in coords) for i in range(3)]
    collections={}
    for row in stats:
        item=collections.setdefault(row['collection'],{'mesh_objects':0,'base_triangles':0,'evaluated_triangles':0})
        item['mesh_objects']+=1;item['base_triangles']+=row['base_triangles'];item['evaluated_triangles']+=row['evaluated_triangles']
    return {'scene':sc.name,'mesh_objects':len(obs),'base_triangles':sum(s['base_triangles'] for s in stats),'evaluated_triangles':sum(s['evaluated_triangles'] for s in stats),'base_vertices':sum(s['vertices'] for s in stats),'min_m':lo,'max_m':hi,'dimensions_m':[hi[i]-lo[i] for i in range(3)],'collections':collections,'objects':stats}
report={'project':'School Redemption Game','character':'El Investigador','phase':'Refinamiento de cara y hombros v03','blender':bpy.app.version_string,'reading':statistics(scene),'a_pose':statistics(technical),'reference_packed':bool(img.packed_file)}
report['checks']={'feet_z0':abs(report['reading']['min_m'][2])<1e-5,'height_1_78m':abs(report['reading']['max_m'][2]-1.78)<.004,'reference_packed':bool(img.packed_file),'no_rig':not any(o.type=='ARMATURE' for o in scene.objects),'no_animation':not any(o.animation_data for o in scene.objects)}
(DATA/'investigador-informe.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
bpy.context.window.scene=scene
for o in bpy.context.selected_objects:o.select_set(False)
root.select_set(True);bpy.context.view_layer.objects.active=root
for screen in bpy.data.screens:
    for a in screen.areas:
        if a.type=='VIEW_3D':
            a.spaces.active.region_3d.view_rotation=cams['hero'].rotation_euler.to_quaternion();a.spaces.active.region_3d.view_distance=2.85;a.spaces.active.region_3d.view_location=(0,0,.9);a.spaces.active.region_3d.view_perspective='ORTHO';a.spaces.active.shading.type='MATERIAL';a.spaces.active.overlay.show_relationship_lines=False
for filename in ['crear_investigador.py','cabeza_investigador.py']:
    text=bpy.data.texts.new(filename);text.write((HERE/filename).read_text(encoding='utf8'))
readme=bpy.data.texts.new('LEEME_Investigador')
readme.write('EL INVESTIGADOR / SCHOOL REDEMPTION GAME\nEscena Investigador_Blockout: tableta sostenida y accesorios de la lamina.\nEscena Investigador_Pose_A: variante de brazos separados sin rig.\nFrente -Y; altura1.78m; pies Z=0.\nReferencias empaquetadas; activar REFERENCES en el Outliner.\nSin UV, texturas, rig ni animaciones. Materiales planos y geometria independiente.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'investigador.blend'))
print('INVESTIGADOR_REPORT',json.dumps({k:report[k] for k in ['checks']}))
print('INVESTIGADOR_COUNTS',report['reading']['mesh_objects'],report['reading']['base_triangles'],report['reading']['evaluated_triangles'],report['reading']['dimensions_m'])
if '--render' in ARGS:
    views=ARGS[ARGS.index('--views')+1].split(',') if '--views' in ARGS else list(cams)
    for view,cam in cams.items():
        if view not in views:continue
        scene.camera=cam;scene.render.filepath=str(WORK/f'preview_{view}.png')
        if view in ['head','head_front','head_side','shoulders','shoulders_front','tablet']:scene.render.resolution_x=1000;scene.render.resolution_y=1000
        else:scene.render.resolution_x=850;scene.render.resolution_y=1100
        bpy.ops.render.render(write_still=True,scene=scene.name)
    scene.camera=cams['hero'];scene.render.resolution_x=850;scene.render.resolution_y=1100
    bpy.context.window.scene=technical;technical.render.filepath=str(WORK/'preview_a_pose.png')
    bpy.ops.render.render(write_still=True,scene=technical.name)
    bpy.context.window.scene=scene
print('INVESTIGADOR_COMPLETE',str(OUT/'investigador.blend'))
