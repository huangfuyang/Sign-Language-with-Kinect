using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                Instance = GetInstance("localhost", 8888);
            }
            return Instance;
        }
        private SocketManager(string addr, int port)
        {
            client = new TcpClient(addr, port);
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
                //msg += "#&";
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
                var lengthData = BitConverter.GetBytes(imageData.Length);
                ns.Write(lengthData, 0, lengthData.Length);
                Console.WriteLine(imageData);
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
