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

namespace CURELab.SignLanguage.HandDetector.Pages
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("img/back01.png", UriKind.Relative);
            bi.EndInit();

            ImageBrush b = new ImageBrush(bi);
            b.AlignmentY = 0;
            b.Stretch = Stretch.Fill;
            btnStart.Background = b;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("clicked");
        }
    }
}
