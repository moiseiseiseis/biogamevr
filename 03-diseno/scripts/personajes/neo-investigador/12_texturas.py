import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from rutas import *
import bpy,sys,oxipng
from herramientas import enable; enable()
bpy.ops.wm.open_mainfile(filepath=W+'/stage8.blend')
tot0=tot1=0
for im in bpy.data.images:
    pf=im.packed_file
    if not pf: continue
    data=bytes(pf.data)
    if data[:8]!=b'\x89PNG\r\n\x1a\n': print('notpng',im.name); continue
    new=oxipng.optimize_from_memory(data, level=4, strip=oxipng.StripChunks.safe())
    tot0+=len(data); tot1+=len(new)
    if len(new)<len(data):
        im.pack(data=new, data_len=len(new))
    print(im.name,len(data),'->',len(new))
print('total',tot0,tot1)
bpy.ops.wm.save_as_mainfile(filepath=W+'/stage9.blend', compress=True)
