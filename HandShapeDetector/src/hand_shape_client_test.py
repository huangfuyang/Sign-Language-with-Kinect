from hand_shape_client import HandShapeClient

port = 51243
client = HandShapeClient('localhost', port)
client.send_data('hahahah\nabcd\n')
