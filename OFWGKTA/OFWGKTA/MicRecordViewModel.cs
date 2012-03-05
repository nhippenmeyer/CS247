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

        private RelayCommand playAllCommand;
        private RelayCommand newTrackCommand;
        private RelayCommand beginRecordingCommand;
        private RelayCommand stopCommand;
        private RelayCommand playbackCommand;

        //private AudioTrack audioTrack;

        private int micIndex;

        public MicRecordViewModel()
        {
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
            this.newTrackCommand = new RelayCommand(() => newTrack());
            this.beginRecordingCommand = new RelayCommand(() => BeginRecording());
            this.stopCommand = new RelayCommand(() => Stop());
            this.playbackCommand = new RelayCommand(() => Playback());
            this.playAllCommand = new RelayCommand(() => playAll());
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
        }
        
        public void Activated(object state)
        {
            micIndex = (int)state;

            if (this.audioTracks == null)
            {
                audioTracks = new List<AudioTrack>();
                AudioTrack audioTrack = new AudioTrack(micIndex);
                this.audioTracks.Add(audioTrack);

                //this.audioTrack = new AudioTrack(micIndex);
                //this.audioTrack.State = AudioTrackState.Monitoring;
                //this.audioTrack.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
                //RaisePropertyChanged("MicrophoneLevel");
            }
        }

        public ICommand NewTrackCommand { get { return newTrackCommand; } }
        private void newTrack()
        {
            AudioTrack audioTrack = new AudioTrack(micIndex);
            this.audioTracks.Add(audioTrack);
        }

        public ICommand PlayAllCommand { get { return playAllCommand; } }
        private void playAll()
        {
            foreach (AudioTrack track in this.audioTracks)
                track.State = AudioTrackState.Playing;
        }

        private List<AudioTrack> audioTracks;


        



        void recorder_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            //lastPeak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            //RaisePropertyChanged("CurrentInputLevel");
            RaisePropertyChanged("RecordedTime");
        }

        /*
        public ICommand RewindCommand { get { return rewindCommand; } }
        private void Rewind()
        {
            this.player.CurrentPosition = new System.TimeSpan(0);
        }
        */

        public ICommand PlaybackCommand { get { return playbackCommand; } }
        private void Playback()
        {
            this.audioTracks.Last().State = AudioTrackState.Playing;
            //this.audioTrack.State = AudioTrackState.Playing;
            //this.player.LoadFile(this.waveFileName);
            //this.player.Play();
        }

        public ICommand BeginRecordingCommand { get { return beginRecordingCommand; } }
        private void BeginRecording()
        {
            //AudioTrack audioTrack = new AudioTrack(micIndex);
            //this.audioTracks.Add(audioTrack);
            this.audioTracks.Last().State = AudioTrackState.Monitoring;
            this.audioTracks.Last().State = AudioTrackState.Recording;


            //RaisePropertyChanged("MicrophoneLevel");
            //RaisePropertyChanged("ShowWaveForm");
        }

        public ICommand StopCommand { get { return stopCommand; } }
        private void Stop()
        {
            this.audioTracks.Last().State = AudioTrackState.StopRecording;
        }
        
        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                // TODO?
                //this.audioTrack.State = AudioTrackState.StopRe;
            }
        }

        public string RecordedTime
        {
            get
            {
                return "poop";
                //TimeSpan current = recorder.RecordedTime;
                //return String.Format("{0:D2}:{1:D2}.{2:D3}", current.Minutes, current.Seconds, current.Milliseconds);
            }
        }

        /*
        public double MicrophoneLevel
        {
            get { return recorder.MicrophoneLevel; }
            set { recorder.MicrophoneLevel = value; }
        }*/

        // multiply by 100 because the Progress bar's default maximum value is 100
        //public float CurrentInputLevel { get { return lastPeak * 100; } }

        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        private void ReturnToWelcome()
        {
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

    }
}
