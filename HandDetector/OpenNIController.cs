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
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using OpenNIWrapper;
using NiTEWrapper;
using CURELab.SignLanguage.StaticTools;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// OpenNI wrapper
    /// </summary>
    public class OpenNIController : KinectController, ISubject
    {
        public static float SMOOTH_FACTOR = 0.3f;
        public static float BLOB_SIZE_FACTOR = 100000f; //blob size(pixels) = factor/distance(Z)

        public static double SPEED = 1;
        public static double DIFF = 50;

        private Matrix<UInt16> DepthMatrix;
        private Matrix<UInt16> PreDepthMatrix;

        private int eventDepth = 0, eventColor = 0, inlineDepth = 0, inlineColor = 0;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary
        private Bitmap colorBitmap;
        private Bitmap grayBitmap;



        private VideoStream m_colorStream;

        /// <summary>
        /// Bitmap that will hold depth information
        /// </summary
        private Bitmap depthBitmap;


        private VideoStream m_depthStream;

        private Device m_device;

        private HandTracker m_hTracker;
        private UserTracker m_uTracker;

        private System.Drawing.PointF handPos;
        private System.Drawing.PointF HeadPos;
        private float handDepth;
        private Rectangle right_hand_contour = new Rectangle();
        private float bodyDepth = 1000;

        public PlaybackControl m_playback;
        private bool m_isPause = false;

        private OpenNIController()
            : base()
        {
            ConsoleManager.Show();
            this.ColorWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            colorBitmap = new Bitmap(1, 1);
            this.DepthWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            depthBitmap = new Bitmap(1, 1);

            this.EdgeBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
            this.ProcessedDepthBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            this.GrayWriteBitmap = new WriteableBitmap(640, 480, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            grayBitmap = new Bitmap(1, 1);
            handPos = new PointF();
            HeadPos = new PointF();

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
            NiTE.Initialize();
            if (!HandleError(status)) { Environment.Exit(0); }
            OpenNI.onDeviceConnected += new OpenNI.DeviceConnectionStateChanged(OpenNI_onDeviceConnected);
            OpenNI.onDeviceDisconnected += new OpenNI.DeviceConnectionStateChanged(OpenNI_onDeviceDisconnected);
            //DeviceInfo[] devices = OpenNI.EnumerateDevices();
            m_device = Device.Open(uri); // lean init and no reset flags 
            if (uri != null)
            {
                m_playback = m_device.PlaybackControl;
            }
            //hand tracker
            m_hTracker = HandTracker.Create(m_device);
            m_hTracker.onNewData += m_hTracker_onNewData;
            //m_hTracker.StartGestureDetection(GestureData.GestureType.HAND_RAISE);
            m_hTracker.SmoothingFactor = SMOOTH_FACTOR;
            //user tracker
            m_uTracker = UserTracker.Create(m_device);
            m_uTracker.onNewData += m_uTracker_onNewData;
            m_uTracker.SkeletonSmoothingFactor = SMOOTH_FACTOR;

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

        void m_hTracker_onNewData(HandTracker uTracker)
        {
            if (!m_hTracker.isValid)
                return;
            using (HandTrackerFrameRef frame = m_hTracker.readFrame())
            {
                if (!frame.isValid)
                    return;
                /*lock (image)
                {
                    using (OpenNIWrapper.VideoFrameRef depthFrame = frame.DepthFrame)
                    {
                        if (image.Width != depthFrame.FrameSize.Width || image.Height != depthFrame.FrameSize.Height)
                            image = new Bitmap(depthFrame.FrameSize.Width, depthFrame.FrameSize.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), image.Size));
                        foreach (GestureData gesture in frame.Gestures)
                            if (gesture.isComplete)
                                hTracker.startHandTracking(gesture.CurrentPosition);
                        if (frame.Hands.Length == 0)
                            g.DrawString("Raise your hand", SystemFonts.DefaultFont, Brushes.White, 10, 10);
                        else
                            foreach (HandData hand in frame.Hands)
                            {
                                if (hand.isTracking)
                                {
                                    Point HandPosEllipse = new Point();
                                    PointF HandPos = hTracker.ConvertHandCoordinatesToDepth(hand.Position);
                                    HandPosEllipse.X = (int)HandPos.X - 5;
                                    HandPosEllipse.Y = (int)HandPos.Y - 5;
                                    g.DrawEllipse(new Pen(Brushes.White, 5), new Rectangle(HandPosEllipse, new Size(5, 5)));
                                }
                            }

                        g.Save();
                    }
                }
                this.Invoke(new MethodInvoker(delegate()
                {
                    fps = ((1000000 / (frame.Timestamp - lastTime)) + (fps * 4)) / 5;
                    lastTime = frame.Timestamp;
                    this.Text = "Frame #" + frame.FrameIndex.ToString() + " - Time: " + frame.Timestamp.ToString() + " - FPS: " + fps.ToString();
                    pb_preview.Image = image.Clone(new Rectangle(new Point(0, 0), image.Size), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }));
            */
            }
        }

        private void m_uTracker_onNewData(UserTracker uTracker)
        {
            if (!m_uTracker.isValid)
                return;
            using (UserTrackerFrameRef frame = m_uTracker.readFrame())
            {
                if (!frame.isValid)
                    return;

                foreach (UserData user in frame.Users)
                {
                    if (user.isNew && user.isVisible)
                        uTracker.StartSkeletonTracking(user.UserId);
                    SkeletonJoint joint = user.Skeleton.getJoint(SkeletonJoint.JointType.RIGHT_HAND);
                    if (user.isVisible &&
                        user.Skeleton.State == Skeleton.SkeletonState.TRACKED &&
                        joint.Position.Z > 0 &&
                        joint.PositionConfidence > 0.5)
                    {
                        handPos = m_uTracker.ConvertJointCoordinatesToDepth(joint.Position);
                        handDepth = (float)joint.Position.Z;
                        bodyDepth = (float)user.Skeleton.getJoint(SkeletonJoint.JointType.HEAD).Position.Z;
                        Console.WriteLine(bodyDepth);
                        lock (depthBitmap)
                        {
                            //right_hand_contour = m_OpenCVController.RecogBlob(handPos, (int)(BLOB_SIZE_FACTOR / handDepth), depthBitmap);
                        }
                    }
                }
            }
        }



        private void DrawHandPosition(Bitmap bitmap)
        {
            lock (bitmap)
            {
                try
                {
                    System.Drawing.Point p = new System.Drawing.Point((int)handPos.X - 3, (int)handPos.Y - 3);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawEllipse(new Pen(Brushes.White, 5),
                                      new Rectangle(p, new System.Drawing.Size(5, 5)));
                        g.Save();
                    }
                }
                catch (Exception) { }
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

                            DepthMatrix = new Matrix<UInt16>(depthBitmap.Height, depthBitmap.Width, frame.Data, frame.DataStrideBytes);
                            //opencv recognition
                            //Bitmap edgeImg = m_OpenCVController.RecogEdge(depthBitmap).ToBitmap();
                            //draw hand  
                            Bitmap CopyDepthImg = depthBitmap.Clone(new Rectangle(0, 0, depthBitmap.Width, depthBitmap.Height), depthBitmap.PixelFormat);
                            //DrawHandPosition(CopyDepthImg);
                            lock (CopyDepthImg)
                            {
                                if (PreDepthMatrix != null)
                                {
                                    FrameDiff(PreDepthMatrix, DepthMatrix, CopyDepthImg);
                                }
                            }
                            PreDepthMatrix = DepthMatrix;
                            //AsyncCombineImage(ProcessedDepthBitmap, CopyDepthImg, edgeImg);
                            AsyncUpdateImage(ProcessedDepthBitmap, CopyDepthImg);

                        }

                        //draw edge
                        //AsyncUpdateImage(ProcessedBitmap, CopyDepthImg);

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
                            DrawHandContour(colorBitmap);
                            //EraseBackground(colorBitmap);

                            lock (grayBitmap)
                            {
                                grayBitmap = m_OpenCVController.Color2Edge(handPos, (int)BLOB_SIZE_FACTOR / 1000, colorBitmap);
                            }
                        }

                        AsyncUpdateImage(ColorWriteBitmap, colorBitmap);
                        AsyncUpdateImage(GrayWriteBitmap, grayBitmap);
                        //if (cb_mirrorSoft.Checked)
                        //    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        SetSpeed(SPEED);
                    }
                }
            }
        }
        private void EraseBackground(Bitmap cbmp)
        {
            lock (depthBitmap)
            {
                Bitmap dbmp = depthBitmap;
                System.Drawing.Imaging.BitmapData cbmpData;
                System.Drawing.Imaging.BitmapData dbmpData;
                Rectangle rect = new Rectangle(0, 0, cbmp.Width, cbmp.Height);
                cbmpData = cbmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, cbmp.PixelFormat);
                dbmpData = dbmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, dbmp.PixelFormat);
                int cstride = cbmpData.Stride;
                int dstride = dbmpData.Stride;
                int step = dstride / dbmp.Width;
                try
                {
                    unsafe
                    {
                        byte* ptr = (byte*)cbmpData.Scan0;
                        byte* dptr = (byte*)dbmpData.Scan0;
                        for (int y = 0; y < cbmp.Height; y++)
                        {
                            for (int x = 0; x < cbmp.Width; x++)
                            {
                                if (dptr[x * step + y * dstride] > bodyDepth + 5 || dptr[x * step + y * dstride] == 0)
                                {
                                    ptr[x * 3 + y * cstride] = 255;
                                    ptr[x * 3 + y * cstride + 1] = 255;
                                    ptr[x * 3 + y * cstride + 2] = 255;
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
                finally
                {
                    cbmp.UnlockBits(cbmpData);
                    dbmp.UnlockBits(dbmpData);
                }
            }

        }

        private void FrameDiff(Matrix<UInt16> preMat, Matrix<UInt16> newMat, Bitmap bmpNew)
        {
            System.Drawing.Imaging.BitmapData nbmpData;
            Rectangle rect = new Rectangle(0, 0, bmpNew.Width, bmpNew.Height);
            nbmpData = bmpNew.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmpNew.PixelFormat);
            int stride = nbmpData.Stride;
            int step = stride / bmpNew.Width;
            try
            {
                unsafe
                {
                    byte* nptr = (byte*)nbmpData.Scan0;
                    fixed (UInt16* pM1 = &newMat.Data[0, 0])
                    {
                        fixed (UInt16* pM2 = &preMat.Data[0, 0])
                        {
                            for (int y = 0; y < bmpNew.Height; y++)
                                for (int x = 0; x < bmpNew.Width; x++)
                                {
                                    UInt16 a = pM1[y * 640 + x];
                                    UInt16 b = pM2[y * 640 + x];
                                    //if (a != 0)
                                    //{ Console.WriteLine(a); }

                                    //int b = preMat.Data[x, y];
                                    if (a > 0 &&
                                        a < bodyDepth + 100 &&
                                         Math.Abs(a - b) > DIFF)
                                    {
                                        nptr[x * 3 + y * stride] = 255;
                                        nptr[x * 3 + y * stride + 1] = 255;
                                        nptr[x * 3 + y * stride + 2] = 255;
                                    }
                                }
                        }
                    }
                    //UInt16* pM1 = (UInt16*)newMat.Data;
                    //UInt16* pM2 = (UInt16*)preMat.Ptr;

                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            finally
            {
                bmpNew.UnlockBits(nbmpData);
            }

        }

        private void DrawHandContour(Bitmap bmp)
        {

            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawRectangle(new Pen(Brushes.White, 2), right_hand_contour);
                    g.Save();
                }
            }
            catch (Exception e) { }


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


        public void AsyncUpdateImage(WriteableBitmap wbmp, Bitmap bmp)
        {
            ColorWriteBitmap.Dispatcher.BeginInvoke(
               new Action(() => UpdateImage(wbmp, bmp)
           ));

        }

        public void AsyncCombineImage(WriteableBitmap wbmp, Bitmap bmp, Bitmap edge)
        {
            ColorWriteBitmap.Dispatcher.BeginInvoke(
               new Action(() => CombineImage(wbmp, bmp, edge)
           ));

        }

        private void CombineImage(WriteableBitmap wbmp, Bitmap bmp, Bitmap edge)
        {
            lock (bmp)
            {
                lock (edge)
                {
                    System.Drawing.Imaging.BitmapData bmpData;
                    System.Drawing.Imaging.BitmapData edgeData;

                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                    edgeData = edge.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, edge.PixelFormat);
                    int stride = bmpData.Stride;
                    int strideEdge = edgeData.Stride;
                    try
                    {
                        unsafe
                        {
                            if (handDepth > 500)
                            {
                                byte* ptr = (byte*)bmpData.Scan0;
                                byte* ptrE = (byte*)edgeData.Scan0;
                                int radius = (int)(BLOB_SIZE_FACTOR / handDepth);
                                radius /= 2;
                                int s_y = (int)handPos.Y - radius > 0 ? (int)handPos.Y - radius : 0;
                                int s_x = (int)handPos.X - radius > 0 ? (int)handPos.X - radius : 0;
                                int e_x = (int)handPos.X + radius > bmp.Width ? bmp.Width : (int)handPos.X + radius;
                                int e_y = (int)handPos.Y + radius > bmp.Height ? bmp.Height : (int)handPos.Y + radius;
                                for (int y = s_y; y < e_y; y++)
                                {
                                    for (int x = s_x; x < e_x; x++)
                                    {
                                        if (ptrE[x + y * strideEdge] != 0)
                                        {
                                            ptr[(x * 3) + y * stride] = 255;
                                            ptr[(x * 3) + y * stride + 1] = 255;
                                            ptr[(x * 3) + y * stride + 2] = 255;
                                        }
                                    }
                                }
                            }

                        }

                        wbmp.Lock();
                        wbmp.WritePixels(
                                              new Int32Rect(0, 0, wbmp.PixelWidth, wbmp.PixelHeight),
                                              bmpData.Scan0,
                                              bmpData.Width * bmpData.Height * 3,
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
                        edge.UnlockBits(edgeData);
                    }
                }
            }
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
                      bmpData.Width * bmpData.Height * 3,
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
        public override void TogglePause()
        {
            if (m_playback != null)
            {
                m_isPause = !m_isPause;
                m_playback.Speed = m_isPause ? -1.0f : 1.0f;
            }
        }

        public override void SetSpeed(double speed)
        {
            if (m_playback != null)
            {
                m_playback.Speed = (float)speed;
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