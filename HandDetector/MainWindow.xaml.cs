using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using System.IO.Compression;
using CURELab.SignLanguage.StaticTools;

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
        private VideoProcessor m_VideoProcessor;
        private OpenCVController m_OpenCVController;
        private DBManager m_DBmanager;
        private KinectStudioController m_kinectStudioController;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            RegisterThreshold("canny", ref OpenCVController.CANNY_THRESH, 100, 8);
            RegisterThreshold("cannyThresh", ref OpenCVController.CANNY_CONNECT_THRESH, 100, 22);
            RegisterThreshold("play speed", ref OpenNIController.SPEED, 2, 1);
            RegisterThreshold("diff", ref VideoProcessor.DIFF, 10, 7);
            RegisterThreshold("Culling", ref VideoProcessor.CullingThresh, 10, 5);

            ConsoleManager.Show();
            Initialize();

        }


        private void Initialize()
        {
            //HandShapeClassifier.GetSingleton();
            m_OpenCVController = OpenCVController.GetSingletonInstance();
            m_VideoProcessor = VideoProcessor.GetSingletonInstance();
            this.sld_progress.DataContext = VisualData.GetSingleton();
            this.img_color.Source = m_VideoProcessor.ColorWriteBitmap;
            this.img_depth.Source = m_VideoProcessor.DepthWriteBitmap;
            this.img_leftFront.Source = m_VideoProcessor.WrtBMP_LeftHandFront;
            this.img_rightFront.Source = m_VideoProcessor.WrtBMP_RightHandFront;
            string path = @"D:\Kinect data\new";
            //m_VideoProcessor.OpenDir(@"D:\Kinect data\newdata\HKG_001_a_0001 Aaron 22");
            
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

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {

            if (m_KinectController != null)
            {
                m_KinectController.Shutdown();

            }
            Environment.Exit(0);
        }

        

        private void Window_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {

                case Key.Space:
                    m_VideoProcessor.ProcessFrame();
                    break;
                case Key.R:
                    m_VideoProcessor.ProcessSample();
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
        private void MenuItem_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = @"D:\Kinect data\new";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = dialog.SelectedPath;
                //string dbPath = @"D:\Kinect data\database_empty.db";
                //m_DBmanager = DBManager.GetSingleton(dbPath);
                DirectoryInfo folder = new DirectoryInfo(folderName);
                wordList = new List<SignWordModel>();
                foreach (var dir in folder.GetDirectories())
                {
                    string fileName = dir.Name;
                    string[] s = fileName.Split();
                    SignWordModel wordModel = new SignWordModel(s[1], s[2], dir.FullName, fileName);
                    wordList.Add(wordModel);
                }

                Console.WriteLine(wordList.Count() + " words to process");
            }

        }

        int signIndex = 0;
        private void MenuItem_Run_Click(object sender, RoutedEventArgs e)
        {
            signIndex = 0;
            if (wordList == null)
            {
                return;
            }
            foreach (var wordModel in wordList)
            {
                Console.WriteLine("Process:"+wordModel.FullName);
                m_VideoProcessor.OpenDir(wordModel.FullName);
                m_VideoProcessor.ProcessSample();
            }
            //System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            //timer.Interval = 1000;
            //timer.Tick += timer_Tick;
            //timer.Start();

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


        private void sld_progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_VideoProcessor != null)
            {
                if ((int)e.OldValue != (int)e.NewValue)
                {
                    m_VideoProcessor.SetCurrentFrame((int)e.NewValue);
                }
            }
        }

        private void MenuItem_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = @"F:\Aaron\";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = dialog.SelectedPath;
                Console.WriteLine(folderName);
                m_VideoProcessor.OpenDir(folderName);
            }
        }
    }
}
