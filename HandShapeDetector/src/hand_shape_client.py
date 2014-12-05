import time
import logging
from echo_client import EchoClient

class HandShapeClient(object):

    def __init__(self, host, port, converter):
        self.client = EchoClient(host, port, self)
        self.converter = converter

    def send_data(self, encoded_data):
        self.send_time = time.strftime("%H:%M:%S")
        self.client.send_data(encoded_data+'\n')

    def callback(self, server_response):
        logging.debug('Send: %s, %s' % (self.send_time, server_response))
