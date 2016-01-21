using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CURELab.SignLanguage.HandDetector;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EducationSystem
{
    public class FrameData
    {
        public float[] handshape { get; set; }
        public float[] lefthandshape { get; set; }
        public int[] pos { get; set; }
        public HandEnum type { get; set; }
    }
    public class JsonData
    {
        public List<FrameData> frames;
    }
    public class KeyFrame
    {
        public int FrameNumber { get; set; }
        public HandEnum Type { get; set; }
        public Point RightPosition { get; set; }
        public Point RightPositionRel { get; set; }
        public Point LeftPositionRel { get; set; }
        public Point LeftPosition { get; set; }
        public BitmapSource RightImage { get; set; }
        public BitmapSource LeftImage { get; set; }

        public KeyFrame ()
        {
            RightImage = null;
            LeftImage = null;
        }
    }
    public class VideoModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Chinese { get; set; }
        public string Path { get; set; }
        public List<KeyFrame> KeyFrames;

        public VideoModel()
        {
            KeyFrames = new List<KeyFrame>();
        }
    }
}
