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
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Drawing;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class OpenCVController : INotifyPropertyChanged
    {
        public static double CANNY_THRESH;
        public static double CANNY_CONNECT_THRESH;


        private static OpenCVController singletonInstance;
        private OpenCVController()
        {
            CANNY_THRESH = 10;
            CANNY_CONNECT_THRESH = 20;
        }

        public static OpenCVController GetSingletonInstance()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new OpenCVController();
            }
            return singletonInstance;
        }

        public Rectangle RecogBlob(PointF pos, int radius, Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            openCVImg.ROI = new Rectangle((int)pos.X - radius, (int)pos.Y - radius, radius * 2, radius * 2);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>().PyrDown().PyrUp();
            Image<Gray, Byte> cannyEdges = openCVImg.Canny(CANNY_THRESH, CANNY_CONNECT_THRESH);
            using (MemStorage stor = new MemStorage())
            {
                //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                Contour<System.Drawing.Point> contours = cannyEdges.FindContours(
                 Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                 Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
                //Contour<System.Drawing.Point> contours = cannyEdges.FindContours();
                Rectangle rtn_rec = new Rectangle(0, 0, 0, 0);
                double max_area = 0;
                for (int i = 0; contours != null; contours = contours.HNext)
                {
                    i++;
                    if ((contours.Area > Math.Pow(10, 2)) && (contours.Area < Math.Pow(radius * 2, 2)) && contours.Area > max_area)
                    {
                        // Console.WriteLine(contours.Area);
                        rtn_rec = contours.GetMinAreaRect().MinAreaRect();
                        max_area = contours.Area;
                    }
                }
                rtn_rec.X += (int)pos.X - radius;
                rtn_rec.Y += (int)pos.Y - radius;
                return rtn_rec;

            }

        }
        public void CalHistogram(Bitmap bmp)
        {
            Image<Gray, Byte> img = new Image<Gray, byte>(bmp);
            // DenseHistogram hist = new DenseHistogram(
        }
        public Rectangle[] RecogBlob(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
            Image<Gray, Byte> binaryImg = gray_image.ThresholdBinaryInv(new Gray(200), new Gray(255));
            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
            Contour<System.Drawing.Point> contours = binaryImg.FindContours(
             Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
             Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL);
            //Contour<System.Drawing.Point> contours = cannyEdges.FindContours();
            if (contours == null)
            {
                return null;
            }
            List<Rectangle> recList = new List<Rectangle>();
            double max_area = 0;
            for (int i = 0; contours != null; contours = contours.HNext)
            {
                if ((contours.Area > Math.Pow(25, 2)) && (contours.Area < Math.Pow(300, 2)))
                {
                    // Console.WriteLine(contours.Area);
                    recList.Add(contours.GetMinAreaRect().MinAreaRect());
                    //max_area = contours.Area;
                    i++;

                }
            }

            return recList.ToArray();
        }

        public Bitmap Histogram(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));
            Histo.Calculate(new Image<Gray, Byte>[] { gray_image }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            float[] GrayHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GrayHist, 0);
            float max = 0;
            for (int i = 0; i < 256; i++)
            {
                if (GrayHist[i] > max)
                {
                    max = GrayHist[i];
                }
            }
            int height = 200;
            Bitmap histbmp = new Bitmap(512, height);
            using (Graphics g = Graphics.FromImage(histbmp))
            {
                for (int i = 0; i < 256; i++)
                {
                    Point p1 = new Point(i, height - (int)(GrayHist[i] / max * height));
                    Point p2 = new Point(i, height);
                    g.DrawLine(new Pen(Brushes.Red, 2), p1, p2);

                }
            }
            return histbmp;

        }
        public Bitmap Color2Gray(Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            return openCVImg.Convert<Gray, byte>().ToBitmap();
        }

        public Bitmap Color2Edge(PointF pos, int radius, Bitmap bmp)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bmp);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>().PyrDown().PyrUp();
            gray_image.ROI = new Rectangle((int)pos.X - radius, (int)pos.Y - radius, radius * 2, radius * 2);
            Image<Gray, Byte> cannyEdges = openCVImg.Canny(50, 150);
            cannyEdges.ROI = Rectangle.Empty;
            return cannyEdges.ToBitmap();
        }


        public Image<Gray, Byte> RecogEdge(BitmapSource bs)
        {
            return RecogEdge(bs.ToBitmap());
        }

        public Image<Gray, Byte> RecogEdge(Bitmap bs)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bs);
            return CannyEdge(openCVImg, CANNY_THRESH, CANNY_CONNECT_THRESH);
        }

        private Image<Gray, Byte> CannyEdge(Image<Bgr, Byte> img, double cannyThresh, double cannyConnectThresh)
        {
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            return gray.Canny(cannyThresh, cannyConnectThresh);
        }

        public Image<Gray, Byte> RecogEdgeBgra(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            return CannyEdgeBgra(openCVImg, CANNY_THRESH, CANNY_CONNECT_THRESH);
        }

        private Image<Gray, Byte> CannyEdgeBgra(Image<Bgra, Byte> img, double cannyThresh, double cannyConnectThresh)
        {
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();
            return gray.Canny(cannyThresh, cannyConnectThresh);
        }




        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}