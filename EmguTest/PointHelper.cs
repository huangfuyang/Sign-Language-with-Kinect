using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmguTest
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
        public static Point ToPoint(this PointF p)
        {
            try
            {
                int x = (int)p.X < 0 ? 0 : (int)p.X;
                int y = (int)p.Y < 0 ? 0 : (int)p.Y;
                return new Point(x, y);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return new Point(0, 0);
            
        }
        
    }
}
