using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Microsoft.Kinect;

namespace CURELab.SignLanguage.HandDetector
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4f
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point2i
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SLR_Skeleton
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public Vector4f[] pos3D;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public Point2i[] pos2D;
    }
    public class OniReader
    {
        private const int HEAD = 3;
        private const int RIGHTSHOULDER = 4;
        private const int LEFTSHOULDER = 8;
        private const int RIGHTHAND = 7;
        private const int LEFTHAND = 11;
        private const int width = 640;
        private const int height = 480;
        private ImageViewer viewer;
        private static OniReader singleInstance;
        private List<long> vColorFrame;
        public List<Image<Emgu.CV.Structure.Bgr, byte>> vColorData;
        private List<long> vDepthFrame;
        public List<ushort[]> vDepthData;
        private List<long> vSkeletonFrame;
        public List<SLR_Skeleton> vSkeletonData;
        private int SLR_size;
        private int frame_rate;
        private OpenCVController m_OpenCVController;
        private string filename;
        private OniReader()
        {
            viewer = new ImageViewer();
            vColorFrame = new List<long>();
            vDepthFrame = new List<long>();
            vSkeletonFrame = new List<long>();
            vColorData = new List<Image<Bgr, byte>>();
            vDepthData = new List<ushort[]>();
            vSkeletonData = new List<SLR_Skeleton>();
            SLR_size = Marshal.SizeOf(typeof(SLR_Skeleton));
            m_OpenCVController = OpenCVController.GetSingletonInstance();
        }



        public static OniReader GetSingletonInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new OniReader();
            }
            return singleInstance;

        }

        private bool readColorFrame(string filename)
        {
            vColorFrame.Clear();
            filename = filename + "\\color.frame";
            if (File.Exists(filename))
            {
                using (BinaryReader colorFrameReader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    while (colorFrameReader.BaseStream.Position != colorFrameReader.BaseStream.Length)
                    {
                        long colorframeno = colorFrameReader.ReadInt64();
                        vColorFrame.Add(colorframeno);
                    }
                }
                return true;
            }
            return false;
        }

        private bool readDepthFrame(string filename)
        {
            vDepthFrame.Clear();
            filename = filename + "\\depth.frame";
            if (File.Exists(filename))
            {
                using (BinaryReader depthFrameReader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    while (depthFrameReader.BaseStream.Position != depthFrameReader.BaseStream.Length)
                    {
                        long depthframeno = depthFrameReader.ReadInt64();
                        vDepthFrame.Add(depthframeno);
                    }
                }
                return true;
            }
            return false;
        }

        private bool readSkeletonFrame(string filename)
        {
            vSkeletonFrame.Clear();
            filename = filename + "\\skeleton.frame";
            if (File.Exists(filename))
            {
                using (BinaryReader skeleFrameReader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    while (skeleFrameReader.BaseStream.Position != skeleFrameReader.BaseStream.Length)
                    {
                        long skelframeno = skeleFrameReader.ReadInt64();
                        vSkeletonFrame.Add(skelframeno);
                    }
                }
                return true;
            }
            return false;
        }

        public void ReadFile(string filePath)
        {
            if (!(readColorFrame(filePath) && readDepthFrame(filePath) && readSkeletonFrame(filePath)))
            {
                return;
            }

            if (!(vSkeletonFrame.Count == vColorFrame.Count && vSkeletonFrame.Count == vDepthFrame.Count))
            {
                return;
            }

            filename = filePath.Split('\\').Last().Split('.').First();
            Console.WriteLine(filename);
            //Read RGB, depth and skeleton data.
            if (!File.Exists(filePath + "\\color.avi"))
                return;
            Capture CCapture = new Capture(filePath + "\\color.avi");
            frame_rate = (int)CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
            if (!File.Exists(filePath + "\\depth.dat"))
                return;
            BinaryReader depthFrameReader = new BinaryReader(File.Open(filePath + "\\depth.dat", FileMode.Open));
            if (!File.Exists(filePath + "\\skeleton.dat"))
                return;
            BinaryReader skeletonFileReader = new BinaryReader(File.Open(filePath + "\\skeleton.dat", FileMode.Open));

            //read first color depth and skeleton data
            vColorData.Clear();
            vDepthData.Clear();
            vSkeletonData.Clear();
            int index = 0;

            while (index < vSkeletonFrame.Count)
            {
                //RGB, depth and skeleton of the next frame
                if (CCapture.Grab())
                {
                    var colorFrame = CCapture.RetrieveBgrFrame();
                    vColorData.Add(colorFrame.Clone());
                }
                //read depth data 
                var depthData = new ushort[width * height];
                var depthDataRaw = new byte[width * height * sizeof(ushort)];
                depthFrameReader.Read(depthDataRaw, 0, depthDataRaw.Length);
                //                depthFrameReader.Read(depthDataRaw, index * depthDataRaw.Length, depthDataRaw.Length);
                Buffer.BlockCopy(depthDataRaw, 0, depthData, 0, depthDataRaw.Length);
                vDepthData.Add(depthData);
                //read skeleton
                var skelDataRaw = new byte[SLR_size];
                skeletonFileReader.Read(skelDataRaw, 0, SLR_size);
                //                skeletonFileReader.Read(skelDataRaw, index * SLR_size, SLR_size);

                //if begin, judge end time and do hand sgementation
                vSkeletonData.Add(ByteArrayToSLR(skelDataRaw));
                index++;
                Console.WriteLine(index);
            }
            //CCapture.Dispose();
            skeletonFileReader.Close();
            depthFrameReader.Close();
        }

        private SLR_Skeleton ByteArrayToSLR(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var skeleton = (SLR_Skeleton)Marshal.PtrToStructure(
                handle.AddrOfPinnedObject(), typeof(SLR_Skeleton));
            handle.Free();
            return skeleton;
        }

        private void PlayVideo()
        {
            for (int i = 0; i < vColorData.Count; i++)
            {
                var image = vColorData[i];
                var s = vSkeletonData[i].pos2D[LEFTHAND];
                foreach (var p in vSkeletonData[i].pos2D)
                {
                    //    image.Draw(new CircleF(new PointF(p.x, p.y), 5), new Bgr(Color.Yellow), 0);
                }
                image.Draw(new CircleF(new PointF(s.x, s.y), 5), new Bgr(Color.Yellow), 0);
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                {
                    viewer.Size = image.Size;
                    viewer.Image = image;
                });
                Thread.Sleep(1000 / frame_rate);
            }
        }

        public void ShowColorVideo()
        {
            viewer.Show();
            Thread t = new Thread(PlayVideo);
            t.Start();
        }

        private Point2i Head;
        private SLR_Skeleton cSkeleton;
        private Point2i leftFirst;
        private Point2i rightFirst;
        public void Run()
        {
            string folderPath = "D:\\devisign\\" + filename;
            CreateFolder(folderPath,false);
            string HandshapePath = folderPath +"\\handshape";
            CreateFolder(HandshapePath+"\\left", false);
            Head = vSkeletonData[0].pos2D[HEAD];
            rightFirst = vSkeletonData[0].pos2D[RIGHTHAND];
            leftFirst = vSkeletonData[0].pos2D[LEFTHAND];
            var headDepth = vDepthData[0][Head.x + Head.y * 640];
            for (int i = 0; i < vSkeletonData.Count; i++)
            {
                cSkeleton = vSkeletonData[i];
                //head
                var h_3x = cSkeleton.pos3D[HEAD].x;
                var h_3y = cSkeleton.pos3D[HEAD].y;
                var h_3z = cSkeleton.pos3D[HEAD].z;
                var h_2x = cSkeleton.pos2D[HEAD].x;
                var h_2y = cSkeleton.pos2D[HEAD].y;
                //right shoulder
                var rs_3x = cSkeleton.pos3D[RIGHTSHOULDER].x;
                var rs_3y = cSkeleton.pos3D[RIGHTSHOULDER].y;
                var rs_3z = cSkeleton.pos3D[RIGHTSHOULDER].z;
                var rs_2x = cSkeleton.pos2D[RIGHTSHOULDER].x;
                var rs_2y = cSkeleton.pos2D[RIGHTSHOULDER].y;
                //left shoulder
                var ls_3x = cSkeleton.pos3D[LEFTSHOULDER].x;
                var ls_3y = cSkeleton.pos3D[LEFTSHOULDER].y;
                var ls_3z = cSkeleton.pos3D[LEFTSHOULDER].z;
                var ls_2x = cSkeleton.pos2D[LEFTSHOULDER].x;
                var ls_2y = cSkeleton.pos2D[LEFTSHOULDER].y;
                string line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},", i, h_3x,
                    h_3y, h_3z, h_2x, h_2y, rs_3x, rs_3y, rs_3z, rs_2x, rs_2y, ls_3x, ls_3y, ls_3z, ls_2x, ls_2y);

                //find hand
                bool leftHandRaise = false;
                HandShapeModel handModel = null;
                handModel = m_OpenCVController.FindHandFromColor(vColorData[i], vDepthData[i], Head, headDepth);
                if (handModel != null && handModel.type != HandEnum.None)
                {
                    if (!cSkeleton.pos2D[LEFTHAND].IsCloseTo(leftFirst))
                    {
                        leftHandRaise = true;
                    }

                    //if (handModel.IntersectRectangle != Rectangle.Empty
                    //        && !leftHandRaise)
                    //{
                    //    //false intersect right hand behind head and left hand on initial position
                    //}
                    //else
                    {
                        if (handModel.type == HandEnum.Intersect)
                        {
                            if (handModel.RightColor != null && !handModel.IntersectRectangle.GetCenter2i().IsCloseTo(rightFirst) &&
                                                                !handModel.IntersectRectangle.GetCenter2i().IsCloseTo(leftFirst))
                            {
                                var colorRight = handModel.RightColor;
                                string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, i, handModel.type, 'C');
                                colorRight.Save(fileName);
                                var depthRight = handModel.RightDepth;
                                fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, i, handModel.type, 'D');
                                //depthRight.Save(fileName);
                            }
                        }
                        else
                        {
                            // to overcome the problem of right hand lost and left hand recognized as intersected.
                            if (handModel.RightColor != null && !handModel.right.GetCenter2i().IsCloseTo(rightFirst) &&
                                                                !handModel.right.GetCenter2i().IsCloseTo(leftFirst))
                            {
                                var colorRight = handModel.RightColor;
                                string fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath, i, handModel.type, 'C');
                                colorRight.Save(fileName);
                                var depthRight = handModel.RightDepth;
                                fileName = String.Format("{0}\\{1}_{2}_{3}.jpg",
                                    HandshapePath,i, handModel.type, 'D');
                                //depthRight.Save(fileName);
                                //left hand
                                if (handModel.LeftColor != null && !handModel.left.GetCenter2i().IsCloseTo(leftFirst))
                                {
                                    var colorleft = handModel.LeftColor;
                                    fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, i, handModel.type, 'C', "left");
                                    colorleft.Save(fileName);

                                    var depthleft = handModel.LeftDepth;
                                    fileName = String.Format("{0}\\{4}\\{1}_{2}_{3}.jpg",
                                        HandshapePath, i, handModel.type, 'D', "left");
                                    //depthleft.Save(fileName);
                                }
                            }

                        }

                        line += GetHandModelString(handModel);
                    }
                }
            }
        }


        protected string GetHandModelString(HandShapeModel model)
        {
            string r = "";
            r += GetRectangleString(model.right) + ",";
            r += GetRectangleString(model.left) + ",";
            if (model.type == HandEnum.Intersect || model.type == HandEnum.IntersectTouch)
            {
                r += GetRectangleString(model.IntersectRectangle);
            }
            return r;
        }

        protected string GetRectangleString(Rectangle rect)
        {
            return rect.GetXCenter().ToString() + "," + rect.GetYCenter().ToString();
        }

        public void CreateFolder(string path, bool delete)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);  // Create the folder if it is not existed
            if (delete)
            {
                var dir = new DirectoryInfo(path);
                foreach (System.IO.FileInfo file in dir.GetFiles()) file.Delete();
                foreach (System.IO.DirectoryInfo subDirectory in dir.GetDirectories()) subDirectory.Delete(true);
            }

        }
    }
}
