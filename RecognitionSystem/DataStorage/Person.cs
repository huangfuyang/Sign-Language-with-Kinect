using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace CURELab.SignLanguage.RecognitionSystem.DataStorage
{
    
    public class Person
    {
        private Skeleton _skeleton;

        public Skeleton m_skeleton
        {
            get { return _skeleton; }
            set { _skeleton = value; }
        }

        private Vector3 _position;

        public Vector3 m_position
        {
            get { return _position; }
            set { _position = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Person()
        {
            m_position = new Vector3(0, 0, 0);
          
        }
       
    }
}
