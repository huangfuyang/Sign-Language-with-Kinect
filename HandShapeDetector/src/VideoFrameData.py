import cv2

class VideoFrameData:

    def __init__(self):
        self.debug = False

    def load(self, srcVideoPath):
        self.cap = cv2.VideoCapture(srcVideoPath)
        return self.cap.isOpened()

    def setDebug(self, debug):
        self.debug = debug

    def readFrame(self):
        if(hasattr(self, 'cap') and self.cap.isOpened()):
            ret, frame = self.cap.read()
            if not ret:
                return False,None

            frame = cv2.cvtColor(frame,cv2.COLOR_BGR2RGB)
            return True,frame
        return False,None

    def close(self):
        self.cap.release()
