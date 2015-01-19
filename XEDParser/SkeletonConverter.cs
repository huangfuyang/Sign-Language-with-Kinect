using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XEDParser
{
    class SkeletonConverter
    {

        private JointType[] jointTypes = new JointType[]{ JointType.Head, JointType.ShoulderLeft, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowLeft, JointType.ElbowRight, JointType.WristLeft, JointType.WristRight, JointType.HandLeft, JointType.HandRight, JointType.Spine, JointType.HipLeft, JointType.HipCenter, JointType.HipRight };
        private const DepthImageFormat depthImageFormat = DepthImageFormat.Resolution640x480Fps30;
        private const ColorImageFormat colorImageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        public String getSkeletonLine(KinectSensor CurrentKinectSensor, JointCollection joints) {
            float[] dataLine = new float[7 * jointTypes.Length];
            DepthImagePoint dp_csv;
            ColorImagePoint cp_csv;

            for (int i = 0; i < jointTypes.Length; i++)
            {
                Joint joint = joints[jointTypes[i]];
                dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, depthImageFormat);
                cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, colorImageFormat);

                dataLine[i * 7] = joint.Position.X;
                dataLine[i * 7 + 1] = joint.Position.Y;
                dataLine[i * 7 + 2] = joint.Position.Z;
                dataLine[i * 7 + 3] = cp_csv.X;
                dataLine[i * 7 + 4] = cp_csv.Y;
                dataLine[i * 7 + 5] = dp_csv.X;
                dataLine[i * 7 + 6] = dp_csv.Y;
            }

            return string.Join(",", dataLine);
        }
    }
}
