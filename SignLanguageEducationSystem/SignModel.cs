using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace SignLanguageEducationSystem
{
    [Serializable]
    public class SignModel
    {
        private List<Skeleton> _skeletons;
        private string name;

        public SignModel()
        {

        }

        public List<Skeleton> Skeletons
        {
            get { return _skeletons; }
            set { _skeletons = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
