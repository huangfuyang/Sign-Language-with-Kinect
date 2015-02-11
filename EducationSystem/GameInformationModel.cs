
using System.Xml;
namespace EducationSystem
{
    public class GameInformationModel
    {
        private string _title;
        public string Title
        {
            get { return _title; }
        }

        private string _shortDescription;
        public string ShortDescription
        {
            get { return _shortDescription; }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
        }

        private string _gamePageType;
        public string GamePageType
        {
            get { return _gamePageType; }
        }

        public GameInformationModel(XmlNode source)
        {
            _title = source["Title"].InnerText;
            _shortDescription = source["ShortDescription"].InnerText;
            _description = source["Description"].InnerText;
            _gamePageType = source["GamePageType"].InnerText;
        }
    }
}
