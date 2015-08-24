import numpy as np
import scipy.interpolate as si
from abstract_classes import AbstractProcessor

class BSplinePreprocessor(AbstractProcessor):

    def __init__(self, name, smoothing, sampling_rate, presenters):
        super(BSplinePreprocessor, self).__init__(name, presenters)
        self.smoothing = smoothing
        self.sampling_rate = sampling_rate

    def process(self, data):
        (tck, uu) = si.splprep(data['hands'].transpose(), s=self.smoothing)
        space = np.linspace(0,1,self.sampling_rate)

        data['tck'] = tck
        data['uu'] = uu
        data['spline'] = np.array(si.splev(space, tck)).transpose()

        return data
