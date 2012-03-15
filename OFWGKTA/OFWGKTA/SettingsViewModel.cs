using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NAudio.Wave;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System.IO;
using Microsoft.Speech.Recognition;
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OFWGKTA 
{
    class SettingsViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "SettingsViewModel";
        private Timer backButtonTimer = null;
        private int micIndex = 0;

        // Sliders retrieved from view as well as parameters related to them
        public Slider sliderMicLevel;
        private double micLevelFraction;
        private int micLevelMin = 0;
        private int micLevelMax = 100;

        public Slider sliderBpm;
        private double bpmFraction;
        private int bpmMin = 60;
        private int bpmMax = 160;

        public Button backButton;

        public event EventHandler<EventArgs> timerUp;

        private Dispatcher uiDispatcher;

        public SettingsViewModel()
        {
            this.uiDispatcher = Application.Current.Dispatcher;
            this.timerUp += TimerListener;
        }

        #region de/activated
        public void Activated(object state)
        {
            this.Kinect = ((AppState)state).Kinect;
            this.SpeechRecognizer = ((AppState)state).SpeechRecognizer;
            this.micIndex = ((AppState)state).MicIndex;
            if (this.Kinect != null)
            {
                this.Kinect.SkeletonUpdated += Kinect_SkeletonUpdated;
            }
        }

        public void Deactivated()
        {
            if (this.Kinect != null)
            {
                this.Kinect.SkeletonUpdated -= Kinect_SkeletonUpdated;
            }
        }
        #endregion

        #region KinectComputation
        public void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (this.Kinect != null)
            {
                double x = this.Kinect.HandRight.X; 
                double y = this.Kinect.HandRight.Y;
                if (IsInBounds(x, y, sliderMicLevel))
                {
                    this.MicLevelFraction = GetFraction(y, sliderMicLevel);
                }
                if (IsInBounds(x, y, sliderBpm))
                {
                    this.BpmFraction = GetFraction(y, sliderBpm);
                }
                if (IsInBounds(x, y, backButton))
                {
                    if (backButtonTimer == null)
                    {
                        StartTimer(1.5);
                    }
                }
                else
                {
                    StopTimer();
                }
            }
        }

        double GetFraction(double y, Slider slider)
        {
            return 1 - (int)(((y - slider.Margin.Top)/ slider.ActualHeight) * 10) / (double) 10; 
        }

        bool IsInBounds(double x, double y, FrameworkElement slider)
        {
            double left = slider.Margin.Left;
            double right = left + slider.ActualWidth;
            double top = slider.Margin.Top;
            double bottom = top + slider.ActualHeight;
            return (x > left && x < right && y > top && y < bottom);
        }

        #endregion

        #region Timer
        public void TimerListener(object sender, EventArgs e)
        {
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, new AppState(this.Kinect, this.SpeechRecognizer, this.micIndex)));
            }));
        }

        private void StartAppTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timerUp != null)
            {
                timerUp(this, e);
            }
        }

        private void StartTimer(double seconds)
        {
            this.backButtonTimer = new Timer(seconds * 1000);
            this.backButtonTimer.AutoReset = false;
            this.backButtonTimer.Elapsed += new ElapsedEventHandler(StartAppTimer_Elapsed);
            this.backButtonTimer.Enabled = true;
            RaisePropertyChanged("BackButtonPushed");
        }

        private void StopTimer()
        {
            if (this.backButtonTimer != null)
            {
                this.backButtonTimer.Dispose();
                this.backButtonTimer = null;
                RaisePropertyChanged("BackButtonPushed");
            }
        }
        #endregion

        #region Properties
        int ConvertFraction(double fraction, int rangeStart, int rangeEnd)
        {
            int rangeWidth = rangeEnd - rangeStart;
            int addToRangeStart = (int) Math.Ceiling(rangeWidth * fraction);
            return rangeStart + addToRangeStart;
        }

        public bool BackButtonPushed { get { return backButtonTimer != null; } }
        public int MicLevel
        {
            get { return ConvertFraction(this.micLevelFraction, this.micLevelMin, this.micLevelMax); }
        }

        public int Bpm
        {
            get { return ConvertFraction(this.bpmFraction, this.bpmMin, this.bpmMax); }
        }

        public double MicLevelFraction
        {
            get { return this.micLevelFraction; }
            set
            {
                if (this.micLevelFraction != value)
                {
                    this.micLevelFraction = value;
                    RaisePropertyChanged("MicLevelFraction");
                    RaisePropertyChanged("MicLevel");
                }
            }
        }

        public double BpmFraction
        {
            get { return this.bpmFraction; }
            set
            {
                if (this.bpmFraction != value)
                {
                    this.bpmFraction = value;
                    RaisePropertyChanged("BpmFraction");
                    RaisePropertyChanged("Bpm");
                }
            }

        }
        #endregion

    }
}
