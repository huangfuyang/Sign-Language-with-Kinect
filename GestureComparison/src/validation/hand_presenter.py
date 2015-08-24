from mayavi import mlab
from abstract_classes import AbstractPresenter

class HandPresenter(AbstractPresenter):

    colormaps = ['Blues', 'Oranges', 'Greens']
    currentColorIndex = 0

    def __init__(self, name):
        super(HandPresenter, self).__init__(name)

    def display(self, data):
        figBSpline = mlab.figure('Original BSpline')
        mlab.plot3d(data['hands'][:,0], data['hands'][:,1], data['hands'][:,2], range(0,len(data['raw_data'][:,0])), tube_radius=0.0025, colormap=self.colormaps[self.currentColorIndex], figure=figBSpline)
        self.currentColorIndex = (self.currentColorIndex + 1) % len(self.colormaps)
