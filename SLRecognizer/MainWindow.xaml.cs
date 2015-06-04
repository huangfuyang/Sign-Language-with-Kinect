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
using CURELab.SignLanguage.HandDetector;
using CURELab.SignLanguage.StaticTools;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace SLRecognizer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        protected const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        protected const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private KinectSensorChooser sensorChooser;
        //pages
        private MainUI mainUI;
        private KinectSensor sensor;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Show();
            var Datacontext = DataContextCollection.GetInstance();
            mainUI = new MainUI();
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            //Menu_Kinect_Click(this, e);  //test
            //Menu_TrainHand_Click(this, e);//train hand shape
            //Menu_Server_Click(this, e);//real time recog
            //Menu_Train_Click(this, e);//train data
            //MenuItem_Test_Click(this, e);//test
            //TrainOnline();
        

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sensor != null)
            {
                sensor.Stop();
            }

            sensorChooser.Stop();
            Environment.Exit(0);

        }


        private void Menu_Server_Click(object sender, RoutedEventArgs e)
        {

            //ResetAll();
            //m_KinectController = KinectRealtime.GetSingletonInstance(socket,this);
            //this.DataContext = m_KinectController;
            //m_KinectController.Start();
        }

        private void Menu_Train_Click(object sender, RoutedEventArgs e)
        {
            //ResetAll();
            //m_KinectController = KinectTrainer.GetSingletonInstance();
            //this.DataContext = m_KinectController;
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //DialogResult result = fbd.ShowDialog();
            //if (result == System.Windows.Forms.DialogResult.OK)
            //    (m_KinectController as KinectTrainer).OpenDir(fbd.SelectedPath);

            //fbd.SelectedPath = @"D:\Kinect data\test\";
            //lbl_folder.Content = files.Length.ToString();
        }

        private void Menu_Kinect_Click(object sender, RoutedEventArgs e)
        {
            //ResetAll();
            //m_KinectController = KinectSDKController.GetSingletonInstance();
            //this.DataContext = m_KinectController;
        }


        private void Menu_TrainHand_Click(object sender, RoutedEventArgs e)
        {
            //ResetAll();
            //m_KinectController = KinectHandShape.GetSingletonInstance();
            //this.DataContext = m_KinectController;
        }

        private void TrainOnline()
        {
            //ResetAll();
            //m_KinectController = KinectTrainOnline.GetSingletonInstance(this);
            //this.DataContext = m_KinectController;
        }



        private bool _isConnected;

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                if (_isConnected)
                {
                    //statusBarKinectStudio.Text = "Kinect Studio Connected";
                }
                else
                {
                    //statusBarKinectStudio.Text = "Kinect Studio Not Connected";
                }
            }
        }
        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e)
        {
            //IsConnected = m_kinectStudioController.Connect();
        }

        private void MenuItem_Start_Click(object sender, RoutedEventArgs e)
        {
            //IsConnected = m_kinectStudioController.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            bool error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException) { error = true; }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();
                    args.NewSensor.Start();
                    if (mainUI != null)
                    {
                        mainUI.ChangeSensor(args.NewSensor);
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }
            else
            {
                error = true;
            }
            if (!error)
            {
                sensor = args.NewSensor;

            }
            else
            {
                this.kinectRegion.KinectSensor = null;
                sensor = null;
            }

        }

        private void btnLearn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRecog_Click(object sender, RoutedEventArgs e)
        {
            if (sensor != null)
            {
                HideUI();
                kinectRegionGrid.Children.Add(mainUI);
                mainUI.Start(sensor);
            }

        }

        public void HideUI()
        {
            btnRecog.Visibility = Visibility.Collapsed;
            tbk_main.Visibility = Visibility.Collapsed;
        }

        public void ShowUI()
        {
            btnRecog.Visibility = Visibility.Visible;
            tbk_main.Visibility = Visibility.Visible;
        }


    }

}
