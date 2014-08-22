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
        private List<double> h_vertical;

        public List<double> H_vertical
        {
            get { return h_vertical; }
            set { h_vertical = value; }
        }
        private List<double> h_horizantal;

        public List<double> H_horizantal
        {
            get { return h_horizantal; }
            set { h_horizantal = value; }
        }

        public SignModel()
        {
            h_vertical = new List<double>();
            h_horizantal = new List<double>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name+':');
            for (int i = 0; i < H_vertical.Count(); i++)
            {
                sb.Append(H_vertical[i].ToString() + ',' + H_horizantal[i].ToString() + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }


        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public static SignModel CreateFromString(string s)
        {
            string name = s.Split(':')[0];
            var sm = new SignModel();
            sm.Name = name;
            string data = s.Split(':')[1];
            string[] datas = data.Split();
            foreach (var item in datas)
            {
                double v = Convert.ToDouble(item.Split(',')[0]);
                double h = Convert.ToDouble(item.Split(',')[1]);
                sm.H_horizantal.Add(h);
                sm.H_vertical.Add(v);
            }
            return sm;
        }
    }
}
