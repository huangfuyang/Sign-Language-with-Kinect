import matplotlib.pyplot as plt
import matplotlib.cm as cm

class DTWPresenter(object):

    def display(self, data):
        path = data['path']
        cost = data['cost']

        fig = plt.figure(1)
        ax = fig.add_subplot(111)
        plot1 = plt.imshow(cost.T, origin='lower', cmap=cm.gray, interpolation='nearest')
        plot2 = plt.plot(path[0], path[1], 'w')
        xlim = ax.set_xlim((-0.5, cost.shape[0]-0.5))
        ylim = ax.set_ylim((-0.5, cost.shape[1]-0.5))
        plt.show()
