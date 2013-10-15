using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CURELab.SignLanguage.Debugger
{
    class DataReader
    {
        string _address;
        DataManager _dataManager;
        int _baseStamp;

        public DataReader(string address, DataManager dataManager)
        {
            _address = address;
            _dataManager = dataManager;
            _baseStamp = Int32.MaxValue;
        }


        /// <summary>
        /// read all data to data manager
        /// </summary>
        /// <returns></returns>
        public bool ReadData()
        {
            try
            {
                _dataManager.ClearAll();
                GetBaseStamp();
                OpenDataStream(DataType.SegmentData);
                OpenDataStream(DataType.VideoTimestampData);
                OpenDataStream(DataType.TestedData);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }



        private void GetBaseStamp()
        {
            StreamReader timeReader = new StreamReader(_address + FilePath.VideoTimestampFilePostfix);
            StreamReader dataReader = null;

            if (FilePath.DataFile1Postfix != "")
            {
                dataReader = new StreamReader(_address + FilePath.DataFile1Postfix);
            }
            else if (FilePath.DataFile2Postfix != "")
            {
                dataReader = new StreamReader(_address + FilePath.DataFile2Postfix);
            }
            else if (FilePath.DataFile3Postfix != "")
            {
                dataReader = new StreamReader(_address + FilePath.DataFile3Postfix);
            }
            else
            {
                _baseStamp = 0;
            }

            string line = timeReader.ReadLine();
            _baseStamp = Convert.ToInt32(line);

            line = dataReader.ReadLine();
            _baseStamp = Math.Min(Convert.ToInt32(line.Split(' ')[0]), _baseStamp);
        }



        private void OpenDataStream(DataType type)
        {
            // read video timestamps
            if (type == DataType.VideoTimestampData)
            {
                StreamReader timeReader = new StreamReader(_address + FilePath.VideoTimestampFilePostfix);
                string line = timeReader.ReadLine();
                while (!String.IsNullOrWhiteSpace(line))
                {
                    int timeStamp = Convert.ToInt32(line) - _baseStamp;
                  
                    _dataManager.ImageTimeStampList.Add(timeStamp);
                    line = timeReader.ReadLine();
                }
                timeReader.Close();
            }

            // read segment data
            if (type == DataType.SegmentData)
            {
                StreamReader segReader = new StreamReader(_address + FilePath.SegmentFilePostfix);

                string segLine = segReader.ReadLine();
                while (!String.IsNullOrWhiteSpace(segLine))
                {
                    int segTimestamp = Convert.ToInt32(segLine) - _baseStamp;
                    if (!_dataManager.SegmentTimeStampList.Contains(segTimestamp))
                    {
                        _dataManager.SegmentTimeStampList.Add(segTimestamp);

                    }
                    segLine = segReader.ReadLine();

                }
                segReader.Close();
            }

            // read data
            if (type == DataType.TestedData)
            {
                if (FilePath.DataFile1Postfix != "")
                {
                    ReadFileData(_address + FilePath.DataFile1Postfix, _dataManager.DataList1);
                }
                if (FilePath.DataFile2Postfix != "")
                {
                    ReadFileData(_address + FilePath.DataFile2Postfix, _dataManager.DataList2);
                }
                if (FilePath.DataFile3Postfix != "")
                {
                    ReadFileData(_address + FilePath.DataFile3Postfix, _dataManager.DataList3);
                }
            }
        }


        private void ReadFileData(string fileURL, List<ShownData> target)
        {

            StreamReader dataReader = new StreamReader(fileURL);

            string line = dataReader.ReadLine();

            while (!String.IsNullOrWhiteSpace(line))
            {
                string[] words = line.Split(' ');
                int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
               
                double valLeft = Convert.ToDouble(words[1]);
                double valRight = Convert.ToDouble(words[2]);

                target.Add(new ShownData()
                {
                    timeStamp = dataTime,
                    val_left = valLeft,
                    val_right = valRight,

                });

                line = dataReader.ReadLine();
            }
            target.Reverse();
            dataReader.Close();

        }
    }
}
