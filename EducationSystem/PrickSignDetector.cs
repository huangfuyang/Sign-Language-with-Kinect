using System;
using System.Windows;
using Microsoft.Kinect;

namespace EducationSystem
{
    public class PrickSignDetector
    {
        private BodyPartDetector bodyPartDetector = new BodyPartDetector();
        private WaitState currentWaitingState = WaitState.START;
        private ShowFeatureMatchedPage showFeatureMatchedPage;

        public PrickSignDetector(ShowFeatureMatchedPage showFeatureMatchedPage)
        {
            this.showFeatureMatchedPage = showFeatureMatchedPage;
            this.showFeatureMatchedPage.NumOfFeature = 2;
        }

        private void setStateString(string state, int numOfFeatureCompleted)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                showFeatureMatchedPage.CurrectWaitingState = state;
                showFeatureMatchedPage.NumOfFeatureCompleted = numOfFeatureCompleted;
            });
        }

        public void Update(Skeleton skeleton)
        {
            Tuple<BodyPart, BodyPart> bodyPartForHands = bodyPartDetector.decide(skeleton);

            switch (currentWaitingState)
            {
                case WaitState.START:
                    {
                        setStateString("Waiting Release Hands", 0);
                        if (IsReleaseHands(bodyPartForHands))
                        {
                            currentWaitingState = WaitState.INITIAL_HANDSHAPE_POSITION;
                        }
                        break;
                    }
                case WaitState.INITIAL_HANDSHAPE_POSITION:
                    {
                        setStateString("Waiting Initial handshape at correct position", 0);
                        if (IsInitialHandShapeAndPositionMatched(skeleton, bodyPartForHands))
                        {
                            currentWaitingState = WaitState.TOUCH;
                        }
                        break;
                    }
                case WaitState.TOUCH:
                    {
                        setStateString("Waiting Hands Touching", 1);
                        if (IsTouchAtCertainPositionWIthCorrectHandShape(skeleton, bodyPartForHands))
                        {
                            currentWaitingState = WaitState.DONE;
                        }
                        break;
                    }
                case WaitState.DONE:
                    {
                        setStateString("All DONE!", 2);
                        if (IsReleaseHands(bodyPartForHands))
                        {
                            currentWaitingState = WaitState.INITIAL_HANDSHAPE_POSITION;
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
            return handDiff < 0.1;
        }

        public bool IsReleaseHands(Tuple<BodyPart, BodyPart> bodyPartForHands)
        {
            return bodyPartForHands.Item1 == BodyPart.NONE_RIGHT && bodyPartForHands.Item2 == BodyPart.NONE_LEFT;
        }

        private enum WaitState { START, INITIAL_HANDSHAPE_POSITION, TOUCH, DONE }
    }
}
