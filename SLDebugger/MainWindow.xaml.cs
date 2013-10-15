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
using System.Windows.Threading;
using CURELab.SignLanguage.Debugger.ViewModel;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.IO;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using CURELab.SignLanguage.RecognitionSystem.StaticTools;

namespace CURELab.SignLanguage.Debugger
{

    public struct ShownData
    {
        public int timeStamp;
        public double val_right;
        public double val_left;
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


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
                    m_leftGraphView.ShowSplitLine();
                    m_rightGraphView.ShowSplitLine();
                }
                else
                {
                    m_leftGraphView.ClearSplitLine();
                    m_rightGraphView.ClearSplitLine();
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

        int preTime = 0;
        double totalDuration;
        int totalFrame;


        private DataManager m_dataManager;
        private DataReader m_dataReader;
        private DispatcherTimer updateTimer;
        private GraphView m_rightGraphView;
        private GraphView m_leftGraphView;


        public MainWindow()
        {
            InitializeComponent();
            InitializeModule();
            InitializeChart();
            InitializeTimer();
            InitializeParams();

            ConsoleManager.Show();
        }

        private void InitializeParams()
        {
            this.DataContext = this;
            m_dataManager.MaxVelocity = 1;
            m_dataManager.MinVelocity = 0;
            FileName = "";
            IsPlaying = false;
            btn_play.IsEnabled = false;
            me_rawImage.SpeedRatio = 0.2;
            cb_data1.IsChecked = true;
            cb_data2.IsChecked = true;
            cb_data3.IsChecked = false;
            IsPauseOnSegment = true;
            IsShowSplitLine = true;
        }

        private void InitializeModule()
        {
            m_dataManager = new DataManager();
        }

        private void InitializeChart()
        {
            m_rightGraphView = new GraphView(cht_right);
            m_leftGraphView = new GraphView(cht_left);


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
                CurrentTime = me_rawImage.Position.TotalMilliseconds;
                int currentFrame = (int)(totalFrame * CurrentTime / totalDuration);
                int currentTimestamp = m_dataManager.GetCurrentTimestamp(currentFrame);

                int currentDataTime = m_dataManager.GetCurrentDataTime(currentTimestamp);
                if (m_dataManager.SegmentTimeStampList.Contains(currentDataTime) && currentDataTime != preTime)
                {
                    if (IsPauseOnSegment)
                    {
                        IsPlaying = false;
                        me_rawImage.Pause();
                        preTime = currentDataTime;
                    }

                    border_media.BorderBrush = Brushes.DimGray;
                }
                else
                {

                    border_media.BorderBrush = Brushes.White;
                }
                m_rightGraphView.DrawSigner(currentDataTime, m_dataManager.MinVelocity, m_dataManager.MaxVelocity);
                m_leftGraphView.DrawSigner(currentDataTime, m_dataManager.MinVelocity, m_dataManager.MaxVelocity);
            }
        }



        private void DrawData()
        {
            foreach (ShownData item in m_dataManager.DataList1)
            {
                m_dataManager.VelocityPointCollection_right_1.Add(new TwoDimensionViewPoint(item.val_right, item.timeStamp));
                m_dataManager.VelocityPointCollection_left_1.Add(new TwoDimensionViewPoint(item.val_left, item.timeStamp));
            }

            foreach (ShownData item in m_dataManager.DataList2)
            {
                m_dataManager.VelocityPointCollection_right_2.Add(new TwoDimensionViewPoint(item.val_right, item.timeStamp));
                m_dataManager.VelocityPointCollection_left_2.Add(new TwoDimensionViewPoint(item.val_left, item.timeStamp));
            }

            foreach (ShownData item in m_dataManager.DataList3)
            {
                m_dataManager.VelocityPointCollection_right_3.Add(new TwoDimensionViewPoint(item.val_right, item.timeStamp));
                m_dataManager.VelocityPointCollection_left_3.Add(new TwoDimensionViewPoint(item.val_left, item.timeStamp));
            }

            //add split line
            foreach (int item in m_dataManager.SegmentTimeStampList)
            {
                m_rightGraphView.AddSplitLine(item, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true);
                m_leftGraphView.AddSplitLine(item, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true);
            }

        }


        private void SetFilePath()
        {
            if (cb_data1.IsChecked == true)
            {
                FilePath.DataFile1Postfix = tb_data1.Text;
            }
            else
            {
                FilePath.DataFile1Postfix = "";
            }

            if (cb_data2.IsChecked == true)
            {
                FilePath.DataFile2Postfix = tb_data2.Text;
            }
            else
            {
                FilePath.DataFile2Postfix = "";
            }

            if (cb_data3.IsChecked == true)
            {
                FilePath.DataFile3Postfix = tb_data3.Text;
            }
            else
            {
                FilePath.DataFile3Postfix = "";
            }

            FilePath.SegmentFilePostfix = tb_segment.Text;

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
            temp_addr += temp_name.Split('_')[0] + '_';

            SetFilePath();
            Console.WriteLine("1");
            m_dataReader = new DataReader(temp_addr, m_dataManager);

            if (!m_dataReader.ReadData())
            {
                btn_play.IsEnabled = false;
                PopupWarn("Open Failed");
                return;
            }
            else
            {
                m_rightGraphView.ClearGraph();
                m_leftGraphView.ClearGraph();

                if (cb_data1.IsChecked == true)
                {
                    m_leftGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_left_1, new Pen(Brushes.DarkBlue, 2), "v left");
                    m_rightGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_right_1, new Pen(Brushes.DarkBlue, 2), "v right");
                }

                if (cb_data2.IsChecked == true)
                {
                    m_rightGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_right_2, new Pen(Brushes.Red, 2), "v right");
                    m_leftGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_left_2, new Pen(Brushes.Red, 2), "v left");
                }

                if (cb_data3.IsChecked == true)
                {
                    m_rightGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_right_3, new Pen(Brushes.Green, 2), "v right");
                    m_leftGraphView.AppendLineGraph(m_dataManager.VelocityPointCollection_left_3, new Pen(Brushes.Green, 2), "v left");
                }

                IsPlaying = false;
                btn_play.IsEnabled = true;
                totalDuration = me_rawImage.NaturalDuration.TimeSpan.TotalMilliseconds;
                totalFrame = (int)(totalDuration * 0.03) + 1;
                DrawData();

                //TODO: dynamic FPS

            }
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
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
            }

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
