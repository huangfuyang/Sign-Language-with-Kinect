__author__ = 'fyhuang'
import select, json,socket,time,webbrowser
import cv2
import os
from cv2 import *
from PIL import Image
import io
import numpy as np
import sys
# import caffe
from numpy import array
RefreshTime = 5
def sendData(sock, msg):
    try :
        sock.send(msg)
    except :
        # broken socket connection may be,  client pressed ctrl+c for example
        sock.close()
        CONNECTION_LIST.remove(sock)
def recv_timeout(the_socket,timeout=0.02):
    #make socket non blocking
    the_socket.setblocking(0)

    #total data partwise in an array
    total_data=[]
    data=''
    begin=time.time()

    while 1:
        #if you got some data, then break after timeout
        if total_data and time.time()-begin > timeout:
            break

        #if you got no data at all, wait a little longer, twice the timeout
        elif time.time()-begin > timeout*2:
            break

        #recv something
        try:
            data = the_socket.recv(1024)
            if data:
                total_data.append(data)
                #change the beginning time for measurement
                begin=time.time()
            else:
                #sleep for sometime to indicate a gap
                time.sleep(0.01)
        except:
            pass

    #join all parts to make final string
    return ''.join(total_data)

if __name__ == "__main__":
    # List to keep track of socket descriptors
    CONNECTION_LIST = []
    PORT = 8080
    RECV_BUFFER = 4096
    server_socket = socket.socket(
        socket.AF_INET, socket.SOCK_STREAM)
    #bind the socket to a public host,
    # and a well-known port
    server_socket.bind(('137.189.89.29', PORT))
    #become a server socket
    server_socket.listen(5)
    CONNECTION_LIST.append(server_socket)
    #os.chdir('D:\\caffe\\caffe-windows-extract-feature\\caffe-windows\\bin')
        #beginning time
    
    caffe_root ="/home/lzz/caffe-master/"  # this file is expected to be i {caffe_root}/examples

    sys.path.insert(0, caffe_root + 'python') 

    net = caffe.Classifier(caffe_root + 'new/proto/lenet_test.prototxt',
	                       caffe_root + 'lenet_iter_3500.caffemodel')
    net.set_phase_test()
    net.set_mode_cpu()
# input preprocessing: 'data' is the name of the input blob == net.inputs[0]
#net.set_mean('data', np.load(caffe_root + 'mean.binaryproto'))  # ImageNet mean
    net.set_raw_scale('data', 1)  # the reference model operates on images in [0,255] range instead of [0,1]
    net.set_channel_swap('data', (2,1,0))
    # Add server socket to the list of readable connections
    print "setup ok. waiting..."
    t=0
    while 1:
        # Get the list sockets which are ready to be read through select
        read_sockets,write_sockets,error_sockets = select.select(CONNECTION_LIST,[],[])

        for sock in read_sockets:
            #New connection
            if sock == server_socket:
                # Handle the case in which there is a new connection recieved through server_socket
                print "incoming socket"
                sockfd, addr = server_socket.accept()
                sockfd.setblocking(0)
                CONNECTION_LIST.append(sockfd)
                print "Client (%s, %s) connected" % addr
                # broadcast_data(sockfd, "[%s:%s] entered room" % addr)

            #Some incoming message from a client
            else:
                # Data recieved from client, process it
                #try:
                    #In Win9dows, sometimes when a TCP program closes abruptly,
                    # a "Connection reset by peer" exception will be thrown
                #print "data received"
                #filename = open('D:/caffe/caffe-windows-extract-feature/caffe-windows/bin/tst.jpg', 'wb')
                r = recv_timeout(sock)
                #print r
                if r:
                    t+=1
                    #filename.write(r)
#                     sendData(sock,json.dumps({"result":"received"}))
                    #filename.close(
                    print "="*20
                    print len(r),"bytes received"
                    s = r.split("SPLIT")
                    print len(s)," images"
                    bytes0=bytearray(s[0])
                    image = Image.open(io.BytesIO(bytes0))
                    #img=imread(image)
                    img=array(image)
                    
                    img0=img
                    
                    #print t
                    #print img
                    #img=imread('D:/caffe/caffe-windows-extract-feature/caffe-windows/bin/tst.jpg')
                    sp=img.shape
                    img2=copyMakeBorder(img, 0,0, int(abs(sp[0]-sp[1])/2),int(abs(sp[0]-sp[1])/2), BORDER_CONSTANT, value=(0, 0, 0, 0))
                    img3=cv2.resize(img2,(128,128))
                    img3=img3/255.0
                    net.predict([img3])
                    #cv2.imwrite("/home/lzz/x.jpg",img3)
		    #print time.time()-now
		    feat = net.blobs['prob'].data[0].flatten().tolist()
		    print feat
                    ind=0
                    maximum=feat[0]
 
                    for i in range(len(feat)):
                        tmp=feat[i]
                        if(tmp>maximum):
                            maximum=tmp
                            ind=i
                    
                    imwrite("/home/lzz/images/"+str(ind)+"_"+str(t)+".jpg",img0)
		    #ind=feat.index(max(feat))
		    #print time.time()-now
                    print "result {}".format(ind)
                    #sendData(sock,ind)

                    '''except Exception,ex:
                        print ex
                        print "Client (%s, %s) is offline " % addr
                        sock.close()
                        if sock in CONNECTION_LIST:
                            CONNECTION_LIST.remove(sock)
                        continue'''
                else:
                    print "socket close (%s,%s)"% addr
                    sock.close()
                    CONNECTION_LIST.remove(sock)

    server_socket.close()
