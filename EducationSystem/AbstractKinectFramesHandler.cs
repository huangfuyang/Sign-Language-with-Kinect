using System.Threading;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;

namespace EducationSystem
{
    abstract class AbstractKinectFramesHandler : AutoNotifyPropertyChanged
    {
        public abstract void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData);
        public abstract void DepthFrameCallback(long timestamp, int frameNumber, DepthImagePixel[] depthPixels);
        public abstract void ColorFrameCallback(long timestamp, int frameNumber, byte[] colorPixels);
        public abstract void HandPointersCallback(long timestamp, HandPointer[] handPointers);

        private bool isRegisterAllFrameReady;

        public AbstractKinectFramesHandler(bool isRegisterAllFrameReady = true)
        {
            this.isRegisterAllFrameReady = isRegisterAllFrameReady;
        }

        public void RegisterCallbackToSensor(KinectSensor sensor)
        {
            if (isRegisterAllFrameReady)
            {
                sensor.AllFramesReady += sensor_AllFramesReady;
            }
            else
            {
                sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
                sensor.DepthFrameReady += sensor_DepthFrameReady;
                sensor.ColorFrameReady += sensor_ColorFrameReady;
            }

            KinectState.Instance.KinectRegion.HandPointersUpdated += KinectRegion_HandPointersUpdated;
        }

        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            handleSkeletonFrame(e.OpenSkeletonFrame());
        }

        private void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            handleDepthImageFrame(e.OpenDepthImageFrame());
        }

        private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            handleColorImageFrame(e.OpenColorImageFrame());
        }

        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            handleSkeletonFrame(e.OpenSkeletonFrame());
            handleDepthImageFrame(e.OpenDepthImageFrame());
            handleColorImageFrame(e.OpenColorImageFrame());
        }

        private void KinectRegion_HandPointersUpdated(object sender, System.EventArgs e)
        {
            HandPointer[] handPointers = new HandPointer[KinectState.Instance.KinectRegion.HandPointers.Count];
            KinectState.Instance.KinectRegion.HandPointers.CopyTo(handPointers, 0);
            long timestampOfLastUpdate = long.MinValue;

            foreach (HandPointer handPointer in handPointers)
            {
                timestampOfLastUpdate = System.Math.Max(handPointer.TimestampOfLastUpdate, timestampOfLastUpdate);
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(o => HandPointersCallback(timestampOfLastUpdate, handPointers)));
        }

        private void handleSkeletonFrame(SkeletonFrame skeletonFrame)
        {
            using (skeletonFrame)
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => SkeletonFrameCallback(skeletonFrame.Timestamp, skeletonFrame.FrameNumber, skeletonData)));
                }
            }
        }

        private void handleDepthImageFrame(DepthImageFrame depthFrame)
        {
            using (depthFrame)
            {
                if (depthFrame != null)
                {
                    DepthImagePixel[] depthPixels = new DepthImagePixel[depthFrame.PixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => DepthFrameCallback(depthFrame.Timestamp, depthFrame.FrameNumber, depthPixels)));
                }
            }
        }

        private void handleColorImageFrame(ColorImageFrame colorFrame)
        {
            using (colorFrame)
            {
                if (colorFrame != null)
                {
                    byte[] colorPixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(colorPixels);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => ColorFrameCallback(colorFrame.Timestamp, colorFrame.FrameNumber, colorPixels)));
                }
            }
        }
    }
}
