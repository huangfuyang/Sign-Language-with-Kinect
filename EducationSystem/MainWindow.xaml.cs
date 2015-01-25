using System.Windows;
using Microsoft.Kinect.Toolkit;

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
            KinectSensorChooser sensorChooser = (KinectSensorChooser)this.Resources["sensorChooser"];
            sensorChooser.KinectChanged += KinectState.Instance.OnKinectChanged;
            sensorChooser.Start();
        }
    }
}
