using System.IO;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Kinect.Toolkit.Controls;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for GameSelectionPage.xaml
    /// </summary>
    public partial class GameSelectionPage : Page
    {

        private const string GAME_INTRODUCTION_FILE_PATH = "Data/XML/gameIntroduction.xml";

        public GameSelectionPage()
        {
            InitializeComponent();
            LoadGameInformation();
        }

        private void LoadGameInformation()
        {
            XmlDocument xmldoc = new XmlDocument();
            FileStream fs = new FileStream(GAME_INTRODUCTION_FILE_PATH, FileMode.Open, FileAccess.Read);

            xmldoc.Load(fs);

            ItemCollection models = GamePanel.Items;
            foreach (XmlNode gameInfo in xmldoc.GetElementsByTagName("GameInformation"))
            {
                models.Add(new GameInformationModel(gameInfo));
            }
        }

        void GameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GameInformationModel gameInformationModel = (sender as KinectTileButton).Tag as GameInformationModel;
            GameIntroductionPage gameIntroductionPage = new GameIntroductionPage(gameInformationModel);
            this.NavigationService.Navigate(gameIntroductionPage);
        }
    }
}
