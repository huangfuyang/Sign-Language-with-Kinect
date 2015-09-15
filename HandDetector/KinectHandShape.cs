using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Point = System.Drawing.Point;

namespace CURELab.SignLanguage.HandDetector
{
    class KinectHandShape : KinectSDKController
    {
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        private KinectHandShape()
            : base()
        {
        }

        public static KinectController GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectHandShape();
            }
            return singleInstance;
        }

        protected override void ChooseSkeleton(Skeleton[] skeletons)
        {
            base.ChooseSkeleton(skeletons);
            backgroundRemovedColorStream.SetTrackedPlayer(currentlyTrackedSkeletonId);
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
                this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(sensor);
                this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);
                this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;
                //sensor.DepthStream.Range = DepthRange.Near;
                // Allocate space to put the pixels we'll receive           
                this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new byte[sensor.DepthStream.FramePixelDataLength];
                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                _mappedColorLocations = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                _mappedDepthLocations = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                // This is the bitmap we'll display on-screen
                this.ColorWriteBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgra32, null);
                this.DepthWriteBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
                // Add an event handler to be called whenever there is new frame data
                this.Status = Properties.Resources.Connected;

                this.colorizer = new Colorizer(AngleRotateTan, 800, 3000);
                headPosition = new Point(320, 0);
                headDepth = 800;
                sensor.Start();
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
                    this.backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);
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
                    //colorFrame.CopyPixelDataTo(this.colorPixels);
                    this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);

                }
            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);
                    // Get the min and max reliable depth for the current frame
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

                    Array.Clear(depthPixels, 0, depthPixels.Length);
                    // stream registration
                    var depthImg = ImageConverter.Array2Image<Gray>(depthPixels, width, height, width);
                    sensor.CoordinateMapper.MapColorFrameToDepthFrame(
                                 ColorFormat, DepthFormat,
                                 this.depthImagePixels,
                                 this._mappedDepthLocations);


                    bool rightHandRaise = false;
                    bool leftHandRaise = false;
                    byte[] processImg;
                    var handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth, out processImg,4);
                    if (handModel == null)
                    {
                        handModel = new HandShapeModel(HandEnum.None);
                    }
                    //Console.WriteLine("recog:{0}", sw.ElapsedMilliseconds);
                    if (currentSkeleton != null && handModel.type != HandEnum.None)
                    {
                        // hand is lower than hip
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandLeft].Position.Y);
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HipCenter].Position.Y);
                        //Console.WriteLine("-------------");
                        if (currentSkeleton.Joints[JointType.HandRight].Position.Y > 
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                            //if (handModel.right.GetYCenter() < hip.Y + 50 || (handModel.IntersectRectangle != Rectangle.Empty && handModel.IntersectRectangle.Y < hip.Y + 50))
                        {
                            rightHandRaise = true;
                        }
                        //if (!handModel.left.IsCloseTo(leftFirst) || (handModel.IntersectRectangle != Rectangle.Empty && !handModel.IntersectRectangle.IsCloseTo(leftFirst)))
                        if (currentSkeleton.Joints[JointType.HandLeft].Position.Y > 
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            leftHandRaise = true;
                        }

                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandRight].Position.Y);


                    }
                    //start recording
                    if (!IsRecording && rightHandRaise)
                    {
                        Console.WriteLine("RECORDING");
                        currentPath = path + frame.ToString();
                        System.IO.Directory.CreateDirectory(currentPath);
                        IsRecording = true;
                    }
                    //stop recording
                    if (IsRecording && !rightHandRaise && !leftHandRaise)
                    {
                        Console.WriteLine("END");
                        IsRecording = false;
                    }



                    //if (currentSkeleton != null)
                    if (IsRecording)
                    {

                        handModel.skeletonData = FrameConverter.GetFrameDataArgString(currentSkeleton);
                        if (handModel.type == HandEnum.Intersect)
                        {
                            if (!leftHandRaise && handModel.type == HandEnum.Both)
                            {
                                handModel.type = HandEnum.Right;
                            }
                            Console.WriteLine(handModel.type);
                            var colorRight = handModel.RightColor;
                            string fileName = String.Format("{0}\\{1}.jpg",
                                currentPath, frame.ToString());
                            colorRight.Save(fileName);
                            //var depthRight = handModel.RightDepth;
                            //fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                            //    currentPath, frame.ToString(), handModel.type, 'D');
                            //depthRight.Save(fileName);
                            frame++;
                        } 
                    }

                    //*******************upadte UI
                    this.DepthWriteBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                        processImg,
                        this.DepthWriteBitmap.PixelWidth * sizeof(int),
                        0);
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
        private string path = @"D:\handimages\";
        private string currentPath = @"D:\handimages\";
    }
}
