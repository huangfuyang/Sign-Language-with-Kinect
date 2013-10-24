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
    /// <summary>
    /// Interaction logic for TrajectoryWindow.xaml
    /// </summary>
    public partial class TrajectoryWindow : Window
    {
        private const float m_renderWidth = 640.0f;
        private const float m_renderHeight = 480.0f;



        private DrawingGroup m_drawingGroup;
        private DrawingImage m_imageSource;



        public TrajectoryWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.m_drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.m_imageSource = new DrawingImage(this.m_drawingGroup);
            
            // Display the drawing using our image control
            Image.Source = this.m_imageSource;
        }

        public void DrawTrajectory(List<Vec3> pointListLeft, List<Vec3> pointListRight){
            using (DrawingContext dc = m_drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, m_renderWidth, m_renderHeight));

                for (int i = 0; i < pointListLeft.Count;i++)
                {
                    Vec3 item = pointListLeft[i];
                    DrawPoint(dc, SkeletonToScreen(item), (1 + i) * 0.6 );
                    
                    if(i != pointListLeft.Count - 1){
                        DrawLine(dc, SkeletonToScreen(pointListLeft[i]), SkeletonToScreen(pointListLeft[i + 1]), (i +1) * 1.5);
                    }
                }

                for (int i = 0; i < pointListRight.Count; i++)
                {
                    Vec3 item = pointListRight[i];
                    DrawPoint(dc, SkeletonToScreen(item), (1 + i) * 0.6);

                    if (i != pointListRight.Count - 1)
                    {
                        DrawLine(dc, SkeletonToScreen(pointListRight[i]), SkeletonToScreen(pointListRight[i + 1]), (i +1)* 1.5);
                    }
                }
                    
            }
        }


        private void DrawPoint(DrawingContext dc, Point point, double thickness)
        {
            dc.DrawEllipse(Brushes.Red, null, point, thickness, thickness);
        }

        private void DrawLine(DrawingContext dc, Point point_i, Point point_j, double thickness)
        {
            dc.DrawLine(new Pen(Brushes.Green, thickness), point_i, point_j);
        }

        private Point SkeletonToScreen(Vec3 point)
        {

            double x = Math.Min((point.x + 0.5) / point.z* m_renderWidth * 1.2, m_renderWidth);
            double y = Math.Min((-point.y + 0.5) / point.z * m_renderHeight * 1.2, m_renderHeight);
            return new Point(x, y);
        }

    }
}
