using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

using CURELab.SignLanguage.RecognitionSystem.StaticTools;
using CURELab.SignLanguage.RecognitionSystem.DataStorage;

namespace CURELab.SignLanguage.RecognitionSystem
{
    /// <remarks>
    /// Interact with Realtime system module
    /// 
    /// </remarks>
    public class DataProcessor : ISubject
    {
        public DataWarehouse m_dataWarehouse;
        public DataProcessor()
        {
            m_dataWarehouse = new DataWarehouse();

        }

        public void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (m_dataWarehouse.SetSkeletonFrameData(skeletonFrame))
                    {
                        //notify modules with updated data.
                        NotifyAll(new DataTransferEventArgs(m_dataWarehouse.m_currentFrame));
                    }

                }
            }
        }



        #region ISubject 成员

        public event DataTransferEventHandler m_dataTransferEvent;

        public void NotifyAll(DataTransferEventArgs e)
        {
            if (m_dataTransferEvent != null)
            {
                m_dataTransferEvent(this, e);
            }
            else
            {
                Console.WriteLine("no boundler");
            }
        }

        #endregion
    } 
}

