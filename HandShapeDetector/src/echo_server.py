import asyncore
import asynchat
import logging
import socket
import thread

logging.basicConfig(level=logging.DEBUG,format='%(name)s: %(message)s')

class EchoServer(asyncore.dispatcher):
    def __init__(self, port, received_data_callback):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.set_reuse_addr()
        self.bind(('', port))
        self.address = self.socket.getsockname()
        self.listen(5)

        self.received_data_callback = received_data_callback
        asyncore.loop()

    def handle_accept(self):
        client_info = self.accept()
        if client_info is not None:
            sock, addr = client_info
            logging.debug('connecting to %s, tid %s', repr(addr), thread.get_ident())
            EchoHandler(client_info[0], self.received_data_callback)

    def handle_close(self):
        self.close()


class EchoHandler(asynchat.async_chat):

    def __init__(self, sock, received_data_callback):
        self.received_data = []
        self.received_data_callback = received_data_callback
        self.logger = logging.getLogger('EchoHandler')
        asynchat.async_chat.__init__(self, sock)
        self.set_terminator('#TERMINATOR#')

    def collect_incoming_data(self, data):
        #self.logger.debug('collect_incoming_data() -> (%d bytes)\n"""%s"""', len(data), data)
        self.received_data.append(data)

    def found_terminator(self):
        self.logger.debug('found_terminator()')
        response = self.received_data_callback(''.join(self.received_data))
        self.push(response)
        self.received_data = []
