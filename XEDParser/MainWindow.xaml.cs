using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using Emgu.CV;
using Emgu.CV.Structure;

namespace XEDParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.28f;

        private KinectSensor _currentKinectSensor;
        public KinectSensor CurrentKinectSensor
        {
            get { return _currentKinectSensor; }
            set { _currentKinectSensor = value; }
        }
        private string _depthframe;
        public string DepthFrame 
        { 
            get { return _depthframe; }
            set { _depthframe = value;
                lbl_Depth.Content = value; }
        }
        private string _colorframe;
        public string ColorFrame
        {
            get { return _colorframe; }
            set
            {
                _colorframe = value;
                lbl_Color.Content = value;
            }
        }

        public List<Image<Bgr, byte>> ColorFrameList;
        public List<Image<Bgr, byte>> DepthFrameList;
        private KinectSensorChooser sensorChooser;
        VideoWriter colorWriter = null;
        long colorFirstTime = 0;
        VideoWriter depthWriter = null;
        StreamWriter skeWriter = null;
        long depthFirstTime = 0;

        Colorizer colorizer;
        public MainWindow()
        {
            InitializeComponent();
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            DepthFrame = "0";
           
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            bool error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException) { error = true; }
            }

            if (args.NewSensor != null)
            {
                CurrentKinectSensor = args.NewSensor;
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    colorWriter = new VideoWriter("c.avi", 30, 640, 480, true);
                    depthWriter = new VideoWriter("d.avi", 30, 640, 480, true);
                    depthPixels = new DepthImagePixel[CurrentKinectSensor.DepthStream.FramePixelDataLength];
                    _colorPixels = new byte[CurrentKinectSensor.ColorStream.FramePixelDataLength];
                    try
                    {
                        
                        //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Switch back to normal mode if Kinect does not support near mode
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }
            else
            {
                error = true;
            }


        }
       

        private byte[] _colorPixels;
        private DepthImagePixel[] depthPixels;
        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                if (sFrame != null)
                {
                    var skeletons = new Skeleton[sFrame.SkeletonArrayLength];
                    sFrame.CopySkeletonDataTo(skeletons);
                    Skeleton skel = skeletons[0];
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {

                    }
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    ColorFrame = colorFrame.FrameNumber.ToString();
                    if (colorFirstTime == 0)
                    {
                        colorFirstTime = colorFrame.Timestamp;
                    }
                 
                    colorFrame.CopyPixelDataTo(this._colorPixels);
                    var img = ImageConverter.Array2Image(_colorPixels, 640, 480, 640 * 4);
                    var time = colorFrame.Timestamp - colorFirstTime;
                    if (img.Ptr != IntPtr.Zero)
                    {
                        colorWriter.WriteFrame(img.Convert<Bgr, byte>());
                    }
                }
            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    DepthFrame = depthFrame.FrameNumber.ToString();
                    
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                    //int minDepth = depthFrame.MinDepth;
                    //int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;

                

                    colorizer.TransformAndConvertDepthFrame(depthPixels, _colorPixels);

                    Image<Bgra, byte> depthImg;
                    depthImg = ImageConverter.Array2Image(_colorPixels, width, height, width * 4);
                    if (depthImg.Ptr != IntPtr.Zero)
                    {
                        depthWriter.WriteFrame(depthImg.Convert<Bgr, byte>());
                    }


                }
                
            }

      

            
        }
     

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (colorWriter != null)
            {
                colorizer = new Colorizer(AaronRotateTan,CurrentKinectSensor.DepthStream.MaxDepth,CurrentKinectSensor.DepthStream.MinDepth);
                CurrentKinectSensor.AllFramesReady += AllFrameReady;
            }
        }

        private void btn_end_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentKinectSensor.AllFramesReady -= AllFrameReady;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CloseAllWriter();
        }

        private void CloseAllWriter()
        {
            if (colorWriter != null)
            {
                colorWriter.Dispose();
            }
            if (depthWriter != null)
            {
                depthWriter.Dispose();
            }
        }

        private string GenerateSkeletonArgs(Skeleton s)
        {
            return null;
        }

        private System.Drawing.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }

    }
}
