// author：      Administrator
// created time：2014/1/14 15:59:58
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System.Runtime.Remoting.Messaging;
using System.Windows.Threading;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

using CURELab.SignLanguage.StaticTools;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

using CURELab.SignLanguage.HandDetector;
using System.Diagnostics;

namespace CURELab.SignLanguage.HandDetector
{

    /// <summary>
    /// Kinect SDK controller
    /// </summary>
    public class KinectSDKController : KinectController
    {

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        public static KinectSensor sensor;

        private SocketManager socket;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        protected DepthImagePixel[] depthImagePixels;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        protected byte[] colorPixels;
        protected byte[] depthPixels;

        protected Colorizer colorizer;

        protected Skeleton skeleton;

        private System.Drawing.Point rightHandPosition;
        protected System.Drawing.Point headPosition;

        public static double CullingThresh;
        public static float AngleRotateTan = MichaelRotateTan;
        // demo
        public const float DemoRotateTan = 0.45f;
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.32f;

        const int handShapeWidth = 60;
        const int handShapeHeight = 60;
        protected int VideoFrame;

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        protected const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        protected const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
        protected KinectSDKController()
            : base()
        {
            KinectSensor.KinectSensors.StatusChanged += Kinect_StatusChanged;
        }

        private bool isRecording;
        public bool IsRecording
        {
            get { return isRecording; }
            set
            {
                isRecording = value;
                OnPropertyChanged("IsRecording");
            }
        }

        public static KinectController GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectSDKController();
            }
            return singleInstance;
        }

        public override void Initialize(string uri = null)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    break;
                }
            }


            if (null != sensor)
            {
                // Turn on the color stream to receive color frames
                sensor.ColorStream.Enable(ColorFormat);
                sensor.DepthStream.Enable(DepthFormat);
                sensor.SkeletonStream.Enable();
                //sensor.DepthStream.Range = DepthRange.Near;
                // Allocate space to put the pixels we'll receive           
                this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new byte[sensor.DepthStream.FramePixelDataLength];
                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                _mappedColorLocations = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                _mappedDepthLocations = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                // This is the bitmap we'll display on-screen
                this.ColorWriteBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
                this.DepthWriteBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
                this.WrtBMP_RightHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
                this.WrtBMP_LeftHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
                WrtBMP_Candidate1 = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
                WrtBMP_Candidate2 = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
                WrtBMP_Candidate3 = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
                // Add an event handler to be called whenever there is new frame data
                this.Status = Properties.Resources.Connected;

                this.colorizer = new Colorizer(AngleRotateTan,800,3000);
                rightHandPosition = new System.Drawing.Point();
                headPosition = new Point(320,0);
                headDepth = 1500;
                sensor.Start();
                rightFirst = Rectangle.Empty;
                leftFirst = Rectangle.Empty;
            }

            if (null == sensor)
            {
                this.Status = Properties.Resources.NoKinectReady;
            }

        }


        private void Kinect_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (sensor == null)
                    {
                        sensor = e.Sensor;
                        Initialize();
                        Start();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (sensor == e.Sensor)
                    {

                        this.Status = Properties.Resources.NoKinectReady;
                        // Notify user, change state of APP appropriately
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (sensor == e.Sensor)
                    {
                        this.Status = Properties.Resources.NoKinectReady;
                        // Notify user, change state of APP appropriately
                    }
                    break;
                default:
                    // Throw exception, notify user or ignore depending on use case
                    break;
            }
        }

        private void ProcessOneFrame()
        {
                        
        }
        protected short headDepth = 0;
        protected int frame = 0;
        protected ColorImagePoint[] _mappedColorLocations;
        protected DepthImagePoint[] _mappedDepthLocations;
        private Rectangle rightFirst;
        private Rectangle leftFirst;
        private bool headTracked;
        protected virtual void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            //Console.Clear();
            //headPosition = new Point(320,0);
            headTracked = false;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    //Console.WriteLine("ske:{0}", skeletonFrame.Timestamp);
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    var s = skeletons.Where(x => x.TrackingState == SkeletonTrackingState.Tracked).
                        OrderBy(x => x.Position.Z);
                    if (s.Count()>0)
                    {
                        var skel = s.First();
                        SkeletonPoint head = skel.Joints[JointType.Head].Position;
                        headPosition = SkeletonPointToScreen(head);

                        if (skel.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked && headPosition.X != 0)
                        {
                            headTracked = true;
                        }

                        skeleton = skel;
                    }
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    //Console.WriteLine("col:{0}", colorFrame.Timestamp);

                    // Write the pixel data into our bitmap
                    this.ColorWriteBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
                        this.colorPixels,
                        this.ColorWriteBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    var sw = Stopwatch.StartNew();
                    // Copy the pixel data from the image to a temporary array
                    //Console.WriteLine("dep:{0}", depthFrame.Timestamp);
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);
                    _mappedColorLocations = new ColorImagePoint[depthFrame.PixelDataLength];
                    sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    this.depthImagePixels,
                    ColorFormat,
                    _mappedColorLocations);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;

                    if (headTracked)
                    {
                        try
                        {
                            headDepth = depthImagePixels[headPosition.X + headPosition.Y * 640].Depth;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine(headPosition.X);
                            Console.WriteLine(headPosition.Y);
                            Console.WriteLine(headPosition.X + headPosition.Y * 640);
                            return;
                        }
                    }
                    

                    //Console.WriteLine("mapping:{0}", sw.ElapsedMilliseconds);
                    //sw.Restart();
                    //*********** Convert cull and transform*****************
                    Array.Clear(depthPixels, 0, depthPixels.Length);
                    //colorizer.TransformAndConvertDepthFrame(depthImagePixels, depthPixels, _mappedColorLocations);
                    //Console.WriteLine("convert:{0}", sw.ElapsedMilliseconds);
                    //sw.Restart();
                    
                    // stream registration
                    var depthImg = ImageConverter.Array2Image<Gray>(depthPixels, width, height, width);
                    sensor.CoordinateMapper.MapColorFrameToDepthFrame(
                                 ColorFormat,DepthFormat,
                                 this.depthImagePixels,
                                 this._mappedDepthLocations);


                    PointF rightVector = PointF.Empty;
                    PointF leftVector = PointF.Empty;
                    bool isSkip = true;
                    bool leftHandRaise = false;
                    var handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth);
                    if (handModel == null)
                    {
                        handModel = new HandShapeModel(HandEnum.None);
                    }
                    //Console.WriteLine("recog:{0}", sw.ElapsedMilliseconds);
                    if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked && handModel.type != HandEnum.None)
                    {
                        PointF hr = SkeletonPointToScreen(skeleton.Joints[JointType.HandRight].Position);
                        PointF hl = SkeletonPointToScreen(skeleton.Joints[JointType.HandLeft].Position);
                        PointF er = SkeletonPointToScreen(skeleton.Joints[JointType.ElbowRight].Position);
                        PointF el = SkeletonPointToScreen(skeleton.Joints[JointType.ElbowLeft].Position);
                        PointF hip = SkeletonPointToScreen(skeleton.Joints[JointType.HipCenter].Position);
                        // hand is lower than hip
                        //Console.WriteLine(skeleton.Joints[JointType.HandRight].Position.Y);
                        //Console.WriteLine(skeleton.Joints[JointType.HipCenter].Position.Y);
                        //Console.WriteLine("-------------");
                        if (handModel.right.GetYCenter()<
                            hip.Y + 50)
                        {
                            isSkip = false;
                        }
                        if (skeleton.Joints[JointType.HandLeft].Position.Y >
                            skeleton.Joints[JointType.HipCenter].Position.Y -0.12)
                        {
                            leftHandRaise = true;
                        }

                        //Console.WriteLine(skeleton.Joints[JointType.HandRight].Position.Y);
                       
                        rightVector.X = (hr.X - er.X);
                        rightVector.Y = (hr.Y - er.Y);
                        leftVector.X = (hl.X - el.X);
                        leftVector.Y = (hl.Y - el.Y);
                    }
                    //start recording
                    if (!IsRecording && !isSkip)
                    {
                        Console.WriteLine("RECORDING");
                        currentPath = path + frame.ToString();
                        System.IO.Directory.CreateDirectory(currentPath);
                        IsRecording = true;
                    }
                    //stop recording
                    if (IsRecording && isSkip)
                    {
                        Console.WriteLine("END");
                        if (socket != null)
                        {
                            socket.SendEndAsync();
                        }
                        IsRecording = false;
                    }

                   

                    //if (skeleton != null)
                    if (!isSkip && skeleton != null)
                    {

                        handModel.skeletonData = FrameConverter.GetFrameDataArgString(skeleton);
                        if (handModel.intersectCenter != Rectangle.Empty
                                && !leftHandRaise)
                        {
                            //false intersect right hand behind head and left hand on initial position
                            // to overcome the problem of right hand lost and left hand recognized as intersected.
                        }
                        else
                        {
                            if (!leftHandRaise && handModel.type == HandEnum.Both)
                            {
                                handModel.type = HandEnum.Right;
                            }
                            socket = SocketManager.GetInstance();
                            if (socket != null)
                            {
                                socket.SendDataAsync(handModel);
                            }
                            Console.WriteLine(handModel.type);
                            
                            var colorRight= handModel.RightColor;
                            string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                currentPath, frame.ToString(), 'C');
                            colorRight.Save(fileName);

                            //var depthRight = handModel.RightDepth;
                            //fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                            //    currentPath, frame.ToString(), handModel.type, 'D');
                            //depthRight.Save(fileName);
                            
                            //frame++;
                            //ImageConverter.UpdateWriteBMP(WrtBMP_RightHandFront, right);
                        }
                        
                       
                    }
                   
                    //*******************upadte UI
                    this.DepthWriteBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                        this.colorPixels,
                        this.DepthWriteBitmap.PixelWidth * sizeof(int),
                        0);
                    //ImageConverter.UpdateWriteBMP(DepthWriteBitmap, depthImg.ToBitmap());
                    // Console.WriteLine("Update UI:" + sw.ElapsedMilliseconds);

                }
            }
        }
        private string path = @"D:\handimages\";
        private string currentPath = @"D:\handimages\";

        public delegate string SendHandler(HandShapeModel model);

        private void GetResponseCallback(IAsyncResult result)
        {
            Console.WriteLine(result.IsCompleted);
            var handler = (SocketManager.AsyncMsgCaller)((AsyncResult)result).AsyncDelegate;
            Console.WriteLine(handler.EndInvoke(result));
        }
        private void GetResponseImageCallback(IAsyncResult result)
        {
            Console.WriteLine(result.IsCompleted);
            var handler = (SocketManager.AsyncBitmapCaller)((AsyncResult)result).AsyncDelegate;
            Console.WriteLine(handler.EndInvoke(result));
        }



        public override void Run()
        {


        }
        public override void Stop()
        {

        }



        protected System.Drawing.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }


        static float KinectFOV = 0.8378f;
        float TanTiltAngle = 0;
        private float CalTiltAngle(int y, int upperDepth, int lowerPartDepth)
        {
            if (lowerPartDepth == 0)
            {
                TanTiltAngle = 0;
                return 0;
            }
            int depthDiff = upperDepth - lowerPartDepth;
            float longEdge = (float)Math.Tan(KinectFOV / 2) *
                 (headDepth * (float)(240 - y) / 240 + upperDepth - depthDiff);
            float shortEdge = depthDiff;
            TanTiltAngle = shortEdge / longEdge;
            TanTiltAngle = TanTiltAngle > 1 ? 0 : TanTiltAngle;
            double angle = Math.Atan(TanTiltAngle);
            return (float)angle;

        }

        protected int GetRealCurrentFrame(long tsOffset)
        {
            return (int)Math.Round(Convert.ToDouble(tsOffset) / 33.3);
        }



        public override void Shutdown()
        {
            if (null != sensor)
            {
                sensor.Stop();
            }
        }

        public override void Start()
        {
            try
            {
                if (sensor != null)
                {
                    sensor.AllFramesReady += AllFrameReady;
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Reset()
        {
            base.Reset();
            headDepth = 100;
            headPosition = new Point();
        }



    }
}