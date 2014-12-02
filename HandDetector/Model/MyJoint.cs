using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector.Model
{
    
    public class MyJoint
    {
        public MyJointType Type { get; set; }
        public SkeletonPoint Position { get; set; }

        public MyJoint()
        {
            
        }
    }
}
