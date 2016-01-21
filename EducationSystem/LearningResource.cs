using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CURELab.SignLanguage.HandDetector;
using Point = System.Windows.Point;

namespace EducationSystem
{
    public class LearningResource
    {
        private static LearningResource _singleton;
        public Dictionary<string, Tuple<string,string>> fullWordList;
        public static LearningResource GetSingleton()
        {
            if (_singleton == null)
            {
                _singleton = new LearningResource();
            }
            return _singleton;
        }


        private void LoadVocab()
        {
            //load wordlist
            fullWordList = new Dictionary<string, Tuple<string,string>>();
            using (var wl = File.Open("Data\\wordlist.txt", FileMode.Open))
            {
                using (StreamReader sw = new StreamReader(wl))
                {
                    var line = sw.ReadLine();
                    while (!String.IsNullOrEmpty(line))
                    {
                        var t = line.Split();
                        fullWordList.Add(t[1], new Tuple<string, string>(t[3],t[4]));
                        line = sw.ReadLine();
                    }
                    sw.Close();
                }
                wl.Close();
            }
            //load key frames
            //string frame_text = "Data\\Education.txt";
            //string frame_text = "Data\\data.json";
            string frame_text = "Data\\education_fy.txt";
            if (File.Exists(frame_text))
            {
                frame_text = File.ReadAllText(frame_text);
            }
            var js = JsonConvert.DeserializeObject(frame_text) as JObject;

            // load videos
            string path = @"videos\";
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                try
                {
                    var fname = file.Split('\\').Last();
                    fname = fname.Substring(0, fname.Length - 6);
                    JToken item;
                    if (js.TryGetValue(fname, out item))
                    {
                        VideoModel model = new VideoModel()
                        {
                            ID = fname.Split()[0],
                            Name = fullWordList[fname.Split()[0]].Item2,
                            Chinese = fullWordList[fname.Split()[0]].Item1,
                            Path = file

                        };
                        VideoModels.Add(model);
                        model.KeyFrames.Add(new KeyFrame()
                        {
                            FrameNumber = 0,
                            RightPosition = new Point(370, 380),
                            LeftPosition = new Point(260,370),
                            Type = HandEnum.None
                        });
                        foreach (var frame in item)
                        {
                            var framenumber = (int) frame["frame"];
                            model.KeyFrames.Add(new KeyFrame()
                            {
                                FrameNumber = framenumber,
                                RightPosition = new Point((int)frame["pos"][0], (int)frame["pos"][1]),
                                LeftPosition = new Point((int)frame["pos"][2], (int)frame["pos"][3]),
                                RightPositionRel = new Point((double)frame["pos"][4], (double)frame["pos"][5]),
                                LeftPositionRel = new Point((double)frame["pos"][6], (double)frame["pos"][7]),
                                Type = frame["type"].ToObject<HandEnum>()
                            });
                            //load key images
                            var rightimages = Directory.GetFiles(
                                    String.Format(@"images\{0}\handshape", fname), framenumber + "*.jpg");
                            var leftimages = Directory.GetFiles(
                                    String.Format(@"images\{0}\handshape\left", fname), framenumber + "*.jpg");
                            // try right image
                            if (rightimages.Length>0)
                            {
                                var i = new Image<Rgb, byte>(rightimages[0]);
                                model.KeyFrames.Last().RightImage = ImageConverter.ToBitmapSource(i);
                            }
                            // try left image
                            if (leftimages.Length > 0)
                            {
                                var i = new Image<Rgb, byte>(leftimages[0]);
                                model.KeyFrames.Last().LeftImage = ImageConverter.ToBitmapSource(i);
                            }
                        }
                    }

                    //var keyframe =
                    //    Directory.GetFiles(
                    //        String.Format(@"D:\Kinectdata\aaron-michael\image\{0}\handshape", fname), "*#.jpg");
                    //if (keyframe.Length > 0)
                    //{
                    //    model.Images = new string[keyframe.Length];
                    //    for (int j = 0; j < keyframe.Length; j++)
                    //    {
                    //        model.Images[j] = keyframe[j];
                    //    }
                    //}
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public List<VideoModel> VideoModels; 
        private LearningResource()
        {
            VideoModels = new List<VideoModel>();
            LoadVocab();
        }
        public static bool IsPropertyExist(dynamic settings, string name)
        {
            return settings.GetType().GetProperty(name) != null;
        }
    }
}
