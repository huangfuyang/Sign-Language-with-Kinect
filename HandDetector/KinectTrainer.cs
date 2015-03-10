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
        private string currentDir;
        private KinectStudioController controler;
        StreamWriter skeWriter = null;
        long depthFirstTime = 0;
        private System.Timers.Timer timer;
        private int CurrentFrame;
        private int PreviousFrame;
        private string HandshapePath = null;
        private bool Connected = false;
        private bool IsRecording = false;
        private long _firstTimeStamp = long.MaxValue;
        private KinectTrainer() :base()
        {
            controler = KinectStudioController.GetSingleton();
            Connected = controler.Connect();
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
                    //Console.WriteLine("skeleton {0} first {1}",skeletonFrame.Timestamp,FirstTimeStamp);
                    VideoFrame = GetRealCurrentFrame(skeletonFrame.Timestamp - FirstTimeStamp);
                    //Console.WriteLine("skeleton:{0}", VideoFrame);
                    line = VideoFrame.ToString() + ",null";
                    var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    foreach (var skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            SkeletonPoint head = skel.Joints[JointType.Head].Position;
                            headPosition = SkeletonPointToScreen(head);
                            skeleton = skel;
                            line = VideoFrame.ToString() + "," + GetSkeletonArgs(skel);
                        }
                    }
                    
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

                    if (headPosition.X == 0)
                    {
                        headDepth = 1500;
                    }
                    else
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
                    if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
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
                        if (skeleton.Joints[JointType.HandRight].Position.Y >
                            skeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            isSkip = false;
                        }
                        if (skeleton.Joints[JointType.HandLeft].Position.Y >
                            skeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            leftHandRaise = true;
                        }

                        //Console.WriteLine(skeleton.Joints[JointType.HandRight].Position.Y);

                        rightVector.X = (hr.X - er.X);
                        rightVector.Y = (hr.Y - er.Y);
                        leftVector.X = (hl.X - el.X);
                        leftVector.Y = (hl.Y - el.Y);
                    }
                    HandShapeModel handModel = null;
                    //if (!isSkip)
                    {
                        handModel = m_OpenCVController.FindHandFromColor(depthImg, colorPixels, _mappedDepthLocations, headPosition, headDepth);

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
                        if (handModel.intersectCenter != Rectangle.Empty
                                && (handModel.intersectCenter.IsCloseTo(rightFirst)
                                || handModel.intersectCenter.IsCloseTo(leftFirst)))
                        {
                            //false intersect right hand behind head and left hand on initial position
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
                            }
                            if (handModel.LeftColor != null && !handModel.left.IsCloseTo(leftFirst))
                            {
                                var colorleft = handModel.LeftColor;
                                string fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, VideoFrame.ToString(), handModel.type, 'C', "left");
                                colorleft.Save(fileName);

                                var depthleft = handModel.LeftDepth;
                                fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, VideoFrame.ToString(), handModel.type, 'D', "left");
                                //depthleft.Save(fileName);
                            }
                        }
                       
                        line += GetHandModelString(handModel);
                        handModel.Dispose();

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

        private string GetHandModelString(HandShapeModel model)
        {
            string r = ",";
            r += GetRectangleString(model.right) + ",";
            r += GetRectangleString(model.left) + ",";
            if (model.type == HandEnum.Intersect || model.type == HandEnum.IntersectTouch)
            {
                r += GetRectangleString(model.intersectCenter);                                
            }
            return r;
        }

        private string GetRectangleString(Rectangle rect)
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
            m_OpenCVController.Reset();
            CurrentFrame = 0;
            PreviousFrame = 0;
            FirstTimeStamp = long.MaxValue;
            rightFirst = Rectangle.Empty;
            leftFirst = Rectangle.Empty;
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
                    if (i % 10 == 9)
                    {
                        GC.Collect();
                    }
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

        private string GetSkeletonArgs(Skeleton skel)
        {
            if (skel != null)
            {
                DepthImagePoint dp_csv;
                ColorImagePoint cp_csv;
                
                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                {
                    #region skeleton
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    //head
                    //skel.Joints[JointType.Head].TrackingState;
                    float headX = skel.Joints[JointType.Head].Position.X;
                    float headY = skel.Joints[JointType.Head].Position.Y;
                    float headZ = skel.Joints[JointType.Head].Position.Z;
                    float headX_color = cp_csv.X;
                    float headY_color = cp_csv.Y;
                    float headX_depth = dp_csv.X;
                    float headY_depth = dp_csv.Y;

                    //shoulder
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float shoulderLX = skel.Joints[JointType.ShoulderLeft].Position.X;
                    float shoulderLY = skel.Joints[JointType.ShoulderLeft].Position.Y;
                    float shoulderLZ = skel.Joints[JointType.ShoulderLeft].Position.Z;
                    float shoulderLX_color = cp_csv.X;
                    float shoulderLY_color = cp_csv.Y;
                    float shoulderLX_depth = dp_csv.X;
                    float shoulderLY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderCenter].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float shoulderCX = skel.Joints[JointType.ShoulderCenter].Position.X;
                    float shoulderCY = skel.Joints[JointType.ShoulderCenter].Position.Y;
                    float shoulderCZ = skel.Joints[JointType.ShoulderCenter].Position.Z;
                    float shoulderCX_color = cp_csv.X;
                    float shoulderCY_color = cp_csv.Y;
                    float shoulderCX_depth = dp_csv.X;
                    float shoulderCY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float shoulderRX = skel.Joints[JointType.ShoulderRight].Position.X;
                    float shoulderRY = skel.Joints[JointType.ShoulderRight].Position.Y;
                    float shoulderRZ = skel.Joints[JointType.ShoulderRight].Position.Z;
                    float shoulderRX_color = cp_csv.X;
                    float shoulderRY_color = cp_csv.Y;
                    float shoulderRX_depth = dp_csv.X;
                    float shoulderRY_depth = dp_csv.Y;

                    //elbow
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ElbowLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float elbowLX = skel.Joints[JointType.ElbowLeft].Position.X;
                    float elbowLY = skel.Joints[JointType.ElbowLeft].Position.Y;
                    float elbowLZ = skel.Joints[JointType.ElbowLeft].Position.Z;
                    float elbowLX_color = cp_csv.X;
                    float elbowLY_color = cp_csv.Y;
                    float elbowLX_depth = dp_csv.X;
                    float elbowLY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ElbowRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float elbowRX = skel.Joints[JointType.ElbowRight].Position.X;
                    float elbowRY = skel.Joints[JointType.ElbowRight].Position.Y;
                    float elbowRZ = skel.Joints[JointType.ElbowRight].Position.Z;
                    float elbowRX_color = cp_csv.X;
                    float elbowRY_color = cp_csv.Y;
                    float elbowRX_depth = dp_csv.X;
                    float elbowRY_depth = dp_csv.Y;

                    //writst
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.WristLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.WristLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float wristLX = skel.Joints[JointType.WristLeft].Position.X;
                    float wristLY = skel.Joints[JointType.WristLeft].Position.Y;
                    float wristLZ = skel.Joints[JointType.WristLeft].Position.Z;
                    float wristLX_color = cp_csv.X;
                    float wristLY_color = cp_csv.Y;
                    float wristLX_depth = dp_csv.X;
                    float wristLY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.WristRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.WristRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float wristRX = skel.Joints[JointType.WristRight].Position.X;
                    float wristRY = skel.Joints[JointType.WristRight].Position.Y;
                    float wristRZ = skel.Joints[JointType.WristRight].Position.Z;
                    float wristRX_color = cp_csv.X;
                    float wristRY_color = cp_csv.Y;
                    float wristRX_depth = dp_csv.X;
                    float wristRY_depth = dp_csv.Y;

                    // hand
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float handLX = skel.Joints[JointType.HandLeft].Position.X;
                    float handLY = skel.Joints[JointType.HandLeft].Position.Y;
                    float handLZ = skel.Joints[JointType.HandLeft].Position.Z;
                    float handLX_color = cp_csv.X;
                    float handLY_color = cp_csv.Y;
                    float handLX_depth = dp_csv.X;
                    float handLY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float handRX = skel.Joints[JointType.HandRight].Position.X;
                    float handRY = skel.Joints[JointType.HandRight].Position.Y;
                    float handRZ = skel.Joints[JointType.HandRight].Position.Z;
                    float handRX_color = cp_csv.X;
                    float handRY_color = cp_csv.Y;
                    float handRX_depth = dp_csv.X;
                    float handRY_depth = dp_csv.Y;

                    //spine
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.Spine].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float spineX = skel.Joints[JointType.Spine].Position.X;
                    float spineY = skel.Joints[JointType.Spine].Position.Y;
                    float spineZ = skel.Joints[JointType.Spine].Position.Z;
                    float spineX_color = cp_csv.X;
                    float spineY_color = cp_csv.Y;
                    float spineX_depth = dp_csv.X;
                    float spineY_depth = dp_csv.Y;

                    //hip
                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float hipLX = skel.Joints[JointType.HipLeft].Position.X;
                    float hipLY = skel.Joints[JointType.HipLeft].Position.Y;
                    float hipLZ = skel.Joints[JointType.HipLeft].Position.Z;
                    float hipLX_color = cp_csv.X;
                    float hipLY_color = cp_csv.Y;
                    float hipLX_depth = dp_csv.X;
                    float hipLY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipCenter].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float hipCX = skel.Joints[JointType.HipCenter].Position.X;
                    float hipCY = skel.Joints[JointType.HipCenter].Position.Y;
                    float hipCZ = skel.Joints[JointType.HipCenter].Position.Z;
                    float hipCX_color = cp_csv.X;
                    float hipCY_color = cp_csv.Y;
                    float hipCX_depth = dp_csv.X;
                    float hipCY_depth = dp_csv.Y;

                    dp_csv = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    cp_csv = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                    float hipRX = skel.Joints[JointType.HipRight].Position.X;
                    float hipRY = skel.Joints[JointType.HipRight].Position.Y;
                    float hipRZ = skel.Joints[JointType.HipRight].Position.Z;
                    float hipRX_color = cp_csv.X;
                    float hipRY_color = cp_csv.Y;
                    float hipRX_depth = dp_csv.X;
                    float hipRY_depth = dp_csv.Y;
                    #endregion
                    return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66},{67},{68},{69},{70},{71},{72},{73},{74},{75},{76},{77},{78},{79},{80},{81},{82},{83},{84},{85},{86},{87},{88},{89},{90},{91},{92},{93},{94},{95},{96},{97}", headX, headY, headZ, headX_color, headY_color, headX_depth, headY_depth, shoulderLX, shoulderLY, shoulderLZ, shoulderLX_color, shoulderLY_color, shoulderLX_depth, shoulderLY_depth, shoulderCX, shoulderCY, shoulderCZ, shoulderCX_color, shoulderCY_color, shoulderCX_depth, shoulderCY_depth, shoulderRX, shoulderRY, shoulderRZ, shoulderRX_color, shoulderRY_color, shoulderRX_depth, shoulderRY_depth, elbowLX, elbowLY, elbowLZ, elbowLX_color, elbowLY_color, elbowLX_depth, elbowLY_depth, elbowRX, elbowRY, elbowRZ, elbowRX_color, elbowRY_color, elbowRX_depth, elbowRY_depth, wristLX, wristLY, wristLZ, wristLX_color, wristLY_color, wristLX_depth, wristLY_depth, wristRX, wristRY, wristRZ, wristRX_color, wristRY_color, wristRX_depth, wristRY_depth, handLX, handLY, handLZ, handLX_color, handLY_color, handLX_depth, handLY_depth, handRX, handRY, handRZ, handRX_color, handRY_color, handRX_depth, handRY_depth, spineX, spineY, spineZ, spineX_color, spineY_color, spineX_depth, spineY_depth, hipLX, hipLY, hipLZ, hipLX_color, hipLY_color, hipLX_depth, hipLY_depth, hipCX, hipCY, hipCZ, hipCX_color, hipCY_color, hipCX_depth, hipCY_depth, hipRX, hipRY, hipRZ, hipRX_color, hipRY_color, hipRX_depth, hipRY_depth);

                }
                else
                {
                    return "untracked";
                }
            }
            else
            {
                return "null";
            }
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
