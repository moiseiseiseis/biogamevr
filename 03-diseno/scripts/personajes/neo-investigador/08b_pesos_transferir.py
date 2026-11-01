import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,numpy as np
from herramientas import *; enable()
WT=np.load(W+'/W.npy'); SV=np.load(W+'/SV.npy'); T=np.load(W+'/T.npy')
gn=open(W+'/gn.txt').read().split('\n')
bvh=BVHTree.FromPolygons([tuple(c) for c in SV],[tuple(int(i) for i in t) for t in T])
def transfer(ob, smooth_iters=0):
    for vg in list(ob.vertex_groups): ob.vertex_groups.remove(vg)
    me=ob.data; n=len(me.vertices); out=np.zeros((n,len(gn)),dtype=np.float32)
    for v in me.vertices:
        loc,nor,idx,d=bvh.find_nearest(v.co)
        t=T[idx]; P=SV[t]; dd=np.linalg.norm(P-np.array(loc[:]),axis=1)+1e-5
        w=1/dd; w/=w.sum()
        out[v.index]=(WT[t]*w[:,None]).sum(0)
    # smooth weights over mesh edges
    for _ in range(smooth_iters):
        acc=out.copy(); cnt=np.ones(n)
        for e in me.edges:
            a,b=e.vertices; acc[a]+=out[b]; acc[b]+=out[a]; cnt[a]+=1; cnt[b]+=1
        out=acc/cnt[:,None]
    # keep top 4 influences, normalize
    for i in range(n):
        r=out[i]; idx=np.argsort(r)[::-1]; r[idx[4:]]=0; s=r.sum()
        if s>0: out[i]=r/s
    used=np.where(out.max(0)>1e-3)[0]
    for gi in used:
        vg=ob.vertex_groups.new(name=gn[gi])
        for i in np.where(out[:,gi]>1e-3)[0]: vg.add([int(i)], float(out[i,gi]), 'REPLACE')
f=W+'/stage6.blend'; outp=W+'/stage7.blend'
bpy.ops.wm.open_mainfile(filepath=f)
for o in bpy.data.objects:
    if o.type=='MESH' and o.name.startswith('INV_') and o.name!='INV_Tableta_Prop' and len(o.vertex_groups)==0:
        transfer(o, smooth_iters=2)
        print('weighted',o.name,len(o.vertex_groups))
bpy.ops.wm.save_as_mainfile(filepath=outp)
