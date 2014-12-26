from mayavi import mlab

class BSplineDisplayer(object):

    colormaps = ['Blues', 'Oranges', 'Greens']
    currentColorIndex = 0

    def display(self, data):
        mlab.plot3d(data['spline'][:,0], data['spline'][:,1], data['spline'][:,2], range(0,len(data['spline'][:,0])), tube_radius=0.0025, colormap=self.colormaps[self.currentColorIndex])
        self.currentColorIndex = (self.currentColorIndex + 1) % len(self.colormaps)
