from os import listdir,makedirs,sys
from os.path import isfile,join,exists,dirname,realpath
import ConfigParser
from VideoFrameData import VideoFrameData
from CSVFrameData import CSVFrameData
from FrameConverter import FrameConverter
from hand_shape_client import HandShapeClient

port = 51243
ROOT_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..')

# Read config form files
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

firstFile = ( f for f in listdir(labelDirectory) if isfile(join(labelDirectory,f)) & f.endswith('.csv') ).next()

labelFrameData = CSVFrameData()
labelFrameData.load(join(labelDirectory, firstFile))

firstFile = firstFile[:-4]

skeletonFrameData = CSVFrameData()
skeletonFrameData.load(join(skeletonDirectory, firstFile+skeletonVideoSuffix+skeletonFilenameExtension))

srcVideoPath = join(videoDirectory,firstFile+depthVideoSuffix+videoFilenameExtension)
depthFrameData = VideoFrameData()
depthFrameData.load(srcVideoPath)

i = 0
resultImages = []

depthRetval,depthFrame = depthFrameData.readFrame()
labelRetval,labelFrame = labelFrameData.readFrame()
skeletonRetval,skeletonFrame = skeletonFrameData.readFrame()
if not depthRetval or not labelRetval or not skeletonRetval:
    exit

h,w = depthFrame.shape[0:2]
encodedFrame = FrameConverter().encode(depthFrame, labelFrame, skeletonFrame)

client = HandShapeClient('localhost', port)
client.send_data(encodedFrame)
