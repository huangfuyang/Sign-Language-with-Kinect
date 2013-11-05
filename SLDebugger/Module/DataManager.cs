// author：      fyhuang
// created time：2013/10/8 0:52:59
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CURELab.SignLanguage.Debugger.Model;
using CURELab.SignLanguage.Debugger.ViewModel;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows;
using CURELab.SignLanguage.Debugger.Model;

namespace CURELab.SignLanguage.Debugger.Module
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class DataManager : INotifyPropertyChanged
    {


        private int _maxVelocity;
        public int MaxVelocity
        {
            get { return _maxVelocity; }
            set { _maxVelocity = value; this.OnPropertyChanged("MaxVelocity"); }
        }

        private int _minVelocity;
        public int MinVelocity
        {
            get { return _minVelocity; }
            set { _minVelocity = value; this.OnPropertyChanged("MinVelocity"); }
        }


        private TwoDimensionViewPointCollection _v_rightPoints;
        public TwoDimensionViewPointCollection V_Right_Points
        {
            get { return _v_rightPoints; }
            set { _v_rightPoints = value; }
        }

        private TwoDimensionViewPointCollection _v_leftPoints;
        public TwoDimensionViewPointCollection V_Left_Points
        {
            get { return _v_leftPoints; }
            set { _v_leftPoints = value; }
        }

        private TwoDimensionViewPointCollection _a_left_Points;

        public TwoDimensionViewPointCollection A_Left_Points
        {
            get { return _a_left_Points; }
            set { _a_left_Points = value; }
        }

        private TwoDimensionViewPointCollection _angle_left;

        public TwoDimensionViewPointCollection Angle_Left_Points
        {
            get { return _angle_left; }
            set { _angle_left = value; }
        }

        private TwoDimensionViewPointCollection _y_left;

        public TwoDimensionViewPointCollection Y_Left_Points
        {
            get { return _y_left; }
            set { _y_left = value; }
        }

        private TwoDimensionViewPointCollection _y_right;

        public TwoDimensionViewPointCollection Y_Right_Points
        {
            get { return _y_right; }
            set { _y_right = value; }
        }
        
        private TwoDimensionViewPointCollection _a_right_points;

        public TwoDimensionViewPointCollection A_Right_Points
        {
            get { return _a_right_points; }
            set { _a_right_points = value; }
        }

        private TwoDimensionViewPointCollection _angle_right_points;

        public TwoDimensionViewPointCollection Angle_Right_Points
        {
            get { return _angle_right_points; }
            set { _angle_right_points = value; }
        }

     
        private TwoDimensionViewPointCollection _y_position_points;

        public TwoDimensionViewPointCollection Y_position_points
        {
            get { return _y_position_points; }
            set { _y_position_points = value; }
        }


        private SegmentedWordCollection _segmented_words;

        public SegmentedWordCollection Segmented_Words
        {
            get { return _segmented_words; }
            set { _segmented_words = value; }
        } 


        List<int> _imageTimeStampList;
        public List<int> ImageTimeStampList
        {
            get { return _imageTimeStampList; }
            set { _imageTimeStampList = value; }
        }

        List<int> _segmentTimeStampList;
        public List<int> SegmentTimeStampList
        {
            get { return _segmentTimeStampList; }
            set { _segmentTimeStampList = value; }
        }

        private Dictionary<int , DataModel> _dataModelDic;
        public Dictionary<int, DataModel> DataModelDic
        {
            get { return _dataModelDic; }
            set { _dataModelDic = value; }
        }


        public DataManager()
        {
            InitializeChartData();
        }

        private void InitializeChartData()
        {
        
            V_Left_Points = new TwoDimensionViewPointCollection();
            V_Right_Points = new TwoDimensionViewPointCollection();
            A_Left_Points = new TwoDimensionViewPointCollection();
            Angle_Left_Points = new TwoDimensionViewPointCollection();
            A_Right_Points = new TwoDimensionViewPointCollection();
            Angle_Right_Points = new TwoDimensionViewPointCollection();
            Y_Right_Points = new TwoDimensionViewPointCollection();
            Y_Left_Points = new TwoDimensionViewPointCollection();
            Segmented_Words = new SegmentedWordCollection();

        
            Y_position_points = new TwoDimensionViewPointCollection();
            ImageTimeStampList = new List<int>();
            SegmentTimeStampList = new List<int>();
            DataModelDic = new Dictionary<int, DataModel>();
        }

        public int GetCurrentDataTime(int timestamp)
        {
            

            foreach (int key in DataModelDic.Keys)
            {
                if (key > timestamp + 40)
                {
                    return key;
                }
            }
            return 0;
        }
        

        public int GetCurrentTimestamp(int frameNumber)
        {
            if (frameNumber >= ImageTimeStampList.Count)
            {
                return ImageTimeStampList.Last();
            }
            return ImageTimeStampList[frameNumber];
        }

        public List<Point> GetLeftPositions(int timestamp)
        {
            List<int> keys = new List<int>(DataModelDic.Keys);

            int index = keys.IndexOf(timestamp);

            List<Point> result = new List<Point>();

            if (index > 0)
            {
                int count = 0;
                while (index >= 0 && count < 8)
                {
                    DataModel item = DataModelDic[keys[index]];
                    result.Add(item.position_2D_left);
                    count++;
                    index--;
                }
                result.Reverse();
            }

            return result;
        }

        public List<Point> GetRightPositions(int timestamp)
        {
            List<int> keys = new List<int>(DataModelDic.Keys);

            int index = keys.IndexOf(timestamp);

            List<Point> result = new List<Point>();

            if (index > 0)
            {
                int count = 0;
                while (index >= 0 && count < 8)
                {
                    DataModel item = DataModelDic[keys[index]];
                    result.Add(item.position_2D_right);     
                    count++;
                    index--;
                }
                result.Reverse();
            }

            return result;
        }

        


        public void ClearAll()
        {
            V_Left_Points.Clear();
            A_Left_Points.Clear();
            Angle_Left_Points.Clear();
            V_Right_Points.Clear();
            A_Right_Points.Clear();
            Angle_Right_Points.Clear();
            Segmented_Words.Clear();
            ImageTimeStampList.Clear();
            SegmentTimeStampList.Clear();
            Segmented_Words.Clear();
            Y_Left_Points.Clear();
            Y_Right_Points.Clear();
            DataModelDic.Clear();
        }

    

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
