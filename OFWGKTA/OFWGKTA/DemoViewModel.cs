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
    class DemoViewModel : ViewModelBase, IView
    {
        public const string ViewName = "DemoViewModel";

        // Instance variables
        private string applicationMode; 
        KinectModel kinect;

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }
        private ObservableCollection<MenuOption> menu = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> Menu { get { return this.menu; } }

        public DemoViewModel()
        {
            this.menu.Add(new MenuOption("hi", null));
            this.menu.Add(new MenuOption("what", null));
            this.menu.Add(new MenuOption("hello", null));
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        public void Activated(object state)
        {
            DemoAppState curState = (DemoAppState)state;
            this.Kinect = curState.Kinect;
            this.ApplicationMode = curState.ApplicationMode;
        }

        private void ReturnToWelcome()
        {
            kinect.Destroy();
            kinect.Dispose();
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        public KinectModel Kinect { 
            get { return kinect; }
            set
            {
                kinect = value;
                RaisePropertyChanged("Kinect");
            }
        }

        public string ApplicationMode 
        {
            get { return applicationMode; }
            set
            {
                if (applicationMode != value)
                {
                    applicationMode = value;
                    RaisePropertyChanged("ApplicationMode");
                }
            }
        }
    }
}
