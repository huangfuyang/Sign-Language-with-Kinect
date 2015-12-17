using System.Windows;
using CURELab.SignLanguage.HandDetector;
using Microsoft.Kinect.Toolkit;
using Newtonsoft.Json.Linq;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Show();
            KinectSensorChooser sensorChooser = (KinectSensorChooser)this.Resources["sensorChooser"];
            var resource = LearningResource.GetSingleton();

            KinectState.Instance.KinectRegion = kinectRegion;

            KinectState.Instance.PropertyChanged += KinectState_PropertyChanged;
            sensorChooser.KinectChanged += KinectState.Instance.OnKinectChanged;
            sensorChooser.Start();
            var socket = SocketManager.GetInstance("137.189.90.112", 51243);
        }   

        void KinectState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("CurrentKinectSensor".Equals(e.PropertyName))
            {
                frmPageContainer.Source = new System.Uri("SplashPage.xaml", System.UriKind.RelativeOrAbsolute);
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            KinectState.Instance.StopKinect();
        }
    }
}
