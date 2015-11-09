using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;
using Image = System.Windows.Controls.Image;

namespace CURELab.SignLanguage.HandDetector
{
    class KinectTrainOnline : KinectTrainer
    {
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        private MainUI mwWindow;
        private KinectTrainOnline(MainUI mw)
            : base()
        {
            mwWindow = mw;
            viewer = new ImageViewer();

        }

        public static KinectController GetSingletonInstance(MainUI mw)
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectTrainOnline(mw);
            }
            return singleInstance;
        }

        public override void Initialize(KinectSensor _sensor)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            sensor = _sensor;
            ShowFinal = true;

            if (null != sensor)
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
            string line = "";
            headTracked = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    //Console.WriteLine("ske:{0}", skeletonFrame.Timestamp);
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                   // this.backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);
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
                    line = GetSkeletonArgs(currentSkeleton);
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
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        this.ColorWriteBitmap.WritePixels(
                            new System.Windows.Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth,
                                this.ColorWriteBitmap.PixelHeight),
                            this.colorPixels,
                            this.ColorWriteBitmap.PixelWidth*sizeof (int),
                            0);
                    }));
                }
            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    VideoFrame = depthFrame.FrameNumber;
                    line = VideoFrame.ToString() + "," + line;
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
                        //if (handModel.right.GetYCenter() < hip.Y + 50 || (handModel.IntersectRectangle != Rectangle.Empty && handModel.IntersectRectangle.Y < hip.Y + 50))
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
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            IsRecording = true;
                            mwWindow.lbl_candidate1.Content = "錄製中";
                            mwWindow.lbl_candidate1.Foreground = Brushes.Red;
                        }));
                    }
                    //stop recording
                    if (IsRecording && handModel.type != HandEnum.None && !rightHandRaise && !leftHandRaise)
                    {
                        Console.WriteLine("END");
                        IsRecording = false;
                        TurnEnd = true;
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            mwWindow.lbl_candidate1.Content = "录制结束";
                            mwWindow.lbl_candidate1.Foreground = Brushes.Black;
                        }));
                    }



                    //if (currentSkeleton != null)
                    if (IsRecording)
                    {

                        handModel.skeletonData = FrameConverter.GetFrameDataArgString(currentSkeleton);
                        if (handModel.IntersectRectangle != Rectangle.Empty
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
                            Console.WriteLine(handModel.type);

                            // to overcome the problem of right hand lost and left hand recognized as intersected.
                            if (handModel.RightColor != null)
                            {
                                var colorRight = handModel.RightColor;
                                string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, VideoFrame.ToString(), handModel.type, 'C');
                                colorRight.Save(fileName);
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    mwWindow.img_right.Source = colorRight.Bitmap.ToBitmapSource();
                                }));
                                var depthRight = handModel.RightDepth;
                                fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, VideoFrame.ToString(), handModel.type, 'D');
                                //depthRight.Save(fileName);
                                //left hand
                                if (handModel.LeftColor != null && leftHandRaise)
                                {
                                    var colorleft = handModel.LeftColor;
                                    fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'C', "left");
                                    colorleft.Save(fileName);
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                                    {
                                        mwWindow.img_left.Source = colorleft.Bitmap.ToBitmapSource();
                                    }));
                                    var depthleft = handModel.LeftDepth;
                                    fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, VideoFrame.ToString(), handModel.type, 'D', "left");
                                    //depthleft.Save(fileName);
                                }
                                else
                                {
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                                    {
                                        mwWindow.img_left.Source = new Bitmap(100,100).ToBitmapSource();
                                    }));
                                }
                            }
                            else
                            {
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    mwWindow.img_right.Source = new Bitmap(100,100).ToBitmapSource();
                                    mwWindow.img_left.Source = new Bitmap(100, 100).ToBitmapSource();
                                }));
                            }

                            line += GetHandModelString(handModel);
                        }
                         

                    }
                    if (skeWriter != null)
                    {
                        skeWriter.WriteLine(line);
                    }
                    //*******************upadte UI
                    if (ShowFinal)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            this.DepthWriteBitmap.WritePixels(
                                new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth,
                                    this.DepthWriteBitmap.PixelHeight),
                                processImg,
                                this.DepthWriteBitmap.PixelWidth*sizeof (int),
                                0);
                        }));
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            this.DepthWriteBitmap.WritePixels(
                                new System.Windows.Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth,
                                    this.DepthWriteBitmap.PixelHeight),
                                colorPixels,
                                this.DepthWriteBitmap.PixelWidth*sizeof (int),
                                0);
                        }));
                    }

                    //ImageConverter.UpdateWriteBMP(DepthWriteBitmap, depthImg.ToBitmap());
                    // Console.WriteLine("Update UI:" + sw.ElapsedMilliseconds);
                    End = TurnEnd;
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

        private List<SignModel> slist;
        private string signer = "hfy";
        public override void Start()
        {
            try
            {
                OpenFileDialog fbd = new OpenFileDialog();

                #region 

                //DialogResult result = fbd.ShowDialog();
                //if (result == System.Windows.Forms.DialogResult.OK)
                //{
                //    var signFile = fbd.OpenFile();
                //    StreamReader sr = new StreamReader(signFile);
                //    string line = sr.ReadLine();
                //    slist = new List<SignModel>();
                //    while (!String.IsNullOrEmpty(line))
                //    {
                //        var s = line.Split();
                //        SignModel model = new SignModel()
                //        {
                //            ID = s[1],
                //            Name = s[3]
                //        };
                //        slist.Add(model);
                //        line = sr.ReadLine();
                //    }
                //    signFile.Close();
                //    Thread t = new Thread(ControlThread);
                //    t.Start();

                //}
                //else

                #endregion

                {
                    int start = 0;
                    int end = 33;
                    slist = new List<SignModel>();
                    string path = @"D:\Kinectdata\aaron-michael\video\";
                    var files = Directory.GetFiles(path);
                    for (int i = start-1; i < end; i++)
                    {
                        if (i>=0 && i < files.Length)
                        {
                            var fname = files[i].Split('\\').Last();
                            fname = fname.Substring(0, fname.Length - 6);
                            SignModel model = new SignModel()
                            {
                                index = i,
                                ID = fname.Split()[0],
                                Name = DataContextCollection.GetInstance().fullWordList[fname.Split()[0]],
                                Video = files[i],
                            };
                            var keyframe =
                                Directory.GetFiles(
                                    String.Format(@"D:\Kinectdata\aaron-michael\image\{0}\handshape", fname), "*#.jpg");
                            if (keyframe.Length > 0)
                            {
                                model.Images = new string[keyframe.Length];
                                for (int j = 0; j < keyframe.Length; j++)
                                {
                                    model.Images[j] = keyframe[j];
                                }
                            }
                            slist.Add(model);
                        }
                       
                    }
               
                 
                    Thread t = new Thread(ControlThread);
                    t.Start();
                }


            }
            catch (Exception)
            {
                Console.WriteLine("could not open file");
            }
        }
        private Timer My_Timer = new Timer();
        Capture _CCapture = null;

        private void PlayVideo(string name)
        {
            Thread t = new Thread(new ParameterizedThreadStart(PlayVideo));
            t.Start(name);
            t.Join();
        }
        private void PlayVideo(object file)
        {
            try
            {
                if (_CCapture != null)
                {
                    _CCapture.Dispose(); //dispose of current capture
                }
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                {
                    viewer.Show();
                });
                _CCapture = new Capture(file as string);
                int FrameRate = (int)_CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
                int cframe = (int)_CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                int framenumber = (int)_CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                while (_CCapture.Grab())
                {
                    var frame = _CCapture.RetrieveBgrFrame().Resize(800, 600, INTER.CV_INTER_LINEAR);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        viewer.Size = frame.Size;
                        viewer.Image = frame;
                    });
                    Thread.Sleep(1000 / FrameRate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                {
                    viewer.Hide();
                });
            }

        }


        private bool TurnEnd = false;
        private bool End = false;
        private ImageViewer viewer;
        private int repeat = 10;
        private void ControlThread()
        {

            string dir = @"D:\TrainingData\" + signer+"\\";
            for (int i = 0; i < slist.Count; i++)
            {
                var m = slist[i];
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    mwWindow.lbl_candidate1.Content = String.Format("播放:{0},{1}/{2}", m.Name,i+1,slist.Count);
                    mwWindow.spn_key.Children.Clear();
                    if (m.Images != null)
                    {
                        for (int j = 0; j < m.Images.Length && j < 10; j++)
                        {
                            mwWindow.spn_key.Children.Add(new Image()
                            {
                                Source = new BitmapImage(new Uri(m.Images[j], UriKind.Absolute))
                            });
                        }
                    }
                }));
                Thread.Sleep(1000);
                //string videoName = String.Format("Videos\\{0}.mpg", m.ID);
                string videoName = m.Video;
                PlayVideo(videoName);
                // add new handshape images
                
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    mwWindow.lbl_candidate1.Content = String.Format("播放:{0} 第二遍", m.Name);
                }));
                Thread.Sleep(500);
                PlayVideo(videoName);
                for (int j = 0; j < repeat; j++)
                {
                    currentDir = dir + m.ID + " " + signer + " " + m.index.ToString() + '_'+j.ToString();
                    Directory.CreateDirectory(currentDir);
                    FileStream file_name = File.Open(currentDir + "\\" + m.ID + ".csv", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    if (skeWriter != null)
                    {
                        skeWriter.Close();
                    }
                    skeWriter = new StreamWriter(file_name);
                    HandshapePath = currentDir + "\\handshape";
                    Directory.CreateDirectory(HandshapePath);
                    Directory.CreateDirectory(HandshapePath + "\\left");
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        mwWindow.lbl_candidate1.Content = String.Format("开始录制第{0}遍", j + 1);
                    }));
                    if (sensor != null)
                    {
                        sensor.AllFramesReady += AllFrameReady;
                    }
                    while (!End)
                    {
                        Thread.Sleep(500);
                    }
                    TurnEnd = false;
                    End = false;
                    sensor.AllFramesReady -= AllFrameReady;
                }
            }
        }
    }

    struct SignModel
    {
        public int index;
        public string ID;
        public string Name;
        public string Video;
        public string[] Images;  
    }
}
