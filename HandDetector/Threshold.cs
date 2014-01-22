// author：      Administrator
// created time：2014/1/22 16:23:40
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class Threshold
    {
        private static List<object> ThresholdList = new List<object>();
        public Threshold()
        {

        }

        public static void RegisterThreshold<T>(string name, T value, T min, T max) where T : System.IComparable<T>
        {
            value = value.CompareTo(max) > 0 ? max : value;
            value = value.CompareTo(min) < 0 ? min : value;
            ThresholdModel<T> newModel = new ThresholdModel<T>()
            {
                Name = name,
                Value = value,
                Max = max,
                Min = min
            };
            ThresholdList.Add(newModel);
            
        }


    }

    public struct ThresholdModel<T>
    {
        public string Name;
        public T Value;
        public T Min;
        public T Max;
    }

}