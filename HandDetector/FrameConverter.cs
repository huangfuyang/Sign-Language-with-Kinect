using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Microsoft.Kinect;
using Newtonsoft.Json;

namespace CURELab.SignLanguage.HandDetector
{
    class FrameData
    {
        public string right { get; set; }
        public string left { get; set; }
        public string skeleton { get; set; }
        public string label { get; set; }
        public string position { get; set; }
    }
    public static class FrameConverter
    {
        public static string EncodeImage(IImage bmp)
        {
            if (bmp == null)
            {
                return null;
            }
            return EncodeImage(bmp.Bitmap);
        }

        public static string EncodeImage(Bitmap bmp)
        {
            if (bmp == null)
            {
                return null;
            }
            string bmpString;
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Bmp);
                var imageData = stream.ToArray();
                bmpString = Convert.ToBase64String(imageData);
            }
            return bmpString;

        }
        public static string Encode(HandShapeModel hand)
        {
            var right = EncodeImage(hand.RightColor);
            string left = null;
            if (hand.type == HandEnum.Both)
            {
                left = EncodeImage(hand.LeftColor);
            }
            var pos = String.Format("{0},{1},{2},{3}",
                hand.right.GetXCenter(), hand.right.GetYCenter(), hand.left.GetXCenter(), hand.left.GetYCenter());
            var frame = new FrameData()
            {
                right = right,
                left = left,
                skeleton = hand.skeletonData,
                label = hand.type.ToString(),
                position = pos
            };
            var jsonData = JsonConvert.SerializeObject(frame, Formatting.Indented);
            return jsonData;
        }

        public static string Encode(Bitmap img)
        {
            string bmpString;
            
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Bmp);
                var imageData = stream.ToArray();
                bmpString = Convert.ToBase64String(imageData);
            }
            var frame = new FrameData()
            {
                right = bmpString,
                left = null,
                skeleton = null,
                label = ""
            };
            var jsonData = JsonConvert.SerializeObject(frame, Formatting.Indented);
            return jsonData;
        }

        public static string Encode(String label)
        {
            var frame = new FrameData()
            {
                right= null,
                left = null,
                skeleton = null,
                label = label
            };
            var jsonData = JsonConvert.SerializeObject(frame, Formatting.Indented);
            return jsonData;
        }

        public static string GetFrameDataArgString(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                return "";
            }
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
            {
                return "untracked";
            }
            string s = String.Empty;
            JointType[] jointTypes = new JointType[]
            {
                JointType.Head, 
                JointType.ShoulderLeft, JointType.ShoulderCenter,JointType.ShoulderRight, 
                JointType.ElbowLeft, JointType.ElbowRight, 
                JointType.WristLeft, JointType.WristRight,
                JointType.HandLeft, JointType.HandRight, 
                JointType.Spine, JointType.HipLeft, JointType.HipCenter,
                JointType.HipRight
            };

            //Joints X, Y
            for (int i = 0; i < jointTypes.Length; i++)
            {
                JointType jointType = jointTypes[i];
                SkeletonPoint point = skeleton.Joints[jointType].Position;
                var cp = KinectSDKController.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(point,
                    ColorImageFormat.RgbResolution640x480Fps30);
                var dp = KinectSDKController.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point,
                    DepthImageFormat.Resolution640x480Fps30);
                s += String.Format("{0},{1},{2},{3},{4},{5},{6},", point.X, point.Y, point.Z,
                    cp.X,cp.Y,dp.X,dp.Y);
            }
            if (s.Length>0)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}
