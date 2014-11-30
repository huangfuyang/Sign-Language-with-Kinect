import random
import time
from echo_server import EchoServer
from FrameConverter import FrameConverter
import collections
import pylab as plt

class HandShapeServer(object):

    def __init__(self, port):
        self.converter = FrameConverter()
        self.server = EchoServer(port, self.received_data)

    def received_data(self, received_data):
        decoded_data = self.converter.decode(received_data)
        return self.process_data(decoded_data)

    def process_data(self, decoded_data):
        # For debug
        sleep_time = random.randint(1, 10)
        plt.imshow(decoded_data['depth_image'])
        plt.show()
        time.sleep(sleep_time)
        message_to_send = "Received=%s, Sleep=%d, Message='%s'\n" % (time.strftime("%H:%M:%S"), sleep_time, self.flatten(decoded_data).keys())
        return message_to_send

    def flatten(self, d, parent_key='', sep='.'):
        items = []
        for k, v in d.items():
            new_key = parent_key + sep + k if parent_key else k
            if isinstance(v, collections.MutableMapping):
                items.extend(self.flatten(v, new_key, sep=sep).items())
            else:
                items.append((new_key, v))
        return dict(items)
