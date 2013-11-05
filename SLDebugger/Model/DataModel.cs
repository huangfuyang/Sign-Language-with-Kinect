// author：      fyhuang
// created time：2013/10/16 18:14:55
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CURELab.SignLanguage.Debugger.Model
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class DataModel
    {
        public int timeStamp;
        public double v_right;
        public double v_left;
        public double a_right;
        public double a_left;
        public double angle_right;
        public double angle_left;
        public Vec3 position_right;
        public Vec3 position_left;
        public bool isSegByAcc;
        public bool isSegByAngle;
        public Point position_2D_right;
        public Point position_2D_left;
    }

    public struct Vec3
    {
        public double x, y, z;
    }
}