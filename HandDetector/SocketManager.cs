using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CURELab.SignLanguage.HandDetector
{
    public class SocketManager
    {
        private static SocketManager Instance;
        private TcpClient client;
        private NetworkStream ns;
        private string[] SPLIT = { "#&" };
        public static SocketManager GetInstance(string addr, int port)
        {
            if (Instance == null)
            {
                Instance = new SocketManager(addr,port);
            }
            return Instance;
        }

        public static SocketManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = GetInstance("137.189.89.29", 8888);
            }
            return Instance;
        }
        private SocketManager(string addr, int port)
        {
            client = new TcpClient();
            IPAddress ipa = IPAddress.Parse(addr);
            IPEndPoint ipe = new IPEndPoint(ipa, port);

            Console.WriteLine("connecting");
            client.Connect(ipe);

            if (client.Connected)
            {   
                Console.WriteLine("connected");
                ns = client.GetStream();
            }
        
        }

        public string GetResponse(string msg)
        {
            if (ns != null)
            {
                msg += "#&";
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
                ns.Write(data, 0, data.Length);
                // Buffer to store the response bytes.
                if (ns.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();
                    int numberOfBytesRead = 0;

                    // Incoming message may be larger than the buffer size. 
                    do
                    {
                        numberOfBytesRead = ns.Read(myReadBuffer, 0, myReadBuffer.Length);
                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                    }
                    while (ns.DataAvailable);
                    //return myCompleteMessage.ToString().Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                    return myCompleteMessage.ToString();
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }

            }
            return null;

        }
        public delegate string AsyncBitmapCaller(Bitmap bmp);
        public delegate string AsyncMsgCaller(string bmp);

        public void GetResponseAsync(Bitmap img, AsyncCallback callback)
        {
            var ac = new AsyncBitmapCaller(SendData);
            ac.BeginInvoke(img, callback, "states");
        }

        public void GetResponseAsync(String msg, AsyncCallback callback)
        {
            var ac = new AsyncMsgCaller(GetResponse);
            ac.BeginInvoke(msg, callback, "states");
        }

        public string SendData(Bitmap img)
        {
            if (ns != null)
            {
                //msg += "#&";
                byte[] imageData;
                using (var stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Jpeg);
                    imageData = stream.ToArray();
                }
                StreamWriter sw = new StreamWriter(ns);
                var lengthData = BitConverter.GetBytes(imageData.Length);
                // ns.Write(lengthData, 0, lengthData.Length);
                ns.Write(imageData, 0, imageData.Length);
                sw.Write("SPLIT");
                sw.Flush();
            }
            return "TODO";
        }
        public string GetResponse(Bitmap img)
        {
            if (ns != null)
            {
                //msg += "#&";

                byte[] imageData;
                using (var stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Jpeg);
                    imageData = stream.ToArray();
                }
                StreamWriter sw = new StreamWriter(ns);
                var lengthData = BitConverter.GetBytes(imageData.Length);
                // ns.Write(lengthData, 0, lengthData.Length);
                ns.Write(imageData, 0, imageData.Length);
                sw.Write("SPLIT");
                sw.Flush();
                // Buffer to store the response bytes.
                if (ns.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();
                    int numberOfBytesRead = 0;
                    int total = 0;
                    // Incoming message may be larger than the buffer size. 
                    do
                    {
                        numberOfBytesRead = ns.Read(myReadBuffer, 0, myReadBuffer.Length);
                        total += numberOfBytesRead;
                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                    }
                    while (ns.DataAvailable);
                    Console.WriteLine(total.ToString()+" bytes received");
                    //return myCompleteMessage.ToString().Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
                    return myCompleteMessage.ToString();
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }

            }
            return null;

        }
    }
}
