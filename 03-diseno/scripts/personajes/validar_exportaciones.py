"""Comprueba por reimportacion las exportaciones estaticas FBX y GLB en Blender."""
import bpy,json,math,struct
from pathlib import Path
ROOT=Path(__file__).resolve().parent.parents[1]
DATA=ROOT/'validacion'/'personajes'
source=json.loads((DATA/'exportaciones.json').read_text(encoding='utf8'))
results=[]
for asset in source['assets']:
    for fileinfo in asset['files']:
        path=ROOT/fileinfo['path'];assert path.is_file() and path.stat().st_size>10000
        if path.suffix=='.glb':
            raw=path.read_bytes();magic,version,total=struct.unpack_from('<4sII',raw)
            assert magic==b'glTF' and version==2 and total==len(raw)
            size,kind=struct.unpack_from('<II',raw,12);data=json.loads(raw[20:20+size])
            assert all('uri' not in b for b in data.get('buffers',[])),path
            assert len(data.get('scenes',[]))==1,(path,'Expected one static export scene')
        bpy.ops.wm.read_factory_settings(use_empty=True)
        if path.suffix=='.fbx':bpy.ops.import_scene.fbx(filepath=str(path))
        else:bpy.ops.import_scene.gltf(filepath=str(path))
        bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get()
        meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
        assert len(meshes)==asset['mesh_objects'],(path,len(meshes),asset['mesh_objects'])
        coords=[];triangles=0;materials=0
        for o in meshes:
            e=o.evaluated_get(dg);me=e.to_mesh();me.calc_loop_triangles()
            triangles+=len(me.loop_triangles);materials+=len(me.materials)
            coords.extend(e.matrix_world@v.co for v in me.vertices)
            e.to_mesh_clear()
        assert all(math.isfinite(x) for v in coords for x in v)
        lo=[min(v[i] for v in coords) for i in range(3)];hi=[max(v[i] for v in coords) for i in range(3)]
        dims=[hi[i]-lo[i] for i in range(3)]
        assert max(abs(dims[i]-asset['dimensions_m'][i]) for i in range(3))<.005,(path,dims,asset['dimensions_m'])
        assert abs(lo[2])<.005,(path,lo)
        assert materials>0,path
        delta=triangles-asset['evaluated_triangles']
        assert abs(delta)<max(10,asset['evaluated_triangles']*.01),(path,triangles,asset['evaluated_triangles'])
        results.append({'file':str(path.relative_to(ROOT)),'reimported_in_blender':True,'mesh_objects':len(meshes),'triangles':triangles,'native_evaluated_triangles':asset['evaluated_triangles'],'triangle_delta':delta,'dimensions_m':dims,'feet_z0':True,'material_slots':materials,'result':'PASS'})
(DATA/'exportaciones-verificacion.json').write_text(json.dumps({'blender':bpy.app.version_string,'tested_in_unity':False,'files':results,'result':'PASS'},indent=2,ensure_ascii=False),encoding='utf8')
print('EXPORT_ROUNDTRIP_PASS',json.dumps(results))
