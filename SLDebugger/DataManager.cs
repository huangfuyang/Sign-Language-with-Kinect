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

        List<int> _imageTimeStampList;
        public List<int> ImageTimeStampList
        {
            get { return _imageTimeStampList; }
            set { _imageTimeStampList = value; }
        }

        List<ShownData> _dataList;
        public List<ShownData> DataList
        {
            get { return _dataList; }
            set { _dataList = value; }
        }

        public DataManager()
        {
            InitializeChartData();
        }

        private void InitializeChartData()
        {
            VelocityPointCollection_right = new VelocityPointCollection();
            AccelerationPointCollection_right = new VelocityPointCollection();

            ImageTimeStampList = new List<int>();
            DataList = new List<ShownData>();
        }

        public ShownData GetCurrentData(int timestamp)
        {
            foreach (ShownData item in DataList)
            {
                if (item.timeStamp <= timestamp + 35)
                {
                    return item;
                }
            }
            return DataList[0];
        }

        public int GetCurrentTimestamp(int frameNumber)
        {
            if (frameNumber >= ImageTimeStampList.Count)
            {
                return ImageTimeStampList.Last();
            }
            return ImageTimeStampList[frameNumber];
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
