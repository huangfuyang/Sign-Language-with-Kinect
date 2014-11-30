import random
import time
from echo_server import EchoServer
from FrameConverter import FrameConverter

class HandShapeServer:

    def __init__(self, port):
        self.server = EchoServer(port, self.process_data)
        self.converter = FrameConverter()

    def process_data(self, received_data):
        sleep_time = random.randint(1, 10)
        time.sleep(sleep_time)
        message_to_send = "Received=%s, Sleep=%d, Message='%s'\n" % (time.strftime("%H:%M:%S"), sleep_time, received_data)
        return message_to_send
