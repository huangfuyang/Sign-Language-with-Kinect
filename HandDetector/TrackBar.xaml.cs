using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// TrackBar.xaml 的交互逻辑
    /// </summary>
    public unsafe partial class TrackBar : UserControl,ISubject
    {
        private double _min;
        public double Min { get { return _min; } set { sld_main.Minimum = value; _min = value; } }
        private double _max;
        public double Max { get { return _max; } set { sld_main.Maximum = value; _max = value; } }

        public double Value
        {
            get { return sld_main.Value; }
            set
            {
                sld_main.Value = value;
            }
        }
        private double* PtrThresh;
        
        public string ValueName
        {
            get { return lbl_Name.Content.ToString(); }
            set
            {
                lbl_Name.Content = value;
            }
        }
  

        public unsafe TrackBar(double* ptr)
        {
            DataContext = this;
            InitializeComponent();
            Min = 0;
            Max = 100;
            ValueName = "default";
            PtrThresh = ptr;
        }

        private void sld_main_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            unsafe
            {
                *PtrThresh = e.NewValue;
            }
            NotifyAll(new DataTransferEventArgs(e.NewValue));
        }






        #region ISubject 成员

        public event DataTransferEventHandler m_dataTransferEvent;

        public void NotifyAll(DataTransferEventArgs e)
        {
            if (m_dataTransferEvent != null)
            {
                m_dataTransferEvent(this, e);
            }
        }

        #endregion
    }
}
