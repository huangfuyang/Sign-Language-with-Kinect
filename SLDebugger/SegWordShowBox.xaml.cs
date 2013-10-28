using System;
using System.Collections.Generic;
using System.ComponentModel;
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



        private Dictionary<int, Line> _splitLineDic;

        public Dictionary<int,Line> SplitLineDic
        {
            get { return _splitLineDic; }
            set { _splitLineDic = value; }
        }

        private int lastEnd;

        public SegWordShowBox()
        {
            InitializeComponent();
            SplitLineDic = new Dictionary<int, Line>();
            Length = 100;
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
            lb.SetValue(Grid.ColumnProperty, index);
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
            grd_main.Children.RemoveRange(0, grd_main.Children.Count);
            grd_Lines.Children.RemoveRange(0, grd_Lines.Children.Count);
            SplitLineDic.Clear();
        }

        public void AddSplitLine(int X)
        {
            Line newLine = new Line();
            newLine.Stroke = System.Windows.Media.Brushes.Black;
            newLine.X1 = grd_Lines.ActualWidth * (double)X / (double)Length;
            newLine.Y1 = 0;
            newLine.X2 = grd_Lines.ActualWidth * (double)X / (double)Length;
            newLine.Y2 = 100;
            newLine.StrokeThickness = 2;
            SplitLineDic.Add(X, newLine);
            grd_Lines.Children.Add(newLine);
        }



        private void grd_Lines_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (KeyValuePair<int, Line> item in SplitLineDic)
            {
                item.Value.X1 = grd_Lines.ActualWidth * (double)item.Key / (double)Length;
                item.Value.X2 = grd_Lines.ActualWidth * (double)item.Key / (double)Length;
            }
        }
    }
}
