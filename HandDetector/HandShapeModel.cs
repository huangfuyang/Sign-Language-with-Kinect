// author：      Administrator
// created time：2014/5/2 14:47:16
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using Emgu.CV;
using Microsoft.Kinect;
using Point = System.Drawing.Point;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// hand shape model
    /// </summary>
    public class HandShapeModel:IDisposable
    {
        public long frame = 0;
        private int hogSize;
        public HandEnum type;
        // sin 
        public float direction = 0;
        // width * height
        public int size = 0;
        // center
        public Point center = Point.Empty;
        // hog right
        public float[] hogRight;
        // hog left
        public float[] hogLeft;
        // hog right side view
        public float[] hogRightSide;
        // hog left side view
        public float[] hogLeftSide;

        public IImage RightColor;
        public IImage RightDepth;
        public IImage LeftColor;
        public IImage LeftDepth;

        public Rectangle right;
        public Rectangle left;
        public Rectangle intersectCenter;
        // currentSkeleton data
        public string skeletonData = "";

        public HandShapeModel(int hogSize, HandEnum type)
        {
            switch (type)
            {
                case HandEnum.Right:
                    hogRight = new float[hogSize];
                    break;
                case HandEnum.Left:
                    hogLeft = new float[hogSize];
                    break;
                case HandEnum.Both:
                    hogRight = new float[hogSize];
                    hogLeft = new float[hogSize];
                    break;
                case HandEnum.Intersect:
                    hogRight = new float[hogSize];
                    break;
                case HandEnum.None:
                    break;
                default:
                    break;
            }
            this.type = type;
            skeletonData = "";
        }

       

        public void SetSkeletonData(Skeleton skeleton)
        {
            if (skeleton != null)
            {
                skeletonData = GetFrameDataArgString(skeleton);
            }
        }

        
      
        private string GetFrameDataArgString(Skeleton skeleton)
        {
            string s = String.Empty;
            JointType[] jointTypes = new JointType[] { JointType.Head, JointType.ShoulderLeft,JointType.ShoulderCenter, 
                JointType.ShoulderRight, JointType.ElbowLeft, JointType.ElbowRight, JointType.WristLeft,  JointType.WristRight, 
                JointType.HandLeft,JointType.HandRight,JointType.Spine,  JointType.HipLeft,JointType.HipCenter,
               JointType.HipRight };

            //Joints X, Y
            for (int i = 0; i < jointTypes.Length; i++)
            {
                JointType jointType = jointTypes[i];
                if (skeleton != null )
                 //if (currentSkeleton != null && currentSkeleton.Joints[jointType].TrackingState != JointTrackingState.NotTracked)
                {
                    SkeletonPoint point = skeleton.Joints[jointType].Position;
                    s += String.Format(", {0}, {1}, {2}", point.X, point.Y, point.Z);
                }
                else
                {
                    s += ", NULL, NULL, NULL";
                }
            }

            //s += String.Format(", {0}", hands.Count);
            //for (int i = 0; i < Math.Min(hands.Count, 2); i++)
            //{
            //    Hand hand = hands[i];
            //    //Fingertips count
            //    s += String.Format(", {0}", hand.fingertips.Count);
            //    //Fingertips X, Y
            //    for (int j = 0; j < Math.Min(hand.fingertips.Count, 5); j++)
            //    {
            //        s += String.Format(", {0}, {1}, {2}", hand.fingertips3D[j].X, hand.fingertips3D[j].Y, hand.fingertips3D[j].Z);
            //    }
            //    for (int j = 0; j < 5 - hand.fingertips.Count; j++)
            //    {
            //        s += ", NULL, NULL, NULL";
            //    }
            //    //Bounding ellipse X, Y, MajorAxis, MinorAxis, AspectRatio
            //    int major = Math.Max(hand.boundingRectangle.Width, hand.boundingRectangle.Height),
            //        minor = Math.Min(hand.boundingRectangle.Width, hand.boundingRectangle.Height);
            //    s += String.Format(", {0}, {1}, {2}, {3}, {4}",
            //        hand.boundingRectangle.X, hand.boundingRectangle.Y,
            //        major, minor, ((minor == 0) ? "NULL" : ((double)major / minor).ToString())
            //        );
            //    s += String.Format(", {0}",
            //        (Double.IsNaN(hand.axisTheta) || Double.IsInfinity(hand.axisTheta)) ? "NULL" : hand.axisTheta.ToString());
            //}
            //for (int i = 0; i < 2 - hands.Count; i++)
            //{
            //    for (int j = 0; j < 22; j++)
            //    {
            //        s += ", NULL";
            //    }
            //}
            return s;
        }
        public HandShapeModel(HandEnum type)
        {
            this.type = type;
        }


        public void Dispose()
        {
            this.Dispose(true);
            //GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (RightColor != null)
            {
                RightColor.Dispose();
            }
            if (RightDepth != null)
            {
                RightDepth.Dispose();
            }
            if (LeftColor != null)
            {
                LeftColor.Dispose();
            } if (LeftDepth != null)
            {
                LeftDepth.Dispose();
            }
            }
        }

        ~HandShapeModel()
        {
            this.Dispose(false);
        }
    }
}