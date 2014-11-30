import logging
from hand_shape_client import EchoClient

logging.basicConfig(level=logging.DEBUG,format='%(name)s: %(message)s')

port = 51243
client = EchoClient('localhost', port)
client.send_data('hahahah\nabcd\n')
