from os import listdir,makedirs,sys
from os.path import isfile,join,exists,dirname,realpath
from sys import exit
import ConfigParser
import cv2
import numpy as np
import math
from VideoFrameData import VideoFrameData
from CSVFrameData import CSVFrameData
from FrameConverter import FrameConverter

# Constants
labelDirectory = ''
skeletonDirectory = ''
videoDirectory = ''
resultDirectory = ''
videoFilenameExtension = ''
skeletonFilenameExtension = ''
skeletonVideoSuffix = ''
depthVideoSuffix = ''
colorVideoSuffix = ''
DEBUG_MODE = False
VISUALIZE_RESULT = False
SAVE_RESULT_VIDEO = False

ROOT_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..')

def init():
    global labelDirectory,skeletonDirectory,videoDirectory,resultDirectory
    global videoFilenameExtension,skeletonFilenameExtension
    global skeletonVideoSuffix,depthVideoSuffix,colorVideoSuffix
    global DEBUG_MODE,VISUALIZE_RESULT,SAVE_RESULT_VIDEO

    config = ConfigParser.RawConfigParser()
    config.read(join(ROOT_DIRECTORY, 'config', 'file_format.cfg'))
    labelDirectory = join(ROOT_DIRECTORY, config.get('Directory', 'Label'))
    videoDirectory = join(ROOT_DIRECTORY, config.get('Directory', 'Video'))
    skeletonDirectory = join(ROOT_DIRECTORY, config.get('Directory', 'Skeleton'))
    resultDirectory = join(ROOT_DIRECTORY, config.get('Directory', 'Result'))
    videoFilenameExtension = config.get('File', 'Video Extension')
    skeletonFilenameExtension = config.get('File', 'Skeleton Extension')
    skeletonVideoSuffix = config.get('File', 'Skeleton Suffix')
    depthVideoSuffix = config.get('File', 'Depth Video Suffix')
    colorVideoSuffix = config.get('File', 'Color Video Suffix')

    config.read(join(ROOT_DIRECTORY, 'config', 'debug.cfg'))
    DEBUG_MODE = config.getboolean('Debug', 'Print Debug Message')
    VISUALIZE_RESULT = config.getboolean('Debug', 'Visualize Result')
    SAVE_RESULT_VIDEO = config.getboolean('Debug', 'Save Result to Video')
    for directory in [labelDirectory,videoDirectory,resultDirectory]:
        if not exists(directory):
            makedirs(directory)

    return [ f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) & f.endswith('.csv') ]

# Read AVI video
def readVideo(fileName, frameCallback, result):
    frameConverter = FrameConverter()

    labelFrameData = CSVFrameData()
    labelFrameData.load(join(labelDirectory, fileName))
    labelFrameData.setDebug(DEBUG_MODE)

    fileName = fileName[:-4]

    skeletonFrameData = CSVFrameData()
    skeletonFrameData.load(join(skeletonDirectory, fileName+skeletonVideoSuffix+skeletonFilenameExtension))
    skeletonFrameData.setDebug(DEBUG_MODE)

    srcVideoPath = join(videoDirectory,fileName+depthVideoSuffix+videoFilenameExtension)
    depthFrameData = VideoFrameData()
    depthFrameData.load(srcVideoPath)

    i = 0
    resultImages = []

    depthRetval,depthFrame = depthFrameData.readFrame()
    labelRetval,labelFrame = labelFrameData.readFrame()
    skeletonRetval,skeletonFrame = skeletonFrameData.readFrame()
    if not depthRetval or not labelRetval or not skeletonRetval:
        return

    h,w = depthFrame.shape[0:2]
    encodedFrame = frameConverter.encode(depthFrame, labelFrame, skeletonFrame)

    if SAVE_RESULT_VIDEO:
        videoPath = join(resultDirectory, 'croppedHand-'+fileName+videoFilenameExtension)
        videoWriter = cv2.VideoWriter()
        fourcc = cv2.cv.CV_FOURCC('m', 'p', '4', 'v')
        videoWriter.open(videoPath, fourcc, 30, (w*2,h))
    else:
        videoWriter = None

    while(labelRetval and depthRetval and skeletonRetval):
        encodedFrame = frameConverter.encode(depthFrame, labelFrame, skeletonFrame)
        res,resultImage = frameCallback(depthFrame, labelFrame, videoWriter)
        resultImages.append(resultImage)

        if VISUALIZE_RESULT:
            if cv2.waitKey(1) & 0xFF == ord('q'):
                depthRetval = False
                continue

        depthRetval,depthFrame = depthFrameData.readFrame()
        labelRetval,labelFrame = labelFrameData.readFrame()
        skeletonRetval,skeletonFrame = skeletonFrameData.readFrame()
    depthFrameData.close()
    labelFrameData.close()
    skeletonFrameData.close()

    if SAVE_RESULT_VIDEO & (videoWriter is not None):
        message = "Saving Video..."
        savingImage = np.zeros((h,w*2,3), np.uint8)
        savingImageTextSize, _ = cv2.getTextSize(message, cv2.FONT_HERSHEY_SIMPLEX, 1, 2)
        savingImageTextLocation = (w-savingImageTextSize[0]/2, h/2-savingImageTextSize[1]/2)
        for i in xrange(0,len(resultImages)):
            savingImage[:] = 0
            cv2.rectangle(savingImage, (0,savingImageTextLocation[1]), (i*w*2/len(resultImages),savingImageTextLocation[1]+savingImageTextSize[1]), (0,255,0), cv2.cv.CV_FILLED)
            cv2.putText(savingImage, message, savingImageTextLocation, cv2.FONT_HERSHEY_SIMPLEX, 1, (255,255,255), 2, cv2.CV_AA)
            cv2.imshow('Depth Video', savingImage)
            videoWriter.write(resultImages[i])

# Extract hand image from video
def extractHand(frame, label, videoWriter=None):

    frameHeight,frameWidth,_ = frame.shape;
    croppedImage = None
    resultImage = np.zeros((frameHeight,frameWidth*2,3), np.uint8)
    currentColumn = 0

    if len(label) > 1:
        currentColumn = 2

        if label[1].lower() != 'none':
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
                croppedImageWithBorder = cv2.copyMakeBorder(croppedImage, 3, 3, 3, 3, cv2.BORDER_CONSTANT, None, (255,255,255))
                croppedHeight,croppedWidth,_ = croppedImageWithBorder.shape
                resultImage[frameHeight/2-croppedHeight/2:frameHeight/2-croppedHeight/2+croppedHeight,
                    frameWidth+frameWidth/2-croppedWidth/2:frameWidth++frameWidth/2-croppedWidth/2+croppedWidth] = croppedImageWithBorder

    if VISUALIZE_RESULT:
        resultImage[:,0:frameWidth,:] = frame
        cv2.imshow('Depth Video', resultImage)

    return croppedImage,resultImage

# Feature extraction using Caffe
# Train SVM model

fileList = init()

if len(fileList) < 1:
    print 'No CSV file is found'
    exit()

if VISUALIZE_RESULT:
    cv2.namedWindow("Depth Video", cv2.cv.CV_WINDOW_NORMAL)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_FULLSCREEN)
    cv2.setWindowProperty("Depth Video", cv2.WND_PROP_FULLSCREEN, cv2.cv.CV_WINDOW_NORMAL)

for fileName in fileList:
    result = []
    readVideo(fileName, extractHand, result)

if VISUALIZE_RESULT:
    cv2.destroyAllWindows()
