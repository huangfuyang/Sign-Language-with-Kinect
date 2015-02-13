
using System.Collections.Generic;
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
