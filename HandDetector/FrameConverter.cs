using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using Newtonsoft.Json;

namespace CURELab.SignLanguage.HandDetector
{
    class FrameData
    {
        public string depth { get; set; }
        public string color { get; set; }
        public string skeleton { get; set; }
        public string label { get; set; }
    }
    public static class FrameConverter
    {
        public static string Encode(Bitmap hand, Skeleton skeleton)
        {
            byte[] imageData;
            string bmpString;
            using (var stream = new MemoryStream())
            {
                hand.Save(stream, ImageFormat.Jpeg);
                imageData = stream.ToArray();
                bmpString= Convert.ToBase64String(imageData);
            }
            var frame = new FrameData()
            {
                depth = bmpString,
                color = null,
                skeleton = GetFrameDataArgString(skeleton),
                label = ""
            };
            var jsonData = JsonConvert.SerializeObject(frame, Formatting.Indented);
            return jsonData;
        }

        private static string GetFrameDataArgString(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                return "";
            }
            string s = String.Empty;
            JointType[] jointTypes = new JointType[]
            {
                JointType.Head, JointType.ShoulderLeft, JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowLeft, JointType.ElbowRight, JointType.WristLeft,
                JointType.WristRight,
                JointType.HandLeft, JointType.HandRight, JointType.Spine, JointType.HipLeft, JointType.HipCenter,
                JointType.HipRight
            };

            //Joints X, Y
            for (int i = 0; i < jointTypes.Length; i++)
            {
                JointType jointType = jointTypes[i];
                if (skeleton != null)
                    //if (skeleton != null && skeleton.Joints[jointType].TrackingState != JointTrackingState.NotTracked)
                {
                    SkeletonPoint point = skeleton.Joints[jointType].Position;
                    s += String.Format("{0},{1},{2},", point.X, point.Y, point.Z);
                }
                else
                {
                    s += "NULL,NULL,NULL,";
                }
            }
            if (s.Length>0)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}
