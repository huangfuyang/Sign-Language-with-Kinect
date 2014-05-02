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
    public enum HandEnum
    {
        Right, Left, Both, Intersect
    }
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
        public Bitmap Histogram(Image<Bgra, Byte> image)
        {
            Image<Gray, byte> gray_image = image.Convert<Gray, byte>();
            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));
            Histo.Calculate(new Image<Gray, Byte>[] { gray_image }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            float[] GrayHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GrayHist, 0);
            float max = 1;
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
        public Bitmap Histogram(Bitmap bmp)
        {
            Image<Bgra, Byte> openCVImg = new Image<Bgra, byte>(bmp);
            return Histogram(openCVImg);

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


        #region Hand recognition

        Image<Gray, Byte> binaryImg;
        Image<Gray, Byte> grayImg;
        int begin = 50;
        int end = 80;
        int minLength = 90;
        private bool Intersect = false;
        int minSize = 1000;
        Point RightHandCenter = new Point();
        Point LeftHandCenter = new Point();
        int hogSize = 4356;

        public unsafe HandShapeModel FindHandPart(ref Image<Bgra, Byte> image)
        {
            HandShapeModel model = new HandShapeModel();
            Image<Gray, byte> gray_image = image.Convert<Gray, byte>();
            grayImg = gray_image;
            binaryImg = gray_image.ThresholdBinaryInv(new Gray(200), new Gray(255));
            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
            IntPtr Dyncontour = new IntPtr();//存放检测到的图像块的首地址

            IntPtr Dynstorage = CvInvoke.cvCreateMemStorage(0);
            int n = CvInvoke.cvFindContours(binaryImg.Ptr, Dynstorage, ref Dyncontour, sizeof(MCvContour),
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new System.Drawing.Point(0, 0));
            Seq<System.Drawing.Point> DyncontourTemp1 = new Seq<System.Drawing.Point>(Dyncontour, null);//方便对IntPtr类型进行操作
            Seq<System.Drawing.Point> DyncontourTemp = DyncontourTemp1;
            List<MCvBox2D> rectList = new List<MCvBox2D>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt32() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                //iterate contours
                if (DyncontourTemp.GetMinAreaRect().GetTrueArea() < minSize)
                {
                    continue;
                }
                CvInvoke.cvDrawContours(image, DyncontourTemp, new MCvScalar(255, 0, 0), new MCvScalar(0, 255, 0), 10, 1, Emgu.CV.CvEnum.LINE_TYPE.FOUR_CONNECTED, new System.Drawing.Point(0, 0));
                PointF[] rect1 = DyncontourTemp.GetMinAreaRect().GetVertices();

                rectList.Add(DyncontourTemp.GetMinAreaRect());
                var pointfSeq =
                               from p in rect1
                               select new System.Drawing.Point((int)p.X, (int)p.Y);
                System.Drawing.Point[] points = pointfSeq.ToArray();
                DrawPoly(points, image, new MCvScalar(255, 0, 0));
            }
            rectList = rectList.OrderByDescending(x => x.GetTrueArea()).ToList();
            MCvBox2D rightHand;
            MCvBox2D leftHand;

            System.Drawing.Point[] SplittedLeftHand;
            Font textFont = new Font(FontFamily.Families[0], 20);
            // count hands number
            using (Graphics g = Graphics.FromImage(image.Bitmap))
            {
                if (rectList.Count() >= 2)//two hands
                {
                    if (rectList[0].center.X > rectList[1].center.X)
                    {
                        rightHand = rectList[0];
                        leftHand = rectList[1];
                    }
                    else
                    {
                        rightHand = rectList[1];
                        leftHand = rectList[0];
                    }
                    if (rightHand.MinAreaRect().IsCloseTo(leftHand.MinAreaRect(), 5))
                    {
                        Intersect = true;
                    }
                    else
                    {
                        Intersect = false;
                    }

                    //right hand
                    SplitAndDrawHand(rightHand, image, HandEnum.Right);
                    //left hand
                    SplitAndDrawHand(leftHand, image, HandEnum.Left);
                    g.DrawString("left and right", textFont, Brushes.Red,
                      0, 20);

                    DrawPoint(image, LeftHandCenter, new MCvScalar(255, 0, 0));
                    model = new HandShapeModel(hogSize, HandEnum.Both);

                }
                else if (rectList.Count() == 1) // one rectangle
                {
                    string text = "";


                    if (Intersect)
                    {
                        text = "Two hands";
                        SplitAndDrawHand(rectList[0], image, HandEnum.Intersect);
                        model = new HandShapeModel(hogSize, HandEnum.Intersect);
                    }
                    else
                    {
                        text = "right";
                        SplitAndDrawHand(rectList[0], image, HandEnum.Right);
                        model = new HandShapeModel(hogSize, HandEnum.Right);
                    }
                    g.DrawString(text, textFont, Brushes.Red,
                        0, 20);

                }
            }
            DrawPoint(image, RightHandCenter, new MCvScalar(255,0,0));
            return model;
        }
      

        private Point GetCenterPoint(Point[] points)
        {
            try
            {
                if (points.Length <= 0)
                {
                    return Point.Empty;
                }
                int X = (int)points.Average((x => x.X));
                int Y = (int)points.Average((x => x.Y));
                return new Point(X, Y);
            }
            catch (Exception)
            {

                return Point.Empty;
            }

        }

        private void SplitAndDrawHand(MCvBox2D rect, Image<Bgra, Byte> image, HandEnum handEnum)
        {
            System.Drawing.Point[] SplittedHand = SplitHand(rect, handEnum);
            DrawPoly(SplittedHand, image, new MCvScalar(0, 0, 255));
            Point center = GetCenterPoint(SplittedHand);
            if (center == Point.Empty)
            {
                center = rect.center.ToPoint();
            }
            if (handEnum == HandEnum.Right)
            {
                RightHandCenter = center;
            }
            if (handEnum == HandEnum.Left)
            {
                LeftHandCenter = center;
            }
            if (handEnum == HandEnum.Intersect)
            {
                RightHandCenter = center;
            }
        }

        private void DrawPoly(System.Drawing.Point[] points, Image<Bgra, Byte> image, MCvScalar color)
        {

            if (points == null || points.Length <= 0)
            {
                return;
            }
            for (int j = 0; j < points.Length; j++)
            {
                CvInvoke.cvLine(image, points[j], points[(j + 1) % points.Length], color, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
            }
        }

        private void DrawPoint(Image<Bgra, Byte> image, Point point, MCvScalar color)
        {
            if (point == null || point == Point.Empty)
            {
                return;
            }
            CvInvoke.cvCircle(image, point,3,color,-1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED,0);
        }
        private System.Drawing.Point[] SplitHand(MCvBox2D rect, HandEnum handEnum)
        {
            if (handEnum == HandEnum.Both || handEnum == HandEnum.Intersect)
            {
                return rect.MinAreaRect().GetPoints() ;
            }
            PointF[] pl = rect.GetVertices();
            Point[] splittedHands = new Point[4];
            //find angle of long edge
            PointF startP = pl[1];
            PointF shortP = pl[0];
            PointF longP = pl[2];
            PointF ap1 = new PointF();
            PointF ap2 = new PointF();

            if (pl[0].DistanceTo(startP) > pl[2].DistanceTo(startP))
            {
                shortP = pl[2];
                longP = pl[0];
            }

            float longDis = longP.DistanceTo(startP);
            if (longDis < minLength)
            {
                return null;
            }
            float shortDis = shortP.DistanceTo(startP);
            // x and long edge slope 
            float longslope = Math.Abs(longP.X - startP.X) / longDis;
            float min = 9999;
            float max = 0;

            // > 45
            if (longslope < 0.707)//vert
            {
                pl = pl.OrderBy((x => x.Y)).ToArray();
                startP = pl[0];
                shortP = pl[1];
                longP = pl[2];
                for (int y = begin; y < Convert.ToInt32(Math.Abs(longP.Y - startP.Y)) && Math.Abs(y) < end; y++)
                {
                    PointF p1 = InterPolateP(startP, longP, y / Math.Abs(longP.Y - startP.Y));
                    PointF p2 = new PointF(p1.X + shortP.X - startP.X, p1.Y + shortP.Y - startP.Y);
                    float dis = GetHandWidthBetween(p1, p2);
                    if (dis < min)
                    {
                        min = dis;
                        ap1 = p1;
                        ap2 = p2;
                    }
                }
            }
            else // horizontal 
            {
                if (handEnum == HandEnum.Right)
                {
                    pl = pl.OrderBy((x => x.X)).ToArray();

                }
                else if (handEnum == HandEnum.Left)
                {
                    pl = pl.OrderByDescending((x => x.X)).ToArray();
                }
                startP = pl[0];
                shortP = pl[1];
                longP = pl[2];
                for (int X = begin; X < Convert.ToInt32(Math.Abs(longP.X - startP.X)) && Math.Abs(X) < end; X++)
                {
                    PointF p1 = InterPolateP(startP, longP, X / Math.Abs(longP.X - startP.X));
                    PointF p2 = new PointF(p1.X + shortP.X - startP.X, p1.Y + shortP.Y - startP.Y);
                    float dis = GetHandWidthBetween(p1, p2);
                    if (dis < min)
                    {
                        min = dis;
                        ap1 = p1;
                        ap2 = p2;
                    }
                }
            }
            if (ap1 == null || ap1 == PointF.Empty)
            {
                return null;
            }
            splittedHands[0] = startP.ToPoint();
            splittedHands[1] = ap1.ToPoint();
            splittedHands[2] = ap2.ToPoint();
            splittedHands[3] = shortP.ToPoint();
            return splittedHands;
        }

        private PointF InterPolateP(PointF p1, PointF p2, float disToP1)
        {
            float x = (p2.X - p1.X) * Math.Abs(disToP1) + p1.X;
            float y = (p2.Y - p1.Y) * Math.Abs(disToP1) + p1.Y;
            return new PointF(x, y);
        }

        private float GetHandWidthBetween(PointF p1, PointF p2)
        {
            float slope = Math.Abs(p2.X - p1.X) / p2.DistanceTo(p1);
            PointF p3 = new PointF();
            PointF p4 = new PointF();
            if (slope < 0.707)//vert
            {

                for (int Y = 0; Y < Math.Abs(p2.Y - p1.Y); Y++)
                {
                    p3 = InterPolateP(p1, p2, Y / (p2.Y - p1.Y));
                    if (IsHand(p3)) break;

                }
                for (int Y = 0; Y < Math.Abs(p2.Y - p1.Y); Y++)
                {
                    p4 = InterPolateP(p2, p1, Y / (p2.Y - p1.Y));
                    if (IsHand(p4)) break;

                }
                return p3.DistanceTo(p4);
            }
            else//hori
            {
                for (int x = 0; x < Math.Abs(p2.X - p1.X); x++)
                {
                    p3 = InterPolateP(p1, p2, x / (p2.X - p1.X));
                    if (IsHand(p3)) break;

                }
                for (int x = 0; x < Math.Abs(p2.X - p1.X); x++)
                {
                    p4 = InterPolateP(p2, p1, x / (p2.X - p1.X));
                    if (IsHand(p4)) break;

                }
                return p3.DistanceTo(p4);
            }
        }

        private bool IsHand(PointF p)
        {
            try
            {
                if (grayImg[p.ToPoint()].Intensity < 200)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

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