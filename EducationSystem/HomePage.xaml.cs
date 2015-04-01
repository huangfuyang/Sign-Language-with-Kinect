using System;
using System.Windows.Controls;

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
    }
}
