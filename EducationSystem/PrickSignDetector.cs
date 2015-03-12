using System;
using Microsoft.Kinect;

namespace EducationSystem
{
    public class PrickSignDetector
    {
        BodyPartDetector bodyPartDetector = new BodyPartDetector();
        WaitState currentWaitingState = WaitState.INITIAL_HANDSHAPE_POSITION;

        public void Update(Skeleton skeleton)
        {
            Tuple<BodyPart, BodyPart> bodyPartForHands = bodyPartDetector.decide(skeleton);

            switch (currentWaitingState)
            {
                case WaitState.INITIAL_HANDSHAPE_POSITION:
                    {
                        if (IsInitialHandShapeAndPositionMatched(skeleton, bodyPartForHands))
                        {
                            currentWaitingState = WaitState.TOUCH;
                        }
                        break;
                    }
                case WaitState.TOUCH:
                    {
                        if (IsTouchAtCertainPositionWIthCorrectHandShape(skeleton, bodyPartForHands))
                        {
                            System.Console.WriteLine("Done!");
                        }
                        break;
                    }
            }
        }

        public bool IsInitialHandShapeAndPositionMatched(Skeleton skeleton, Tuple<BodyPart, BodyPart> bodyPartForHands)
        {

            return (bodyPartForHands.Item1 == BodyPart.TORSO_TOP_RIGHT || bodyPartForHands.Item1 == BodyPart.BODY_RIGHT)
                && (bodyPartForHands.Item2 == BodyPart.TORSO_TOP_LEFT || bodyPartForHands.Item2 == BodyPart.BODY_LEFT);
        }

        public bool IsTouchAtCertainPositionWIthCorrectHandShape(Skeleton skeleton, Tuple<BodyPart, BodyPart> bodyPartForHands)
        {
            float handDiff = Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.HandRight].Position.X);
            //System.Console.WriteLine("HAHAHA: {0}", handDiff);
            return handDiff < 0.1;
        }

        private enum WaitState { INITIAL_HANDSHAPE_POSITION, TOUCH }
    }
}
