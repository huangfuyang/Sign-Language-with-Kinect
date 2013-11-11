// author：      fyhuang
// created time：2013/11/11 16:33:39
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
    public class CSDataProcessor:IDataProcessor     
    {
        public CSDataProcessor()
        {

        }

        public void GaussianFilter(ref double[] data)
        {

        }

        public void MeanFilter(ref double[] data)
        {

        }

        public double[] CalVelocity(double[] data)
        {
            return data;
        }

        public double[] CalAcceleration(double[] data)
        {
            return data;
        }
    }
}