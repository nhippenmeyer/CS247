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
using System.Windows.Threading;
using System.Windows;
using Visiblox.Charts;



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

        private MenuRecognizer menuRecognizerHoriz;
        private MenuRecognizer menuRecognizerVert;

        private ObservableCollection<MenuOption> menuListHoriz = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListHoriz { get { return this.menuListHoriz; } }
        private ObservableCollection<MenuOption> menuListVert = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListVert { get { return this.menuListVert; } }

        private Dispatcher uiDispatcher;
        private DispatcherTimer metronomeTimer;
        private Boolean metronomeDotVisible = true;
        private double bpm = 120.0;
        private double dotDuration = 200.0;

        /**
         * Constructor
         */     
        public MicRecordViewModel()
        {
            this.MenuRecognizerHoriz = new MenuRecognizer(3, 50);
            this.menuListHoriz.Add(new MenuOption("Record", null, 3, this.menuRecognizerHoriz));
            this.menuListHoriz.Add(new MenuOption("Play", null, 3, this.menuRecognizerHoriz));
            this.menuListHoriz.Add(new MenuOption("New Track", null, 3, this.menuRecognizerHoriz));

            MenuRecognizerHoriz.MenuItemSelected += OnHorizMenuItemSelected;

            /*
            this.MenuRecognizerVert = new MenuRecognizer(3, 100, false);
            this.menuListVert.Add(new MenuOption("Record", null, 3, this.menuRecognizerVert));
            this.menuListVert.Add(new MenuOption("Stop Recording", null, 3, this.menuRecognizerVert));
            this.menuListVert.Add(new MenuOption("New Track", null, 3, this.menuRecognizerVert));

            MenuRecognizerVert.MenuItemSelected += OnVertMenuItemSelected;
            this.gestureController.Add(this.MenuRecognizerVert);
            */

            this.gestureController.Add(this.MenuRecognizerHoriz);

            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));

            this.uiDispatcher = Application.Current.Dispatcher;
            metronomeTimer = new System.Windows.Threading.DispatcherTimer();
            metronomeTimer.Tick += new EventHandler(metronomeTimer_Tick);
            metronomeTimer.Interval = TimeSpan.FromMilliseconds(dotDuration); 
            metronomeTimer.Start();
        }

        private void metronomeTimer_Tick(object sender, EventArgs e)
        {
            if (metronomeDotVisible)
            {
                //about to turn invisible, so interval should be [desired interval] - dotDuration
                metronomeTimer.Interval = TimeSpan.FromMilliseconds((60 * 1000 / bpm) - dotDuration);
            }
            else
            {
                metronomeTimer.Interval = TimeSpan.FromMilliseconds(dotDuration);
            }
            metronomeDotVisible = !metronomeDotVisible;
            RaisePropertyChanged("MetronomeDotVisibility");
        }

        public double BPM
        {
            get { return bpm; }
            set
            {
                bpm = value;
                this.metronomeTimer.Interval = TimeSpan.FromMilliseconds(60 * 1000 / bpm);
            }
        }
        
        public Visibility MetronomeDotVisibility
        {
            get
            {
                return metronomeDotVisible? Visibility.Visible : Visibility.Hidden;
            }
        }

        private BindableSamplePointCollection backgroundTrackSamples = new BindableSamplePointCollection();
        public BindableSamplePointCollection BackgroundTrackData
        {
            get {
                return backgroundTrackSamples;
            }
        }

        public DoubleRange BackgroundTrackXRange
        {
            get {
                DoubleRange xRange = new DoubleRange();
                if (backgroundTrackSamples.Count < 300)
                {
                    xRange.Minimum = 0;
                    xRange.Maximum = 300;
                }
                else
                {
                    xRange.Minimum = backgroundTrackSamples.Count - 210;
                    xRange.Maximum = backgroundTrackSamples.Count + 90;
                }
                xRange.Minimum = 0;
                xRange.Maximum = 300;
                return xRange;
            }
        }

        private BindableSamplePointCollection currentTrackSamples = new BindableSamplePointCollection();
        public BindableSamplePointCollection CurrentTrackData
        {
            get { return currentTrackSamples; }
        }

        public DoubleRange CurrentTrackXRange
        {
            get
            {
                DoubleRange xRange = new DoubleRange();
                int numSamples = currentTrackSamples.Count;

                if (this.currentAudioTrack.State == AudioTrackState.Recording || this.currentAudioTrack.State == AudioTrackState.StopRecording)
                {
                    if (numSamples < 250)
                    {
                        xRange.Minimum = 0;
                        xRange.Maximum = 300;
                    }
                    else
                    {
                        xRange.Minimum = numSamples - 250;
                        xRange.Maximum = numSamples + 50;
                    }
                }
                else if (this.currentAudioTrack.State == AudioTrackState.Monitoring)
                {
                    xRange.Minimum = currentTrackSamples.Count - 150;
                    xRange.Maximum = currentTrackSamples.Count + 150;
                }

                return xRange;
            }
        }

        void recorder_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            SamplePoint newSample = new SamplePoint();
            newSample.sampleNum = currentTrackSamples.Count;
            newSample.sampleVal = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            currentTrackSamples.Add(newSample);

            RaisePropertyChanged("CurrentTrackData");
            RaisePropertyChanged("CurrentTrackXRange");

            lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            RaisePropertyChanged("CurrentInputLevel");
        }

        /**
         * Initialization
         */
        #region de/activated
        public void Activated(object state)
        {
            this.Kinect = ((AppState)state).Kinect;
            this.SpeechRecognizer = ((AppState)state).SpeechRecognizer;
            if (this.Kinect != null)
            {
                this.StateRecognizer.PropertyChanged += StateListener;
                this.Kinect.SkeletonUpdated += Kinect_SkeletonUpdated;
            }
            if (this.SpeechRecognizer != null)
            {
                //this.SpeechRecognizer.SetSpeechCallback(speechCallback);
            }

            this.micIndex = ((AppState)state).MicIndex;

            this.timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Interval = 10;
            timer.Enabled = true;
            timer.AutoReset = true;
        }

        public void Deactivated()
        {
            if (this.Kinect != null)
            {
                this.StateRecognizer.PropertyChanged -= StateListener;
                this.Kinect.SkeletonUpdated -= Kinect_SkeletonUpdated;
            }
            if (this.SpeechRecognizer != null)
            {
                //this.SpeechRecognizer.SetSpeechCallback(null); // deactivates callback
            }
        }

        void OnTimer(Object source, ElapsedEventArgs e)
        {
            if (this.audioTracks.Count > 0 && this.audioTracks[0].State != AudioTrackState.Playing)
            {
                this.menuListHoriz[1].Label = "Play";
            }
            RaisePropertyChanged("Time");
        }
        
        #endregion

        void OnHorizMenuItemSelected(object sender, MenuEventArgs e)
        {
            switch (e.SelectedIndex)
            {
                case 0:
                    startOrStopRecording();
                    break;
                case 1:
                    playOrStopAll();
                    break;
                case 2:
                    newTrack();
                    break;
            }
        }

        void OnVertMenuItemSelected(object sender, MenuEventArgs e)
        {
            switch (e.SelectedIndex)
            {
                case 0:
                    startRecording();
                    break;
                case 1:
                    stopRecording();
                    break;
                case 2:
                    newTrack();
                    break;
            }
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
                
                if (this.StateRecognizer != null)
                    StateRecognizer.PropertyChanged -= StateListener; // this listeners for changes in stage status, so we're unsubsribing before we leave
                
                Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
            }
        }

        void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (Kinect.Runtime != null)
            {
                this.StateRecognizer.Update(this.Kinect);
                if (this.StateRecognizer.IsOnStage)
                {
                    this.gestureController.Update(this.Kinect);
                }
            }
        }

        void SwipeDetected(string gesture)
        {
            if (this.StateRecognizer.IsOnStage)
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

        void StateListener(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "IsOnStage")
            {
                if (!this.StateRecognizer.IsOnStage)
                {
                    stopRecording();
                    stop();
                }
            }
        }

        private const double speechConfidenceMin = .88;

        void speechCallback(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.StateRecognizer.IsOnStage
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
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                if (this.audioTracks.Count() > 0)
                {
                    if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                        return;

                    BindableSamplePointCollection newBackgroundSamples = new BindableSamplePointCollection();

                    for (int i = 0; i < Math.Max(currentTrackSamples.Count, backgroundTrackSamples.Count); ++i)
                    {
                        SamplePoint sample = new SamplePoint();

                        double backVal = (backgroundTrackSamples.Count > i) ? backgroundTrackSamples[i].sampleVal : 0.0;
                        double curVal = (currentTrackSamples.Count > i) ? currentTrackSamples[i].sampleVal : 0.0;
                        sample.sampleVal = Math.Min(backVal + curVal, 1.0);
                        sample.sampleNum = i;

                        newBackgroundSamples.Add(sample);
                    }

                    backgroundTrackSamples = newBackgroundSamples;

                    RaisePropertyChanged("BackgroundTrackData");
                    RaisePropertyChanged("BackgroundTrackXRange");

                    this.currentAudioTrack.SampleAggregator.MaximumCalculated -= new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
                }

                AudioTrack audioTrack = new AudioTrack(micIndex);
                audioTrack.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
                this.audioTracks.Add(audioTrack);
            }));
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
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                if (this.isAnyTrackPlaying)
                {
                    stopAll();
                }
                else
                {
                    playAll();
                }

                RaisePropertyChanged("PlayOrStopAllButtonTitle");
            }));
        }
        
        private void playAll()
        {
            foreach (AudioTrack track in this.audioTracks)
                if (track.State == AudioTrackState.Loaded)
                    track.State = AudioTrackState.Playing;
            this.menuListHoriz[1].Label = "Stop Playing";
        }
        
        private void stopAll()
        {
            foreach (AudioTrack track in this.audioTracks)
                if (track.State == AudioTrackState.Playing)
                    track.State = AudioTrackState.Loaded;
            this.menuListHoriz[1].Label = "Play";
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
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                if (this.currentAudioTrack.State == AudioTrackState.Recording)
                    this.currentAudioTrack.State = AudioTrackState.StopRecording;

                if (this.currentAudioTrack.State != AudioTrackState.Loaded)
                    return;

                this.currentAudioTrack.State = AudioTrackState.Playing;
            }));
        }
        private void stop()
        {
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                if (this.currentAudioTrack.State != AudioTrackState.Playing)
                    return;

                this.currentAudioTrack.State = AudioTrackState.Loaded;
            }));
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
            {
                stopRecording();
            }
            else
            {
                startRecording();
            }

            RaisePropertyChanged("StartOrStopRecordingButtonTitle");
        }
        private void stopRecording()
        {
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                if (this.currentAudioTrack.State != AudioTrackState.Recording)
                    return;

                this.currentAudioTrack.State = AudioTrackState.StopRecording;
                this.menuListHoriz[0].Label = "Record";
            }));
        }
        private void startRecording()
        {
            // "start recording" -> overwrite current track
            // note that at this point if the track was supposed to be saved
            // it already has been (during stoprecording).
            //this.currentTrackSamples.Clear();
            this.uiDispatcher.Invoke(new Action(delegate() { 
                if (this.currentAudioTrack.State == AudioTrackState.Loaded)
                    this.currentAudioTrack.State = AudioTrackState.Monitoring;
    
                if (this.currentAudioTrack.State != AudioTrackState.Monitoring)
                    return;

                this.currentTrackSamples.Clear(); 
                RaisePropertyChanged("CurrentTrackData"); 

                this.currentAudioTrack.State = AudioTrackState.Recording;
                this.menuListHoriz[0].Label = "Stop Recording";
            }));
            
            RaisePropertyChanged("CurrentTrackData");

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

        #region Properties
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

        public MenuRecognizer MenuRecognizerHoriz
        {
            get { return this.menuRecognizerHoriz; }
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
        #endregion Properties
    }
}
