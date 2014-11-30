import logging
from hand_shape_server import EchoServer
import random
import time

def process_data_test(received_data):
    logging.debug('process_data_test: "%s"', received_data)

    sleep_time = random.randint(1, 10)
    time.sleep(sleep_time)

    message_to_send = "Received=%s, Sleep=%d, Message='%s'\n" % (time.strftime("%H:%M:%S"), sleep_time, received_data)
    return message_to_send

logging.basicConfig(level=logging.DEBUG,format='%(name)s: %(message)s')
server = EchoServer(51243, process_data_test)
