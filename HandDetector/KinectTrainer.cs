using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector
{
    class KinectTrainer : KinectSDKController
    {
        protected string currentDir;
        private KinectStudioController controler;
        protected StreamWriter skeWriter = null;
        long depthFirstTime = 0;
        private System.Timers.Timer timer;
        private int CurrentFrame;
        private int PreviousFrame;
        protected string HandshapePath = null;
        private bool Connected = false;
        private bool IsRecording = false;
        private long _firstTimeStamp = long.MaxValue;
        protected KinectTrainer() :base()
        {
            controler = KinectStudioController.GetSingleton();
            Connected = controler.Connect();
            //m_OpenCVController.ShowImg();
        }

        public long FirstTimeStamp
        {
            get { return _firstTimeStamp; }
            set
            {
                if (value != 0)
                {
                    _firstTimeStamp = value;
                }
            }
        }

        public new static KinectController GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectTrainer();
            }
            return singleInstance;
        }

        /// <summary>
        /// Use the sticky currentSkeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
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
                //Console.WriteLine(currentlyTrackedSkeletonId);
            }
        }

        private int firstFrame = 0;
        private Rectangle rightFirst;
        private Rectangle leftFirst;
        protected override void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            if (!IsRecording)
            {
                return;
            }
            string line = "";
            headTracked = false;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (skeletonFrame.Timestamp == 0)
                    {
                        Reset();
                        return;
                    }
                    if (skeletonFrame.Timestamp < FirstTimeStamp)
                    {
                        FirstTimeStamp = skeletonFrame.Timestamp;
                    }
                    //Console.WriteLine("currentSkeleton {0} first {1}",skeletonFrame.Timestamp,FirstTimeStamp);
                    VideoFrame = GetRealCurrentFrame(skeletonFrame.Timestamp - FirstTimeStamp);
                    //Console.WriteLine("currentSkeleton:{0}", VideoFrame);
                    var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
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
                    line = VideoFrame.ToString() + "," + GetSkeletonArgs(currentSkeleton);
                   
                }
                
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (colorFrame.Timestamp == 0)
                    {
                        Reset();
                        return;
                    }
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    if (colorFrame.Timestamp < FirstTimeStamp)
                    {
                        FirstTimeStamp = colorFrame.Timestamp;
                    }
                    // Write the pixel data into our bitmap
                    //this.ColorWriteBitmap.WritePixels(
                    //    new System.Windows.Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
                    //    this.colorPixels,
                    //    this.ColorWriteBitmap.PixelWidth * sizeof(int),
                    //    0);
                }
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    if (depthFrame.Timestamp == 0)
                    {
                        Reset();
                        return;
                    }
                    if (depthFrame.Timestamp < FirstTimeStamp)
                    {
                        FirstTimeStamp = depthFrame.Timestamp;
                    }
                    //var sw = Stopwatch.StartNew();
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);
                    _mappedColorLocations = new ColorImagePoint[depthFrame.PixelDataLength];
                    sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    this.depthImagePixels,
                    ColorFormat,
                    _mappedColorLocations);

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
                                 ColorFormat, DepthFormat,
                                 this.depthImagePixels,
                                 this._mappedDepthLocations);


                    Image<Gray, Byte> rightFront = null;
                    Image<Gray, Byte> leftFront = null;
                    PointF rightVector = PointF.Empty;
                    PointF leftVector = PointF.Empty;
                    bool isSkip = true;
                    bool leftHandRaise = false;
                    bool rightHandRaise = false;
                    HandShapeModel handModel = null;
                    //if (!isSkip)
                    {
                        handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth,3);
                    }
                    if (handModel != null && handModel.type != HandEnum.None)
                    {
                        if (!handModel.left.IsCloseTo(leftFirst))
                        {
                            leftHandRaise = true;
                        }
                    }
                    
                    //Console.WriteLine("recog:{0}", sw.ElapsedMilliseconds);
                    if (currentSkeleton != null )
                    {
                        PointF hr = SkeletonPointToScreen(currentSkeleton.Joints[JointType.HandRight].Position);
                        PointF hl = SkeletonPointToScreen(currentSkeleton.Joints[JointType.HandLeft].Position);
                        PointF er = SkeletonPointToScreen(currentSkeleton.Joints[JointType.ElbowRight].Position);
                        PointF el = SkeletonPointToScreen(currentSkeleton.Joints[JointType.ElbowLeft].Position);
                        PointF hip = SkeletonPointToScreen(currentSkeleton.Joints[JointType.HipCenter].Position);
                        // hand is lower than hip
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandRight].Position.Y);
                        //Console.WriteLine(currentSkeleton.Joints[JointType.HipCenter].Position.Y);
                        //Console.WriteLine("-------------");
                        if (currentSkeleton.Joints[JointType.HandRight].Position.Y >
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            isSkip = false;
                        }
                        if (currentSkeleton.Joints[JointType.HandRight].Position.Y >
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        //if (handModel.right.GetYCenter() < hip.Y + 50 || (handModel.IntersectRectangle != Rectangle.Empty && handModel.IntersectRectangle.Y < hip.Y + 50))
                        {
                            rightHandRaise = true;
                        }
                        if (currentSkeleton.Joints[JointType.HandLeft].Position.Y >
                            currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            leftHandRaise = true;
                        }
                        //if (currentSkeleton.Joints[JointType.HandLeft].Position.Y >currentSkeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        //{
                            //leftHandRaise = true;
                        //}

                        //Console.WriteLine(currentSkeleton.Joints[JointType.HandRight].Position.Y);

                        rightVector.X = (hr.X - er.X);
                        rightVector.Y = (hr.Y - er.Y);
                        leftVector.X = (hl.X - el.X);
                        leftVector.Y = (hl.Y - el.Y);
                    }
                    
                    //Console.WriteLine("recog:{0}", sw.ElapsedMilliseconds);
                    //sw.Restart();
                    VideoFrame = GetRealCurrentFrame(depthFrame.Timestamp - FirstTimeStamp);
                    //Console.WriteLine("depth:{0}",VideoFrame);
                    if (handModel != null && handModel.type != HandEnum.None)
                    {
                        if (rightFirst == Rectangle.Empty && handModel.type == HandEnum.Both)
                        {
                            rightFirst = handModel.right;
                            leftFirst = handModel.left;
                        }
                        if (handModel.IntersectRectangle != Rectangle.Empty
                                 && !leftHandRaise)
                        {
                            //false intersect right hand behind head and left hand on initial position
                            // to overcome the problem of right hand lost and left hand recognized as intersected.
                        }
                        else
                        {
                            if (handModel.type == HandEnum.Intersect)
                            {
                                if (handModel.RightColor != null && !handModel.IntersectRectangle.IsCloseTo(rightFirst) && !handModel.IntersectRectangle.IsCloseTo(leftFirst))
                                {
                                    var colorRight = handModel.RightColor;
                                    string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'C');
                                    colorRight.Save(fileName);
                                    var depthRight = handModel.RightDepth;
                                    fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'D');
                                    //depthRight.Save(fileName);
                                }
                            }
                            else
                            {
                                // to overcome the problem of right hand lost and left hand recognized as intersected.
                                if (handModel.RightColor != null && !handModel.right.IsCloseTo(rightFirst) && !handModel.right.IsCloseTo(leftFirst))
                                {
                                    var colorRight = handModel.RightColor;
                                    string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'C');
                                    colorRight.Save(fileName);
                                    var depthRight = handModel.RightDepth;
                                    fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'D');
                                    //depthRight.Save(fileName);
                                    //left hand
                                    if (handModel.LeftColor != null && !handModel.left.IsCloseTo(leftFirst))
                                    {
                                        var colorleft = handModel.LeftColor;
                                        fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                            HandshapePath, VideoFrame.ToString(), handModel.type, 'C', "left");
                                        colorleft.Save(fileName);

                                        var depthleft = handModel.LeftDepth;
                                        fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                            HandshapePath, VideoFrame.ToString(), handModel.type, 'D', "left");
                                        //depthleft.Save(fileName);
                                    }
                                }

                            }
                            
                            line += GetHandModelString(handModel);
                        }
                       

                    }
                    depthImg.Dispose();
                    //Console.WriteLine("save:{0}", sw.ElapsedMilliseconds);
                    //sw.Restart();
                    //*******************upadte UI
                    //this.DepthWriteBitmap.WritePixels(
                    //    new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                    //    this.colorPixels,
                    //    this.DepthWriteBitmap.PixelWidth * sizeof(int),
                    //    0);
             

                }
            }
            if (skeWriter != null)
            {
                skeWriter.WriteLine(line);
            }
            CurrentFrame++;
        }

        protected string GetHandModelString(HandShapeModel model)
        {
            string r = ",";
            r += GetRectangleString(model.right) + ",";
            r += GetRectangleString(model.left) + ",";
            if (model.type == HandEnum.Intersect || model.type == HandEnum.IntersectTouch)
            {
                r += GetRectangleString(model.IntersectRectangle);                                
            }
            return r;
        }

        protected string GetRectangleString(Rectangle rect)
        {
            return rect.GetXCenter().ToString() + "," + rect.GetYCenter().ToString();
        }
        public void OpenDir(string dir)
        {
            if (!Connected)
            {
                Console.WriteLine("not connected");
                return;
            }
            var t = new Thread(new ParameterizedThreadStart(ControlThread));
            t.Start(dir);
        }

        public override void Reset()
        {
            //base.Reset();
            m_OpenCVController.Reset();
            CurrentFrame = 0;
            PreviousFrame = 0;
            FirstTimeStamp = long.MaxValue;
            rightFirst = Rectangle.Empty;
            leftFirst = Rectangle.Empty;
            currentSkeleton = null;
        }
        private void ControlThread(object obj)
        {
            string dir = obj as string;
            var fileNames = Directory.GetFiles(dir, "*.xed").ToList();
            var DI = new DirectoryInfo(dir);
            foreach (var subDir in DI.GetDirectories())
            {
                fileNames.AddRange(Directory.GetFiles(subDir.FullName, "*.xed").ToList());
            }
            sensor.AllFramesReady += AllFrameReady;
            for (int i = 0; i < fileNames.Count; i++)
            {
                try
                {
                    var single_file_name = System.IO.Path.GetFileNameWithoutExtension(fileNames[i]);
                    dir = Path.GetDirectoryName(fileNames[i]);
                    CreateFolder(dir + "\\" + single_file_name,false);
                    HandshapePath = dir + "\\" + single_file_name + "\\handshape";
                    CreateFolder(HandshapePath ,true);
                    CreateFolder(HandshapePath + "\\left",true);
                    FileStream file_name = File.Open(dir + "\\" + single_file_name + "\\" + single_file_name + ".csv", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //Console.WriteLine(FirstTimeStamp);
                    controler.Open_File(fileNames[i]);
                    Console.WriteLine("Processing... :" + (i + 1) + "\\" + fileNames.Count + " " + single_file_name);
                    System.Threading.Thread.Sleep(1500);
                    Console.WriteLine("Run frame");
                    skeWriter = new StreamWriter(file_name);
                    Reset();
                    IsRecording = true;
                    controler.Run();

                    while (IsRecording)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (CurrentFrame <= 0)
                        {
                            continue;
                        }
                        if (CurrentFrame != PreviousFrame)
                        {
                            Console.WriteLine("Running " + CurrentFrame);
                            IsRecording = true;
                            PreviousFrame = CurrentFrame;        // Save the frame no.
                        }
                        else
                        {
                            Console.WriteLine("End of the frame");
                            IsRecording = false;                // Tell the program to proceed the next file
                        }
                    }
                    System.Threading.Thread.Sleep(2000);
                    skeWriter.Flush();
                    skeWriter.Close();
                    skeWriter = null;
                    //Console.WriteLine(FirstTimeStamp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

            }
            Console.WriteLine("Finish");
        }

       

        public void CreateFolder(string path, bool delete)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);  // Create the folder if it is not existed
            if (delete)
            {
                var dir = new DirectoryInfo(path);
                foreach (System.IO.FileInfo file in dir.GetFiles()) file.Delete();
                foreach (System.IO.DirectoryInfo subDirectory in dir.GetDirectories()) subDirectory.Delete(true);
            }

        }
    }
}
