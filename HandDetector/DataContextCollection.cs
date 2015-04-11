using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.HandDetector
{
    public class DataContextCollection : INotifyPropertyChanged
    {
        private static DataContextCollection instance;
        public Dictionary<string, string> fullWordList;

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public static DataContextCollection GetInstance()
        {
            if (instance == null)
            {
                instance = new DataContextCollection();
            }
            return instance;
        }
        private DataContextCollection()
        {
            LoadVocab();
        }

        private void LoadVocab()
        {
            // load word list
            fullWordList = new Dictionary<string, string>();
            using (var wl = File.Open("wordlist.txt", FileMode.Open))
            {
                using (StreamReader sw = new StreamReader(wl))
                {
                    var line = sw.ReadLine();
                    while (!String.IsNullOrEmpty(line))
                    {
                        var t = line.Split();
                        fullWordList.Add(t[1], t[3]);
                        line = sw.ReadLine();
                    }
                    sw.Close();
                }
                wl.Close();
            }
        }

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
