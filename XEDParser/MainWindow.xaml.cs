using System.Windows.Forms;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Diagnostics;

namespace XEDParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.28f;

        //adding
        CURELab.SignLanguage.HandDetector.KinectStudioController controler = CURELab.SignLanguage.HandDetector.KinectStudioController.GetSingleton();

        private SkeletonConverter mSkeletonConverter = new SkeletonConverter();

        public int PreColorFrameNumber = -1;
        public int PreDepthFrameNumber = -1;
        public int first_skeleton_frame_number = -1;
        public int kinect_current_color_frame_number = -1;
        public int kinect_current_depth_frame_number = -1;
        public int kinect_current_skeleton_frame_number = -1;

        public int old_color_frame_no = -1;
        public int old_depth_frame_no = -1;
        public int old_skeleton_frame_no = -1;

        public int current_color_frame_no = -1;
        public int current_depth_frame_no = -1;
        public int current_skeleton_frame_no = -1;

        public int old_frame_number_for_stop = -1;
        public int current_frame_number_for_stop = -1;

        public int old_depth_frame_number_for_stop = -1;
        public int current_depth_frame_number_for_stop = -1;

        public int old_skeleton_frame_number_for_stop = -1;
        public int current_skeleton_frame_number_for_stop = -1;
        public bool frame_thread_isrunning = true;
        public bool file_IsReady = false;
        private bool file_ready2 = false;
        String folder_selected = "C:\\Users\\user";
        string single_file_name = "abc";
        public bool Is_running = true;
        public bool all_finished = false;
        string[] fileName;

        enum States { start, running_start, Isrunning, ending , finished };
        //end of adding

        private KinectSensor _currentKinectSensor;
        public KinectSensor CurrentKinectSensor
        {
            get { return _currentKinectSensor; }
            set { _currentKinectSensor = value; }
        }
        private long _depthframe;
        public long DepthTS 
        { 
            get { return _depthframe; }
            set { _depthframe = value;
                lbl_Depth.Content = value.ToString(); }
        }
        private long _colorframe;
        public long ColorTS
        {
            get { return _colorframe; }
            set
            {
                _colorframe = value;
                lbl_Color.Content = value.ToString();
            }
        }

        public List<Image<Bgr, byte>> ColorFrameList;
        public List<Image<Bgr, byte>> DepthFrameList;
        private KinectSensorChooser sensorChooser;
        VideoWriter colorWriter = null;
        long FirstTimeStamp = 0;
        VideoWriter depthWriter = null;
        StreamWriter skeWriter = null;
        long depthFirstTime = 0;
        private System.Timers.Timer timer;
        int waiting = 0;

        public int Waiting
        {
            get { return waiting; }
            set {
                //Console.WriteLine(value);
                waiting = value;
            }
        }

        Colorizer colorizer;
        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
        /// <summary>
        /// Intermediate storage for the depth to color mapping
        /// </summary>
        private ColorImagePoint[] colorCoordinates;
        public MainWindow()
        {
            ConsoleManager.Show();
            Console.WriteLine("initialization");
            InitializeComponent();
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            Console.WriteLine("sensor start.....");
            this.sensorChooser.Start();
            DepthTS = 0;
            ColorTS = 0;
            timer = new System.Timers.Timer(20);
            timer.Elapsed += timer_Elapsed;
            Console.WriteLine("initialized");
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            bool error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    //args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException) { error = true; }
            }

            if (args.NewSensor != null)
            {
                CurrentKinectSensor = args.NewSensor;
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();

                    depthPixels = new DepthImagePixel[CurrentKinectSensor.DepthStream.FramePixelDataLength];
                    _colorPixels = new byte[CurrentKinectSensor.ColorStream.FramePixelDataLength];
                    _depthPixels = new byte[CurrentKinectSensor.DepthStream.FramePixelDataLength*3];
                    colorCoordinates = new ColorImagePoint[CurrentKinectSensor.DepthStream.FramePixelDataLength];
                    try
                    {
                        
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                        //args.NewSensor.DepthStream.Range = DepthRange.Near;
                        //args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Switch back to normal mode if Kinect does not support near mode
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        //args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

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
        }

        private byte[] _colorPixels;
        private byte[] _depthPixels;
        private DepthImagePixel[] depthPixels;

        private int GetRealCurrentFrame(long tsOffset)
        {
            return (int)Math.Round(Convert.ToDouble(tsOffset) / 33.3);
        }

        public async void ExecuteAsync(Action action, int timeoutInMilliseconds)
        {
            await Task.Delay(timeoutInMilliseconds);
            action();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("timer"+Waiting.ToString());
            if (Waiting == 0)
            {
                controler.Run_by_clik();
                //Console.WriteLine("click");
            }
        }

        //private void skele_FrameReady(object sender, AllFramesReadyEventArgs e)
        private int pre_skeleton_frame = 0;
        private void skele_FrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                current_skeleton_frame_number_for_stop++;
                if (Waiting == 0)
                {
                    Waiting++;
                }
                //Console.WriteLine("enter skeleton FrameReady" + current_skeleton_frame_number_for_stop);
                //Console.WriteLine("    " + controler.ReadCurrentFrame());
                using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
                {
                    if (sFrame != null)
                    {
                        if (first_skeleton_frame_number == 0)
                        {
                            first_skeleton_frame_number = sFrame.FrameNumber;
                            pre_skeleton_frame = sFrame.FrameNumber-1;
                        }

                        var skeletons = new Skeleton[sFrame.SkeletonArrayLength];
                        sFrame.CopySkeletonDataTo(skeletons);
                        Skeleton skel = skeletons[0];
                        
                        for (int i = 0; i < sFrame.FrameNumber-pre_skeleton_frame-1; i++)
                        {
                            skeWriter.WriteLine("null");
                        }
                        pre_skeleton_frame = sFrame.FrameNumber;
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            skeWriter.WriteLine(mSkeletonConverter.getSkeletonLine(CurrentKinectSensor, skel.Joints));
                        }
                        else
                        {
                            skeWriter.WriteLine("untracked");
                        }
                    }
                    else
                    {
                        skeWriter.WriteLine("null");
                    }

                }
            }
            finally
            {
                Waiting -= 1;
            }
        }

        private void color_FrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {

        }

        private void depth_FrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            
            
        }

        private void frame_threading()
        {
            int i = 0;
            States current_states = new States();
            current_states = States.start;
            fileName = Directory.GetFiles(@"" + folder_selected, "*.xed");
            Console.WriteLine("Successful to get all file name");
            if (fileName.Length < 1)
            {
                Console.WriteLine("No xed file in this folder");
                return;
            }
            colorizer = new Colorizer(AaronRotateTan, CurrentKinectSensor.DepthStream.MaxDepth, CurrentKinectSensor.DepthStream.MinDepth);

            while (Is_running)
            {
                switch (current_states)
                {
                    case States.start:
                        single_file_name = System.IO.Path.GetFileNameWithoutExtension(fileName[i]);
                        if (!Directory.Exists(folder_selected + "\\" + single_file_name))
                            Directory.CreateDirectory(folder_selected + "\\" + single_file_name);
                        colorWriter = new VideoWriter(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_c.avi", 30, 640, 480, true);
                        depthWriter = new VideoWriter(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_d.avi", 30, 640, 480, true);
                        FileStream file_name = File.Open(@folder_selected + "\\" + single_file_name + "\\" + single_file_name + ".csv", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        skeWriter = new StreamWriter(file_name);

                        // !!!!  Detecting signer by the filename
                        if (single_file_name.Contains("Micheal"))
                        {
                            colorizer.Angle = MichaelRotateTan;
                            Console.WriteLine("Start state .. .. .. " + MichaelRotateTan);
                        }
                        else if (single_file_name.Contains("Anita"))
                        {
                            colorizer.Angle = AnitaRotateTan;
                            Console.WriteLine("Start state .. .. .. " + AnitaRotateTan);
                        }
                        else if (single_file_name.Contains("Aaron"))
                        {
                            colorizer.Angle = AaronRotateTan;
                            Console.WriteLine("Start state .. .. .. " + AaronRotateTan);
                        }

                        controler.Open_File(fileName[i]);
                        System.Threading.Thread.Sleep(1000);

                        old_frame_number_for_stop = 0;
                        old_skeleton_frame_number_for_stop = 0;
                        current_frame_number_for_stop = 0;
                        current_skeleton_frame_number_for_stop = 0;

                        PreColorFrameNumber = -1;
                        PreDepthFrameNumber = -1;
                        first_skeleton_frame_number = 0;
                        FirstTimeStamp = long.MaxValue;
                        current_states = States.running_start;
                        break;
                    case States.running_start:
                        file_IsReady = true;
                        timer.Start();
                        controler.Run_by_clik();
                        //controler.Run();
                        
                        current_states = States.Isrunning;
                        break;
                    case States.Isrunning:
                        System.Threading.Thread.Sleep(2000);
                        Console.WriteLine("Is Running State");
                        //Console.WriteLine("Isrunning states..." + current_frame_number_for_stop + "  " + old_frame_number_for_stop + "  " + current_skeleton_frame_number_for_stop +"  " + old_skeleton_frame_number_for_stop +"  " + Waiting);
                        if ((current_frame_number_for_stop == old_frame_number_for_stop) &&
                            (current_skeleton_frame_number_for_stop == old_skeleton_frame_number_for_stop))
                        {
                            current_states = States.ending;
                            timer.Stop();
                            Waiting = 0;
                        }
                        old_frame_number_for_stop = current_frame_number_for_stop;
                        old_skeleton_frame_number_for_stop = current_skeleton_frame_number_for_stop;
                        break;
                    case States.ending:
                        Console.WriteLine("Is ending state......");
                        i++;
                        //CloseAllWriter();
                        System.Threading.Thread.Sleep(400);
                        if (i >= fileName.Length)
                            current_states = States.finished;
                        else
                            current_states = States.start;
                            CloseAllWriter();
                        break;
                    case States.finished:
                        Console.WriteLine("Happy finished all file in this folder");
                        CloseAllWriter();
                        all_finished = true;
                        break;
                    default:
                        break;
                }
                if (all_finished)
                    break;
            }
        }

        private void RunAll()
        {
            int i = 0;
            fileName = Directory.GetFiles(@"" + folder_selected, "*.xed");
            Console.WriteLine("Successful for reading the folder");
            
            do
            {
                file_ready2 = false;
                single_file_name = System.IO.Path.GetFileNameWithoutExtension(fileName[i]);
                if (!Directory.Exists(folder_selected + "\\" + single_file_name))
                    Directory.CreateDirectory(folder_selected + "\\" + single_file_name);   // Create the folder if it is not existed

                FileStream file_name = File.Open(@folder_selected + "\\" + single_file_name + "\\" + single_file_name + ".csv", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                skeWriter = new StreamWriter(file_name);
                Console.WriteLine("Successful for setting the writter");

                // !!!!  Detecting signer by the filename
                /*if (single_file_name.Contains("Michael"))
                    colorizer.Angle = MichaelRotateTan;
                else if (single_file_name.Contains("Anita"))
                    colorizer.Angle = AnitaRotateTan;
                else if (single_file_name.Contains("Aaron"))
                    colorizer.Angle = AaronRotateTan;

                // !!!! End of detecting signer by the filename
                Console.WriteLine("Angle: " + colorizer.Angle);*/

                //lbl_process.Content = "Processing... :" + i + "\\" + fileName.Length + " " + filename;
                controler.Open_File(fileName[i]);
                Console.WriteLine("Processing... :" + (i+1) + "\\" + fileName.Length + " " + single_file_name);
                //k.ReadFirstFrame();
                System.Threading.Thread.Sleep(1500);
                file_ready2 = true;
                Console.WriteLine("Run frame");
                first_skeleton_frame_number = 0;
                CurrentKinectSensor.SkeletonFrameReady -= skele_FrameReady;
                CurrentKinectSensor.SkeletonFrameReady += skele_FrameReady;
                controler.Run();

                bool IsRunning = true;
                while (IsRunning)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (current_skeleton_frame_number_for_stop <= 0)
                    {
                        continue;
                    }
                    if (current_skeleton_frame_number_for_stop != old_skeleton_frame_number_for_stop)
                    {
                        Console.WriteLine("Running " + current_skeleton_frame_number_for_stop);
                        IsRunning = true;
                        old_skeleton_frame_number_for_stop = current_skeleton_frame_number_for_stop;        // Save the frame no.
                    }
                    else
                    {
                        Console.WriteLine("End of the frame");
                        IsRunning = false;                                // Tell the program to proceed the next file
                    }
                    //Console.WriteLine("Waiting...");
                }
                System.Threading.Thread.Sleep(2000);
                CloseAllWriter();

                Console.WriteLine("Finish");
                Console.WriteLine("==============================================");

                i++;
            } while (i < fileName.Length);
            Console.WriteLine("Finish Processing on all files");
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            var t = new Thread(new ThreadStart(frame_threading));
            t.Start();

            //while (!all_finished)
            //{
                while (colorWriter == null || /*skeWriter == null ||*/ depthWriter == null || !file_IsReady) ;
                //CurrentKinectSensor.SkeletonFrameReady += skele_FrameReady;
                //CurrentKinectSensor.ColorFrameReady += color_FrameReady;
                //CurrentKinectSensor.DepthFrameReady += depth_FrameReady;
                //while (frame_thread_isrunning) ;
                CurrentKinectSensor.AllFramesReady += CurrentKinectSensor_AllFramesReady;
            //}
        }

        private void btn_Ske_Start_Click(object sender, RoutedEventArgs e)
        {
            
            var t1 = new Thread(new ThreadStart(RunAll));
            t1.Start();
           
            //CurrentKinectSensor.AllFramesReady += skele_FrameReady;
        }

        void CurrentKinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                current_frame_number_for_stop++;
                if (Waiting == 0)
                {
                    Waiting++;
                }

                //Console.WriteLine("Color:" + Waiting.ToString());
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame != null)
                    {
                        int count = 0;
                        int frameNumber = 0;
                        if (colorFrame.Timestamp != 0)
                        {
                            if (colorFrame.Timestamp < FirstTimeStamp)
                            {
                                FirstTimeStamp = colorFrame.Timestamp;
                            }
                            frameNumber = GetRealCurrentFrame(colorFrame.Timestamp - FirstTimeStamp);
                            count = frameNumber - PreColorFrameNumber;
                            PreColorFrameNumber = frameNumber;
                            //Console.WriteLine("Color {0} {1} {2} {3} {4}", FirstTimeStamp, colorFrame.Timestamp, colorFrame.Timestamp - FirstTimeStamp, frameNumber,count);

                            colorFrame.CopyPixelDataTo(this._colorPixels);
                            var img = ImageConverter.Array2Image(_colorPixels, 640, 480, 640 * 4).Convert<Bgr, byte>();
                            if (img.Ptr != IntPtr.Zero)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    if (colorWriter != null)
                                        colorWriter.WriteFrame(img);
                                }
                            }
                        }
                    }
                }
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame != null)
                    {
                        int count = 0;
                        int frameNumber = 0;
                        if (depthFrame.Timestamp != 0)
                        {
                            if (depthFrame.Timestamp < FirstTimeStamp)
                            {
                                FirstTimeStamp = depthFrame.Timestamp;
                            }

                            frameNumber = GetRealCurrentFrame(depthFrame.Timestamp - FirstTimeStamp);
                            count = frameNumber - PreDepthFrameNumber;
                            PreDepthFrameNumber = frameNumber;
                            //Console.WriteLine("Depth {0} {1} {2} {3} {4}", FirstTimeStamp, depthFrame.Timestamp, depthFrame.Timestamp - FirstTimeStamp, frameNumber, count);

                            depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                            //int minDepth = depthFrame.MinDepth;
                            //int maxDepth = depthFrame.MaxDepth;

                            int width = depthFrame.Width;
                            int height = depthFrame.Height;
                            //Console.WriteLine("Depth:{0} {1}" ,DepthTS,count);

                            CurrentKinectSensor.CoordinateMapper.MapDepthFrameToColorFrame(
                                   DepthFormat,
                                   this.depthPixels,
                                   ColorFormat,
                                   this.colorCoordinates);
                            colorizer.TransformAndConvertDepthFrame(depthPixels, _depthPixels, colorCoordinates);

                            var depthImg = ImageConverter.Array2Image(_depthPixels, width, height, width * 3);
                            if (depthImg.Ptr != IntPtr.Zero)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    if (depthWriter != null)
                                        depthWriter.WriteFrame(depthImg);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Waiting -= 1;
            }
        }

        private void btn_end_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentKinectSensor.SkeletonFrameReady -= skele_FrameReady;
                //CurrentKinectSensor.AllFramesReady -= skele_FrameReady;
                //CurrentKinectSensor.ColorFrameReady -= color_FrameReady;
                //CurrentKinectSensor.DepthFrameReady -= depth_FrameReady;
                CurrentKinectSensor.AllFramesReady -= CurrentKinectSensor_AllFramesReady;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CloseAllWriter();
        }

        private void CloseAllWriter()
        {
            if (colorWriter != null)
            {
                colorWriter.Dispose();
                colorWriter = null;
            }
            if (depthWriter != null)
            {
                depthWriter.Dispose();
                depthWriter = null;
            }
            if (skeWriter != null)
            {
                skeWriter.Dispose();
                skeWriter = null;
            }
        }

        private string GenerateSkeletonArgs(Skeleton s)
        {
            return null;
        }

        private System.Drawing.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }

        private void btn_Start_Kinect(object sender, RoutedEventArgs e)
        {
            bool con = controler.Start();
            if (con)
            {
                lbl_connect.Content = "Connected";
            }
            else
            {
                lbl_connect.Content = "Not Connected";
            }
        }

        private void btn_Connect_Kinect(object sender, RoutedEventArgs e)
        {
            bool con = controler.Connect();
            controler.connect_kinect();
            if (con)
            {
                lbl_connect.Content = "Connected";
            }
            else
            {
                lbl_connect.Content = "Not Connected";
            }
        }

        private void btn_Folder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            string[] files;
            if (DialogResult != null)
                files = Directory.GetFiles(fbd.SelectedPath);
            //fbd.SelectedPath = @"F:\Aaron\test\";
            //lbl_folder.Content = files.Length.ToString();
            lbl_folder.Content = fbd.SelectedPath;
            folder_selected = fbd.SelectedPath;
        }
    }
}
