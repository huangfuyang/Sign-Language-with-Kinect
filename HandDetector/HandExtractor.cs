using System;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector
{
    public class HandExtractor
    {
        private static HandExtractor singleInstance;
        public static HandExtractor GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new HandExtractor();
            }
            return singleInstance;
        }

        private OpenCVController m_OpenCVController;

        private byte[] colorPixels;
        private byte[] depthPixels;
        private ColorImagePoint[] _mappedColorLocations;
        private DepthImagePoint[] _mappedDepthLocations;
        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthImagePixels;
        private Colorizer colorizer;
        private Skeleton currentSkeleton;
        private short headDepth = 0;
        private int frame = 0;
        private bool headTracked;
        private System.Drawing.Point rightHandPosition;
        private System.Drawing.Point headPosition;
        private int currentlyTrackedSkeletonId = -1;
        private KinectSensor sensor;
        private bool IsInitialize = false;

        public static float AngleRotateTan = MichaelRotateTan;
        // demo
        public const float DemoRotateTan = 0.45f;
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.32f;

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private HandExtractor()
        {
            m_OpenCVController = OpenCVController.GetSingletonInstance();
        }

        public bool Initialize(KinectSensor sensor)
        {
            this.sensor = sensor;
            if (null != sensor && !IsInitialize)
            {
                this.colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new byte[sensor.DepthStream.FramePixelDataLength];
                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                _mappedColorLocations = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                _mappedDepthLocations = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                this.colorizer = new Colorizer(AngleRotateTan, 800, 3000);
                headPosition = new Point(320, 0);
                headDepth = 800;
                IsInitialize = true;
            }
            return IsInitialize;
        }

        private void ChooseSkeleton(Skeleton[] skeletons)
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var skeletonId = 0;
            Skeleton skeleton = null;
            foreach (var skel in skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                //if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                //{
                //    isTrackedSkeltonVisible = true;
                //    break;
                //}

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    skeletonId = skel.TrackingId;
                    skeleton = skel;
                }
            }

            if (!isTrackedSkeltonVisible && skeletonId != 0)
            {
                currentSkeleton = skeleton;
                this.currentlyTrackedSkeletonId = skeletonId;
                //Console.WriteLine(currentlyTrackedSkeletonId);
            }
        }

        private System.Drawing.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }

        public void OnSkeletonFrameUpdated(Skeleton[] skeletons)
        {
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

        public void OnColorFrameUpdated(byte[] colorPixels)
        {
            colorPixels.CopyTo(this.colorPixels, 0);
            //this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
            // Write the pixel data into our bitmap
            //this.ColorWriteBitmap.WritePixels(
            //new System.Windows.Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
            //this.colorPixels,
            //this.ColorWriteBitmap.PixelWidth * sizeof(int),
            //0);
        }

        public HandShapeModel OnDepthFrameUpdated(DepthImageFrame depthFrame, out byte[] processImg)
        {
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
                    processImg = null;
                    return null;
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
            HandShapeModel handModel = null;
            handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth, out processImg, 4);
            if (handModel == null)
            {
                handModel = new HandShapeModel(HandEnum.None);
            }

            //if (currentSkeleton != null)
            {

                handModel.skeletonData = FrameConverter.GetFrameDataArgString(sensor, currentSkeleton);
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
                    return handModel;
                    Console.WriteLine(handModel.type);
                }

            }

            processImg = null;
            return null;
        }

        public HandShapeModel ProcessAllFrame(AllFramesReadyEventArgs e, out byte[] processImg)
        {
            if (!IsInitialize)
            {
                processImg = null;
                return null;
            }
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
                    OnSkeletonFrameUpdated(skeletons);
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    //Console.WriteLine("col:{0}", colorFrame.Timestamp);

                    OnColorFrameUpdated(this.colorPixels);
                }
            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    //this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    var sw = Stopwatch.StartNew();
                    // Copy the pixel data from the image to a temporary array
                    //Console.WriteLine("dep:{0}", depthFrame.Timestamp);
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);

                    return OnDepthFrameUpdated(depthFrame, out processImg);
                }
            }
            processImg = null;
            return null;
        }

    }

}
