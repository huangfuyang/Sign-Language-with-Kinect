
using System;
using Microsoft.Kinect;
namespace EducationSystem.Detectors
{
    public enum BodyPart { NONE, NONE_LEFT, NONE_RIGHT, HEAD_LEFT, HEAD_RIGHT, TORSO_TOP_LEFT, TORSO_TOP_RIGHT, TORSO_BOTTOM_LEFT, TORSO_BOTTOM_RIGHT, BODY_LEFT, BODY_RIGHT }

    class BodyPartDetector : AbstractDetector<Skeleton, Tuple<BodyPart, BodyPart>>
    {
        private bool isRightHandPrimary = true;

        private BodyPart[,] bodyPartMapping = new BodyPart[3, 4] { 
            {BodyPart.HEAD_LEFT, BodyPart.HEAD_LEFT, BodyPart.HEAD_RIGHT, BodyPart.HEAD_RIGHT},
            {BodyPart.BODY_LEFT, BodyPart.TORSO_TOP_LEFT, BodyPart.TORSO_TOP_RIGHT, BodyPart.BODY_RIGHT},
            {BodyPart.NONE_LEFT, BodyPart.TORSO_BOTTOM_LEFT, BodyPart.TORSO_BOTTOM_RIGHT, BodyPart.NONE_RIGHT}
        };

        private BodyPart decide(Skeleton skeleton, SkeletonPoint targetPoint)
        {
            SkeletonPoint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position;
            SkeletonPoint shoulderRight = skeleton.Joints[JointType.ShoulderRight].Position;
            SkeletonPoint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter].Position;
            SkeletonPoint head = skeleton.Joints[JointType.Head].Position;
            SkeletonPoint spine = skeleton.Joints[JointType.Spine].Position;

            int xRegion = 0;
            int yRegion = 0;

            if (targetPoint.X > shoulderRight.X)
            {
                xRegion = 3;
            }
            else if (targetPoint.X > shoulderCenter.X)
            {
                xRegion = 2;
            }
            else if (targetPoint.X > shoulderLeft.X)
            {
                xRegion = 1;
            }

            if (targetPoint.Y < spine.Y)
            {
                yRegion = 2;
            }
            else if (targetPoint.Y < shoulderCenter.Y)
            {
                yRegion = 1;
            }

            return bodyPartMapping[yRegion, xRegion];
        }

        public override Tuple<BodyPart, BodyPart> decide(Skeleton skeleton)
        {
            SkeletonPoint primaryHand = skeleton.Joints[isRightHandPrimary ? JointType.HandRight : JointType.HandLeft].Position;
            SkeletonPoint secondaryHand = skeleton.Joints[!isRightHandPrimary ? JointType.HandRight : JointType.HandLeft].Position;
            return new Tuple<BodyPart, BodyPart>(decide(skeleton, primaryHand), decide(skeleton, secondaryHand));
        }
    }
}
