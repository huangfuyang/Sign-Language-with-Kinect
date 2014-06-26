// author：      Administrator
// created time：2014/5/9 5:15:42
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
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
    public class SignWordModel
    {
        public string SignID;
        public string Signer;
        public string File;
        public string FullName;
        public string Chinese;
        public string English;
        public SignWordModel(string sign, string signer,string fullName, string file)
        {
            SignID = sign;
            Signer = signer;
            File = file;
            FullName = fullName;

        }
    }
}