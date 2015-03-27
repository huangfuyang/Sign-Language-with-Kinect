using System;

namespace EducationSystem.Detectors
{
    class ReleaseHandDetector : AbstractDetector<Tuple<BodyPart, BodyPart>, bool>
    {
        public override bool decide(Tuple<BodyPart, BodyPart> bodyPartForHands)
        {
            return bodyPartForHands.Item1 == BodyPart.NONE_RIGHT && bodyPartForHands.Item2 == BodyPart.NONE_LEFT;
        }
    }
}
