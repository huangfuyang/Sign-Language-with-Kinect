using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for GamePlayPage.xaml
    /// </summary>
    public partial class GamePlayPage : Page
    {
        private bool _isRecordingUserAction = false;
        public bool IsRecordingUserAction
        {
            get { return _isRecordingUserAction; }
        }

        private WriteableBitmap _playScreenBitmap;
        public WriteableBitmap PlayScreenBitmap
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _playScreenBitmap; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { PlayScreenImage.Source = _playScreenBitmap = value; }
        }

        private string _videoSource = "Data/Videos/HKG_001_a_0001 Aaron 11_c.avi";
        public string VideoSource
        {
            get { return _videoSource; }
            set { this._videoSource = value; }
        }

        public Visibility PlayScreenImageVisibility
        {
            get { return IsRecordingUserAction ? Visibility.Visible : Visibility.Hidden; }
        }

        public Visibility VideoScreenVisibility
        {
            get { return IsRecordingUserAction ? Visibility.Hidden : Visibility.Visible; }
        }

        private GamePlayFramesHandler framesHandler;
        private int repeatTime = 0;

        public GamePlayPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.framesHandler = new GamePlayFramesHandler(this);
            this.framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
            this.VideoScreen.Play();
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _isRecordingUserAction = true;
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);

            timer.Interval = new TimeSpan(0, 0, 3);
            timer.Tick += (object sender1, EventArgs e1) =>
             {
                 lblScore.Content = Convert.ToUInt32(lblScore.Content) + new Random().Next(10000);
                 _isRecordingUserAction = false;

                 PlayScreenImage.Visibility = PlayScreenImageVisibility;
                 VideoScreen.Visibility = VideoScreenVisibility;
                 timer.Stop();

                 if ((++repeatTime) < 3)
                 {
                     this.VideoScreen.Position = new TimeSpan(0, 0, 0);
                     this.VideoScreen.Play();
                 }
                 else
                 {
                     GameOverPage gameOverPage = new GameOverPage();
                     this.NavigationService.Navigate(gameOverPage);
                 }
             };
            timer.Start();
        }

        private class GamePlayFramesHandler : AbstractKinectFramesHandler
        {

            private GamePlayPage gamePlayPage;
            private ReaderWriterLockSlim frameLock;

            public GamePlayFramesHandler(GamePlayPage gamePlayPage)
            {
                this.gamePlayPage = gamePlayPage;
                this.frameLock = new ReaderWriterLockSlim();
            }

            public override void SkeletonFrameCallback(long timestamp, int frameNumber, Microsoft.Kinect.Skeleton[] skeletonData)
            {

            }

            public override void DepthFrameCallback(long timestamp, int frameNumber, Microsoft.Kinect.DepthImagePixel[] depthPixels)
            {

            }

            public override void ColorFrameCallback(long timestamp, int frameNumber, byte[] colorPixels)
            {
                if (gamePlayPage.IsRecordingUserAction && colorPixels != null && colorPixels.Length > 0)
                {
                    frameLock.EnterWriteLock();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (gamePlayPage.PlayScreenBitmap == null)
                        {
                            gamePlayPage.PlayScreenBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
                        }

                        gamePlayPage.PlayScreenBitmap.WritePixels(new Int32Rect(0, 0, 640, 480), colorPixels, 640 * sizeof(int), 0);
                        gamePlayPage.PlayScreenImage.Visibility = gamePlayPage.PlayScreenImageVisibility;
                        gamePlayPage.VideoScreen.Visibility = gamePlayPage.VideoScreenVisibility;
                    });
                    frameLock.ExitWriteLock();
                }
            }

            public override void HandPointersCallback(long timestamp, Microsoft.Kinect.Toolkit.Controls.HandPointer[] handPointers)
            {

            }
        }
    }
}
