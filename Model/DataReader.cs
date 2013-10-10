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

        public DataReader(string address, DataManager dataManager){
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
                OpenDataStream(DataType.AccelerationVelocityData); 
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
            StreamReader accReader = new StreamReader(_address + FilePath.AccelerationFilePostfix);

            string line = timeReader.ReadLine();
            _baseStamp = Convert.ToInt32(line);

            line = accReader.ReadLine();
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

            // read acceleration
            if (type == DataType.AccelerationVelocityData)
            {
                StreamReader accReader = new StreamReader(_address + FilePath.AccelerationFilePostfix);
                StreamReader velReader = new StreamReader(_address + FilePath.VelociyFilePostfix);
                StreamReader segPointReader = new StreamReader(_address + FilePath.SegmentFilePostfix);
               
                string aLine = accReader.ReadLine();
                string vLine = velReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(vLine) && !String.IsNullOrWhiteSpace(aLine))
                {
                    string[] accWords = aLine.Split(' ');
                    string[] velWords = vLine.Split(' ');

                    // get data timestamp
                    int dataTime = Convert.ToInt32(accWords[0]) - _baseStamp;

                    // get acceleration
                    double aLeft = Convert.ToDouble(accWords[1]);
                    double aRight = Convert.ToDouble(accWords[2]);
                    
                    // get velocity
                    double vLeft = Convert.ToDouble(velWords[1]);
                    double vRight = Convert.ToDouble(velWords[2]);

                    // get segmentation time;
                    bool isSegpoint = _dataManager.SegmentTimeStampList.Contains(dataTime);

                    // add data to data manager
                    _dataManager.DataList.Add(new ShownData()
                    {
                        timeStamp = dataTime,
                        a_left = aLeft,
                        a_right = aRight,
                        v_left = vLeft,
                        v_right = vRight,
                        isSegmentPoint = isSegpoint
                    });

                  
                    // read new line
                    aLine = accReader.ReadLine();
                    vLine = velReader.ReadLine();
                }

                _dataManager.DataList.Reverse();
                accReader.Close();
                velReader.Close();
            }
        }
    }
}
