import caffe
import sys

class CaffeServerHandler(object):
    def __init__(self, caffe_root, img_path):
        sys.path.insert(0, caffe_root + 'python')
        self.model_file = caffe_root + 'models/bvlc_reference_caffenet/deploy.prototxt'
        self.pretrain_file = caffe_root + 'models/bvlc_reference_caffenet/bvlc_reference_caffenet.caffemodel'

    def caffe_init(self, caffe_root):
        net = caffe.Classifier(self.model_file, self.pretrain_file)
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
