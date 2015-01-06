'''
Created on 2014-9-16

@author: lenovo
'''
from basic import *
from constant_numbers import *

import math
def get_bin_for_vec(vec_2d,bin_num):
    if vec_2d==(0,0):
        return 0
    ang=angle(vec_2d,(1,0))
    if vec_2d[1]<0:
        ang=2*math.pi-ang
    ret= int(ang/(2*math.pi)*bin_num)
    return ret

def hod(features,bin_num=8,level_num=2):
    his=[]
    his+=hod_of_a_joint_origin(features,3,4,5,bin_num,level_num)
    his+=hod_of_a_joint_origin(features,9,10,11,bin_num,level_num)
    his+=hod_of_a_joint_origin(features,12,13,14,bin_num,level_num)
    right=hod_of_a_joint(features,27,28,29,bin_num,level_num)
    his+=list(right)[0]
    right_v=right[1]
    right_h=right[2]
    
    his+=hod_of_a_joint_origin(features,15,16,17,bin_num,level_num)
    his+=hod_of_a_joint_origin(features,18,19,20,bin_num,level_num)
    his+=hod_of_a_joint_origin(features,21,22,23,bin_num,level_num)
    his+=hod_of_a_joint_origin(features,24,25,26,bin_num,level_num)

#     print len(his),his
    return his,right_v,right_h

def hod_of_a_joint_origin(features,x_index,y_index,z_index,bin_num,level_num):  
    his=get_hod_for_a_node_of_pyramid_origin(features,x_index,y_index,z_index,bin_num,1,level_num)              
    return his

def hod_of_a_joint(features,x_index,y_index,z_index,bin_num,level_num):  
    ret=get_hod_for_a_node_of_pyramid(features,x_index,y_index,z_index,bin_num,1,level_num) 
    his=ret[0]
    right_v=ret[1]             
    height=ret[2]
    return his,right_v,height


def get_hod_for_a_node_of_pyramid_origin(features,x_index,y_index,z_index,bin_num,level_index,level_num):
    try:
        if level_index==level_num:
            return []
        px=features[0][x_index]
        py=features[0][y_index]
        pz=features[0][z_index]
        bin_xy=[0]*bin_num
        bin_xz=[0]*bin_num
        bin_yz=[0]*bin_num
        for i in range(1,len(features)):
            x=features[i][x_index]
            y=features[i][y_index]
            z=features[i][z_index]
            v_xy=(y-py,x-px)
            v_xz=(z-pz,x-px)
            v_yz=(z-pz,y-py)
            bin_xy[get_bin_for_vec(v_xy,bin_num)]+=length(v_xy)
            bin_xz[get_bin_for_vec(v_xz,bin_num)]+=length(v_xz)
            bin_yz[get_bin_for_vec(v_yz,bin_num)]+=length(v_yz)

    #         print bin_xz
            px=x
            py=y
            pz=z


        left_hod=get_hod_for_a_node_of_pyramid_origin(features[0:len(features)/2],x_index,y_index,z_index,bin_num,level_index+1,level_num)
        right_hod=get_hod_for_a_node_of_pyramid_origin(features[len(features)/2+1:len(features)],x_index,y_index,z_index,bin_num,level_index+1,level_num)
        bin_xy=normalize_histogram(bin_xy)
        bin_xz=normalize_histogram(bin_xz)
        bin_yz=normalize_histogram(bin_yz)
        return bin_xy+bin_xz+bin_yz+left_hod+right_hod
    except:
        return [0,0,0,0,0,0,0,0,0]
def get_hod_for_a_node_of_pyramid(features,x_index,y_index,z_index,bin_num,level_index,level_num):
    try:
        if level_index==level_num:
            return []
        length_list=[]
        px=features[0][x_index]
        py=features[0][y_index]
        pz=features[0][z_index]
        bin_xy=[0]*bin_num
        bin_xz=[0]*bin_num
        bin_yz=[0]*bin_num
        flag=0
        y0=py
        right_v=[0]*len(features)
        right_h=[0]*len(features)
        for i in range(1,len(features)):
            x=features[i][x_index]
            y=features[i][y_index]
            z=features[i][z_index]
            v_xy=(y-py,x-px)
            v_xz=(z-pz,x-px)
            v_yz=(z-pz,y-py)
            bin_xy[get_bin_for_vec(v_xy,bin_num)]+=length(v_xy)
            bin_xz[get_bin_for_vec(v_xz,bin_num)]+=length(v_xz)
            bin_yz[get_bin_for_vec(v_yz,bin_num)]+=length(v_yz)

            len_of_v=math.sqrt((x-px)**2+(y-py)**2+(z-pz)**2);
            right_v[i]=len_of_v
            right_h[i]=y
        left_hod=get_hod_for_a_node_of_pyramid_origin(features[0:len(features)/2],x_index,y_index,z_index,bin_num,level_index+1,level_num)
        right_hod=get_hod_for_a_node_of_pyramid_origin(features[len(features)/2+1:len(features)],x_index,y_index,z_index,bin_num,level_index+1,level_num)
        bin_xy=normalize_histogram(bin_xy)
        bin_xz=normalize_histogram(bin_xz)
        bin_yz=normalize_histogram(bin_yz)
        return bin_xy+bin_xz+bin_yz+left_hod+right_hod,right_v,right_h
    except:
        return [0,0,0,0,0,0,0,0,0],[0]*len(features),[0]*len(features)