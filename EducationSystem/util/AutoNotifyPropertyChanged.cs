using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EducationSystem
{
    public abstract class AutoNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Checks if a property already matches a desired value. Notifies listeners only when necessary.
        protected bool SetProperty<T>(ref T storage, T value, Boolean flush, [CallerMemberName] String propertyName = null)
        {
            if (!flush && object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
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
