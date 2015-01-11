from mayavi import mlab
from abstract_classes import AbstractPresenter
import numpy as np

class BodyBoxPresenter(AbstractPresenter):

    colormaps = ['Blues', 'Oranges', 'Greens']
    currentColorIndex = 0

    def __init__(self, name):
        super(BodyBoxPresenter, self).__init__(name)

    def display(self, data):
        figBSpline = mlab.figure('Original BSpline')
        points = np.array([
            data['body_box']['shoulder_left_3d'],
            data['body_box']['shoulder_right_3d'],
            data['body_box']['hip_right_3d'],
            data['body_box']['hip_left_3d'],
            data['body_box']['shoulder_left_3d']
        ])
        points = points[~np.all(points < 1e-6, axis=1)]
        mlab.plot3d(points[:,0],points[:,1],points[:,2], range(0,len(points[:,2])), tube_radius=0.0025, colormap=self.colormaps[self.currentColorIndex], figure=figBSpline)
        self.currentColorIndex = (self.currentColorIndex + 1) % len(self.colormaps)
