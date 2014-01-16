// author：      Administrator
// created time：2014/1/8 15:34:27
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenNIWrapper;
using CURELab.SignLanguage.StaticTools;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// OpenNI wrapper
    /// </summary>
    public class OpenNIController : KinectController,ISubject
    {
        private int eventDepth = 0, eventColor = 0, inlineDepth = 0, inlineColor = 0;
       
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary
        private Bitmap colorBitmap;
 
        private VideoStream m_colorStream;

        /// <summary>
        /// Bitmap that will hold depth information
        /// </summary
        private Bitmap depthBitmap;

        private VideoStream m_depthStream;

        private Device m_device;


        private OpenNIController()
        {
            ConsoleManager.Show();
            this.ColorWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr24, null);
            colorBitmap = new Bitmap(1, 1);
            this.DepthWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr24, null);
            depthBitmap = new Bitmap(1, 1);
        }

        public static KinectController GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new OpenNIController();
            }
            return singleInstance;
        }

        public override void Initialize(string uri = null)
        {
            OpenNI.Status status;
            Console.WriteLine(OpenNI.Version.ToString());
            status = OpenNI.Initialize();
            if (!HandleError(status)) { Environment.Exit(0); }
            OpenNI.onDeviceConnected += new OpenNI.DeviceConnectionStateChanged(OpenNI_onDeviceConnected);
            OpenNI.onDeviceDisconnected += new OpenNI.DeviceConnectionStateChanged(OpenNI_onDeviceDisconnected);
            //DeviceInfo[] devices = OpenNI.EnumerateDevices();
            m_device = Device.Open(uri); // lean init and no reset flags           
            SensorInfo sensorInfo = m_device.getSensorInfo(Device.SensorType.DEPTH);
        
            if (m_device.hasSensor(Device.SensorType.DEPTH) &&
                m_device.hasSensor(Device.SensorType.COLOR))
            {

                m_depthStream = m_device.CreateVideoStream(Device.SensorType.DEPTH);
                m_colorStream = m_device.CreateVideoStream(Device.SensorType.COLOR);
                if (m_depthStream.isValid && m_colorStream.isValid)
                {
                    //new System.Threading.Thread(new System.Threading.ThreadStart(DisplayInfo)).Start();
                    m_depthStream.onNewFrame += depthStream_onNewFrame;
                    m_colorStream.onNewFrame += colorStream_onNewFrame;

                }
                if (uri == null)
                {
                    this.Status = Properties.Resources.ConnectedOpenNI;
                }
                else
                {
                    this.Status = Properties.Resources.ONIFile;
                }

            }
        }

        public override void Start()
        {
            if (!HandleError(m_depthStream.Start())) { OpenNI.Shutdown(); return; }
            if (!HandleError(m_colorStream.Start())) { OpenNI.Shutdown(); return; }

        }



        public override void Shutdown()
        {
            m_colorStream.Stop();
            m_depthStream.Stop();
            m_device.Close();
            OpenNI.Shutdown();
        }

        private bool HandleError(OpenNI.Status status)
        {
            if (status == OpenNI.Status.OK)
                return true;
            Console.WriteLine("Error: " + status.ToString() + " - " + OpenNI.LastError);
            Console.ReadLine();
            return false;
        }


        private void depthStream_onNewFrame(VideoStream vStream)
        {
            if (vStream.isValid && vStream.isFrameAvailable())
            {
                using (VideoFrameRef frame = vStream.readFrame())
                {
                    if (frame.isValid)
                    {
                        VideoFrameRef.copyBitmapOptions options = VideoFrameRef.copyBitmapOptions.Force24BitRGB | VideoFrameRef.copyBitmapOptions.DepthFillShadow;

                        /////////////////////// Instead of creating a bitmap object for each frame, you can simply
                        /////////////////////// update one you have. Please note that you must be very careful 
                        /////////////////////// with multi-thread situations.
                        lock (depthBitmap)
                        {
                            try
                            {
                                frame.updateBitmap(depthBitmap, options);
                            }
                            catch (Exception) // Happens when our Bitmap object is not compatible with returned Frame
                            {
                                depthBitmap = frame.toBitmap(options);
                            }
                        }
                        AsyncUpdateImage(DepthWriteBitmap,depthBitmap);

                        //if (cb_mirrorSoft.Checked)
                        //    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);


                    }
                }
            }
        }
        private void colorStream_onNewFrame(VideoStream vStream)
        {
            if (vStream.isValid && vStream.isFrameAvailable())
            {
                using (VideoFrameRef frame = vStream.readFrame())
                {
                    if (frame.isValid)
                    {
                        VideoFrameRef.copyBitmapOptions options = VideoFrameRef.copyBitmapOptions.Force24BitRGB | VideoFrameRef.copyBitmapOptions.DepthFillShadow;

                        /////////////////////// Instead of creating a bitmap object for each frame, you can simply
                        /////////////////////// update one you have. Please note that you must be very careful 
                        /////////////////////// with multi-thread situations.
                        lock (colorBitmap)
                        {
                            try
                            {
                                frame.updateBitmap(colorBitmap, options);
                            }
                            catch (Exception) // Happens when our Bitmap object is not compatible with returned Frame
                            {
                                colorBitmap = frame.toBitmap(options);
                            }
                        }
                        AsyncUpdateImage(ColorWriteBitmap,colorBitmap);

                        //if (cb_mirrorSoft.Checked)
                        //    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);


                    }
                }
            }
        }

        private void OpenNI_onDeviceDisconnected(DeviceInfo Device)
        {
            Console.WriteLine(Device.Name + " Disconnected ...");
        }

        private void OpenNI_onDeviceConnected(DeviceInfo Device)
        {
            Console.WriteLine(Device.Name + " Connected ...");
        }

        private int lUpdate;

        private void DisplayInfo()
        {
            while (true)
            {
                if (lUpdate == 0)
                {
                    lUpdate = Environment.TickCount;
                    continue;
                }
                if (Environment.TickCount - lUpdate > 1000)
                {
                    lUpdate = Environment.TickCount;
                    Console.Clear();
                    Console.WriteLine("Inline Depth: " + inlineDepth.ToString() + " - Inline Color: " + inlineColor.ToString() +
                        " - Event Depth: " + eventDepth.ToString() + " - Event Color: " + eventColor.ToString());
                    inlineDepth = inlineColor = eventDepth = eventColor = 0;
                }
                else
                    continue;
                System.Threading.Thread.Sleep(100);
            }
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        private WriteableBitmap BitmapToWriteableBitmap(Bitmap bmp)
        {
            WriteableBitmap wbmp;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                wbmp = new WriteableBitmap(image);
            }
            return wbmp;

        }

        public void AsyncUpdateImage(WriteableBitmap wbmp, Bitmap bmp)
        {
            ColorWriteBitmap.Dispatcher.BeginInvoke(
               new Action(() => UpdateImage(wbmp, bmp)
           ));

        }

        private void UpdateImage(WriteableBitmap wbmp, Bitmap bmp)
        {

            lock (bmp)
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

                try
                {
                    wbmp.Lock();

                    wbmp.WritePixels(
                      new Int32Rect(0, 0, wbmp.PixelWidth, wbmp.PixelHeight),
                      bmpData.Scan0,
                      bmpData.Width * bmpData.Height * 4,
                      bmpData.Stride);
                    wbmp.Unlock();


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    bmp.UnlockBits(bmpData);
                }
            }


        }




        #region ISubject 成员

        public event DataTransferEventHandler m_dataTransferEvent;

        public void NotifyAll(DataTransferEventArgs e)
        {
            if (m_dataTransferEvent != null)
            {
                m_dataTransferEvent(this, e);
            }
            else
            {
                Console.WriteLine("no boundler");
            }
        }


        #endregion
    }
}