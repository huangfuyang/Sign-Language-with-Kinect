from hand_shape_server import HandShapeServer
from FrameConverter import FrameConverter

port = 51243
converter = FrameConverter()
server = HandShapeServer(port, converter)
