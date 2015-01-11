import numpy as np
from abstract_classes import AbstractProcessor

class HandSelector(AbstractProcessor):
    def __init__(self, name, select_left, select_right):
        super(HandSelector, self).__init__(name, None)
        self.select_left = select_left
        self.select_right = select_right

    def process(self, data):
        raw_data = data['raw_data']
        if self.select_left and self.select_right:
            data['hands'] = np.concatenate((raw_data[:,56:59], raw_data[:,63:66]))
        elif self.select_left:
            data['hands'] = raw_data[:,56:59]
        elif self.select_right:
            data['hands'] = raw_data[:,63:66]

        return data
