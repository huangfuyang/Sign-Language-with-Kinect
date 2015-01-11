import numpy as np
from abstract_classes import AbstractProcessor

class BodyBoxExtractor(AbstractProcessor):
    def __init__(self, name, presenters):
        super(BodyBoxExtractor, self).__init__(name, presenters)

    def process(self, data):
        raw_data = data['raw_data']

        data['body_box'] = {
            'shoulder_left_3d': np.mean(raw_data[:, 7:10], axis=0),
            'shoulder_right_3d': np.mean(raw_data[:, 21:24], axis=0),
            'hip_left_3d': np.mean(raw_data[:, 77:80], axis=0),
            'hip_right_3d': np.mean(raw_data[:, 91:94], axis=0)
        }

        return data
