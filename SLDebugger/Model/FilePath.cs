using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.Debugger
{
    class FilePath
    {
        private static string dataFile1Postfix = "ac_left_right.txt";
        public static string DataFile1Postfix
        {
            get { return FilePath.dataFile1Postfix; }
            set { FilePath.dataFile1Postfix = value; }
        }

        private static string dataFile2Postfix = "vo_left_right.txt";
        public static string DataFile2Postfix
        {
            get { return FilePath.dataFile2Postfix; }
            set { FilePath.dataFile2Postfix = value; }
        }

        private static string dataFile3Postfix = "";
        public static string DataFile3Postfix
        {
            get { return FilePath.dataFile3Postfix; }
            set { FilePath.dataFile3Postfix = value; }
        }


        private static string segmentFilePostfix = "output.txt";
        public static string SegmentFilePostfix
        {
            get { return FilePath.segmentFilePostfix; }
            set { FilePath.segmentFilePostfix = value; }
        }

        private static string videoTimestampFilePostfix = "timestamp.txt";
        public static string VideoTimestampFilePostfix
        {
            get { return FilePath.videoTimestampFilePostfix; }
            set { FilePath.videoTimestampFilePostfix = value; }
        }
    }
}
