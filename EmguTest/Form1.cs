using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmguTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Image<Gray, Byte> binaryImg;
        Image<Gray, Byte> grayImg;
        int begin = 30;
        int width = 60;
        int minLength = 65;
        Image<Bgr, Byte> image;
        private unsafe void Form1_Load(object sender, EventArgs e)
        {
            image = new Image<Bgr, byte>(@"C:\Users\Administrator\Desktop\Picture1.jpg");
            //ProcessImage(ref image);
            long time;
            Find(image, out time);

            imageBox1.Image = image;
        }

        void imageBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ImageBox ib = sender as ImageBox;
            image = new Image<Bgr, byte>(ib.ImageLocation);
            ProcessImage(ref image);
            imageBox1.Image = image;
        }

        private unsafe void ProcessImage(ref Image<Bgr, byte> img)
        {
            Image<Gray, byte> gray_image = img.Convert<Gray, byte>();
            grayImg = gray_image;
            binaryImg = gray_image.ThresholdBinaryInv(new Gray(200), new Gray(255));
            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
            IntPtr Dyncontour = new IntPtr();//存放检测到的图像块的首地址

            IntPtr Dynstorage = CvInvoke.cvCreateMemStorage(0);
            int n = CvInvoke.cvFindContours(binaryImg.Ptr, Dynstorage, ref Dyncontour, sizeof(MCvContour),
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new Point(0, 0));
            Seq<Point> DyncontourTemp1 = new Seq<Point>(Dyncontour, null);//方便对IntPtr类型进行操作
            Seq<Point> DyncontourTemp = DyncontourTemp1;
            List<MCvBox2D> rectList = new List<MCvBox2D>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt32() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {

                CvInvoke.cvDrawContours(image, DyncontourTemp, new MCvScalar(255, 0, 0), new MCvScalar(0, 255, 0), 10, 1, Emgu.CV.CvEnum.LINE_TYPE.FOUR_CONNECTED, new Point(0, 0));
                PointF[] rect1 = DyncontourTemp.GetMinAreaRect().GetVertices();
                rectList.Add(DyncontourTemp.GetMinAreaRect());
                var pointfSeq =
                               from p in rect1
                               select new Point((int)p.X, (int)p.Y);
                Point[] points = pointfSeq.ToArray();
                for (int j = 0; j < 4; j++)
                {
                    CvInvoke.cvLine(image, points[j], points[(j + 1) % 4], new MCvScalar(0, 0, 255), 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
                }
                //CvInvoke.cvNamedWindow("main");
            }
            for (int i = 0; i < rectList.Count(); i++)
            {
                MCvBox2D rect = rectList[i];
                PointF[] pl = rect.GetVertices();
                var points =
                  from p in pl
                  orderby p.Y ascending
                  select p;
                pl = points.ToArray();
                PointF startP = pl[0];
                PointF shortP = pl[1];
                PointF longP = pl[2];
                if (pl[1].DistanceTo(startP) > pl[2].DistanceTo(startP))
                {
                    shortP = pl[2];
                    longP = pl[1];
                }

                float longDis = longP.DistanceTo(startP);
                if (longDis < minLength)
                {
                    continue;
                }
                float shortDis = shortP.DistanceTo(startP);
                float longslope = Math.Abs(longP.X - startP.X) / longDis;
                float min = 9999;
                PointF ap1 = new PointF();
                PointF ap2 = new PointF();

                if (longslope < 0.707)//vert
                {

                    for (int y = begin; y < Convert.ToInt32(Math.Abs(longP.Y - startP.Y)) && Math.Abs(y) < width; y++)
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
                else
                {

                    for (int X = begin; X < Convert.ToInt32(Math.Abs(longP.X - startP.X)) && Math.Abs(X) < width; X++)
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
                CvInvoke.cvLine(image, ap1.ToPoint(), ap2.ToPoint(), new MCvScalar(0, 0, 255), 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);

            }



            //CvInvoke.cvShowImage("main", tempImage);


            //image = binaryImg.Convert<Bgr, Byte>();
            //Bitmap bmp = image.ToBitmap();
            //using (Graphics g = Graphics.FromImage(bmp))
            //{
            //    List<Rectangle> recList = new List<Rectangle>();
            //    double max_area = 0;
            //    for (int i = 0; contours != null; contours = contours.HNext)
            //    {
            //        if ((contours.Area > Math.Pow(25, 2)) && (contours.Area < Math.Pow(300, 2)))
            //        {
            //            // Console.WriteLine(contours.Area);
            //            Seq<Point> seq = contours.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
            //            var pointfSeq =
            //                    from p in seq
            //                    select new PointF(p.X, p.Y);
            //            //max_area = contours.Area;
            //            // MCvBox2D ellp = CvInvoke.cvFitEllipse2(seq.Ptr);
            //            MCvBox2D ellp = contours.GetMinAreaRect();

            //            //g.DrawPolygon(new Pen(Brushes.Red, 2), contours.ToArray());
            //            //Ellipse elps = PointCollection.EllipseLeastSquareFitting(pointfSeq.ToArray());
            //            g.TranslateTransform(ellp.center.X, ellp.center.Y);
            //            g.RotateTransform(ellp.angle);
            //            g.DrawRectangle(new Pen(Brushes.Red, 2), -ellp.MinAreaRect().Width / 2,
            //                                                    -ellp.MinAreaRect().Height / 2,
            //                                                    ellp.MinAreaRect().Width,
            //                                                    ellp.MinAreaRect().Height);
            //            g.RotateTransform(-ellp.angle);

            //            g.DrawRectangle(new Pen(Brushes.Red, 2), -ellp.MinAreaRect().Width / 2,
            //                                                   -ellp.MinAreaRect().Height / 2,
            //                                                   ellp.MinAreaRect().Width,
            //                                                   ellp.MinAreaRect().Height);
            //            // g.DrawRectangle(new Pen(Brushes.Red, 2), -elps.MCvBox2D.MinAreaRect().Width / 2, -elps.MCvBox2D.MinAreaRect().Height / 2, elps.MCvBox2D.MinAreaRect().Width, elps.MCvBox2D.MinAreaRect().Height);
            //            //g.DrawEllipse(new Pen(Brushes.Red, 2), -ellp.MinAreaRect().Width / 2,
            //            //                                        -ellp.MinAreaRect().Height / 2, 
            //            //                                        ellp.MinAreaRect().Width, 
            //            //                                        ellp.MinAreaRect().Height);
            //            g.ResetTransform();

            //            // break;
            //        }
            //    }
            //}

            //imageBox1.Image = new Image<Bgr, byte>(bmp);
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
        public static Image<Bgr, Byte> Find(Image<Bgr, Byte> image, out long processingTime)
        {
            Stopwatch watch;
            Rectangle[] regions;
            float[] result;

            using (HOGDescriptor des = new HOGDescriptor())
            {
                watch = Stopwatch.StartNew();
                result = des.Compute(image, new Size(16, 16), Size.Empty, null);
                watch.Stop();
                result = result.Where(x => x != 0).ToArray();
                //regions = des.DetectMultiScale(image);
            }


            processingTime = watch.ElapsedMilliseconds;

          
            return image;
        }
    }
}
