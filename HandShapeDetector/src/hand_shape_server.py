import random
import time
from echo_server import EchoServer
import collections
import pylab as plt

class HandShapeServer(object):

    def __init__(self, port, converter, data_handler=None):
        self.converter = converter
        self.data_handler = data_handler
        self.server = EchoServer(port, self.received_data)

    def received_data(self, received_data):
        decoded_data = self.converter.decode(received_data)
        return self.process_data(decoded_data)

    def process_data(self, decoded_data):
        if hasattr(self, 'data_handler') and self.data_handler is not None:
            self.data_handler.handle_data(decoded_data)

        # For debug
        sleep_time = random.randint(1, 10)
        plt.subplot(1,2,1), plt.imshow(decoded_data['depth_image'])
        plt.subplot(1,2,2), plt.imshow(decoded_data['color_image'])
        plt.show()
        arrive_time = time.strftime("%H:%M:%S")
        time.sleep(sleep_time)
        message_to_send = "Arrive=%s, Sleep=%d, Response=%s, Message='%s'\n" % (arrive_time, sleep_time, time.strftime("%H:%M:%S"), self.flatten(decoded_data).keys())
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
