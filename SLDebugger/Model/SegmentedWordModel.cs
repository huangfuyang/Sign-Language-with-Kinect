// author：      fyhuang
// created time：2013/10/17 17:23:52
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CURELab.SignLanguage.Debugger.Model
{
    /// <summary>
    /// add summary here
    /// </summary>
    public struct SegmentedWordModel
    {
        public int StartTimestamp;
        public int EndTimestamp;
        public string Word;
    }
}