from os import listdir,getcwd
from os.path import isfile, join
import csv
import cv2
import numpy as np
import math

# Constants
labelDirectory = getcwd()+'/../data/label'
videoDirectory = getcwd()+'/../data/video'
videoFilenameExtension = 'avi'
DEBUG_MODE = True
VISUALIZE_RESULT = True

# Read config
def readCSV(fileName):
    with open(fileName, 'r') as csvfile:
        return [tuple(line) for line in csv.reader(csvfile, delimiter=',', quotechar='\'')]        

# Read AVI video
def readVideo(fileName, frameCallback, labels):
    cap = cv2.VideoCapture(fileName)
    i = 0
    
    while(cap.isOpened()):
        ret, frame = cap.read()
        if (not ret) | (i>=len(labels)):
            break
            
        if VISUALIZE_RESULT:
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break;
                
        frameCallback(frame, labels[i])
        i = i+1
    cap.release()

# Extract hand image from video
def extractHand(frame, label):
    
    frameHeight,frameWidth,_ = frame.shape;
    resultImage = np.zeros((frameHeight,frameWidth*2,3), np.uint8)
    currentColumn = 0

    if len(label) > 1:
        currentColumn = 2
                
        if (label[1].lower()=='right') | (label[1].lower()=='left'):
            centerX,centerY,width,height,rotateAngle = label[currentColumn:currentColumn+5]
            center = (float(centerX),float(centerY))
            size = (float(width),float(height))
            rotateAngle = float(rotateAngle)
            
            rect = (center, size, rotateAngle)
            box = cv2.cv.BoxPoints(rect)
            box = np.int0(box)
            
            # Cropping
            if (rotateAngle < -45.):
                rotateAngle += 90.0
                width,height = height,width
            M = cv2.getRotationMatrix2D(center, rotateAngle, 1.0)
            dSize = (int(math.floor(float(width))),int(math.floor(float(height))))
            rotatedImage = cv2.warpAffine(frame, M, (frameHeight,frameWidth))
            croppedImage = cv2.getRectSubPix(rotatedImage, dSize, center)
            
            if VISUALIZE_RESULT:
                cv2.drawContours(frame,[box],0,(0,0,255),2)
                croppedHeight,croppedWidth,_ = croppedImage.shape
                resultImage[frameHeight/2-croppedHeight/2:frameHeight/2-croppedHeight/2+croppedHeight, 
                    frameWidth+frameWidth/2-croppedWidth/2:frameWidth++frameWidth/2-croppedWidth/2+croppedWidth] = croppedImage
            
    if DEBUG_MODE:
        print label
                        
    if VISUALIZE_RESULT:
        resultImage[:,0:frameWidth,:] = frame
        cv2.imshow('Depth Video', resultImage)

# Feature extraction using Caffe
# Train SVM model

fileList = [ f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) ]

if VISUALIZE_RESULT:
    cv2.namedWindow("Depth Video", cv2.cv.CV_WINDOW_NORMAL)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_FULLSCREEN)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_NORMAL)

for fileName in fileList:
    labels = readCSV(join(labelDirectory, fileName))
    readVideo(join(videoDirectory,'depth_'+fileName[:-3]+videoFilenameExtension), extractHand, labels)

if VISUALIZE_RESULT:
    cv2.destroyAllWindows()
