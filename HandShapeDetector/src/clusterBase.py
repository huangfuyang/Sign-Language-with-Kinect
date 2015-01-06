'''
Created on Oct 14, 2014

@author: liuzz
'''
from math import sqrt
 
def importData(FIFE = 'D:/eclipse/project/src/blogdata.txt') :
    blogwords = []
    blognames = []
    f = open("C:/Users/liuzz/Desktop/1.txt") 
    words = f.readline().split()
    #//remove '\r\n'
    for line in f:    
        blog = line[:-2].split('\t')
        blognames.append(blog[0])        
        blogwords.append([int(word_c) for word_c in blog[1:]]       ) 
    return blogwords,blognames
 
 
def pearson_distance(vector1, vector2) :
    """
    Calculate distance between two vectors using pearson method
    See more : http://en.wikipedia.org/wiki/Pearson_product-moment_correlation_coefficient
    """
    sum1 = sum(vector1)
    sum2 = sum(vector2)
 
    sum1Sq = sum([pow(v,2) for v in vector1])
    sum2Sq = sum([pow(v,2) for v in vector2])
 
    pSum = sum([vector1[i] * vector2[i] for i in range(len(vector1))])
 
    num = pSum - (sum1*sum2/len(vector1))
    den = sqrt((sum1Sq - pow(sum1,2)/len(vector1)) * (sum2Sq - pow(sum2,2)/len(vector1)))
 
    if den == 0 : return 0.0
    return 1.0 - num/den