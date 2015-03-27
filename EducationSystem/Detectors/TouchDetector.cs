using System;
using Microsoft.Kinect;

namespace EducationSystem.Detectors
{
    class TouchDetector : AbstractDetector<Skeleton, bool>
    {
        public override bool decide(Skeleton input)
        {
            float handDiff = Math.Abs(input.Joints[JointType.HandLeft].Position.X - input.Joints[JointType.HandRight].Position.X);
            return handDiff < 0.1;
        }
    }
}
