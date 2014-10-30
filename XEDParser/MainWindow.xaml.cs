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
using System.Windows.Forms;
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

        //we add
        public float RotateTan = MichaelRotateTan;      // Default person

        public Image<Bgra, byte> depthImg_old;

        public Image<Bgr, byte> colorImg_old;

        public bool isSaving_oldColorFrame = false;

        public bool isSaving_oldDepthFrame = false;

        public TextWriter writer { get; set; }

        CURELab.SignLanguage.HandDetector.KinectStudioController k = CURELab.SignLanguage.HandDetector.KinectStudioController.GetSingleton();

        //end of we add
        private KinectSensor _currentKinectSensor;
        public KinectSensor CurrentKinectSensor
        {
            get { return _currentKinectSensor; }
            set { _currentKinectSensor = value; }
        }
        private string _depthframe;
        public string DepthFrame 
        { 
            get { return _depthframe; }
            set { _depthframe = value;
                lbl_Depth.Content = value; }
        }
        private string _colorframe;
        public string ColorFrame
        {
            get { return _colorframe; }
            set
            {
                _colorframe = value;
                lbl_Color.Content = value;
            }
        }
        //private KinectSensor sensor;
        public List<Image<Bgr, byte>> ColorFrameList;
        public List<Image<Bgr, byte>> DepthFrameList;
        private KinectSensorChooser sensorChooser;
        VideoWriter colorWriter = null;
        long colorFirstTime = 0;
        VideoWriter depthWriter = null;
        StreamWriter skeWriter = null;
        long depthFirstTime = 0;
        string single_file_name = "abc";
        String folder_selected="C:\\Users\\user";
        string[] fileName;
        System.Timers.Timer timer;
        int old_frame_no = -1;
        int CurrentFrame = 0;
        bool IsRunning = false;

        Colorizer colorizer;
        public MainWindow()
        {
            ConsoleManager.Show();
            InitializeComponent();
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            DepthFrame = "0";
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            bool error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
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
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();
                    colorizer = new Colorizer(RotateTan, CurrentKinectSensor.DepthStream.MaxDepth, CurrentKinectSensor.DepthStream.MinDepth);

                    depthPixels = new DepthImagePixel[CurrentKinectSensor.DepthStream.FramePixelDataLength];
                    _colorPixels = new byte[CurrentKinectSensor.ColorStream.FramePixelDataLength];
                    try
                    {

                        //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Switch back to normal mode if Kinect does not support near mode
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
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
        private DepthImagePixel[] depthPixels;
        private System.Drawing.Point skeletonToDepth(SkeletonPoint sp, float angle_in)
        {
            DepthImagePoint dp = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(sp, DepthImageFormat.Resolution640x480Fps30);
            
            colorizer.Angle = angle_in;
            return new System.Drawing.Point(dp.X,dp.Y);
        }
        /*
        private void ColorFrameReady(object s, ColorImageFrameReadyEventArgs r)
        {
            Console.WriteLine("color:"+CurrentFrame.ToString());
            CurrentFrame++;
        }*/

        private void Run()
        {
            int i=0;
            fileName = Directory.GetFiles(@"" + folder_selected, "*.xed");
            Console.WriteLine("Successful for reading the folder");
            do
            {
                single_file_name = System.IO.Path.GetFileNameWithoutExtension(fileName[i]);
                if (!Directory.Exists(folder_selected + "\\" + single_file_name))
                    Directory.CreateDirectory(folder_selected + "\\" + single_file_name);   // Create the folder if it is not existed

                FileStream file_name = File.Open(@folder_selected + "\\" + single_file_name + "\\" + single_file_name + ".csv", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                colorWriter = new VideoWriter(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_c.avi", 30, 640, 480, true);
                depthWriter = new VideoWriter(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_d.avi", 30, 640, 480, true);
                skeWriter = new StreamWriter(file_name);
                Console.WriteLine("Successful for setting the writter");

                // !!!!  Detecting signer by the filename
                if (single_file_name.Contains("Michael"))
                    RotateTan = MichaelRotateTan;
                else if (single_file_name.Contains("Anita"))
                    RotateTan = AnitaRotateTan;
                else if (single_file_name.Contains("Aaron"))
                    RotateTan = AaronRotateTan;

                // !!!! End of detecting signer by the filename
                Console.WriteLine("Angle: " + RotateTan);
                colorizer.Angle = RotateTan;

                //lbl_process.Content = "Processing... :" + i + "\\" + fileName.Length + " " + filename;
                k.Open_File(fileName[i]);
                Console.WriteLine("Processing... :" + i + "\\" + fileName.Length + " " + single_file_name);
                //k.ReadFirstFrame();
                System.Threading.Thread.Sleep(500);

                Console.WriteLine("Run frame");
                k.Run();

                Console.WriteLine("Run frame finished");
                if (colorWriter != null)
                {
                    Console.WriteLine("ColorWriter");
                }
                Console.WriteLine("After ColorWriter");

                IsRunning = true;
                while (IsRunning)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine(old_frame_no);
                    Console.WriteLine(CurrentFrame);
                    if (CurrentFrame == 0)
                    {
                        continue;
                    }
                    if (old_frame_no != CurrentFrame)
                    {
                        Console.WriteLine("Running");
                        IsRunning = true;
                        old_frame_no = CurrentFrame;        // Save the frame no.
                    }
                    else
                    {
                        Console.WriteLine("End of the frame");
                        IsRunning = false;                                // Tell the program to proceed the next file
                    }
                    Console.WriteLine("Waiting...");
                }
                CloseAllWriter();

                Console.WriteLine("Finish");
                Console.WriteLine("==============================================");

                long c_length = new System.IO.FileInfo(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_c.avi").Length;
                long d_length = new System.IO.FileInfo(folder_selected + "\\" + single_file_name + "\\" + single_file_name + "_d.avi").Length;
                //if (c_length==d_length)
                    i++;
                //else Console.WriteLine("Redo!!!!!!!!");
                CurrentFrame = 0;
                old_frame_no = -1;


            } while (i < fileName.Length);
        }

        private int preColorFrame = 0;

        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            //Console.WriteLine("all:"+CurrentFrame.ToString());
            CurrentFrame++;

            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                if (sFrame != null)
                {
                    var skeletons = new Skeleton[sFrame.SkeletonArrayLength];
                    sFrame.CopySkeletonDataTo(skeletons);
                    Skeleton skel = skeletons[0];
                    DepthImagePoint dp_csv;
                    ColorImagePoint cp_csv;

                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        #region skeleton
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.Head].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        //head
                        float headX = skel.Joints[JointType.Head].Position.X;
                        float headY = skel.Joints[JointType.Head].Position.Y;
                        float headZ = skel.Joints[JointType.Head].Position.Z;
                        float headX_color = cp_csv.X;
                        float headY_color = cp_csv.Y;
                        float headX_depth = dp_csv.X;
                        float headY_depth = dp_csv.Y;

                        //shoulder
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float shoulderLX = skel.Joints[JointType.ShoulderLeft].Position.X;
                        float shoulderLY = skel.Joints[JointType.ShoulderLeft].Position.Y;
                        float shoulderLZ = skel.Joints[JointType.ShoulderLeft].Position.Z;
                        float shoulderLX_color = cp_csv.X;
                        float shoulderLY_color = cp_csv.Y;
                        float shoulderLX_depth = dp_csv.X;
                        float shoulderLY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderCenter].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float shoulderCX = skel.Joints[JointType.ShoulderCenter].Position.X;
                        float shoulderCY = skel.Joints[JointType.ShoulderCenter].Position.Y;
                        float shoulderCZ = skel.Joints[JointType.ShoulderCenter].Position.Z;
                        float shoulderCX_color = cp_csv.X;
                        float shoulderCY_color = cp_csv.Y;
                        float shoulderCX_depth = dp_csv.X;
                        float shoulderCY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ShoulderRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ShoulderRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float shoulderRX = skel.Joints[JointType.ShoulderRight].Position.X;
                        float shoulderRY = skel.Joints[JointType.ShoulderRight].Position.Y;
                        float shoulderRZ = skel.Joints[JointType.ShoulderRight].Position.Z;
                        float shoulderRX_color = cp_csv.X;
                        float shoulderRY_color = cp_csv.Y;
                        float shoulderRX_depth = dp_csv.X;
                        float shoulderRY_depth = dp_csv.Y;

                        //elbow
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ElbowLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float elbowLX = skel.Joints[JointType.ElbowLeft].Position.X;
                        float elbowLY = skel.Joints[JointType.ElbowLeft].Position.Y;
                        float elbowLZ = skel.Joints[JointType.ElbowLeft].Position.Z;
                        float elbowLX_color = cp_csv.X;
                        float elbowLY_color = cp_csv.Y;
                        float elbowLX_depth = dp_csv.X;
                        float elbowLY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.ElbowRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float elbowRX = skel.Joints[JointType.ElbowRight].Position.X;
                        float elbowRY = skel.Joints[JointType.ElbowRight].Position.Y;
                        float elbowRZ = skel.Joints[JointType.ElbowRight].Position.Z;
                        float elbowRX_color = cp_csv.X;
                        float elbowRY_color = cp_csv.Y;
                        float elbowRX_depth = dp_csv.X;
                        float elbowRY_depth = dp_csv.Y;

                        //writst
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.WristLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.WristLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float wristLX = skel.Joints[JointType.WristLeft].Position.X;
                        float wristLY = skel.Joints[JointType.WristLeft].Position.Y;
                        float wristLZ = skel.Joints[JointType.WristLeft].Position.Z;
                        float wristLX_color = cp_csv.X;
                        float wristLY_color = cp_csv.Y;
                        float wristLX_depth = dp_csv.X;
                        float wristLY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.WristRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.WristRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float wristRX = skel.Joints[JointType.WristRight].Position.X;
                        float wristRY = skel.Joints[JointType.WristRight].Position.Y;
                        float wristRZ = skel.Joints[JointType.WristRight].Position.Z;
                        float wristRX_color = cp_csv.X;
                        float wristRY_color = cp_csv.Y;
                        float wristRX_depth = dp_csv.X;
                        float wristRY_depth = dp_csv.Y;

                        // hand
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float handLX = skel.Joints[JointType.HandLeft].Position.X;
                        float handLY = skel.Joints[JointType.HandLeft].Position.Y;
                        float handLZ = skel.Joints[JointType.HandLeft].Position.Z;
                        float handLX_color = cp_csv.X;
                        float handLY_color = cp_csv.Y;
                        float handLX_depth = dp_csv.X;
                        float handLY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HandRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float handRX = skel.Joints[JointType.HandRight].Position.X;
                        float handRY = skel.Joints[JointType.HandRight].Position.Y;
                        float handRZ = skel.Joints[JointType.HandRight].Position.Z;
                        float handRX_color = cp_csv.X;
                        float handRY_color = cp_csv.Y;
                        float handRX_depth = dp_csv.X;
                        float handRY_depth = dp_csv.Y;

                        //spine
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.Spine].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.Spine].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float spineX = skel.Joints[JointType.Spine].Position.X;
                        float spineY = skel.Joints[JointType.Spine].Position.Y;
                        float spineZ = skel.Joints[JointType.Spine].Position.Z;
                        float spineX_color = cp_csv.X;
                        float spineY_color = cp_csv.Y;
                        float spineX_depth = dp_csv.X;
                        float spineY_depth = dp_csv.Y;

                        //hip
                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float hipLX = skel.Joints[JointType.HipLeft].Position.X;
                        float hipLY = skel.Joints[JointType.HipLeft].Position.Y;
                        float hipLZ = skel.Joints[JointType.HipLeft].Position.Z;
                        float hipLX_color = cp_csv.X;
                        float hipLY_color = cp_csv.Y;
                        float hipLX_depth = dp_csv.X;
                        float hipLY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipCenter].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipCenter].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float hipCX = skel.Joints[JointType.HipCenter].Position.X;
                        float hipCY = skel.Joints[JointType.HipCenter].Position.Y;
                        float hipCZ = skel.Joints[JointType.HipCenter].Position.Z;
                        float hipCX_color = cp_csv.X;
                        float hipCY_color = cp_csv.Y;
                        float hipCX_depth = dp_csv.X;
                        float hipCY_depth = dp_csv.Y;

                        dp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skel.Joints[JointType.HipRight].Position, DepthImageFormat.Resolution640x480Fps30);
                        cp_csv = CurrentKinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skel.Joints[JointType.HipRight].Position, ColorImageFormat.RgbResolution640x480Fps30);
                        float hipRX = skel.Joints[JointType.HipRight].Position.X;
                        float hipRY = skel.Joints[JointType.HipRight].Position.Y;
                        float hipRZ = skel.Joints[JointType.HipRight].Position.Z;
                        float hipRX_color = cp_csv.X;
                        float hipRY_color = cp_csv.Y;
                        float hipRX_depth = dp_csv.X;
                        float hipRY_depth = dp_csv.Y;


                        skeWriter.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66},{67},{68},{69},{70},{71},{72},{73},{74},{75},{76},{77},{78},{79},{80},{81},{82},{83},{84},{85},{86},{87},{88},{89},{90},{91},{92},{93},{94},{95},{96},{97}", headX, headY, headZ, headX_color, headY_color, headX_depth, headY_depth, shoulderLX, shoulderLY, shoulderLZ, shoulderLX_color, shoulderLY_color, shoulderLX_depth, shoulderLY_depth, shoulderCX, shoulderCY, shoulderCZ, shoulderCX_color, shoulderCY_color, shoulderCX_depth, shoulderCY_depth, shoulderRX, shoulderRY, shoulderRZ, shoulderRX_color, shoulderRY_color, shoulderRX_depth, shoulderRY_depth, elbowLX, elbowLY, elbowLZ, elbowLX_color, elbowLY_color, elbowLX_depth, elbowLY_depth, elbowRX, elbowRY, elbowRZ, elbowRX_color, elbowRY_color, elbowRX_depth, elbowRY_depth, wristLX, wristLY, wristLZ, wristLX_color, wristLY_color, wristLX_depth, wristLY_depth, wristRX, wristRY, wristRZ, wristRX_color, wristRY_color, wristRX_depth, wristRY_depth, handLX, handLY, handLZ, handLX_color, handLY_color, handLX_depth, handLY_depth, handRX, handRY, handRZ, handRX_color, handRY_color, handRX_depth, handRY_depth, spineX, spineY, spineZ, spineX_color, spineY_color, spineX_depth, spineY_depth, hipLX, hipLY, hipLZ, hipLX_color, hipLY_color, hipLX_depth, hipLY_depth, hipCX, hipCY, hipCZ, hipCX_color, hipCY_color, hipCX_depth, hipCY_depth, hipRX, hipRY, hipRZ, hipRX_color, hipRY_color, hipRX_depth, hipRY_depth);
                                                #endregion
                    }
                    else
                    {
                        skeWriter.WriteLine("untracked");
                    }
                }
                else
                {
                    skeWriter.WriteLine("0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");
                }
            }

            var sw = new Stopwatch();
            sw.Start();
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (preColorFrame == 0)
                    {
                        preColorFrame = colorFrame.FrameNumber;
                    }
                    
                    //Console.WriteLine('c' + colorFrame.FrameNumber.ToString());
                    //Console.WriteLine("================Enter normal color frame ===========");
                    ColorFrame = colorFrame.FrameNumber.ToString();    ColorFrame = colorFrame.FrameNumber.ToString();

                    if (colorFirstTime == 0)
                    {
                        colorFirstTime = colorFrame.Timestamp;
                    }

                    colorFrame.CopyPixelDataTo(this._colorPixels);
                    var img = ImageConverter.Array2Image(_colorPixels, 640, 480, 640 * 4);
                    var time = colorFrame.Timestamp - colorFirstTime;
                    int count = colorFrame.FrameNumber - preColorFrame;
                    if (img.Ptr != IntPtr.Zero)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            colorWriter.WriteFrame(img.Convert<Bgr, byte>());
                        }
                        colorImg_old = img.Convert<Bgr, byte>();
                    }

                    preColorFrame = colorFrame.FrameNumber;
                }
                else
                {
                    Console.WriteLine("================Enter color frame old ===========");

                    if (colorImg_old.Ptr != IntPtr.Zero)
                    {
                        Console.WriteLine("=======================================================");
                        Console.WriteLine("=============Enter color writer =======================");
                        Console.WriteLine("=======================================================");
                        colorWriter.WriteFrame(colorImg_old);
                    }
                }
            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    Console.WriteLine('d'+depthFrame.FrameNumber.ToString());
                    //Console.WriteLine("================Enter normal depth frame ===========");
                    DepthFrame = depthFrame.FrameNumber.ToString();

                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                    //int minDepth = depthFrame.MinDepth;
                    //int maxDepth = depthFrame.MaxDepth;
                    int width = depthFrame.Width;
                    int height = depthFrame.Height;

                    colorizer.TransformAndConvertDepthFrame(depthPixels, _colorPixels);

                    Image<Bgra, byte> depthImg;
                    depthImg = ImageConverter.Array2Image(_colorPixels, width, height, width * 4);
                    depthImg_old = depthImg;
                    if (depthImg.Ptr != IntPtr.Zero)
                    {
                        depthWriter.WriteFrame(depthImg.Convert<Bgr, byte>());
                    }
                }
                else if (depthImg_old!=null)
                {
                    Console.WriteLine("================Enter depth frame old ===========");

                    if (depthImg_old.Ptr != IntPtr.Zero)
                    {
                        Console.WriteLine("=======================================================");
                        Console.WriteLine("=============Enter depth writer =======================");
                        Console.WriteLine("=======================================================");
                        depthWriter.WriteFrame(depthImg_old.Convert<Bgr, byte>());
                    }
                }

            }
            

        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            
            var t = new Thread(new ThreadStart(Run));
            t.Start();

            while(colorWriter == null)
            {
            }
            CurrentKinectSensor.AllFramesReady += AllFrameReady;
        }

    
        private void btn_end_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentKinectSensor.AllFramesReady -= AllFrameReady;

            }
            catch (Exception)
            {
                //throw;
                Console.WriteLine("Error");
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
            }
            if (depthWriter != null)
            {
                depthWriter.Dispose();
            }
            if (skeWriter != null)
            {
                skeWriter.Dispose();
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

        /*** Edit start **/

        private void btn_Start_Kinect(object sender, RoutedEventArgs e)
        {
            bool con = k.Start();
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
            bool con = k.Connect();
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
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath);
                //lbl_folder.Content = files.Length.ToString();
                fbd.SelectedPath = @"D:\Kinect data\test";
                lbl_folder.Content = fbd.SelectedPath;
                folder_selected = fbd.SelectedPath;
            }
           
        }

        /*** Edit end **/

    }
}
