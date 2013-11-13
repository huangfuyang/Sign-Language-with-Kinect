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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using CURELab.SignLanguage.Debugger.ViewModel;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace CURELab.SignLanguage.Debugger.View
{
    /// <summary>
    /// ChartView.xaml 的交互逻辑
    /// </summary>
    public partial class ChartView : UserControl
    {
        private LineGraph lastSigner;
        private List<LineGraph> _accSegLineList;
        private List<LineGraph> _velSegLineList;
        private List<LineGraph> _angSegLineList;
        private List<RectangleHighlight> _rectList;

        public string Title
        {
            get
            {
                return title.Content.ToString();
            }
            set
            {
                title.Content = value;
            }
        }
        public ChartView()
        {
            InitializeComponent();
            chart.Legend.AutoShowAndHide = false;
            chart.LegendVisible = false;
            _accSegLineList = new List<LineGraph>();
            _velSegLineList = new List<LineGraph>();
            _angSegLineList = new List<LineGraph>();
            _rectList = new List<RectangleHighlight>();

        }
        /// <summary>
        /// add new line graph
        /// </summary>
        /// <param name="name">line graph name</param>
        /// <param name="datasource">graph data source </param>
        /// <param name="pen">pen color</param>
        /// <param name="isShow">is show at beginning</param>
        public void AddLineGraph(string name, TwoDimensionViewPointCollection datasource, Pen pen, bool isShow)
        {

            CheckBox newCheckBox = new CheckBox();
            newCheckBox.Content = name;
            newCheckBox.IsChecked = isShow;
            newCheckBox.Foreground = pen.Brush;
            lb_main.Items.Add(newCheckBox);
             
            LineAndMarker<ElementMarkerPointsGraph> line = AppendLineGraph(datasource, pen, name);
            Binding binding = new Binding("IsChecked")
            {
                Converter = new BoolToVisibilityConverter(),
                Mode = BindingMode.OneWay,
                Source = newCheckBox
            };

            line.LineGraph.SetBinding(LineGraph.VisibilityProperty, binding);
            line.MarkerGraph.SetBinding(LineGraph.VisibilityProperty, binding);

        }

        /// <summary>
        /// Append VelocityPointCollection to the graph
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="pen"></param>
        /// <param name="description"></param>

        private LineAndMarker<ElementMarkerPointsGraph> AppendLineGraph(TwoDimensionViewPointCollection collection, Pen pen, string description)
        {
            CircleElementPointMarker pointMaker = new CircleElementPointMarker();
            pointMaker.Size = 3;
            pointMaker.Brush = Brushes.Yellow;
            pointMaker.Fill = Brushes.Purple;

            var v = new EnumerableDataSource<TwoDimensionViewPoint>(collection);
            v.SetXMapping(x => x.TimeStamp);
            v.SetYMapping(y => y.Value);
            return chart.AddLineGraph(v, pen, pointMaker, new PenDescription(description));

        }


        /// <summary>
        /// draw split line
        /// </summary>
        /// <param name="split"></param>
        /// <param name="stroke"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>

        public LineGraph AddSplitLine(int split, double stroke, double min, double max, SegmentType segType, Color color)
        {
            var tempPoints = new TwoDimensionViewPointCollection();
            var v_right = new EnumerableDataSource<TwoDimensionViewPoint>(tempPoints);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Value);
            tempPoints.Add(new TwoDimensionViewPoint(max, split));
            tempPoints.Add(new TwoDimensionViewPoint(min, split));

            LineGraph newSplit;
            if (segType == SegmentType.NotSegment)
            {
                newSplit = chart.AddLineGraph(v_right, color, stroke, "seg line");
            }
            else
            {
                newSplit = new LineGraph(v_right);
                newSplit.LinePen = new Pen(new SolidColorBrush(color), stroke);
            }
            if (segType == SegmentType.AccSegment)
            {
                _accSegLineList.Add(newSplit);
            }
            if (segType == SegmentType.VelSegment)
            {
                _velSegLineList.Add(newSplit);
            }
            if (segType == SegmentType.AngSegment)
            {
                _angSegLineList.Add(newSplit);
            }


            return newSplit;
        }

        public void AddRect(int start, int end, Brush color, double height = 1)
        {
            RectangleHighlight rec = new RectangleHighlight();
            rec.Bounds = new System.Windows.Rect(start, 0, end - start, height);
            rec.Fill = color;
            rec.Opacity = 0.5;
            _rectList.Add(rec);
            //  _chartPlotter.Children.Add(rec);
        }


        public void RemoveRect()
        {
            foreach (RectangleHighlight rect in _rectList)
            {
                chart.Children.Remove(rect);
            }
        }

        public void ShowRect()
        {
            foreach (RectangleHighlight rect in _rectList)
            {
                chart.Children.Add(rect);
            }
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        public void ShowSplitLine(bool isShowAcc, bool isShowVel, bool isShowAng)
        {
            if (isShowAcc)
            {
                foreach (LineGraph item in _accSegLineList)
                {
                    item.AddToPlotter(chart);
                }
            }
            if (isShowVel)
            {
                foreach (LineGraph item in _velSegLineList)
                {
                    item.AddToPlotter(chart);
                }
            }
            if (isShowAng)
            {
                foreach (LineGraph item in _angSegLineList)
                {
                    item.AddToPlotter(chart);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public void ClearSplitLine(bool isClearAcc, bool isClearVel, bool isClearAng)
        {
            if (isClearAcc)
            {
                foreach (LineGraph item in _accSegLineList)
                {
                    item.Remove();
                }
            }
            if (isClearVel)
            {
                foreach (LineGraph item in _velSegLineList)
                {
                    item.Remove();
                }
            }
            if (isClearAng)
            {
                foreach (LineGraph item in _angSegLineList)
                {
                    item.Remove();
                }
            }
        }

        /// <summary>
        /// 
        /// draw signer
        /// </summary>
        /// <param name="split"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void DrawSigner(int split, double min, double max)
        {
            if (lastSigner != null)
            {
                lastSigner.Remove();
            }
            lastSigner = AddSplitLine(split, 1, min, max, SegmentType.NotSegment, Colors.Black);

        }

        /// <summary>
        /// 
        /// clear all the graphs
        /// </summary>

        public void ClearAllGraph()
        {
            chart.Children.RemoveAll(typeof(ElementMarkerPointsGraph));
            chart.Children.RemoveAll(typeof(LineGraph));
            chart.Children.RemoveAll(typeof(RectangleHighlight));
            lb_main.Items.Clear();
            _accSegLineList.Clear();
            _velSegLineList.Clear();
            _angSegLineList.Clear();
            _rectList.Clear();
        }

        public void SetYRestriction(double ylow, double yhigh)
        {
            ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();
            restr.YRange = new DisplayRange(ylow, yhigh);
            chart.Viewport.Restrictions.Add(restr);
        }

        public void SetXRestriction(double xlow, double xhigh)
        {
            ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();
            restr.XRange = new DisplayRange(xlow, xhigh);
            chart.Viewport.Restrictions.Add(restr);
        }
    }

    [ValueConversion(typeof(System.Windows.Visibility), typeof(bool))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Visibility visi = (System.Windows.Visibility)value;
            if (visi == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
