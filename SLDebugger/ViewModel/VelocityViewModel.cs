// author：      fyhuang
// created time：2013/10/7 14:35:42
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DynamicDataDisplay.Common;


namespace CURELab.SignLanguage.Debugger.ViewModel
{
    /// <summary>
    /// VelocityViewModel is a data model to be shown in data chart
    /// </summary>
    public class VelocityPointCollection : RingArray<VelocityPoint>
    {
        private const int TOTAL_POINTS = 300;

        public VelocityPointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }

    public class VelocityPoint
    {
        public int TimeStamp { get; set; }

        public double Velocity { get; set; }

        public VelocityPoint(double velocity, int timeStamp)
        {
            this.TimeStamp = timeStamp;
            this.Velocity = velocity;
        }
    }
}