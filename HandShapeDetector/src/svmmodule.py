from svmutil import *
from svm import *
import sqlite3
import math
import numpy
import struct

#from cluster import KMeansClustering
import time
import random
#import matplotlib.pyplot as plt1  
import marshal, pickle
from basic import *
from constant_numbers import *



'''def get_classes_with_at_least_num_of_data(labels,num=2):
    ret=set()
    count={}
    for label in labels:
        if not count.has_key(label):
            count[label]=1
        else:
            count[label]+=1
    for label in labels:
        if count[label]>=num:
            ret.add(label)
    return ret'''

def train_svm_model(labels,data,para='-t 0 -c 1000 -b 1'):
    assert len(labels)==len(data)
    prob  = svm_problem(labels,data) 
#     param = svm_parameter('-t 0 -c 4 -b 1')
    param = svm_parameter(para)

    ## training the model
    m = svm_train(prob, param)
    return m

def test_svm_model(labels,data,m):
    p_labels, p_acc, p_vals = svm_predict([0], [data], m)
    return p_labels, p_acc, p_vals



#def test_svm(labels,data,variance,bin_num=4,level_num=2,level_num_hog=3,para='-s 0 -c 2048 -t 2 -g 0.5'):
#    train_labels,train_data,test_labels,test_data,initial_index_test,testClass2initialIndex=split_data(labels,data,get_classes_with_at_least_num_of_data(labels,num=3))
    '''Sparse Representation
    f_sr_l=open("F:/study/save/generating/sparse/sr_l.txt","w")
    f_sr_d=open("F:/study/save/generating/sparse/sr_d.txt","w")
    f_sr_d2=open("F:/study/save/generating/sparse/sr_d2.txt","w")
    f_sr_t=open("F:/study/save/generating/sparse/sr_t.txt","w")
    f_sr_l.write(str(train_labels).replace("["," ").replace("]"," ").replace(","," "))
    f_sr_d.write(str(train_data).replace("["," ").replace("]"," ").replace(","," "))
    f_sr_d2.write(str(test_data).replace("["," ").replace("]"," ").replace(","," "))
    f_sr_t.write(str(test_labels).replace("["," ").replace("]"," ").replace(","," "))
    f_sr_l.flush()
    f_sr_d.flush()   
    f_sr_d2.flush() 
    f_sr_t.flush() 
    '''
    '''Hu's invariant moment
    hu_d=open("F:/study/save/generating/hu/hu.txt")
    hu_d2=open("F:/study/save/generating/hu/hu2.txt")
    hu_l=open("F:/study/save/generating/hu/label.txt")
    hu_l2=open("F:/study/save/generating/hu/label2.txt")

    train_labels1=hu_l.readline().split(" ")
    train_data1=hu_d.readline().split(" ")
    test_labels1=hu_l2.readline().split(" ")
    test_data1=hu_d2.readline().split(" ")
    train_labels=[]
    train_data2=[]
    test_labels=[]
    test_data2=[]
    train_data=[[0] for col in range(80)]
    test_data=[[0] for col in range(8)]
    for i in range(len(train_labels1)-1):
        train_labels.append(int(train_labels1[i]))
    index=0
    for i in range(len(train_data1)-1):
        train_data2.append(float(train_data1[i]))
        for index in range(7):
            train_data[int(i/7)].append(train_data2[i])
            index+=1
    for i in range(len(test_labels1)-1):
        test_labels.append(int(test_labels1[i]))
    index=0
    for i in range(len(test_data1)-1):
        test_data2.append(float(test_data1[i]))
        for index in range(7):
            test_data[int(i/7)].append(test_data2[i])
            index+=1
'''
    '''svm_m1=train_svm_model(train_labels,train_data)
    svm_res1=test_svm_model(svm_m1,test_labels,test_data)
    #plot_a_graph();
    
    pred_labels=svm_res1[0];
    varianceSum1=0
    varianceSum2=0
    same=0
    different=0
    for i in range(len(pred_labels)):
        if(pred_labels[i]==test_labels[i]):
            varianceSum1+=variance[initial_index_test[i]]
            same+=1
        else:
            varianceSum2+=variance[initial_index_test[i]]
            different+=1
    print float(varianceSum1)/float(same),float(varianceSum2)/float(different)
    return pred_labels,test_labels,initial_index_test,testClass2initialIndex'''
    
    
    
    
    
    
    
    
    
    '''pred_three=[];
    right=0;
    rightonly=0;
    list1=[];
    list2=[];
    for i in range(0,len(test_labels)):
        pred_labels[i]=int(pred_labels[i]);
        third=pred_labels[i]%1000;
        second=(pred_labels[i]-third)/1000%1000;
        first=int(pred_labels[i]/1000000);
        pred_three.append([first,second,third]);
        if test_labels[i]==first or test_labels[i]==second or test_labels[i]==third:
            right=right+1;
        if test_labels[i]==first:
            rightonly=rightonly+1;
            list1.append(test_labels[i]);
            list2.append(first);'''
#        else:
#            print test_labels[i],first
            #print classNo2Label[test_labels[i]],classNo2Label[first]
#    str2="top3:right=%r,total=%r,accurary=%10.3f%%"%(right,len(test_labels),100*right/len(test_labels));
#    str1="top1:right=%r,total=%r,accurary=%10.3f%%"%(rightonly,len(test_labels),100*rightonly/len(test_labels));
#    print str2
#    print str1
 #   return 0;
