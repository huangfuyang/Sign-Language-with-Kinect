// author：      Administrator
// created time：2014/1/14 16:03:44
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class KinectController : INotifyPropertyChanged
    {
        public static double DIFF = 10;

        protected KinectController()
        {
            m_OpenCVController = OpenCVController.GetSingletonInstance();
        }
        private WriteableBitmap colorWriteBitmap;
        public WriteableBitmap ColorWriteBitmap
        {
            get { return colorWriteBitmap; }
            protected set { colorWriteBitmap = value; }
        }

        private WriteableBitmap depthWriteBitmap;
        public WriteableBitmap DepthWriteBitmap
        {
            get { return depthWriteBitmap; }
            protected set { depthWriteBitmap = value; }
        }

        private WriteableBitmap processedDepthBitmap;
        public WriteableBitmap ProcessedDepthBitmap
        {
            get { return processedDepthBitmap; }
            protected set { processedDepthBitmap = value; }
        }

        private WriteableBitmap grayWriteBitmap;
        public WriteableBitmap GrayWriteBitmap
        {
            get { return grayWriteBitmap; }
            protected set { grayWriteBitmap = value; }
        }

        public WriteableBitmap WrtBMP_RightHandFront { get; set; }
        public WriteableBitmap WrtBMP_LeftHandFront { get; set; }
        public WriteableBitmap WrtBMP_Candidate2 { get; set; }
        public WriteableBitmap WrtBMP_Candidate3 { get; set; }
        public WriteableBitmap WrtBMP_Candidate1 { get; set; }
        public WriteableBitmap WrtBMP_RightHandSide { get; set; }
        public WriteableBitmap WrtBMP_LeftHandSide { get; set; }

        private WriteableBitmap edgeBitmap;
        public WriteableBitmap EdgeBitmap
        {
            get { return edgeBitmap; }
            protected set { edgeBitmap = value; }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }


        protected OpenCVController m_OpenCVController;

        public virtual void Run()
        {

        }
        public virtual void Stop()
        {

        }

        protected static KinectController singleInstance;

        public virtual void Initialize(String uri = null) { }
        public virtual void Start() { }
        public virtual void Shutdown() { }

        protected void DrawHandPosition(Bitmap bitmap, System.Drawing.Point p, System.Drawing.Brush color)
        {
            lock (bitmap)
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawEllipse(new Pen(color, 5),
                                      new Rectangle(p, new System.Drawing.Size(5, 5)));
                        g.Save();
                    }
                }
                catch (Exception) { }
            }

        }

        protected void DrawString(Bitmap bitmap, Font textFont, String text)
        {
            lock (bitmap)
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawString(text, textFont, Brushes.Red,
                      100, 20);
                        g.Save();
                    }
                }
                catch (Exception) { }
            }

        }

        protected void RegionGrow(PointF startPoint, short[] depthData, Bitmap bmpNew)
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
            //int connectThresh = 2;
            int connectThresh = (int)KinectController.DIFF;
            thresh = cx < thresh ? cx : thresh;
            thresh = cx + thresh >= bmpNew.Width ? cx + thresh - bmpNew.Width : thresh;
            thresh = cy < thresh ? cy : thresh;
            thresh = cy + thresh >= bmpNew.Height ? cy + thresh - bmpNew.Height : thresh;
            short[] connectivity = new short[bmpNew.Width * bmpNew.Height];
            //connectivity[cx,cy] = 1;

            try
            {
                unsafe
                {
                    byte* nptr = (byte*)nbmpData.Scan0;
                    connectivity[cx + 640 * cy] = 2;
                    //Console.WriteLine("new"+depthData[cx + cy * 640] + " " + cx + " " + cy);
                    for (int i = 1; i <= thresh; i++)
                    {
                        //left
                        // Console.WriteLine(depthData[cx-i + cy * 640] + " " + (cx-i) + " " + cy);
                        IsConnected(depthData, connectivity, cx - i, cy, Direction.Right, connectThresh);
                        IsConnected(depthData, connectivity, cx - i, cy, Direction.Right, connectThresh);
                        for (int j = 1; j <= i; j++)
                        {
                            //up
                            IsConnected(depthData, connectivity, cx - i, cy - j, Direction.Right, connectThresh);
                            IsConnected(depthData, connectivity, cx - i, cy - j, Direction.Down, connectThresh);
                            //down
                            IsConnected(depthData, connectivity, cx - i, cy + j, Direction.Right, connectThresh);
                            IsConnected(depthData, connectivity, cx - i, cy + j, Direction.Up, connectThresh);
                        }
                        //right
                        IsConnected(depthData, connectivity, cx + i, cy, Direction.Left, connectThresh);
                        IsConnected(depthData, connectivity, cx + i, cy, Direction.Left, connectThresh);
                        for (int j = 1; j <= i; j++)
                        {
                            //up
                            IsConnected(depthData, connectivity, cx + i, cy - j, Direction.Left, connectThresh);
                            IsConnected(depthData, connectivity, cx + i, cy - j, Direction.Down, connectThresh);
                            //down
                            IsConnected(depthData, connectivity, cx + i, cy + j, Direction.Left, connectThresh);
                            IsConnected(depthData, connectivity, cx + i, cy + j, Direction.Up, connectThresh);
                        }
                        //up
                        IsConnected(depthData, connectivity, cx, cy - i, Direction.Down, connectThresh);
                        IsConnected(depthData, connectivity, cx, cy - i, Direction.Down, connectThresh);
                        for (int j = 1; j <= i; j++)
                        {
                            //left
                            IsConnected(depthData, connectivity, cx - j, cy - i, Direction.Down, connectThresh);
                            IsConnected(depthData, connectivity, cx - j, cy - i, Direction.Right, connectThresh);
                            //right
                            IsConnected(depthData, connectivity, cx + j, cy - i, Direction.Down, connectThresh);
                            IsConnected(depthData, connectivity, cx + j, cy - i, Direction.Left, connectThresh);
                        }
                        //down
                        IsConnected(depthData, connectivity, cx, cy + i, Direction.Up, connectThresh);
                        IsConnected(depthData, connectivity, cx, cy + i, Direction.Up, connectThresh);
                        for (int j = 1; j <= i; j++)
                        {
                            //left
                            IsConnected(depthData, connectivity, cx - j, cy + i, Direction.Up, connectThresh);
                            IsConnected(depthData, connectivity, cx - j, cy + i, Direction.Right, connectThresh);
                            //right
                            IsConnected(depthData, connectivity, cx + j, cy + i, Direction.Up, connectThresh);
                            IsConnected(depthData, connectivity, cx + j, cy + i, Direction.Left, connectThresh);
                        }


                    }
                    //MeanCenter(connectivity);
                    DrawConnectedRegion(connectivity, nptr);


                    //UInt16* depthDatah = (UInt16*)newMat.Data;
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
            //handPos = new PointF(nx, ny);
        }
        private unsafe void DrawConnectedRegion(short[] connect, Byte* bmp)
        {
            int singleStride = 4;
            int stride = 640 * singleStride;
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    if (connect[x + 640 * y] >= 2)
                    {
                        bmp[x * singleStride + y * stride] = 0;
                        bmp[x * singleStride + y * stride + 1] = 0;
                        bmp[x * singleStride + y * stride + 2] = 255;
                    }
                    if (connect[x + 640 * y] == 1)
                    {
                        bmp[x * singleStride + y * stride] = 255;
                        bmp[x * singleStride + y * stride + 1] = 0;
                        bmp[x * singleStride + y * stride + 2] = 0;
                    }

                }
            }



        }
        private bool IsConnected(short[] depth, short[] connect, int x, int y, Direction dir, int connectThresh, int cthresh = 1)
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


        public virtual void TogglePause() { }

        public virtual void Reset()
        {
            
        }
        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public virtual void SetSpeed(double speed)
        {
        }
    }
}