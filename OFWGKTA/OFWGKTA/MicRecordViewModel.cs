using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NAudio.Wave;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System.IO;
using Microsoft.Speech.Recognition;
using System.ComponentModel;
using Kinect.Toolbox;


namespace OFWGKTA 
{
    class MicRecordViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "MicRecordViewModel";

        private RelayCommand beginRecordingCommand;
        private RelayCommand stopCommand;
        private RelayCommand rewindCommand;
        private RelayCommand playbackCommand;

        private AudioRecorder recorder;
        private AudioPlayer player;
        
        private int leftPosition;
        private int rightPosition;
        private int totalWaveFormSamples;

        private float lastPeak;
        private string waveFileName;

        private int micIndex;

        private MenuRecognizer menuRecognizerHoriz;
        private MenuRecognizer menuRecognizerVert;

        private ObservableCollection<MenuOption> menuListHoriz = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListHoriz { get { return this.menuListHoriz; } }
        private ObservableCollection<MenuOption> menuListVert = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListVert { get { return this.menuListVert; } }

        protected readonly SwipeGestureDetector swipeGestureRecognizer = new SwipeGestureDetector();

        public MicRecordViewModel()
        {
            this.menuListHoriz.Add(new MenuOption("Play", null, 4));
            this.menuListHoriz.Add(new MenuOption("Rewind", null, 4));
            this.menuListHoriz.Add(new MenuOption("Start Recording", null, 4));
            this.menuListHoriz.Add(new MenuOption("Stop Recording", null, 4));
            this.MenuRecognizerHoriz = new MenuRecognizer(this.MenuListHoriz.Count, 100);

            this.menuListVert.Add(new MenuOption("Play", null, 4));
            this.menuListVert.Add(new MenuOption("Rewind", null, 4));
            this.menuListVert.Add(new MenuOption("Start Recording", null, 4));
            this.menuListVert.Add(new MenuOption("Stop Recording", null, 4));
            this.MenuRecognizerVert = new MenuRecognizer(this.MenuListVert.Count, 100, false);

            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());

            // Recorder instance ould be passed in as a parameter to the constructor
            // this.recorder = recorder;

            // Instead I just instantiate it here:
            this.recorder = new AudioRecorder();
            this.player = new AudioPlayer();

            // set recorder event handlers
            this.recorder.Stopped += new EventHandler(recorder_Stopped);
            this.recorder.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);

            this.beginRecordingCommand = new RelayCommand(() => BeginRecording(),
                () => recorder.RecordingState == RecordingState.Monitoring);
            this.stopCommand = new RelayCommand(() => Stop(),
                () => recorder.RecordingState == RecordingState.Recording);
            this.rewindCommand = new RelayCommand(() => Rewind(),
                () => recorder.RecordingState != RecordingState.Recording && recorder.RecordedTime.Ticks > 0);
            this.playbackCommand = new RelayCommand(() => Playback(),
                () => recorder.RecordingState != RecordingState.Recording && recorder.RecordedTime.Ticks > 0);

            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }

        public void Activated(object state)
        {
            this.Kinect = ((AppState)state).Kinect;
            if (this.Kinect != null)
            {
                this.swipeGestureRecognizer.OnGestureDetected += SwipeDetected;
                this.Kinect.SetSpeechCallback(speechCallback);
                // subscribe to changes in kinect properties
                // allows us to set callbacks at this level when stage status changes 
                // (remember to unsubscribe from this)
                this.Kinect.PropertyChanged += KinectListener; 
                this.Kinect.SkeletonUpdated += new EventHandler<SkeletonEventArgs>(Kinect_SkeletonUpdated);
            }

            this.recorder.SampleAggregator.RaiseRestart();
            this.micIndex = ((AppState)state).MicIndex;

            if (this.recorder.RecordingState != RecordingState.Monitoring)
                BeginMonitoring();
        }

        void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            this.menuRecognizerHoriz.Add(Kinect.HandRight, Kinect.ShoulderCenter, Kinect.ShoulderRight);
            this.menuRecognizerVert.Add(Kinect.HandRight, Kinect.ShoulderCenter, Kinect.ShoulderRight);

            this.swipeGestureRecognizer.Add(e.RightHandPosition, Kinect.KinectRuntime.SkeletonEngine);
        }

        void SwipeDetected(string gesture)
        {
            if (this.Kinect.IsOnStage)
            {
                if (gesture == "SwipeToRight")
                {
                    if (this.recorder.RecordingState != RecordingState.Recording)
                    {
                        this.BeginRecording();
                    }
                }
                else if (gesture == "SwipeToLeft")
                {
                    if (this.recorder.RecordingState == RecordingState.Recording)
                    {
                        this.Stop();
                    }
                    else
                    {
                        if (this.player.PlaybackState == PlaybackState.Playing)
                        {
                            this.player.Stop();
                        }
                        else
                        {
                            this.Playback();
                        }
                    }
                }
            }
        }

        void KinectListener(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "IsOnStage")
            {
                if (!Kinect.IsOnStage)
                {
                    this.Stop();
                }
            }
        }

        void speechCallback(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Kinect.IsOnStage)
            {
                if (e.Result.Text == "record")
                {
                    if (e.Result.Confidence > .88)
                    {
                        if (recorder.RecordingState != RecordingState.Recording && player.PlaybackState == PlaybackState.Stopped)
                        {
                            recorder.SampleAggregator.RaiseRestart();
                            this.BeginRecording();
                        }
                    }
                }
                else if (e.Result.Text == "play" && recorder.RecordingState != RecordingState.Recording)
                {
                    if (e.Result.Confidence > .88)
                    {
                        if (recorder.RecordedTime != TimeSpan.Zero)
                        {
                            this.Playback();
                        }
                    }
                }
                else if (e.Result.Text == "stop" && this.player.PlaybackState == PlaybackState.Playing)
                {
                    if (e.Result.Confidence > .88)
                    {
                        this.player.Stop();
                    }
                }

            }
        }

        private void BeginMonitoring()
        {
            recorder.BeginMonitoring(this.micIndex);
            RaisePropertyChanged("MicrophoneLevel");
        }

        /*
        private TimeSpan PositionToTimeSpan(int position)
        {
            // TODO: WaveFormat is a property on WaveFileReader--NOT recorder
            int samplesPerSecond = this.recorder.WaveFormat.SampleRate;
            int samples = SampleAggregator.NotificationCount * position;
            return TimeSpan.FromSeconds((double)samples / samplesPerSecond);
        }
        */

        /*
        public SampleAggregator SampleAggregator
        {
            get 
            {
                return sampleAggregator;  
            }
            set
            {
                if (sampleAggregator != value)
                {
                    sampleAggregator = value; 
                    RaisePropertyChanged("SampleAggregator");
                }
            }
        }
        */

        public SampleAggregator SampleAggregator
        {
            get
            {
                return recorder.SampleAggregator;
            }
        }
        
        public int TotalWaveFormSamples
        {
            get
            {
                return totalWaveFormSamples;
            }
            set
            {
                if (totalWaveFormSamples != value)
                {
                    totalWaveFormSamples = value;
                    RaisePropertyChanged("TotalWaveFormSamples");
                }
            }
        }

        public int LeftPosition
        {
            get
            {
                return leftPosition;
            }
            set
            {
                if (leftPosition != value)
                {
                    leftPosition = value;
                    RaisePropertyChanged("LeftPosition");
                }
            }
        }

        public int RightPosition
        {
            get
            {
                return rightPosition;
            }
            set
            {
                if (rightPosition != value)
                {
                    rightPosition = value;
                    RaisePropertyChanged("RightPosition");
                }
            }
        }

        void recorder_Stopped(object sender, EventArgs e)
        {
            AudioSaver saver = new AudioSaver(this.waveFileName);
            
            // TODO: allow trimming recording
            //saver.TrimFromStart = PositionToTimeSpan(LeftPosition);
            //saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition);

            // TODO: generate a unique filename dynamically via a private method/property
            // output filename is hardcoded here:
            // string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Guid.NewGuid().ToString() + ".wav");
            saver.SaveFileFormat = SaveFileFormat.Wav;
            saver.SaveAudio(fileName);
            
            this.BeginMonitoring();
        }

        void recorder_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            RaisePropertyChanged("CurrentInputLevel");
            RaisePropertyChanged("RecordedTime");
        }


        public ICommand RewindCommand { get { return rewindCommand; } }
        private void Rewind()
        {
            this.player.CurrentPosition = new System.TimeSpan(0);
        }

        public ICommand PlaybackCommand { get { return playbackCommand; } }
        private void Playback()
        {
            if (this.recorder.RecordedTime != TimeSpan.Zero && this.recorder.RecordingState != RecordingState.Recording)
            {
                if (this.player.PlaybackState == PlaybackState.Stopped)
                {
                    this.player.LoadFile(this.waveFileName);
                    this.player.Play();
                }
            }
        }

        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        private void BeginRecording()
        {
            if (this.player.PlaybackState == PlaybackState.Stopped)
            {
                this.waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
                recorder.BeginRecording(waveFileName);
                recorder.SampleAggregator.RaiseRestart();
                recorder.SampleAggregator.RaiseStart();
                RaisePropertyChanged("MicrophoneLevel");
                //RaisePropertyChanged("ShowWaveForm");
            }
        }

        public ICommand StopCommand { get { return stopCommand; } }
        private void Stop()
        {
            recorder.SampleAggregator.RaiseStop();
            recorder.Stop();
        }
        

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                this.Stop();
            }
        }

        public string RecordedTime
        {
            get
            {
                TimeSpan current = recorder.RecordedTime;
                return String.Format("{0:D2}:{1:D2}.{2:D3}", current.Minutes, current.Seconds, current.Milliseconds);
            }
        }

        public double MicrophoneLevel
        {
            get { return recorder.MicrophoneLevel; }
            set { recorder.MicrophoneLevel = value; }
        }

        /*
        public bool ShowWaveForm
        {
            get { return recorder.RecordingState == RecordingState.Recording || 
                recorder.RecordingState == RecordingState.RequestedStop; }
        }*/

        // multiply by 100 because the Progress bar's default maximum value is 100
        public float CurrentInputLevel { get { return lastPeak * 100; } }


        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        private void ReturnToWelcome()
        {
            this.Stop();
            Kinect.PropertyChanged -= KinectListener; // this listeners for changes in stage status, so we're unsubsribing before we leave
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        public MenuRecognizer MenuRecognizerHoriz
        {
            get { return menuRecognizerHoriz; }
            set
            {
                if (this.menuRecognizerHoriz != value)
                {
                    this.menuRecognizerHoriz = value;
                    RaisePropertyChanged("MenuRecognizerHoriz");
                }
            }
        }

        public MenuRecognizer MenuRecognizerVert
        {
            get { return menuRecognizerVert; }
            set
            {
                if (this.menuRecognizerVert != value)
                {
                    this.menuRecognizerVert = value;
                    RaisePropertyChanged("MenuRecognizerVert");
                }
            }
        }
    }
}
