﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CURELab.SignLanguage.HandDetector;
using CURELab.SignLanguage.StaticTools;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using UserControl = System.Windows.Controls.UserControl;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Microsoft.Kinect;
using TrackBar = CURELab.SignLanguage.HandDetector.TrackBar;

namespace SLRecognizer
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : UserControl
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
        private ImageViewer viewer;
        private SocketManager socket;
        public MainUI()
        {
            InitializeComponent();
            //Menu_Kinect_Click(this, e);  //test
            //Menu_TrainHand_Click(this, e);//train hand shape
            ServerMode();//real time recog
            //Menu_Train_Click(this, e);//train data
            //MenuItem_Test_Click(this, e);//test
            //TrainOnlineMode();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            //sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            //this.sensorChooser.Start(); 
            RegisterThreshold("V min", ref OpenCVController.VMIN, 150, 134);
            //RegisterThreshold("cannyThresh", ref OpenCVController.CANNY_CONNECT_THRESH, 100, 22);
            //RegisterThreshold("play speed", ref OpenNIController.SPEED, 2, 1);
            //RegisterThreshold("diff", ref KinectController.DIFF, 10, 7);
            //RegisterThreshold("Culling", ref KinectSDKController.CullingThresh, 100, 40);

         
            foreach (var row in DataContextCollection.GetInstance().fullWordList)
            {
                string name = row.Value;
                string id = row.Key;
                panelSignList.Children.Add(createKinectButton(id, name));
            }
            KinectScrollViewer.ScrollToVerticalOffset(100);
            viewer = new ImageViewer();

        }

        private KinectTileButton createKinectButton(string id,string name)
        {
            KinectTileButton button = new KinectTileButton();
            button.DataContext = id;
            button.Click += btnSignWord_Click;
            button.Content = name;
            button.Width = 250;
            button.Height = 110;
            button.FontSize = 48;
            SolidColorBrush brush = new SolidColorBrush(Brushes.Aqua.Color);
            brush.Opacity = 0.2;
            button.Background = brush;
            return button;
        }

        private void btnSignWord_Click(object sender, RoutedEventArgs e)
        {
            KinectTileButton button = (KinectTileButton)sender;
            string videoName = String.Format("Videos\\{0}.mpg", button.DataContext.ToString());
            viewer.Show();
            Thread t = new Thread(new ParameterizedThreadStart(PlayVideo));
            t.Start(videoName);

        }
        Capture _CCapture = null;
        private void PlayVideo(object file)
        {
            try
            {
                if (_CCapture != null)
                {
                    _CCapture.Dispose(); //dispose of current capture
                }
                _CCapture = new Capture(file as string);
                int FrameRate = (int) _CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
                int cframe = (int) _CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                int framenumber = (int) _CCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                while (_CCapture.Grab())
                {
                    var frame = _CCapture.RetrieveBgrFrame().Resize(800,600,INTER.CV_INTER_LINEAR);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) delegate()
                    {
                        viewer.Size = frame.Size;
                        viewer.Image = frame;
                    });
                    Thread.Sleep(1000/FrameRate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) delegate()
                {
                    viewer.Hide();
                });
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


        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e)
        {
            //IsConnected = m_kinectStudioController.Connect();
        }

        private void MenuItem_Start_Click(object sender, RoutedEventArgs e)
        {
            //IsConnected = m_kinectStudioController.Start();
        }

        private void ServerMode()
        {
            ResetAll();
            m_KinectController = KinectRealtime.GetSingletonInstance();
            //this.socket = SocketManager.GetInstance("137.189.89.29", 51243);
            //AsnycDataRecieved();
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
            m_KinectController.Start();
        }

        private void TrainOnlineMode()
        {
            ResetAll();
            m_KinectController = KinectTrainOnline.GetSingletonInstance();
        }

        private void AsnycDataRecieved()
        {
            var t = new Thread(new ThreadStart(DataRecieved));
            t.Start();
        }
        private string[] SPLIT = { "#TERMINATOR#" };
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
                        var list = r.Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var s in list)
                        {
                            try
                            {
                                if (s != "" && s != "0")
                                {
                                    Console.WriteLine("Data:{0}", s);
                                    var w = String.Format("Data:{0} word:{1}", s, DataContextCollection.GetInstance().fullWordList[s]);
                                    Console.WriteLine(w);
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                                    {
                                        lbl_candidate1.Content = DataContextCollection.GetInstance().fullWordList[s];
                                    });
                                }
                                if (s.ToLower() == "redo")
                                {
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate()
                                    {
                                        lbl_candidate1.Content = "請重做一次";
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                        }

                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("receive data error:{0}",e);
                    }

                }

            }
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

        public void Start(KinectSensor sensor)
        {
            m_KinectController.Initialize(sensor);
            m_KinectController.Start();
            this.DataContext = m_KinectController;
            this.img_color.Source = m_KinectController.ColorWriteBitmap;
            this.img_depth.Source = m_KinectController.DepthWriteBitmap;
        }

        public void ChangeSensor(KinectSensor _sensor)
        {
            if (m_KinectController != null)
            {
                m_KinectController.ChangeSensor(_sensor);
            }
        }



        private void btn_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (m_KinectController != null)
            {
                m_KinectController.ShowFinal = !m_KinectController.ShowFinal;
            }
        }

        private void Menu_Server_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}