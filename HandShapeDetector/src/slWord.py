from svmutil import *
from svmmodule import *
import matplotlib
import sqlite3
import math
import numpy
import struct
#from hmm.continuous.GMHMM import GMHMM
#from cluster import KMeansClustering
import time
import random
import matplotlib.pyplot as plt
import marshal, pickle

from basic import *
#from load import *
from constant_numbers import *
from hodmodule import *
import hogmodule
from svmmodule import *
import os
#from Tkinter import *
import numpy as np
import cv2
import csv
import caffe

from skimage.feature import hog
from skimage import data, color, exposure
import skimage

class SlWord():
    def __init__(self):
        #loc=path.rfind("/")
        #name=path[loc:]
        #self.sampleName=name
        #self.wordName=self.sampleName[1:self.sampleName.find(" ")]
        
        self.skeletons=[]
        
        self.batch=[]
        self.imgset=[]
        
        self.displacement=[]
        self.handshape=[]
        self.hogFeature=[]
        
        self.keyNo=10
        #self.path=path
        self.pred=''
        
        self.label=0
        self.combinedFeature=[]
        
        
        #self.loadDisplacement()
        #self.getVelocity()
        #self.findTopHandshape()
        ''''self.getHogFeature()'''
        #print self.wordName
        
        
        
        
        
    def getVelocity(self,rightV,rightH):

        self.vList= []
        self.hList=[]
        self.indexList=[]
        self.value=[]
        index=-1
        for img in self.imgset:
            index+=1
            if img==[]:
                continue
            else:
                self.indexList.append(index)
                self.vList.append(rightV[index])
                self.hList.append(rightH[index])
                self.value.append(0)
        for i in range(len(self.indexList)):
            self.value[i]=float(self.hList[i])-10*float(self.vList[i])-0.02*abs(i-len(self.indexList)/2)

        if(len(self.value)<self.keyNo):
            self.keyNo=len(self.value)

        
    def loadSkeleton(self,skeleton0):
        skeleton=skeleton0.split(",")
        for i in range(len(skeleton)):
            skeleton[i]=float(skeleton[i])
        print len(skeleton)
        self.skeletons.append(skeleton)

        
    def findTopHandshape(self):
        
        self.top=[]
        self.topIndex=[]
        for i in range(self.keyNo):
            self.top.append(self.value[i])
            self.topIndex.append(self.indexList[i])
        #top5Index=[indexList[0],indexList[1],indexList[2],indexList[3],indexList[4]]
        for i in range(self.keyNo,len(self.indexList)):
            if(self.value[i]>min(self.top)):
                ind=self.top.index(min(self.top))
                self.top[ind]=self.value[i]
                self.topIndex[ind]=self.indexList[i]
        #print top5Index
        

        for i in range(len(self.indexList)):
            self.batch.append(self.imgset[i])
        return self.top,self.topIndex




        
        
    def getHogFeature(self):
        files=os.listdir(self.path+"/handshape/")
        hogSet=[]
        for file in files:
            if file[-3:]!="jpg":
                continue
            
            img=cv2.imread(self.path+"/handshape/"+file)
            sp=img.shape
            img2=cv2.copyMakeBorder(img, 0,0, int(abs(sp[0]-sp[1])/2),int(abs(sp[0]-sp[1])/2), cv2.BORDER_CONSTANT, value=(0, 0, 0, 0))
            img3=cv2.resize(img2,(128,128))
            image=img3/255.0
            #image = color.rgb2gray(skimage.data.astronaut())
            image = color.rgb2gray(image)
            
            fd, hog_image = hog(image, orientations=9, pixels_per_cell=(16,16),cells_per_block=(2, 2), visualise=True)
            hogSet.append(fd)

        self.hogFeature=hogmodule.findKey(hogSet)


    def idvdCaffeFeature(self,img_sum,featureTotal,featureTotal2):


        feature=[]
        feature2=[]
        
        for i in range(self.keyNo):
            # print img_sum,i
            feat = featureTotal[img_sum+i]
            feat2 = featureTotal2[img_sum+i]
            #print feat.index(max(feat))
            feature.append(feat)
            feature2.append(feat2)
        #print feature

        self.handshape=self.pooling(feature,1)
        
        self.getVariance(feature2)
        
    
    def pooling(self,feature,types):
        handshape=[]
        if(types==0):
            #max pooling
            for i in range(len(feature[0])):
                maxvalue=feature[0][i]
                for j in range(len(feature)):
                    if(feature[j][i]>maxvalue):
                        maxvalue=feature[j][i]
                handshape.append(maxvalue)
            return handshape
        if(types==1):
            for i in range(len(feature[0])):
                sum0=0
                for j in range(len(feature)):
                    sum0=sum0+feature[j][i]
                ave=sum0/len(feature)
                handshape.append(ave)
            return handshape
        if(types==2):
            for i in range(len(feature[0])):
                seq=[]
                for j in range(len(feature)):
                    seq.append(feature[j][i])
                seq1=sorted(seq)
                midValue=seq1[len(seq1) // 2]
                handshape.append(midValue)
            return handshape
        
    
    
    
    
    def getVariance(self,feature):
        self.hand_index_list=[]
        for x in range(len(feature)):
            hand_index=feature[x].index(max(feature[x]))
            self.hand_index_list.append(hand_index)
        #hand_result.write(str(l)+" "+index2name[l]+" "+str(hand_index_list)+"\n")
        hand_exist=[]
        #print hand_index_list
        self.variance=0
        for x in range(self.keyNo):

            if((self.hand_index_list[x] in hand_exist)==0):
                hand_exist.append(self.hand_index_list[x])
                self.variance+=1

    
    def combineFeature(self):
        self.combinedFeature=self.displacement+normalize_histogram(self.handshape)+normalize_histogram(self.hogFeature)
