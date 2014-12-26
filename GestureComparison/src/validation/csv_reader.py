from os.path import join
from numpy import genfromtxt

class CSVReader(object):

    def __init__(self, root_directory):
        self.root_directory = root_directory

    def set_file(self, filename):
        self.filename = filename

    def read(self):
        return genfromtxt(join(self.root_directory, self.filename), delimiter=',')
