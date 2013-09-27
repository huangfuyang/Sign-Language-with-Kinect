using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LearningSystem.DataStorage
{
    public struct SegmentationData
    {
        public int startFrame,endFrame;
        /// <summary>
        /// 1 = very relieble 0 = very un
        /// </summary>
        public float reliebility;
    }
}
