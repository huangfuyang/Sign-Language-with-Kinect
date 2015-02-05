using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace EducationSystem
{
    /// <summary>
    /// Interaction logic for GameIntroductionPage.xaml
    /// </summary>
    public partial class GameIntroductionPage : Page
    {
        private string _gameTitle = "Title";
        public string GameTitle
        {
            get { return _gameTitle; }
            set { _gameTitle = value; }
        }

        private string _gameDescription = "Description";
        public string GameDescription
        {
            get { return _gameDescription; }
            set { _gameDescription = value; }
        }

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

        public GameIntroductionPage(string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            xmldoc.Load(fs);
            GameTitle = xmldoc.GetElementsByTagName("Title")[0].InnerText;
            GameDescription = xmldoc.GetElementsByTagName("Description")[0].InnerText;
            _gameDescriptionImagePath = "Images/Signing.png";
            Console.WriteLine("{0} - {1}", this.Resources["GameTitle"], this.Resources["GameDescription"]);

            InitializeComponent();
        }
    }
}
