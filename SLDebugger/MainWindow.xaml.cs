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
        private int _maxVoltage;
        public int MaxVoltage
        {
            get { return _maxVoltage; }
            set { _maxVoltage = value; this.OnPropertyChanged("MaxVoltage"); }
        }

        private int _minVoltage;
        public int MinVoltage
        {
            get { return _minVoltage; }
            set { _minVoltage = value; this.OnPropertyChanged("MinVoltage"); }
        }

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
                _currentTime = value;
            }
        }

        List<int> imageTimeStampList;
        List<ShownData> dataList;
        bool isPauseOnSegment;

        double totalDuration;
        int totalFrame;
        int preTimestamp = 0;
        int graphTime = 0;


        private VoltagePointCollection voltagePointCollection_right;
        private VoltagePointCollection voltagePointCollection_left;
        private DispatcherTimer updateCollectionTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeParams();
            InitializeChart();
            InitializeTimer();
        }

        private void InitializeParams()
        {
            this.DataContext = this;
            MaxVoltage = 1;
            MinVoltage = 0;
            FileName = "";
            IsPlaying = false;
            btn_play.IsEnabled = false;
        }
        private void InitializeTimer()
        {
            updateCollectionTimer = new DispatcherTimer();
            updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            updateCollectionTimer.Start();

        }

        private void InitializeChart()
        {
            voltagePointCollection_right = new VoltagePointCollection();
            var v_right = new EnumerableDataSource<VoltagePoint>(voltagePointCollection_right);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Voltage);
            CircleElementPointMarker pointMaker = new CircleElementPointMarker();
            pointMaker.Size = 5;
            pointMaker.Brush = Brushes.Red;
            pointMaker.Fill = Brushes.Purple;
            cht_right.AddLineGraph(v_right, new Pen(Brushes.AliceBlue, 2), pointMaker, new PenDescription("right v"));
           // cht_right.AddLineGraph(v_right, Colors.DarkBlue, 2, "right velo");

        }

        void updateCollectionTimer_Tick(object sender, EventArgs e)
        {
            //i++;
            //voltagePointCollection.Add(new VoltagePoint(Math.Sin(i * 0.1), DateTime.Now));
            CurrentTime = me_rawImage.Position.TotalMilliseconds;
        }
        private ShownData GetCurrentData(int timestamp)
        {
            foreach (ShownData item in dataList)
            {
                if (item.timeStamp <= timestamp + 35)
                {
                    return item;
                }
            }
            return dataList[0];
        }

        private int GetCurrentTimestamp(int frameNumber)
        {
            if (frameNumber >= imageTimeStampList.Count)
            {
                return imageTimeStampList.Last();
            }
            return imageTimeStampList[frameNumber];
        }

        private bool OpenDataStreams(string address)
        {
            try
            {
                imageTimeStampList = new List<int>();
                dataList = new List<ShownData>();
                preTimestamp = 0;
                // read timestamp 
                StreamReader timeReader = new StreamReader(address + "timestamp.txt");
                string line = timeReader.ReadLine();
                int firstStamp = Convert.ToInt32(line);
                while (!String.IsNullOrWhiteSpace(line))
                {
                    int timeStamp = Convert.ToInt32(line) - firstStamp;
                    imageTimeStampList.Add(timeStamp);
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


                    dataList.Add(new ShownData()
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
                dataList.Reverse();
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
            foreach (ShownData item in dataList)
            {
                voltagePointCollection_right.Add(new VoltagePoint(item.v_right, item.timeStamp));
                if (item.isSegmentPoint)
                {
                    AddSplitLine(item.timeStamp);
                }
            }
        }

        private void AddSplitLine(int split)
        {
            var tempPoints = new VoltagePointCollection();
            var v_right = new EnumerableDataSource<VoltagePoint>(tempPoints);
            v_right.SetXMapping(x => x.TimeStamp);
            v_right.SetYMapping(y => y.Voltage);
            cht_right.AddLineGraph(v_right, Colors.Black, 2, "V right");
            tempPoints.Add(new VoltagePoint(MaxVoltage, split));
            tempPoints.Add(new VoltagePoint(MinVoltage, split));

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
                DrawData();

                //TODO: dynamic FPS
                //totalFrame = (int)SliderSeek.Maximum / 200 + 1;
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
            if (me_rawImage.Source != null)
            {
                if (IsPlaying)
                {
                    IsPlaying = false;
                    me_rawImage.Play();
                    //Timer.Start();
                }
                else
                {
                    IsPlaying = true;
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
            if (me_rawImage.Source != null)
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
            me_rawImage.Position = TimeSpan.FromMilliseconds(sld_progress.Value);
            CurrentTime = sld_progress.Value;
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
