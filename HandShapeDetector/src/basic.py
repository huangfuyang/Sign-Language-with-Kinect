'''
Created on 2014-9-16

@author: lenovo
'''
import math
def dotproduct(v1, v2):
    return sum((a*b) for a, b in zip(v1, v2))

def length(v):
    return math.sqrt(dotproduct(v, v))

def angle(v1, v2):
#     print v1, v2
    if(length(v1) * length(v2)==0):
        return 0
    return math.acos(dotproduct(v1, v2) / (length(v1) * length(v2)))

def distance(x1,y1,z1,x2,y2,z2):
    return math.sqrt((x1-x2)**2+(y1-y2)**2+(z1-z2)**2)

def distance_2d(x1,y1,x2,y2):
    return math.sqrt((x1-x2)**2+(y1-y2)**2) 
 
def get_unit_vector(x1,y1,z1,x2,y2,z2):
#     print x1,y1,x2,y2
    if x1==x2 and y1==y2 and z1==z2:
        return 0,0
    return (float(x1-x2))/distance(x1,y1,z1,x2,y2,z2),(float(y1-y2))/distance(x1,y1,z1,x2,y2,z2),(float(z1-z2))/distance(x1,y1,z1,x2,y2,z2)

def get_2d_unit_vector(x1,y1,x2,y2):
#     print x1,y1,x2,y2
    if x1==x2 and y1==y2:
        return 0,0
    dis=distance_2d(x1,y1,x2,y2)
    return (float(x1-x2))/dis,(float(y1-y2))/dis


def get_vector(x1,y1,z1,x2,y2,z2,scale):
#     print x1,y1,x2,y2
    if x1==x2 and y1==y2 and z1==z2:
        return 0,0
    return (float(x1-x2)/float(scale)),(float(y1-y2)/float(scale)),(float(z1-z2)/float(scale))

'''def split_data(labels,data,classes_in_test=range(1,51)):
#     labels,data=shuffle_data(labels,data)
    test_indexes=[]
#     flag=[0]*(len(set(labels))+1)
    flag=set()
    for i in range(0,len(labels)):
        if not (labels[i] in flag) and labels[i] in classes_in_test:
            test_indexes.append(i)
            flag.add(labels[i])
    train_labels=[]
    train_data=[]
    test_labels=[]
    test_data=[]
    initial_index_test=[]
    testClass2initialIndex={}
    #for i in range(0, min(len(labels),len(data))-1):
    for i in range(0, len(labels)):
        if not (labels[i] in classes_in_test):
            continue
        if i in test_indexes:
            test_labels.append(labels[i])
#             test_data.append(convert_features(data[i]))
            test_data.append(data[i])
            initial_index_test.append(i)
            testClass2initialIndex[labels[i]]=i
        else:
            train_labels.append(labels[i])
#             train_data.append(convert_features(data[i]))
            train_data.append(data[i])
    return train_labels,train_data,test_labels,test_data,initial_index_test,testClass2initialIndex'''


def normalize_histogram(bins):
    total=sum(bins)
    if total==0:
        return [0.0]*len(bins)
    for i in range(0,len(bins)):
        bins[i]=float(bins[i])/float(total)
#     print bins
    return bins