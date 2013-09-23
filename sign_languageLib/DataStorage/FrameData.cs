using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LearningSystem.StaticTools;

namespace LearningSystem.DataStorage
{
    public class FrameData
    {
        private List<Person> _persons;
        public List<Person> m_persons
        {
            get { return _persons; }
            set { _persons = value; }
        }
        public Person m_Player1
        {
            get { return _persons[0]; }
            private set { _persons[0] = value; }
        }

        private int _frameNumber;
        public int m_frameNumer
        {
            get { return _frameNumber; }
            private set { _frameNumber = value; }
        }

        public FrameData(int number)
        {
            m_frameNumer = number;
            m_persons = new List<Person>(StaticParams.MAX_NUMBER_OF_PLAYERS);
            m_persons.Add(new Person());
        }
    }
}
