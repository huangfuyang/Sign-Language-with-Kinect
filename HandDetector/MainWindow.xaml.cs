﻿using System;
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
using CURELab.SignLanguage.StaticTools;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;

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
        private KinectStudioController m_kinectStudioController;
        private DBManager m_DBmanager;
        private SocketManager socket;
        private Dictionary<string, string> fullWordList;
        public MainWindow()
        {
            InitializeComponent();
            m_kinectStudioController = KinectStudioController.GetSingleton();
            HandShapeClassifier.GetSingleton();
        }

        private void GetResponseImageCallback(IAsyncResult result)
        {
            var handler = (SocketManager.AsyncBitmapCaller)((AsyncResult)result).AsyncDelegate;
            Console.WriteLine(handler.EndInvoke(result));
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Show();
            m_OpenCVController = OpenCVController.GetSingletonInstance();

            //RegisterThreshold("canny", ref OpenCVController.CANNY_THRESH, 100, 8);
            //RegisterThreshold("cannyThresh", ref OpenCVController.CANNY_CONNECT_THRESH, 100, 22);
            //RegisterThreshold("play speed", ref OpenNIController.SPEED, 2, 1);
            //RegisterThreshold("diff", ref KinectController.DIFF, 10, 7);
            //RegisterThreshold("Culling", ref KinectSDKController.CullingThresh, 100, 40);

            Menu_Kinect_Click(this, e);
            //Menu_Server_Click(this, e);
            //Menu_Train_Click(this, e);
            //MenuItem_Test_Click(this, e);

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
                this.img_leftFront.Source = m_KinectController.GrayWriteBitmap;
                m_KinectController.Start();
            }

        }
        private void Menu_Server_Click(object sender, RoutedEventArgs e)
        {
            //socket = SocketManager.GetInstance("127.0.0.1", 51243);
            socket = SocketManager.GetInstance("137.189.89.29", 51243);
            //socket = SocketManager.GetInstance("192.168.209.67", 51243);
            
            Menu_Kinect_Click(sender, e);
            //var hand = new HandShapeModel(HandEnum.Both)
            //{
            //    RightColor = new Image<Bgr, byte>("right.jpg"),
            //    LeftColor = new Image<Bgr, byte>("left.jpg")
            //};

            //var hand1 = new HandShapeModel(HandEnum.Intersect)
            //{
            //    RightColor = new Image<Bgr, byte>("intersect.jpg")
            //};
            //for (int i = 0; i < 10; i++)
            //{
            //    socket.SendData(hand1, null);
            //    Thread.Sleep(33);
            //    socket.SendData(hand, null);
            //    Thread.Sleep(33);

            //}
            //socket.SendEnd();
            AsnycDataRecieved();
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
                        if (r != "0" && r != "")
                        {
                            r = r.Trim();
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
            this.img_leftFront.Source = m_KinectController.WrtBMP_LeftHandFront;
            this.img_rightFront.Source = m_KinectController.WrtBMP_RightHandFront;
            this.img_candidate3.Source = m_KinectController.WrtBMP_Candidate3;
            this.img_candidate2.Source = m_KinectController.WrtBMP_Candidate2;
            this.img_candidate1.Source = m_KinectController.WrtBMP_Candidate1;

            m_KinectController.Start();
        }


        private void Menu_Gesture_Click(object sender, RoutedEventArgs e)
        {

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

        private void Window_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
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

        private List<SignWordModel> wordList;
        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = @"D:\Kinect data\Aaron\Aaron 1-180";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = dialog.SelectedPath;
                string dbPath = @"D:\Kinect data\Aaron\Aaron 1-180\database_empty.db";
                m_DBmanager = DBManager.GetSingleton(dbPath);
                DirectoryInfo folder = new DirectoryInfo(folderName);
                wordList = new List<SignWordModel>();
                foreach (var item in folder.GetFiles("*.xed"))
                {
                    string fileName = item.Name;
                    string[] s = fileName.Split();
                    SignWordModel wordModel = new SignWordModel(s[0], s[1], item.FullName, fileName);
                    wordList.Add(wordModel);
                }
                foreach (var dir in folder.GetDirectories())
                {

                    foreach (var item in dir.GetFiles("*.xed"))
                    {
                        string fileName = item.Name;
                        string[] s = fileName.Split();
                        SignWordModel wordModel = new SignWordModel(s[0], s[1], item.FullName, fileName);
                        wordList.Add(wordModel);
                    }
                }
                Console.WriteLine(wordList.Count() + " words to process");
            }

        }

        int signIndex = 0;
        private void MenuItem_Run_Click(object sender, RoutedEventArgs e)
        {
            signIndex = 0;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();

        }
        private void MenuItem_Test_Click(object sender, RoutedEventArgs e)
        {
            #region mog txt to database
            StreamReader sr = new StreamReader(File.Open(@"J:\Kinect data\mog141-180.txt", FileMode.Open));
            string dataPath = @"J:\Kinect data\database141-181.db";
            m_DBmanager = DBManager.GetSingleton(dataPath);
            m_DBmanager.BeginTrans();

            string line = sr.ReadLine();
            int count = 1;
            while (line != null && line != "")
            {
                string[] cell = line.Split();
                int frame = Convert.ToInt32(cell[1]);
                bool isRight = cell[2] == "r";
                if (cell.Count() >= 27)
                {
                    float[] Mog = cell.Skip(3).Take(24).Select(x => Convert.ToSingle(x)).ToArray();
                    m_DBmanager.UpdateMogData(frame, Mog, isRight);
                }
                Console.WriteLine(count++);
                line = sr.ReadLine();
            }
            m_DBmanager.Commit();
            m_DBmanager.Close();
            sr.Close();
            #endregion

            #region kmeans

            #endregion
        }


        void timer_Tick(object sender, EventArgs e)
        {
            //end
            if (signIndex >= wordList.Count())
            {
                Console.WriteLine("finish");
                ((System.Windows.Forms.Timer)sender).Stop();
            }
            if (!m_DBmanager.Begin)
            {
                //begin
                m_KinectController.Reset();
                m_OpenCVController.Reset();
                Console.WriteLine("[{0}/{1} {2:P}] \nloading:{3}",
                    signIndex, wordList.Count(),
                    (float)signIndex / wordList.Count(),
                    wordList[signIndex].File);
                if (wordList[signIndex].Signer == "Aaron")
                {
                    KinectSDKController.AngleRotateTan = KinectSDKController.AaronRotateTan;
                }
                else if (wordList[signIndex].Signer == "Anita")
                {
                    KinectSDKController.AngleRotateTan = KinectSDKController.AnitaRotateTan;
                }
                else
                {
                    KinectSDKController.AngleRotateTan = KinectSDKController.MichaelRotateTan;
                }
                // Console.WriteLine("current threshold:"+KinectSDKController.AngleRotateTan);
                m_DBmanager.BeginTrans();
                m_DBmanager.AddWordSample(wordList[signIndex]);
                m_kinectStudioController.Open_File(wordList[signIndex].FullName);
                m_kinectStudioController.Run();
                m_DBmanager.Begin = true;
                signIndex++;
            }
            else
            {
                //running
                Console.WriteLine("waiting");
            }
        }


        private void Menu_End_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("END");
            if (socket != null)
            {
                socket.SendEnd();
            }
        }
    }
}
