using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace CURELab.SignLanguage.HandDetector
{
    public class KinectRealtime : KinectSDKController
    {
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        private bool IsInitialized = false;
        private SocketManager socket;
        private Label label;
        private KinectRealtime(SocketManager _socket, Label _label)
            : base()
        {
            try
            {
                socket = _socket;
                label = _label;
                ShowFinal = true;
                IsInitialized = false;
            }
            catch (Exception)
            {
                Console.WriteLine("not connected");
            }
        }

        public static KinectController GetSingletonInstance(SocketManager socket, Label label)
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectRealtime(socket,label);
            }
            return singleInstance;
        }


        public override void ChangeSensor(KinectSensor _sensor)
        {
            base.ChangeSensor(_sensor);
            if (IsInitialized)
            {
                sensor.AllFramesReady += AllFrameReady;
            }
        }

        public override void Initialize(KinectSensor _sensor)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            sensor = _sensor;


            if (null != sensor && !IsInitialized)
            {
                // Turn on the color stream to receive color frames
                //this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(sensor);
                //this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);
                //this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;
                //sensor.DepthStream.Range = DepthRange.Near;

                m_extractor.Initialize(sensor);
                // This is the bitmap we'll display on-screen
                this.ColorWriteBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
                this.DepthWriteBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr32, null);
                // Add an event handler to be called whenever there is new frame data
                this.Status = Properties.Resources.Connected;
                IsInitialized = true;
            }

            if (null == sensor)
            {
                this.Status = Properties.Resources.NoKinectReady;
            }

        }


        protected override void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {

            byte[] processImg;
            var handModel = m_extractor.ProcessAllFrame(e, out processImg);
            if (processImg != null)
            {
                this.DepthWriteBitmap.WritePixels(
                    new Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth,
                        this.DepthWriteBitmap.PixelHeight),
                    processImg,
                    this.DepthWriteBitmap.PixelWidth * sizeof(int),
                    0);
            }

            if (handModel != null && handModel.type != HandEnum.None)
            {
                socket.SendDataAsync(handModel);
            }

        }


        public override void Start()
        {
            try
            {
                if (IsInitialized && sensor != null)
                {
                    sensor.AllFramesReady += AllFrameReady;
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Stop()
        {
            base.Stop();
            if (sensor != null)
            {
                sensor.AllFramesReady -= AllFrameReady;
            }
        }

        ///// <summary>
        ///// Handle the background removed color frame ready event. The frame obtained from the background removed
        ///// color stream is in RGBA format.
        ///// </summary>
        ///// <param name="sender">object that sends the event</param>
        ///// <param name="e">argument of the event</param>
        //private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        //{
        //    using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
        //    {
        //        if (backgroundRemovedFrame != null)
        //        {
        //            backgroundRemovedFrame.CopyPixelDataTo(colorPixels);
        //            // Write the pixel data into our bitmap
        //            this.ColorWriteBitmap.WritePixels(
        //                new Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
        //                colorPixels,
        //                this.ColorWriteBitmap.PixelWidth * sizeof(int),
        //                0);
        //        }
        //    }
        //}



    }
}
