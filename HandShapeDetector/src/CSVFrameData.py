import csv

class CSVFrameData:

    def __init__(self):
        self.debug = False

    def load(self, fileName):
        self.index = 0
        with open(fileName, 'r') as csvfile:
            self.data = [tuple(line) for line in csv.reader(csvfile, delimiter=',', quotechar='\'')]
            csvfile.close()

    def setDebug(self, debug):
        self.debug = debug

    def readFrame(self):
        if self.index < len(self.data):
            self.index = self.index + 1
            if self.debug:
                print self.data[self.index-1]

            return True,self.data[self.index-1]
        else:
            return False,None

    def close(self):
        return True
