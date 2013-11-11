// author：      fyhuang
// created time：2013/11/8 18:47:56
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CURELab.SignLanguage.Calculator
{
    /// <summary>
    /// add summary here
    /// </summary>
    public interface IDataProcessor
    {
        void GaussianFilter(ref double[] data);

        void MeanFilter(ref double[] data);

         double[] CalVelocity(double[] data);

         double[] CalAcceleration(double[] data);

    }
}