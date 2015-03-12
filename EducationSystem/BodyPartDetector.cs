
using Microsoft.Kinect;
namespace EducationSystem
{
    public enum BodyPart { NONE, HEAD_LEFT, HEAD_RIGHT, TORSO_TOP_LEFT, TORSO_TOP_RIGHT, TORSO_BOTTOM_LEFT, TORSO_BOTTOM_RIGHT, BODY_LEFT, BODY_RIGHT }

    class BodyPartDetector
    {
        private bool isRightHandPrimary = true;

        private BodyPart[,] bodyPartMapping = new BodyPart[3, 4] { 
            {BodyPart.HEAD_LEFT, BodyPart.HEAD_LEFT, BodyPart.HEAD_RIGHT, BodyPart.HEAD_RIGHT},
            {BodyPart.BODY_LEFT, BodyPart.TORSO_TOP_LEFT, BodyPart.TORSO_TOP_RIGHT, BodyPart.BODY_RIGHT},
            {BodyPart.NONE, BodyPart.TORSO_BOTTOM_LEFT, BodyPart.TORSO_BOTTOM_RIGHT, BodyPart.NONE}
        };

        public BodyPart decide(Skeleton skeleton)
        {
            SkeletonPoint primaryHand = skeleton.Joints[isRightHandPrimary ? JointType.HandRight : JointType.HandLeft].Position;
            SkeletonPoint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position;
            SkeletonPoint shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position;
            SkeletonPoint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position;
            SkeletonPoint head = skeleton.Joints[JointType.Head].Position;
            SkeletonPoint spine = skeleton.Joints[JointType.Spine].Position;

            int xRegion = 0;
            int yRegion = 0;

            if (primaryHand.X > shoulderRight.X)
            {
                xRegion = 3;
            }
            else if (primaryHand.X > shoulderCenter.X)
            {
                xRegion = 2;
            }
            else if (primaryHand.X > shoulderLeft.X)
            {
                xRegion = 1;
            }

            if (primaryHand.Y < spine.Y)
            {
                yRegion = 2;
            }
            else if (primaryHand.Y < shoulderCenter.Y)
            {
                yRegion = 1;
            }

            //System.Console.WriteLine(bodyPartMapping[yRegion, xRegion]);
            return bodyPartMapping[yRegion, xRegion];
        }
    }
}
