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

namespace SignLanguageEducationSystem {
	/// <summary>
	/// Interaction logic for StartPage.xaml
	/// </summary>
	public partial class StartPage : UserControl {

		private HomePage homePage;

		public StartPage(SystemStatusCollection systemStatusCollection) {
			InitializeComponent();
			this.DataContext = systemStatusCollection;

			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.UriSource = new Uri("Data/Images/back01.png", UriKind.Relative);
			bi.EndInit();

			ImageBrush b = new ImageBrush(bi);
			b.AlignmentY = 0;
			b.Stretch = Stretch.UniformToFill;
			btnStart.Background = b;
		}

		private void KinectTileButton_Click(object sender, RoutedEventArgs e) {
			if (homePage == null) {
				homePage = new HomePage((SystemStatusCollection)this.DataContext);
			}

			UIElementCollection children = ((Panel)this.Parent).Children;
			if (!children.Contains(homePage)) {
				children.Add(homePage);
			}
		}
	}
}
