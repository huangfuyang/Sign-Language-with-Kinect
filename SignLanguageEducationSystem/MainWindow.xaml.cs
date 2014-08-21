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
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace SignLanguageEducationSystem {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private KinectSensorChooser sensorChooser;
		private SystemStatusCollection systemStatusCollection;
		private StartPage startPage;

		public MainWindow() {
			InitializeComponent();
			this.DataContext = systemStatusCollection = new SystemStatusCollection();
			this.startPage = new StartPage(systemStatusCollection);
			this.kinectRegionGrid.Children.Add(this.startPage);

			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			this.sensorChooser = new KinectSensorChooser();
			this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
			this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
			this.sensorChooser.Start(); 
		}

		private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args) {

			bool error = false;

			if (args.OldSensor != null) {
				try {
					args.OldSensor.DepthStream.Range = DepthRange.Default;
					args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
					args.OldSensor.DepthStream.Disable();
					args.OldSensor.ColorStream.Disable();
				} catch (InvalidOperationException) { error = true; }
			}

			if (args.NewSensor != null) {
				try {
					args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
					args.NewSensor.SkeletonStream.Enable();

					try {
						args.NewSensor.DepthStream.Range = DepthRange.Near;
						args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
						args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
					} catch (InvalidOperationException) {
						// Switch back to normal mode if Kinect does not support near mode
						args.NewSensor.DepthStream.Range = DepthRange.Default;
						args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
						error = true;
					}
				} catch (InvalidOperationException) { error = true; }
			} else {
				error = true;
			}

			if (!error) {
				this.kinectRegion.KinectSensor = systemStatusCollection.CurrentKinectSensor = args.NewSensor;
				systemStatusCollection.IsKinectAllSet = true;
			} else {
				this.kinectRegion.KinectSensor = systemStatusCollection.CurrentKinectSensor = null;
				systemStatusCollection.IsKinectAllSet = false;
			}
		}
	}
}
