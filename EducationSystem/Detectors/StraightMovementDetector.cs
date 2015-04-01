using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace EducationSystem.Detectors
{
    class StraightMovementDetector : AbstractDetector<Tuple<Skeleton, int>, Tuple<Point3D, Point3D>>
    {
        private List<SkeletonPoint> leftHandPositions;
        private List<SkeletonPoint> rightHandPositions;

        public StraightMovementDetector()
        {
            leftHandPositions = new List<SkeletonPoint>();
            rightHandPositions = new List<SkeletonPoint>();
        }

        public Point3D decide(List<SkeletonPoint> handPositions, SkeletonPoint latestPoint, int minimumFrame)
        {
            Point3D direction = new Point3D();
            float[] deltaX = null;
            handPositions.Add(latestPoint);

            if (rightHandPositions.Count > 1)
            {
                deltaX = Enumerable.Range(1, handPositions.Count - 1).Select<int, float>(i => handPositions[i].X - handPositions[i - 1].X).ToArray();
            }

            if (deltaX != null && deltaX.Length == 100)
            {
                Console.WriteLine("{0}", string.Join(", ", deltaX));
            }

            return direction;
        }

        public override Tuple<Point3D, Point3D> decide(Tuple<Skeleton, int> input)
        {
            Skeleton skeleton = input.Item1;
            int minimumFrame = input.Item2;

            return new Tuple<Point3D, Point3D>(
                decide(rightHandPositions, skeleton.Joints[JointType.HandRight].Position, minimumFrame),
                decide(leftHandPositions, skeleton.Joints[JointType.HandLeft].Position, minimumFrame));
        }
    }
}
