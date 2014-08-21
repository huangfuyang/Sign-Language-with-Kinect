using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using CURELab.SignLanguage.HandDetector;
using System.Xml.Serialization;

namespace SignLanguageEducationSystem
{
    /// <summary>
    /// Interaction logic for SignWordPage.xaml
    /// </summary>
    public partial class SignWordPage : UserControl
    {
        private byte[] _colorPixels;
        private DepthImagePixel[] _depthPixels;
        private Skeleton[] _skeletons;
        private bool isPlayed;
        private KinectSensor sensor;
        private SignModel templateModel;
        private List<Skeleton> capturedSkeletons;

        public SignWordPage(SystemStatusCollection systemStatusCollection)
        {
            InitializeComponent();
            this.DataContext = systemStatusCollection;

            systemStatusCollection.ColorBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);

            sensor = systemStatusCollection.CurrentKinectSensor;

            _colorPixels = new byte[systemStatusCollection.CurrentKinectSensor.ColorStream.FramePixelDataLength];
            _depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
            capturedSkeletons = new List<Skeleton>();
            //templateModel = LoadSkeleton("sign1.txt");
            systemStatusCollection.CurrentKinectSensor.AllFramesReady += AllFrameReady;
        }

        private void SaveSkeleton(string filename, SignModel data)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine(data.ToString());
            sw.Close();
        }

        private SignModel LoadSkeleton(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string line = sr.ReadLine();
            var sm = SignModel.CreateFromString(line);
            return sm;
        
        }



        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            //Console.Clear();
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    _skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    Skeleton skel = _skeletons[0];
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        SkeletonPoint rightHand = _skeletons[0].Joints[JointType.HandRight].Position;
                        SkeletonPoint head = _skeletons[0].Joints[JointType.Head].Position;
                        //rightHandPosition = SkeletonPointToScreen(rightHand);
                        //headPosition = SkeletonPointToScreen(head);

                    }
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    WriteableBitmap ColorBitmap = ((SystemStatusCollection)this.DataContext).ColorBitmap;

                    colorFrame.CopyPixelDataTo(this._colorPixels);
                    ((SystemStatusCollection)this.DataContext).ColorBitmap.Lock();
                    ColorBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, ColorBitmap.PixelWidth, ColorBitmap.PixelHeight),
                        _colorPixels,
                        ColorBitmap.PixelWidth * sizeof(int),
                        0);
                    ColorBitmap.Unlock();
                }
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    var sw = Stopwatch.StartNew();
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(_depthPixels);
                    // short[] depthData = new short[depthFrame.PixelDataLength];
                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;

                    //if (headPosition.X == 0)
                    //{
                    //    headDepth = 100;
                    //}
                    //else
                    //{
                    //    headDepth = depthPixels[headPosition.X + headPosition.Y * 640].Depth;
                    //}
                    //var lowDepths = depthPixels.Skip(depthPixels.Length - 640).Where(x => x.Depth > 0);
                    //int lowDepth = lowDepths.Count() > 0 ? lowDepths.Min(x => x.Depth) : 0;
                    //float a = CalTiltAngle(headPosition.Y, depthPixels[headPosition.X+headPosition.Y*640].Depth, lowDepth);
                    //Console.WriteLine("head depth :{0} lowDepth:{1}, TiltAngle:{2}", depthPixels[headPosition.X + headPosition.Y * 640].Depth, lowDepth);
                    //    Console.WriteLine("get low depth:" + sw.ElapsedMilliseconds);
                    //sw.Restart();
                    //*********** Convert cull and transform*****************
                    //colorizer.TransformCullAndConvertDepthFrame(
                    //    depthPixels, minDepth, maxDepth, colorPixels,
                    //    AngleRotateTan,
                    //    (short)(headDepth - (short)CullingThresh), headPosition);

                    //Image<Bgra, byte> depthImg;
                    ////Console.WriteLine("iteration:" + sw.ElapsedMilliseconds);
                    //sw.Restart();




                    //*******find hand*****************
                    //Image<Gray, Byte> rightFront = null;
                    //Image<Gray, Byte> leftFront = null;
                    //depthImg = ImageConverter.Array2Image(colorPixels, width, height, width * 4);
                    Point rightVector = new Point();
                    Point leftVector = new Point();
                    bool isSkip = false;
                    bool leftHandRaise = false;
                    if (_skeletons != null && _skeletons[0].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        capturedSkeletons.Add(_skeletons[0]);
                        Point hr = SkeletonPointToScreen(_skeletons[0].Joints[JointType.HandRight].Position);
                        Point hl = SkeletonPointToScreen(_skeletons[0].Joints[JointType.HandLeft].Position);
                        Point er = SkeletonPointToScreen(_skeletons[0].Joints[JointType.ElbowRight].Position);
                        Point el = SkeletonPointToScreen(_skeletons[0].Joints[JointType.ElbowLeft].Position);
                        Point hip = SkeletonPointToScreen(_skeletons[0].Joints[JointType.HipCenter].Position);
                        // hand is lower than hip
                        //Console.WriteLine(_skeletons[0].Joints[JointType.HandRight].Position.Y);
                        //Console.WriteLine(_skeletons[0].Joints[JointType.HipCenter].Position.Y);
                        //Console.WriteLine("-------------");
                        if (_skeletons[0].Joints[JointType.HandRight].Position.Y <
                            _skeletons[0].Joints[JointType.HipCenter].Position.Y + 0.05)
                        {
                            isSkip = true;
                        }
                        if (_skeletons[0].Joints[JointType.HandLeft].Position.Y >
                            _skeletons[0].Joints[JointType.HipCenter].Position.Y)
                        {
                            leftHandRaise = true;
                        }

                        rightVector.X = (hr.X - er.X);
                        rightVector.Y = (hr.Y - er.Y);
                        leftVector.X = (hl.X - el.X);
                        leftVector.Y = (hl.Y - el.Y);
                    }
                    HandShapeModel handModel = null;
                    if (!isSkip)
                    {
                        //handModel = m_OpenCVController.FindHandPart(ref depthImg, out rightFront, out leftFront, headDepth - (int)CullingThresh, rightVector, leftVector, leftHandRaise);
                    
                    }


                    // no hands detected
                    if (handModel == null)
                    {
                        handModel = new HandShapeModel(0, HandEnum.None);
                    }
                    //sw.Restart();

                    //Image<Bgr, byte>[] result = HandShapeClassifier.GetSingleton()
                    //.RecognizeGesture(handModel.hogRight, 3);
                    ////Console.WriteLine(sw.ElapsedMilliseconds);
                    //if (result != null)
                    //{
                    //    ImageConverter.UpdateWriteBMP(WrtBMP_Candidate1, result[0].Convert<Gray, byte>().ToBitmap());
                    //    ImageConverter.UpdateWriteBMP(WrtBMP_Candidate2, result[1].Convert<Gray, byte>().ToBitmap());
                    //    ImageConverter.UpdateWriteBMP(WrtBMP_Candidate3, result[2].Convert<Gray, byte>().ToBitmap());
                    //}
                    
                    //string currentSign = db == null ? "0" : db.CurrentSign.ToString();
                    //string path = @"J:\Kinect data\Aaron 141-180\hands\" + currentSign + " " + handModel.frame.ToString();
                    //// UI update
                    //if (rightFront != null)
                    //{
                    //    Bitmap right = rightFront.ToBitmap();
                    //    //right.Save(path + " r.jpg");
                    //    ImageConverter.UpdateWriteBMP(WrtBMP_RightHandFront, right);
                    //}
                    //if (leftFront != null)
                    //{
                    //    Bitmap left = leftFront.ToBitmap();
                    //    //left.Save(path + " l.jpg");
                    //    ImageConverter.UpdateWriteBMP(WrtBMP_LeftHandFront, left);
                    //}
                    //if (sw.ElapsedMilliseconds > 15)
                    //{
                    //    Console.WriteLine("Find hand:" + sw.ElapsedMilliseconds);
                    //}
                    //sw.Restart();

                    //**************************draw gray histogram
                    //Bitmap histo = m_OpenCVController.Histogram(depthImg);
                    //ImageConverter.UpdateWriteBMP(GrayWriteBitmap, histo);

                    //  draw hand position from kinect
                    // DrawHandPosition(depthBMP, rightHandPosition, System.Drawing.Brushes.Purple);

                    //*******************upadte UI
                    //ImageConverter.UpdateWriteBMP(DepthWriteBitmap, depthImg.ToBitmap());
                    // Console.WriteLine("Update UI:" + sw.ElapsedMilliseconds);

                }
            }


        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent != null)
            {
                UIElementCollection children = ((Panel)this.Parent).Children;
                if (children.Contains(this))
                {
                    children.Remove(this);
                }
            }
        }

        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            isPlayed = false;
            WaitingImage.Visibility = Visibility.Visible;
            videoPlayer.Play();
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPlayed = true;
            WaitingImage.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            var signmodel = ProcessSkeleton("sign1", capturedSkeletons);
            SaveSkeleton(signmodel.Name + ".txt", signmodel);
            capturedSkeletons.Clear();
        }

        private SignModel ProcessSkeleton(string name, List<Skeleton> s)
        {
            var signmodel = new SignModel { Name = name };
            foreach (var item in s)
            {
                SkeletonPoint rh = item.Joints[JointType.HandRight].Position;
                SkeletonPoint rs = item.Joints[JointType.ShoulderRight].Position;
                SkeletonPoint ls = item.Joints[JointType.ShoulderLeft].Position;
                SkeletonPoint head = item.Joints[JointType.Head].Position;
                SkeletonPoint hip = item.Joints[JointType.HipCenter].Position;

                float v = (rh.Y - head.Y) / (head.Y - hip.Y);
                float h = (rh.X - rs.X) / (rs.X - ls.X);
                signmodel.H_horizantal.Add(h);
                signmodel.H_vertical.Add(v);
            }
            return signmodel;
        
        }

    }
}
