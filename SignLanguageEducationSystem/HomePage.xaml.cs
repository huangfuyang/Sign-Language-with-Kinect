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
using Microsoft.Kinect.Toolkit.Controls;

namespace SignLanguageEducationSystem {
	/// <summary>
	/// Interaction logic for HomePage.xaml
	/// </summary>
	public partial class HomePage : UserControl {

		private SignBrowserPage signBrowserPage;

		public HomePage(SystemStatusCollection systemStatusCollection) {
			InitializeComponent();
			this.DataContext = systemStatusCollection;
			KinectRegion.AddHandPointerEnterHandler(btnLearn, btnLearn_Enter);
			KinectRegion.AddHandPointerEnterHandler(btnWatchVideo, btnWatchVideo_Enter);
			KinectRegion.AddHandPointerLeaveHandler(btnLearn, btnLearn_Leave);
			KinectRegion.AddHandPointerLeaveHandler(btnWatchVideo, btnWatchVideo_Leave);
		}

		private void btnLearn_Enter(object sender, RoutedEventArgs e) {
			txtDescription.Text = "Learning Sign Language";
		}

		private void btnWatchVideo_Enter(object sender, RoutedEventArgs e) {
			txtDescription.Text = "Watch Video";
		}

		private void btnLearn_Click(object sender, RoutedEventArgs e) {
			if (signBrowserPage == null) {
				signBrowserPage = new SignBrowserPage((SystemStatusCollection) this.DataContext);
			}

			UIElementCollection children = ((Panel)this.Parent).Children;
			if (!children.Contains(signBrowserPage)) {
				children.Add(signBrowserPage);
			}
		}

		private void btnWatchVideo_Click(object sender, RoutedEventArgs e) {
		}

		private void btnLearn_Leave(object sender, RoutedEventArgs e) {
			txtDescription.Text = "";
		}

		private void btnWatchVideo_Leave(object sender, RoutedEventArgs e) {
			txtDescription.Text = "";
		}
	}
}
