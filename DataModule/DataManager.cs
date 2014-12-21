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
using System.Collections;

namespace CURELab.SignLanguage.DataModule
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class DataManager 
    {

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

        private List<DataModel> _dataList;
        public List<DataModel> DataModelList
        {
            set { _dataList = value; }
            get
            {
                return _dataList;
            }
        }

        private static DataManager singletonInstance;

        private DataManager()
        {
            InitializeChartData();
        }

        public static DataManager GetSingletonInstance()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new DataManager();
            }
            return singletonInstance;
        }

        private void InitializeChartData()
        {
           
            ImageTimeStampList = new List<int>();
            AcSegmentTimeStampList = new List<int>();
            VeSegmentTimeStampList = new List<int>();
            AngSegmentTimeStampList = new List<int>();
            DataModelList = new List<DataModel>();
            True_Segmented_Words = new SegmentedWordCollection();
        }

        public int GetFrameNumber(double timestamp)
        {
            return (int)Math.Round((double)timestamp * 3 / 100);
        }

        
        public DataModel GetCurrentData(int timestamp)
        {
            return _dataList[GetFrameNumber(timestamp)]; 
            
        }


     

        public List<Point> GetLeftPositions(int frame)
        {
            List<Point> result = new List<Point>();

            if (frame > 0)
            {
                int count = 0;
                while (frame >= 0 && count < 8)
                {
                    DataModel item = DataModelList[frame];
                    result.Add(item.position_2D_left);
                    count++;
                    frame--;
                }
                result.Reverse();
            }

            return result;
        }

        public List<Point> GetRightPositions(int frame)
        {       
            List<Point> result = new List<Point>();

            if (frame > 0)
            {
                int count = 0;
                while (frame >= 0 && count < 8)
                {
                    DataModel item = DataModelList[frame];
                    result.Add(item.position_2D_right);
                    count++;
                    frame--;
                }
                result.Reverse();
            }

            return result;
        }

        public void ClearAll()
        {
            ImageTimeStampList.Clear();
            True_Segmented_Words.Clear();
            AcSegmentTimeStampList.Clear();
            VeSegmentTimeStampList.Clear();
            AngSegmentTimeStampList.Clear();

            DataModelList.Clear();
        }

    }
}
