from dtw import dtw

class DTWProcessor(object):

    def __init__(self, norm, presenters):
        self.norm = norm
        self.presenters = presenters

    def cost_3d(self, a, b):
        return sum((a-b)**self.norm)**(1.0/self.norm)

    def process(self, preprocessed_data):
        dist, cost, path = dtw(preprocessed_data[0]['spline'], preprocessed_data[1]['spline'], dist=self.cost_3d)
        processed_data = {'dist': dist, 'cost': cost, 'path': path}

        for i in xrange(0, len(self.presenters)):
            self.presenters[i].display(processed_data)

        return processed_data
