import caffe
import sys
import ConfigParser
from os.path import join,dirname,realpath

class CaffeServerHandler(object):
    def __init__(self):
        ROOT_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..')

        config = ConfigParser.RawConfigParser()
        config.read(join(ROOT_DIRECTORY, 'config', 'caffe.cfg'))

        caffe_root = config.get('Directory', 'Caffe Root')
        caffe_python = config.get('Directory', 'Caffe Python')
        caffe_train_directory = config.get('Directory', 'Train')
        caffe_model_definiation_file = join(caffe_train_directory, config.get('File', 'Model Definition'))
        caffe_pre_trained_model_file = join(caffe_train_directory, config.get('File', 'Pre-trained model'))

        sys.path.insert(0, join(caffe_root, caffe_python))
        self.caffe_init(caffe_model_definiation_file, caffe_pre_trained_model_file)

    def caffe_init(self, caffe_model_definiation_file, caffe_pre_trained_model_file):
        net = caffe.Classifier(caffe_model_definiation_file, caffe_pre_trained_model_file)
        net.set_phase_test()
        net.set_mode_cpu()
        net.set_raw_scale('data', 255)
        net.set_channel_swap('data', (2,1,0))

        self.net = net

    def handle_data(self, decoded_data):
        self.net.predict([decoded_data['depth']])
        feat = self.net.blobs['fc7'].data[4].flatten().tolist()
        tmpS = ''

        for i,f in enumerate(feat):
            tmpS += str(f) + ' '

        f = file('handshape.txt', 'a')
        f.write(tmpS)
        f.close()
