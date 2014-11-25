using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using CURELab.SignLanguage.HandDetector.Annotations;
using CURELab.SignLanguage.HandDetector.Model;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Kinect;
using Skeleton = Microsoft.Kinect.Skeleton;

namespace CURELab.SignLanguage.HandDetector
{
    internal class VideoProcessor : INotifyPropertyChanged
    {
        public static double DIFF = 10;
        private static VideoProcessor m_VideoProcessor;
        private OpenCVController m_opencv;
        private ImageViewer viewer;
        private CsvFileWriter labelWriter;
        private System.Drawing.Point rightHandPosition;
        private System.Drawing.Point headPosition;

        public static double CullingThresh;
        public static float AngleRotateTan = AaronRotateTan;
        // demo
        public const float DemoRotateTan = 0.45f;
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.28f;

        private const int handShapeWidth = 60;
        private const int handShapeHeight = 60;
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;
        private List<MySkeleton> sktList;
        Capture _CCapture;
        Capture _DCapture;
        private Image<Bgr, byte> CurrentImageC;
        private Image<Bgr, byte> CurrentImageD;

        public double FrameRate = 0;
        int FrameCount;

        private WriteableBitmap colorWriteBitmap;

        public WriteableBitmap ColorWriteBitmap
        {
            get { return colorWriteBitmap; }
            protected set { colorWriteBitmap = value; }
        }

        private WriteableBitmap depthWriteBitmap;

        public WriteableBitmap DepthWriteBitmap
        {
            get { return depthWriteBitmap; }
            protected set { depthWriteBitmap = value; }
        }

        private WriteableBitmap processedDepthBitmap;

        public WriteableBitmap ProcessedDepthBitmap
        {
            get { return processedDepthBitmap; }
            protected set { processedDepthBitmap = value; }
        }

        private WriteableBitmap grayWriteBitmap;

        public WriteableBitmap GrayWriteBitmap
        {
            get { return grayWriteBitmap; }
            protected set { grayWriteBitmap = value; }
        }

        public WriteableBitmap WrtBMP_RightHandFront { get; set; }
        public WriteableBitmap WrtBMP_LeftHandFront { get; set; }
        public WriteableBitmap WrtBMP_Candidate2 { get; set; }
        public WriteableBitmap WrtBMP_Candidate3 { get; set; }
        public WriteableBitmap WrtBMP_Candidate1 { get; set; }
        public WriteableBitmap WrtBMP_RightHandSide { get; set; }
        public WriteableBitmap WrtBMP_LeftHandSide { get; set; }

        private VisualData vs;
        public int CurrentFrame
        {
            get { return vs.CurrentFrame; }
            set
            {
                vs.CurrentFrame = value;
            }
        }

        public int TotalFrames
        {
            get { return vs.TotalFrames; }
            set
            {
                vs.TotalFrames = value;
            }
        }

        protected VideoProcessor()
        {
            m_opencv = OpenCVController.GetSingletonInstance();
            viewer = new ImageViewer();
            vs = VisualData.GetSingleton();
            viewer.Show();
            this.ColorWriteBitmap = new WriteableBitmap(FrameWidth, FrameHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Bgr24, null);
            this.DepthWriteBitmap = new WriteableBitmap(FrameWidth, FrameHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Bgr24, null);
            this.WrtBMP_RightHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Gray8, null);
            this.WrtBMP_LeftHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Gray8, null);

            rightHandPosition = new System.Drawing.Point();
        }

        public static VideoProcessor GetSingletonInstance()
        {
            return m_VideoProcessor ?? (m_VideoProcessor = new VideoProcessor());
        }

        public enum StreamType
        {
            Color,
            Depth
        }

        public void OpenDir(string path)
        {
            // color stream
            string tpath = path+"\\" + path.Split('\\').Last();
            var c = new Capture(tpath + "_c.avi");
            if (_CCapture != null)
            {
                _CCapture.Dispose();//dispose of current capture
            }
            _CCapture = c;
            // depth stream
            c = new Capture(tpath + "_d.avi");
            if (_DCapture != null)
            {
                _DCapture.Dispose();//dispose of current capture
            }
            _DCapture = c;

            FrameRate = _DCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
            TotalFrames = (int)_DCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
            Console.WriteLine("Total frame:"+TotalFrames.ToString());
            double codec_double = c.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FOURCC);
            CurrentFrame = 0;
            // skeleton
            OpenSkeleton(tpath + ".csv");
            // label
            if (labelWriter != null)
            {
                labelWriter.Close();
                labelWriter.Dispose();
            }
            labelWriter = new CsvFileWriter(path+"\\label.csv");
        }



        public void OpenSkeleton(string path)
        {
            using (CsvFileReader reader = new CsvFileReader(path))
            {
                sktList = new List<MySkeleton>();
                CsvRow row = new CsvRow();
                // one frame skeleton
                while (reader.ReadRow(row))
                {
                    var skt = new MySkeleton(14);
                    if (row[0].ToLower().Trim().Equals("untracked"))
                    {
                        skt.Tracked = false;
                    }
                    else
                    {
                        int step = 7;
                        // one joint
                        for (int i = 0; i < row.Count; i += step)
                        {
                            var type = i / step;
                            var joint = new MyJoint()
                            {
                                Pos3D = new SkeletonPoint()
                                {
                                    X = Convert.ToSingle(row[i]),
                                    Y = Convert.ToSingle(row[i + 1]),
                                    Z = Convert.ToSingle(row[i + 2])
                                },
                                PosColor = new System.Drawing.Point()
                                {
                                    X = (int)Convert.ToSingle(row[i + 3]),
                                    Y = (int)Convert.ToSingle(row[i + 4])
                                },
                                PosDepth = new System.Drawing.Point()
                                {
                                    X = (int)Convert.ToSingle(row[i + 5]),
                                    Y = (int)Convert.ToSingle(row[i + 6])
                                }
                            };
                            skt[type] = joint;
                        }
                    }
                    sktList.Add(skt);
                }
            }
            //foreach (var mySkeleton in sktList)
            //{
            //    Console.Write(mySkeleton[MyJointType.Head].PosColor.X);
            //}
        }



        private int headDepth;
        public bool ProcessFrame()
        {
            if (CurrentFrame >= TotalFrames -1)
            {
                return false;
            }
            if (_CCapture != null)
            {
                var r = _CCapture.Grab();
                var imgc = _CCapture.RetrieveBgrFrame();
                ImageConverter.UpdateWriteBMP(colorWriteBitmap, imgc.ToBitmap());

            }
            if (_DCapture != null)
            {
                var r = _DCapture.Grab();
                var img = _DCapture.RetrieveBgrFrame();
                
                int framenumber = (int)_DCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                Console.WriteLine("frame:"+framenumber.ToString());
                CurrentFrame = framenumber - 1;
                if (img == null)
                {
                    return false;
                }
                //Show time stamp
                double time_index = _DCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_MSEC);

                PointF rightVector = new PointF(-10, -10);
                PointF leftVector = new PointF(10, -10);
                bool isSkip = false;
                bool leftHandRaise = false;
                if (CurrentFrame >= sktList.Count || !sktList[CurrentFrame].Tracked)
                {
                    // no skeleton detected
                    headDepth = 0;
                }
                else
                {
                    headPosition = sktList[CurrentFrame][MyJointType.Head].PosDepth;
                    headDepth = Math.Min((int)img[headPosition.Y, headPosition.X].Green,(int)img[headPosition.Y, headPosition.X].Red);
                    headDepth = Math.Min(headDepth, (int) img[headPosition.Y, headPosition.X].Blue);
                    PointF hr = sktList[CurrentFrame][MyJointType.HandR].PosDepth;
                    PointF hl = sktList[CurrentFrame][MyJointType.HandL].PosDepth;
                    PointF er = sktList[CurrentFrame][MyJointType.ElbowR].PosDepth;
                    PointF el = sktList[CurrentFrame][MyJointType.ElbowL].PosDepth;
                    PointF hip = sktList[CurrentFrame][MyJointType.HipCenter].PosDepth;
                    // hand is lower than hip
                    //Console.WriteLine(sktList[CurrentFrame][MyJointType.HandR].Pos3D.Y);
                    //Console.WriteLine(sktList[CurrentFrame][MyJointType.HipCenter].Pos3D.Y);
                    //Console.WriteLine("-------------");
                    if (sktList[CurrentFrame][MyJointType.HandR].Pos3D.Y <
                        sktList[CurrentFrame][MyJointType.HipCenter].Pos3D.Y + 0.05)
                    {
                        isSkip = true;
                    }
                    if (sktList[CurrentFrame][MyJointType.HandL].Pos3D.Y >
                        sktList[CurrentFrame][MyJointType.HipCenter].Pos3D.Y)
                    {
                        leftHandRaise = true;
                    }

                    rightVector.X = (hr.X - er.X);
                    rightVector.Y = (hr.Y - er.Y);
                    leftVector.X = (hl.X - el.X);
                    leftVector.Y = (hl.Y - el.Y);
                }
               

                #region temp

                //isSkip = false;
                //leftHandRaise = false;
                //rightVector = new PointF(-10, -10);
                //leftVector = new PointF(10, -10);
                //headDepth = (int)img[85, 315].Blue;
                #endregion
               // Console.WriteLine("headdepth："+headDepth.ToString());

                //***********cull image*****************
                double cull = headDepth - CullingThresh;
                var depthImg = img.ThresholdToZeroInv(new Bgr(cull, cull, cull));
                //Image<Gray, Byte> depthImg = img.Convert<Gray, byte>().ThresholdBinary(new Gray(160), new Gray(255));
                var sw = Stopwatch.StartNew();
                int handDepth = (int)(2600.0 / 255 * cull + 400);
                HandShapeModel handModel = null;
                Image<Gray, Byte> rightFront = null;
                Image<Gray, Byte> leftFront = null;
                // isskip is invalid coz no hip data
                isSkip = false;
                if (!isSkip && cull> 0)
                {
                    handModel = m_opencv.FindHandPart(ref depthImg, out rightFront, out leftFront, handDepth, rightVector, leftVector, leftHandRaise);
                }
                viewer.Image = depthImg;


                // no hands detected
                if (handModel == null)
                {
                    handModel = new HandShapeModel(0, HandEnum.None);
                }
                var row = new CsvRow();
                row.Add(CurrentFrame.ToString());
                row.Add(handModel.type.ToString());
                switch (handModel.type)
                {
                    case HandEnum.Intersect:
                    case HandEnum.Right:
                        row.Add(handModel.handPosRight.center.X.ToString());
                        row.Add(handModel.handPosRight.center.Y.ToString());
                        row.Add(handModel.handPosRight.size.Width.ToString());
                        row.Add(handModel.handPosRight.size.Height.ToString());
                        row.Add(handModel.handPosRight.angle.ToString());
                        break;
                    case HandEnum.Both:
                        row.Add(handModel.handPosRight.center.X.ToString());
                        row.Add(handModel.handPosRight.center.Y.ToString());
                        row.Add(handModel.handPosRight.size.Width.ToString());
                        row.Add(handModel.handPosRight.size.Height.ToString());
                        row.Add(handModel.handPosRight.angle.ToString());
                        row.Add(handModel.handPosLeft.center.X.ToString());
                        row.Add(handModel.handPosLeft.center.Y.ToString());
                        row.Add(handModel.handPosLeft.size.Width.ToString());
                        row.Add(handModel.handPosLeft.size.Height.ToString());
                        row.Add(handModel.handPosLeft.angle.ToString());
                        break;
                }
                row.Add(cull.ToString());
                labelWriter.WriteRow(row);
                if (rightFront != null)
                {
                    ImageConverter.UpdateWriteBMP(WrtBMP_RightHandFront, rightFront.ToBitmap());
                }
                

                ImageConverter.UpdateWriteBMP(depthWriteBitmap, img.ToBitmap());
                
                if (CurrentFrame < TotalFrames -1)
                {
                    return true;
                }
            }
            return false;
        }


        public void ProcessSample()
        {
            CurrentFrame = 0;
            while (ProcessFrame())
            {

            }
            _CCapture.Dispose();
            _DCapture.Dispose(); 
            labelWriter.Close();
            labelWriter.Dispose();
        }

        public void SetCurrentFrame(int index)
        {
            if (index == CurrentFrame)
            {
                return;
            }
            if (_DCapture != null)
            {
                _DCapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, index);
            }
            if (_CCapture != null)
            {
                _CCapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, index);
            }
            CurrentFrame = index;
            ProcessFrame();
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
