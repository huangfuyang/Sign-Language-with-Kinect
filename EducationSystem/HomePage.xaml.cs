using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Kinect.Toolkit.Controls;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

        }

        private void btnGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("SignNumGame/GameSignNumPlayPage.xaml", UriKind.Relative));
        }

        private void btnLearn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("ShowFeatureMatchedPage.xaml", UriKind.Relative));
        }

        private void btnRecognition_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HandDetector;component/HandDetectorPage.xaml", UriKind.Relative));
        }

        private void btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                KinectTileButton btn = sender as KinectTileButton;
                var index = Convert.ToInt32(btn.Name.Split('_').Last());
                this.NavigationService.Navigate(new ShowFeatureMatchedPage(index));
            }
            catch (Exception)
            {
                MessageBox.Show("Error");
            }
        }
    }
}
