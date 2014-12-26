import numpy as np
import numpy.linalg as la

class DTWProcessor(object):

    def __init__(self, norm):
        self.norm = norm

    def cost_3d(self, a, b):
        return sum((a-b)**self.norm)**(1.0/self.norm)

    def initialize_dmatrix(self, rows, cols):
        d = np.zeros((rows,cols),dtype='float')

        d[:,0] = 1e6
        d[0,:] = 1e6
        d[0,0] = 0

        return d

    def dtw_distance(self, list1, list2, costf=lambda x,y: la.norm(x - y) ):

        n = len(list1)
        m = len(list2)
        dtw = self.initialize_dmatrix(n+1,m+1)

        for (i,x) in enumerate(list1):
            i += 1
            for (j,y) in enumerate(list2):
                j += 1

                cost = costf(x,y)
                dtw[i,j] = cost + min(dtw[i-1,j],dtw[i,j-1],dtw[i-1][j-1])

        return dtw[n,m]

    def process(self, preprocessed_data):
        cost = self.dtw_distance(preprocessed_data[0]['spline'], preprocessed_data[1]['spline'], self.cost_3d)
        print cost
