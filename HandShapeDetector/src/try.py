import cv2
import os
modeldic={}
pred_labels=1.0
modelindex=open("/home/lzz/ModelIndex.txt","r")
for line in open("/home/lzz/ModelIndex.txt"):
    line = modelindex.readline()
    sep=line.split(" ")
    modeldic[sep[0]]=sep[1]
wordname=modeldic[str(int(pred_labels))]
dirs=os.listdir("/media/lzz/Data1/Aaron/1-250/")
for dir in dirs:
    if(dir.find(wordname)):
        video=dir
        break

filename = "/media/lzz/Data1/Aaron/1-250/"+video+"/"+video+"_c.avi"
print filename
win_name=filename
capture = cv2.cv.CaptureFromFile(filename)


cv2.cv.NamedWindow(win_name, cv2.cv.CV_WINDOW_AUTOSIZE)


while 1:


    image = cv2.cv.QueryFrame(capture)


    cv2.cv.ShowImage(win_name, image)

    c = cv2.cv.WaitKey(33)
    print image
    if(image==None):
        break
cv2.cv.DestroyWindow(win_name)