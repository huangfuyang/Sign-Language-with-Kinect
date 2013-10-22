﻿using System;
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

namespace CURELab.SignLanguage.Debugger
{
    class GraphView
    {
        private ChartPlotter _chartPlotter;
        private LineGraph lastSigner;
        private List<LineGraph> _splitLineList;


        public GraphView(ChartPlotter cht)
        {          
            _chartPlotter = cht;
            _chartPlotter.Legend.AutoShowAndHide = false;
            _chartPlotter.LegendVisible = false;
            _splitLineList = new List<LineGraph>();
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

        public LineGraph AddSplitLine(int split, double stroke, double min, double max, bool isSegment, Color color)
        {
            var tempPoints = new TwoDimensionViewPointCollection();
            var v_right = new EnumerableDataSource<TwoDimensionViewPoint>(tempPoints);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Value);
            tempPoints.Add(new TwoDimensionViewPoint(max, split));
            tempPoints.Add(new TwoDimensionViewPoint(min, split));
            LineGraph newSplit = _chartPlotter.AddLineGraph(v_right, color, stroke, "seg line");
            if (isSegment)
            {
                _splitLineList.Add(newSplit);
            }


            return newSplit;
        }

     

        /// <summary>
        /// 
        /// 
        /// </summary>
        public void ShowSplitLine()
        {
            foreach (LineGraph item in _splitLineList)
            {
                item.AddToPlotter(_chartPlotter);
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public void ClearSplitLine()
        {
            foreach (LineGraph item in _splitLineList)
            {
                item.Remove();
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
            lastSigner = AddSplitLine(split, 1, min, max, false, Colors.Black);

        }

        /// <summary>
        /// 
        /// clear all the graphs
        /// </summary>

        public void ClearAllGraph()
        {
            _chartPlotter.Children.RemoveAll(typeof(ElementMarkerPointsGraph));
            _chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        public void ClearGraph()
        {
            
        }



    }
}
