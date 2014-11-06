from os import listdir,getcwd
from os.path import isfile, join
import csv
import cv2

# Constants
labelDirectory = getcwd()+'/../data/label'
videoDirectory = getcwd()+'/../data/video'
videoFilenameExtension = 'avi'
DEBUG_MODE = True

# Read config
def readCSV(fileName):
    with open(fileName, 'r') as csvfile:
        return [tuple(line) for line in csv.reader(csvfile, delimiter=',', quotechar='\'')]        

# Read AVI video
def readVideo(fileName, frameCallback, labels):
    cap = cv2.VideoCapture(fileName)
    i = 0
    
    if DEBUG_MODE:
        cv2.namedWindow("Depth Video", cv2.cv.CV_WINDOW_NORMAL);
        cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_FULLSCREEN);
        cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_NORMAL);
    
    while(cap.isOpened()):
        ret, frame = cap.read()
        if (not ret) | (i>=len(labels)) | (cv2.waitKey(1) & 0xFF == ord('q')):
            break
        frameCallback(frame, labels[i])
        i = i+1
    cap.release()

# Extract hand image from video
def extractHand(frame, label):
    if DEBUG_MODE:
        print label
        cv2.imshow('Depth Video', frame)

# Feature extraction using Caffe
# Train SVM model

fileList = [ f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) ]

for fileName in fileList:
    labels = readCSV(join(labelDirectory, fileName))
    readVideo(join(videoDirectory,'depth_'+fileName[:-3]+videoFilenameExtension), extractHand, labels)
    cv2.destroyAllWindows()
