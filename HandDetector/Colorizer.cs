﻿// author：      Administrator
// created time：2014/4/30 19:46:05
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class Colorizer
    {
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Byte offset to the red byte of a Bgr32 pixel.
        /// </summary>
        private const int RedIndex = 2;

        /// <summary>
        /// Byte offset to the green byte of a Bgr32 pixel.
        /// </summary>
        private const int GreenIndex = 1;

        /// <summary>
        /// Byte offset to the blue byte of a Bgr32 pixel.
        /// </summary>
        private const int BlueIndex = 0;
        /// <summary>
        /// The nearest depth (in millimeters) to be rendered with a gradient.
        /// </summary>
        private const int MinMinDepth = 400;

        /// <summary>
        /// The furthest depth (in millimeters) to be rendered with a gradient.
        /// </summary>
        private const int MaxMaxDepth = 16383;


        /// <summary>
        /// A static lookup table that maps depth (in millimeters) to intensity (0-255).
        /// </summary>
        private byte[] intensityTable = new byte[MaxMaxDepth + 1];  // 16 KiB

        /// <summary>
        /// A static lookup table that maps depth (in millimeters) to intensity (0-255).
        /// </summary>
        private short[,] TwoD_intensityTable; // 16 KiB

        /// <summary>
        /// Flag to indicate whether the color mapping table needs to be initialized
        /// </summary>
        private bool initializeTransformTable = false;

        private float angle;
        public float Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                InitializeTransformTable(MaxMaxDepth, angle);
            }
        }

        public Colorizer()
        {
            
        }
        public Colorizer(float transformAngle, int min, int max)
        {
            this.Angle = transformAngle;
            intensityTable = GetColorMappingTable(min, max);
        }

        public void CullImage(Image<Bgr,byte> img, int cull )
        {
            
        }
        public void TransformCullAndConvertDepthFrame(
          DepthImagePixel[] depthFrame,
          int minDepth,
          int maxDepth,
          byte[] depthPixel,
          float transformAngle,
          short cullingThreshold,
          System.Drawing.Point headPostition)
        {
            // Test that the buffer lengths are appropriately correlated, which allows us to use only one
            // value as the loop condition.
            if ((depthFrame.Length * Bgr32BytesPerPixel) != depthPixel.Length)
            {
                throw new InvalidOperationException();
            }
            //get intensity map
            cullingThreshold = TransformDepth(cullingThreshold, headPostition.Y, transformAngle);
            byte[] mappingTable = GetColorMappingTable(minDepth, maxDepth, transformAngle, cullingThreshold);
            //initial transform table
            InitializeTransformTable(MaxMaxDepth, transformAngle);

            // process data

            for (int depthIndex = 0, colorIndex = 0;
                colorIndex < depthPixel.Length;
                depthIndex++, colorIndex += Bgr32BytesPerPixel)
            {
                try
                {
                    short depth = depthFrame[depthIndex].Depth;
                    //transform
                    depth = TwoD_intensityTable[depthIndex / 640, depth];
                    // look up in intensity table
                    byte color = mappingTable[(ushort)depth];


                    // Write color pixel to buffer
                    depthPixel[colorIndex + RedIndex] = color;
                    depthPixel[colorIndex + GreenIndex] = color;
                    depthPixel[colorIndex + BlueIndex] = color;
                }
                catch (Exception)
                {
                    continue;
                }
               
            }
        }


        public void TransformAndConvertDepthFrame(DepthImagePixel[] depthFrame, byte[] depthPixels, ColorImagePoint[] coordinate)
        {


            // Test that the buffer lengths are appropriately correlated, which allows us to use only one
            // value as the loop condition.
            if (depthFrame.Length != depthPixels.Length)
            {
                throw new InvalidOperationException();
            }
            //get intensity map
            byte[] mappingTable = this.intensityTable;

            // process data
            Array.Clear(depthPixels, 0, depthPixels.Length);
            for (int depthIndex = 0; depthIndex < depthFrame.Length; depthIndex++)
            {
                var cpixel = coordinate[depthIndex];
                int colorInDepthX = cpixel.X;
                int colorInDepthY = cpixel.Y;
                if (colorInDepthX > 0 && colorInDepthX < 640 && colorInDepthY >= 0 &&
                        colorInDepthY < 480 )
                {
                    short depth = depthFrame[depthIndex].Depth;
                    if (depth < MaxMaxDepth / 2 && depth >= 0)
                    {
                        //transform
                        depth = TwoD_intensityTable[depthIndex / 640, depth];
                        // look up in intensity table
                        byte color = mappingTable[(ushort)depth];
                        int colorIndex = colorInDepthY * 640 + colorInDepthX;

                        // Write color pixel to buffer
                        depthPixels[colorIndex] = color;
                    }
                   
                }
                   
               
            }
        }


        private void InitializeTransformTable(int maxDepth, float angle)
        {
            if (!initializeTransformTable)
            {
                TwoD_intensityTable = new short[480, maxDepth];

                // transform depth map
                for (int y = 0; y < 480; y++)
                {
                    for (int i = 0; i < maxDepth; i++)
                    {
                        TwoD_intensityTable[y,i] = TransformDepth(i, y, angle);
                    }
                }

                initializeTransformTable = true;

            }
        }

        const float Tan215 = 0.3939f;
        private short TransformDepth(int initialDepth, int y, double angleTan)
        {
            if (initialDepth == 0)
            {
                return 0;
            }
            float height = initialDepth * y / 240 * Tan215;
            short newdepth = Convert.ToInt16(initialDepth + angleTan * height / (1 - angleTan * Tan215));

            return newdepth;
        }

        /// <summary>
        /// Returns the depth-to-color mapping table.
        /// </summary>
        /// <param name="minDepth">The minimum reliable depth value.</param>
        /// <param name="maxDepth">The maximum reliable depth value.</param>
        /// <param name="depthTreatment">The depth treatment to apply.</param>
        /// <returns>The color mapping table.</returns>
        private byte[] GetColorMappingTable(int minDepth, int maxDepth, float tranformTanAngle, short culling)
        {
          
            culling = culling <= 0 ? (short)minDepth : culling;
           
            
            // Fill in the "near" portion of the table with solid color
            for (int i = 0; i < minDepth; i++)
            {
                this.intensityTable[i] = 255;
            }

            // Fill in the "far" portion of the table with solid color
            for (int i = maxDepth; i < MaxMaxDepth; i++)
            {
                this.intensityTable[i] = 255;
            }


            // Fill in values that will be rendered normally with a gray gradient
            for (int i = minDepth; i < culling; i++)
            {

                this.intensityTable[i] = (byte)(255f / (maxDepth - minDepth) * (i - minDepth));
            }
            // culling far part
            for (int i = culling; i < maxDepth; i++)
            {

                this.intensityTable[i] = 255;
            }
            return this.intensityTable;
        }

        private byte[] GetColorMappingTable(int minDepth, int maxDepth)
        {

            // Fill in the "near" portion of the table with solid color
            for (int i = 0; i < minDepth; i++)
            {
                this.intensityTable[i] = 255;
            }

            // Fill in the "far" portion of the table with solid color
            for (int i = maxDepth; i < MaxMaxDepth; i++)
            {
                this.intensityTable[i] = 255;
            }


            // Fill in values that will be rendered normally with a gray gradient
            for (int i = minDepth; i < maxDepth; i++)
            {

                this.intensityTable[i] = (byte)(255f / (maxDepth - minDepth) * (i - minDepth));
            }

            return this.intensityTable;
        }



    }
}