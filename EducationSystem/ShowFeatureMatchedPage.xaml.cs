using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EducationSystem.Detectors;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
using System.Timers;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Timers.Timer;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for ShowFeatureMatchedPage.xaml
    /// </summary>
    public partial class ShowFeatureMatchedPage : Page
    {
        private WriteableBitmap _playScreenBitmap;
        public WriteableBitmap PlayScreenBitmap
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _playScreenBitmap; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { KinectViewImage.Source = _playScreenBitmap = value; }
        }

        public static readonly DependencyProperty DominantHandPointTopProperty =
            DependencyProperty.Register("DominantHandPointTop", typeof(int), typeof(ShowFeatureMatchedPage), null);

        public int DominantHandPointTop
        {
            get { return (int)(GetValue(DominantHandPointTopProperty)); }
            set { SetValue(DominantHandPointTopProperty, value); }
        }

        public static readonly DependencyProperty DominantHandPointLeftProperty =
            DependencyProperty.Register("DominantHandPointLeft", typeof(int), typeof(ShowFeatureMatchedPage), null);

        public int DominantHandPointLeft
        {
            get { return (int)GetValue(DominantHandPointLeftProperty); }
            set { SetValue(DominantHandPointLeftProperty, value); }
        }

        public static readonly DependencyProperty DotSizeProperty =
            DependencyProperty.Register("DotSize", typeof(int), typeof(ShowFeatureMatchedPage), null);

        public int DotSize
        {
            get { return (int)GetValue(DotSizeProperty); }
            set { SetValue(DotSizeProperty, value); }
        }

        public static readonly DependencyProperty BodyPartProperty =
            DependencyProperty.Register("BodyPart", typeof(string), typeof(ShowFeatureMatchedPage), null);

        public string BodyPart
        {
            get { return (string)GetValue(BodyPartProperty); }
            set { SetValue(BodyPartProperty, value); }
        }

        public static readonly DependencyProperty CurrectWaitingStateProperty =
            DependencyProperty.Register("CurrectWaitingState", typeof(string), typeof(ShowFeatureMatchedPage), null);

        public string CurrectWaitingState
        {
            get { return (string)GetValue(CurrectWaitingStateProperty); }
            set { SetValue(CurrectWaitingStateProperty, value); }
        }

        public static readonly DependencyProperty SignStateProperty =
           DependencyProperty.Register("SignState", typeof(string), typeof(ShowFeatureMatchedPage), null);

        public string SignState
        {
            get { return (string)GetValue(SignStateProperty); }
            set { SetValue(SignStateProperty, value); }
        }

        public static readonly DependencyProperty NumOfFeatureProperty =
            DependencyProperty.Register("NumOfFeature", typeof(int), typeof(ShowFeatureMatchedPage), null);

        public int NumOfFeature
        {
            get { return (int)GetValue(NumOfFeatureProperty); }
            set { SetValue(NumOfFeatureProperty, value); }
        }

        public static readonly DependencyProperty NumOfFeatureCompletedProperty =
    DependencyProperty.Register("NumOfFeatureCompleted", typeof(int), typeof(ShowFeatureMatchedPage), null);

        public int NumOfFeatureCompleted
        {
            get { return (int)GetValue(NumOfFeatureCompletedProperty); }
            set { SetValue(NumOfFeatureCompletedProperty, value); }
        }
        // video controlling
        public static readonly DependencyProperty CurrentFrameBitmapSourceProperty =
    DependencyProperty.Register("CurrentFrameBitmapSource", typeof(BitmapSource), typeof(ShowFeatureMatchedPage), null);

        public BitmapSource CurrentFrameBitmapSource
        {
            get { return (BitmapSource)GetValue(CurrentFrameBitmapSourceProperty); }
            set { SetValue(CurrentFrameBitmapSourceProperty, value); }
        }


        private ShowFeatureMatchedPageFramesHandler framesHandler;
        private VideoModel currentModel;
        private Shape SignArrow;
        public ShowFeatureMatchedPage()
        {
            InitializeComponent();
        }

        public ObservableCollection<FeatureViewModel> FeatureList { get; set; }

        private ImageViewer viewer;
        private Timer timer_guide;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.FeatureList = new ObservableCollection<FeatureViewModel>();
            this.FeatureList.Add(new FeatureViewModel("Dominant Hand X", "0"));
            this.FeatureList.Add(new FeatureViewModel("Dominant Hand Y", "0"));
            this.FeatureList.Add(new FeatureViewModel("Region", "0"));
            this.DataContext = this;

            DominantHandPointLeft = 300;
            DotSize = 5;

            this.framesHandler = new ShowFeatureMatchedPageFramesHandler(this);
            this.framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
            KinectState.Instance.KinectRegion.IsCursorVisible = false;

            //load signs
            foreach (var row in LearningResource.GetSingleton().VideoModels)
            {
                //load button

                panelSignList.Children.Add(createKinectButton(row));
            }
            KinectScrollViewer.ScrollToVerticalOffset(100);
            viewer = new ImageViewer();
            timer_guide = new Timer()
            {
                Interval = 20
            };
            timer_guide.Elapsed += timer_guide_Elapsed;
        }

        private int GetCurrentFrame()
        {
            int r = 0;
            if (MediaMain.HasVideo)
            {
                r = (int)Math.Round(MediaMain.Position.TotalMilliseconds / 33.333);
            }

            return r;
        }

        private void RemoveArrow()
        {
            if (BodyPartCanvas.Children.Contains(SignArrow))
            {
                BodyPartCanvas.Children.Remove(SignArrow);
            }
        }

        private int CurrentKeyFrame = 0;
        void timer_guide_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (currentModel != null)
                {
                    var frame = GetCurrentFrame();
                    int startframe = -1;
                    int endframe = -1;
                    for (int i = 0; i < currentModel.KeyFrames.Count; i++)
                    {
                        if (frame >= currentModel.KeyFrames[i].FrameNumber)
                        {
                            startframe = i;
                            endframe = i+1;
                        }
                    }
                    //if (CurrentKeyFrame != startframe)
                    //{
                    //    CurrentKeyFrame = startframe;
                    //}
                    //if current frame fall in start or end
                    if (startframe == -1 || endframe >= currentModel.KeyFrames.Count)
                    {   
                        RemoveArrow();
                    }
                    else
                    {
                        MoveArror(currentModel.KeyFrames[startframe].RightPosition, currentModel.KeyFrames[endframe].RightPosition);
                    }
                    // set state
                    SignState = currentModel.KeyFrames[startframe].Type.ToString();
                    // image
                    if (currentModel.KeyFrames[startframe].RightImage != null)
                    {
                        img_right.Source = currentModel.KeyFrames[startframe].RightImage;
                    }
                    else
                    {
                        img_right.Source = null;
                    }
                    if (currentModel.KeyFrames[startframe].LeftImage != null)
                    {
                        img_left.Source = currentModel.KeyFrames[startframe].LeftImage;
                    }
                    else
                    {
                        img_left.Source = null;
                    }
                }
            }));

        }


        private KinectTileButton createKinectButton(VideoModel dc)
        {
            KinectTileButton button = new KinectTileButton();
            button.DataContext = dc;
            button.Click += btnSignWord_Click;
            button.Content = dc.Chinese;
            button.Width = 250;
            button.Height = 110;
            button.FontSize = 48;
            SolidColorBrush brush = new SolidColorBrush(Brushes.Aqua.Color);
            brush.Opacity = 0.2;
            button.Background = brush;
            return button;
        }

        private static Shape DrawLinkArrow(Point p1, Point p2)
        {
            p1 = p1 * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            p2 = p2 * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            //            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            Point p = p2;
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            Path path = new Path { Data = lineGroup, StrokeThickness = 2 };
            path.Stroke = path.Fill = Brushes.Black;
            path.Opacity = 0.5;
            return path;
        }

        private void btnSignWord_Click(object sender, RoutedEventArgs e)
        {
            KinectTileButton button = (KinectTileButton)sender;
            var dc = button.DataContext as VideoModel;
            //string videoName = String.Format("Videos\\{0}.mpg", button.DataContext.ToString());
            //viewer.Show();
            //Thread t = new Thread(new ParameterizedThreadStart(PlayVideo));
            //t.Start(dc.Path);
            currentModel = dc;
            if (dc.KeyFrames.Count>0)
            {
                
                MediaMain.Source = new Uri(dc.Path);
                MediaMain.Play();
                timer_guide.Start();
            }
            else
            {
                MessageBox.Show("This word is not prepared");
            }
            

        }
        Capture _CCapture = null;
        private void PlayVideo(object file)
        {
            try
            {
                if (_CCapture != null)
                {
                    _CCapture.Dispose(); //dispose of current capture
                }
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

        private class ShowFeatureMatchedPageFramesHandler : AbstractKinectFramesHandler
        {
            private BodyPartDetector detector = new BodyPartDetector();
            private PrickSignDetector prickSignDetector;
            private ShowFeatureMatchedPage showFeatureMatchedPage;
            private ReaderWriterLockSlim frameLock;
            private bool isRightHandPrimary = true;

            public ShowFeatureMatchedPageFramesHandler(ShowFeatureMatchedPage showFeatureMatchedPage)
            {
                this.showFeatureMatchedPage = showFeatureMatchedPage;
                this.frameLock = new ReaderWriterLockSlim();
                this.prickSignDetector = new PrickSignDetector(showFeatureMatchedPage);
            }

            public override void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData)
            {
                bool isTracked = false;
                Tuple<BodyPart, BodyPart> bodyPartForHands = null;
                Point relativePosition = new Point();

                foreach (Skeleton skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        isTracked = true;
                        bodyPartForHands = detector.decide(skeleton);
                        prickSignDetector.Update(skeleton);

                        Joint hand1 = skeleton.Joints[isRightHandPrimary ? JointType.HandRight : JointType.HandLeft];
                        Joint hand2 = skeleton.Joints[isRightHandPrimary ? JointType.HandLeft : JointType.HandRight];
                        Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                        Joint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];
                        Joint shoulderRight = skeleton.Joints[JointType.ShoulderRight];

                        if (hand1.Position.X > shoulderCenter.Position.X)
                        {
                            relativePosition.X = (hand1.Position.X - shoulderCenter.Position.X) / (shoulderRight.Position.X - shoulderCenter.Position.X);
                        }
                        else
                        {
                            relativePosition.X = -(hand1.Position.X - shoulderCenter.Position.X) / (shoulderLeft.Position.X - shoulderCenter.Position.X);
                        }

                        relativePosition.Y = 0;

                        foreach (FeatureViewModel viewModel in showFeatureMatchedPage.FeatureList)
                        {
                            if ("Dominant Hand X".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = relativePosition.X.ToString();
                            }
                            else if ("Dominant Hand Y".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = relativePosition.Y.ToString();
                            }
                            else if ("Region".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = bodyPartForHands.Item1.ToString();
                            }
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isTracked && bodyPartForHands != null)
                    {
                        showFeatureMatchedPage.DominantHandPointLeft = 421 + (int)(relativePosition.X * (467 - 375) / 2);
                        showFeatureMatchedPage.DominantHandPointTop = 135 - (int)(relativePosition.Y * (135 - 65) / 2);
                        showFeatureMatchedPage.FeatureDataGrid.Items.Refresh();
                        showFeatureMatchedPage.BodyPart = bodyPartForHands.Item1.ToString();
                    }
                    else
                    {
                        showFeatureMatchedPage.BodyPart = "";
                    }
                });
            }

            public override void ColorFrameCallback(long timestamp, int frameNumber, byte[] colorPixels)
            {
                if (colorPixels != null && colorPixels.Length > 0)
                {
                    frameLock.EnterWriteLock();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (showFeatureMatchedPage.PlayScreenBitmap == null)
                        {
                            showFeatureMatchedPage.PlayScreenBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
                        }

                        showFeatureMatchedPage.PlayScreenBitmap.WritePixels(new Int32Rect(0, 0, 640, 480), colorPixels, 640 * sizeof(int), 0);
                    });
                    frameLock.ExitWriteLock();
                }
            }
        }

        private void drawRegionOnCanvas(BodyPart bodyPart)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                DominantHandPointLeft = int.Parse((sender as TextBox).Text);
            }
            catch (Exception)
            {

            }

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                DominantHandPointTop = int.Parse((sender as TextBox).Text);
            }
            catch (Exception)
            {

            }
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {

        }

        private void MoveArror(Point from, Point to)
        {
            if (SignArrow != null)
            {
                RemoveArrow();
            }
            SignArrow = DrawLinkArrow(from, to);
            BodyPartCanvas.Children.Add(SignArrow);
        }


    }
}
