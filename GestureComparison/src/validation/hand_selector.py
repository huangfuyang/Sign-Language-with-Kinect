import numpy as np
from abstract_classes import AbstractProcessor

class HandSelector(AbstractProcessor):
    def __init__(self, name, select_left, select_right):
        super(HandSelector, self).__init__(name, None)
        self.select_left = select_left
        self.select_right = select_right

    def process(self, data):
        if self.select_left and self.select_right:
            return np.concatenate((data[:,56:59], data[:,63:66]))
        elif self.select_left:
            return data[:,56:59]
        elif self.select_right:
            return data[:,63:66]
        else:
            return None
