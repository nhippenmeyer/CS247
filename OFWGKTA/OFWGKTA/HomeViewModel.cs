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
    class HomeViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "HomeViewModel";

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public HomeViewModel()
        {
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        public void Activated(object state)
        {
            AppState curState = (AppState)state;
            this.Kinect = curState.Kinect;
        }

        private void ReturnToWelcome()
        {
            kinect.Destroy();
            kinect.Dispose();
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

    }
}
