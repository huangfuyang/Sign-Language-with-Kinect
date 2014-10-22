using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector.Model
{
    public class MySkeleton
    {
        public MyJoint[] Joints 
        { get; set; }

        public MySkeleton(int count = 20)
        {
            Joints = new MyJoint[20];
            
        }
        public MyJoint this[MyJointType key]
        {
            get
            {
                return Joints[(int)key];
            }
            set
            {
                Joints[(int)key] = value;
            }
        }

        public MyJoint this[int key]
        {
            get
            {
                return Joints[key];
            }
            set
            {
                Joints[key] = value;
            }
        }
    }
}
