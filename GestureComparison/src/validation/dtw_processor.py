from dtw import dtw
from abstract_classes import AbstractProcessor

class DTWProcessor(AbstractProcessor):

    def __init__(self, name, norm, presenters):
        super(DTWProcessor, self).__init__(name, presenters)
        self.norm = norm

    def cost_3d(self, a, b):
        return sum((a-b)**self.norm)**(1.0/self.norm)

    def process(self, data):
        dist, cost, path = dtw(data[0]['spline'], data[1]['spline'], dist=self.cost_3d)
        processed_data = {'input': data, 'output': {'dist': dist, 'cost': cost, 'path': path}}

        return processed_data
