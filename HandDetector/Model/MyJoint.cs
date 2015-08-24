using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector.Model
{
    
    public class MyJoint
    {
        public SkeletonPoint Pos3D { get; set; }
        public Point PosDepth { get; set; }
        public Point PosColor { get; set; }
        public MyJoint()
        {
            
        }
    }
}
