import asyncore
import asynchat
import logging
import socket

logging.basicConfig(level=logging.DEBUG,format='%(name)s: %(message)s')

class EchoClient(asynchat.async_chat):

    def __init__(self, host, port, handler):
        self.received_data = []
        self.handler = handler
        self.logger = logging.getLogger('EchoClient')
        asynchat.async_chat.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.logger.debug('connecting to %s', (host, port))
        self.connect((host, port))
        self.set_terminator('\n')

    def handle_connect(self):
        self.logger.debug('handle_connect()')

    def send_data(self, message):
        self.logger.debug('send_data()')
        self.push_with_producer(EchoProducer(message))
        asyncore.loop()

    def collect_incoming_data(self, data):
        self.logger.debug('collect_incoming_data() -> (%d bytes)', len(data))
        self.received_data.append(data)

    def found_terminator(self):
        self.logger.debug('found_terminator()')
        received_message = ''.join(self.received_data)
        self.handler.callback(received_message)
        self.received_data = []

class EchoProducer(asynchat.simple_producer):

    logger = logging.getLogger('EchoProducer')

    def more(self):
        response = asynchat.simple_producer.more(self)
        #self.logger.debug('more() -> (%s bytes)\n"""%s"""', len(response), response)
        return response
