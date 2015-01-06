'''
Created on 2014-9-16

@author: lenovo
'''

import sqlite3
import struct

def load_templates(template_file="../data/hog_template5.txt"):
    ret=[]
    templates=open(template_file)
    for t in templates:
        ret.append([float(i) for i in t.split()])
#     random.shuffle(ret)
#     print len(ret)
#     ret=ret[0:5]+ret[30:35]+ret[285:290]
    return ret

def load_handshapes(db_file_name="../data/Aaron1-50.db"):
    ret=[]
    db = sqlite3.connect(db_file_name)
    for hog in db.execute("select LeftHandHOG from framedata;"):
        if hog[0]:
            new_hog= tuple([struct.unpack('f',hog[0][i:i+4])[0] for i in range(0,len(hog[0]),4)])
            ret.append(new_hog)
            #print len(new_hog)
    for hog in db.execute("select rightHandHOG from framedata;"):
        if hog[0]:
            new_hog=tuple( [struct.unpack('f',hog[0][i:i+4])[0] for i in range(0,len(hog[0]),4)])
            ret.append(new_hog)
    return ret

