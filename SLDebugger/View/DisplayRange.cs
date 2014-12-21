using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.Debugger
{
    public class DisplayRange
    {
        public double Start { get; set; }
        public double End { get; set; }

        public DisplayRange(double start, double end)
        {
            Start = start;
            End = end;
        }
    }
}
