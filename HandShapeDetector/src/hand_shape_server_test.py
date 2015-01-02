from hand_shape_server import HandShapeServer
from FrameConverter import FrameConverter
from caffe_server_handler import CaffeServerHandler

port = 51243
caffe_root = ''
img_path = ''

converter = FrameConverter()
data_handler = CaffeServerHandler()
server = HandShapeServer(port, converter, '\r\n\r\n', data_handler)
