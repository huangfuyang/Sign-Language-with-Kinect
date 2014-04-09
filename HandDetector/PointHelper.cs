using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.HandDetector
{
    static class PointHelper
    {
        public static float DistanceTo(this PointF p1, PointF p2)
        {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        
        }

        public static  PointF add(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }


        public static float GetTrueArea(this MCvBox2D rec)
        {
            PointF[] pl = rec.GetVertices();
            return pl[0].DistanceTo(pl[1]) * pl[1].DistanceTo(pl[2]);
        }

        public static int GetRectArea(this Rectangle rect)
        {
            return rect.Size.Height * rect.Size.Width;
        }
        public static int GetXCenter(this Rectangle rect)
        {
            return (rect.Right + rect.Left) / 2;
        }
        public static int GetYCenter(this Rectangle rect)
        {
            return (rect.Top + rect.Bottom) / 2;
        }



      
        public static System.Drawing.Point ToPoint(this PointF p)
        {
            try
            {
                int x = (int)p.X < 0 ? 0 : (int)p.X;
                int y = (int)p.Y < 0 ? 0 : (int)p.Y;
                return new System.Drawing.Point(x, y);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return new System.Drawing.Point(0, 0);

        }

        public static bool IsCloseTo(this Rectangle r1, Rectangle r2, int minDis = 5)
        {
            Rectangle tr1 = new Rectangle(r1.Location.X - minDis, r1.Location.Y - minDis, r1.Width + minDis * 2, r1.Height + minDis * 2);
            Rectangle tr2 = new Rectangle(r2.Location.X - minDis, r2.Location.Y - minDis, r2.Width + minDis * 2, r2.Height + minDis * 2);
            return tr1.IntersectsWith(tr2);
        }
        


    }
}
