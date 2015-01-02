import random
import time
import binascii
import bz2
from echo_server import EchoServer
import collections
import pylab as plt
import cv2
import caffeDL
import hodmodule
import slWord
class HandShapeServer(object):

    def __init__(self, port, converter, terminator, data_handler=None):
        self.converter = converter
        self.data_handler = data_handler
        self.server = EchoServer(port, terminator, self.received_data)
        self.slword=slWord.SlWord()
        self.caffedl=caffeDL()
        self.server.start()

    def received_data(self, received_data):
        print (len(received_data), binascii.crc32(received_data))
        if(received_data=="END"):
            self.process_all()
            return
        
        decompressed_data = bz2.decompress(received_data)
        decoded_data = self.converter.decode(decompressed_data)
        return self.process_data(decoded_data)

    def process_data(self, decoded_data):
        if hasattr(self, 'data_handler') and self.data_handler is not None:
            self.data_handler.handle_data(decoded_data)
            
            self.slword.loadSkeleton(decoded_data["skeleton"])
            
            if(decoded_data['depth_image']!=[]):
                img=decoded_data['depth_image']
                sp=img.shape
                img2=cv2.copyMakeBorder(img, 0,0, int(abs(sp[0]-sp[1])/2),int(abs(sp[0]-sp[1])/2), cv2.BORDER_CONSTANT, value=(0, 0, 0, 0))
                img3=cv2.resize(img2,(128,128))
                img3=img3/255.0
                self.slword.imgset.append(img3)
            else:
                self.slword.imgset.append([])
            
            







        # For debug
        '''sleep_time = random.randint(1, 10)
        plt.subplot(1,2,1), plt.imshow(decoded_data['depth_image'])
        plt.subplot(1,2,2), plt.imshow(decoded_data['color_image'])
        plt.show()
        arrive_time = time.strftime("%H:%M:%S")
        time.sleep(sleep_time)
        message_to_send = "Arrive=%s, Sleep=%d, Response=%s, Message='%s%s" % (arrive_time, sleep_time, time.strftime("%H:%M:%S"), self.flatten(decoded_data).keys(), self.server.terminator)
        return message_to_send'''

    def process_all(self):
        self.slword.displacement,rightV,rightH=hodmodule.hod(self.slword.skeletons)
        self.slword.getVelocity(rightV,rightH)
        self.slword.findTopHandshape()
        self.caffedl.net.predict(self.batch)
        feature=[]
        for s in range(len(self.batch)):
            feat = self.caffedl.net.blobs['ip1'].data[s].flatten().tolist()
            feature.append(feat)
        self.slword.handshape=self.slword.pooling(feature,1)
        self.slword.combineFeature()
        svmModel= svm_load_model("/home/lzz/svmModel")
        svm_res1=test_svm_model(svmModel,test_labels=0,self.slword.combinedFeature)
        pred_labels=svm_res1[0];
        

        




    def flatten(self, d, parent_key='', sep='.'):
        items = []
        for k, v in d.items():
            new_key = parent_key + sep + k if parent_key else k
            if isinstance(v, collections.MutableMapping):
                items.extend(self.flatten(v, new_key, sep=sep).items())
            else:
                items.append((new_key, v))
        return dict(items)
