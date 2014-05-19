// author：      Administrator
// created time：2014/1/15 15:52:25
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public static class ImageConverter
    {

        public static void UpdateWriteBMP(WriteableBitmap wbmp, Bitmap bmp)
        {

            lock (bmp)
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

                try
                {
                    wbmp.Lock();

                    wbmp.WritePixels(
                      new Int32Rect(0, 0, wbmp.PixelWidth, wbmp.PixelHeight),
                      bmpData.Scan0,
                      bmpData.Width * bmpData.Height * 4,
                      bmpData.Stride);
                    wbmp.Unlock();


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    bmp.UnlockBits(bmpData);
                }
            }


        }

        public static Image<Bgra, byte> Array2Image(byte[] data, int width, int height, int stride)
        {
            unsafe
            {
                fixed (byte* p_data = data)
                {

                    return new Image<Bgra, byte>(width, height, stride, (IntPtr)p_data);

                }
            }
        }

        public static System.Drawing.Bitmap WriteBMPtoBMP(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
                return bitmap;
            }
        }

        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
               (
                   bmp.GetHbitmap(),
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions()
               );

            return bitmapSource;
        }

        public static byte[] ToByteArray(this float[] array)
        {
            byte[] byteArray = new byte[array.Length * sizeof(float)];
            for (int i = 0; i < array.Length; i++)
            {
                BitConverter.GetBytes(array[i]).CopyTo(byteArray, i * sizeof(float));
            }
            return byteArray;
        }


    }
}