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
using System.Timers;



namespace OFWGKTA 
{
    class MicRecordViewModel : KinectViewModelBase, IView
    {

        System.Timers.Timer timer;

        public const string ViewName = "MicRecordViewModel";

        /**
         * Instance variables
         */
        private List<AudioTrack> audioTracks;

        private float lastPeak;
        
        private int micIndex;

        private MenuRecognizer menuRecognizer;
        private ObservableCollection<MenuOption> menuList = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuList { get { return this.menuList; } }

        protected readonly SwipeGestureDetector swipeGestureRecognizer = new SwipeGestureDetector();

        /**
         * Constructor
         */     
        public MicRecordViewModel()
        {
            this.menuList = new ObservableCollection<MenuOption>();
            this.MenuList.Add(new MenuOption("one", null));
            this.MenuList.Add(new MenuOption("two", null));
            this.MenuList.Add(new MenuOption("three", null));

            this.MenuRecognizer = new MenuRecognizer(this.MenuList.Count, 100);

            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }
        
        /**
         * Initialization
         */
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

            this.micIndex = ((AppState)state).MicIndex;

            this.timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Interval = 10;
            timer.Enabled = true;
            timer.AutoReset = true;
        }

        void OnTimer(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("time!!!!");
            RaisePropertyChanged("RecordedTime");
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                // TODO clean up AudioTrack states
                
                if (this.Kinect != null)
                    Kinect.PropertyChanged -= KinectListener; // this listeners for changes in stage status, so we're unsubsribing before we leave
                
                Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
            }
        }

        void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            this.menuRecognizer.Add(Kinect.HandRight, Kinect.ShoulderCenter, Kinect.ShoulderRight);
            this.swipeGestureRecognizer.Add(e.RightHandPosition, Kinect.KinectRuntime.SkeletonEngine);
        }

        void SwipeDetected(string gesture)
        {
            if (this.Kinect.IsOnStage)
            {
                if (gesture == "SwipeToRight")
                {
                    this.BeginRecording();
                }
                else if (gesture == "SwipeToLeft")
                {
                    if (this.currentAudioTrack.State == AudioTrackState.Recording)
                        this.StopRecording();
                    else if (this.currentAudioTrack.State == AudioTrackState.Playing)
                        this.StopPlaying();
                    else if (this.currentAudioTrack.State == AudioTrackState.Loaded)
                        this.Playback();

                }
            }
        }

        void KinectListener(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "IsOnStage")
            {
                if (!Kinect.IsOnStage)
                {
                    this.StopRecording();
                }
            }
        }

        void speechCallback(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Kinect.IsOnStage)
            {
                /*
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
                */
            }
        }

        /**
         * Properties
         */

        public double MicrophoneLevel
        {
            get { return this.currentAudioTrack.MicrophoneLevel; }
            set { this.currentAudioTrack.MicrophoneLevel = value; }
        }

        public string RecordedTime
        {
            get
            {
                TimeSpan current = this.currentAudioTrack.Time;
                return String.Format("{0:D2}:{1:D2}.{2:D3}", current.Minutes, current.Seconds, current.Milliseconds);
            }
        }

        // multiply by 100 because the Progress bar's default maximum value is 100
        public float CurrentInputLevel { get { return lastPeak * 100; } }


        /**
         * Handlers
         */

        void recorder_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            RaisePropertyChanged("CurrentInputLevel");
            //RaisePropertyChanged("RecordedTime");
        }

        // 
        private AudioTrack currentAudioTrack 
        { 
            get 
            {
                if (this.audioTracks == null) {
                    audioTracks = new List<AudioTrack>();
                    newTrack();
                }
                return this.audioTracks.Last(); 
            } 
        }

        public String PlayButtonTitle
        {
            get
            {
                return "test";
            }
        }


        #region Commands

        // new track
        private RelayCommand newTrackCommand;
        public ICommand NewTrackCommand 
        { 
            get 
            { 
                if (newTrackCommand == null)
                    newTrackCommand = new RelayCommand(() => newTrack());

                return newTrackCommand; 
            } 
        }
        private void newTrack()
        {
            if (this.audioTracks.Count() > 0)
            {
                if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                    return;

                this.currentAudioTrack.SampleAggregator.MaximumCalculated -= new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            }

            AudioTrack audioTrack = new AudioTrack(micIndex);
            audioTrack.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            this.audioTracks.Add(audioTrack);

            RaisePropertyChanged("RecordedTime");
        }

        private RelayCommand playAllCommand;
        public ICommand PlayAllCommand 
        { 
            get 
            { 
                if (playAllCommand == null)
                    playAllCommand = new RelayCommand(() => playAll());

                return playAllCommand; 
            } 
        }
        private void playAll()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                return;

            foreach (AudioTrack track in this.audioTracks)
                track.State = AudioTrackState.Playing;
        }

        private RelayCommand playbackCommand;
        public ICommand PlaybackCommand 
        { 
            get 
            { 
                if (playbackCommand == null)
                    playbackCommand = new RelayCommand(() => Playback());

                return playbackCommand; 
            } 
        }
        private void Playback()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                return; 
            
            this.currentAudioTrack.State = AudioTrackState.Playing;
        }

        private RelayCommand beginRecordingCommand;
        public ICommand BeginRecordingCommand 
        { 
            get 
            {
                if (beginRecordingCommand == null)
                    beginRecordingCommand = new RelayCommand(() => BeginRecording());

                return beginRecordingCommand; 
            }
        }
        private void BeginRecording()
        {
            if (this.currentAudioTrack.State == AudioTrackState.Loaded)
                this.currentAudioTrack.State = AudioTrackState.Monitoring;

            if (this.currentAudioTrack.State != AudioTrackState.Monitoring)
                return;

            this.currentAudioTrack.State = AudioTrackState.Recording;
        }

        private RelayCommand stopRecordingCommand;
        public ICommand StopRecordingCommand 
        { 
            get 
            {
                if (stopRecordingCommand == null)
                    stopRecordingCommand = new RelayCommand(() => StopRecording());
                    
                return stopRecordingCommand; 
            } 
        }
        private void StopRecording()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Recording)
                return;

            this.currentAudioTrack.State = AudioTrackState.StopRecording;
        }

        private RelayCommand stopPlayingCommand;
        public ICommand StopPlayingCommand 
        { 
            get 
            {
                if (stopPlayingCommand == null)
                    stopPlayingCommand = new RelayCommand(() => StopPlaying());

                return stopPlayingCommand; 
            } 
        }
        private void StopPlaying()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Playing)
                return;

            this.currentAudioTrack.State = AudioTrackState.Loaded;
        }

        private ICommand goBackCommand;
        public ICommand GoBackCommand 
        { 
            get 
            { 
                if (goBackCommand == null)
                    goBackCommand = new RelayCommand(() => GoBack());

                return goBackCommand; 
            } 
        }
        private void GoBack()
        {
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        #endregion


        public MenuRecognizer MenuRecognizer
        {
            get { return menuRecognizer; }
            set
            {
                if (this.menuRecognizer != value)
                {
                    this.menuRecognizer = value;
                    RaisePropertyChanged("MenuRecognizer");
                }
            }
        }
    }
}
