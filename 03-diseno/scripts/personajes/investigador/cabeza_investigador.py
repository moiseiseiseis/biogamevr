"""Cabeza editable del Investigador, School Redemption Game.

Submodule consumed by crear_investigador.py. Coordinates are in meters.
Front is -Y. Chin 1.535 m; eye line 1.652 m; hair silhouette 1.780 m.
No external images, displacement, armature or baked geometry are used.
"""
import math
from mathutils import Vector


def build_head(api):
    """Build a youthful angular face, broad swept hair locks and fine glasses.

    api provides mesh, ellipsoid, tube and box helpers. Material keys used:
    skin, skin_shadow, skin_light, hair, hair_hi, charcoal, eye_white, iris,
    pupil, lens, metal, lip. Collections: HEAD, HAIR, GLASSES.
    """
    made = []

    def mesh(name, verts, faces, col, material, smooth=False):
        # Close the shared corner of eyelid strips without zero-area quads.
        if name.startswith('HEAD_Parpado_Piel_'):
            clean=[];mapping={};remap={}
            for i,v in enumerate(verts):
                key=tuple(round(float(a),9) for a in v)
                if key not in mapping:
                    mapping[key]=len(clean);clean.append(v)
                remap[i]=mapping[key]
            cleaned=[]
            for f in faces:
                seq=[]
                for k in f:
                    j=remap[k]
                    if not seq or seq[-1]!=j:seq.append(j)
                if len(seq)>1 and seq[-1]==seq[0]:seq.pop()
                if len(set(seq))>=3:cleaned.append(tuple(seq))
            verts,faces=clean,cleaned
        obj = api['mesh'](name, verts, faces, col, material, smooth)
        made.append(obj)
        return obj

    def ellipsoid(name, loc, radii, col, material, seg=16, rings=8):
        obj = api['ellipsoid'](name, loc, radii, col, material, seg=seg, rings=rings)
        made.append(obj)
        return obj

    def tube(name, points, radius, col, material, sides=8):
        obj = api['tube'](name, points, radius, col, material, sides=sides)
        made.append(obj)
        return obj

    # Anatomical surface: uniform front samples preserve bridge, nose, sockets
    # and lip transition when subdivision is evaluated. The old angular
    # superellipse had almost no vertices across the central nose column.
    profiles = [
        (1.535,.015,.027,-.008),(1.541,.027,.038,-.003),
        (1.550,.042,.048,.001),(1.561,.055,.056,.004),
        (1.574,.067,.064,.007),(1.587,.074,.068,.009),
        (1.598,.079,.071,.010),(1.607,.082,.073,.011),
        (1.612,.084,.074,.011),(1.617,.085,.075,.011),
        (1.622,.086,.076,.011),(1.629,.087,.078,.011),
        (1.637,.088,.080,.011),(1.645,.088,.081,.011),
        (1.652,.088,.082,.011),(1.660,.088,.083,.011),
        (1.668,.088,.083,.011),(1.677,.088,.083,.012),
        (1.690,.086,.082,.012),(1.706,.082,.079,.013),
        (1.721,.074,.072,.014),(1.735,.056,.056,.014),
        (1.747,.020,.027,.012),
    ]
    power = 2.18

    def profile(z):
        if z<=profiles[0][0]:return profiles[0][1:]
        if z>=profiles[-1][0]:return profiles[-1][1:]
        for a,b in zip(profiles,profiles[1:]):
            if a[0]<=z<=b[0]:
                t=(z-a[0])/(b[0]-a[0])
                return tuple(a[k]*(1-t)+b[k]*t for k in range(1,4))

    def g(x,z,cx,cz,sx,sz):
        return math.exp(-((x-cx)/sx)**2-((z-cz)/sz)**2)

    def relief(x,z):
        # Negative is toward the viewer. All facial pieces reuse this surface.
        nose=-(.022*g(x,z,0,1.618,.0105,.010)
               +.009*g(x,z,0,1.641,.0075,.023)
               +.0040*g(x,z,.009,1.613,.0045,.0045)
               +.0040*g(x,z,-.009,1.613,.0045,.0045))
        sockets=.0032*(g(x,z,.043,1.652,.025,.015)
                       +g(x,z,-.043,1.652,.025,.015))
        cheek=-.0030*(g(x,z,.055,1.623,.024,.014)
                     +g(x,z,-.055,1.623,.024,.014))
        brow=-.0020*(g(x,z,.041,1.675,.028,.008)
                    +g(x,z,-.041,1.675,.028,.008))
        muzzle=-.0018*g(x,z,0,1.588,.026,.012)
        chin=-.0018*g(x,z,0,1.551,.026,.010)
        philtrum=.0006*g(x,z,0,1.600,.0028,.006)
        return nose+sockets+cheek+brow+muzzle+chin+philtrum

    def face_y(x,z):
        rx,ry,cy=profile(z)
        q=min(.999999,abs(x)/rx)
        return cy-ry*(1-q**power)**(1/power)+relief(x,z)

    nfront,nback=41,23
    n=nfront+nback
    verts=[]
    for z,rx,ry,cy in profiles:
        # Front is sampled by X rather than polar angle: ~4mm spacing at eyes.
        for j in range(nfront):
            x=rx*(-1+2*j/(nfront-1))
            verts.append((x,face_y(x,z),z))
        for j in range(1,nback+1):
            a=math.pi*j/(nback+1)
            verts.append((rx*math.cos(a),cy+ry*math.sin(a),z))
    faces=[]
    for i in range(len(profiles)-1):
        for j in range(n):faces.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
    faces += [tuple(reversed(range(n))),tuple((len(profiles)-1)*n+j for j in range(n))]
    head=mesh('HEAD_Rostro_Anatomia_Refinada',verts,faces,'HEAD','skin',True)
    mod=head.modifiers.new('Suavizado_Facial_Editable','SUBSURF')
    mod.subdivision_type='CATMULL_CLARK';mod.levels=1;mod.render_levels=1
    head['design']='Puente y punta nasal, cuencas, pomulos, mandibula y menton integrados; frente muestreado uniformemente'

    # Sample the evaluated skin once it exists: feature ribbons then sit on
    # the actual subdivided cheek/muzzle rather than a nearby analytic proxy.
    import bpy
    analytic_face_y=face_y
    bpy.context.view_layer.update()
    from mathutils.bvhtree import BVHTree
    skin_bvh=BVHTree.FromObject(head,bpy.context.evaluated_depsgraph_get())
    def face_y(x,z):
        point,normal,index,distance=skin_bvh.ray_cast(Vector((x,-.30,z)),Vector((0,1,0)),.65)
        return point.y if point is not None else analytic_face_y(x,z)

    # Discreet human pinna: a small, irregular oval body partly buried in the
    # side of the skull, with an open C-shaped helix. No complete thick ring
    # or dark oval insert remains to read as an earpiece/medallion.
    for s,side in [(-1,'R'),(1,'L')]:
        cx,cy,cz=.0875,.006,1.627
        rx,ry,rz=.0100,.0120,.0195
        ear=ellipsoid('HEAD_Oreja_Pabellon_'+side,(s*cx,cy,cz),(rx,ry,rz),'HEAD','skin',24,12)
        # Narrower lobule and a slightly rearward upper rim break the ellipse.
        for v in ear.data.vertices:
            normalized_z=v.co.z/rz
            v.co.y*=.87+.13*max(0,normalized_z)
            if normalized_z<-.30:v.co.y-=.0011*(-normalized_z)
        def ear_surface(y,z,push=0):
            zn=(z-cz)/rz
            factor=.87+.13*max(0,zn)
            shift=.0011*(-zn) if zn<-.30 else 0
            yr=(y-cy+shift)/factor
            q=(yr/ry)**2+zn**2
            x=cx+rx*math.sqrt(max(.03,1-q))+push
            return (s*x,y,z)
        helix=[]
        for deg in [140,120,95,70,45,20,-5,-30,-55,-78]:
            a=math.radians(deg)
            y=cy+ry*.77*math.cos(a)
            z=cz+rz*.80*math.sin(a)
            helix.append(ear_surface(y,z,.00030))
        tube('HEAD_Oreja_Helix_Fino_'+side,helix,.00065,'HEAD','skin',6)
        anti=[ear_surface(y,z,.00015) for y,z in [(.006,1.617),(.004,1.624),(.004,1.632),(.008,1.638)]]
        tube('HEAD_Oreja_Antihelix_'+side,anti,.00050,'HEAD','skin',6)
        crease=[ear_surface(y,z,.00012) for y,z in [(.000,1.621),(-.0005,1.627),(.002,1.631)]]
        tube('HEAD_Oreja_Pliegue_Concha_'+side,crease,.00020,'HEAD','skin_shadow',5)
        # A small continuous tragal fold; no detached round button.
        tragus=[ear_surface(y,z,.00012) for y,z in [(-.0023,1.6215),(-.0030,1.624),(-.0020,1.6265)]]
        tube('HEAD_Oreja_Plague_Trago_'+side,tragus,.00065,'HEAD','skin',6)

    def eye_arc(t,upper=True):
        x=.0205+.046*t
        z=1.6503+.0017*t
        z+=(.0071 if upper else -.0052)*math.sin(math.pi*t)**.9
        return x,z

    def ocular_y(s,x,z):
        # A shallow convex cornea contained by the socket, not a full eyeball.
        q=((x-s*.0435)/.026)**2+((z-1.652)/.0090)**2
        return face_y(x,z)-.00065-.0034*max(0,1-q)

    def disc(name,s,cx,cz,rx,rz,material,offset=.0002,count=24):
        vs=[(cx,ocular_y(s,cx,cz)-offset,cz)]
        for j in range(count):
            a=j*math.tau/count;x=cx+rx*math.cos(a);z=cz+rz*math.sin(a)
            vs.append((x,ocular_y(s,x,z)-offset,z))
        fs=[(0,1+j,1+(j+1)%count) for j in range(count)]
        return mesh(name,vs,fs,'HEAD',material,True)

    for s,side in [(-1,'R'),(1,'L')]:
        upper=[eye_arc(j/12,True) for j in range(13)]
        lower=[eye_arc(j/12,False) for j in range(12,-1,-1)]
        contour=upper+lower[1:-1]
        # Counterclockwise in X/Z = outward -Y. Mirror winding for left side.
        c=(s*.0435,1.652)
        vs=[(c[0],ocular_y(s,*c),c[1])]
        for scale in [.38,.72,1.0]:
            for xx,zz in contour:
                x=c[0]+scale*(s*xx-c[0]);z=c[1]+scale*(zz-c[1])
                vs.append((x,ocular_y(s,x,z),z))
        m=len(contour);fs=[]
        for j in range(m):fs.append((0,1+j,1+(j+1)%m))
        for row in range(2):
            a0=1+row*m;a1=a0+m
            for j in range(m):fs.append((a0+j,a0+(j+1)%m,a1+(j+1)%m,a1+j))
        # Arc walks clockwise for positive X; reverse it for exterior normals.
        if s>0:fs=[tuple(reversed(f)) for f in fs]
        mesh('HEAD_Ojo_Curvado_En_Cuenca_'+side,vs,fs,'HEAD','eye_white',True)
        disc('HEAD_Iris_Gris_Azul_'+side,s,s*.0435,1.6515,.0078,.0059,'iris',.00025,28)
        disc('HEAD_Pupila_'+side,s,s*.0435,1.6515,.0027,.0043,'pupil',.00050,20)
        disc('HEAD_Reflejo_Ojo_'+side,s,s*.0435-.0020,1.6540,.0010,.0013,'eye_white',.0007,12)

        # Skin eyelids are modeled strips with an actual raised free margin
        # and a broad blend into the cheek/upper socket on the same surface.
        for up,path,label in [(True,upper,'Superior'),(False,list(reversed(lower)),'Inferior')]:
            lv=[]
            for band in [0,1,2]:
                for k,(xx,zz) in enumerate(path):
                    t=k/(len(path)-1);taper=math.sin(math.pi*t)**.55
                    dz=(1 if up else -1)*[0,.00125,.0039 if up else .0032][band]*taper
                    x=s*xx;z=zz+dz
                    y=face_y(x,z)-[.0009,.0018,.00025][band]*taper
                    lv.append((x,y,z))
            lf=[];ln=len(path)
            for row in range(2):
                for k in range(ln-1):lf.append((row*ln+k,row*ln+k+1,(row+1)*ln+k+1,(row+1)*ln+k))
            # Upper strip +X produces a -Y normal. Lower strip reverses.
            if (s<0)==up:lf=[tuple(reversed(f)) for f in lf]
            mesh('HEAD_Parpado_Piel_'+label+'_'+side,lv,lf,'HEAD','skin',True)
        lash=[(s*x,face_y(s*x,z)-.0011,z) for x,z in upper]
        tube('HEAD_Borde_Parpado_Superior_'+side,lash,.00060,'HEAD','charcoal',5)
        crease=[]
        for k,(x,z) in enumerate(upper[2:-2]):
            z+=.0047
            crease.append((s*x,face_y(s*x,z)-.00020,z))
        tube('HEAD_Pliegue_Parpado_'+side,crease,.00020,'HEAD','skin_shadow',4)
        corner=(s*.0205,face_y(s*.0205,1.6503)-.0004,1.6503)
        ellipsoid('HEAD_Lagrimal_'+side,corner,(.0011,.00055,.00085),'HEAD','skin_shadow',8,4)
        # Brows are restrained, tapered volumes and follow the brow ridge.
        brow=[(.020,1.6722),(.029,1.6744),(.047,1.6752),(.066,1.6714)]
        bv=[]
        for k,(x,z) in enumerate(brow):
            half=[.00065,.00155,.00145,.0004][k]
            bv.extend([(s*x,face_y(s*x,z-half)-.0007,z-half),(s*x,face_y(s*x,z+half)-.0009,z+half)])
        bf=[(2*i,2*i+1,2*i+3,2*i+2) for i in range(3)]
        if s>0:bf=[tuple(reversed(f)) for f in bf]
        mesh('HEAD_Ceja_Concentrada_'+side,bv,bf,'HEAD','hair',True)

    # Lower nasal definition belongs to the integrated nose and is only lightly
    # marked. It does not replace its volume with painted dots or cylinders.
    for s,side in [(-1,'R'),(1,'L')]:
        cx=s*.0074;cz=1.6122
        nv=[(cx,face_y(cx,cz)-.00035,cz)]
        for j in range(12):
            a=j*math.tau/12;x=cx+.0020*math.cos(a);z=cz+.00075*math.sin(a)
            nv.append((x,face_y(x,z)-.00035,z))
        mesh('HEAD_Fosa_Nasal_'+side,nv,[(0,j+1,1+(j+1)%12) for j in range(12)],'HEAD','skin_shadow',True)
        tube('HEAD_Pliegue_Alar_'+side,[(s*.011,face_y(s*.011,1.6127)-.0002,1.6127),(s*.0122,face_y(s*.0122,1.615)-.0002,1.615),(s*.0115,face_y(s*.0115,1.618)-.0002,1.618)],.00020,'HEAD','skin_shadow',4)

    # Subtle lips are curved ribbons merging into the muzzle, not tubes laid
    # on a blank face. The separation line remains thin and neutral.
    lip_material=head.data.materials[0].copy()
    lip_material.name='MAT_Facial_Lip_Natural'
    lip_material.diffuse_color=(.62,.355,.225,1)
    if lip_material.use_nodes:
        shader=lip_material.node_tree.nodes.get('Principled BSDF')
        shader.inputs['Base Color'].default_value=(.62,.355,.225,1)
        shader.inputs['Roughness'].default_value=.78
    xs=[-.016,-.012,-.008,-.004,0,.004,.008,.012,.016]
    seam=[]
    for upper in [True,False]:
        vs=[]
        for band in range(3):
            for x in xs:
                shape=max(0,1-(x/.016)**2)
                zs=1.5840+.00045*(abs(x)/.016)**1.6
                if upper:
                    cupid=.00045*math.exp(-((abs(x)-.004)/.003)**2)
                    dz=[0,.0009,.0019][band]*shape+(cupid if band==2 else 0)
                    push=[.00045,.00085,.00008][band]*shape
                else:
                    dz=-[0,.0012,.0026][band]*shape
                    push=[.00045,.0010,.00008][band]*shape
                z=zs+dz;vs.append((x,face_y(x,z)-push,z))
        fs=[]
        for row in range(2):
            for j in range(len(xs)-1):fs.append((row*len(xs)+j,row*len(xs)+j+1,(row+1)*len(xs)+j+1,(row+1)*len(xs)+j))
        if not upper:fs=[tuple(reversed(f)) for f in fs]
        obj=mesh('HEAD_Labio_'+('Superior' if upper else 'Inferior'),vs,fs,'HEAD','skin',True)
        obj.data.materials[0]=lip_material
    for x in xs:
        z=1.5840+.00045*(abs(x)/.016)**1.6
        seam.append((x,face_y(x,z)-.00050*max(0,1-(x/.016)**2)-.00008,z))
    tube('HEAD_Linea_Boca_Neutral',seam,.00020,'HEAD','lip',5)
    for s,side in [(-1,'R'),(1,'L')]:
        x=s*.016;z=1.58445
        ellipsoid('HEAD_Comisura_'+side,(x,face_y(x,z)-.0002,z),(.0006,.00030,.00045),'HEAD','skin_shadow',8,4)

    # Dark cap is only the foundation beneath broad sculptural locks. Its
    # asymmetric lower boundary sits higher at the forehead than at the nape.
    # Explicit hair profiles stay outside the skull even below the occipital
    # bulge. A spherical cap narrows too early here and exposes a bald crescent.
    hair_profiles=[
        (1.582,.077,.066,.007),(1.595,.085,.076,.008),
        (1.620,.094,.086,.011),(1.645,.099,.093,.012),
        (1.670,.101,.095,.013),(1.695,.100,.093,.013),
        (1.720,.092,.089,.013),(1.745,.075,.076,.013),
        (1.760,.044,.048,.012),(1.770,.007,.010,.010)]

    def hair_profile(z):
        if z<=hair_profiles[0][0]:
            return hair_profiles[0][1:]
        if z>=hair_profiles[-1][0]:
            return hair_profiles[-1][1:]
        for a,b in zip(hair_profiles,hair_profiles[1:]):
            if a[0]<=z<=b[0]:
                t=(z-a[0])/(b[0]-a[0])
                return tuple(a[k]*(1-t)+b[k]*t for k in range(1,4))

    hair_verts=[]
    ns=36
    for t in [.045,.23,.46,.72,1.0]:
        for j in range(ns):
            theta=2*math.pi*j/ns
            si,co=math.sin(theta),math.cos(theta)
            low=1.640 + .048*max(0,-si) - .054*max(0,si)
            z=1.770+(low-1.770)*(1-math.cos(math.pi*t/2))
            rx,ry,cy=hair_profile(z)
            wobble=1+.018*math.sin(3*theta+.6)+.010*math.cos(7*theta)
            hair_verts.append((rx*co*wobble,cy+ry*si*wobble,z))
    hair_faces=[]
    for k in range(4):
        for j in range(ns):
            # Top->bottom rings: reverse the usual bottom->top loft winding.
            hair_faces.append((k*ns+j,(k+1)*ns+j,(k+1)*ns+(j+1)%ns,k*ns+(j+1)%ns))
    hair_faces.append(tuple(range(ns)))
    scalp=mesh('HAIR_Base_Craneo_Con_Nuca',hair_verts,hair_faces,'HAIR','hair',True)
    scalp['design']='Base cubierta por mechones anchos; flequillo asimetrico diagonal'

    def lock(name, points, widths, thickness=.005, material='hair', normal=(0,-1,0)):
        """Lenticular swept lock with a front ridge and a sharply tapered end.

        Six vertices per section produce a broad sheet-like volume rather
        than a round strand, with a soft raised ridge along the visible face.
        """
        vs=[]
        nor=Vector(normal).normalized()
        count=6
        for i,p in enumerate(points):
            p=Vector(p)
            tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
            tangent.normalize()
            u=nor.cross(tangent).normalized()
            v=tangent.cross(u).normalized()
            # Ensure the outward crest stays oriented toward the supplied normal.
            if v.dot(nor)<0:
                u=-u;v=-v
            width=widths[i]
            th=thickness*(.25+.75*(width/max(widths)))
            # Elliptical cross section in the width/depth plane; its leading
            # face reads as two broad facets beneath the smooth normal field.
            for k in range(count):
                a=2*math.pi*k/count
                vec=p+u*(width*.5*math.cos(a))+v*(th*math.sin(a))
                vec.z=min(vec.z,1.780)
                vs.append(tuple(vec))
        fs=[]
        for i in range(len(points)-1):
            for k in range(count):
                fs.append((i*count+k,i*count+(k+1)%count,(i+1)*count+(k+1)%count,(i+1)*count+k))
        fs += [tuple(reversed(range(count))),tuple((len(points)-1)*count+k for k in range(count))]
        return mesh('HAIR_'+name,vs,fs,'HAIR',material,True)

    # A few main sweeping masses carry the silhouette. Front pieces overlap
    # each other with a visible side part and taper around, rather than over,
    # the central portions of the two lenses.
    main_locks=[
        ('Flequillo_Diagonal_01',[(.060,-.033,1.745),(.026,-.078,1.724),(-.013,-.092,1.698),(-.045,-.091,1.669),(-.064,-.078,1.650)],[.028,.042,.044,.027,.0015],.007,'hair'),
        ('Flequillo_Diagonal_02',[(.071,-.018,1.738),(.048,-.063,1.716),(.017,-.091,1.687),(-.005,-.092,1.661)],[.026,.035,.030,.0015],.006,'hair_hi'),
        ('Flequillo_Diagonal_03',[(.080,-.012,1.732),(.077,-.050,1.702),(.067,-.078,1.672),(.054,-.084,1.638)],[.026,.028,.020,.0012],.006,'hair'),
        ('Flequillo_Lateral_R',[(.025,-.062,1.739),(-.023,-.087,1.716),(-.064,-.090,1.687),(-.086,-.068,1.658)],[.032,.039,.034,.0015],.006,'hair'),
        ('Flequillo_Marco_R',[(-.039,-.043,1.731),(-.080,-.065,1.704),(-.096,-.058,1.672),(-.086,-.063,1.632)],[.029,.030,.022,.0014],.0055,'hair'),
        ('Flequillo_Marco_L',[(.087,-.010,1.705),(.097,-.037,1.674),(.093,-.058,1.644),(.080,-.061,1.613)],[.020,.024,.016,.0015],.005,'hair'),
    ]
    for name,pts,ws,th,ma in main_locks:
        lock(name,pts,ws,th,ma)

    # Crown locks sweep in the same broad direction as the fringe. Soft tips
    # are only modestly outside the cap: an irregular cut, not an even hedgehog.
    crown=[
        ('Corona_Barrido_01',[(.059,.049,1.745),(.020,.018,1.768),(-.027,-.025,1.756),(-.066,-.056,1.721)],[.026,.043,.043,.002],.006,'hair_hi'),
        ('Corona_Barrido_02',[(.080,.030,1.722),(.054,-.006,1.751),(.005,-.052,1.745),(-.043,-.081,1.714)],[.027,.041,.035,.002],.006,'hair'),
        ('Corona_Barrido_03',[(.036,.077,1.732),(-.008,.059,1.761),(-.046,.025,1.750),(-.081,-.006,1.719)],[.028,.041,.039,.002],.006,'hair'),
        ('Corona_Barrido_04',[(.001,.090,1.710),(-.043,.072,1.742),(-.077,.042,1.727),(-.100,.007,1.694)],[.028,.039,.035,.002],.006,'hair'),
        ('Corona_Remolino',[(.036,.028,1.755),(.010,.020,1.773),(-.012,.012,1.780),(-.033,.005,1.763)],[.015,.022,.012,.001],.0045,'hair'),
        ('Corona_Costa_L',[(.083,.049,1.707),(.096,.018,1.721),(.102,-.017,1.693),(.100,-.029,1.663)],[.027,.033,.025,.002],.005,'hair'),
    ]
    for name,pts,ws,th,ma in crown:
        lock(name,pts,ws,th,ma,normal=(0,-.5,.85))

    # Side and nape masses, alternating lengths and large overlapping chunks.
    # They form the rear silhouette visible in the supplied turnaround.
    for s,side in [(-1,'R'),(1,'L')]:
        side_locks=[
            ([(s*.062,.055,1.725),(s*.095,.034,1.701),(s*.105,.017,1.668),(s*.098,.009,1.641)],[.029,.037,.031,.002]),
            ([(s*.077,.058,1.696),(s*.100,.054,1.671),(s*.105,.043,1.640),(s*.091,.025,1.606)],[.030,.034,.025,.002]),
            ([(s*.070,.079,1.674),(s*.088,.080,1.645),(s*.086,.066,1.615),(s*.073,.047,1.590)],[.030,.036,.026,.002]),
        ]
        for i,(pts,ws) in enumerate(side_locks):
            lock('Lado_'+side+'_'+str(i+1),pts,ws,.005,'hair' if i!=1 else 'hair_hi',normal=(s*.9,.3,0))
    def back_point(x,z,offset=.0038):
        rx,ry,cy=hair_profile(z)
        y=cy+ry*math.sqrt(max(.02,1-(x/rx)**2))
        return (x,y+offset,z)

    # The rear is covered by two overlapping rows of broad tapered locks.
    # Anchoring their center lines to the cap prevents the earlier hidden
    # strands/large smooth rear surface, while preserving editable volumes.
    # Hand-placed sweeping trajectories fan out from the offset crown swirl.
    # Their roots and tips intentionally do not share horizontal rows.
    high_paths=[
        [(.025,1.749),(-.010,1.733),(-.055,1.708),(-.085,1.678)],
        [(.046,1.742),(.012,1.718),(-.031,1.680),(-.066,1.641)],
        [(.068,1.727),(.041,1.704),(.004,1.666),(-.034,1.627)],
        [(.075,1.710),(.066,1.682),(.032,1.647),(.002,1.604)],
        [(.021,1.750),(.061,1.730),(.081,1.697),(.087,1.662)],
        [(-.015,1.746),(-.054,1.729),(-.078,1.699),(-.095,1.663)],
        [(-.044,1.725),(-.066,1.698),(-.081,1.666),(-.087,1.626)],
    ]
    for i,path in enumerate(high_paths):
        pts=[back_point(x,z,.0038) for x,z in path]
        widths=[.026,.045+(i%3)*.002,.036+(i%2)*.004,.0015]
        lock('Occipital_Barrido_'+str(i+1),pts,widths,.0048,'hair_hi' if i in [1,5] else 'hair',normal=(0,1,.10))
    low_paths=[
        [(-.076,1.672),(-.074,1.639),(-.064,1.614),(-.071,1.597)],
        [(-.056,1.683),(-.044,1.655),(-.034,1.620),(-.047,1.583)],
        [(-.026,1.689),(-.016,1.653),(-.005,1.620),(-.020,1.593)],
        [(.002,1.674),(.010,1.644),(.016,1.611),(.005,1.580)],
        [(.036,1.682),(.042,1.650),(.044,1.618),(.032,1.588)],
        [(.062,1.665),(.067,1.640),(.063,1.615),(.054,1.601)],
        [(.078,1.683),(.086,1.653),(.080,1.623),(.071,1.594)],
        [(-.060,1.650),(-.042,1.624),(-.020,1.603),(-.032,1.579)],
        [(.020,1.637),(.030,1.620),(.023,1.598),(.042,1.584)],
    ]
    for i,path in enumerate(low_paths):
        pts=[back_point(x,z,.0045) for x,z in path]
        widths=[.025+(i%3)*.002,.034+(i%2)*.004,.029+(i%3)*.002,.0015]
        lock('Nuca_Mechon_Irregular_'+str(i+1),pts,widths,.0047,'hair_hi' if i in [2,6] else 'hair',normal=(0,1,-.08))

    # Fine, rounded rectangular spectacles. The lenses only tint the eyes very
    # lightly; the readable design comes from the frame, bridge and temples.
    def glass_y(x):
        return -.092+.014*(abs(x)/.080)**2

    def rounded_rect(cx,cz,w,h,r):
        pts=[]
        # In X/Z this is counterclockwise, hence points toward front -Y.
        for x,z,start in [(cx+w/2-r,cz+h/2-r,0),(cx-w/2+r,cz+h/2-r,90),
                          (cx-w/2+r,cz-h/2+r,180),(cx+w/2-r,cz-h/2+r,270)]:
            for j in range(4):
                a=math.radians(start+j*30)
                px=x+r*math.cos(a);pz=z+r*math.sin(a)
                pts.append((px,glass_y(px),pz))
        return pts

    for s,side in [(-1,'R'),(1,'L')]:
        cx=s*.043
        frame=rounded_rect(cx,1.651,.067,.031,.004)
        tube('GLASS_Montura_Rectangular_'+side,frame+[frame[0]],.00135,'GLASSES','charcoal',6)
        lens=rounded_rect(cx,1.651,.063,.027,.003)
        lens=[(x,y+.0005,z) for x,y,z in lens]
        # No thickness: transparent coating over a surface is enough for blockout.
        obj=mesh('GLASS_Lente_Filtro_Azul_'+side,lens,[tuple(range(len(lens)))],'GLASSES','lens',True)
        obj['design']='Filtro azul tenue; lentes transparentes, no placa opaca'
        if obj.data.materials:
            m=obj.data.materials[0]
            if m.use_nodes:
                shader=m.node_tree.nodes.get('Principled BSDF')
                if shader:
                    shader.inputs['Alpha'].default_value=.10
                    shader.inputs['Roughness'].default_value=.10
                    if shader.inputs.get('Transmission Weight'):
                        shader.inputs['Transmission Weight'].default_value=.65
                    shader.inputs['IOR'].default_value=1.1
            if hasattr(m,'surface_render_method'):
                m.surface_render_method='DITHERED'
        outer=s*.077
        tube('GLASS_Patilla_'+side,[(outer,glass_y(outer),1.654),(s*.092,-.029,1.650),(s*.096,.015,1.643),(s*.093,.030,1.628)],.0017,'GLASSES','charcoal',6)
        ellipsoid('GLASS_Bisagra_'+side,(s*.078,glass_y(.078)-.0003,1.655),(.0027,.0022,.0030),'GLASSES','metal',8,4)
    tube('GLASS_Puente_Nasal',[(-.010,-.0917,1.655),(-.005,-.096,1.659),(0,-.097,1.660),(.005,-.096,1.659),(.010,-.0917,1.655)],.0012,'GLASSES','charcoal',6)

    return {'objects':made,'chin_z':1.535,'eye_z':1.652,'top_z':1.780,'skin_width':.176,'hair_width_approx':.218}
