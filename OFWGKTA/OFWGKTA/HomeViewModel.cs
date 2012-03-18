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
using System.Windows.Controls;
using System.Diagnostics;

namespace OFWGKTA 
{
    class HomeViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "HomeViewModel";
        private Timer quitButtonTimer = null;
        private Timer startButtonTimer = null;
        private int micIndex = 0;

        public Button quitButton;
        public Button startButton;

        private Dispatcher uiDispatcher;
        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public HomeViewModel()
        {
            this.uiDispatcher = Application.Current.Dispatcher;
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        public void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (this.Kinect != null)
            {
                double x = this.Kinect.HandRight.X; 
                double y = this.Kinect.HandRight.Y;
                if (IsInBounds(x, y, quitButton))
                {
                    if (quitButtonTimer == null)
                    {
                        StartTimer_Quit(1.5);
                    }
                }
                else
                {
                    StopTimer_Quit();
                }
                if (IsInBounds(x, y, startButton))
                {
                    if (startButtonTimer == null)
                    {
                        StartTimer_Start(1.5);
                    }
                }
                else
                {
                    StopTimer_Start();
                }
            }
        }

        bool IsInBounds(double x, double y, FrameworkElement slider)
        {
            double left = slider.Margin.Left;
            double right = left + slider.ActualWidth;
            double top = slider.Margin.Top;
            double bottom = top + slider.ActualHeight;
            return (x > left && x < right && y > top && y < bottom);
        }

        #region de/activated
        public void Activated(object state)
        {
            this.Kinect = ((AppState)state).Kinect;
            this.SpeechRecognizer = ((AppState)state).SpeechRecognizer;
            this.micIndex = ((AppState)state).MicIndex;
        }

        public void Deactivated() { }

        #endregion

        #region Timer
        private void StartButtonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, new AppState(this.Kinect, this.SpeechRecognizer, this.micIndex)));
            }));
        }

        private void StartTimer_Start(double seconds)
        {
            this.startButtonTimer = new Timer(seconds * 1000);
            this.startButtonTimer.AutoReset = false;
            this.startButtonTimer.Elapsed += new ElapsedEventHandler(StartButtonTimer_Elapsed);
            this.startButtonTimer.Enabled = true;
            RaisePropertyChanged("StartButtonPushed");
        }

        private void StopTimer_Start()
        {
            if (this.startButtonTimer != null)
            {
                this.startButtonTimer.Dispose();
                this.startButtonTimer = null;
                RaisePropertyChanged("StartButtonPushed");
            }
        }

        private void StartTimer_Quit(double seconds)
        {
            this.quitButtonTimer = new Timer(seconds * 1000);
            this.quitButtonTimer.AutoReset = false;
            this.quitButtonTimer.Elapsed += new ElapsedEventHandler(QuitButtonTimer_Elapsed);
            this.quitButtonTimer.Enabled = true;
            RaisePropertyChanged("QuitButtonPushed");
        }

        private void StopTimer_Quit()
        {
            if (this.quitButtonTimer != null)
            {
                this.quitButtonTimer.Dispose();
                this.quitButtonTimer = null;
                RaisePropertyChanged("QuitButtonPushed");
            }
        }

        private void QuitButtonTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.uiDispatcher.Invoke(new Action(delegate()
            {
                Process.GetCurrentProcess().Kill();
            }));
        }

        #endregion

        public void ReturnToWelcome()
        {
            Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, new AppState(this.Kinect, this.SpeechRecognizer, this.micIndex)));
            //Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        public bool QuitButtonPushed { get { return quitButtonTimer != null; } }
        public bool StartButtonPushed { get { return startButtonTimer != null; } }

    }
}
