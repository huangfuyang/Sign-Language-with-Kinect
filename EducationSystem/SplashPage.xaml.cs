using System;
using System.Windows.Controls;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for SplashPage.xaml
    /// </summary>
    public partial class SplashPage : Page
    {
        public SplashPage()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("HomePage.xaml", UriKind.Relative));
        }
    }
}
