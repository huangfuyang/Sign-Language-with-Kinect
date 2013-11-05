using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CURELab.SignLanguage.Debugger.Model;

namespace CURELab.SignLanguage.Debugger
{
    class TrajectoryView
    {
        private Image m_drawboard;

        private double m_renderWidth;
        private double m_renderHeight;

        private DrawingGroup m_drawingGroup;
        private DrawingImage m_imageSource;

        public TrajectoryView(Image img)
        {
            // Create the drawing group we'll use for drawing
            this.m_drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.m_imageSource = new DrawingImage(this.m_drawingGroup);

            // Display the drawing using our image control
            img.Source = this.m_imageSource;

            m_drawboard = img;
            m_renderHeight = img.Height;
            m_renderWidth = img.Width;

            Console.WriteLine(m_renderWidth + " ", m_renderHeight);
        }
    

        public void DrawTrajectory(List<Point> pointListLeft, List<Point> pointListRight)
        {
            using (DrawingContext dc = m_drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, m_renderWidth, m_renderHeight));

                for (int i = 0; i < pointListLeft.Count; i++)
                {
                    Point item = pointListLeft[i];
                    DrawPoint(dc, item, (1 + i) * 0.6);

                    if (i != pointListLeft.Count - 1)
                    {
                        DrawLine(dc, pointListLeft[i], pointListLeft[i + 1], (i + 1) * 1.5);
                    }
                }

                for (int i = 0; i < pointListRight.Count; i++)
                {
                    Point item = pointListRight[i];
                    DrawPoint(dc, item, (1 + i) * 0.6);

                    if (i != pointListRight.Count - 1)
                    {
                        DrawLine(dc, pointListRight[i], pointListRight[i + 1], (i + 1) * 1.5);
                    }
                }

            }
        }


        private void DrawPoint(DrawingContext dc, Point point, double thickness)
        {


            dc.DrawEllipse(Brushes.LightYellow, null, SkeletonToScreen(point), thickness, thickness);
        }

        private void DrawLine(DrawingContext dc, Point point_i, Point point_j, double thickness)
        {
            dc.DrawLine(new Pen(Brushes.SkyBlue, thickness), SkeletonToScreen(point_i), SkeletonToScreen(point_j));
        }

        private Point SkeletonToScreen(Point point)
        {

            return point;
        }

        public void ClearBoard()
        {
            using (DrawingContext dc = m_drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, m_renderWidth, m_renderHeight));
            }

        }

    }
}
