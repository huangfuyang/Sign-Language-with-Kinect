// author：      Administrator
// created time：2014/1/14 15:59:58
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// Kinect SDK controller
    /// </summary>
    public class KinectSDKController : KinectController
    {

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;


        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;


        private KinectSDKController()
        {
            KinectSensor.KinectSensors.StatusChanged += Kinect_StatusChanged;
        }

        public static KinectController GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new KinectSDKController();
            }
            return singleInstance;
        }

        public override void Initialize(string uri = null)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the pixels we'll receive           
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.ColorWriteBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.DepthWriteBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);


                // Add an event handler to be called whenever there is new frame data
                this.sensor.AllFramesReady += this.AllFrameReady;
                this.Status = Properties.Resources.Connected;
               
            }

            if (null == this.sensor)
            {
                this.Status = Properties.Resources.NoKinectReady;
            }

        }


        private void Kinect_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (sensor == null)
                    {
                        sensor = e.Sensor;
                        Initialize();
                        Start();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (sensor == e.Sensor)
                    {

                        this.Status = Properties.Resources.NoKinectReady;
                        // Notify user, change state of APP appropriately
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (sensor == e.Sensor)
                    {
                        this.Status = Properties.Resources.NoKinectReady;
                        // Notify user, change state of APP appropriately
                    }
                    break;
                default:
                    // Throw exception, notify user or ignore depending on use case
                    break;
            }
        }

        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.ColorWriteBitmap.WritePixels(
                        new Int32Rect(0, 0, this.ColorWriteBitmap.PixelWidth, this.ColorWriteBitmap.PixelHeight),
                        this.colorPixels,
                        this.ColorWriteBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;
                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        int intensity = (depth >= minDepth && depth <= maxDepth ? depth : minDepth);
                        intensity = (intensity - minDepth) / (maxDepth - minDepth) * 255;
                        byte density = (byte)intensity;
                        // Write out blue byte
                        this.colorPixels[colorPixelIndex++] = density;

                        // Write out green byte
                        this.colorPixels[colorPixelIndex++] = density;

                        // Write out red byte                        
                        this.colorPixels[colorPixelIndex++] = density;

                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }
                    
                    BitmapSource depthBmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, colorPixels, width * 4);
                    Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(depthBmp.ToBitmap());
                    Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
                    using (MemStorage stor = new MemStorage())
                    {
                        //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                        Contour<System.Drawing.Point> contours = gray_image.FindContours(
                         Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                         Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                         stor);
                        int blobCount = 0;

                        for (int i = 0; contours != null; contours = contours.HNext)
                        {
                            i++;

                            if ((contours.Area > Math.Pow(10, 2)) && (contours.Area < Math.Pow(100, 2)))
                            {
                                MCvBox2D box = contours.GetMinAreaRect();
                                openCVImg.Draw(box, new Bgr(System.Drawing.Color.Red), 2);
                                blobCount++;
                            }
                        }
                    }
                    ImageConverter.UpdateWriteBMP(DepthWriteBitmap, openCVImg.ToBitmap());
                    // Write the pixel data into our bitmap
                    //this.DepthWriteBitmap.WritePixels(
                    //    new Int32Rect(0, 0, this.DepthWriteBitmap.PixelWidth, this.DepthWriteBitmap.PixelHeight),
                    //    this.colorPixels,
                    //    this.DepthWriteBitmap.PixelWidth * sizeof(int),
                    //    0);
                }
            }
        }




        public override void Shutdown()
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        public override void Start()
        {
            // Start the sensor!
            try
            {
                this.sensor.Start();
            }
            catch (IOException)
            {
                this.sensor = null;
            }
        }



    }
}