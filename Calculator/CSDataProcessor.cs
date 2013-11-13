// author：      fyhuang
// created time：2013/11/11 16:33:39
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CURELab.SignLanguage.DataModule;

namespace CURELab.SignLanguage.Calculator
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class CSDataProcessor : IDataProcessor
    {
        private DataManager dataManager;
        private static CSDataProcessor SingletonInstance;
        public static CSDataProcessor GetSingletonInstance()
        {
            if (SingletonInstance == null)
            {
                SingletonInstance = new CSDataProcessor();
            }
            return SingletonInstance;
        }
        private CSDataProcessor()
        {
            dataManager = DataManager.GetSingletonInstance();
        }

        public void GaussianFilter(ref double[] data)
        {

        }

        public double[] MeanFilter(double[] data)
        {
            return data;
        }

        public double[] MeanFilter(double[] data, int[] time)
        {
            double[] result = new double[dataManager.DataModelList.Count];
            bool[] temp = new bool[dataManager.DataModelList.Count];
            data.CopyTo(result, 0);
            for (int i = 1; i < result.Length-1; i++)
            {           
                temp[i] = (data[i + 1] - data[i]) * (data[i] - data[i - 1]) < 0;       
            }
            bool isInNoise = false;
            int index = 0;
            for (int i = 1; i < result.Length - 1; i++)
            {
                /* mean i-1 i+1
              result[i] = y[i - 1] + (y[i + 1] - y[i-1]) * 
                  (time[i] - time[i - 1]) / (time[i + 1] - time[i - 1]);
                */
                /*
                double alpha = 0.7;
                result[i] = alpha * result[i - 1] + (1 - alpha) * data[i];*/
                if (!isInNoise && temp[i])
                {
                    isInNoise = true;
                    index = i;
                }
                if (isInNoise && !temp[i])
                {
                    isInNoise = false;
                    for (int j = index ; j < i ; j++)
                    {
                        result[j] = data[index - 1] + (data[i] - data[index - 1]) *
                        (time[j] - time[index - 1]) / (time[i] - time[index - 1]);
                    }
                }
                
            }

            return result;
        }

        public double[] GetMeanPosition()
        {

            double[] result = new double[dataManager.DataModelList.Count];
            

            return result;
        }
        public double[] CalVelocity(double[] data, int[] time)
        {
            if (data.Length < 1)
            {
                return data;
            }
            double[] result = new double[data.Length];
            result[0] = 0;
            for (int i = 1; i < data.Length - 1; i++)
            {
                result[i] = Math.Abs(data[i+1] - data[i-1])/(time[i+1]-time[i-1]);
            }
            return Normalize(result);
        }

        public double[] CalAcceleration(double[] data)
        {
            return data;
        }

        public double[] CalAcceleration(double[] data, int[] time)
        {
            if (data.Length < 1)
            {
                return data;
            }
            double[] result = new double[data.Length];
            result[0] = 0;
            for (int i = 1; i < data.Length - 1; i++)
            {
                result[i] = Math.Abs(data[i+1] + data[i - 1] - 2* data[i]) / 
                            (time[i+1] - time[i - 1]);
            }
            return Normalize(result);
        }

        public double[] Normalize(double[] data)
        {
            double max = data.Max();
            return data.Select(d => d / max).ToArray();

        }

        public double[] CalVelocity()
        {
            double[] result = new double[dataManager.DataModelList.Count];
            result[0] = 0;
            DataModel[] data = dataManager.DataModelList.ToArray();
            for (int i = 1; i < result.Length-1; i++)
            {
                double deltaRight = Math.Sqrt(Math.Pow(data[i + 1].position_right.x - data[i - 1].position_right.x, 2) +
                          Math.Pow(data[i + 1].position_right.y - data[i - 1].position_right.y, 2) +
                          Math.Pow(data[i + 1].position_right.z - data[i - 1].position_right.z, 2));
                double deltaLeft = Math.Sqrt(Math.Pow(data[i + 1].position_left.x - data[i - 1].position_left.x, 2) +
                          Math.Pow(data[i + 1].position_left.y - data[i - 1].position_left.y, 2) +
                          Math.Pow(data[i + 1].position_left.z - data[i - 1].position_left.z, 2));

                result[i] = Math.Sqrt((deltaRight * deltaRight +deltaLeft * deltaLeft)/2)  / (data[i + 1].timeStamp - data[i - 1].timeStamp);
            }
            return Normalize(result);
        }
        public double[] CalVelocity(double[] data)
        {
            throw new NotImplementedException();
        }

        
    }
}