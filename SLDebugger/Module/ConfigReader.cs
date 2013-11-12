// author：      fyhuang
// created time：2013/11/12 17:55:28
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CURELab.SignLanguage.Debugger.Module
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class ConfigReader : XMLReader
    {
        private static ConfigReader singletonInstance;
        private ConfigReader():base("Config.xml")
        {

        }

        public static ConfigReader GetSingletonConfigReader()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new ConfigReader();
            }
            return singletonInstance;
        }
    }
}