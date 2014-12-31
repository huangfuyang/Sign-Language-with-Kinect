'''
Created on 2014-9-16

@author: lenovo
'''

from basic import *
import time
from constant_numbers import *
import struct
from load import *
from kMedoid import *
from numpy import *
from cv2 import *
from visualization import *
def hog(hand_data):
    new_hog=tuple( [struct.unpack('f',hand_data[i:i+4])[0] for i in range(0,len(hand_data),4)])
#     print new_hog
    return new_hog





#def construct_hog_binary_features(frames,templates,level_index,level_num,fout,f,f_length,left,right):
def construct_hog_binary_features(frames,templates,level_index,level_num,left,right):
    if level_index==level_num:
        return []
    left_indices=[]
    right_indices=[]
    index=0
    exist=0

    blogword=zeros((1,4356))           
    best_choice=[]
    ind=0
    inde=0
    for frame in frames:

        if frame[RIGHT_HOG]:
            inde+=1
            hoglist=list(hog(frame[RIGHT_HOG]));
            mmm=zeros((1,4356))
            mmm[0,:]=hoglist
            
            if(right[index]==1):
                #get_hogdescriptor_visual_image(normalize_histogram(hoglist),inde,"1","1")
                blogword=concatenate((blogword,mmm))
                ind+=1
        index=index+1
    #print ind
    #print inde
    if (ind==0):
        return []
    blogword=blogword[1:size(blogword),:]
    best_choice = kmedoids(blogword,1)
#    print best_choice
#    print >>fout,sum(x1),sum(x2)
    li=list(blogword[best_choice,:])
    return normalize_histogram(list(blogword[best_choice,:]))

def findKey(frames):
    blogword=zeros((1,len(frames[0])))           
    ind=0
    for frame in frames:
            mmm=zeros((1,len(frames[0])))
            mmm[0,:]=frame
                #get_hogdescriptor_visual_image(normalize_histogram(hoglist),inde,"1","1")
            blogword=concatenate((blogword,mmm))
            ind+=1

    #print ind
    #print inde
    if (ind==0):
        return []
    blogword=blogword[1:size(blogword),:]
    best_choice = kmedoids(blogword,1)
#    print best_choice
#    print >>fout,sum(x1),sum(x2)
    li=list(blogword[best_choice,:])
    return normalize_histogram(list(blogword[best_choice,:]))




def construct_hog_binary_features2(frames,templates,level_index,level_num,left,right,name):
    if level_index==level_num:
        return []
    left_indices=[]
    right_indices=[]
    index=0
    exist=0

    blogword=zeros((1,4356))           
    best_choice=[]
    ind=0
    inde=0
    dic={}
    for frame in frames:

        if frame[RIGHT_HOG]:
            inde+=1
            hoglist=list(hog(frame[RIGHT_HOG]));
            mmm=zeros((1,4356))
            mmm[0,:]=hoglist
            #get_hogdescriptor_visual_image2(normalize_histogram(hoglist),index)
            if(right[index]==1):
                get_hogdescriptor_visual_image2(normalize_histogram(hoglist),index+25)
                blogword=concatenate((blogword,mmm))
                ind+=1
                dic[ind]=index+25
        index=index+1
#    print ind
#    print inde
    if (ind==0):
        return []
    blogword=blogword[1:size(blogword),:]
    best_choice = kmedoids(blogword,1)
    print dic[best_choice]
#    print >>fout,sum(x1),sum(x2)
    li=list(blogword[best_choice,:])
    return normalize_histogram(list(blogword[best_choice,:]))



    
