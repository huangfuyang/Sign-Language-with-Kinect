using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CURELab.SignLanguage.HandDetector.Annotations;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CURELab.SignLanguage.HandDetector
{
    internal class VideoProcessor : INotifyPropertyChanged
    {
        public static double DIFF = 10;
        private static VideoProcessor m_VideoProcessor;
        private OpenCVController m_opencv;

        private System.Drawing.Point rightHandPosition;
        private System.Drawing.Point headPosition;

        public static double CullingThresh;
        public static float AngleRotateTan = AaronRotateTan;
        // demo
        public const float DemoRotateTan = 0.45f;
        // anita
        public const float AnitaRotateTan = 0.3f;
        // michael
        public const float MichaelRotateTan = 0.23f;
        // Aaron
        public const float AaronRotateTan = 0.28f;

        private const int handShapeWidth = 60;
        private const int handShapeHeight = 60;
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;

        Capture _Capture;

        double FrameRate = 0;
        double TotalFrames = 0;

        int FrameCount;

        private WriteableBitmap colorWriteBitmap;

        public WriteableBitmap ColorWriteBitmap
        {
            get { return colorWriteBitmap; }
            protected set { colorWriteBitmap = value; }
        }

        private WriteableBitmap depthWriteBitmap;

        public WriteableBitmap DepthWriteBitmap
        {
            get { return depthWriteBitmap; }
            protected set { depthWriteBitmap = value; }
        }

        private WriteableBitmap processedDepthBitmap;

        public WriteableBitmap ProcessedDepthBitmap
        {
            get { return processedDepthBitmap; }
            protected set { processedDepthBitmap = value; }
        }

        private WriteableBitmap grayWriteBitmap;

        public WriteableBitmap GrayWriteBitmap
        {
            get { return grayWriteBitmap; }
            protected set { grayWriteBitmap = value; }
        }

        public WriteableBitmap WrtBMP_RightHandFront { get; set; }
        public WriteableBitmap WrtBMP_LeftHandFront { get; set; }
        public WriteableBitmap WrtBMP_Candidate2 { get; set; }
        public WriteableBitmap WrtBMP_Candidate3 { get; set; }
        public WriteableBitmap WrtBMP_Candidate1 { get; set; }
        public WriteableBitmap WrtBMP_RightHandSide { get; set; }
        public WriteableBitmap WrtBMP_LeftHandSide { get; set; }

        protected VideoProcessor()
        {
            m_opencv = OpenCVController.GetSingletonInstance();
            this.ColorWriteBitmap = new WriteableBitmap(FrameWidth, FrameHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Bgr32, null);
            this.DepthWriteBitmap = new WriteableBitmap(FrameWidth, FrameHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Bgr32, null);
            this.WrtBMP_RightHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Gray8, null);
            this.WrtBMP_LeftHandFront = new WriteableBitmap(handShapeWidth, handShapeHeight, 96.0, 96.0,
                System.Windows.Media.PixelFormats.Gray8, null);

            rightHandPosition = new System.Drawing.Point();
        }

        public static VideoProcessor GetSingletonInstance()
        {
            return m_VideoProcessor ?? (m_VideoProcessor = new VideoProcessor());
        }

        public void OpenFile(string path)
        {
            if (_Capture != null)
            {
                _Capture.Dispose();//dispose of current capture
            }
            _Capture= new Capture(path);
            FrameRate = _Capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
            TotalFrames = _Capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
            //The four_cc returns a double so we must convert it
            double codec_double = _Capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FOURCC);

        }

        public void ProcessFrame()
        {
            if (_Capture!= null)
            {
                var r = _Capture.Grab();
                var img = _Capture.RetrieveGrayFrame();
                ImageConverter.UpdateWriteBMP(colorWriteBitmap, img.ToBitmap());

            }
        }

    #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
