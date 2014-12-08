import ConfigParser
from os import makedirs,sys
from os.path import join,exists,dirname,realpath

CONFIG_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..', 'config')
SOCKET_CONFIG_FILE_NAME = 'socket.cfg'
FILE_FORMAT_CONFIG_FILE_NAME = 'file_format.cfg'
DEBUG_CONFIG_FILE_NAME = 'debug.cfg'
SKELETON_CONFIG_FILE_NAME = 'skeleton.cfg'
CAFFE_CONFIG_FILE_NAME = 'caffe.cfg'

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
config.set('Directory', 'Skeleton', join('data', 'skeleton'))
config.set('Directory', 'Video', join('data', 'video'))
config.set('Directory', 'Result', 'result')

config.add_section('File')
config.set('File', 'Video Extension', '.avi')
config.set('File', 'Skeleton Extension', '.csv')
config.set('File', 'Skeleton Suffix', '_skeleton')
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


# For Skeleton joint mapping
config = ConfigParser.RawConfigParser()
config.add_section('Enumeration')
config.set('Enumeration', 'Joint Types', ','.join(['head', 'shoulderLeft', 'shoulderCenter', 'shoulderRight', 'elbowL', 'elbowR', 'wristL', 'wristR', 'handL', 'handR', 'spine', 'hipL', 'hipCenter', 'hipR']))
config.set('Enumeration', 'Joint Data Keys', ','.join(['3d_x', '3d_y', '3d_z', 'color_x', 'color_y', 'depth_x', 'depth_y']))

with open(join(CONFIG_DIRECTORY, SKELETON_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)


# For Caffe
config = ConfigParser.RawConfigParser()
config.add_section('Directory')
config.set('Directory', 'Caffe Root', '')
config.set('Directory', 'Caffe Python', 'python')
config.set('Directory', 'Train', join('models', 'bvlc_reference_caffenet'))

config.add_section('File')
config.set('File', 'Model Definition', 'deploy.prototxt')
config.set('File', 'Pre-trained model', 'bvlc_reference_caffenet.caffemodel')

with open(join(CONFIG_DIRECTORY, CAFFE_CONFIG_FILE_NAME), 'wb') as configfile:
    config.write(configfile)
