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
        private KinectSensor _currentKinectSensor;
        public KinectSensor CurrentKinectSensor
        {
            get { return _currentKinectSensor; }
            set { _currentKinectSensor = value; }
        }
        private KinectSensorChooser sensorChooser;
        VideoWriter colorWriter = null;
        long colorFirstTime = 0;
        VideoWriter depthWriter = null;
        StreamWriter skeWriter = null;
        long depthFirstTime = 0;
        public MainWindow()
        {
            InitializeComponent();
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            //this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

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
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    colorWriter = new VideoWriter("t.avi", 30, 640, 480, true);

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

                    CurrentKinectSensor = args.NewSensor;
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
        void NewSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {


        }

        void NewSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private byte[] _colorPixels;
        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (colorFirstTime == 0)
                    {
                        colorFirstTime = colorFrame.Timestamp;
                    }
                    _colorPixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(this._colorPixels);
                    var img = ImageConverter.Array2Image(_colorPixels, 640, 480, 640 * 4);
                    var time = colorFrame.Timestamp - colorFirstTime;
                    if (img.Ptr != IntPtr.Zero)
                    {
                        colorWriter.WriteFrame(img.Convert<Bgr, byte>());
                    }
                }
            }
        }
        void NewSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (colorFirstTime == 0)
                    {
                        colorFirstTime = colorFrame.Timestamp;
                    }
                    _colorPixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(this._colorPixels);
                    var img = ImageConverter.Array2Image(_colorPixels, 640, 480, 640 * 4);
                    var time = colorFrame.Timestamp - colorFirstTime;
                    if (img.Ptr != IntPtr.Zero)
                    {
                        colorWriter.WriteFrame(img.Convert<Bgr,byte>());
                    }
                }
            }
        }


        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (colorWriter != null)
            {
                //CurrentKinectSensor.ColorFrameReady += NewSensor_ColorFrameReady;
                CurrentKinectSensor.AllFramesReady += AllFrameReady;
                
            }
            if (depthWriter != null)
            {
                CurrentKinectSensor.DepthFrameReady += NewSensor_DepthFrameReady;
            }
            if (skeWriter != null)
            {
                CurrentKinectSensor.SkeletonFrameReady += NewSensor_SkeletonFrameReady;
            }
        }

        private void btn_end_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                CurrentKinectSensor.ColorFrameReady -= NewSensor_ColorFrameReady;
                CurrentKinectSensor.DepthFrameReady -= NewSensor_DepthFrameReady;
                CurrentKinectSensor.SkeletonFrameReady -= NewSensor_SkeletonFrameReady;
                colorWriter.Dispose();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
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


    }
}
