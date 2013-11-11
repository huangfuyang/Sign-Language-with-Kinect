// author：      fyhuang
// created time：2013/10/16 15:38:22
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CURELab.SignLanguage.Debugger.Module
{
    /// <summary>
    /// manage all modules
    /// </summary>
    public static class ModuleManager
    {
        private static DataManager _dataManager;
        public static DataManager DataManager
        {
            get
            {
                if (_dataManager == null)
                {
                    _dataManager = new DataManager();
                }
                return _dataManager;
            }
        }

        private static DataReader _dataReader;
        public static DataReader DataReader
        {
            get
            {

                return ModuleManager._dataReader;
            }
        }

        private static XMLReader _configReader;
        public static XMLReader ConfigReader
        {
            get
            {
                if (_configReader == null)
                {
                    string path = @"Config.xml";
                    _configReader = new XMLReader(path);
                }
                return ModuleManager._configReader;
            }
        }



        public static DataReader CreateDataReader(string address)
        {
            _dataReader = new DataReader(address);
            return DataReader;
        }

       
    }
}