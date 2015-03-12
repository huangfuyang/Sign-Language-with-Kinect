using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

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
            get { return BodyPart; }
            set { SetValue(BodyPartProperty, value); }
        }

        private ShowFeatureMatchedPageFramesHandler framesHandler;

        public ShowFeatureMatchedPage()
        {
            InitializeComponent();
        }

        public ObservableCollection<FeatureViewModel> FeatureList { get; set; }


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
        }

        private class ShowFeatureMatchedPageFramesHandler : AbstractKinectFramesHandler
        {
            private BodyPartDetector detector = new BodyPartDetector();
            private ShowFeatureMatchedPage showFeatureMatchedPage;
            private ReaderWriterLockSlim frameLock;
            private bool isRightHandPrimary = true;

            public ShowFeatureMatchedPageFramesHandler(ShowFeatureMatchedPage showFeatureMatchedPage)
            {
                this.showFeatureMatchedPage = showFeatureMatchedPage;
                this.frameLock = new ReaderWriterLockSlim();
            }

            public override void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData)
            {
                foreach (Skeleton skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Tuple<BodyPart, BodyPart> bodyPartForHands = detector.decide(skeleton);

                        Joint hand1 = skeleton.Joints[isRightHandPrimary ? JointType.HandRight : JointType.HandLeft];
                        Joint hand2 = skeleton.Joints[isRightHandPrimary ? JointType.HandLeft : JointType.HandRight];
                        Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                        Joint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];
                        Joint shoulderRight = skeleton.Joints[JointType.ShoulderRight];
                        //Joint spine = skeleton.Joints[JointType.Spine];
                        Point relativePosition = new Point();

                        if (hand1.Position.X > shoulderCenter.Position.X)
                        {
                            relativePosition.X = (hand1.Position.X - shoulderCenter.Position.X) / (shoulderRight.Position.X - shoulderCenter.Position.X);
                        }
                        else
                        {
                            relativePosition.X = -(hand1.Position.X - shoulderCenter.Position.X) / (shoulderLeft.Position.X - shoulderCenter.Position.X);
                        }

                        foreach (FeatureViewModel viewModel in showFeatureMatchedPage.FeatureList)
                        {
                            if ("Dominant Hand X".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = relativePosition.X.ToString();
                                Console.WriteLine(relativePosition.X);
                            }
                            else if ("Dominant Hand X".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = relativePosition.Y.ToString();
                                Console.WriteLine(relativePosition.Y);
                            }
                            else if ("Region".Equals(viewModel.FeatureName))
                            {
                                viewModel.Value = bodyPartForHands.Item1.ToString();
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            showFeatureMatchedPage.DominantHandPointLeft = 421 + (int)(relativePosition.X * (467 - 375) / 2);
                            //showFeatureMatchedPage.DominantHandPointTop = 421 + (int)(relativePosition.Y * (467 - 375) / 2);
                            showFeatureMatchedPage.FeatureDataGrid.Items.Refresh();
                            showFeatureMatchedPage.BodyPart = bodyPartForHands.Item1.ToString();
                        });
                    }
                }
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
    }
}
