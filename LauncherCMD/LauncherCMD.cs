using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CURELab.SignLanguage.RecognitionSystem;

namespace CURELab.SignLanguage.Launcher
{
    class LauncherCMD
    {
        static void Main(string[] args)
        {
            Console.WriteLine("this is a launcher without kinect");
            RecognitionController lc = new RecognitionController();
            lc.BeginTest();
            Console.WriteLine("end");
            Console.Read();
        }
    }
}
