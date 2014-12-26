import numpy as np

class HandSelector(object):
    def __init__(self, select_left, select_right):
        self.select_left = select_left
        self.select_right = select_right

    def preprocess(self, data):
        if self.select_left and self.select_right:
            return np.concatenate((data[:,56:59], data[:,63:66]))
        elif self.select_left:
            return data[:,56:59]
        elif self.select_right:
            return data[:,63:66]
        else:
            return None
