using GalaSoft.MvvmLight;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Visiblox.Charts;
using System.Windows.Controls;
using System;
using System.Timers;

namespace OFWGKTA
{
    class FancyGraphViewModel : ViewModelBase, IView
    {
        public const string ViewName = "FancyGraphViewModel";

        /**
         * Instance variables
         */
        private List<AudioTrack> audioTracks;
        private int micIndex;
        private AudioTrack currentTrack;
        
        /**
         * Constructor
         */ 
        public FancyGraphViewModel()
        {
            this.goBackCommand = new RelayCommand(() => GoBack());

            /*
            this.newTrackCommand = new RelayCommand(() => newTrack());
            this.beginRecordingCommand = new RelayCommand(() => BeginRecording());
            this.stopCommand = new RelayCommand(() => Stop());
            this.playbackCommand = new RelayCommand(() => Playback());
            this.playAllCommand = new RelayCommand(() => playAll());
             */
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
                this.audioTracks = new List<AudioTrack>();
                newTrack();
            }
        }

        private BindableSamplePointCollection sampleData = new BindableSamplePointCollection();
        public BindableSamplePointCollection SampleData {
            get { return sampleData; }
        }

        private DoubleRange xRange = new DoubleRange();
        public DoubleRange XRange
        {
            get {
                if (sampleData.Count > 70)
                {
                    xRange.Minimum = sampleData.Count - 70;
                    xRange.Maximum = sampleData.Count + 30;
                }
                else
                {
                    xRange.Minimum = 0;
                    xRange.Maximum = 100;
                }
                return xRange;
            }
        }
        

        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }
        private void GoBack()
        {
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            if (message.CurrentViewName == ViewName)
            {
                // TODO?
                //this.audioTrack.State = AudioTrackState.StopRe;
            }
        }

        private RelayCommand newTrackCommand;
        public ICommand NewTrackCommand { get { return newTrackCommand; } }
        private void newTrack()
        {
            if (audioTracks.Count > 0)
            {
                if (this.audioTracks[this.audioTracks.Count - 1].State != AudioTrackState.Loaded)
                    return;

                currentTrack.SampleAggregator.MaximumCalculated -= new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            }

            AudioTrack audioTrack = new AudioTrack(micIndex);
            audioTrack.SampleAggregator.MaximumCalculated += new EventHandler<MaxSampleEventArgs>(recorder_MaximumCalculated);
            this.audioTracks.Add(audioTrack);
            currentTrack = audioTrack;

            //RaisePropertyChanged("RecordedTime");
        }

        void recorder_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            SamplePoint newSample = new SamplePoint();
            newSample.sampleNum = sampleData.Count;
            newSample.sampleVal = Math.Max(e.MaxSample, Math.Abs(e.MinSample));
            sampleData.Add(newSample);
            RaisePropertyChanged("SampleData");
            RaisePropertyChanged("XRange");
        }

    }
}