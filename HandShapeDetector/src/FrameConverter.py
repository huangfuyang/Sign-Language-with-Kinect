import base64
from json import JSONEncoder,JSONDecoder
import numpy as np

class FrameConverter:

    def __init__(self):
        self.debug = True
        self.encoder = JSONEncoder()
        self.decoder = JSONDecoder()

    def setDebug(self, debug):
        self.debug = debug

    def encode(self, depthFrame, labelFrame, skeletonFrame):
        encodedDepthFrame = base64.b64encode(depthFrame)
        depthFrameShape = depthFrame.shape

        if self.debug:
            decodedDepthImage = base64.decodestring(encodedDepthFrame)
            decodedDepthImage = np.frombuffer(decodedDepthImage,dtype='uint8')
            decodedDepthImage = decodedDepthImage.reshape(depthFrameShape)
            assert np.array_equal(decodedDepthImage, depthFrame)

        encodedObject = {
            'depth': {
                'image': encodedDepthFrame,
                'shape': depthFrameShape
            },
            'label': labelFrame,
            'skeleton': skeletonFrame
        }
        encodedJSON = self.encoder.encode(encodedObject)

        return encodedJSON

    def decode(self, json):
        pass
