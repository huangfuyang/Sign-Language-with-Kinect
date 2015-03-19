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
using System.Windows;
using System.Windows.Forms;
using Emgu.Util;
using Emgu.CV;
using Emgu.CV.UI;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Kinect;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace CURELab.SignLanguage.HandDetector
{
    public enum HandEnum
    {
        Right, LeftTouch, RightTouch, Left, BothTouch,Both, Intersect, IntersectTouch, None
    }
    /// <summary>
    /// add summary here
    /// </summary>
    public class OpenCVController : INotifyPropertyChanged
    {
        public static double CANNY_THRESH;
        public static double CANNY_CONNECT_THRESH;
        public HOGDescriptor Hog_Descriptor;
        private ImageViewer viewer;
        private static OpenCVController singletonInstance;
        private OpenCVController()
        {
            CANNY_THRESH = 10;
            CANNY_CONNECT_THRESH = 20;
            Hog_Descriptor = new HOGDescriptor(new Size(60, 60), new Size(10, 10), new Size(5, 5), new Size(5, 5), 9, 1, -1, 0.2, false);
            viewer = new ImageViewer();
            //viewer.Show();
            headRec = new Rectangle(new Point(320, 0), new Size());
            rightRec = new Rectangle(new Point(640, 480), new Size());
            leftRec = new Rectangle(new Point(0, 480), new Size());
            headMinDepth = 0;

        }

        public static OpenCVController GetSingletonInstance()
        {
            return singletonInstance ?? (singletonInstance = new OpenCVController());
        }


        public void CalHistogram(Bitmap bmp)
        {
            Image<Gray, Byte> img = new Image<Gray, byte>(bmp);
            // DenseHistogram hist = new DenseHistogram(
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


        private bool Intersect = false;

        Point RightHandCenter = new Point();
        Point LeftHandCenter = new Point();
        int hogSize = 4356;
        Image<Gray, Byte> grayImg;
        public unsafe HandShapeModel FindHandPart(
            ref Image<Bgra, Byte> image,
            out Image<Gray, Byte> rightFront,
            out Image<Gray, Byte> leftFront,
            int handDepth,
            PointF rightVector,
            PointF leftVector,
            bool leftHandRaise
       )
        {
            //process hand size
            if (handDepth > 0)
            {
                begin = (int)(130.0 * 240.0 / handDepth / 0.39);
                end = (int)(250 * 240.0 / handDepth / 0.39);
                minLength = (int)(150 * 240.0 / handDepth / 0.39);
            }
            else
            {
                rightFront = null;
                leftFront = null;
                return null;
            }

            //Console.WriteLine(begin);
            //Console.WriteLine(end);
            //Console.WriteLine(minLength);
            rightFront = null;
            leftFront = null;
            HandShapeModel model = null;
            //find contour
            grayImg = image.Convert<Gray, byte>();
            Image<Gray, Byte> binaryImg = GetBinaryImg(image);
            var rectList = FindContourMBox(binaryImg, 2);
            //draw contour
            foreach (var rect in rectList)
            {
                DrawPoly(rect.GetVertices().ToPoints(), image, new MCvScalar(255, 0, 0));
            }
            // find hand
            rectList = rectList.OrderByDescending(x => x.GetTrueArea()).ToList();
            MCvBox2D rightHand;
            MCvBox2D leftHand;

            Font textFont = new Font(FontFamily.Families[0], 20);
            // count hands number
            using (Graphics g = Graphics.FromImage(image.Bitmap))
            {
                // 3 conditions in total
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
                    // mark intersect state
                    Intersect = rightHand.MinAreaRect().IsCloseTo(leftHand.MinAreaRect(), 5);
                    //right hand
                    MCvBox2D SplittedRightHand = SplitHand(rightHand, HandEnum.Right, rightVector);
                    rightFront = GetSubImage<Gray>(binaryImg, SplittedRightHand, rightHand.angle);
                    float[] rightHog = CalHog(rightFront);
                    DrawHand(SplittedRightHand, image, HandEnum.Right);
                    //left hand
                    MCvBox2D SplittedLeftHand = SplitHand(leftHand, HandEnum.Left, leftVector);
                    leftFront = GetSubImage<Gray>(binaryImg, SplittedLeftHand, leftHand.angle);
                    float[] leftHog = CalHog(leftFront);
                    DrawHand(SplittedLeftHand, image, HandEnum.Left);
                    g.DrawString("left and right", textFont, Brushes.Red, 0, 20);

                    model = new HandShapeModel(hogSize, HandEnum.Both);
                    model.hogLeft = leftHog;
                    model.hogRight = rightHog;
                }
                else if (rectList.Count() == 1) // one rectangle
                {
                    string text = "";
                    leftFront = null;
                    Intersect = false;

                    if (leftHandRaise || Intersect)
                    {

                        text = "Two hands";
                        var twoHands = FindContourRect(binaryImg);
                        if (twoHands != null && twoHands.Count > 0)
                        {
                            rightFront = GetBinarySubImageByRect<Gray>(binaryImg, twoHands[0]);
                            DrawHand(twoHands[0].ToCvBox2D(), image, HandEnum.Intersect);
                            float[] TwoHandHOG = CalHog(rightFront);
                            model = new HandShapeModel(hogSize, HandEnum.Intersect);
                            model.hogRight = TwoHandHOG;
                        }

                    }
                    else
                    {
                        text = "right";
                        MCvBox2D SplittedRightHand = SplitHand(rectList[0], HandEnum.Right, rightVector);

                        rightFront = GetSubImage<Gray>(binaryImg, SplittedRightHand, rectList[0].angle);
                        DrawHand(SplittedRightHand, image, HandEnum.Right);
                        float[] TwoHandHOG = CalHog(rightFront);
                        model = new HandShapeModel(hogSize, HandEnum.Right);
                        model.hogRight = TwoHandHOG;
                    }
                    g.DrawString(text, textFont, Brushes.Red, 0, 20);

                }
                else // no hand detected
                {
                    Intersect = false;
                }
            }


            return model;
        }

        // skin detection
        private Rectangle headRec;
        private Rectangle rightRec;
        private Rectangle leftRec;
        private int headMinDepth;
        public HandShapeModel FindHandFromColor(
            Image<Gray, byte> depthImage, ref byte[] colorPixels, DepthImagePoint[] depthMap, PointF head, int headDepth
            )
        {
            //Console.WriteLine(headDepth + 200);
            //Console.WriteLine("head min:{0}", headMinDepth);CV
            //Console.WriteLine("head :x{0} y{1}", head.X,head.Y);
            int width = 640;
            int height = 480;
            int bytePerPixel = colorPixels.Length / width / height;
            // loop over each row and column of the depth

            for (int colorIndex = 0; colorIndex < colorPixels.Length; colorIndex += bytePerPixel)
            {
                try
                {

                    var B = colorPixels[colorIndex];
                    var G = colorPixels[colorIndex + 1];
                    var R = colorPixels[colorIndex + 2];
                    //var Y = ((66*R + 129*G + 25*B + 128) >> 8) + 16;
                    var U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                    var V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;
                    var d = depthMap[colorIndex/bytePerPixel].Depth;
                    //if (!(U > 100 && U < 129 && V > 140 && V < 170))//aaron
                    //if (!(U > 100 && U < 135 && V > 138 && V < 170))//Micheal
                    //if (!(U > 95 && U < 135 && V > 134 && V < 170) || d > headDepth + 200 || d == 0) //realtime
                    if (!(U > 95 && U < 135 && V > 134 && V < 170) || d > headDepth + 200) //realtime with hole filling
                    {
                        colorPixels[colorIndex] = 0;
                        colorPixels[colorIndex + 1] = 0;
                        colorPixels[colorIndex + 2] = 0;  
                    }

                }
                catch (Exception)
                {     
                    continue;
                }

            }
            var colorImg = ImageConverter.Array2Image<Bgra>(colorPixels, width, height, width * bytePerPixel).
                Convert<Gray, byte>().SmoothMedian(3);
            var binaryImg = colorImg.ThresholdToZeroInv(new Gray(240));
            var clist = FindContourRect(binaryImg, 3);
            HandShapeModel handModel = null;
            //Console.WriteLine(headMinDepth);
            switch (clist.Count)
            {
                case 1://two hands and head touch
                    handModel = GetSubImageByRectAndDepth(depthImage, colorImg, clist, headMinDepth, depthMap);
                    break;
                case 2:
                    // touch or intersect
                    handModel = GetSubImageByRectAndDepth(depthImage, colorImg, clist, headMinDepth, depthMap);
                    break;
                case 3:
                    clist = clist.OrderBy(x => x.GetCenter().DistanceTo(head)).ToList();
                    var tlist = clist.Skip(1).OrderBy(x => x.GetXCenter()).ToList();
                    rightRec = tlist[1];
                    leftRec = tlist[0];
                    headRec = clist[0];
                    if (!rightRec.IsCloseTo(headRec,2) && !leftRec.IsCloseTo(headRec,2))
                    {
                        var cdepth = GetRectMinDepth(headRec, depthMap);
                        if (cdepth < 10000)
                        {
                            if (headMinDepth == 0)
                            {
                                headMinDepth = cdepth;
                            }
                            else
                            {
                                // head hand occluded
                                // do not use when real time
                                if (cdepth >= headMinDepth - 50 || Math.Abs(headMinDepth-headDepth)>200)
                                {
                                    headMinDepth = cdepth;
                                }
                            }
                        }
                    }
                   
                    handModel = new HandShapeModel(HandEnum.Both)
                    {
                        RightColor = GetSubImageByRect(colorImg, rightRec),
                        RightDepth = GetSubImageByRect(depthImage, rightRec),
                        LeftColor = GetSubImageByRect(colorImg, leftRec),
                        LeftDepth = GetSubImageByRect(depthImage, leftRec),
                        right = rightRec,
                        left = leftRec
                    };
                    break;
                default:
                    break;

            }
            //Console.WriteLine("{0} {1}",leftRec.X,rightRec.X);
            foreach (var rect in clist)
            {
                DrawPoly(rect.GetPoints(), colorImg, new MCvScalar(255, 255, 255));
            }
            DrawRects(handModel,colorImg);
            viewer.Image = colorImg;
            colorPixels = colorImg.Convert<Bgra, byte>().Bytes;
            return handModel;
        }

        private void DrawRects(HandShapeModel model, Image<Gray, Byte> image)
        {
            if (model == null)
            {
                return;
            }
            switch (model.type)
            {
                case HandEnum.None:
                    break;

                case HandEnum.Both:
                case HandEnum.BothTouch:
                case HandEnum.LeftTouch:
                case HandEnum.RightTouch:
                    DrawPoly(model.left.GetPoints(), image, new MCvScalar(123, 123, 123));
                    DrawPoly(model.right.GetPoints(), image, new MCvScalar(123, 123, 123));
                    break;
                case HandEnum.Intersect:
                case HandEnum.IntersectTouch:
                    DrawPoly(model.intersectCenter.GetPoints(), image, new MCvScalar(123, 123, 123));
                    break;
            }
        }

        private int GetRectMinDepth(Rectangle rect, DepthImagePoint[] depthMap)
        {
            int Min = int.MaxValue;
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;
            rect.Width = rect.Width + rect.X > 640 ? 640 - rect.X : rect.Width;
            rect.Height = rect.Height + rect.Y > 480 ? 480 - rect.Y : rect.Height;
            for (int i = rect.Y; i < rect.Y + rect.Height; i++)
            {
                for (int j = rect.X; j < rect.X + rect.Width; j++)
                {
                    var d = depthMap[i * 640 + j].Depth;
                    Min = d < Min && d != 0 ? d : Min;
                }
            }
            return Min;

        }

        private HandShapeModel GetSubImageByRectAndDepth(Image<Gray, Byte> depthImage, Image<Gray, Byte> colorImage, List<Rectangle> rects, int depth, DepthImagePoint[] depthMap)
        {

            if (rects == null || rects.Count == 0)
            {
                return null;
            }
            HandShapeModel ret = null;
            if (rects.Count == 1)//all parts occluded
            {
                var rect = rects[0];
                AssertRectangle(ref rect);
                var data = colorImage.Data;
                for (int i = rect.Y; i < rect.Y + rect.Height; i++)
                {
                    for (int j = rect.X; j < rect.X + rect.Width; j++)
                    {
                        data[i, j, 0] = depthMap[i * 640 + j].Depth < depth ? data[i, j, 0] : byte.MinValue;
                    }
                }
                var binaryImg = colorImage.Copy(rect).ThresholdToZeroInv(new Gray(240));
                var clist = FindContourRect(binaryImg, 2,false);
                if (clist.Count == 1) //Intersect touch
                {
                    var srect = clist[0];
                    srect.X += rect.X;
                    srect.Y += rect.Y;
                    AssertRectangle(ref srect);
                    ret = new HandShapeModel(HandEnum.Intersect)//intersect touch
                    {
                        RightColor = GetSubImageByRect(colorImage,srect),
                        RightDepth = GetSubImageByRect(depthImage,srect),
                        right = rightRec,
                        left = leftRec,
                        intersectCenter = srect
                    };
                }
                if (clist.Count == 2)
                {
                    clist = clist.OrderBy(x => x.GetXCenter()).ToList();
                    leftRec = clist[0];
                    rightRec = clist[1];
                    leftRec.X += rect.X;
                    leftRec.Y += rect.Y;
                    rightRec.X += rect.X;
                    rightRec.Y += rect.Y;
                    AssertRectangle(ref leftRec);
                    AssertRectangle(ref rightRec);
                    ret = new HandShapeModel(HandEnum.Both) //Both touch
                    {
                        RightColor = GetSubImageByRect(colorImage, rightRec),
                        RightDepth = GetSubImageByRect(depthImage, rightRec),
                        LeftColor = GetSubImageByRect(colorImage, leftRec),
                        LeftDepth = GetSubImageByRect(depthImage, leftRec),
                        right = rightRec,
                        left = leftRec
                    };
                }
            }
            //intersect or touch
            if (rects.Count == 2)
            {
                rects = rects.OrderBy(x => x.GetCenter().DistanceTo(headRec.GetCenter())).ToList();
                var headTouch = rects[0];
                var untouchRec = rects[1];

                var rect = headTouch;
                AssertRectangle(ref rect);
                AssertRectangle(ref untouchRec);
                var data = colorImage.Data;
                for (int i = rect.Y; i < rect.Y + rect.Height; i++)
                {
                    for (int j = rect.X; j < rect.X + rect.Width; j++)
                    {
                        data[i, j, 0] = depthMap[i*640 + j].Depth < depth ? data[i, j, 0] : byte.MinValue;
                    }
                }
                var binaryImg = colorImage.Copy(rect).ThresholdToZeroInv(new Gray(240));
                var clist = FindContourRect(binaryImg, 1,false);
                binaryImg.Dispose();
                //Intersect untouch
                if (clist.Count == 0)
                {
                    var cdepth = GetRectMinDepth(headTouch, depthMap);
                    if (cdepth < 10000)
                    {
                        if (headMinDepth == 0)
                        {
                            headMinDepth = cdepth;
                        }
                        else
                        {
                            // head hand occluded
                            if (cdepth >= headMinDepth - 50)
                            {
                                headMinDepth = cdepth;
                            }
                        }
                    }
                    ret = new HandShapeModel(HandEnum.Intersect)
                    {
                        RightColor = GetSubImageByRect(colorImage,untouchRec),
                        RightDepth = GetSubImageByRect(depthImage,untouchRec),
                        right = rightRec,
                        left = leftRec,
                        intersectCenter = untouchRec
                    };
                }
                // right or left touch face
                if (clist.Count == 1)
                {
                    var srect = clist[0];
                    srect.X += rect.X;
                    srect.Y += rect.Y;
                    AssertRectangle(ref srect);
                    // right touch
                    if (srect.X > untouchRec.X)
                    {
                        rightRec = srect;
                        leftRec = untouchRec;
                        ret = new HandShapeModel(HandEnum.Both)
                        {
                            RightColor = GetSubImageByRect(colorImage, rightRec),
                            RightDepth = GetSubImageByRect(depthImage, rightRec),
                            LeftColor = GetSubImageByRect(colorImage, leftRec),
                            LeftDepth = GetSubImageByRect(depthImage, leftRec),
                            right = rightRec,
                            left = leftRec
                        };
                    }
                    else //left touch
                    {
                        rightRec = untouchRec;
                        leftRec = srect;
                        ret = new HandShapeModel(HandEnum.Both)
                        {
                            RightColor = GetSubImageByRect(colorImage, rightRec),
                            RightDepth = GetSubImageByRect(depthImage, rightRec),
                            LeftColor = GetSubImageByRect(colorImage, leftRec),
                            LeftDepth = GetSubImageByRect(depthImage, leftRec),
                            right = rightRec,
                            left = leftRec
                        };
                    }
                }
            }
            //Console.WriteLine("x{0} y{1} w{2} h{3}",rect.X,rect.Y,rect.Width,rect.Height);
            return ret;
        }

        private void AssertRectangle(ref Rectangle rect, int width = 640, int height = 480)
        {
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;
            rect.Width = rect.Width + rect.X > width ? width - rect.X : rect.Width;
            rect.Height = rect.Height + rect.Y > height ? height - rect.Y : rect.Height;
        }
        public float[] ResizeAndCalHog(ref Image<Bgr, byte> image)
        {
            Image<Gray, byte> binaryImg = GetBinaryImg(image);
            List<Rectangle> rectList = FindContourRect(binaryImg);
            rectList = rectList.OrderByDescending(x => x.GetRectArea()).ToList();
            if (rectList.Count() >= 1)
            {
                Rectangle r = rectList[0];
                Image<Gray, byte> s_image = (binaryImg.Copy(r) * 255);
                image = ResizeImage<Gray>(s_image, 60, 60).Convert<Bgr, byte>();
                return CalHog(image);

            }
            return null;
        }

        public float[] ResizeAndCalHog(Image<Gray, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            Image<Bgr, byte> bgr = image.Convert<Bgr, byte>();
            return ResizeAndCalHog(ref bgr);
        }

        public float[] CalHog(Image<Gray, byte> image)
        {
            return null;
            if (image == null)
            {
                return null;
            }
            return CalHog(image.Convert<Bgr, byte>());
        }

        public float[] CalHog(Image<Bgr, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return Hog_Descriptor.Compute(image, new Size(1, 1), new Size(0, 0), null);
        }



        public Image<Gray, byte> GetBinaryImg(Image<Bgr, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Convert<Gray, byte>().ThresholdBinaryInv
                (new Gray(200), new Gray(255));
        }

        public Image<Gray, byte> GetBinaryImg(Image<Bgra, byte> image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Convert<Gray, byte>().ThresholdBinaryInv
                (new Gray(200), new Gray(255));
        }


        int minSize = 400;
        int maxSize = 40000;
        private List<MCvBox2D> FindContourMBox(Image<Gray, byte> image, int count)
        {
            Seq<System.Drawing.Point> DyncontourTemp = FindContourSeq(image);
            var rectList = new List<MCvBox2D>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt64() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                //iterate contours
                if (DyncontourTemp.GetMinAreaRect().GetTrueArea() < minSize
                    || DyncontourTemp.GetMinAreaRect().GetTrueArea() > maxSize ||
                    (DyncontourTemp.GetMinAreaRect().center.X > 540 && DyncontourTemp.GetMinAreaRect().center.Y > 380))
                {
                    continue;
                }
                rectList.Add(DyncontourTemp.GetMinAreaRect());
            }

            if (rectList.Count() > count)
            {
                rectList = rectList.OrderByDescending(x => x.MinAreaRect().GetRectArea()).Take(count).ToList();
            }
            return rectList;
        }

        private List<Rectangle> FindContourRect(Image<Gray, byte> image, int count,bool cull = true)
        {
            var DyncontourTemp = FindContour(image);
            var rectList = new List<Rectangle>();
            int cull_length = 120;
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt64() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                var rect = DyncontourTemp.BoundingRectangle;
                //iterate contours
                if (rect.GetRectArea() < minSize
                    || rect.GetRectArea() > maxSize)
                {
                    continue;
                }
                if (cull &&(rect.GetXCenter()>640-cull_length || rect.GetXCenter()<cull_length))
                {
                    continue;
                }
                rectList.Add(rect);
            }
            for (int i = 0; i < rectList.Count; i++)
            {
                for (int j = i+1; j < rectList.Count; j++)
                {
                    if (rectList[i].IsCloseTo(rectList[j]))
                    {
                        rectList[j] = rectList[i].Unite(rectList[j]);
                        rectList.RemoveAt(i);
                        i--;
                        break;
                    }
                } 
            }
            if (rectList.Count() > count)
            {
                rectList = rectList.OrderByDescending(x => x.GetRectArea()).Take(count).ToList();
            }
            if (DyncontourTemp != null) DyncontourTemp.Clear();
            return rectList;
        }



        private List<Rectangle> FindContourRect(Image<Gray, byte> image)
        {
            //Seq<System.Drawing.Point> DyncontourTemp = FindContourSeq(image);
            //var rectList = new List<Rectangle>();
            //for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt64() != 0; DyncontourTemp = DyncontourTemp.HNext)
            //{
            //    //iterate contours
            //    if (DyncontourTemp.BoundingRectangle.GetRectArea() < minSize
            //        || DyncontourTemp.BoundingRectangle.GetRectArea() > maxSize)
            //    {
            //        continue;
            //    }
            //    rectList.Add(DyncontourTemp.BoundingRectangle);
                
            //}
            //rectList = rectList.OrderBy(x => x.GetYCenter()).ToList();
            //DyncontourTemp.Clear()

            var DyncontourTemp = FindContour(image);
            var rectList = new List<Rectangle>();
            for (; DyncontourTemp != null && DyncontourTemp.Ptr.ToInt64() != 0; DyncontourTemp = DyncontourTemp.HNext)
            {
                //iterate contours
                if (DyncontourTemp.BoundingRectangle.GetRectArea() < minSize
                    || DyncontourTemp.BoundingRectangle.GetRectArea() > maxSize)
                {
                    continue;
                }
                rectList.Add(DyncontourTemp.BoundingRectangle);

            }
            rectList = rectList.OrderBy(x => x.GetYCenter()).ToList();
            if (DyncontourTemp != null) DyncontourTemp.Clear();
            return rectList;
        }
        private Contour<Point> FindContour(Image<Gray, byte> image) 
        {
            return image.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL);
        }
        private unsafe Seq<System.Drawing.Point> FindContourSeq(Image<Gray, byte> image)
        {
            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
            IntPtr Dyncontour = new IntPtr();//存放检测到的图像块的首地址
            IntPtr Dynstorage = CvInvoke.cvCreateMemStorage(0);
            int n = CvInvoke.cvFindContours(image.Ptr, Dynstorage, ref Dyncontour, sizeof(MCvContour),
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new System.Drawing.Point(0, 0));
            return new Seq<System.Drawing.Point>(Dyncontour, null);//方便对IntPtr类型进行操作
        }



        private Image<T, Byte> GetSubImage<T>(Image<T, Byte> image, MCvBox2D box, float angle) where T : struct, IColor
        {

            if (box.Equals(MCvBox2D.Empty))
            {
                return null;
            }
            // ensure the low most side being horizontal. 
            // angle is between the horizontal axis and the first side (i.e. width) in degrees
            //if (box.angle < -45)
            //{
            //    box.angle += 90;
            //    float width = box.size.Width;
            //    box.size.Width = box.size.Height;
            //    box.size.Height = width;
            //}

            Image<T, Byte> img = (image.Copy(box) * 255);
            //img = ResizeImage<T>(img, 60, 60);
            return img;

        }

        private Image<T, Byte> ResizeImage<T>(Image<T, Byte> mask, int width, int height) where T : struct, IColor
        {
            try
            {
                int l = mask.Width > mask.Height ? mask.Width : mask.Height;
                Image<T, Byte> result = new Image<T, byte>(l, l);
                Rectangle rect = new Rectangle();
                rect.X = result.Width / 2 - mask.Width / 2;
                rect.Y = result.Height / 2 - mask.Height / 2;
                rect.Width = mask.Width;
                rect.Height = mask.Height;
                result.ROI = rect;
                rect.X = rect.X < 0 ? 0 : rect.X;
                rect.Y = rect.Y < 0 ? 0 : rect.Y;
                CvInvoke.cvResize(mask.Ptr, result.Ptr, Emgu.CV.CvEnum.INTER.CV_INTER_NN);
                result.ROI = Rectangle.Empty;
                result = result.Resize(width, height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                return result;

            }
            catch (Exception)
            {
                return null;
            }

        }
        private Image<T, Byte> GetBinarySubImageByRect<T>(Image<T, Byte> image, Rectangle rect) where T : struct, IColor
        {

            if (rect == null)
            {
                return null;
            }
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;
            Image<T, Byte> result = (image.Copy(rect) * 255);
            //result = result.Resize(60, 60, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            return result;

        }

        private Image<Rgb , Byte> GetSubImageByRect<T>(Image<T, Byte> image, Rectangle rect) where T : struct, IColor
        {

            if (rect == null)
            {
                return null;
            }
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;
            rect.Width = rect.Width + rect.X > image.Width ? image.Width - rect.X : rect.Width;
            rect.Height = rect.Height + rect.Y > image.Height ? image.Height - rect.Y : rect.Height;
            //Console.WriteLine("x{0} y{1} w{2} h{3}",rect.X,rect.Y,rect.Width,rect.Height);
            Image<Rgb, Byte> result = image.Copy(rect).Convert<Rgb, byte>();
            return result;

        }
        /// <summary>
        /// 将IplImage*转换为Bitmap（注：在OpenCV中IplImage* 对应EmguCV的IntPtr类型）       
        /// </summary>
        /// <param name="ptrImage"></param>
        /// <returns>Bitmap对象</returns>
        public static Image<T, byte> ConvertIntPrToBitmap<T>(IntPtr ptrImage) where T : struct,IColor
        {
            //将IplImage指针转换成MIplImage结构
            MIplImage mi = (MIplImage)Marshal.PtrToStructure(ptrImage, typeof(MIplImage));

            Image<T, byte> image = new Image<T, byte>(mi.width, mi.height, mi.widthStep, mi.imageData);
            return image;
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

        private void DrawHand(MCvBox2D rect, Image<Bgra, Byte> image, HandEnum handEnum)
        {
            System.Drawing.Point[] points = rect.GetVertices().Select(x => x.ToPoint()).ToArray();
            DrawPoly(points, image, new MCvScalar(0, 0, 255));
            Point center = rect.center.ToPoint();
            DrawPoint(image, center, new MCvScalar(255, 0, 0));

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

        private void DrawPoly(System.Drawing.Point[] points, IntPtr image, MCvScalar color)
        {

            if (points == null || points.Length <= 0)
            {
                return;
            }
            for (int j = 0; j < points.Length; j++)
            {
                CvInvoke.cvLine(image, points[j], points[(j + 1) % points.Length], color, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
            }
        }

        private void DrawPoint(Image<Bgra, Byte> image, Point point, MCvScalar color)
        {
            if (point == null || point == Point.Empty)
            {
                return;
            }
            CvInvoke.cvCircle(image, point, 3, color, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
        }


        int begin = 35;
        int end = 70;
        int minLength = 50;

        private MCvBox2D SplitHand(MCvBox2D rect, HandEnum handEnum, PointF Elbow2HandVector)
        {

            if (handEnum == HandEnum.Both || handEnum == HandEnum.Intersect)
            {
                return new MCvBox2D(rect.center, rect.MinAreaRect().Size, 0);
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

            SizeF size = new SizeF();
            size.Width = shortP.DistanceTo(startP);
            size.Height = longP.DistanceTo(startP);
            PointF shortEdge = new PointF(shortP.X - startP.X, shortP.Y - startP.Y);
            float t_angle = (float)(Math.Atan(shortEdge.Tan()) * 180 / Math.PI);

            MCvBox2D box = new MCvBox2D();
            box.size = size;
            box.center = pl.GetCenter();

            float longDis = longP.DistanceTo(startP);
            float shortDis = shortP.DistanceTo(startP);
            // x and long edge slope 
            float longslope = Math.Abs(longP.X - startP.X) / longDis;
            float min = float.MaxValue;

            float factor = Math.Max(Math.Abs(longP.Y - startP.Y) / longDis, Math.Abs(longP.X - startP.X) / longDis);
            int TransformEnd = Convert.ToInt32(factor * end);
            int TransformBegin = Convert.ToInt32(factor * begin);

            // > 45
            if (longslope < 0.707)//vert
            {
                // point up
                if (Elbow2HandVector.Y <= 0)
                {
                    box.angle = t_angle;
                    startP = pl.OrderBy((x => x.Y)).First();
                }
                else
                {
                    box.angle = t_angle + 180;
                    startP = pl.OrderByDescending((x => x.Y)).First();
                }


                if (longDis < minLength)
                {
                    return box;
                }

                pl = pl.OrderBy(x => x.DistanceTo(startP)).ToArray();
                shortP = pl[1];
                longP = pl[2];

                for (int y = TransformBegin; y < Convert.ToInt32(Math.Abs(longP.Y - startP.Y)) && Math.Abs(y) < TransformEnd; y++)
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
                // point top for right hand
                if (t_angle < 0)
                {
                    if (handEnum == HandEnum.Right)
                    {
                        box.angle = t_angle;
                    }
                    else
                    {
                        box.angle = t_angle + 180;
                    }
                }
                // point bottom for right hand
                else
                {
                    if (handEnum == HandEnum.Right)
                    {
                        box.angle = t_angle - 180;
                    }
                    else
                    {
                        box.angle = t_angle;
                    }
                }
                if (handEnum == HandEnum.Right)
                {
                    startP = pl.OrderBy((x => x.X)).ToArray()[0];

                }
                else if (handEnum == HandEnum.Left)
                {
                    startP = pl.OrderByDescending((x => x.X)).ToArray()[0];
                }


                if (longDis < minLength)
                {
                    return box;
                }
                pl = pl.OrderBy(x => x.DistanceTo(startP)).ToArray();
                shortP = pl[1];
                longP = pl[2];
                for (int X = TransformBegin; X < Convert.ToInt32(Math.Abs(longP.X - startP.X)) && Math.Abs(X) < TransformEnd; X++)
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
                return box;
            }
            splittedHands[0] = startP.ToPoint();
            splittedHands[1] = ap1.ToPoint();
            splittedHands[2] = ap2.ToPoint();
            splittedHands[3] = shortP.ToPoint();


            //Point lowP = p.OrderByDescending(x => x.Y).First();
            //Point highP = p.OrderByDescending(x => x.DistanceTo(lowP)).First();
            //Point widthP = p.OrderBy(x => x.TanWith(lowP)).First();
            box.center = splittedHands.GetCenter();
            box.size.Height = startP.DistanceTo(ap1);

            return box;
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

        public void Reset()
        {
            headRec = new Rectangle(new Point(320, 0), new Size());
            rightRec = new Rectangle(new Point(640, 480), new Size());
            leftRec = new Rectangle(new Point(0, 480), new Size());
            headMinDepth = 0;
            Intersect = false;
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