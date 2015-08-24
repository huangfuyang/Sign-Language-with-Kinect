using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for GameIntroductionPage.xaml
    /// </summary>
    public partial class GameIntroductionPage : Page
    {

        private string _gameDescriptionImagePath = "Data/Images/back01.png";
        public BitmapImage GameDescriptionImage
        {
            get
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(_gameDescriptionImagePath, UriKind.Relative);
                bi.EndInit();
                return bi;
            }
        }

        public GameIntroductionPage(GameInformationModel informationModel)
        {
            this.DataContext = informationModel;
            _gameDescriptionImagePath = "Images/Signing.png";
            InitializeComponent();
        }

        private void btnStartGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string gamePlayTypeString = String.Format("{0}.{1}", this.GetType().Namespace, ((GameInformationModel)this.DataContext).GamePageType);
            Page gamePlayPage = (Page)Activator.CreateInstance(Type.GetType(gamePlayTypeString));
            this.NavigationService.Navigate(gamePlayPage);
        }
    }
}
