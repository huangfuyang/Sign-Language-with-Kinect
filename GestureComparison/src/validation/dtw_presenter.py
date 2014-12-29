import matplotlib.pyplot as plt
import matplotlib.cm as cm
from mayavi import mlab

class DTWPresenter(object):

    def display(self, data):
        input_data = data['input']
        path = data['output']['path']
        cost = data['output']['cost']

        fig = plt.figure(1)
        ax = fig.add_subplot(111)
        plot1 = plt.imshow(cost.T, origin='lower', cmap=cm.gray, interpolation='nearest')
        plot2 = plt.plot(path[0], path[1], 'w')
        xlim = ax.set_xlim((-0.5, cost.shape[0]-0.5))
        ylim = ax.set_ylim((-0.5, cost.shape[1]-0.5))
        plt.show()

        figDTW = mlab.figure('DTW BSpline')
        mlab.plot3d(input_data[0]['spline'][:,0], input_data[0]['spline'][:,1], input_data[0]['spline'][:,2], range(0,len(input_data[0]['spline'][:,0])), tube_radius=0.0025, colormap='Blues', figure=figDTW)
        mlab.plot3d(input_data[1]['spline'][:,0], input_data[1]['spline'][:,1], input_data[1]['spline'][:,2], range(0,len(input_data[1]['spline'][:,0])), tube_radius=0.0025, colormap='Oranges', figure=figDTW)

        for i in xrange(0, len(path[0])):
            point1 = input_data[0]['spline'][path[0][i]]
            point2 = input_data[1]['spline'][path[1][i]]
            mlab.plot3d([point1[0], point2[0]], [point1[1], point2[1]], [point1[2], point2[2]], tube_radius=0.001, color=(1,1,0), figure=figDTW)
