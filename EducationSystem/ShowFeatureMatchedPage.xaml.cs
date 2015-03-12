using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private ShowFeatureMatchedPageFramesHandler framesHandler;

        public ShowFeatureMatchedPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.framesHandler = new ShowFeatureMatchedPageFramesHandler(this);
            this.framesHandler.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
            KinectState.Instance.KinectRegion.IsCursorVisible = false;
        }

        private class ShowFeatureMatchedPageFramesHandler : AbstractKinectFramesHandler
        {
            private ShowFeatureMatchedPage showFeatureMatchedPage;
            private ReaderWriterLockSlim frameLock;

            public ShowFeatureMatchedPageFramesHandler(ShowFeatureMatchedPage showFeatureMatchedPage)
            {
                this.showFeatureMatchedPage = showFeatureMatchedPage;
                this.frameLock = new ReaderWriterLockSlim();
            }

            public override void SkeletonFrameCallback(long timestamp, int frameNumber, Microsoft.Kinect.Skeleton[] skeletonData)
            {

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
    }
}
