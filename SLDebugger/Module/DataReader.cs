using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CURELab.SignLanguage.DataModule;
using System.Windows.Media.Media3D;

namespace CURELab.SignLanguage.Debugger.Module
{
    public class DataReader
    {
        string _address;
        DataManager _dataManager;
        ConfigReader _configReader;

        int _baseStamp;

        public DataReader(string address)
        {
            _address = address;
            _dataManager = DataManager.GetSingletonInstance();
            _configReader = ConfigReader.GetSingletonConfigReader();
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
                LoadImageTimestamp();
                LoadSegmentationData();

                LoadData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        private void LoadImageTimestamp()
        {
            StreamReader timeReader = new StreamReader(_address + _configReader.GetFileName(FileName.TIMESTAMP));
            string line = timeReader.ReadLine();
            _baseStamp = Convert.ToInt32(line);
            while (!String.IsNullOrWhiteSpace(line))
            {
                int timeStamp = Convert.ToInt32(line) - _baseStamp;

                _dataManager.ImageTimeStampList.Add(timeStamp);
                line = timeReader.ReadLine();
            }
            timeReader.Close();
        }

        private void LoadSegmentationData()
        {
            StreamReader segReader;

            if (File.Exists(_address + _configReader.GetFileName(FileName.AC_SEGMENTATION)))
            {
                segReader = new StreamReader(_address + _configReader.GetFileName(FileName.AC_SEGMENTATION));
                string segLine = segReader.ReadLine();
                while (!String.IsNullOrWhiteSpace(segLine))
                {
                    int segTimestamp = Convert.ToInt32(segLine) - _baseStamp;
                    if (!_dataManager.AcSegmentTimeStampList.Contains(segTimestamp))
                    {
                        _dataManager.AcSegmentTimeStampList.Add(segTimestamp);

                    }
                    segLine = segReader.ReadLine();
                }
                segReader.Close();
            }

            if (File.Exists(_address + _configReader.GetFileName(FileName.VEL_SEGMENTATION)))
            {

                segReader = new StreamReader(_address + _configReader.GetFileName(FileName.VEL_SEGMENTATION));
                string segLine = segReader.ReadLine();
                while (!String.IsNullOrWhiteSpace(segLine))
                {
                    int segTimestamp = Convert.ToInt32(segLine) - _baseStamp;
                    if (!_dataManager.VeSegmentTimeStampList.Contains(segTimestamp))
                    {
                        _dataManager.VeSegmentTimeStampList.Add(segTimestamp);

                    }
                    segLine = segReader.ReadLine();
                }
                segReader.Close();
            }

            if (File.Exists(_address + _configReader.GetFileName(FileName.ANG_SEGMENTATION)))
            {
                segReader = new StreamReader(_address + _configReader.GetFileName(FileName.ANG_SEGMENTATION));
                string segLine = segReader.ReadLine();
                while (!String.IsNullOrWhiteSpace(segLine))
                {
                    int segTimestamp = Convert.ToInt32(segLine) - _baseStamp;
                    if (!_dataManager.AngSegmentTimeStampList.Contains(segTimestamp))
                    {
                        _dataManager.AngSegmentTimeStampList.Add(segTimestamp);

                    }
                    segLine = segReader.ReadLine();
                }
                segReader.Close();
            }
        }

        private void LoadData()
        {

            // read velo
            if (File.Exists(_address + _configReader.GetFileName(FileName.VELOCITY)))
            {
                StreamReader veloReader = new StreamReader(_address + _configReader.GetFileName(FileName.VELOCITY));
                string line = veloReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(' ');
                    int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
                    int frame = _dataManager.GetFrameNumber(dataTime);
                    double vl = Convert.ToDouble(words[1]);
                    double vr = Convert.ToDouble(words[2]);

                    if (_dataManager.DataModelList.Count < frame + 1)
                    {
                        _dataManager.DataModelList.Add(new DataModel() { timeStamp = frame });
                        continue;
                    }
                    _dataManager.DataModelList[frame].v_right = vr;
                    _dataManager.DataModelList[frame].v_left = vl;
                    line = veloReader.ReadLine();
                }
                veloReader.Close();

            }
            //read accleration
            if (File.Exists(_address + _configReader.GetFileName(FileName.ACCELERATION)))
            {
                StreamReader acclReader = new StreamReader(_address + _configReader.GetFileName(FileName.ACCELERATION));
                string line = acclReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(' ');
                    int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
                    int frame = _dataManager.GetFrameNumber(dataTime);

                    double al = Convert.ToDouble(words[1]);
                    double ar = Convert.ToDouble(words[2]);


                    if (_dataManager.DataModelList.Count < frame + 1)
                    {
                        _dataManager.DataModelList.Add(new DataModel() { timeStamp = frame });
                        continue;
                    }

                    _dataManager.DataModelList[frame].a_left = al;
                    _dataManager.DataModelList[frame].a_right = ar;
                    line = acclReader.ReadLine();
                }
                acclReader.Close();

            }
            //read angle
            if (File.Exists(_address + _configReader.GetFileName(FileName.ANGLE)))
            {
                StreamReader angleReader = new StreamReader(_address + _configReader.GetFileName(FileName.ANGLE));
                string line = angleReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(' ');
                    int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
                    int frame = _dataManager.GetFrameNumber(dataTime);

                    double angleleft = Convert.ToDouble(words[1]);
                    double angleright = Convert.ToDouble(words[2]);

                    if (_dataManager.DataModelList.Count < frame + 1)
                    {
                        _dataManager.DataModelList.Add(new DataModel() { timeStamp = frame });
                        continue;
                    }

                    _dataManager.DataModelList[frame].angle_left = angleleft;
                    _dataManager.DataModelList[frame].angle_right = angleright;
                    line = angleReader.ReadLine();
                }
                angleReader.Close();

            }
            //read skeleton
            if (File.Exists(_address + _configReader.GetFileName(FileName.SKELETON)))
            {
                StreamReader skeletonReader = new StreamReader(_address + _configReader.GetFileName(FileName.SKELETON));
                string line = skeletonReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(',');
                    int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
                    int frame = _dataManager.GetFrameNumber(dataTime);

                    double leftx = Convert.ToDouble(words[1]);
                    double lefty = Convert.ToDouble(words[2]);
                    double leftz = Convert.ToDouble(words[3]);

                    double rightx = Convert.ToDouble(words[4]);
                    double righty = Convert.ToDouble(words[5]);
                    double rightz = Convert.ToDouble(words[6]);


                    if (_dataManager.DataModelList.Count < frame + 1)
                    {
                        _dataManager.DataModelList.Add(new DataModel() { timeStamp = frame });
                        continue;
                    }

                    _dataManager.DataModelList[frame].position_left = new Vector3D() { X = leftx, Y = lefty, Z = leftz };
                    _dataManager.DataModelList[frame].position_right = new Vector3D() { X = rightx, Y = righty, Z = rightz };


                    line = skeletonReader.ReadLine();

                }
                skeletonReader.Close();

            }

            // read words
            if (File.Exists(_address + _configReader.GetFileName(FileName.WORDS)))
            {
                StreamReader wordReader = new StreamReader(_address + _configReader.GetFileName(FileName.WORDS), Encoding.UTF8);
                string line = wordReader.ReadLine();


                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(' ');
                    string content = words[0];

                    int startTime = _dataManager.GetFrameNumber(Convert.ToInt32(words[1]));
                    int endTime = _dataManager.GetFrameNumber(Convert.ToInt32(words[2]));

                    _dataManager.True_Segmented_Words.Add(new SegmentedWordModel(content, startTime, endTime));
                    line = wordReader.ReadLine();
                }

                wordReader.Close();
            }


            // read 2d position
            if (File.Exists(_address + _configReader.GetFileName(FileName.POSITION)))
            {
                StreamReader positionReader = new StreamReader(_address + _configReader.GetFileName(FileName.POSITION), Encoding.UTF8);
                string line = positionReader.ReadLine();

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(',');
                    int dataTime = Convert.ToInt32(words[0]) - _baseStamp;
                    int frame = _dataManager.GetFrameNumber(dataTime);

                    double leftx = Convert.ToDouble(words[1]);
                    double lefty = Convert.ToDouble(words[2]);

                    double rightx = Convert.ToDouble(words[3]);
                    double righty = Convert.ToDouble(words[4]);



                    if (_dataManager.DataModelList.Count < frame + 1)
                    {
                        _dataManager.DataModelList.Add(new DataModel() { timeStamp = frame });
                        continue;
                    }

                    _dataManager.DataModelList[frame].position_2D_left = new Point { x = leftx, y = lefty };
                    _dataManager.DataModelList[frame].position_2D_right = new Point { x = rightx, y = righty };

                    line = positionReader.ReadLine();
                }
            }
            // interpolate data
            bool isInGap = false;
            int startFrame = 0;
            foreach (var item in _dataManager.DataModelList)
            {

                if (!isInGap && item.timeStamp != _dataManager.DataModelList.IndexOf(item))
                {
                    isInGap = true;
                    continue;
                }
                if (isInGap && item.timeStamp == _dataManager.DataModelList.IndexOf(item))
                {
                    for (int i = startFrame + 1; i < item.timeStamp; i++)
                    {
                        double a = (double)(i-startFrame) / (item.timeStamp - startFrame);
                        _dataManager.DataModelList[i].timeStamp = i;
                        _dataManager.DataModelList[i].position_2D_left.x = (1 - a) * _dataManager.DataModelList[startFrame].position_2D_left.x + a * item.position_2D_left.x;
                        _dataManager.DataModelList[i].position_2D_left.y = (1 - a) * _dataManager.DataModelList[startFrame].position_2D_left.y + item.position_2D_left.y * a;
                        _dataManager.DataModelList[i].position_2D_right.x = (1 - a) * _dataManager.DataModelList[startFrame].position_2D_right.x + item.position_2D_right.x * a;
                        _dataManager.DataModelList[i].position_2D_right.y = (1 - a) * _dataManager.DataModelList[startFrame].position_2D_right.y + item.position_2D_right.y * a;
                        _dataManager.DataModelList[i].position_right = (1 - a) * _dataManager.DataModelList[startFrame].position_right + item.position_right * a;
                        _dataManager.DataModelList[i].position_left = (1 - a) * _dataManager.DataModelList[startFrame].position_left + item.position_left * a;
                        _dataManager.DataModelList[i].v_right = (1 - a) * _dataManager.DataModelList[startFrame].v_right + item.v_right * a;
                        _dataManager.DataModelList[i].v_left = (1 - a) * _dataManager.DataModelList[startFrame].v_left + item.v_left * a;
                    }
                    isInGap = false;
                }
                if (!isInGap)
                {
                    startFrame = item.timeStamp;
                }
            }

        }


    }
}
