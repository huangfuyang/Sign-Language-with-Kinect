using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EducationSystem.SignNumGame;
using CURELab.SignLanguage.HandDetector;
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
        public Point LeftPosition  { get; set; }
        public string RightHandShape { get; set; }
        public string LeftHandShape { get; set; }
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
