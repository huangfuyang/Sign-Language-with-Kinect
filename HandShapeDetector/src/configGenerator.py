import ConfigParser
from os import makedirs,sys
from os.path import join,exists,dirname,realpath

CONFIG_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..', 'config')
SOCKET_CONFIG_FILE_NAME = 'socket.cfg'
FILE_FORMAT_CONFIG_FILE_NAME = 'file_format.cfg'
DEBUG_CONFIG_FILE_NAME = 'debug.cfg'

if not exists(CONFIG_DIRECTORY):
    makedirs(CONFIG_DIRECTORY)

# For Socket
config = ConfigParser.RawConfigParser()
config.add_section('Server')
config.set('Server', 'address', 'localhost')
config.set('Server', 'port', '57681')

with open(join(CONFIG_DIRECTORY, SOCKET_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)


# For File Name Format
config = ConfigParser.RawConfigParser()
config.add_section('Directory')
config.set('Directory', 'Label', join('data', 'label'))
config.set('Directory', 'Video', join('data', 'video'))
config.set('Directory', 'Result', 'result')

config.add_section('File')
config.set('File', 'Video Extension', '.avi')
config.set('File', 'Depth Video Suffix', '_d')
config.set('File', 'Color Video Suffix', '_c')

with open(join(CONFIG_DIRECTORY, FILE_FORMAT_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)


# For Debugging
config = ConfigParser.RawConfigParser()
config.add_section('Debug')
config.set('Debug', 'Print Debug Message', True)
config.set('Debug', 'Visualize Result', True)
config.set('Debug', 'Save Result to Video', True)

with open(join(CONFIG_DIRECTORY, DEBUG_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)
