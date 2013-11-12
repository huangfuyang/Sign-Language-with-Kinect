using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

using CURELab.SignLanguage.Debugger.ViewModel;
using CURELab.SignLanguage.RecognitionSystem.StaticTools;
using CURELab.SignLanguage.Debugger.Module;
using CURELab.SignLanguage.DataModule;
using CURELab.SignLanguage.Calculator;
using CURELab.SignLanguage.Debugger.View;

namespace CURELab.SignLanguage.Debugger
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        #region bindings
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; this.OnPropertyChanged("FileName"); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                if (_isPlaying)
                {

                    btn_play.Content = "Pause";
                }
                else
                {

                    btn_play.Content = "Play";
                }
            }
        }

        private bool _isShowSplitLine;

        public bool IsShowSplitLine
        {
            get { return _isShowSplitLine; }
            set
            {
                _isShowSplitLine = value;
                if (_isShowSplitLine)
                {
                    ssb_wordBox.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
                    cht_left.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
                    cht_right.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
                }
                else
                {
                    ssb_wordBox.ClearSplitLine(true, true, true);
                    cht_left.ClearSplitLine(true, true, true);
                    cht_right.ClearSplitLine(true, true, true);
                }
            }
        }


        private bool _isShowTrajectory;

        public bool IsShowTrajectory
        {
            get { return _isShowTrajectory; }
            set {
                if (value == false)
                {
                    m_trajectoryWindow.ClearBoard();
                }
                _isShowTrajectory = value;
            }
        }

        private bool _isShowGroundTruth;

        public bool IsShowGroundTruth
        {
            get { return _isShowGroundTruth; }
            set { _isShowGroundTruth = value;
            if (value == true)
            {
                cht_left.ShowRect();
                cht_right.ShowRect();
            }
            else
            {
                cht_left.RemoveRect();
                cht_right.RemoveRect();
            }
            }
        }
 
        private double _currentTime;

        public double CurrentTime
        {
            get { return _currentTime; }
            set
            {
                sld_progress.Value = value;
                _currentTime = value;
            }
        }

        bool _isPauseOnSegment;

        public bool IsPauseOnSegment
        {
            get { return _isPauseOnSegment; }
            set { _isPauseOnSegment = value; }
        }


        bool _isSegByAcc;

        public bool IsSegByAcc
        {
            get { return _isSegByAcc; }
            set { _isSegByAcc = value;
            if (IsShowSplitLine)
            {
                RefreshSplitLine();
            }
            }
        }

        bool _isSegByVel;

        public bool IsSegByVel
        {
            get { return _isSegByVel; }
            set { 
                _isSegByVel = value;
                if (IsShowSplitLine)
                {
                    RefreshSplitLine();
                }
            }
        }

        bool _isSegByAng;

        public bool IsSegByAng
        {
            get { return _isSegByAng; }
            set { _isSegByAng = value;
            if (IsShowSplitLine)
            {
                RefreshSplitLine();
            }
            }
        }

        #endregion


        #region parameter
        int xrange = 3000;
        int preTime = 0;
        double totalDuration;
        int totalFrame;
        int Max = 1;
        int Min = 0;
        List<SegmentedWordModel> wordList;

        private DataManager m_dataManager;
        private DataReader m_dataReader;
        private ConfigReader m_configReader;
        private DispatcherTimer updateTimer;
        private TrajectoryView m_trajectoryWindow;

        private IDataProcessor m_csDataProcessor;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            CustomizeComponent();
            InitializeModule();
            InitializeChart();
            InitializeTimer();
            InitializeParams();

            ConsoleManager.Show();
        }

        private void CustomizeComponent()
        {
            m_trajectoryWindow = new TrajectoryView(im_image);
      
        }

        private void InitializeModule()
        {

            m_dataManager = DataManager.GetSingletonInstance();
            m_configReader = ConfigReader.GetSingletonConfigReader();
            m_csDataProcessor = new CSDataProcessor();
        }

        private void InitializeParams()
        {
            this.DataContext = this;
            FileName = "";
            IsPlaying = false;
            btn_play.IsEnabled = false;
            me_rawImage.SpeedRatio = 0.2;


            IsPauseOnSegment = true;
            IsShowSplitLine = true;

            wordList = new List<SegmentedWordModel>();
        }

       

        private void InitializeChart()
        {
            cht_right.SetYRestriction(-0.3,1.1);
            cht_left.SetYRestriction(-0.3, 1.1);
            cht_right.Title = "Right";
            cht_left.Title = "Left";
            cht_big.Title = "Data";
            
        }

        private void InitializeTimer()
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Start();
        }





        void updateTimer_Tick(object sender, EventArgs e)
        {
            if (me_rawImage.HasVideo && IsPlaying)
            {
                //update time
                if (CurrentTime == totalDuration)
                {
                    IsPlaying = false;
                    me_rawImage.Stop();
                    return;
                }
                CurrentTime = me_rawImage.Position.TotalMilliseconds;
                int currentFrame = (int)(totalFrame * CurrentTime / totalDuration);
                int currentTimestamp = m_dataManager.GetCurrentTimestamp(currentFrame);

                int currentDataTime = m_dataManager.GetCurrentDataTime(currentTimestamp);

                //pause on segment
                if (IsPauseOnSegment && IsOnSegment(currentDataTime))
                {
                        IsPlaying = false;
                        me_rawImage.Pause();
                        preTime = currentDataTime;
                        border_media.BorderBrush = Brushes.DimGray;
                }
                else
                {
                    border_media.BorderBrush = Brushes.LightBlue;
                }
                
                //update word segmentation show text block
                tbk_words.Inlines.Clear();
                if (m_dataManager.True_Segmented_Words.Count > 0)
                {
                    foreach (SegmentedWordModel item in m_dataManager.True_Segmented_Words)
                    {
                        if (currentTimestamp >= item.StartTime && currentDataTime <= item.EndTime)
                        {
                            tbk_words.Inlines.Add(new Bold(new Run(item.Word + " ")));
                        }
                        else
                        {
                            tbk_words.Inlines.Add(new Run(item.Word + " "));
                        }
                        
                    }

                }
                //update chart data range           
                cht_big.SetXRestriction(currentDataTime - xrange, currentDataTime);
                //update chart signer
                cht_right.DrawSigner(currentDataTime, Min, Max);
                cht_left.DrawSigner(currentDataTime, Min, Max);
                ssb_wordBox.DrawSigner(currentDataTime);
                //udpate arm track
                if (IsShowTrajectory)
                {
                    m_trajectoryWindow.DrawTrajectory(m_dataManager.GetLeftPositions(currentDataTime), m_dataManager.GetRightPositions(currentDataTime));
                }
                Console.WriteLine(currentDataTime);
            }
        }



        private bool IsOnSegment(int currentDataTime)
        {

            if (IsSegByAcc && m_dataManager.AcSegmentTimeStampList.Contains(currentDataTime) && currentDataTime != preTime)
            {
                return true;
            }
            else if (IsSegByVel && m_dataManager.VeSegmentTimeStampList.Contains(currentDataTime) && currentDataTime != preTime)
            {
                return true;

            }
            else if (IsSegByAng && m_dataManager.AngSegmentTimeStampList.Contains(currentDataTime) && currentDataTime != preTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessData()
        {
            
        }

        private void DrawData()
        {
            //preprocess data
            double[] xPosition = new double[m_dataManager.DataModelDic.Count];
            double[] yPosition = new double[m_dataManager.DataModelDic.Count];
            double[] zPosition = new double[m_dataManager.DataModelDic.Count];
            int index = 0;
            foreach (KeyValuePair<int, DataModel> item in m_dataManager.DataModelDic)
            {
                xPosition[index] = item.Value.position_right.x;
                yPosition[index] = item.Value.position_right.y;
                zPosition[index] = item.Value.position_right.z;
                //Math.Sqrt(Math.Pow(item.Value.position_right.x, 2) +
                //              Math.Pow(item.Value.position_right.y, 2));
            }

            m_csDataProcessor.MeanFilter(ref xPosition);
            m_csDataProcessor.MeanFilter(ref yPosition);
            for (int i = 0; i < m_dataManager.DataModelDic.Count; i++)
            {
            }
            //draw data
            index = 0;
            //right
            TwoDimensionViewPointCollection V_Right_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection A_Right_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection Angle_Right_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection Y_Right_Points = new TwoDimensionViewPointCollection();
            //left
            TwoDimensionViewPointCollection V_Left_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection A_Left_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection Angle_Left_Points = new TwoDimensionViewPointCollection();
            TwoDimensionViewPointCollection Y_Left_Points = new TwoDimensionViewPointCollection();
            
            foreach (KeyValuePair<int, DataModel> item in m_dataManager.DataModelDic)
            {
                V_Right_Points.Add(new TwoDimensionViewPoint(item.Value.v_right, item.Value.timeStamp));
                V_Left_Points.Add(new TwoDimensionViewPoint(item.Value.v_left, item.Value.timeStamp));
                A_Right_Points.Add(new TwoDimensionViewPoint(item.Value.a_right, item.Value.timeStamp));
                A_Left_Points.Add(new TwoDimensionViewPoint(item.Value.a_left, item.Value.timeStamp));
                Angle_Right_Points.Add(new TwoDimensionViewPoint(item.Value.angle_right, item.Value.timeStamp));
                Angle_Left_Points.Add(new TwoDimensionViewPoint(item.Value.angle_left, item.Value.timeStamp));
                Y_Right_Points.Add(new TwoDimensionViewPoint(item.Value.position_right.y, item.Value.timeStamp));
                Y_Left_Points.Add(new TwoDimensionViewPoint(item.Value.position_left.y, item.Value.timeStamp));

                //Y_filtered_left_position.Add(new TwoDimensionViewPoint(yPosition[index], item.Value.timeStamp));
                index++;
            }
            
            Pen veloPen = new Pen(Brushes.DarkBlue, 2);
            Pen accPen = new Pen(Brushes.Red, 2);
            Pen anglePen = new Pen(Brushes.ForestGreen, 2);
            Pen posPen = new Pen(Brushes.Purple, 2);

            cht_right.AddLineGraph("velocity", V_Right_Points, veloPen, true);
            cht_right.AddLineGraph("acceleration", A_Right_Points, accPen,false);
            cht_right.AddLineGraph("angle", Angle_Right_Points, anglePen, false);
            cht_right.AddLineGraph("Y", Y_Right_Points, posPen, false);
            
            cht_left.AddLineGraph("velocity", V_Left_Points, veloPen,true);
            cht_left.AddLineGraph("acceleration", A_Left_Points, accPen, false);
            cht_left.AddLineGraph("angle", Angle_Left_Points, anglePen, false);
            cht_left.AddLineGraph("Y", Y_Left_Points, posPen, false);

            cht_big.AddLineGraph("r_velocity", V_Right_Points, veloPen, false);
            cht_big.AddLineGraph("r_acceleration", A_Right_Points, accPen, false);
            cht_big.AddLineGraph("r_angle", Angle_Right_Points, anglePen, false);
            cht_big.AddLineGraph("r_Y", Y_Right_Points, posPen, false);
            cht_big.AddLineGraph("l_velocity", V_Left_Points, veloPen, false);
            cht_big.AddLineGraph("l_acceleration", A_Left_Points, accPen, false);
            cht_big.AddLineGraph("l_angle", Angle_Left_Points, anglePen, false);
            cht_big.AddLineGraph("l_Y", Y_Left_Points, posPen, false);

            //add true word split line  
            ssb_wordBox.Length = m_dataManager.DataModelDic.Last().Value.timeStamp;
            ssb_wordBox.AddWords(m_dataManager.True_Segmented_Words);
            tbk_words.Text = "";
            //add word rect
            foreach (var item in m_dataManager.True_Segmented_Words)
            {
                tbk_words.Text += item.Word;
                cht_right.AddRect(item.StartTime, item.EndTime);
                cht_left.AddRect(item.StartTime, item.EndTime);
                cht_big.AddRect(item.StartTime, item.EndTime);

            }

            foreach (int item in m_dataManager.AcSegmentTimeStampList)
            {
                cht_right.AddSplitLine(item, 2, Min, Max, SegmentType.AccSegment, Colors.DarkRed);
                cht_left.AddSplitLine(item, 2, Min, Max, SegmentType.AccSegment, Colors.DarkRed);
                ssb_wordBox.AddSplitLine(item, 2, SegmentType.AccSegment, Colors.DarkRed);
            }


            foreach (int item in m_dataManager.VeSegmentTimeStampList)
            {
                cht_right.AddSplitLine(item, 2, Min, Max, SegmentType.VelSegment, Colors.DarkBlue);
                cht_left.AddSplitLine(item, 2, Min, Max, SegmentType.VelSegment, Colors.DarkBlue);
                ssb_wordBox.AddSplitLine(item, 2, SegmentType.VelSegment, Colors.DarkBlue);
            }


            foreach (int item in m_dataManager.AngSegmentTimeStampList)
            {
                cht_right.AddSplitLine(item, 2, Min, Max, SegmentType.AngSegment, Colors.DarkGreen);
                cht_left.AddSplitLine(item, 2, Min, Max, SegmentType.AngSegment, Colors.DarkGreen);
                ssb_wordBox.AddSplitLine(item, 2, SegmentType.AngSegment, Colors.DarkGreen);
            }

            cb_show_rect.IsChecked = true;
        }




        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            sld_progress.Maximum = me_rawImage.NaturalDuration.TimeSpan.TotalMilliseconds;
            totalDuration = sld_progress.Maximum;
            //file name                         1_mas.avi
            string temp_name = FileName.Split('\\').Last();
            //file addr                         C:\sss\ss\s\
            string temp_addr = FileName.Substring(0, FileName.Length - temp_name.Length);
            //file name without appendix(.avi)  1_mas
            temp_name = temp_name.Substring(0, temp_name.Length - temp_name.Split('.').Last().Length - 1);
            //file addr with file number & _    C:\sss\ss\s\1_
            if (temp_name.IndexOf('_') != -1)
            {
                temp_addr += temp_name.Split('_')[0] + '_';
            }
            else
            {
                temp_addr += temp_name + "_";
            }
            Console.WriteLine(temp_addr);
            //SetFilePath();
            //Console.WriteLine("1");
            m_dataReader = new DataReader(temp_addr);

            if (!m_dataReader.ReadData())
            {
                btn_play.IsEnabled = false;
                PopupWarn("Open Failed");
                return;
            }
            else
            {
                //clear all graph
                cht_right.ClearAllGraph();
                cht_left.ClearAllGraph();
                cht_big.ClearAllGraph();
                ssb_wordBox.RemoveAll();
           
                IsPlaying = false;
                btn_play.IsEnabled = true;
                totalDuration = me_rawImage.NaturalDuration.TimeSpan.TotalMilliseconds;
                totalFrame = (int)(totalDuration * 0.03) + 1;
                ProcessData();
                DrawData();

                //TODO: dynamic FPS

            }
            IsPlaying = false;
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            btn_Stop_Click(new object(), new RoutedEventArgs());
            sld_progress.Value = 0;
            IsPlaying = false;
        }


        private void btn_openFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".avi";
            dlg.Filter = "media file (.avi)|*.avi";
            if (dlg.ShowDialog().Value)
            {
                // Open document 

                me_rawImage.Source = new Uri(dlg.FileName);
                try
                {
                    me_rawImage.LoadedBehavior = MediaState.Manual;
                    me_rawImage.UnloadedBehavior = MediaState.Manual;
                    string addr = dlg.FileName.Substring(0, dlg.FileName.Length - dlg.SafeFileName.Length);

                    string temp_fileName = addr + dlg.SafeFileName.Split('_')[0] + '_';
                    FileName = dlg.FileName;
                    me_rawImage.Play();
                    me_rawImage.Pause();

                }
                catch (Exception e1)
                {

                    PopupWarn(e1.ToString());
                }
            }
        }


        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            if (me_rawImage.HasVideo)
            {
                if (!IsPlaying)
                {
                    IsPlaying = true;
                    me_rawImage.Play();
                }
                else
                {
                    IsPlaying = false;
                    me_rawImage.Pause();
                }
            }
            else
            {
                PopupWarn("no video");
            }
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (me_rawImage.HasVideo)
            {
                me_rawImage.Stop();
                IsPlaying = false;
                CurrentTime = 0;
            }

        }

        private void RefreshSplitLine()
        {
            cht_left.ClearSplitLine(true, true, true);
            cht_right.ClearSplitLine(true, true, true);
            ssb_wordBox.ClearSplitLine(true, true, true);
            cht_left.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
            cht_right.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
            ssb_wordBox.ShowSplitLine(IsSegByAcc, IsSegByVel, IsSegByAng);
        }

        private void PopupWarn(string msg)
        {
            string text = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            System.Windows.MessageBox.Show(text, caption, button, icon);
            return;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void sld_progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (me_rawImage.HasVideo)
            {
                me_rawImage.Position = TimeSpan.FromMilliseconds(sld_progress.Value);
                CurrentTime = sld_progress.Value;
            }
        }

        private void sld_progress_DragEnter(object sender, DragEventArgs e)
        {
            IsPlaying = false;
        }

        private void sld_progress_DragLeave(object sender, DragEventArgs e)
        {
            IsPlaying = true;
        }

    }

}
