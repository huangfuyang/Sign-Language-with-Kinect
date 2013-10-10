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

using CURELab.SignLanguage.Debugger.ViewModel;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace CURELab.SignLanguage.Debugger
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

        private EnumerableDataSource<VelocityPoint> _shownData_V_Right;
        public EnumerableDataSource<VelocityPoint> ShownData_V_Right
        {
            get { return _shownData_V_Right; }
            set { _shownData_V_Right = value; }
        }


        private VelocityPointCollection _velocityPointCollection_right;
        public VelocityPointCollection VelocityPointCollection_right
        {
            get { return _velocityPointCollection_right; }
            set { _velocityPointCollection_right = value; }
        }

        private VelocityPointCollection _velocityPointCollection_left_1;
        public VelocityPointCollection VelocityPointCollection_left_1
        {
            get { return _velocityPointCollection_left_1; }
            set { _velocityPointCollection_left_1 = value; }
        }

        private VelocityPointCollection _velocityPointCollection_left_2;

        public VelocityPointCollection VelocityPointCollection_left_2
        {
            get { return _velocityPointCollection_left_2; }
            set { _velocityPointCollection_left_2 = value; }
        }

        private VelocityPointCollection _velocityPointCollection_left_3;

        public VelocityPointCollection VelocityPointCollection_left_3
        {
            get { return _velocityPointCollection_left_3; }
            set { _velocityPointCollection_left_3 = value; }
        }

        private VelocityPointCollection _velocityPointCollection_right_1;

        public VelocityPointCollection VelocityPointCollection_right_1
        {
            get { return _velocityPointCollection_right_1; }
            set { _velocityPointCollection_right_1 = value; }
        }
        private VelocityPointCollection _velocityPointCollection_right_2;

        public VelocityPointCollection VelocityPointCollection_right_2
        {
            get { return _velocityPointCollection_right_2; }
            set { _velocityPointCollection_right_2 = value; }
        }
        private VelocityPointCollection _velocityPointCollection_right_3;

        public VelocityPointCollection VelocityPointCollection_right_3
        {
            get { return _velocityPointCollection_right_3; }
            set { _velocityPointCollection_right_3 = value; }
        }

        private VelocityPointCollection _velocityPointCollection_left;
        public VelocityPointCollection VelocityPointCollection_left
        {
            get { return _velocityPointCollection_left; }
            set { _velocityPointCollection_left = value; }
        }

        private VelocityPointCollection _accelerationPointCollection_right;
        public VelocityPointCollection AccelerationPointCollection_right
        {
            get { return _accelerationPointCollection_right; }
            set { _accelerationPointCollection_right = value; }
        }

        private VelocityPointCollection _accelerationPointCollection_left;
        public VelocityPointCollection AccelerationPointCollection_left
        {
            get { return _accelerationPointCollection_left; }
            set { _accelerationPointCollection_left = value; }
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

        List<ShownData> _dataList1;
        public List<ShownData> DataList1
        {
            get { return _dataList1; }
            set { _dataList1 = value; }
        }

        List<ShownData> _dataList2;
        public List<ShownData> DataList2
        {
            get { return _dataList2; }
            set { _dataList2 = value; }
        }

        List<ShownData> _dataList3;
        public List<ShownData> DataList3
        {
            get { return _dataList3; }
            set { _dataList3 = value; }
        }

        public DataManager()
        {
            InitializeChartData();
        }

        private void InitializeChartData()
        {
            VelocityPointCollection_left = new VelocityPointCollection();
            VelocityPointCollection_right = new VelocityPointCollection();
            AccelerationPointCollection_left = new VelocityPointCollection();
            AccelerationPointCollection_right = new VelocityPointCollection();

            VelocityPointCollection_left_1 = new VelocityPointCollection();

            VelocityPointCollection_left_2 = new VelocityPointCollection();

            VelocityPointCollection_left_3 = new VelocityPointCollection();

            VelocityPointCollection_right_1 = new VelocityPointCollection();
            VelocityPointCollection_right_2 = new VelocityPointCollection();
            VelocityPointCollection_right_3 = new VelocityPointCollection();

            ImageTimeStampList = new List<int>();
            SegmentTimeStampList = new List<int>();
            DataList1 = new List<ShownData>();
            DataList2 = new List<ShownData>();
            DataList3 = new List<ShownData>();
        }

        public int GetCurrentDataTime(int timestamp)
        {
            List<ShownData> Datalist = null;

            if (DataList1.Count != 0)
            {
                Datalist = DataList1;
            }
            else if (DataList2.Count != 0)
            {
                Datalist = DataList2;
            }
            else if (DataList3.Count != 0)
            {
                Datalist = DataList3;
            }

            foreach (ShownData item in Datalist)
            {
                if (item.timeStamp <= timestamp + 40)
                {
                    return item.timeStamp;
                }
            }
            return Datalist[0].timeStamp;
        }
        

        public int GetCurrentTimestamp(int frameNumber)
        {
            if (frameNumber >= ImageTimeStampList.Count)
            {
                return ImageTimeStampList.Last();
            }
            return ImageTimeStampList[frameNumber];
        }

        public void ClearAll()
        {
       
            VelocityPointCollection_left.Clear();
            VelocityPointCollection_right.Clear();
            _accelerationPointCollection_left.Clear();
            AccelerationPointCollection_right.Clear();

            VelocityPointCollection_left_1.Clear();
            VelocityPointCollection_left_2.Clear();
            VelocityPointCollection_left_3.Clear();
            VelocityPointCollection_right_1.Clear();
            VelocityPointCollection_right_2.Clear();
            VelocityPointCollection_right_3.Clear();

            ImageTimeStampList.Clear();
            SegmentTimeStampList.Clear();
            DataList1.Clear();
            DataList2.Clear();
            DataList3.Clear();
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
