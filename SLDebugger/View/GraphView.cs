using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.IO;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System.Windows.Media;

using CURELab.SignLanguage.Debugger.ViewModel;
using Microsoft.Research.DynamicDataDisplay.Charts;

namespace CURELab.SignLanguage.Debugger
{
    class GraphView
    {
        private ChartPlotter _chartPlotter;
        private LineGraph lastSigner;
        private List<LineGraph> _accSegLineList;
        private List<LineGraph> _velSegLineList;
        private List<LineGraph> _angSegLineList;
        private List<RectangleHighlight> _rectList;


        public GraphView(ChartPlotter cht)
        {          
            _chartPlotter = cht;
            _chartPlotter.Legend.AutoShowAndHide = false;
            _chartPlotter.LegendVisible = false;
            _accSegLineList = new List<LineGraph>();
            _velSegLineList = new List<LineGraph>();
            _angSegLineList = new List<LineGraph>();
            _rectList = new List<RectangleHighlight>();

        }

        /// <summary>
        /// Append VelocityPointCollection to the graph
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="pen"></param>
        /// <param name="description"></param>

        public LineAndMarker<ElementMarkerPointsGraph> AppendLineGraph(TwoDimensionViewPointCollection collection, Pen pen, string description)
        {
            CircleElementPointMarker pointMaker = new CircleElementPointMarker();
            pointMaker.Size = 3;
            pointMaker.Brush = Brushes.Yellow;
            pointMaker.Fill = Brushes.Purple;

            var v = new EnumerableDataSource<TwoDimensionViewPoint>(collection);
            v.SetXMapping(x => x.TimeStamp);
            v.SetYMapping(y => y.Value);
            return _chartPlotter.AddLineGraph(v, pen, pointMaker, new PenDescription(description));
            
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
                 newSplit = _chartPlotter.AddLineGraph(v_right, color, stroke, "seg line");
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

        public void AddRect(int start, int end)
        {
            RectangleHighlight rec = new RectangleHighlight();
            rec.Bounds = new System.Windows.Rect(start, 0, end - start, 1);
            rec.Fill = Brushes.LightPink;
            rec.Opacity = 0.5;
            _rectList.Add(rec);
          //  _chartPlotter.Children.Add(rec);
        }


        public void RemoveRect()
        {
            foreach (RectangleHighlight rect in _rectList)
            {
                _chartPlotter.Children.Remove(rect);
            }
        }

        public void ShowRect()
        {
            foreach (RectangleHighlight rect in _rectList)
            {
                _chartPlotter.Children.Add(rect);
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
                    item.AddToPlotter(_chartPlotter);
                }
            }
            if (isShowVel)
            {
                foreach (LineGraph item in _velSegLineList)
                {
                    item.AddToPlotter(_chartPlotter);
                }
            }
            if (isShowAng)
            {
                foreach (LineGraph item in _angSegLineList)
                {
                    item.AddToPlotter(_chartPlotter);
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
            if(isClearVel){
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
            _chartPlotter.Children.RemoveAll(typeof(ElementMarkerPointsGraph));
            _chartPlotter.Children.RemoveAll(typeof(LineGraph));
            _chartPlotter.Children.RemoveAll(typeof(RectangleHighlight));
            _accSegLineList.Clear();
            _velSegLineList.Clear();
            _angSegLineList.Clear();
            _rectList.Clear();
        }

        public void ClearGraph()
        {
            
        }



    }
}
