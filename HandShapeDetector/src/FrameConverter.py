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

    def encode(self, depthFrame, colorFrame, labelFrame, skeletonFrame):
        encodedObject = {
            'depth': self.encode_image(depthFrame),
            'color': self.encode_image(colorFrame),
            'label': labelFrame,
            'skeleton': skeletonFrame
        }
        encodedJSON = self.encoder.encode(encodedObject)
        if self.debug:
            decodedFrame = self.decode(encodedJSON)
            assert np.array_equal(decodedFrame['depth_image'], depthFrame)
            assert np.array_equal(decodedFrame['color_image'], colorFrame)
            assert np.array_equal(decodedFrame['label'], labelFrame)
            assert np.array_equal(decodedFrame['skeleton'], skeletonFrame)

        return encodedJSON

    def decode(self, json):
        decodedDict = self.decoder.decode(json)
        depthFrame = self.decode_image(decodedDict['depth'])
        colorFrame = self.decode_image(decodedDict['color'])
        labelFrame = decodedDict['label']
        skeletonFrame = decodedDict['skeleton']

        return {
            'depth_image': depthFrame,
            'color_image': colorFrame,
            'label': labelFrame,
            'skeleton': skeletonFrame
        }

    def encode_image(self, original_image):
        encoded_image = base64.b64encode(original_image)
        image_shape = original_image.shape
        return {
            'image': encoded_image,
            'shape': image_shape
        }

    def decode_image(self, encoded_image_frame):
        encoded_image = encoded_image_frame['image']
        image_shape = encoded_image_frame['shape']

        depthFrame = base64.decodestring(encoded_image)
        depthFrame = np.frombuffer(depthFrame,dtype='uint8')
        return depthFrame.reshape(image_shape)
