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
using CURELab.SignLanguage.Debugger.Model;

namespace CURELab.SignLanguage.Debugger
{
    /// <summary>
    /// SegWordShowBox.xaml 的交互逻辑
    /// </summary>
    public partial class SegWordShowBox : UserControl
    {
        private int _length;
        public int Length
        {
            get { return _length; }

            set 
            { 
                _length = value;
                lb_time2.Content = (value / 2).ToString();
                lb_time3.Content = value.ToString();
            }
        }

        private int lastEnd;

        public SegWordShowBox()
        {
            InitializeComponent();
            lastEnd = 0;

        }

        private void AddLabel(int length, string content, Brush back)
        {
            grd_main.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(Math.Abs(length), GridUnitType.Star)
            });
            Label lb = new Label();
            lb.Content = content;
            lb.Background = back;
            lb.Foreground = Brushes.Black;
            lb.FontSize = 12;
            lb.HorizontalContentAlignment = HorizontalAlignment.Center;
            lb.VerticalContentAlignment = VerticalAlignment.Center;
            int index = grd_main.Children.Add(lb);
            lb.SetValue(Grid.ColumnProperty, index - 1);
        }
        public void AddNewWord(string word, int start, int end)
        {
            if (end > Length)
            {
                Length = end;
            }
            //label ME
            if (lastEnd ==0)
            {
                AddLabel(start - lastEnd, "start", Brushes.White);
            }
            else
            {
                AddLabel(start - lastEnd, "me", Brushes.White);
            }
            //label word
            AddLabel(end - start, word, Brushes.SteelBlue);
            lastEnd = end;

        }

        public void AddWords(SegmentedWordCollection words)
        {
            foreach (var item in words)
            {
                AddNewWord(item.Word, item.StartTime, item.EndTime);
            }
            AddLabel(Length - words.Last().EndTime, "end", Brushes.White);
            

        }
        public void RemoveAll()
        {
            Length = 100;
            lastEnd = 0;
            grd_main.ColumnDefinitions.Clear();
            grd_main.Children.RemoveRange(1, grd_main.Children.Count - 1);
        }

    }
}
