// author：      Administrator
// created time：2014/5/16 12:45:17
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class HandShapeClassifier
    {
        private static HandShapeClassifier singleton;

        private OpenCVController m_OpenCVController;
        private HandShapeClassifier()
        {
            m_OpenCVController = OpenCVController.GetSingletonInstance();
            InitializeAllGesture();
        }

        public static HandShapeClassifier GetSingleton()
        {
            if (singleton == null)
            {
                singleton = new HandShapeClassifier();
            }
            return singleton;
        }

        float[][] templateHOGs;
        Image<Bgr, byte>[] templateImages;

        private void InitializeAllGesture()
        {
            templateImages = new Image<Bgr, byte>[4];
            string path = @"C:\Users\Administrator\Desktop\handshapes\";
            templateHOGs = new float[4][];
            for (int i = 0; i < templateImages.Length; i++)
            {
                templateImages[i] = new Image<Bgr, byte>(path + "handshape" + (i+1) + "-1.jpg");
                templateHOGs[i] = m_OpenCVController.CalHog(templateImages[i]);
            }

        }

        private float GetDistance(float[] v1, float[] v2)
        {
            float sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                float diff = v1[i] - v2[i];
                sum += diff * diff;

            }
            sum /= v1.Length;
            return sum;
        }
        public Image<Bgr, byte> RecognizeGesture(Image<Bgr, byte> image)
        {
            float[] hog = m_OpenCVController.CalHog(image);
            float min = float.MaxValue;
            int index = -1;
            for (int i = 0; i < templateHOGs.Length; i++)
            {
                float dis = GetDistance(hog,templateHOGs[i]);
                if (dis<min)
                {
                    min = dis;
                    index = i;
                }
            }
            Console.WriteLine(index);
            return templateImages[index];
        }
    }
}