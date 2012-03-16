using System;
using System.Windows.Input;
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
using System.Timers;
using System.Windows.Threading;
using System.Windows;

namespace OFWGKTA 
{
    class HomeViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "HomeViewModel";
        private Timer startAppTimer = null;
        private int micIndex = 0;

        public event EventHandler<EventArgs> timerUp;

        private Dispatcher uiDispatcher;
        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public HomeViewModel()
        {
            this.uiDispatcher = Application.Current.Dispatcher;
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
            this.timerUp += TimerListener;
        }

        public void TimerListener(object sender, EventArgs e)
        {

            this.uiDispatcher.Invoke(new Action(delegate()
            {
                Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, new AppState(this.Kinect, this.SpeechRecognizer, this.micIndex)));
            }));
        }

        #region de/activated
        public void Activated(object state)
        {
            this.Kinect = ((AppState)state).Kinect;
            this.SpeechRecognizer = ((AppState)state).SpeechRecognizer;
            this.micIndex = ((AppState)state).MicIndex;
            if (this.Kinect != null)
            {
                this.StateRecognizer.PropertyChanged += StateListener;
                this.Kinect.SkeletonUpdated += Kinect_SkeletonUpdated;
            }
        }

        public void StateListener(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HandsOverHead")
            {
                if (this.StateRecognizer.HandsOverHead == true)
                {
                    if (startAppTimer == null)
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

        public void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (this.Kinect != null && this.Kinect.Runtime != null)
            {
                this.StateRecognizer.Update(this.Kinect);
            }
        }

        public void Deactivated()
        {
            if (this.Kinect != null)
            {
                this.StateRecognizer.PropertyChanged -= StateListener;
                this.Kinect.SkeletonUpdated -= Kinect_SkeletonUpdated;
            }
        }
        #endregion

        #region Timer
        private void StartAppTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timerUp != null)
            {
                timerUp(this, e);
            }
        }

        private void StartTimer(double seconds)
        {
            this.startAppTimer = new Timer(seconds * 1000);
            this.startAppTimer.AutoReset = false;
            this.startAppTimer.Elapsed += new ElapsedEventHandler(StartAppTimer_Elapsed);
            this.startAppTimer.Enabled = true;
        }

        private void StopTimer()
        {
            if (this.startAppTimer != null)
            {
                this.startAppTimer.Dispose();
                this.startAppTimer = null;
            }
        }
        #endregion

        public void ReturnToWelcome()
        {
            Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, new AppState(this.Kinect, this.SpeechRecognizer, this.micIndex)));
            //Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

    }
}
