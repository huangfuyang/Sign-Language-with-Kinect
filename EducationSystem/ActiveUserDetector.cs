
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
namespace EducationSystem
{
    class ActiveUserDetector : AbstractKinectFramesHandler
    {

        private int _activeUserCount;
        public int ActiveUserCount
        {
            get { return _activeUserCount; }
            set { SetProperty(ref _activeUserCount, value, true); }
        }

        public override void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData)
        {

        }

        public override void DepthFrameCallback(long timestamp, int frameNumber, DepthImagePixel[] depthPixels)
        {

        }

        public override void ColorFrameCallback(long timestamp, int frameNumber, byte[] colorPixels)
        {

        }

        public override void HandPointersCallback(long timestamp, HandPointer[] handPointers)
        {
            HashSet<int> activeUserIds = new HashSet<int>();

            foreach (HandPointer handPointer in handPointers)
            {
                if (handPointer.IsActive)
                {
                    activeUserIds.Add(handPointer.TrackingId);
                }
            }

            ActiveUserCount = activeUserIds.Count;
        }
    }
}
