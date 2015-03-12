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
            this.NavigationService.Navigate(new Uri("GameSelectionPage.xaml", UriKind.Relative));
        }

        private void btnLearn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("ShowFeatureMatchedPage.xaml", UriKind.Relative));
        }
    }
}
