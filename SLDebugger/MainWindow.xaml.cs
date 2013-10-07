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
        public double a_right;
        public double a_left;
        public double v_right;
        public double v_left;
        public bool isSegmentPoint;
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

        bool isPauseOnSegment;

        double totalDuration;
        int totalFrame;
        private LineGraph lastSigner;

        private DataManager m_dataManager;
        private DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeModule();
            InitializeParams();
            InitializeChart();
            InitializeTimer();

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
        }

        private void InitializeModule()
        {
            m_dataManager = new DataManager();
        }

        private void InitializeChart()
        {
          
            CircleElementPointMarker pointMaker = new CircleElementPointMarker();
            pointMaker.Size = 3;
            pointMaker.Brush = Brushes.Yellow;
            pointMaker.Fill = Brushes.Purple;
            var v_right = new EnumerableDataSource<VelocityPoint>(m_dataManager.VelocityPointCollection_right);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Velocity);
            cht_right.AddLineGraph(v_right, new Pen(Brushes.DarkBlue, 2), pointMaker, new PenDescription("right v"));

            var a_right = new EnumerableDataSource<VelocityPoint>(m_dataManager.AccelerationPointCollection_right);
            a_right.SetXMapping(x => x.TimeStamp);
            a_right.SetYMapping(y => y.Velocity);
            cht_right.AddLineGraph(a_right, new Pen(Brushes.Red, 2), pointMaker, new PenDescription("right a"));

            cht_right.Legend.AutoShowAndHide = false;
            cht_right.LegendVisible = false;
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
            if (me_rawImage.HasVideo)
            {
                CurrentTime = me_rawImage.Position.TotalMilliseconds;
                int currentFrame = (int)(totalFrame * CurrentTime / totalDuration);
                int currentTimestamp = m_dataManager.GetCurrentTimestamp(currentFrame);
                ShownData currentData = m_dataManager.GetCurrentData(currentTimestamp);
                if (lastSigner != null)
                {
                    lastSigner.Remove();
                }
                lastSigner = AddSplitLine(cht_right, currentData.timeStamp,1);

            }
        }
  
        private bool OpenDataStreams(string address)
        {
            try
            {
                // read timestamp 
                StreamReader timeReader = new StreamReader(address + "timestamp.txt");
                string line = timeReader.ReadLine();
                int firstStamp = Convert.ToInt32(line);
                while (!String.IsNullOrWhiteSpace(line))
                {
                    int timeStamp = Convert.ToInt32(line) - firstStamp;
                    m_dataManager.ImageTimeStampList.Add(timeStamp);
                    line = timeReader.ReadLine();
                }
                timeReader.Close();
                timeReader = null;

                // read acc & velo
                StreamReader accReader = new StreamReader(address + "ac_left_right.txt");
                StreamReader veloReader = new StreamReader(address + "vo_left_right.txt");
                StreamReader segPointReader = new StreamReader(address + "output.txt");

                string a_line = accReader.ReadLine();
                string v_line = veloReader.ReadLine();
                string seg_line = segPointReader.ReadLine();

                firstStamp = Convert.ToInt32(a_line.Split(' ')[0]);
                while (!String.IsNullOrWhiteSpace(v_line) && !String.IsNullOrWhiteSpace(a_line))
                {
                    string[] words = a_line.Split(' ');
                    int dataTime = Convert.ToInt32(words[0]) - firstStamp;

                    double aLeft = Convert.ToDouble(words[1]);
                    double aRight = Convert.ToDouble(words[2]);
                    words = v_line.Split(' ');
                    double vLeft = Convert.ToDouble(words[1]);
                    double vRight = Convert.ToDouble(words[2]);

                    int segTime = Convert.ToInt32(seg_line) - firstStamp;
                    bool isSegpoint;
                    if (segTime == dataTime)
                    {
                        isSegpoint = true;
                    }
                    else
                    {
                        isSegpoint = false;
                    }


                    m_dataManager.DataList.Add(new ShownData()
                    {
                        timeStamp = dataTime,
                        a_left = aLeft,
                        a_right = aRight,
                        v_left = vLeft,
                        v_right = vRight,
                        isSegmentPoint = isSegpoint
                    });

                    //Console.WriteLine(dataTime + " " + segTime + " " + isSegpoint);
                    a_line = accReader.ReadLine();
                    v_line = veloReader.ReadLine();
                    if (isSegpoint)
                    {
                        string newTime = segPointReader.ReadLine();
                        while (Convert.ToInt32(newTime) == dataTime)
                        {
                            newTime = segPointReader.ReadLine();
                        }
                        seg_line = newTime;

                    }
                }
                //foreach (ShownData item in dataList)
                //{
                //    if (item.isSegmentPoint)
                //    {
                //        Console.WriteLine(item.timeStamp.ToString());
                //    }
                //}
                m_dataManager.DataList.Reverse();
                accReader.Close();
                accReader = null;
                veloReader.Close();
                veloReader = null;
                segPointReader.Close();
                segPointReader = null;
            }
            catch (Exception e)
            {
                PopupWarn(e.ToString());
                return false;
            }


            return true;


        }

        private void DrawData()
        {
            foreach (ShownData item in m_dataManager.DataList)
            {
                m_dataManager.VelocityPointCollection_right.Add(new VelocityPoint(item.v_right, item.timeStamp));
                m_dataManager.AccelerationPointCollection_right.Add(new VelocityPoint(item.a_right, item.timeStamp));
                if (item.isSegmentPoint)
                {
                    AddSplitLine(cht_right,item.timeStamp,2);
                }
            }
        }

        private LineGraph AddSplitLine(ChartPlotter chart, int split, double stroke)
        {
            var tempPoints = new VelocityPointCollection();
            var v_right = new EnumerableDataSource<VelocityPoint>(tempPoints);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Velocity);
            tempPoints.Add(new VelocityPoint(m_dataManager.MaxVelocity, split));
            tempPoints.Add(new VelocityPoint(m_dataManager.MinVelocity, split));
            return chart.AddLineGraph(v_right, Colors.Black, stroke, "seg line");
            

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
            if (!OpenDataStreams(temp_addr))
            {
                btn_play.IsEnabled = false;
                PopupWarn("Open Failed");
                return;
            }
            else
            {
                btn_play.IsEnabled = true;
                totalDuration = me_rawImage.NaturalDuration.TimeSpan.TotalMilliseconds;
                totalFrame = (int)totalDuration / 200 + 1;
                DrawData();
                //TODO: dynamic FPS

            }

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
                    //Timer.Start();
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
