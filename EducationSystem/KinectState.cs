using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationSystem
{
    class KinectState : AutoNotifyPropertyChanged
    {
        private static KinectState instance;

        private bool _isKinectAllSet;
        public bool IsKinectAllSet
        {
            get { return _isKinectAllSet; }
            set { SetProperty(ref _isKinectAllSet, value, true); }
        }

        private KinectSensor _currentKinectSensor;
        public KinectSensor CurrentKinectSensor
        {
            get { return _currentKinectSensor; }
            set { SetProperty(ref _currentKinectSensor, value, true); }
        }

        private KinectState() { }
        
        public static KinectState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KinectState();
                }
                return instance;
            }
        }

        public void OnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;

            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Switch back to normal mode if Kinect does not support near mode
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                }
            }
            else
            {
                error = true;
            }

            if (!error)
            {
                CurrentKinectSensor = args.NewSensor;
                IsKinectAllSet = true;
            }
            else
            {
                CurrentKinectSensor = null;
                IsKinectAllSet = false;
            }
        }
    }
}
