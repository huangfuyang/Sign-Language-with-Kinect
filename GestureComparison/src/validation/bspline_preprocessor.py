import numpy as np
import scipy.interpolate as si

class BSplinePreprocessor(object):

    def __init__(self, smoothing, sampling_rate, displayers):
        self.smoothing = smoothing
        self.sampling_rate = sampling_rate
        self.displayers = displayers

    def preprocess(self, data):
        (tck, uu) = si.splprep(data.transpose(), s=self.smoothing)
        space = np.linspace(0,1,self.sampling_rate)
        spline = np.array(si.splev(space, tck)).transpose()

        preprocessed_data = {'spline': spline, 'tck': tck, 'uu': uu}

        for i in xrange(0, len(self.displayers)):
            self.displayers[i].display(preprocessed_data)

        return preprocessed_data
