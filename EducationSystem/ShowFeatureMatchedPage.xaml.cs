using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CURELab.SignLanguage.HandDetector;
using EducationSystem.Detectors;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Brushes = System.Windows.Media.Brushes;
using ImageConverter = CURELab.SignLanguage.HandDetector.ImageConverter;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Windows.Point;
using SystemColors = System.Windows.SystemColors;
using Timer = System.Timers.Timer;

namespace EducationSystem
{
    public enum GuideState
    {
        StartPlay,
        StartPlayTwice,
        EndPlay,
        EndPlayTwice,
        StartGuide,
        EndGuide,
        StartEvaluation,
        EndEvaluation
    }
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

        private int wordlist = 0;
        private ShowFeatureMatchedPageFramesHandler framesHandler;
        private VideoModel currentModel;
        private Shape RightSignArrow;
        private Shape LeftSignArrow;
        private SocketManager socket;
        public ShowFeatureMatchedPage(int index)
        {
            InitializeComponent();
            wordlist = index;
        }

        public ObservableCollection<FeatureViewModel> FeatureList { get; set; }

        private ImageViewer viewer;
        private Timer timer_guide;
        private int currentWordCount = 0;
        private int total = 0;
        private bool NextReady = true;
        private int guideTime = 0;
        private int evaluateTime = 0;
        private int playTime = 0;
        private int sleepTime = 5000;
        private Timer controlTimer;
        private GuideState _state;
        public  GuideState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                switch (value)
                {
                    case GuideState.StartPlay:
                    case GuideState.StartPlayTwice:
                        btn_Guide.Visibility = Visibility.Collapsed;
                        btn_Evaluate.Visibility = Visibility.Collapsed;
                        img_left.Visibility = Visibility.Visible;
                        img_intersect.Visibility = Visibility.Visible;
                        img_right.Visibility = Visibility.Visible;
                        KinectState.Instance.KinectRegion.IsCursorVisible = true;
                        framesHandler.UnregisterCallbacks(KinectState.Instance.CurrentKinectSensor);
                        RightGuider.Visibility = Visibility.Collapsed;
                        LefttGuider.Visibility = Visibility.Collapsed;
                        break;
                    case GuideState.EndPlay:
                    case GuideState.EndPlayTwice:
                        btn_Evaluate.Visibility = Visibility.Visible;
                        btn_Guide.Visibility = Visibility.Visible;
                        KinectState.Instance.KinectRegion.IsCursorVisible = true;
                        framesHandler.UnregisterCallbacks(KinectState.Instance.CurrentKinectSensor);
                        RightGuider.Visibility = Visibility.Collapsed;
                        LefttGuider.Visibility = Visibility.Collapsed;
                        img_left.Visibility = Visibility.Collapsed;
                        img_intersect.Visibility = Visibility.Collapsed;
                        img_right.Visibility = Visibility.Collapsed;
                        break;
                    case GuideState.StartEvaluation:
                        KinectScrollViewer.Visibility = Visibility.Collapsed;
                        img_guide_intersect.Visibility = Visibility.Visible;
                        img_guide_left.Visibility = Visibility.Visible;
                        img_guide_right.Visibility = Visibility.Visible;
                        img_left.Visibility = Visibility.Visible;
                        img_intersect.Visibility = Visibility.Visible;
                        img_right.Visibility = Visibility.Visible;
                        btn_Guide.Visibility = Visibility.Collapsed;
                        btn_Evaluate.Visibility = Visibility.Collapsed;
                        KinectState.Instance.KinectRegion.IsCursorVisible = false;
                        CurrectWaitingState = "2秒鐘后開始錄製";
                        framesHandler.IsRecording = false;
                        new Thread(() =>
                        {
                            Thread.Sleep(2000);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
                                CurrectWaitingState = string.Format("請打出【{0}】的手語", currentModel.Chinese);
                            });
                        }).Start();
                        RightGuider.Visibility = Visibility.Collapsed;
                        LefttGuider.Visibility = Visibility.Collapsed;
                        break;
                    case GuideState.StartGuide:
                        KinectScrollViewer.Visibility = Visibility.Collapsed;
                        img_guide_intersect.Visibility = Visibility.Visible;
                        img_guide_left.Visibility = Visibility.Visible;
                        img_guide_right.Visibility = Visibility.Visible;
                        img_left.Visibility = Visibility.Visible;
                        img_intersect.Visibility = Visibility.Visible;
                        img_right.Visibility = Visibility.Visible;
                        btn_Guide.Visibility = Visibility.Collapsed;
                        btn_Evaluate.Visibility = Visibility.Collapsed;
                        KinectState.Instance.KinectRegion.IsCursorVisible = false;
                        framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
                        RightGuider.Visibility = Visibility.Visible;
                        LefttGuider.Visibility = Visibility.Visible;
                        break;
                    case GuideState.EndEvaluation:
                    case GuideState.EndGuide:
                        KinectScrollViewer.Visibility = Visibility.Visible;
                        img_guide_intersect.Visibility = Visibility.Collapsed;
                        img_guide_left.Visibility = Visibility.Collapsed;
                        img_guide_right.Visibility = Visibility.Collapsed;
                        img_left.Visibility = Visibility.Collapsed;
                        img_intersect.Visibility = Visibility.Collapsed;
                        img_right.Visibility = Visibility.Collapsed;
                        btn_Guide.Visibility = Visibility.Visible;
                        btn_Evaluate.Visibility = Visibility.Visible;
                        KinectState.Instance.KinectRegion.IsCursorVisible = true;
                        framesHandler.UnregisterCallbacks(KinectState.Instance.CurrentKinectSensor);
                        RightGuider.Visibility = Visibility.Collapsed;
                        LefttGuider.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.FeatureList = new ObservableCollection<FeatureViewModel>();
            this.FeatureList.Add(new FeatureViewModel("Dominant Hand X", "0"));
            this.FeatureList.Add(new FeatureViewModel("Dominant Hand Y", "0"));
            this.FeatureList.Add(new FeatureViewModel("Region", "0"));
            this.DataContext = this;
            socket = SocketManager.GetInstance();
            DominantHandPointLeft = 300;
            DotSize = 5;

            this.framesHandler = new ShowFeatureMatchedPageFramesHandler(this);
            KinectState.Instance.KinectRegion.IsCursorVisible = true;
            total = LearningResource.GetSingleton().VideoModels[wordlist].Count;
            //load signs
            foreach (var row in LearningResource.GetSingleton().VideoModels[wordlist])
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
            State = GuideState.StartPlay;
            // register data receive event
            socket.DataReceivedEvent += SocketOnDataReceivedEvent;
            // opencv
            //RegisterThreshold("V min", ref OpenCVController.VMIN, 150, OpenCVController.VMIN);
            controlTimer = new Timer(100);
            controlTimer.Elapsed += controlTimer_Elapsed;
            controlTimer.Start();
        }

        private bool startNew = true;
        void controlTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (State == GuideState.StartPlay && NextReady && playTime == 0)
            {
                playTime = 1;
                NextReady = false;
                Application.Current.Dispatcher.Invoke(
                    () => btnSignWord_Click(panelSignList.Children[currentWordCount], null));
            }
            if (State == GuideState.EndPlay && playTime == 1)
            {
                sleepTime = 0;
                playTime = 2;
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                    Application.Current.Dispatcher.Invoke(
                        () => btnSignWord_Click(panelSignList.Children[currentWordCount], null));
                }).Start();
            }
            // start guide
            if (State == GuideState.EndPlayTwice && playTime == 2)
            {
                playTime = -1;
                Application.Current.Dispatcher.Invoke(() => State = GuideState.StartGuide);
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                    Application.Current.Dispatcher.Invoke(
                        () => Btn_Guide_OnClick(btn_Guide, null));
                }).Start();
            }

            if (State == GuideState.EndGuide)
            {
                Application.Current.Dispatcher.Invoke(() => State = GuideState.StartEvaluation);
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                    Application.Current.Dispatcher.Invoke(
                        () => Btn_Evaluate_OnClick(btn_Guide, null));
                }).Start();
            }

            if (State == GuideState.EndEvaluation && !NextReady)
            {
                NextReady = true;
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                    Application.Current.Dispatcher.Invoke(StartNewWord);
                    currentWordCount++;
                    if (currentWordCount >= total)
                    {
                        controlTimer.Stop();
                        MessageBox.Show("Finish all");
                        return;
                    }
                }).Start();
            }

        }


        void StartNewWord()
        {
            NextReady = true;
            guideTime = 0;
            evaluateTime = 0;
            playTime = 0;
            sleepTime = 5000;
            State = GuideState.StartPlay;
        }

        private void SocketOnDataReceivedEvent(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var js = JsonConvert.DeserializeObject(msg) as JObject;
                    string type = js["type"].ToString();
                    Console.WriteLine(msg);
                    if (type == "guide")
                    {
                        int p = (int)js["position"];
                        int s = (int)js["handshape"];
                        // p right s wrong
                        if (p == 1 && s == 0)
                        {
                            RightGuider.Stroke = Brushes.Gold;
                            LefttGuider.Stroke = Brushes.Gold;
                        }

                        if (p == 0 && s == 0)
                        {
                            RightGuider.Stroke = Brushes.Red;
                            LefttGuider.Stroke = Brushes.Red;
                        }

                        if (p == 1 && s == 1)
                        {
                            RightGuider.Stroke = Brushes.Red;
                            LefttGuider.Stroke = Brushes.Red;
                            CurrentKeyFrame++;
                            KeyFrameChange(CurrentKeyFrame);
                        }
                    }
                    else //"evaluation
                    {
                        string m = "";
                        int c = 1;
                        int correct = 1;
                        foreach (var frame in js["data"])
                        {
                            m += "第" +c+ "帧：";
                            c += 1;
                            int p = (int)frame["position"];
                            int s = (int)frame["handshape"];
                            correct &= p;
                            correct &= s;
                            if (p == 1 && s == 0)
                            {
                                m += "位置正确,手型错误";
                            }
                            if (p == 0 && s== 0)
                            {
                                m += "位置不正确";
                            }
                            if (p == 1 && s == 1)
                            {
                                m += "完全正确";
                            }
                            m += "\n";
                        }
                        CurrectWaitingState = m;
                        Console.WriteLine(m);
                        if (correct == 1)
                        {
                            evaluateTime ++;
                        }
                        if (evaluateTime >= 3)
                        {
                            State = GuideState.EndEvaluation;
                        }
                    }
                    
                }
                catch (Exception)
                {
                    Console.WriteLine("***********************");
                    Console.WriteLine(msg);
                    Console.WriteLine("***********************");
                }
                
            });
        }

        //private unsafe void RegisterThreshold(string valuename, ref double thresh, double max, double initialValue)
        //{

        //    fixed (double* ptr = &thresh)
        //    {
        //        thresh = initialValue;
        //        TrackBar tcb = new TrackBar(ptr);
        //        tcb.Max = max;
        //        tcb.Margin = new Thickness(5);
        //        tcb.ValueName = valuename;
        //        initialValue = initialValue > max ? max : initialValue;
        //        tcb.Value = initialValue;
        //        SPn_right.Children.Add(tcb);
        //    }

        //}
        private int GetCurrentFrame()
        {
            int r = 0;
            if (MediaMain.HasVideo)
            {
                r = (int)Math.Round(MediaMain.Position.TotalMilliseconds / 33.333);
            }

            return r;
        }

        private void RemoveArrow(ref Shape s)
        {
            if (BodyPartCanvas.Children.Contains(s))
            {
                BodyPartCanvas.Children.Remove(s);
            }
        }

        private void MoveGuider(Point right, Point left)
        {
            right = right * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            left = left * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Canvas.SetLeft(RightGuider, right.X - RightGuider.Width);
                //Canvas.SetTop(RightGuider, right.Y - RightGuider.Height);
                //Canvas.SetTop(LefttGuider, left.Y - LefttGuider.Width);
                //Canvas.SetLeft(LefttGuider, left.X - LefttGuider.Height);
                Canvas.SetLeft(RightGuider, right.X );
                Canvas.SetTop(RightGuider, right.Y );
                Canvas.SetTop(LefttGuider, left.Y );
                Canvas.SetLeft(LefttGuider, left.X);
            });
           
        }

        private int CurrentKeyFrame = -1;
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
                            endframe = i + 1;
                        }
                    }

                    //if current frame fall in start or end
                    if (startframe == -1 || endframe >= currentModel.KeyFrames.Count)
                    {
                        //remove arrow
                        RemoveArrow(ref RightSignArrow);
                        RemoveArrow(ref LeftSignArrow);
                        // set state
                        SignState = "Finish";
                        NumOfFeatureCompleted = startframe;
                        CurrectWaitingState = "完成";
                        // remove image
                        img_right.Source = null;
                        img_left.Source = null;
                        CurrentKeyFrame = -1;
                        timer_guide.Stop();
                        if (playTime == 1)
                        {
                            State = GuideState.EndPlay;
                        }
                        else
                        {
                            State = GuideState.EndPlayTwice;                     
                        }

                        return;
                    }
                    else
                    {
                        // set state
                        SignState = currentModel.KeyFrames[endframe].Type.ToString();
                        // draw arrow
                        if (currentModel.KeyFrames[endframe].Type == HandEnum.Both || currentModel.KeyFrames[endframe].Type == HandEnum.Intersect)
                        {
                            MoveArror(ref RightSignArrow, currentModel.KeyFrames[startframe].RightPosition, currentModel.KeyFrames[endframe].RightPosition);
                            MoveArror(ref LeftSignArrow, currentModel.KeyFrames[startframe].LeftPosition, currentModel.KeyFrames[endframe].LeftPosition);
                        }
                        else if (currentModel.KeyFrames[endframe].Type == HandEnum.Right)
                        {
                            MoveArror(ref RightSignArrow, currentModel.KeyFrames[startframe].RightPosition, currentModel.KeyFrames[endframe].RightPosition);
                            RemoveArrow(ref LeftSignArrow);
                        }

                        SetSampleImage(endframe);
                    }
                    if (CurrentKeyFrame != startframe)
                    {
                        CurrentKeyFrame = startframe;
                        NumOfFeatureCompleted = CurrentKeyFrame;
                        KeyFrameChange(CurrentKeyFrame+1);                        
                        MediaMain.Pause();
                        timer_guide.Stop();
                        new Thread(() =>
                        {
                            Thread.Sleep(sleepTime);
                            timer_guide.Start();
                            Application.Current.Dispatcher.Invoke(() => MediaMain.Play());
                        }).Start();

                    }
                }
            }));

        }

        private void SetSampleImage(int frame)
        {
            if (currentModel != null && currentModel.KeyFrames.Count > frame)
            {
                if (currentModel.KeyFrames[frame].Type == HandEnum.Intersect)
                {
                    img_intersect.Source = currentModel.KeyFrames[frame].RightImage;
                    img_right.Source = null;
                    img_left.Source = null;
                }
                else if (currentModel.KeyFrames[frame].Type == HandEnum.Both)
                {
                    img_right.Source = currentModel.KeyFrames[frame].RightImage;
                    img_left.Source = currentModel.KeyFrames[frame].LeftImage;
                    img_intersect.Source = null;
                }
                else if (currentModel.KeyFrames[frame].Type == HandEnum.Right)
                {
                    img_right.Source = currentModel.KeyFrames[frame].RightImage;
                    img_left.Source = null;
                    img_intersect.Source = null;
                }
            }
        }

        private static Shape DrawLinkArrow(Point p1, Point p2)
        {
            p1 = p1 * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            p2 = p2 * new Matrix(0.75, 0, 0, 0.75, 0, 0);
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            //PathGeometry pathGeometry = new PathGeometry();
            //PathFigure pathFigure = new PathFigure();
            //            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            Point p = p2;
            //pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            //LineSegment seg1 = new LineSegment();
            //seg1.Point = lpoint;
            //pathFigure.Segments.Add(seg1);

            //LineSegment seg2 = new LineSegment();
            //seg2.Point = rpoint;
            //pathFigure.Segments.Add(seg2);

            //LineSegment seg3 = new LineSegment();
            //seg3.Point = p;
            //pathFigure.Segments.Add(seg3);

            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;

            LineGeometry lGeometry = new LineGeometry();
            lGeometry.StartPoint = p;
            lGeometry.EndPoint = lpoint;
            lGeometry.Transform = transform;
            lineGroup.Children.Add(lGeometry);
            LineGeometry rGeometry = new LineGeometry();
            rGeometry.StartPoint = p;
            rGeometry.EndPoint = rpoint;
            rGeometry.Transform = transform;
            lineGroup.Children.Add(rGeometry);

            //pathGeometry.Figures.Add(pathFigure);


            //pathGeometry.Transform = transform;
            //lineGroup.Children.Add(pathGeometry);


            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            Path path = new Path { Data = lineGroup, StrokeThickness = 3 };
            path.Stroke = path.Fill = Brushes.Red;
            path.Opacity = 0.8;
            return path;
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


        private void btnSignWord_Click(object sender, RoutedEventArgs e)
        {
            KinectTileButton button = (KinectTileButton)sender;
            var dc = button.DataContext as VideoModel;
            //string videoName = String.Format("Videos\\{0}.mpg", button.DataContext.ToString());
            //viewer.Show();
            //Thread t = new Thread(new ParameterizedThreadStart(PlayVideo));
            //t.Start(dc.Path);
            currentModel = dc;
            if (dc.KeyFrames.Count > 0)
            {
                MediaMain.Source = new Uri(dc.Path,UriKind.Relative);
                CurrentKeyFrame = -1;
                NumOfFeature = dc.KeyFrames.Count - 1;
                NumOfFeatureCompleted = 0;
                State = GuideState.StartPlay;
                lbl_Name.Content = string.Format("{0}/{1} {2} 第{3}遍",currentWordCount+1,total, dc.Chinese,playTime);
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

        private void MoveArror(ref Shape s, Point from, Point to)
        {
            if (s != null)
            {
                RemoveArrow(ref s);
            }
            s = DrawLinkArrow(from, to);
            BodyPartCanvas.Children.Add(s);
        }



        private void MediaMain_OnLoaded(object sender, RoutedEventArgs e)
        {
            var s = sender as MediaElement;
            s.Play();
            s.Pause();
        }

        private void Btn_Evaluate_OnClick(object sender, RoutedEventArgs e)
        {
            //CurrentKeyFrame = -1;
            //NumOfFeatureCompleted = 0;
            //State = GuideState.StartPlay;
            //MediaMain.Position = TimeSpan.FromSeconds(0);
            //timer_guide.Start();
            CurrentKeyFrame = 0;
            State = GuideState.StartEvaluation;
            
            var j = new JObject(); 
            j["wordname"] = currentModel.ID;
            socket.SendDataAsync(j.ToString());
        }

        private void KeyFrameChange(int frame)
        {
            if (currentModel != null && currentModel.KeyFrames.Count > frame && frame >= 0)
            {
                SetSampleImage(frame);

                CurrectWaitingState = String.Format("第{0}/{1}步", frame, currentModel.KeyFrames.Count-1);
                switch (currentModel.KeyFrames[frame].Type)
                {
                    case HandEnum.Both:
                        CurrectWaitingState += "移動你的【左手】和【右手】到所示位置，并做出上圖中的手勢";
                        break;
                    case HandEnum.Intersect:
                        CurrectWaitingState += "移動你的【雙手】到所示位置，并做出上圖中的手勢";
                        break;
                    case HandEnum.Right:
                        CurrectWaitingState += "移動你的【右手】到箭頭所示位置，并做出上圖中的手勢";
                        break;
                }
            }
            else
            {
                CurrectWaitingState = String.Format("完成了");
                State = GuideState.EndGuide;
            }
            
        }

        private void Btn_Guide_OnClick(object sender, RoutedEventArgs e)
        {
            State = GuideState.StartGuide;
            var j = new JObject();
            j["label"] = "guide";
            j["wordname"] = currentModel.ID;
            socket.SendDataAsync(j.ToString());
            CurrentKeyFrame = 1;
            KeyFrameChange(CurrentKeyFrame);
            RightGuider.Stroke = Brushes.Red;
            LefttGuider.Stroke = Brushes.Red;
        }

        private class ShowFeatureMatchedPageFramesHandler : AbstractKinectFramesHandler
        {
            private BodyPartDetector detector = new BodyPartDetector();
            private PrickSignDetector prickSignDetector;
            private ShowFeatureMatchedPage showFeatureMatchedPage;
            private ReaderWriterLockSlim frameLock;
            private bool isRightHandPrimary = true;
            private Skeleton skeleton;
            private byte[] colorPixels;
            private byte[] colorPixelsToShow;
            private DepthImagePoint[] depthMap;
            private DepthImagePixel[] depthPixels;
            private System.Drawing.Point headPosition;
            private int headDepth;

            public ShowFeatureMatchedPageFramesHandler(ShowFeatureMatchedPage showFeatureMatchedPage)
            {
                this.showFeatureMatchedPage = showFeatureMatchedPage;
                this.frameLock = new ReaderWriterLockSlim();
                this.prickSignDetector = new PrickSignDetector(showFeatureMatchedPage);
                OpenCVController.GetSingletonInstance().StartDebug();
                colorPixelsToShow = new byte[4*640*480];
            }

            private HandShapeModel GenerateTest(Skeleton skl)
            {
                var model = new HandShapeModel(HandEnum.Right);
                model.right = new System.Drawing.Rectangle(0, 0, 0, 0);
                model.left = new System.Drawing.Rectangle(0, 0, 0, 0);
                model.skeletonData = FrameConverter.GetFrameDataArgString(sensor, skl);
                return model;
            }

            public override void DepthFrameCallback(long timestamp, int frameNumber, DepthImagePixel[] depthPixel)
            {
                if (depthMap == null)
                {
                    depthMap = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                }
                this.depthPixels = depthPixel;
                sensor.CoordinateMapper.MapColorFrameToDepthFrame(
                             sensor.ColorStream.Format, sensor.DepthStream.Format,
                             depthPixel,
                             this.depthMap);
            }

            public bool IsRecording = false;
            private int counter = 0;

            public override void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData)
            {
                bool isTracked = false;
                Tuple<BodyPart, BodyPart> bodyPartForHands = null;
                Point relativePosition = new Point();
                bool rightHandRaise = false;
                bool leftHandRaise = false;
                foreach (Skeleton skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        isTracked = true;
                        //bodyPartForHands = detector.decide(skeleton);
                        //prickSignDetector.Update(skeleton);

                        Joint hand1 = skeleton.Joints[isRightHandPrimary ? JointType.HandRight : JointType.HandLeft];
                        Joint hand2 = skeleton.Joints[isRightHandPrimary ? JointType.HandLeft : JointType.HandRight];
                        Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                        Joint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];
                        Joint shoulderRight = skeleton.Joints[JointType.ShoulderRight];
                        if (skeleton.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked)
                        {
                            SkeletonPoint head = skeleton.Joints[JointType.Head].Position;
                            headPosition = SkeletonPointToScreen(head);
                            try
                            {
                                headDepth = depthPixels[headPosition.X + headPosition.Y * 640].Depth;
                                // move guider
                                if (showFeatureMatchedPage.State == GuideState.StartGuide)
                                {
                                    if (showFeatureMatchedPage.CurrentKeyFrame < showFeatureMatchedPage.currentModel.KeyFrames.Count)
                                    {
                                        var keyframe = showFeatureMatchedPage.currentModel.KeyFrames[showFeatureMatchedPage.CurrentKeyFrame];
                                        var s_right = SkeletonPointToColor(shoulderRight.Position);
                                        var s_left = SkeletonPointToColor(shoulderLeft.Position);
                                        var c_head = SkeletonPointToColor(head);
                                        var hip = SkeletonPointToColor(skeleton.Joints[JointType.HipCenter].Position);
                                        var rightP = new Point()
                                        {
                                            X = keyframe.RightPositionRel.X * (s_right.X - s_left.X) + c_head.X,
                                            Y = keyframe.RightPositionRel.Y*(hip.Y - c_head.Y) + c_head.Y
                                        };
                                        var leftP = new Point()
                                        {
                                            X = keyframe.LeftPositionRel.X * (s_right.X - s_left.X) + c_head.X,
                                            Y = keyframe.LeftPositionRel.Y*(hip.Y - c_head.Y) + c_head.Y
                                        };
                                        showFeatureMatchedPage.MoveGuider(rightP,leftP);
                                    }
                                    else
                                    {
                                        showFeatureMatchedPage.State = GuideState.EndGuide;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(headPosition.X);
                                Console.WriteLine(headPosition.Y);
                                Console.WriteLine(headPosition.X + headPosition.Y * 640);
                                return;
                            }

                            
                        }
                        if (hand1.Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            rightHandRaise = true;
                        }
                        if (hand2.Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y - 0.12)
                        {
                            leftHandRaise = true;
                        }

                        if (hand1.Position.X > shoulderCenter.Position.X)
                        {
                            relativePosition.X = (hand1.Position.X - shoulderCenter.Position.X) / (shoulderRight.Position.X - shoulderCenter.Position.X);
                        }
                        else
                        {
                            relativePosition.X = -(hand1.Position.X - shoulderCenter.Position.X) / (shoulderLeft.Position.X - shoulderCenter.Position.X);
                        }

                        relativePosition.Y = 0;
                        if (colorPixels == null || depthMap == null)
                        {
                            return;
                        }
                        var handModel = OpenCVController.GetSingletonInstance()
                            .FindHandFromColor(null, colorPixels, depthMap, headPosition, headDepth, 4);
                        if (handModel == null)
                        {
                             handModel = new HandShapeModel(HandEnum.None);
                        }
                        //stop recording
                        if (IsRecording && handModel.type != HandEnum.None && !rightHandRaise && !leftHandRaise && showFeatureMatchedPage.State == GuideState.StartEvaluation)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                showFeatureMatchedPage.CurrectWaitingState = "END";
                            });
                            Console.WriteLine("END");
                            if (SocketManager.GetInstance() != null)
                            {
                                SocketManager.GetInstance().SendEndAsync();
                            }
                            IsRecording = false;
                        }
                        //start recording
                        if (!IsRecording && rightHandRaise)
                        {
                            if (showFeatureMatchedPage.State == GuideState.StartEvaluation)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    showFeatureMatchedPage.CurrectWaitingState = "RECORDING";
                                });
                            }
                            Console.WriteLine("RECORDING");
                            IsRecording = true;
                        }

                        counter++;

                        if (handModel != null && handModel.type != HandEnum.None)
                        {
                            if (handModel.intersectCenter != System.Drawing.Rectangle.Empty
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
                                //Console.WriteLine(handModel.type);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    switch (handModel.type)
                                    {
                                        case HandEnum.Right:
                                            showFeatureMatchedPage.img_guide_right.Visibility = Visibility.Visible;
                                            showFeatureMatchedPage.img_guide_left.Visibility = Visibility.Collapsed;
                                            showFeatureMatchedPage.img_guide_intersect.Visibility = Visibility.Collapsed;
                                            showFeatureMatchedPage.img_guide_right.Source = handModel.RightColor.Bitmap.ToBitmapSource();
                                            break;
                                        case HandEnum.Both:
                                            showFeatureMatchedPage.img_guide_right.Visibility = Visibility.Visible;
                                            showFeatureMatchedPage.img_guide_left.Visibility = Visibility.Visible;
                                            showFeatureMatchedPage.img_guide_intersect.Visibility = Visibility.Collapsed;
                                            showFeatureMatchedPage.img_guide_right.Source = handModel.RightColor.Bitmap.ToBitmapSource();
                                            showFeatureMatchedPage.img_guide_left.Source = handModel.LeftColor.Bitmap.ToBitmapSource();
                                            break;
                                        case HandEnum.Intersect:
                                            showFeatureMatchedPage.img_guide_right.Visibility = Visibility.Collapsed;
                                            showFeatureMatchedPage.img_guide_left.Visibility = Visibility.Collapsed;
                                            showFeatureMatchedPage.img_guide_intersect.Visibility = Visibility.Visible;
                                            showFeatureMatchedPage.img_guide_intersect.Source = handModel.RightColor.Bitmap.ToBitmapSource();
                                            break;
                                        default:
                                            return;
                                    }
                                }); 
                                handModel.skeletonData = FrameConverter.GetFrameDataArgString(sensor, skeleton);
                                if (showFeatureMatchedPage.State == GuideState.StartGuide)
                                {
                                    if (counter >= 10)
                                    {
                                        counter = 0;
                                        SocketManager.GetInstance().SendDataAsync(handModel);
                                    }
                                }
                                else
                                {
                                    SocketManager.GetInstance().SendDataAsync(handModel);
                                }
                               
                            }
                        }
                        //Console.WriteLine("tracked");
                        break;
                        //foreach (FeatureViewModel viewModel in showFeatureMatchedPage.FeatureList)
                        //{
                        //    if ("Dominant Hand X".Equals(viewModel.FeatureName))
                        //    {
                        //        viewModel.Value = relativePosition.X.ToString();
                        //    }
                        //    else if ("Dominant Hand Y".Equals(viewModel.FeatureName))
                        //    {
                        //        viewModel.Value = relativePosition.Y.ToString();
                        //    }
                        //    else if ("Region".Equals(viewModel.FeatureName))
                        //    {
                        //        viewModel.Value = bodyPartForHands.Item1.ToString();
                        //    }
                        //}
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
                    this.colorPixels = colorPixels;
                    colorPixels.CopyTo(colorPixelsToShow,0);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (showFeatureMatchedPage.PlayScreenBitmap == null)
                        {
                            showFeatureMatchedPage.PlayScreenBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
                        }

                        showFeatureMatchedPage.PlayScreenBitmap.WritePixels(new Int32Rect(0, 0, 640, 480), colorPixelsToShow, 640 * sizeof(int), 0);
                    });
                    frameLock.ExitWriteLock();
                }
            }
        }

    }

}
