from svmutil import *
from svmmodule import *
import random
import time
import binascii
import bz2
from FrameConverter import FrameConverter
from echo_server import EchoServer
import collections
import pylab as plt
import cv2
from caffeDL import *
import hodmodule
import slWord
class HandShapeServer(object):

    def __init__(self, port):
        self.converter = FrameConverter()
        self.slword=slWord.SlWord()

        self.caffedl=caffeDL()
        self.server = EchoServer(port,  self.received_data)


    def received_data(self, received_data):
        print (len(received_data), binascii.crc32(received_data))
        
        #decompressed_data = bz2.decompress(received_data)
        decoded_data = self.converter.decode(received_data)
	    #import pprint
	    #pprint.pprint(decoded_data)
        return self.process_data(decoded_data)

    def process_data(self, decoded_data):
        sleep_time = random.randint(1, 10)
        if(decoded_data["label"]=="End"):
            self.process_all()
            return "0"

        self.slword.loadSkeleton(decoded_data["skeleton"])


        if(decoded_data['depth_image']!=''):
            print "skl"
            img=decoded_data['depth_image']
            sp=img.shape
            print sp
            img2=cv2.copyMakeBorder(img, 0,0, int(abs(sp[0]-sp[1])/2),int(abs(sp[0]-sp[1])/2), cv2.BORDER_CONSTANT, value=(0, 0, 0, 0))
            img3=cv2.resize(img2,(128,128))
            img3=img3/255.0
            self.slword.imgset.append(img3)
            return "0"
        else:
            self.slword.imgset.append([])
        return "0"
            

    def play(self,pred_labels):
        modeldic={}
        modelindex=open("/home/lzz/ModelIndex.txt","r")
        for line in open("/home/lzz/ModelIndex.txt"):
            line = modelindex.readline()
            sep=line.split(" ")
            modeldic[sep[0]]=sep[1]
        wordname=modeldic[str(int(pred_labels))]
        dirs=os.listdir("/media/lzz/Data1/Aaron/1-250/")
        for dir in dirs:
            if(dir.find(wordname)):
                video=dir
                break

        filename = "/media/lzz/Data1/Aaron/1-250/"+video+"/"+video+"_c.avi"
        print filename
        win_name=filename
        capture = cv2.cv.CaptureFromFile(filename)


        cv2.cv.NamedWindow(win_name, cv2.cv.CV_WINDOW_AUTOSIZE)


        while 1:


            image = cv2.cv.QueryFrame(capture)


            cv2.cv.ShowImage(win_name, image)

            c = cv2.cv.WaitKey(33)
            if(image==None):
                break
        cv2.cv.DestroyWindow(win_name)






    def process_all(self):
        self.slword.displacement,rightV,rightH=hodmodule.hod(self.slword.skeletons)
        print self.slword.displacement,rightV
        print len(self.slword.displacement),len(rightV)
        self.slword.getVelocity(rightV,rightH)
        self.slword.findTopHandshape()
        self.caffedl.net.predict(self.slword.batch)
        feature=[]
        for s in range(len(self.slword.batch)):
            feat = self.caffedl.net.blobs['ip1'].data[s].flatten().tolist()
            feature.append(feat)
        self.slword.handshape=self.slword.pooling(feature,1)
        self.slword.combineFeature()
        svmModel= svm_load_model("/home/lzz/svmModel")
        print self.slword.combinedFeature
        print len(self.slword.combinedFeature)
        svm_res1=test_svm_model(0,self.slword.combinedFeature,svmModel)
        pred_labels=svm_res1[0];
        print "pred_labels",pred_labels
        self.play(pred_labels)





    def flatten(self, d, parent_key='', sep='.'):
        items = []
        for k, v in d.items():
            new_key = parent_key + sep + k if parent_key else k
            if isinstance(v, collections.MutableMapping):
                items.extend(self.flatten(v, new_key, sep=sep).items())
            else:
                items.append((new_key, v))
        return dict(items)
