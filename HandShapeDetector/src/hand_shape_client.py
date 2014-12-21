import time
import logging
import bz2
from echo_client import EchoClient

class HandShapeClient(object):

    def __init__(self, host, port, terminator, converter):
        self.client = EchoClient(host, port, terminator, self)
        self.converter = converter

    def send_data(self, encoded_data):
        self.send_time = time.strftime("%H:%M:%S")

        compressed_data = bz2.compress(encoded_data)
        self.client.send_data(compressed_data+self.client.terminator)

    def callback(self, server_response):
        logging.debug('Send: %s, %s' % (self.send_time, server_response))
