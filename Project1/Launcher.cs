using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Project1
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello world");
            LearningController lc = new LearningController();
            lc.Begin();
            Console.Read();
        }
    }
}
