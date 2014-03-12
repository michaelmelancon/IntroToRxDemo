// Original code taken from Dan Fernandez's BUILD talk: Building Apps for the Kinect for Windows SDK
// http://channel9.msdn.com/Events/TechEd/NorthAmerica/2013/DEV-B305
// Original source: http://video.ch9.ms/sessions/teched/na/2013/DEVB305_BuildingAppsWithKinect.zip
// Modifications by Donna Malayeri
// Additional Mods by Michael Melancon

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Coding4Fun.Toolkit.Controls.Common;
using Microsoft.Kinect;

namespace RxKinect
{
    public partial class MainWindow : Window
    {
        #region Private variables & constructor

        KinectSensor _kinect = null;

        private IDisposable _subscriptions = Disposable.Empty;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (_kinect != null)
            {

                _coorMapper = new CoordinateMapper(_kinect);

                _subscriptions = new CompositeDisposable(
                    SubscribeToColorFrame(_kinect),
                    SubscribeToSkeleton(GetJointsObservable(_kinect)),
                    _kinect);

                _kinect.Start();
                //_kinect.ElevationAngle = 15;
            }
        }

        private IObservable<JointCollection> GetJointsObservable(KinectSensor kinect)
        {
            var skeletonFrames = Observable.FromEventPattern<SkeletonFrameReadyEventArgs>(
                addHandler: h => kinect.SkeletonFrameReady += h,
                removeHandler: h => kinect.SkeletonFrameReady -= h
            );

            kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            kinect.SkeletonStream.Enable(_transformParams);

            var skeletons = skeletonFrames
                .Select(sf =>
                {
                    using (var frame = sf.EventArgs.OpenSkeletonFrame())
                    {
                        if (frame != null)
                        {
                            var sd = new Skeleton[frame.SkeletonArrayLength];
                            frame.CopySkeletonDataTo(sd);
                            return sd;
                        }
                        else return new Skeleton[0];
                    }
                });


            var joints = from sd in skeletons
                         let tracked = sd.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked)
                         where tracked != null
                         select tracked.Joints;


            return joints;
        }

        private IDisposable SubscribeToSkeleton(IObservable<JointCollection> joints)
        {
            var subscriptions = new CompositeDisposable();

            var rightHand = joints.Select(joint => joint[JointType.HandRight]);
            var leftHand = joints.Select(joint => joint[JointType.HandLeft]);

            var rightHandSub =
                rightHand.Subscribe(
                    joint =>
                    {
                        ScalePosition(_rightEllipse, joint); // scale relative to the UI so that user doesn't have to make big movements
                        CheckForNewColor(_rightEllipse);
                    });


            var leftHandSub =
                leftHand.Subscribe(joint => MoveToCameraPosition(_leftEllipse, joint));

            subscriptions.Add(rightHandSub);
            subscriptions.Add(leftHandSub);


            // Detect hand motion left/right
            //
            var moves = 
                (
                    from joint in joints
                    let delta = joint[JointType.HandLeft].Position.X - joint[JointType.ElbowLeft].Position.X
                    where Math.Abs(delta) > 0.05
                    select delta < 0 ? "Left" : "Right")
                .DistinctUntilChanged();

            // Detect hand wave (at least 4 moves in 2 seconds)
            //
            var gestures = moves.Buffer(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(100))
                    .Select(moveBuffer => moveBuffer.Count >= 4 ? "WAVE!" : moveBuffer.LastOrDefault())
                    .DistinctUntilChanged();

            var gesuresSub = gestures.ObserveOnDispatcher()
                .Subscribe(OnGesture);

            subscriptions.Add(gesuresSub);

            return subscriptions;
        }

        private void OnGesture(string move)
        {
            _infoBox.Text = move;
            if (move == "WAVE!")
                HueLightingWrapper.SetHue(_currentColor);
        }

        #region Set up video image from Kinect
        private IDisposable SubscribeToColorFrame(KinectSensor kinect)
        {
            var colorFrames =
               Observable.FromEventPattern<ColorImageFrameReadyEventArgs>(
                     addHandler: h => kinect.ColorFrameReady += h,
                     removeHandler: h => kinect.ColorFrameReady -= h)
               .Select(e => e.EventArgs);

            kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            return colorFrames.Subscribe(CopyColorFrame);
        }
        #endregion

        #region Cleanup
        private void Window_Closed(object sender, EventArgs e)
        {
            _subscriptions.Dispose();
            HueLightingWrapper.TurnOff();
        }
        #endregion
    }
}
