import ConfigParser
from os import getcwd,makedirs
from os.path import join,exists

CONFIG_DIRECTORY = join(getcwd(), 'config')
SOCKET_CONFIG_FILE_NAME = 'socket.cfg'

if not exists(CONFIG_DIRECTORY):
    makedirs(CONFIG_DIRECTORY)

config = ConfigParser.RawConfigParser()
config.add_section('Server')
config.set('Server', 'address', 'localhost')
config.set('Server', 'port', '57681')

with open(join(CONFIG_DIRECTORY, SOCKET_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)
