using System;
using System.Reflection;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

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

        public void StopKinect()
        {
            if (CurrentKinectSensor == null)
            {
                return;
            }

            // Detach event handlers
            ClearEventInvocations(CurrentKinectSensor, "AllFramesReady");
            ClearEventInvocations(CurrentKinectSensor, "SkeletonFrameReady");
            ClearEventInvocations(CurrentKinectSensor, "DepthFrameReady");
            ClearEventInvocations(CurrentKinectSensor, "ColorFrameReady");

            CurrentKinectSensor.Stop();
        }

        private void ClearEventInvocations(object obj, string eventName)
        {
            var fi = GetEventField(obj.GetType(), eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private FieldInfo GetEventField(Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }
            return field;
        }
    }
}
