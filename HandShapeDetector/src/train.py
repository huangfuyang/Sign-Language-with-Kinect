from os import listdir,getcwd
from os.path import isfile, join
import csv
import cv2
import numpy as np
import math

# Constants
labelDirectory = getcwd()+'/../data/label'
videoDirectory = getcwd()+'/../data/video'
resultDirectory = getcwd()+'/../result'
videoFilenameExtension = '.avi'
DEBUG_MODE = True
VISUALIZE_RESULT = True
SAVE_RESULT_VIDEO = True

# Read config
def readCSV(fileName):
    with open(fileName, 'r') as csvfile:
        return [tuple(line) for line in csv.reader(csvfile, delimiter=',', quotechar='\'')]        

def readVideoFrame(cap):
    if(cap.isOpened()):
        ret, frame = cap.read()
        if not ret:
            return False,None
            
        if VISUALIZE_RESULT:
            if cv2.waitKey(1) & 0xFF == ord('q'):
                return False,None

        return True,frame

    return False,None
                
# Read AVI video
def readVideo(fileName, frameCallback, labels, result):
    srcVideoPath = join(videoDirectory,'depth_'+fileName+videoFilenameExtension)
    cap = cv2.VideoCapture(srcVideoPath)
    i = 0
    resultImages = []
    
    retval,frame = readVideoFrame(cap)
    h,w = frame.shape[0:2]
    print(type(h))
    if SAVE_RESULT_VIDEO:
        videoPath = join(resultDirectory, 'croppedHand-'+fileName+'.avi')
        videoWriter = cv2.VideoWriter()
        fourcc = cv2.cv.CV_FOURCC('m', 'p', '4', 'v')
        videoWriter.open(videoPath, fourcc, 30, (w*2,h))
    else:
        videoWriter = None

    while((i<len(labels)) & retval):
        res,resultImage = frameCallback(frame, labels[i], videoWriter)
        resultImages.append(resultImage)
        i = i+1
        retval,frame = readVideoFrame(cap)
    cap.release()
    
    if SAVE_RESULT_VIDEO & (videoWriter is not None):
        for i in xrange(1,len(resultImages)):
            videoWriter.write(resultImages[i])

# Extract hand image from video
def extractHand(frame, label, videoWriter=None):
    
    frameHeight,frameWidth,_ = frame.shape;
    croppedImage = None
    resultImage = np.zeros((frameHeight,frameWidth*2,3), np.uint8)
    currentColumn = 0

    if len(label) > 1:
        currentColumn = 2
        thresh = float(label[-1])
                
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
            retval, croppedImage = cv2.threshold(croppedImage, thresh, 256., cv2.THRESH_TOZERO_INV)
            
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
        
    return croppedImage,resultImage

# Feature extraction using Caffe
# Train SVM model

fileList = [ f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) ]

if VISUALIZE_RESULT:
    cv2.namedWindow("Depth Video", cv2.cv.CV_WINDOW_NORMAL)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_FULLSCREEN)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_NORMAL)

for fileName in fileList:
    result = []
    labels = readCSV(join(labelDirectory, fileName))
    readVideo(fileName[:-4], extractHand, labels, result)

if VISUALIZE_RESULT:
    cv2.destroyAllWindows()
