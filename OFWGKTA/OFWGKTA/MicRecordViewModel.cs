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


namespace OFWGKTA 
{
    class MicRecordViewModel : ViewModelBase, IView
    {
        public const string ViewName = "MicRecordViewModel";

        /**
         * Instance variables
         */
        private List<AudioTrack> audioTracks;
        private int micIndex;
        private float lastPeak;

        /**
         * Constructor
         */ 
        public MicRecordViewModel()
        {
            this.goBackCommand = new RelayCommand(() => GoBack());
            this.newTrackCommand = new RelayCommand(() => newTrack());
            this.beginRecordingCommand = new RelayCommand(() => BeginRecording());
            this.stopCommand = new RelayCommand(() => Stop());
            this.playbackCommand = new RelayCommand(() => Playback());
            this.playAllCommand = new RelayCommand(() => playAll());
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }
        
        /**
         * Initialization
         */
        public void Activated(object state)
        {
            micIndex = (int)state;

            if (this.audioTracks == null)
            {
                audioTracks = new List<AudioTrack>();
                newTrack();
            }
        }

        /**
         * Properties
         */

        /* TODO:
        public double MicrophoneLevel
        {
            get { return recorder.MicrophoneLevel; }
            set { recorder.MicrophoneLevel = value; }
        }
        */

        public string RecordedTime
        {
            get
            {
                if (audioTracks == null
                    ||
                    audioTracks.Count() == 0)
                    return "";

                TimeSpan current = audioTracks.Last().RecordedTime;
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
            RaisePropertyChanged("RecordedTime");
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                // TODO?
                //this.audioTrack.State = AudioTrackState.StopRe;
            }
        }


        /**
         * Commands
         */

        private RelayCommand newTrackCommand;
        public ICommand NewTrackCommand { get { return newTrackCommand; } }
        private void newTrack()
        {
            if (this.audioTracks.Count() > 0)
            {
                if (this.audioTracks.Last().State != AudioTrackState.Loaded)
                    return;

                this.audioTracks.Last().SampleAggregator.MaximumCalculated -= new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            }

            AudioTrack audioTrack = new AudioTrack(micIndex);
            audioTrack.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            this.audioTracks.Add(audioTrack);

            RaisePropertyChanged("RecordedTime");
        }

        private RelayCommand playAllCommand;
        public ICommand PlayAllCommand { get { return playAllCommand; } }
        private void playAll()
        {
            if (this.audioTracks.Count == 0
                ||
                this.audioTracks.Last().State != AudioTrackState.Loaded)
                return;

            foreach (AudioTrack track in this.audioTracks)
                track.State = AudioTrackState.Playing;
        }

        private RelayCommand playbackCommand;
        public ICommand PlaybackCommand { get { return playbackCommand; } }
        private void Playback()
        {
            if (this.audioTracks.Count == 0
                ||
                this.audioTracks.Last().State != AudioTrackState.Loaded)
                return; 
            
            this.audioTracks.Last().State = AudioTrackState.Playing;
        }

        private RelayCommand beginRecordingCommand;
        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        private void BeginRecording()
        {
            if (this.audioTracks.Count == 0)
                return;

            if (this.audioTracks.Last().State == AudioTrackState.Loaded)
                this.audioTracks.Last().State = AudioTrackState.Monitoring;

            if (this.audioTracks.Last().State != AudioTrackState.Monitoring)
                return;

            this.audioTracks.Last().State = AudioTrackState.Recording;
        }

        private RelayCommand stopCommand;
        public ICommand StopCommand { get { return stopCommand; } }
        private void Stop()
        {
            if (this.audioTracks.Last().State != AudioTrackState.Recording)
                return;

            this.audioTracks.Last().State = AudioTrackState.StopRecording;
        }
        
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }
        private void GoBack()
        {
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

    }
}
