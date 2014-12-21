using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CURELab.SignLanguage.HandDetector
{
    public delegate void DataTransferEventHandler(Object sender, DataTransferEventArgs args);

    public interface ISubject
    {
        event DataTransferEventHandler m_dataTransferEvent;
        void NotifyAll(DataTransferEventArgs e);

    }
}
