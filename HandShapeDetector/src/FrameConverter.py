import base64
from json import JSONEncoder,JSONDecoder
import numpy as np

class FrameConverter(object):

    def __init__(self):
        self.debug = True
        self.encoder = JSONEncoder()
        self.decoder = JSONDecoder()

    def setDebug(self, debug):
        self.debug = debug

    def encode(self, depthFrame, labelFrame, skeletonFrame):
        encodedDepthFrame = base64.b64encode(depthFrame)
        depthFrameShape = depthFrame.shape

        encodedObject = {
            'depth': {
                'image': encodedDepthFrame,
                'shape': depthFrameShape
            },
            'label': labelFrame,
            'skeleton': skeletonFrame
        }
        encodedJSON = self.encoder.encode(encodedObject)
        if self.debug:
            decodedFrame = self.decode(encodedJSON)
            assert np.array_equal(decodedFrame['depth_image'], depthFrame)
            assert np.array_equal(decodedFrame['label'], labelFrame)
            assert np.array_equal(decodedFrame['skeleton'], skeletonFrame)

        return encodedJSON

    def decode(self, json):
        decodedDict = self.decoder.decode(json)

        depthFrameShape = decodedDict['depth']['shape']
        encodedDepthFrame = decodedDict['depth']['image']
        depthFrame = base64.decodestring(encodedDepthFrame)
        depthFrame = np.frombuffer(depthFrame,dtype='uint8')
        depthFrame = depthFrame.reshape(depthFrameShape)
        labelFrame = decodedDict['label']
        skeletonFrame = decodedDict['skeleton']

        return {
            'depth_image': depthFrame,
            'label': labelFrame,
            'skeleton': skeletonFrame
        }
