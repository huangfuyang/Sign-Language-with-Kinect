using System.Security.Principal;
using System.Windows.Controls;
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
        public static float DistanceTo(this Point p1, Point p2)
        {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));

        }

        public static PointF add(this PointF p1, PointF p2)
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



        public static Rectangle GetBoundingRectangle(this Point[] ps)
        {
            int x = ps.Min(a => a.X);
            int y = ps.Min(a => a.Y);
            int width = ps.Max(a => a.X) - x;
            int height = ps.Max(a => a.Y) - y;
            return new Rectangle(x, y, width, height);
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
        public static Point[] ToPoints(this PointF[] p)
        {
            return p.Select(x => x.ToPoint()).ToArray();

        }

        public static double Distance(this Point2i p1, Point2i p2)
        {
            return Math.Sqrt((p1.x - p2.x)*(p1.x - p2.x) + (p1.y - p2.y)*(p1.y - p2.y));
        }



        public static bool IsCloseTo(this Point2i p1, Point2i p2, double minDis = 20)
        {
            return p1.Distance(p2) < minDis;
        }

        public static bool IsCloseTo(this Rectangle r1, Rectangle r2, int minDis = 5)
        {
            Rectangle tr1 = new Rectangle(r1.Location.X - minDis, r1.Location.Y - minDis, r1.Width + minDis * 2, r1.Height + minDis * 2);
            Rectangle tr2 = new Rectangle(r2.Location.X - minDis, r2.Location.Y - minDis, r2.Width + minDis * 2, r2.Height + minDis * 2);
            return tr1.IntersectsWith(tr2);
        }

        public static Point[] GetPoints(this Rectangle r)
        {
            Point[] ps = new Point[4];
            ps[0] = new Point(r.X, r.Y);
            ps[1] = new Point(r.X + r.Width, r.Y);
            ps[2] = new Point(r.X + r.Width, r.Y + r.Height);
            ps[3] = new Point(r.X, r.Y + r.Height);
            return ps;

        }
        public static PointF GetCenter(this PointF[] p)
        {
            float X = p.Average(x => x.X);
            float Y = p.Average(x => x.Y);
            return new PointF(X, Y);

        }

        public static Point GetCenter(this Point[] p)
        {
            double X = p.Average(x => x.X);
            double Y = p.Average(x => x.Y);
            return new Point((int)X, (int)Y);
        
        }

        public static float TanWith(this Point p1, Point p2)
        {
            if (p1.X - p2.X == 0)
            {
                return Single.MaxValue;
            }
            float result = (float)Math.Abs(p1.Y - p2.Y) / Math.Abs(p1.X - p2.X);
            return result;
        }

        public static float RealTanWith(this Point p1, Point p2)
        {
            if (p1.X - p2.X == 0)
            {
                return Single.MaxValue;
            }
            float result = (float)(p2.Y - p1.Y) / (p2.X - p1.X);
            return result;
        }

        public static float AngleBetween(this PointF p1, PointF p2)
        {
            float dotproduct = p1.Dotproduct(p2);
            return dotproduct / p1.Length() / p2.Length();
        }

        public static float Tan(this PointF p)
        {
            if (p.X == 0)
            {
                return Single.MaxValue;
            }
            return p.Y / p.X;
        }

        public static float Dotproduct(this PointF p1, PointF p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
        public static float Length(this PointF p)
        {
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static MCvBox2D ToCvBox2D(this Rectangle r)
        {
            var box = new MCvBox2D();
            box.center= new PointF(r.GetXCenter(), r.GetYCenter());
            box.size = new SizeF(r.Width,r.Height);
            box.angle = 0;
            return box;
        }

        public static PointF GetCenter(this Rectangle r)
        {
            return new PointF(r.X+r.Width/2,r.Y+r.Height/2);
        }

        public static Point2i GetCenter2i(this Rectangle r)
        {
            return new Point2i()
            {
                x = (r.X + r.Width / 2),
                y = (r.Y + r.Height / 2)
            };
        }


        public static Rectangle Unite(this Rectangle r1, Rectangle r2)
        {
            int x = Math.Min(r1.X, r2.X);
            int y = Math.Min(r1.Y, r2.Y);
            int width = Math.Max(r1.Right, r2.Right) - x;
            int height= Math.Max(r1.Bottom, r2.Bottom) - y;
            return new Rectangle(x,y,width,height);
        }
    }
}
