import numpy as np
from abstract_classes import AbstractProcessor

class BodyBoxExtractor(AbstractProcessor):
    def __init__(self, name, presenters):
        super(BodyBoxExtractor, self).__init__(name, presenters)

    def process(self, data):
        raw_data = data['raw_data']
        names = ['head', 'shoulder_left', 'shoulder_center', 'shoulder_right', 'elbow_left', 'elbow_right', 'wrist_left', 'wrist_right', 'hand_left', 'hand_right', 'spine', 'hip_left', 'hip_center', 'hip_right']
        data_mappings = [('3d', (0,3)), ('color', (3,5)), ('depth', xrange(5,7))]

        data['body_box'] = {}

        for i,name in enumerate(names):
            selected_data = raw_data[:, i*7:i*7+7]

            data['body_box'][name] = {}

            for d,(start,end) in data_mappings:
                data['body_box'][name][d] = {
                    'first': selected_data[0, start:end],
                    'last': selected_data[-1, start:end],
                    'min': np.min(selected_data[:, start:end], axis=0),
                    'mean': np.mean(selected_data[:, start:end], axis=0),
                    'max': np.max(selected_data[:, start:end], axis=0),
                    'std': np.std(selected_data[:, start:end], axis=0)
                }

        return data
