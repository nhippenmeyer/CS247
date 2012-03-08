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
            RaisePropertyChanged("Time");
        }

        private AudioTrack currentAudioTrack
        {
            get
            {
                if (this.audioTracks == null)
                {
                    audioTracks = new List<AudioTrack>();
                    newTrack();
                }
                return this.audioTracks.Last();
            }
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
                    this.startRecording();
                }
                else if (gesture == "SwipeToLeft")
                {
                    if (this.currentAudioTrack.State == AudioTrackState.Recording)
                        this.stopRecording();
                    else if (this.currentAudioTrack.State == AudioTrackState.Playing)
                        this.stop();
                    else if (this.currentAudioTrack.State == AudioTrackState.Loaded)
                        this.play();

                }
            }
        }

        void KinectListener(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "IsOnStage")
            {
                if (!Kinect.IsOnStage)
                {
                    this.stopRecording();
                }
            }
        }

        private const double speechConfidenceMin = .88;

        void speechCallback(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Kinect.IsOnStage
                && 
                e.Result.Confidence > speechConfidenceMin)
            {
                if (e.Result.Text == "record"
                    &&
                    (this.currentAudioTrack.State == AudioTrackState.Monitoring
                     ||
                     this.currentAudioTrack.State == AudioTrackState.Loaded))
                {
                    startRecording();
                }
                else if (e.Result.Text == "play" 
                         && 
                         this.currentAudioTrack.State == AudioTrackState.Loaded)
                {
                    play();
                }
                else if (e.Result.Text == "stop" 
                         && 
                         this.currentAudioTrack.State == AudioTrackState.Playing)
                {
                    stop();
                }
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

        public string Time
        {
            get
            {
                AudioTrack currentlyPlayingTrack = this.currentAudioTrack;

                if (this.isAnyTrackPlaying
                    &&
                    this.currentAudioTrack.State != AudioTrackState.Playing)
                {
                    foreach (AudioTrack track in this.audioTracks)
                        if (track.State == AudioTrackState.Playing)
                            currentlyPlayingTrack = track;
                }
                
                TimeSpan current = currentlyPlayingTrack.Time;
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
        }

        
        

        public String PlayButtonTitle
        {
            get
            {
                if (this.currentAudioTrack.State == AudioTrackState.Playing)
                    return "Stop";
                else
                    return "Play";
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

        }

        /*
         * Play or stop all tracks
         */
        public string PlayOrStopAllButtonTitle
        {
            get
            {
                if (this.isAnyTrackPlaying)
                    return "Stop Playing All Tracks";
                else
                    return "Play All Tracks";
            }
        }
        
        private RelayCommand playOrStopAllCommand;
        public ICommand PlayOrStopAllCommand 
        { 
            get 
            {
                if (playOrStopAllCommand == null)
                    playOrStopAllCommand = new RelayCommand(() => playOrStopAll());

                return playOrStopAllCommand; 
            } 
        }
        
        private void playOrStopAll()
        {
            if (this.isAnyTrackPlaying)
                stopAll();
            else
                playAll();

            RaisePropertyChanged("PlayOrStopAllButtonTitle");
        }
        
        private void playAll()
        {
            foreach (AudioTrack track in this.audioTracks)
                if (track.State == AudioTrackState.Loaded)
                    track.State = AudioTrackState.Playing;
        }
        
        private void stopAll()
        {
            foreach (AudioTrack track in this.audioTracks)
                if (track.State == AudioTrackState.Playing)
                    track.State = AudioTrackState.Loaded;
        }
        // helper method:
        private bool isAnyTrackPlaying
        {
            get
            {
                foreach (AudioTrack track in this.audioTracks)
                    if (track.State == AudioTrackState.Playing)
                        return true;

                return false;
            }
        }


        /*
         * Play or stop the current track
         */
        public string PlayOrStopButtonTitle
        {
            get
            {
                if (this.currentAudioTrack.State == AudioTrackState.Playing)
                    return "Stop Playing this Track";
                else
                    return "Play this Track";
            }
        }
        private RelayCommand playOrStopCommand;
        public ICommand PlayOrStopCommand
        { 
            get 
            {
                if (playOrStopCommand == null)
                    playOrStopCommand = new RelayCommand(() => playOrStop());

                return playOrStopCommand; 
            } 
        }
        private void playOrStop()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Playing)
                play();
            else
                stop();

            RaisePropertyChanged("PlayOrStopButtonTitle");
        }
        private void play()
        {
            if (this.currentAudioTrack.State == AudioTrackState.Recording)
                this.currentAudioTrack.State = AudioTrackState.StopRecording;

            if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                return;

            this.currentAudioTrack.State = AudioTrackState.Playing;
        }
        private void stop()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Playing)
                return;

            this.currentAudioTrack.State = AudioTrackState.Loaded;
        }


        /*
         * Start or stop recording current track
         */
        public string StartOrStopRecordingButtonTitle
        {
            get
            {
                if (this.currentAudioTrack.State == AudioTrackState.Recording)
                    return "Stop Recording";
                else
                    return "Start Recording";
            }
        }
        private RelayCommand startOrStopRecordingCommand;
        public ICommand StartOrStopRecordingCommand 
        { 
            get 
            {
                if (startOrStopRecordingCommand == null)
                    startOrStopRecordingCommand = new RelayCommand(() => startOrStopRecording());

                return startOrStopRecordingCommand; 
            }
        }
        private void startOrStopRecording()
        {
            if (this.currentAudioTrack.State == AudioTrackState.Recording)
                stopRecording();
            else
                startRecording();

            RaisePropertyChanged("StartOrStopRecordingButtonTitle");
        }
        private void stopRecording()
        {
            if (this.currentAudioTrack.State != AudioTrackState.Recording)
                return;

            this.currentAudioTrack.State = AudioTrackState.StopRecording;
        }
        private void startRecording()
        {
            if (this.currentAudioTrack.State == AudioTrackState.Loaded)
                this.currentAudioTrack.State = AudioTrackState.Monitoring;

            if (this.currentAudioTrack.State != AudioTrackState.Monitoring)
                return;

            this.currentAudioTrack.State = AudioTrackState.Recording;
        }


        /*
         * Go back to home screen
         */
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
