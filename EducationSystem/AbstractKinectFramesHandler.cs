using System.Threading;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;

namespace EducationSystem
{
    abstract class AbstractKinectFramesHandler : AutoNotifyPropertyChanged
    {
        public virtual void SkeletonFrameCallback(long timestamp, int frameNumber, Skeleton[] skeletonData) { }
        public virtual void DepthFrameCallback(long timestamp, int frameNumber, DepthImagePixel[] depthPixels) { }
        public virtual void ColorFrameCallback(long timestamp, int frameNumber, byte[] colorPixels) { }
        public virtual void HandPointersCallback(long timestamp, HandPointer[] handPointers) { }
        public virtual void OnHandPointerGripRelease(HandPointer grippedHandpointer, Point startGripPoint, Point endGripPoint) { }

        private bool isRegisterAllFrameReady;

        private enum GripState { Released, Gripped }
        private GripState lastGripStatus;
        private Point gripPoint;
        private UIElement element;
        private HandPointer capturedHandPointer;
        private HandPointer grippedHandpointer;

        protected KinectSensor sensor;
        public AbstractKinectFramesHandler(bool isRegisterAllFrameReady = true)
            : this(KinectState.Instance.KinectRegion, isRegisterAllFrameReady)
        {

        }

        public AbstractKinectFramesHandler(UIElement element, bool isRegisterAllFrameReady = true)
        {
            this.lastGripStatus = GripState.Released;
            this.element = element;
            this.isRegisterAllFrameReady = isRegisterAllFrameReady;
        }

        public void UnregisterCallbacks(KinectSensor sensor)
        {
            sensor.AllFramesReady -= sensor_AllFramesReady;
            sensor.SkeletonFrameReady -= sensor_SkeletonFrameReady;
            sensor.DepthFrameReady -= sensor_DepthFrameReady;
            sensor.ColorFrameReady -= sensor_ColorFrameReady;
        }

        public void RegisterCallbackToSensor(KinectSensor sensor)
        {
            UnregisterCallbacks(sensor);
            this.sensor = sensor;
            if (isRegisterAllFrameReady)
            {
                sensor.AllFramesReady += sensor_AllFramesReady;
            }
            else
            {
                sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
                sensor.DepthFrameReady += sensor_DepthFrameReady;
                sensor.ColorFrameReady += sensor_ColorFrameReady;
            }

            KinectRegion.AddHandPointerGotCaptureHandler(element, this.OnHandPointerCaptured);
            KinectRegion.AddHandPointerLostCaptureHandler(element, this.OnHandPointerLostCapture);
            KinectRegion.AddHandPointerEnterHandler(element, this.OnHandPointerEnter);
            KinectRegion.AddHandPointerMoveHandler(element, this.OnHandPointerMove);
            KinectRegion.AddHandPointerPressHandler(element, this.OnHandPointerPress);
            KinectRegion.AddHandPointerGripHandler(element, this.OnHandPointerGrip);
            KinectRegion.AddHandPointerGripReleaseHandler(element, this._onHandPointerGripRelease);
            KinectRegion.AddQueryInteractionStatusHandler(element, this.OnQueryInteractionStatus);
            KinectRegion.SetIsGripTarget(element, true);
            KinectState.Instance.KinectRegion.HandPointersUpdated += KinectRegion_HandPointersUpdated;
        }

        private void HandleHandPointerGrip(HandPointer handPointer)
        {
            if (handPointer == null)
            {
                return;
            }

            if (this.capturedHandPointer != handPointer)
            {
                if (handPointer.Captured == null)
                {
                    handPointer.Capture(element);
                }
                else
                {
                    return;
                }
            }

            this.lastGripStatus = GripState.Gripped;
            this.gripPoint = handPointer.GetPosition(Application.Current.MainWindow);
        }

        private void OnHandPointerCaptured(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer != null)
            {
                this.capturedHandPointer.Capture(null);
            }
            this.capturedHandPointer = kinectHandPointerEventArgs.HandPointer;
            kinectHandPointerEventArgs.Handled = true;
        }

        private void OnHandPointerLostCapture(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (this.capturedHandPointer == kinectHandPointerEventArgs.HandPointer)
            {
                this.capturedHandPointer.Capture(null);
                this.capturedHandPointer = null;
                this.lastGripStatus = GripState.Released;
                kinectHandPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerEnter(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (kinectHandPointerEventArgs.HandPointer.IsPrimaryHandOfUser && kinectHandPointerEventArgs.HandPointer.IsPrimaryUser)
            {
                kinectHandPointerEventArgs.Handled = true;
                if (this.grippedHandpointer == kinectHandPointerEventArgs.HandPointer)
                {
                    this.HandleHandPointerGrip(kinectHandPointerEventArgs.HandPointer);
                    this.grippedHandpointer = null;
                }
            }
        }

        private void OnHandPointerMove(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (element.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                kinectHandPointerEventArgs.Handled = true;

                if (this.lastGripStatus == GripState.Released)
                {
                    return;
                }

                if (!kinectHandPointerEventArgs.HandPointer.IsInteractive)
                {
                    this.lastGripStatus = GripState.Released;
                }
            }
        }

        private void OnHandPointerPress(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (element.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                kinectHandPointerEventArgs.Handled = true;
            }
        }

        private void OnHandPointerGrip(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (kinectHandPointerEventArgs.HandPointer.IsPrimaryUser
                && kinectHandPointerEventArgs.HandPointer.IsPrimaryHandOfUser
                && kinectHandPointerEventArgs.HandPointer.IsInteractive)
            {
                this.HandleHandPointerGrip(kinectHandPointerEventArgs.HandPointer);
                kinectHandPointerEventArgs.Handled = true;
            }
        }

        private void _onHandPointerGripRelease(object sender, HandPointerEventArgs kinectHandPointerEventArgs)
        {
            if (element.Equals(kinectHandPointerEventArgs.HandPointer.Captured))
            {
                Point newHandPosition = kinectHandPointerEventArgs.HandPointer.GetPosition(Application.Current.MainWindow);
                double windowWidth = Application.Current.MainWindow.ActualWidth;
                double windowHeight = Application.Current.MainWindow.ActualHeight;

                kinectHandPointerEventArgs.Handled = true;
                kinectHandPointerEventArgs.HandPointer.Capture(null);
                this.lastGripStatus = GripState.Released;

                if (newHandPosition.X >= 0 && newHandPosition.Y >= 0 && newHandPosition.X < windowWidth && newHandPosition.Y < windowHeight)
                {
                    OnHandPointerGripRelease(kinectHandPointerEventArgs.HandPointer, gripPoint, newHandPosition);
                }
            }
        }

        private void OnQueryInteractionStatus(object sender, QueryInteractionStatusEventArgs queryInteractionStatusEventArgs)
        {
            if (element.Equals(queryInteractionStatusEventArgs.HandPointer.Captured))
            {
                queryInteractionStatusEventArgs.IsInGripInteraction = this.lastGripStatus == GripState.Gripped;
                queryInteractionStatusEventArgs.Handled = true;
            }
        }

        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            handleSkeletonFrame(e.OpenSkeletonFrame());
        }

        private void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            handleDepthImageFrame(e.OpenDepthImageFrame());
        }

        private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            handleColorImageFrame(e.OpenColorImageFrame());
        }

        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            handleSkeletonFrame(e.OpenSkeletonFrame());
            handleDepthImageFrame(e.OpenDepthImageFrame());
            handleColorImageFrame(e.OpenColorImageFrame());
        }

        private void KinectRegion_HandPointersUpdated(object sender, System.EventArgs e)
        {
            HandPointer[] handPointers = new HandPointer[KinectState.Instance.KinectRegion.HandPointers.Count];
            KinectState.Instance.KinectRegion.HandPointers.CopyTo(handPointers, 0);
            long timestampOfLastUpdate = long.MinValue;

            foreach (HandPointer handPointer in handPointers)
            {
                timestampOfLastUpdate = System.Math.Max(handPointer.TimestampOfLastUpdate, timestampOfLastUpdate);
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(o => HandPointersCallback(timestampOfLastUpdate, handPointers)));
        }

        private void handleSkeletonFrame(SkeletonFrame skeletonFrame)
        {
            using (skeletonFrame)
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => SkeletonFrameCallback(skeletonFrame.Timestamp, skeletonFrame.FrameNumber, skeletonData)));
                }
            }
        }


        private void handleDepthImageFrame(DepthImageFrame depthFrame)
        {
            using (depthFrame)
            {
                if (depthFrame != null)
                {
                    DepthImagePixel[] depthPixels = new DepthImagePixel[depthFrame.PixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => DepthFrameCallback(depthFrame.Timestamp, depthFrame.FrameNumber, depthPixels)));
                }
            }
        }

        private void handleColorImageFrame(ColorImageFrame colorFrame)
        {
            using (colorFrame)
            {
                if (colorFrame != null)
                {
                    byte[] colorPixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(colorPixels);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(o => ColorFrameCallback(colorFrame.Timestamp, colorFrame.FrameNumber, colorPixels)));
                }
            }
        }

        protected System.Drawing.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Drawing.Point(depthPoint.X, depthPoint.Y);
        }
    }
}
