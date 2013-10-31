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
using CURELab.SignLanguage.Debugger.Model;

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


        private bool _isShowTrajectory;

        public bool IsShowTrajectory
        {
            get { return _isShowTrajectory; }
            set { _isShowTrajectory = value; }
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

        bool _isShowVeloRight;
        LineAndMarker<ElementMarkerPointsGraph> v_right_graph;
        public bool IsShowVeloRight
        {
            get { return _isShowVeloRight; }
            set
            {
                _isShowVeloRight = value;
                if (value)
                {
                    v_right_graph = m_rightGraphView.AppendLineGraph(m_dataManager.V_Right_Points, new Pen(Brushes.DarkBlue, 2), "v right");
                }
                else
                {
                    if (v_right_graph != null)
                    {
                        v_right_graph.LineGraph.Remove();
                        v_right_graph.MarkerGraph.Remove();
                        v_right_graph = null;
                    }

                }
            }
        }


        bool _isShowVeloLeft;
        LineAndMarker<ElementMarkerPointsGraph> v_left_graph;
        public bool IsShowVeloLeft
        {
            get { return _isShowVeloLeft; }
            set
            {
                _isShowVeloLeft = value;
                if (value)
                {
                    v_left_graph = m_leftGraphView.AppendLineGraph(m_dataManager.V_Left_Points, new Pen(Brushes.DarkBlue, 2), "v leftt");
                }
                else
                {
                    if (v_left_graph != null)
                    {
                        v_left_graph.LineGraph.Remove();
                        v_left_graph.MarkerGraph.Remove();
                        v_left_graph = null;
                    }

                }
            }
        }

        bool _isShowAccRight;
        LineAndMarker<ElementMarkerPointsGraph> acc_right_graph;
        public bool IsShowAccRight
        {
            get { return _isShowAccRight; }
            set
            {
                _isShowAccRight = value;
                if (value)
                {
                    acc_right_graph = m_rightGraphView.AppendLineGraph(m_dataManager.A_Right_Points, new Pen(Brushes.Red, 2), "a right");
                }
                else
                {
                    if (acc_right_graph != null)
                    {
                        acc_right_graph.LineGraph.Remove();
                        acc_right_graph.MarkerGraph.Remove();
                        acc_right_graph = null;
                    }
                }
            }
        }

        bool _isShowAccLeft;
        LineAndMarker<ElementMarkerPointsGraph> acc_left_graph;
        public bool IsShowAccLeft
        {
            get { return _isShowAccLeft; }
            set
            {
                _isShowAccLeft = value;
                if (value)
                {
                    acc_left_graph = m_leftGraphView.AppendLineGraph(m_dataManager.A_Left_Points, new Pen(Brushes.Red, 2), "a left");
                }
                else
                {
                    if (acc_left_graph != null)
                    {
                        acc_left_graph.LineGraph.Remove();
                        acc_left_graph.MarkerGraph.Remove();
                        acc_left_graph = null;
                    }
                }
            }
        }


        bool _isShowAngleRight;
        LineAndMarker<ElementMarkerPointsGraph> angle_right_graph;
        public bool IsShowAngleRight
        {
            get { return _isShowAngleRight; }
            set
            {
                _isShowAngleRight = value;
                if (value)
                {
                    angle_right_graph = m_rightGraphView.AppendLineGraph(m_dataManager.Angle_Right_Points, new Pen(Brushes.ForestGreen, 2), "a right");
                }
                else
                {
                    if (angle_right_graph != null)
                    {
                        angle_right_graph.LineGraph.Remove();
                        angle_right_graph.MarkerGraph.Remove();
                        angle_right_graph = null;
                    }
                }
            }
        }


        bool _isShowAngleLeft;
        LineAndMarker<ElementMarkerPointsGraph> angle_left_graph;
        public bool IsShowAngleLeft
        {
            get { return _isShowAngleLeft; }
            set
            {
                _isShowAngleLeft = value;
                if (value)
                {
                    angle_left_graph = m_leftGraphView.AppendLineGraph(m_dataManager.Angle_Left_Points, new Pen(Brushes.ForestGreen, 2), "a left");
                }
                else
                {
                    if (angle_left_graph != null)
                    {
                        angle_left_graph.LineGraph.Remove();
                        angle_left_graph.MarkerGraph.Remove();
                        angle_left_graph = null;
                    }
                }
            }
        }

        bool _isShowYLeft;
        LineAndMarker<ElementMarkerPointsGraph> y_left_graph;
        public bool IsShowYLeft
        {
            get { return _isShowYLeft; }
            set
            {
                _isShowYLeft = value;
                if (value)
                {
                    y_left_graph = m_leftGraphView.AppendLineGraph(m_dataManager.Y_Left_Points, new Pen(Brushes.Purple, 2), "a left");
                }
                else
                {
                    if (y_left_graph != null)
                    {
                        y_left_graph.LineGraph.Remove();
                        y_left_graph.MarkerGraph.Remove();
                        y_left_graph = null;
                    }
                }
            }
        }

        bool _isShowYRight;
        LineAndMarker<ElementMarkerPointsGraph> y_right_graph;
        public bool IsShowYRight
        {
            get { return _isShowYRight; }
            set
            {
                _isShowYRight = value;
                if (value)
                {
                    y_right_graph = m_rightGraphView.AppendLineGraph(m_dataManager.Y_Right_Points, new Pen(Brushes.Purple, 2), "a left");
                }
                else
                {
                    if (y_right_graph != null)
                    {
                        y_right_graph.LineGraph.Remove();
                        y_right_graph.MarkerGraph.Remove();
                        y_right_graph = null;
                    }
                }
            }
        }



        bool _isShowVeloLeftBig;
        LineAndMarker<ElementMarkerPointsGraph> v_left_big_graph;
        public bool IsShowVeloLeftBig
        {
            get { return _isShowVeloLeftBig; }
            set
            {
                _isShowVeloLeftBig = value;
                if (value)
                {
                    v_left_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.V_Left_Points, new Pen(Brushes.DarkBlue, 2), "v leftt");
                }
                else
                {
                    if (v_left_big_graph != null)
                    {
                        v_left_big_graph.LineGraph.Remove();
                        v_left_big_graph.MarkerGraph.Remove();
                        v_left_big_graph = null;
                    }

                }
            }
        }


        bool _isShowVeloRightBig;
        LineAndMarker<ElementMarkerPointsGraph> v_right_big_graph;
        public bool IsShowVeloRightBig
        {
            get { return _isShowVeloRightBig; }
            set
            {
                _isShowVeloRightBig = value;
                if (value)
                {
                    v_right_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.V_Right_Points, new Pen(Brushes.DarkBlue, 2), "v leftt");
                }
                else
                {
                    if (v_right_big_graph != null)
                    {
                        v_right_big_graph.LineGraph.Remove();
                        v_right_big_graph.MarkerGraph.Remove();
                        v_right_big_graph = null;
                    }

                }
            }
        }


        bool _isShowAccLeftBig;
        LineAndMarker<ElementMarkerPointsGraph> a_left_big_graph;
        public bool IsShowAccLeftBig
        {
            get { return _isShowAccLeftBig; }
            set
            {
                _isShowAccLeftBig = value;
                if (value)
                {
                    a_left_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.A_Left_Points, new Pen(Brushes.Red, 2), "v leftt");
                }
                else
                {
                    if (a_left_big_graph != null)
                    {
                        a_left_big_graph.LineGraph.Remove();
                        a_left_big_graph.MarkerGraph.Remove();
                        a_left_big_graph = null;
                    }

                }
            }
        }


        bool _isShowAccRightBig;
        LineAndMarker<ElementMarkerPointsGraph> a_right_big_graph;
        public bool IsShowAccRightBig
        {
            get { return _isShowAccRightBig; }
            set
            {
                _isShowAccRightBig = value;
                if (value)
                {
                    a_right_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.A_Right_Points, new Pen(Brushes.Red, 2), "v leftt");
                }
                else
                {
                    if (a_right_big_graph != null)
                    {
                        a_right_big_graph.LineGraph.Remove();
                        a_right_big_graph.MarkerGraph.Remove();
                        a_right_big_graph = null;
                    }

                }
            }
        }

        bool _isShowAngleLeftBig;
        LineAndMarker<ElementMarkerPointsGraph> angle_left_big_graph;
        public bool IsShowAngleLeftBig
        {
            get { return _isShowAngleLeftBig; }
            set
            {
                _isShowAngleLeftBig = value;
                if (value)
                {
                    angle_left_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.Angle_Left_Points, new Pen(Brushes.ForestGreen, 2), "v leftt");
                }
                else
                {
                    if (angle_left_big_graph != null)
                    {
                        angle_left_big_graph.LineGraph.Remove();
                        angle_left_big_graph.MarkerGraph.Remove();
                        angle_left_big_graph = null;
                    }

                }
            }
        }

        bool _isShowAngleRightBig;
        LineAndMarker<ElementMarkerPointsGraph> angle_right_big_graph;
        public bool IsShowAngleRightBig
        {
            get { return _isShowAngleRightBig; }
            set
            {
                _isShowAngleRightBig = value;
                if (value)
                {
                    angle_right_big_graph = m_bigGraphView.AppendLineGraph(m_dataManager.Angle_Right_Points, new Pen(Brushes.ForestGreen, 2), "v leftt");
                }
                else
                {
                    if (angle_right_big_graph != null)
                    {
                        angle_right_big_graph.LineGraph.Remove();
                        angle_right_big_graph.MarkerGraph.Remove();
                        angle_right_big_graph = null;
                    }

                }
            }
        }
        
        #endregion


        int xrange = 3000;
        int preTime = 0;
        double totalDuration;
        int totalFrame;
        List<SegmentedWordModel> wordList;

        private DataManager m_dataManager;
        private DataReader m_dataReader;
        private XMLReader m_configReader;
        private DispatcherTimer updateTimer;
        private GraphView m_rightGraphView;
        private GraphView m_leftGraphView;
        private GraphView m_truthGraphView;
        private GraphView m_bigGraphView;

        private TrajectoryView m_trajectoryWindow;

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
          //  m_trajectoryWindow.Show();
           // m_trajectoryWindow.Hide();  
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
            

            IsPauseOnSegment = true;
            IsShowSplitLine = true;

            wordList = new List<SegmentedWordModel>();
        }

        private void InitializeModule()
        {
            m_dataManager = ModuleManager.CreateDataManager();
            m_configReader = ModuleManager.CreateConfigReader();
        }

        private void InitializeChart()
        {
            m_rightGraphView = new GraphView(cht_right);
            m_leftGraphView = new GraphView(cht_left);
            m_truthGraphView = new GraphView(cht_truth);
            m_bigGraphView = new GraphView(cht_bigChart);
            ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();
            restr.YRange = new DisplayRange(0, 1);
            cht_bigChart.Viewport.Restrictions.Add(restr);
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

                    border_media.BorderBrush = Brushes.LightBlue;
                }

                ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();
                restr.XRange = new DisplayRange(currentDataTime - xrange, currentDataTime);
                cht_bigChart.Viewport.Restrictions.Add(restr);
                

                
                tbk_Words.Inlines.Clear();


                foreach (SegmentedWordModel item in m_dataManager.Segmented_Words)
                {
                    if (currentTimestamp >= item.StartTime && currentDataTime <= item.EndTime)
                    {
                        tbk_Words.Inlines.Add(new Bold(new Run(item.Word + " ")));
                    }
                    else
                    {
                        tbk_Words.Inlines.Add(new Run(item.Word + " "));
                    }

                }
    
                m_rightGraphView.DrawSigner(currentDataTime, m_dataManager.MinVelocity, m_dataManager.MaxVelocity);
                m_leftGraphView.DrawSigner(currentDataTime, m_dataManager.MinVelocity, m_dataManager.MaxVelocity);
                m_truthGraphView.DrawSigner(currentDataTime, m_dataManager.MinVelocity, m_dataManager.MaxVelocity);
                if (IsShowTrajectory)
                {
                    m_trajectoryWindow.DrawTrajectory(m_dataManager.GetLeftPositions(currentDataTime), m_dataManager.GetRightPositions(currentDataTime));
                }
            }
        }



        private void DrawData()
        {
            foreach (KeyValuePair<int, DataModel> item in m_dataManager.DataModelDic)
            {
                m_dataManager.V_Right_Points.Add(new TwoDimensionViewPoint(item.Value.v_right, item.Value.timeStamp));
                m_dataManager.V_Left_Points.Add(new TwoDimensionViewPoint(item.Value.v_left, item.Value.timeStamp));
                m_dataManager.A_Right_Points.Add(new TwoDimensionViewPoint(item.Value.a_right, item.Value.timeStamp));
                m_dataManager.A_Left_Points.Add(new TwoDimensionViewPoint(item.Value.a_left, item.Value.timeStamp));
                m_dataManager.Angle_Right_Points.Add(new TwoDimensionViewPoint(item.Value.angle_right, item.Value.timeStamp));
                m_dataManager.Angle_Left_Points.Add(new TwoDimensionViewPoint(item.Value.angle_left, item.Value.timeStamp));
                m_dataManager.Y_Right_Points.Add(new TwoDimensionViewPoint(item.Value.position_right.y, item.Value.timeStamp));
                m_dataManager.Y_Left_Points.Add(new TwoDimensionViewPoint(item.Value.position_left.y, item.Value.timeStamp));
            }

       
            //add split line
            m_rightGraphView.AddSplitLine(0, 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            m_rightGraphView.AddSplitLine(m_dataManager.ImageTimeStampList.Last(), 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            m_leftGraphView.AddSplitLine(0, 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            m_leftGraphView.AddSplitLine(m_dataManager.ImageTimeStampList.Last(), 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            m_truthGraphView.AddSplitLine(0, 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            m_truthGraphView.AddSplitLine(m_dataManager.ImageTimeStampList.Last(), 1, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);


            foreach (SegmentedWordModel item in m_dataManager.Segmented_Words)
            {
                m_truthGraphView.AddSplitLine(item.StartTime, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
                m_truthGraphView.AddSplitLine(item.EndTime, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.DarkBlue);
                tbk_Words.Text += item.Word;
            }


            
            foreach (int item in m_dataManager.SegmentTimeStampList)
            {
                m_rightGraphView.AddSplitLine(item, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
                m_leftGraphView.AddSplitLine(item, 2, m_dataManager.MinVelocity, m_dataManager.MaxVelocity, true, Colors.Black);
            }

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

            //SetFilePath();
            //Console.WriteLine("1");
            m_dataReader = ModuleManager.CreateDataReader(temp_addr);

            if (!m_dataReader.ReadData())
            {
                btn_play.IsEnabled = false;
                PopupWarn("Open Failed");
                return;
            }
            else
            {
                m_rightGraphView.ClearAllGraph();
                m_leftGraphView.ClearAllGraph();
                m_truthGraphView.ClearAllGraph();
                cb_v_right.IsChecked = true;
                cb_v_left.IsChecked = true;
               // m_leftGraphView.AppendLineGraph(m_dataManager.V_Left_Points, new Pen(Brushes.DarkBlue, 2), "v left");
               // m_leftGraphView.AppendLineGraph(m_dataManager.A_Left_Points, new Pen(Brushes.Red, 2), "v left");
               // m_leftGraphView.AppendLineGraph(m_dataManager.Angle_Left_Points, new Pen(Brushes.Green, 2), "v left");

                IsPlaying = false;
                btn_play.IsEnabled = true;
                totalDuration = me_rawImage.NaturalDuration.TimeSpan.TotalMilliseconds;
                totalFrame = (int)(totalDuration * 0.03) + 1;
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
