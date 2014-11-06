from os import listdir
from os.path import isfile, join
import csv

# Read config
def readCSV(fileName):
    with open(fileName, 'r') as csvfile:
        return [tuple(line) for line in csv.reader(csvfile, delimiter=',', quotechar='\'')]        

# Read AVI video
# Extract hand image from video
# Feature extraction using Caffe
# Train SVM model

labelDirectory = '../data/label'
fileList = [ f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) ]

for fileName in fileList:
    labels = readCSV(join(labelDirectory,fileName))
