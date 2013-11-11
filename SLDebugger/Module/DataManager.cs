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
using System.Collections;

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

        private List<IList> DataListCollection;


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


        private TwoDimensionViewPointCollection _X_right_position;

        public TwoDimensionViewPointCollection X_right_position
        {
            get { return _X_right_position; }
            set { _X_right_position = value; }
        }

        private TwoDimensionViewPointCollection _Y_filtered_right_position;

        public TwoDimensionViewPointCollection Y_filtered_right_position
        {
            get { return _Y_filtered_right_position; }
            set { _Y_filtered_right_position = value; }
        }

        private TwoDimensionViewPointCollection _X_filtered_right_position;

        public TwoDimensionViewPointCollection X_filtered_right_position
        {
            get { return _X_filtered_right_position; }
            set { _X_filtered_right_position = value; }
        }

        private TwoDimensionViewPointCollection _X_left_position;

        public TwoDimensionViewPointCollection X_left_position
        {
            get { return _X_left_position; }
            set { _X_left_position = value; }
        }

        private TwoDimensionViewPointCollection _Y_filtered_left_position;

        public TwoDimensionViewPointCollection Y_filtered_left_position
        {
            get { return _Y_filtered_left_position; }
            set { _Y_filtered_left_position = value; }
        }

        private TwoDimensionViewPointCollection _X_filtered_left_position;

        public TwoDimensionViewPointCollection X_filtered_left_position
        {
            get { return _X_filtered_left_position; }
            set { _X_filtered_left_position = value; }
        }


        private SegmentedWordCollection _segmented_words;

        public SegmentedWordCollection True_Segmented_Words
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

        List<int> _acSegmentTimeStampList;
        public List<int> AcSegmentTimeStampList
        {
            get { return _acSegmentTimeStampList; }
            set { _acSegmentTimeStampList = value; }
        }

        List<int> _veSegmentTimeStampList;

        public List<int> VeSegmentTimeStampList
        {
            get { return _veSegmentTimeStampList; }
            set { _veSegmentTimeStampList = value; }
        }

        List<int> _angSegmentTimeStampList;

        public List<int> AngSegmentTimeStampList
        {
            get { return _angSegmentTimeStampList; }
            set { _angSegmentTimeStampList = value; }
        }



        private Dictionary<int, DataModel> _dataModelDic;
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
            DataListCollection = new List<IList>();

            V_Left_Points = new TwoDimensionViewPointCollection();
            V_Right_Points = new TwoDimensionViewPointCollection();
            A_Left_Points = new TwoDimensionViewPointCollection();
            Angle_Left_Points = new TwoDimensionViewPointCollection();
            A_Right_Points = new TwoDimensionViewPointCollection();
            Angle_Right_Points = new TwoDimensionViewPointCollection();
           
            True_Segmented_Words = new SegmentedWordCollection();

            Y_Right_Points = new TwoDimensionViewPointCollection();
            Y_Left_Points = new TwoDimensionViewPointCollection();
            X_right_position = new TwoDimensionViewPointCollection();
            X_left_position = new TwoDimensionViewPointCollection();
            Y_filtered_right_position = new TwoDimensionViewPointCollection();
            Y_filtered_left_position = new TwoDimensionViewPointCollection();
            X_filtered_right_position = new TwoDimensionViewPointCollection();
            X_filtered_left_position = new TwoDimensionViewPointCollection();

            ImageTimeStampList = new List<int>();
            AcSegmentTimeStampList = new List<int>();
            VeSegmentTimeStampList = new List<int>();
            AngSegmentTimeStampList = new List<int>();
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
            ImageTimeStampList.Clear();

            V_Left_Points.Clear();
            A_Left_Points.Clear();
            Angle_Left_Points.Clear();
            V_Right_Points.Clear();
            A_Right_Points.Clear();
            Angle_Right_Points.Clear();

            Y_Left_Points.Clear();
            Y_Right_Points.Clear();
            Y_filtered_right_position.Clear();
            Y_filtered_left_position.Clear();

            X_right_position.Clear();
            X_left_position.Clear();
            X_filtered_right_position.Clear();
            X_filtered_left_position.Clear();
            True_Segmented_Words.Clear();
            AcSegmentTimeStampList.Clear();
            VeSegmentTimeStampList.Clear();
            AngSegmentTimeStampList.Clear();

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
