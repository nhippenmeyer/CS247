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
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }
        
        /**
         * Initialization
         */
        public void Activated(object state)
        {
            micIndex = (int)state;
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
                TimeSpan current = this.currentAudioTrack.RecordedTime;
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

        private RelayCommand stopCommand;
        public ICommand StopCommand 
        { 
            get 
            {
                if (stopCommand == null)
                    stopCommand = new RelayCommand(() => Stop());

                return stopCommand; 
            } 
        }
        private void Stop()
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
    }
}
