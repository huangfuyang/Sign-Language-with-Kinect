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
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;

using CURELab.SignLanguage.RecognitionSystem.StaticTools;

namespace VideoPlayer
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer Timer;
        BindingList<RealtimeGraphItem> ItemsAccLeft = new BindingList<RealtimeGraphItem>();
        BindingList<RealtimeGraphItem> ItemsAccRight = new BindingList<RealtimeGraphItem>();
        BindingList<RealtimeGraphItem> ItemsVelLeft = new BindingList<RealtimeGraphItem>();
        BindingList<RealtimeGraphItem> ItemsVelRight = new BindingList<RealtimeGraphItem>();

        List<int> imageTimeStampList;
        List<ShownData> dataList;


        bool isPauseOnSegment;

        double totalDuration;
        double currentTime;

        string FileName = "";
        int totalFrame;
        int preTimestamp = 0;

        public MainWindow()
        {
            InitializeComponent();

            isPauseOnSegment = false;
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
          
            // init time
            currentTime = 0;

            // attach source 
            Graph_1.SeriesSource = ItemsAccLeft;
            Graph_2.SeriesSource = ItemsAccRight;
            Graph_3.SeriesSource = ItemsVelLeft;
            Graph_4.SeriesSource = ItemsVelRight;


            // init timer
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };

            Timer.Tick += TimerTick;

            ConsoleManager.Show();

       
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


        private void TimerTick(object sender, EventArgs e)
        {

            if (MediaPlayer.HasVideo)
            {
                
                currentTime = MediaPlayer.Position.TotalMilliseconds;
                int currentFrame =(int)( totalFrame * currentTime / totalDuration);
                int currentTimestamp = GetCurrentTimestamp(currentFrame);


                if (currentTime <= totalDuration)
                {
                    //FilenameBox.Text = SliderSeek.Value.ToString();
                    SliderSeek.Value = currentTime;
                    ShownData currentData = GetCurrentData(currentTimestamp);
                    double accLeft, accRight;
                    double velLeft, velRight;
                    accLeft = currentData.a_left;
                    accRight = currentData.a_right;
                  
                    velLeft = currentData.v_left;
                    velRight = currentData.v_right;
                   
                    // add data to the graph
                    RealtimeGraphItem newItem = new RealtimeGraphItem
                    {
                        Time = (int)DateTime.Now.TimeOfDay.TotalMilliseconds,
                        Value = accLeft
                    };

                    ItemsAccLeft.Add(newItem);
                    AccLeft.Text = accLeft + "";

                    newItem = new RealtimeGraphItem
                    {
                        Time = (int)DateTime.Now.TimeOfDay.TotalMilliseconds,
                        Value = accRight
                    };

                    ItemsAccRight.Add(newItem);
                    AccRight.Text = accRight + "";
               //     Console.WriteLine(currentData.timeStamp);

                    newItem = new RealtimeGraphItem
                    {
                        Time = (int)DateTime.Now.TimeOfDay.TotalMilliseconds,
                        Value = velLeft
                    };

                    ItemsVelLeft.Add(newItem);
                    VelLeft.Text = velLeft + "";

                    newItem = new RealtimeGraphItem
                    {
                        Time = (int)DateTime.Now.TimeOfDay.TotalMilliseconds,
                        Value = velRight
                    };

                    ItemsVelRight.Add(newItem);
                    VelRight.Text = velRight + "";
             //       Console.WriteLine(currentData.timeStamp);



                    if (isPauseOnSegment &&  currentData.isSegmentPoint && currentData.timeStamp != preTimestamp)
                    {
                        Console.WriteLine(currentData.timeStamp.ToString());
                        PauseButtonClick(new object(), new RoutedEventArgs());
                    }

                    preTimestamp = currentData.timeStamp;
                }
            }
        }


        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            SliderSeek.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        
            totalDuration = SliderSeek.Maximum;
            if (!OpenDataStreams(FileName))
            {
                PlayButton.IsEnabled = false;
                PopupWarn("Open Failed");
                return;
            }
            else
            {
                PlayButton.IsEnabled = true;
            }
            //TODO: dynamic FPS
            totalFrame = (int)SliderSeek.Maximum / 200 + 1;

        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            StopButtonClick(new object(), new RoutedEventArgs());
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
                while (!String.IsNullOrWhiteSpace(line))
                {
                    int timeStamp = Convert.ToInt32(line);
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

                int firstTime = Convert.ToInt32(a_line.Split(' ')[0]);
                while (!String.IsNullOrWhiteSpace(v_line) && !String.IsNullOrWhiteSpace(a_line) )
                {
                    string[] words = a_line.Split(' ');
                    int dataTime = Convert.ToInt32(words[0]);
                   
                    double aLeft = Convert.ToDouble(words[1]);
                    double aRight = Convert.ToDouble(words[2]);
                    words = v_line.Split(' ');
                    double vLeft = Convert.ToDouble(words[1]);
                    double vRight = Convert.ToDouble(words[2]);

                    int segTime = Convert.ToInt32(seg_line);
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
                    if ( isSegpoint)
                    {
                        string newTime = segPointReader.ReadLine();
                        while (Convert.ToInt32(newTime) == dataTime)
                        {
                            newTime = segPointReader.ReadLine();
                        }
                        seg_line = newTime;

                    }
                }
                foreach (ShownData item in dataList)
                {
                    if (item.isSegmentPoint)
                    {
                        Console.WriteLine(item.timeStamp.ToString());
                    }
                }
                dataList.Reverse();
                accReader.Close();
                accReader = null;
                veloReader.Close();
                veloReader = null;
            }
            catch (Exception e)
            {
                PopupWarn("Insufficient data file!");
                return false;
            }


            return true;

        }


        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Position = TimeSpan.FromMilliseconds(SliderSeek.Value);
            currentTime = SliderSeek.Value;
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                MediaPlayer.Play();
                Timer.Start();
                
               
                PauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                PlayButton.IsEnabled = false;
            }
            else
            {
                PopupWarn("no video");
            }

        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            MediaPlayer.Pause();

           

            PlayButton.IsEnabled = true;
            StopButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            currentTime = 0;
            SliderSeek.Value = 0;
            MediaPlayer.Stop();

           
            
            PauseButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".avi";
            dlg.Filter = "media file (.avi)|*.avi";
            if (dlg.ShowDialog().Value)
            {
                // Open document 
                FilenameBox.Text = dlg.SafeFileName;
                MediaPlayer.Source = new Uri(dlg.FileName);
                try
                {
                    MediaPlayer.LoadedBehavior = MediaState.Manual;
                    MediaPlayer.UnloadedBehavior = MediaState.Manual;
                    string addr = dlg.FileName.Substring(0, dlg.FileName.Length - dlg.SafeFileName.Length);
                    FileName = addr + dlg.SafeFileName.Split('_')[0] + '_';
                    
                }
                catch (Exception e1)
                {

                    PopupWarn(e1.ToString());
                }
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

        private void SpeedSlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.SpeedRatio = e.NewValue;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e){
            isPauseOnSegment = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isPauseOnSegment = false;
        }

    }
}
