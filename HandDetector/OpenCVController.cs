// author：      Administrator
// created time：2014/1/15 14:34:49
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.UI;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class OpenCVController
    {
        private static OpenCVController singletonInstance;
        private OpenCVController()
        {

        }

        public static OpenCVController GetSingletonInstance()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new OpenCVController();
            }
            return singletonInstance;
        }

        public void Start()
        {
            
        }
    }
}