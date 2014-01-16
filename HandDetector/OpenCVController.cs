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

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class OpenCVController : INotifyPropertyChanged
    {
        public int CANNY_LOW_THRESH { get; set; }
        public int CANNY_HIGH_THRESH { get; set; }
        private int CANNY_SIZE = 3;
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

        public Image<Bgr, Byte> RecogEdge(BitmapSource bs)
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(bs.ToBitmap());
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
            CannyEdge(bs, bs);
        }

        private void CannyEdge(IntPtr imgin, IntPtr imgout)
        {
            CvInvoke.cvSmooth(imgin, imgout, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 3, 0, 0, 0);
            CvInvoke.cvCanny(imgin, imgout, CANNY_LOW_THRESH, CANNY_HIGH_THRESH, CANNY_SIZE);

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