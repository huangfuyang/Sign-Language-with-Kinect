using System;
using System.Windows;
using System.Windows.Media.Media3D;
using EducationSystem.Detectors;
using Microsoft.Kinect;

namespace EducationSystem
{
    public class PrickSignDetector
    {
        private BodyPartDetector bodyPartDetector = new BodyPartDetector();
        private TouchDetector touchDetector = new TouchDetector();
        private StraightMovementDetector straightMovementDetector = new StraightMovementDetector();
        private ReleaseHandDetector releaseHandDetector = new ReleaseHandDetector();
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
            Tuple<Point3D, Point3D> directions = straightMovementDetector.decide(new Tuple<Skeleton, int>(skeleton, 5));

            switch (currentWaitingState)
            {
                case WaitState.START:
                    {
                        setStateString("Waiting Release Hands", 0);
                        if (releaseHandDetector.decide(bodyPartForHands))
                        {
                            currentWaitingState = WaitState.INITIAL_HANDSHAPE_POSITION;
                        }
                        break;
                    }
                case WaitState.INITIAL_HANDSHAPE_POSITION:
                    {
                        setStateString("Waiting Initial handshape at correct position", 0);
                        if (IsInitialPosition(bodyPartForHands))
                        {
                            currentWaitingState = WaitState.TOUCH;
                        }
                        break;
                    }
                case WaitState.TOUCH:
                    {
                        setStateString("Waiting Hands Touching", 1);
                        if (touchDetector.decide(skeleton))
                        {
                            currentWaitingState = WaitState.DONE;
                        }
                        break;
                    }
                case WaitState.DONE:
                    {
                        setStateString("All DONE!", 2);
                        if (releaseHandDetector.decide(bodyPartForHands))
                        {
                            currentWaitingState = WaitState.INITIAL_HANDSHAPE_POSITION;
                        }
                        break;
                    }
            }
        }

        public bool IsInitialPosition(Tuple<BodyPart, BodyPart> bodyPartForHands)
        {
            return (bodyPartForHands.Item1 == BodyPart.TORSO_TOP_RIGHT || bodyPartForHands.Item1 == BodyPart.BODY_RIGHT)
                && (bodyPartForHands.Item2 == BodyPart.TORSO_TOP_LEFT || bodyPartForHands.Item2 == BodyPart.BODY_LEFT);
        }

        private enum WaitState { START, INITIAL_HANDSHAPE_POSITION, TOUCH, DONE }
    }
}
