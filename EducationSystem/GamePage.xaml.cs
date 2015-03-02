using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        public readonly DependencyProperty ActiveUserCount = DependencyProperty.Register("ActiveUserCount", typeof(int), typeof(GamePage));

        private GameIntroductionPage gameIntroduction;

        public GamePage(GameIntroductionPage gameIntroduction)
        {
            this.gameIntroduction = gameIntroduction;
            InitializeComponent();

            ActiveUserDetector activeUserDetector = (ActiveUserDetector)this.Resources["ActiveUserDetector"];
            activeUserDetector.RegisterCallbackToSensor(KinectState.Instance.CurrentKinectSensor);
            Binding myBinding = new Binding("ActiveUserCount");
            myBinding.Source = activeUserDetector;
            this.SetBinding(this.ActiveUserCount, myBinding);
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(gameIntroduction);
        }
    }
}
