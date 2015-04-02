using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using CURELab.SignLanguage.HandDetector.Pages;
using CURELab.SignLanguage.StaticTools;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

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

        private KinectController m_KinectController;
        private KinectStudioController m_kinectStudioController;
        private SocketManager socket;
        private Dictionary<string, string> fullWordList;
        private KinectSensorChooser sensorChooser;

        //pages
        private UserControl startPage;
        public MainWindow()
        {
            InitializeComponent();
            this.startPage = new StartPage();
            //this.kinectRegionGrid.Children.Add(this.startPage);
            m_kinectStudioController = KinectStudioController.GetSingleton();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Show();
            this.sensorChooser = new KinectSensorChooser();
            //this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            //sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            //this.sensorChooser.Start(); 
            RegisterThreshold("V min", ref OpenCVController.VMIN, 150, 134);
            //RegisterThreshold("cannyThresh", ref OpenCVController.CANNY_CONNECT_THRESH, 100, 22);
            //RegisterThreshold("play speed", ref OpenNIController.SPEED, 2, 1);
            //RegisterThreshold("diff", ref KinectController.DIFF, 10, 7);
            //RegisterThreshold("Culling", ref KinectSDKController.CullingThresh, 100, 40);

            //Menu_Kinect_Click(this, e);  //test
            //Menu_TrainHand_Click(this, e);//train hand shape
            Menu_Server_Click(this, e);//real time recog
            //Menu_Train_Click(this, e);//train data
            //MenuItem_Test_Click(this, e);//test

            // load word list
            fullWordList = new Dictionary<string, string>();
            using (var wl = File.Open("wordlist.txt", FileMode.Open))
            {
                using (StreamReader sw = new StreamReader(wl))
                {
                    var line = sw.ReadLine();
                    while (!String.IsNullOrEmpty(line))
                    {
                        var t = line.Split();
                        fullWordList.Add(t[1], t[3]);
                        line = sw.ReadLine();
                    }
                    sw.Close();
                }
                wl.Close();
            }


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

  

        private void AsnycDataRecieved()
        {
            var t = new Thread(new ThreadStart(DataRecieved));
            t.Start();
        }

        private void DataRecieved()
        {
            if (socket != null)
            {
                Console.WriteLine("waiting reponse");

                while (true)
                {
                    try
                    {
                        var r = socket.GetResponse();
                        if (r == null)
                        {
                            Console.WriteLine("finish receive");
                            break;
                        }
                        r = r.Trim();
                        if (r != "")
                        {
                            Console.WriteLine("Data:{0}", r);
                            var w = String.Format("Data:{0} word:{1}", r, fullWordList[r]);
                            Console.WriteLine(w);
                            this.Dispatcher.BeginInvoke((Action)delegate()
                                    {
                                        lbl_candidate1.Content = fullWordList[r];
                                    });
                        }
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("receive data error:{0}",e);
                    }

                }

            }
        }

        private void Menu_Server_Click(object sender, RoutedEventArgs e)
        {
            //socket = SocketManager.GetInstance("127.0.0.1", 51243);
            socket = SocketManager.GetInstance("137.189.89.29", 51243);
            //socket = SocketManager.GetInstance("192.168.209.67", 51243);

            ResetAll();
            m_KinectController = KinectRealtime.GetSingletonInstance(socket,this);
            this.DataContext = m_KinectController;
            m_KinectController.Initialize();
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.DepthWriteBitmap;
            m_KinectController.Start();
            AsnycDataRecieved();
        }

        private void Menu_Train_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            m_KinectController = KinectTrainer.GetSingletonInstance();
            this.DataContext = m_KinectController;
            m_KinectController.Initialize();
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.DepthWriteBitmap;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                (m_KinectController as KinectTrainer).OpenDir(fbd.SelectedPath);

            //fbd.SelectedPath = @"D:\Kinect data\test\";
            //lbl_folder.Content = files.Length.ToString();
        }

        private void Menu_Kinect_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            m_KinectController = KinectSDKController.GetSingletonInstance();
            this.DataContext = m_KinectController;
            m_KinectController.Initialize();
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.DepthWriteBitmap;
            m_KinectController.Start();
        }


        private void Menu_TrainHand_Click(object sender, RoutedEventArgs e)
        {
            ResetAll();
            m_KinectController = KinectHandShape.GetSingletonInstance();
            this.DataContext = m_KinectController;
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
                    statusBarKinectStudio.Text = "Kinect Studio Connected";
                }
                else
                {
                    statusBarKinectStudio.Text = "Kinect Studio Not Connected";
                }
            }
        }
        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e)
        {
            IsConnected = m_kinectStudioController.Connect();
        }

        private void MenuItem_Start_Click(object sender, RoutedEventArgs e)
        {
            IsConnected = m_kinectStudioController.Start();
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
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();
                    args.NewSensor.Start();
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

            //if (!error)
            //{
            //    this.kinectRegion.KinectSensor = systemStatusCollection.CurrentKinectSensor = args.NewSensor;
            //    systemStatusCollection.IsKinectAllSet = true;
            //}
            //else
            //{
            //    this.kinectRegion.KinectSensor = systemStatusCollection.CurrentKinectSensor = null;
            //    systemStatusCollection.IsKinectAllSet = false;
            //}
        }


    }
}
