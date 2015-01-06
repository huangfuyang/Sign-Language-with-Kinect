
'''
Created on Oct 14, 2014

@author: liuzz
'''
'''
Another clusting method , k-medoids.
See more : http://en.wikipedia.org/wiki/K-medoids
The most common realisation of k-medoid clustering is the Partitioning Around Medoids (PAM) algorithm and is as follows:[2]
1. Initialize: randomly select k of the n data points as the medoids
2. Associate each data point to the closest medoid. ("closest" here is defined using any valid distance metric, most commonly Euclidean distance, Manhattan distance or Minkowski distance)
3. For each medoid m
     For each non-medoid data point o
         Swap m and o and compute the total cost of the configuration
4. Select the configuration with the lowest cost.
5. repeat steps 2 to 4 until there is no change in the medoid.
'''
from clusterBase import importData, pearson_distance 
import random
from numpy import *
distances_cache = {}
 
def totalcost(blogwords, costf, medoids_idx) :
    size = len(blogwords)
    total_cost = 0.0
    medoids = {}
    for idx in medoids_idx :
        medoids[idx] = []
    for i in range(size) :
        choice = None
        min_cost = 2.1
        for m in medoids :
            tmp = distances_cache.get((m,i),None)
            if tmp == None :
                tmp = pearson_distance(blogwords[m],blogwords[i])
                distances_cache[(m,i)] = tmp
            if tmp < min_cost :
                choice = m
                min_cost = tmp
        medoids[choice].append(i)
        total_cost += min_cost
    return total_cost, medoids
     
'''
def kmedoids(blogwords, k) :
    size = len(blogwords)
    medoids_idx = random.sample([i for i in range(size)], k)
    pre_cost, medoids = totalcost(blogwords,pearson_distance,medoids_idx)
    print pre_cost
    current_cost = 2.1 * size # maxmum of pearson_distances is 2.    
    best_choice = []
    best_res = {}
    iter_count = 0
    counter=0
    while 1 :
        counter+=1
        for m in medoids :
            for item in medoids[m] :
#                if item != m :
                    idx = medoids_idx.index(m)
                    swap_temp = medoids_idx[idx]
                    medoids_idx[idx] = item
                    tmp,medoids_ = totalcost(blogwords,pearson_distance,medoids_idx)
                    #print tmp,'-------->',medoids_.keys()
                    if tmp < current_cost :
                        best_choice = list(medoids_idx)
                        best_res = dict(medoids_)
                        current_cost = tmp
                    medoids_idx[idx] = swap_temp
        iter_count += 1
#        print current_cost,iter_count

        if best_choice == medoids_idx: break
        if current_cost <= pre_cost :
            pre_cost = current_cost
            medoids = best_res
            medoids_idx = best_choice
         
     
    return current_cost, best_choice, best_res
 '''
def kmedoids(blogword,k):
    l=len(blogword)
    v=zeros((1,l))
    m1=zeros((l,l))
    for i in range(l):
        v[0,i]=dot(blogword[i,:],blogword[i,:])
    for i in range(l):
        m1[i,:]=v
    m2=transpose(m1)
    m3=m1+m2
    m4=dot(blogword,transpose(blogword))*2
    d=m3-m4
    ds=sum(d,0)
    ret=argmin(ds)
    return ret
    
    
    
    
    
    
    
'''
    v=[]
    for j in range(l):
        sum=0
        for i in range(4356):
            sum+=blogword[j][i]**2
        v.append(sum)
    m1=[[0 for col in range(l)] for row in range(l)]
    for j in range(l):
        for i in range(l):
            m1[j][i]=v[i]
    for j in range(l):
        for i in range(l):
            m1[j][i]+=v[j]
    m2=[[0 for row in range(l)]for col in range(l)]
    m3=[[0 for row in range(l)]for col in range(l)]
    for x in range(l):
        for y in range(l):
            tmp=0
            for j in range(4356):
                tmp+=blogword[x][j]*blogword[y][j]
            m2[x][y]=tmp*2
    for x in range(l):
        for y in range(l):
            m3[x][y]=m1[x][y]-m2[x][y]
    ds=[]
    for x in range(l):
        tm=0
        for y in range(l):  
            tm+=m3[x][y]  
        ds.append(tm) 
    mini=ds[0]
    for x in range(l):
        if ds[x]<mini:
            mini=ds[x]
            x0=x
    return x0'''
     
    
def print_match(best_medoids, blognames) :
    for medoid in best_medoids :
        print blognames[medoid],'----->',
        for m in best_medoids[medoid] :
            print '(',m,blognames[m],')',
        print
        print '---------' * 20
 
if __name__ == '__main__' :
#    f = open("C:/Users/liuzz/Desktop/1.txt") 
#    blogwords, blognames = importData()
    blogwords=[[1,2,3],[1,2,3],[2,1,1]]
    blognames=[1,1,2]
    best_cost,best_choice,best_medoids = kmedoids(blogwords,1)
    print_match(best_medoids,blognames)
    a=1
    