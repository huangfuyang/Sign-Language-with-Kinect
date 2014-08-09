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

namespace SignLanguageEducationSystem {
	/// <summary>
	/// Interaction logic for SignWordPage.xaml
	/// </summary>
	public partial class SignWordPage : UserControl {
		private byte[] _colorPixels;
		private bool isPlayed;

		public SignWordPage(SystemStatusCollection systemStatusCollection) {
			InitializeComponent();
			this.DataContext = systemStatusCollection;

			systemStatusCollection.ColorBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);

			systemStatusCollection.CurrentKinectSensor.ColorStream.Enable();
			_colorPixels = new byte[systemStatusCollection.CurrentKinectSensor.ColorStream.FramePixelDataLength];
			systemStatusCollection.CurrentKinectSensor.ColorFrameReady += CurrentKinectSensor_ColorFrameReady;
		}

		private void CurrentKinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
				if (colorFrame != null) {
					WriteableBitmap ColorBitmap = ((SystemStatusCollection)this.DataContext).ColorBitmap;

					colorFrame.CopyPixelDataTo(this._colorPixels);
					((SystemStatusCollection)this.DataContext).ColorBitmap.Lock();
					ColorBitmap.WritePixels(
						new System.Windows.Int32Rect(0, 0, ColorBitmap.PixelWidth, ColorBitmap.PixelHeight),
						_colorPixels,
						ColorBitmap.PixelWidth * sizeof(int),
						0);
					ColorBitmap.Unlock();
				}
			}
		}

		private void btnBack_Click(object sender, RoutedEventArgs e) {
			if (this.Parent != null) {
				UIElementCollection children = ((Panel)this.Parent).Children;
				if (children.Contains(this)) {
					children.Remove(this);
				}
			}
		}

		private void videoPlayer_Loaded(object sender, RoutedEventArgs e) {
			isPlayed = false;
			WaitingImage.Visibility = Visibility.Visible;
			videoPlayer.Play();
		}

		private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e) {
			isPlayed = true;
			WaitingImage.Visibility = Visibility.Hidden;
		}
	}
}
