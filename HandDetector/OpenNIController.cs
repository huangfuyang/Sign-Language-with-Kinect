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
        public static double Time = 0;
        private double tempSpeed = 1;
        public static double ANGLE_TRANSFORM = Math.PI / 9;

        private Matrix<UInt16> DepthMatrix;
        private Matrix<UInt16> PreDepthMatrix;

        private int eventDepth = 0, eventColor = 0, inlineDepth = 0, inlineColor = 0;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary
        private Bitmap colorBitmap;
        private Bitmap grayBitmap;

        /* lagency 

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

        */
        private OpenNIController()
            : base()
        {
            //ConsoleManager.Show();
            //this.ColorWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            //colorBitmap = new Bitmap(1, 1);
            //this.DepthWriteBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            //depthBitmap = new Bitmap(1, 1);

            //this.EdgeBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
            //this.ProcessedDepthBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            //this.GrayWriteBitmap = new WriteableBitmap(640, 480, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            //grayBitmap = new Bitmap(1, 1);
            //handPos = new PointF();
            //HeadPos = new PointF();

        }

        public static KinectController GetSingletonInstance()
        {
            return singleInstance ?? (singleInstance = new OpenNIController());
        }

        /*
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
                lock (image)
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
                    SkeletonJoint jointHead = user.Skeleton.getJoint(SkeletonJoint.JointType.HEAD);
                    if (user.isVisible &&
                        user.Skeleton.State == Skeleton.SkeletonState.TRACKED &&
                        joint.Position.Z > 0 &&
                        joint.PositionConfidence > 0.5)
                    {
                        //if (handPos.X == 0 && handPos.Y == 0)
                            handPos = m_uTracker.ConvertJointCoordinatesToDepth(joint.Position);
                        HeadPos = m_uTracker.ConvertJointCoordinatesToDepth(jointHead.Position);
                        handDepth = (float)joint.Position.Z;
                        bodyDepth = (float)user.Skeleton.getJoint(SkeletonJoint.JointType.HEAD).Position.Z;
                        //Console.WriteLine(handDepth);
                        //lock (depthBitmap)
                        //{
                        //    right_hand_contour = m_OpenCVController.RecogBlob(handPos, (int)(BLOB_SIZE_FACTOR / handDepth), depthBitmap);
                        //}
                    }
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


                            DepthMatrix = new Matrix<UInt16>(depthBitmap.Height, depthBitmap.Width, frame.Data, frame.DataStrideBytes);
                            lock (DepthMatrix)
                            {
                                DepthMatrix = TransformDepth(DepthMatrix, OpenNIController.ANGLE_TRANSFORM);
                            }
                            //opencv recognition
                            //Bitmap edgeImg = m_OpenCVController.RecogEdge(depthBitmap).ToBitmap();
                            //draw hand  
                            Bitmap CopyDepthImg = depthBitmap.Clone(new Rectangle(0, 0, depthBitmap.Width, depthBitmap.Height), depthBitmap.PixelFormat);
                            lock (CopyDepthImg)
                            {
                                if (PreDepthMatrix != null)
                                {
                                    //FrameDiff(PreDepthMatrix, DepthMatrix, CopyDepthImg);
                                    //RegionGrow(HeadPos, DepthMatrix, CopyDepthImg);
                                    RegionGrow(handPos, DepthMatrix, CopyDepthImg);
                                }
                            }
                            //DrawHandPosition(CopyDepthImg);
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

        private Matrix<UInt16> TransformDepth(Matrix<UInt16> mat, double angle)
        {
            try
            {
                unsafe
                {
                    UInt16[,] result = new ushort[mat.Height, mat.Width];

                    fixed (UInt16* pM = &mat.Data[0, 0])
                    {
                        for (int y = 0; y < mat.Height; y++)
                        {
                            for (int x = 0; x < mat.Width; x++)
                            {
                                if (pM[y * 640 + x] == 0)
                                {
                                    continue;
                                }

                                int theta = (240 - y) / 3;
                                double depth = pM[y * 640 + x] * Math.Sin(Math.PI / 2 - angle) - theta * Math.Sin(angle);

                                result[y, x] = Convert.ToUInt16(depth);
                            }


                        }
                    }
                    return new Matrix<ushort>(result);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                return null;
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
                            //DrawHandContour(colorBitmap);
                            if (DepthMatrix != null)
                            {
                                lock (DepthMatrix)
                                {
                                    EraseBackground(DepthMatrix, colorBitmap);
                                }
                            }


                            lock (grayBitmap)
                            {
                                grayBitmap = m_OpenCVController.Color2Edge(handPos, (int)BLOB_SIZE_FACTOR / 1000, colorBitmap);
                            }
                        }
                        System.Drawing.Point p = new System.Drawing.Point((int)handPos.X - 3, (int)handPos.Y - 3);
                        DrawHandPosition(colorBitmap,p,System.Drawing.Brushes.Yellow);

                        AsyncUpdateImage(ColorWriteBitmap, colorBitmap);
                        AsyncUpdateImage(GrayWriteBitmap, grayBitmap);
                        //if (cb_mirrorSoft.Checked)
                        //    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        SetSpeed(SPEED);
                    }
                }
            }
        }
        private void EraseBackground(Matrix<UInt16> depthMat, Bitmap cbmp)
        {

            System.Drawing.Imaging.BitmapData cbmpData;
            Rectangle rect = new Rectangle(0, 0, cbmp.Width, cbmp.Height);
            cbmpData = cbmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, cbmp.PixelFormat);
            int stride = cbmpData.Stride;
            int step = stride / cbmp.Width;
            try
            {
                unsafe
                {
                    byte* ptr = (byte*)cbmpData.Scan0;
                    fixed (UInt16* pM = &depthMat.Data[0, 0])
                    {
                        for (int y = 0; y < depthMat.Height; y++)
                            for (int x = 0; x < depthMat.Width; x++)
                            {
                                UInt16 a = pM[y * 640 + x];
                                //if (a != 0)
                                //{ Console.WriteLine(a); }

                                //int b = preMat.Data[x, y];
                                if (a == 0 || a > bodyDepth - OpenCVController.CANNY_THRESH)
                                {
                                    ptr[x * 3 + y * stride] = 255;
                                    ptr[x * 3 + y * stride + 1] = 255;
                                    ptr[x * 3 + y * stride + 2] = 255;
                                }
                            }

                    }

                }
            }
            catch (Exception) { }
            finally
            {
                cbmp.UnlockBits(cbmpData);
            }


        }
        private void RegionGrow(PointF startPoint, Matrix<UInt16> newMat, Bitmap bmpNew)
        {
            System.Drawing.Imaging.BitmapData nbmpData;
            Rectangle rect = new Rectangle(0, 0, bmpNew.Width, bmpNew.Height);
            nbmpData = bmpNew.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmpNew.PixelFormat);
            int stride = nbmpData.Stride;
            int step = stride / bmpNew.Width;
            int cx = (int)startPoint.X;
            int cy = (int)startPoint.Y;
            //Console.WriteLine(cx + " " + cy);
            int thresh = 50;
            int connectThresh = (int)OpenNIController.DIFF;
            thresh = cx < thresh ? cx : thresh;
            thresh = cx + thresh >= bmpNew.Width ? cx + thresh - bmpNew.Width : thresh;
            thresh = cy < thresh ? cy : thresh;
            thresh = cy + thresh >= bmpNew.Height ? cy + thresh - bmpNew.Height : thresh;
            Matrix<Byte> connectivity = new Matrix<byte>(new byte[480, 640]);
            //connectivity[cx,cy] = 1;

            try
            {
                unsafe
                {
                    byte* nptr = (byte*)nbmpData.Scan0;
                    fixed (UInt16* pM1 = &newMat.Data[0, 0])
                    {
                        fixed (Byte* cM = &connectivity.Data[0, 0])
                        {
                            cM[cx + 640 * cy] = 2;
                            //Console.WriteLine("new"+pM1[cx + cy * 640] + " " + cx + " " + cy);
                            for (int i = 1; i <= thresh; i++)
                            {
                                //left
                                // Console.WriteLine(pM1[cx-i + cy * 640] + " " + (cx-i) + " " + cy);
                                cM[cx + (cy - i) * 640] = 1;
                                cM[cx + (cy + i) * 640] = 1;
                                cM[cx + i + cy * 640] = 1;
                                cM[cx - i + cy * 640] = 1;
                                IsConnected(pM1, cM, cx - i, cy, Direction.Right, connectThresh);
                                for (int j = 1; j <= i; j++)
                                {
                                    //up
                                    IsConnected(pM1, cM, cx - i, cy - j, Direction.Right, connectThresh);
                                    IsConnected(pM1, cM, cx - i, cy - j, Direction.Down, connectThresh);
                                    //down
                                    IsConnected(pM1, cM, cx - i, cy + j, Direction.Right, connectThresh);
                                    IsConnected(pM1, cM, cx - i, cy + j, Direction.Up, connectThresh);
                                }
                                //right
                                IsConnected(pM1, cM, cx + i, cy, Direction.Left, connectThresh);
                                for (int j = 1; j <= i; j++)
                                {
                                    //up
                                    IsConnected(pM1, cM, cx + i, cy - j, Direction.Left, connectThresh);
                                    IsConnected(pM1, cM, cx + i, cy - j, Direction.Down, connectThresh);
                                    //down
                                    IsConnected(pM1, cM, cx + i, cy + j, Direction.Left, connectThresh);
                                    IsConnected(pM1, cM, cx + i, cy + j, Direction.Up, connectThresh);
                                }
                                //up
                                IsConnected(pM1, cM, cx, cy - i, Direction.Down, connectThresh);
                                for (int j = 1; j <= i; j++)
                                {
                                    //left
                                    IsConnected(pM1, cM, cx - j, cy - i, Direction.Down, connectThresh);
                                    IsConnected(pM1, cM, cx - j, cy - i, Direction.Right, connectThresh);
                                    //right
                                    IsConnected(pM1, cM, cx + j, cy - i, Direction.Down, connectThresh);
                                    IsConnected(pM1, cM, cx + j, cy - i, Direction.Left, connectThresh);
                                }
                                //down
                                IsConnected(pM1, cM, cx, cy + i, Direction.Up, connectThresh);
                                for (int j = 1; j <= i; j++)
                                {
                                    //left
                                    IsConnected(pM1, cM, cx - j, cy + i, Direction.Up, connectThresh);
                                    IsConnected(pM1, cM, cx - j, cy + i, Direction.Right, connectThresh);
                                    //right
                                    IsConnected(pM1, cM, cx + j, cy + i, Direction.Up, connectThresh);
                                    IsConnected(pM1, cM, cx + j, cy + i, Direction.Left, connectThresh);
                                }


                            }
                            //MeanCenter(cM);
                            DrawConnectedRegion(cM, nptr);

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
        enum Direction
        {
            Right,
            Left,
            Up,
            Down
        }

        private unsafe void MeanCenter(Byte* connect)
        {
            int nx = 0, ny = 0, count = 0;
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    if (connect[x + 640 * y] >= 2)
                    {
                        nx += x;
                        ny += y;
                        count++;
                    }

                }
            }
            nx /= count;
            ny /= count;
            handPos = new PointF(nx, ny);
        }
        private unsafe void DrawConnectedRegion(Byte* connect, Byte* bmp)
        {
            int stride = 640 * 3;
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    if (connect[x + 640 * y] >= 2)
                    {
                        bmp[x * 3 + y * stride] = 0;
                        bmp[x * 3 + y * stride + 1] = 0;
                        bmp[x * 3 + y * stride + 2] = 255;
                    }
                    if (connect[x + 640 * y] == 1)
                    {
                        bmp[x * 3 + y * stride] = 255;
                        bmp[x * 3 + y * stride + 1] = 0;
                        bmp[x * 3 + y * stride + 2] = 0;
                    }

                }
            }



        }
        private unsafe bool IsConnected(UInt16* depth, Byte* connect, int x, int y, Direction dir, int connectThresh, int cthresh = 1)
        {
            //Console.WriteLine(depth[x + y * 640] + " " + (x) + " " + y);
            //Console.WriteLine(depth[x +1+ y * 640] + " " + (x) + " " + y);
            //Console.WriteLine(Math.Abs(depth[x + y * 640] - depth[x + 1 + y * 640]));
            //int stride = 640 * 3;
            switch (dir)
            {
                case Direction.Right:
                    if (Math.Abs(depth[x + y * 640] - depth[x + 1 + y * 640]) < connectThresh &&
                        connect[x + 1 + y * 640] >= cthresh)
                    {
                        connect[x + y * 640]++;
                        return true;
                    }
                    return false;
                case Direction.Left:
                    if (Math.Abs(depth[x + y * 640] - depth[x - 1 + y * 640]) < connectThresh &&
                               connect[x - 1 + y * 640] >= cthresh)
                    {
                        connect[x + y * 640]++;
                        return true;
                    }
                    return false;
                case Direction.Up:
                    if (Math.Abs(depth[x + y * 640] - depth[x + (y - 1) * 640]) < connectThresh &&
                               connect[x + (y - 1) * 640] >= cthresh)
                    {
                        connect[x + y * 640]++;
                        return true;
                    }
                    return false;
                case Direction.Down:
                    if (Math.Abs(depth[x + y * 640] - depth[x + (y + 1) * 640]) < connectThresh &&
                               connect[x + (y + 1) * 640] >= cthresh)
                    {
                        connect[x + y * 640]++;
                        return true;
                    }
                    return false;
                default:
                    return false;
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
                                    if (a == 0 || a > bodyDepth + 100)
                                    {
                                        nptr[x * 3 + y * stride] = 255;
                                        nptr[x * 3 + y * stride + 1] = 255;
                                        nptr[x * 3 + y * stride + 2] = 255;
                                    }
                                    else if (Math.Abs(a - b) > DIFF)
                                    {
                                        nptr[x * 3 + y * stride] = 200;
                                        nptr[x * 3 + y * stride + 1] = 200;
                                        nptr[x * 3 + y * stride + 2] = 200;
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
            m_isPause = !m_isPause;
            if (m_isPause)
            {
                tempSpeed = SPEED;
                SPEED = -1;
                SetSpeed(-1f);
            }
            else
            {
                SPEED = tempSpeed;
                SetSpeed(SPEED);
            }

        }

        public override void SetSpeed(double speed)
        {
            if (m_playback != null)
            {
                speed = speed == 0 ? -1 : speed;
                m_playback.Speed = (float)speed;
            }
        }


        */
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