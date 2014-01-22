// author：      Administrator
// created time：2014/1/14 16:03:44
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class KinectController : INotifyPropertyChanged
    {
        protected KinectController()
        {
            m_OpenCVController = OpenCVController.GetSingletonInstance();
        }
        private WriteableBitmap colorWriteBitmap;
        public WriteableBitmap ColorWriteBitmap
        {
            get { return colorWriteBitmap; }
            protected set { colorWriteBitmap = value; }
        }

        private WriteableBitmap depthWriteBitmap;
        public WriteableBitmap DepthWriteBitmap
        {
            get { return depthWriteBitmap; }
            protected set { depthWriteBitmap = value; }
        }

        private WriteableBitmap processedBitmap;
        public WriteableBitmap ProcessedBitmap
        {
            get { return processedBitmap; }
            protected set { processedBitmap = value; }
        }

        private WriteableBitmap edgeBitmap;
        public WriteableBitmap EdgeBitmap
        {
            get { return edgeBitmap; }
            protected set { edgeBitmap = value; }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set 
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }


        protected OpenCVController m_OpenCVController;

        protected static KinectController singleInstance;

        public virtual void Initialize(String uri = null) { }
        public virtual void Start() { }
        public virtual void Shutdown() { }

        public void Reset()
        {
            singleInstance = null;
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