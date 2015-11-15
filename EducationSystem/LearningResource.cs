using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using EducationSystem.SignNumGame;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CURELab.SignLanguage.HandDetector;
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
            string frame_text = "Data\\Education.txt";
            if (File.Exists(frame_text))
            {
                frame_text = File.ReadAllText(frame_text);
            }
            var js = JsonConvert.DeserializeObject(frame_text) as JObject;

            // load videos
            string path = @"D:\Kinectdata\aaron-michael\video\";
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                try
                {
                    var fname = file.Split('\\').Last();
                    fname = fname.Substring(0, fname.Length - 6);
                    JToken item;
                    if (js.TryGetValue(fname.Split()[0], out item))
                    {
                        VideoModel model = new VideoModel()
                        {
                            ID = fname.Split()[0],
                            Name = fullWordList[fname.Split()[0]].Item2,
                            Chinese = fullWordList[fname.Split()[0]].Item1,
                            Path = file

                        };
                        VideoModels.Add(model);
                        foreach (var frame in item)
                        {
                            model.KeyFrames.Add(new KeyFrame()
                            {
                                FrameNumber = (int)frame["frame"],
                                LeftHandShape = "",
                                LeftPosition = new Point((int)frame["pos"][2], (int)frame["pos"][3]),
                                RightPosition = new Point((int)frame["pos"][0], (int)frame["pos"][1]),
                                RightHandShape = "",
                                Type = frame["type"].ToObject<HandEnum>()
                            });
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
