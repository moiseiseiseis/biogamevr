import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,numpy as np
from herramientas import *; enable()
# source: original full Body
bpy.ops.wm.open_mainfile(filepath=W+'/base.blend')
src=bpy.data.objects['Body']; sme=src.data
gn=[g.name for g in src.vertex_groups]
WT=np.zeros((len(sme.vertices),len(gn)),dtype=np.float32)
for v in sme.vertices:
    for g in v.groups: WT[v.index,g.group]=g.weight
SV=np.array([v.co[:] for v in sme.vertices])
tris=[]; 
sme.calc_loop_triangles()
for t in sme.loop_triangles:
    if t.material_index in (7,8,4): continue   # skip glasses & hairback
    tris.append(tuple(t.vertices))
bvh=BVHTree.FromPolygons([tuple(c) for c in SV],tris)
np.save(W+'/W.npy',WT); np.save(W+'/SV.npy',SV); np.save(W+'/T.npy',np.array(tris))
open(W+'/gn.txt','w').write('\n'.join(gn))
