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


namespace OFWGKTA 
{
    class MicRecordViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "MicRecordViewModel";

        private RelayCommand beginRecordingCommand;
        private RelayCommand stopCommand;
        private RelayCommand rewindCommand;
        private RelayCommand playbackCommand;

        private IAudioRecorder recorder;
        private IAudioPlayer player;
        
        private int leftPosition;
        private int rightPosition;
        private int totalWaveFormSamples;

        private float lastPeak;
        private string waveFileName;

        private int micIndex;

        public MicRecordViewModel()
        {
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
            this.Kinect.SetSpeechCallback(speechCallback);
            this.Kinect.PropertyChanged += KinectListener;

            this.recorder.SampleAggregator.RaiseRestart();
            this.micIndex = ((AppState)state).MicIndex;

            if (this.recorder.RecordingState != RecordingState.Monitoring)
                BeginMonitoring();
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
                    this.BeginRecording();
                }
                else if (e.Result.Text == "play" && recorder.RecordingState != RecordingState.Recording)
                {
                    this.Playback();
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
            this.player.LoadFile(this.waveFileName);
            this.player.Play();
        }

        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        private void BeginRecording()
        {
            this.waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            recorder.BeginRecording(waveFileName);
            recorder.SampleAggregator.RaiseStart();
            RaisePropertyChanged("MicrophoneLevel");
            //RaisePropertyChanged("ShowWaveForm");
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
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

    }
}