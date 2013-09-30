using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
namespace RecognitionSystem.StaticTools
{
    public static class UtilityTools
    {
        public static Vector3 SkeletonPointToVector3(SkeletonPoint sp)
        {
            return new Vector3(sp.X, sp.Y, sp.Z);
        }
    }
}
