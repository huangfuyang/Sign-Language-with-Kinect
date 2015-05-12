using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using CURELab.SignLanguage.StaticTools;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace CURELab.SignLanguage.HandDetector
{
    class KinectRealtime : KinectSDKController
    {
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        private SocketManager socket;
        private MainUI mwWindow;
        private bool IsInitialized = false;
        private KinectRealtime(MainUI mw)
            : base()
        {
            try
            {
                //socket = SocketManager.GetInstance("127.0.0.1", 51243);
                var socket = SocketManager.GetInstance("137.189.89.29", 51243);
                ////socket = SocketManager.GetInstance("192.168.209.67", 51243);
                this.socket = socket;
                AsnycDataRecieved();
                ShowFinal = true;
                IsInitialized = false;
            }
            catch (Exception)
            {
                Console.WriteLine("not connected");
                
            }
          
            mwWindow = mw;
        }

        public static KinectController GetSingletonInstance(MainUI mw)
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectRealtime(mw);
            }
            return singleInstance;
        }

        protected override void ChooseSkeleton(Skeleton[] skeletons)
        {
            base.ChooseSkeleton(skeletons);
            //backgroundRemovedColorStream.SetTrackedPlayer(currentlyTrackedSkeletonId);
        }

        public override void ChangeSensor(KinectSensor _sensor)
        {
            base.ChangeSensor(_sensor);
            if (IsInitialized)
            {
                sensor.AllFramesReady += AllFrameReady;
            }
        }

        public override void Initialize(KinectSensor _sensor)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            sensor = _sensor;


            if (null != sensor && !IsInitialized)
            {
                // Turn on the color stream to receive color frames
                //this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(sensor);
                //this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);
                //this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;
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
                // Add an event handler to be called whenever there is new frame data
                this.Status = Properties.Resources.Connected;

                this.colorizer = new Colorizer(AngleRotateTan, 800, 3000);
                headPosition = new Point(320, 0);
                headDepth = 800;
                IsInitialized = true;
            }

            if (null == sensor)
            {
                this.Status = Properties.Resources.NoKinectReady;
            }

        }

        protected override void AllFrameReady(object sender, AllFramesReadyEventArgs e)
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
                    //this.backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);
                    ChooseSkeleton(skeletons);
                    if (currentSkeleton != null)
                    {
                        if (currentSkeleton.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked)
                        {
                            headTracked = true;
                            SkeletonPoint head = currentSkeleton.Joints[JointType.Head].Position;
                            headPosition = SkeletonPointToScreen(head);
                        }
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

                    //this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
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
                    socket = SocketManager.GetInstance();
                    //this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    var sw = Stopwatch.StartNew();
                    // Copy the pixel data from the image to a temporary array
                    //Console.WriteLine("dep:{0}", depthFrame.Timestamp);
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);
                    //_mappedColorLocations = new ColorImagePoint[depthFrame.PixelDataLength];
                    //sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    //DepthFormat,
                    //this.depthImagePixels,
                    //ColorFormat,
                    //_mappedColorLocations);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;

                    //Console.WriteLine("Frame {0} Time {1}",depthFrame.FrameNumber,depthFrame.Timestamp);
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
                                 ColorFormat, DepthFormat,
                                 this.depthImagePixels,
                                 this._mappedDepthLocations);


                    bool rightHandRaise = false;
                    bool leftHandRaise = false;

                    //Console.WriteLine("recog:{0}", sw.ElapsedMilliseconds);
                    if (currentSkeleton != null)
                    {
                        // hand is lower than hip
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandLeft].Position.Y);
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HipCenter].Position.Y);
                        //Console.WriteLine("-------------");
                        if (currentSkeleton.Joints[JointType.HandRight].Position.Y >
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        //if (handModel.right.GetYCenter() < hip.Y + 50 || (handModel.intersectCenter != Rectangle.Empty && handModel.intersectCenter.Y < hip.Y + 50))
                        {
                            rightHandRaise = true;
                        }
                        if (currentSkeleton.Joints[JointType.HandLeft].Position.Y >
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            leftHandRaise = true;
                        }

                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandRight].Position.Y);


                    }
                    byte[] processImg;
                    HandShapeModel handModel = null;
                    handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth, out processImg, 4);
                    if (handModel == null)
                    {
                        handModel = new HandShapeModel(HandEnum.None);
                    }

                    //start recording
                    if (!IsRecording && rightHandRaise)
                    {
                        Console.WriteLine("RECORDING");
                        IsRecording = true;
                        mwWindow.lbl_candidate1.Content = "錄製中";
                        mwWindow.lbl_candidate1.Foreground = Brushes.Red;
                    }
                    //stop recording
                    if (IsRecording && handModel.type != HandEnum.None && !rightHandRaise && !leftHandRaise)
                    {
                        Console.WriteLine("END");
                        if (socket != null)
                        {
                            socket.SendEndAsync();
                        }
                        IsRecording = false;
                        mwWindow.lbl_candidate1.Content = "等待結果";
                        mwWindow.lbl_candidate1.Foreground = Brushes.Black;
                    }



                    //if (currentSkeleton != null)
                    if (IsRecording)
                    {

                        handModel.skeletonData = FrameConverter.GetFrameDataArgString(currentSkeleton);
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
                            if (socket != null)
                            {
                                socket.SendDataAsync(handModel);
                            }
                            Console.WriteLine(handModel.type);
                        }


                    }

                    //*******************upadte UI
                    if (ShowFinal)
                    {
                        this.DepthWriteBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                        processImg,
                        this.DepthWriteBitmap.PixelWidth * sizeof(int),
                        0);
                    }
                    else
                    {
                        this.DepthWriteBitmap.WritePixels(
                      new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                      colorPixels,
                      this.DepthWriteBitmap.PixelWidth * sizeof(int),
                      0);
                    }
                    
                    //ImageConverter.UpdateWriteBMP(DepthWriteBitmap, depthImg.ToBitmap());
                    // Console.WriteLine("Update UI:" + sw.ElapsedMilliseconds);

                }
            }

        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    backgroundRemovedFrame.CopyPixelDataTo(colorPixels);
                    // Write the pixel data into our bitmap
                    this.ColorWriteBitmap.WritePixels(
                        new Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
                        colorPixels,
                        this.ColorWriteBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        private void AsnycDataRecieved()
        {
            var t = new Thread(new ThreadStart(DataRecieved));
            t.Start();
        }
        private string[] SPLIT =  { "#TERMINATOR#" };
        private void DataRecieved()
        {
            if (socket != null)
            {
                Console.WriteLine("waiting reponse");

                while (true)
                {
                    try
                    {
                        var r = socket.GetResponse();
                        if (r == null)
                        {
                            Console.WriteLine("finish receive");
                            break;
                        }
                        r = r.Trim();
                        var list = r.Split(SPLIT,StringSplitOptions.RemoveEmptyEntries);
                        foreach (var s in list)
                        {
                            try
                            {
                                if (s != "" && s != "0")
                                {
                                    Console.WriteLine("Data:{0}", s);
                                    var w = String.Format("Data:{0} word:{1}", s, DataContextCollection.GetInstance().fullWordList[s]);
                                    Console.WriteLine(w);
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                                    {
                                        mwWindow.lbl_candidate1.Content = DataContextCollection.GetInstance().fullWordList[s];
                                    });
                                }
                                if (s.ToLower() == "redo")
                                {
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                                    {
                                        mwWindow.lbl_candidate1.Content = "請重做一次";
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            
                        }                        
                        
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("receive data error:{0}",e);
                    }

                }

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

        public override void Stop()
        {
            base.Stop();
            if (sensor != null)
            {
                sensor.AllFramesReady -= AllFrameReady;
            }
        }
    }
}
