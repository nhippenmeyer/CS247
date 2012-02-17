using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kinect.Toolbox;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;
using System.IO;
using Microsoft.Win32;
using Kinect.Toolbox.Voice;

namespace GesturesViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        Runtime kinectRuntime;
        readonly SwipeGestureDetector swipeGestureRecognizer = new SwipeGestureDetector();
        TemplatedGestureDetector circleGestureRecognizer;
        readonly ColorStreamManager streamManager = new ColorStreamManager();
        SkeletonDisplayManager skeletonDisplayManager;
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        readonly AlgorithmicPostureDetector algorithmicPostureRecognizer = new AlgorithmicPostureDetector();
        TemplatedPostureDetector templatePostureDetector;
        bool recordNextFrameForPosture;

        string circleKBPath;
        string letterT_KBPath;

        SkeletonRecorder recorder;
        SkeletonReplay replay;

        BindableNUICamera nuiCamera;

        VoiceCommander voiceCommander;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectRuntime == null)
                    {
                        kinectRuntime = e.KinectRuntime;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (kinectRuntime == e.KinectRuntime)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectRuntime == e.KinectRuntime)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no more powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            circleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\circleKB.save");
            letterT_KBPath = Path.Combine(Environment.CurrentDirectory, @"data\t_KB.save");

            try
            {
                //listen to any status change for Kinects
                Runtime.Kinects.StatusChanged += Kinects_StatusChanged;

                //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
                foreach (Runtime kinect in Runtime.Kinects)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectRuntime = kinect;
                        break;
                    }
                }

                if (Runtime.Kinects.Count == 0)
                    MessageBox.Show("No Kinect found");
                else
                    Initialize();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Initialize()
        {
            if (kinectRuntime == null)
                return;

            kinectRuntime.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            kinectRuntime.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
            kinectRuntime.VideoFrameReady += kinectRuntime_VideoFrameReady;

            swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;

            skeletonDisplayManager = new SkeletonDisplayManager(kinectRuntime.SkeletonEngine, kinectCanvas);

            kinectRuntime.SkeletonEngine.TransformSmooth = true;
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 1.0f,
                Correction = 0.1f,
                Prediction = 0.1f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f
            };
            kinectRuntime.SkeletonEngine.SmoothParameters = parameters;

            LoadCircleGestureDetector();
            LoadLetterTPostureDetector();

            nuiCamera = new BindableNUICamera(kinectRuntime.NuiCamera);

            elevationSlider.DataContext = nuiCamera;

            voiceCommander = new VoiceCommander("record", "stop");
            voiceCommander.OrderDetected += voiceCommander_OrderDetected;

            StartVoiceCommander();
        }

        void kinectRuntime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            kinectDisplay.Source = streamManager.Update(e);
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (e.SkeletonFrame == null)
                return;

            if (recorder != null)
                recorder.Record(e.SkeletonFrame);

            if (e.SkeletonFrame.Skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked).Count() == 0)
                return;

            ProcessFrame(e.SkeletonFrame);
        }

        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                barycenterHelper.Add(skeleton.Position.ToVector3(), skeleton.TrackingID);

                stabilities.Add(skeleton.TrackingID, barycenterHelper.IsStable(skeleton.TrackingID) ? "Stable" : "Unstable");
                //if (!barycenterHelper.IsStable(skeleton.TrackingID))
                //    continue;

                if (recordNextFrameForPosture)
                {
                    recordNextFrameForPosture = false;
                    templatePostureDetector.AddTemplate(skeleton);
                }

                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.Position.W < 0.8f || joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    if (joint.ID == JointID.HandRight)
                    {
                        swipeGestureRecognizer.Add(joint.Position, kinectRuntime.SkeletonEngine);
                        circleGestureRecognizer.Add(joint.Position, kinectRuntime.SkeletonEngine);
                    }
                }

                algorithmicPostureRecognizer.TrackPostures(skeleton);
                templatePostureDetector.TrackPostures(skeleton);
            }

            skeletonDisplayManager.Draw(frame);

            stabilitiesList.ItemsSource = stabilities;

            currentPosture.Text = "Current posture: " + algorithmicPostureRecognizer.CurrentPosture;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }

        private void Clean()
        {
            swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;

            CloseGestureDetector();

            ClosePostureDetector();

            if (voiceCommander != null)
            {
                voiceCommander.OrderDetected -= voiceCommander_OrderDetected;
                voiceCommander.Dispose();
                voiceCommander = null;
            }

            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }

            if (kinectRuntime != null)
            {
                kinectRuntime.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectRuntime.VideoFrameReady -= kinectRuntime_VideoFrameReady;
                kinectRuntime.Uninitialize();
                kinectRuntime = null;
            }
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };

            if (openFileDialog.ShowDialog() == true)
            {
                if (replay != null)
                {
                    replay.SkeletonFrameReady -= replay_SkeletonFrameReady;
                    replay.Stop();
                }
                Stream recordStream = File.OpenRead(openFileDialog.FileName);

                replay = new SkeletonReplay(recordStream);

                replay.SkeletonFrameReady += replay_SkeletonFrameReady;

                replay.Start();
            }
        }

        void replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
        }
    }
}
