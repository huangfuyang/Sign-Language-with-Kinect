using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.HandDetector
{
    class VisualData : INotifyPropertyChanged
    {
        private static VisualData Singleton;
        private int currentFrame;
        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = value;
                OnPropertyChanged("CurrentFrame");
            }
        }

        private int _totalFrames;
        public int TotalFrames
        {
            get { return _totalFrames; }
            set
            {
                OnPropertyChanged("TotalFrames");
                _totalFrames = value;
            }
        }

        public static VisualData GetSingleton()
        {
            if (Singleton == null)
            {
                Singleton = new VisualData();
            }
            return Singleton;
        }
        private VisualData()
        {
            CurrentFrame = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Checks if a property already matches a desired value. Notifies listeners only when necessary.
        protected bool SetProperty<T>(ref T storage, T value, Boolean flush, [CallerMemberName] String propertyName = null)
        {
            if (!flush && object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Notifies listeners that a property value has changed.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
