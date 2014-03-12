using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

using Microsoft.Kinect;
using System.IO;

using Emgu.CV;
using Emgu.Util;
namespace CURELab.SignLanguage.HandDetector
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary
        public WriteableBitmap colorBitmap;


        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        public WriteableBitmap depthBitmap;

        public WriteableBitmap grayBitmap;
        private KinectController m_KinectController;
        private OpenCVController m_OpenCVController;

        public MainWindow()
        {
            InitializeComponent();
        }



        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            m_OpenCVController = OpenCVController.GetSingletonInstance();

            RegisterThreshold("canny", ref OpenCVController.CANNY_THRESH, 300, 1);
            RegisterThreshold("cannyThresh", ref OpenCVController.CANNY_CONNECT_THRESH, 500, 10);
            RegisterThreshold("play speed", ref OpenNIController.SPEED, 2, 1);
            RegisterThreshold("diff", ref OpenNIController.DIFF, 30, 10);


            Menu_ONI_Click(this, e);
        }

        private unsafe void RegisterThreshold(string valuename, ref double thresh, double max, double initialValue)
        {
            
            fixed (double* ptr = &thresh)
            {
                thresh = initialValue;
                TrackBar tcb = new TrackBar(ptr);
                tcb.Max = max;
                tcb.Margin = new Thickness(5);
                tcb.ValueName = valuename;
                initialValue = initialValue > max ? max : initialValue;
                tcb.Value = initialValue;
                SPn_right.Children.Add(tcb);
            }

        }



        private void Initialize()
        {
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_KinectController != null)
            {
                m_KinectController.Shutdown();

            }
        }


        private void Kinect_Setup()
        {

        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {

            if (m_KinectController != null)
            {
                m_KinectController.Shutdown();

            }
            Environment.Exit(0);
        }

        private void Menu_OpenNI_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            m_KinectController = OpenNIController.GetSingletonInstance();
            statusBar.DataContext = m_KinectController;
            m_KinectController.Initialize();
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.ProcessedDepthBitmap;
            m_KinectController.Start();
        }

        private void Menu_ONI_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".oni";
            ofd.Filter = "oni file|*.oni";
            if (ofd.ShowDialog() == true)
            {
                m_KinectController = OpenNIController.GetSingletonInstance();
                statusBar.DataContext = m_KinectController;
                m_KinectController.Initialize(ofd.FileName);
                this.img_color.Source = m_KinectController.ColorWriteBitmap;
                this.img_depth.Source = m_KinectController.ProcessedDepthBitmap;
                this.img_gray.Source = m_KinectController.GrayWriteBitmap;
                m_KinectController.Start();
            }

        }

        private void Menu_Kinect_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            m_KinectController = KinectSDKController.GetSingletonInstance();
            statusBar.DataContext = m_KinectController;
            m_KinectController.Initialize();
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.DepthWriteBitmap;
            m_KinectController.Start();
        }

        private void ResetAll()
        {
            if (m_KinectController != null)
            {
                m_KinectController.Shutdown();
                m_KinectController.Reset();
                m_KinectController = null;
                GC.Collect();
            }
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                        
                case Key.Space:
                    m_KinectController.TogglePause();
                    break;
                default:
                    break;
            }
        }
    }
}
