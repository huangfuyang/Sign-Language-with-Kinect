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


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// hand shape model
    /// </summary>
    public class HandShapeModel
    {
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
                default:
                    break;
            }
        }

        public HandShapeModel()
        {
        }


    }
}