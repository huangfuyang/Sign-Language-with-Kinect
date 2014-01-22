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
            CANNY_THRESH = 100;
            CANNY_CONNECT_THRESH = 200;
        }

        public static OpenCVController GetSingletonInstance()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new OpenCVController();
            }
            return singletonInstance;
        }

        public Image<Bgr, Byte> RecogBlob(BitmapSource bs)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bs.ToBitmap());
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
            using (MemStorage stor = new MemStorage())
            {
                //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                Contour<System.Drawing.Point> contours = gray_image.FindContours(
                 Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                 Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                 stor);
                int blobCount = 0;

                for (int i = 0; contours != null; contours = contours.HNext)
                {
                    i++;

                    if ((contours.Area > Math.Pow(10, 2)) && (contours.Area < Math.Pow(100, 2)))
                    {
                        MCvBox2D box = contours.GetMinAreaRect();
                        openCVImg.Draw(box, new Bgr(System.Drawing.Color.Red), 2);
                        blobCount++;
                    }
                }
            }
            return (openCVImg);

        }

        public Image<Gray, Byte> RecogEdge(BitmapSource bs)
        {
            return RecogEdge(bs.ToBitmap());
        }

        public Image<Gray, Byte> RecogEdge(Bitmap bs)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bs);
            return CannyEdge(openCVImg, CANNY_THRESH,CANNY_CONNECT_THRESH );
        }

        private Image<Gray, Byte> CannyEdge(Image<Bgr, Byte> img, double cannyThresh, double cannyConnectThresh)
        {
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>();
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