import caffe
class caffeDL():
    def __init__(self):
        caffe_root ="/home/lzz/caffe-master/"    
        self.net = caffe.Classifier(caffe_root + 'new/proto/lenet_test.prototxt', caffe_root + 'lenet_iter_3500.caffemodel')
        self.net.set_phase_test()
        self.net.set_mode_cpu()
    # input preprocessing: 'data' is the name of the input blob == net.inputs[0]
    #net.set_mean('data', np.load(caffe_root + 'mean.binaryproto'))  # ImageNet mean
        self.net.set_raw_scale('data', 1)  # the reference model operates on images in [0,255] range instead of [0,1]
        self.net.set_channel_swap('data', (2,1,0))
