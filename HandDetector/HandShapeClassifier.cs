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
using System.IO;
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
            InitializeAllGestureFromData();
        }

        public static HandShapeClassifier GetSingleton()
        {
            if (singleton == null)
            {
                singleton = new HandShapeClassifier();
            }
            return singleton;
        }

        List<float[]> templateHOGs;
        List<Image<Bgr, byte>> templateImages;

        private void InitializeAllGestureFromImage()
        {
            //templateImages = new Image<Bgr, byte>[4];
            //string path = @"C:\Users\Administrator\Desktop\handshapes\";
            //templateHOGs = new float[4][];
            //for (int i = 0; i < templateImages.Length; i++)
            //{
            //    templateImages[i] = new Image<Bgr, byte>(path + "handshape" + (i + 1) + "-1.jpg");
            //    templateHOGs[i] = m_OpenCVController.ResizeAndCalHog(ref templateImages[i]);
            //}

        }

        private void InitializeAllGestureFromData()
        {
            try
            {
                string path = @"C:\Users\Administrator\Desktop\handshapes\standart hands\out_resized\";            
                StreamReader sr = File.OpenText(path +"hog.txt");
                templateHOGs = new List<float[]>();
                templateImages = new List<Image<Bgr, byte>>();

                string line = sr.ReadLine();
                int i = 1;
                while (!String.IsNullOrEmpty(line))
                {
                    string[] cell = line.Substring(0,line.Length-1).Split(' ');
                    templateHOGs.Add(cell.Select(x => Convert.ToSingle(x)).ToArray());
                    //string temppath = path + ((i / 5) + 1) + "_" + (i % 5 +1) + ".jpg";
                    string temppath = path + i.ToString() + "_0"  + ".jpg";
                    //Console.WriteLine(temppath);
                    templateImages.Add(new Image<Bgr, byte>(temppath));
                    line = sr.ReadLine();
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private float GetDistance(float[] v1, float[] v2)
        {
            float sum = 0;
            if (v1 == null)
            {
                return float.MaxValue;
            }
            for (int i = 0; i < v1.Length; i++)
            {
                float diff = v1[i] - v2[i];
                sum += diff * diff;

            }
            sum /= v1.Length;
            return sum;
        }
        public Image<Bgr, byte>[] RecognizeGesture(Image<Bgr, byte> image,int number)
        {
            float[] hog = m_OpenCVController.ResizeAndCalHog(ref image);
            return RecognizeGesture(hog,number);
        }
        struct pair
        {
            public int index;
            public float value;
        }

        /// <summary>
        /// recognize gesture.
        /// </summary>
        /// <param name="hog"></param>
        /// <param name="number">number of top similarity images </param>
        /// <returns></returns>
        public Image<Bgr, byte>[] RecognizeGesture(float[] hog, int number)
        {
            if (hog == null)
            {
                return null;
            }
            List<pair> result = new List<pair>();
            for (int i = 0; i < templateHOGs.Count(); i++)
            {
                float dis = GetDistance(hog, templateHOGs[i]);
                result.Add(new pair(){index = i,value = dis});
            }
            pair[] p = result.OrderBy(x => x.value).Take(number).ToArray();
            //Console.WriteLine(Math.Sqrt(p[0].value));
            //Console.WriteLine(Math.Sqrt(p[1].value));
            //Console.WriteLine(Math.Sqrt(p[2].value));
            //Console.WriteLine("-------------");
            Image<Bgr, byte>[] imgArray = p.Select(x=>templateImages[x.index]).ToArray();

            return imgArray;
        }
    }
}