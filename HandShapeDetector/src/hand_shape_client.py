import logging
from echo_client import EchoClient
from FrameConverter import FrameConverter

class HandShapeClient(object):

    def __init__(self, host, port):
        self.client = EchoClient(host, port, self)
        self.converter = FrameConverter()

    def send_data(self, encoded_data):
        self.client.send_data(encoded_data+'\n')

    def callback(self, server_response):
        logging.debug('Result from server: %s' % server_response)
