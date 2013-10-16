// author：      fyhuang
// created time：2013/10/16 15:37:01
// organizatioin:CURE lab, CUHK
// copyright：   2013-2015
// CLR：         4.0.30319.18052
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



namespace CURELab.SignLanguage.Debugger.Module
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class XMLReader
    {
        private XmlDocument xmlDocument;
        public XMLReader(string path)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
        }

        public string GetFileName(string fn)
        {
            XmlNode node = xmlDocument.SelectSingleNode("configuration");
            node = node.SelectSingleNode("dataFile");
            node = node.SelectSingleNode(fn);
            return node.Attributes[0].Value.ToString();            
        }
    }
}