using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using EducationSystem.SignNumGame;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

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
            var js = JsonConvert.DeserializeObject<Dictionary<string,JsonData>>(frame_text);
            

            // load videos
            string path = @"D:\Kinectdata\aaron-michael\video\";
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fname = file.Split('\\').Last();
                fname = fname.Substring(0, fname.Length - 6);
                VideoModel model = new VideoModel()
                {
                    ID = fname.Split()[0],
                    Name = fullWordList[fname.Split()[0]].Item2,
                    Chinese = fullWordList[fname.Split()[0]].Item1,
                    Path = fname123

                };
                if (js.ContainsKey(fname.Split()[0]))
                {
                    VideoModels.Add(model);
                    foreach (var jsonData in js[fname.Split()[0]].frames)
                    {
                        model.KeyFrames.Add(new KeyFrame()
                        {
                            //TODO
                            FrameNumber = 0,
                            LeftHandShape = "",
                            LeftPosition = new Point(jsonData.pos[2], jsonData.pos[3]),
                            RightPosition = new Point(jsonData.pos[0], jsonData.pos[1]),
                            RightHandShape = "",
                            Type = jsonData.type
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


        }

        public List<VideoModel> VideoModels; 
        private LearningResource()
        {
            VideoModels = new List<VideoModel>();
            LoadVocab();
        }
    }
}
