using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Data/Images/back01.png", UriKind.Relative);
            bi.EndInit();

            ImageBrush b = new ImageBrush(bi);
            b.AlignmentY = 0;
            b.Stretch = Stretch.Fill;
            btnStart.Background = b;
        }

        private void btnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("HomePage.xaml", UriKind.Relative));
        }
    }
}
