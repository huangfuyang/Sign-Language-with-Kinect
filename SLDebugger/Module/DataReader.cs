﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CURELab.SignLanguage.Debugger.Model;

namespace CURELab.SignLanguage.Debugger.Module
{
    public class DataReader
    {
        string _address;
        DataManager _dataManager;
        XMLReader _configReader;

        int _baseStamp;

        public DataReader(string address)
        {
            _address = address;
            _dataManager = ModuleManager.DataManager;
            _configReader = ModuleManager.ConfigReader;
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



        private void GetBaseStamp()
        {
            StreamReader timeReader = new StreamReader(_address + _configReader.GetFileName(FileName.TIMESTAMP));
            StreamReader dataReader = new StreamReader(_address + _configReader.GetFileName(FileName.VELOCITY));

            string line = timeReader.ReadLine();
            _baseStamp = Convert.ToInt32(line);

            line = dataReader.ReadLine();
            _baseStamp = Math.Min(Convert.ToInt32(line.Split(' ')[0]), _baseStamp);
            timeReader.Close();
            dataReader.Close();

        }


        private void LoadImageTimestamp()
        {
            StreamReader timeReader = new StreamReader(_address + _configReader.GetFileName(FileName.TIMESTAMP));
            string line = timeReader.ReadLine();
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
            StreamReader segReader = new StreamReader(_address + _configReader.GetFileName(FileName.SEGMENTATION));

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
        private void  LoadData()
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

                    double vl = Convert.ToDouble(words[1]);
                    double vr = Convert.ToDouble(words[2]);

                    if (!_dataManager.DataModelDic.ContainsKey(dataTime))
                    {
                        _dataManager.DataModelDic.Add(dataTime, new DataModel()
                        {
                            timeStamp = dataTime,
                        }); 
                    }

                    _dataManager.DataModelDic[dataTime].v_left = vl;
                    _dataManager.DataModelDic[dataTime].v_right = vr;
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

                    double al = Convert.ToDouble(words[1]);
                    double ar = Convert.ToDouble(words[2]);

                    if (!_dataManager.DataModelDic.ContainsKey(dataTime))
                    {
                        _dataManager.DataModelDic.Add(dataTime, new DataModel()
                        {
                            timeStamp = dataTime,
                        });
                    }

                    _dataManager.DataModelDic[dataTime].a_left = al;
                    _dataManager.DataModelDic[dataTime].a_right = ar;
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

                    double angleleft = Convert.ToDouble(words[1]);
                    double angleright = Convert.ToDouble(words[2]);

                    if (!_dataManager.DataModelDic.ContainsKey(dataTime))
                    {
                        _dataManager.DataModelDic.Add(dataTime, new DataModel()
                        {
                            timeStamp = dataTime,
                        });
                    }

                    _dataManager.DataModelDic[dataTime].angle_left = angleleft;
                    _dataManager.DataModelDic[dataTime].angle_right = angleright;
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

                    double leftx = Convert.ToDouble(words[1]);
                    double lefty = Convert.ToDouble(words[2]);
                    double leftz = Convert.ToDouble(words[3]);

                    double rightx = Convert.ToDouble(words[4]);
                    double righty = Convert.ToDouble(words[5]);
                    double rightz = Convert.ToDouble(words[6]);

                    if (!_dataManager.DataModelDic.ContainsKey(dataTime))
                    {
                        _dataManager.DataModelDic.Add(dataTime, new DataModel()
                        {
                            timeStamp = dataTime,
                        });
                    }

                    _dataManager.DataModelDic[dataTime].position_left = new Vec3() {x = leftx,y = lefty,z = leftz };
                    _dataManager.DataModelDic[dataTime].position_right = new Vec3() { x = rightx, y = righty, z = rightz };
                    line = skeletonReader.ReadLine();
                }
                skeletonReader.Close();

            }

            if (File.Exists(_address + _configReader.GetFileName(FileName.WORDS)))
            {
                StreamReader wordReader = new StreamReader(_address + _configReader.GetFileName(FileName.WORDS), Encoding.Default);
                string line = wordReader.ReadLine();
        

                while (!String.IsNullOrWhiteSpace(line))
                {
                    string[] words = line.Split(' ');
                    string content = words[0];
                 
                    int startTime = Convert.ToInt32(words[1]);
                    int endTime = Convert.ToInt32(words[2]);

                    _dataManager.Segmented_Words.Add(new SegmentedWordModel(content, startTime, endTime));
                    line = wordReader.ReadLine();
                }

                wordReader.Close();
            }



            _dataManager.DataModelDic.Reverse();

        }

     
    }
}