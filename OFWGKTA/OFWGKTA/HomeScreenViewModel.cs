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

namespace OFWGKTA 
{
    class HomeScreenViewModel : ViewModelBase, IView
    {
        public const string ViewName = "HomeScreenViewModel";

        // Instance variables
        private string applicationMode; 
        private KinectModel kinectModel;
        private int selectedKinectIndex;

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public HomeScreenViewModel()
        {
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        public void Activated(object state)
        {
            Stream fileStream;
            ApplicationMode = (string)state;
            switch (ApplicationMode)
            {
                case ("Replay"):
                    OpenFileDialog openFileDialog = new OpenFileDialog { };
                    openFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenRead(openFileDialog.FileName);
                        kinectModel = new ReplayKinectModel(fileStream);
                    }
                    catch
                    {
                        Messenger.Default.Send<NavigateMessage>(new NavigateMessage(WelcomeViewModel.ViewName, null));
                    }
                    break;
                case ("Record"):
                    SaveFileDialog saveFileDialog = new SaveFileDialog { };
                    saveFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenWrite(saveFileDialog.FileName);
                        kinectModel = new FreePlayKinectModel(fileStream);
                    }
                    catch
                    {
                        Messenger.Default.Send<NavigateMessage>(new NavigateMessage(WelcomeViewModel.ViewName, null));
                    }
                    break;
                case ("Free Use"):
                    kinectModel = new FreePlayKinectModel(null);
                    break;
            }
            RaisePropertyChanged("Kinect");
        }

        private void ReturnToWelcome()
        {
            kinectModel.Destroy();
            kinectModel.Dispose();
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, SelectedIndex));
        }

        public KinectModel Kinect { 
            get { return kinectModel; }
            set { kinectModel = value; RaisePropertyChanged("Kinect"); }
        }

        public string ApplicationMode 
        {
            get
            {
                return applicationMode;
            }
            set
            {
                if (applicationMode != value)
                {
                    applicationMode = value;
                    RaisePropertyChanged("ApplicationMode");
                }
            }
        }
            
        public int SelectedIndex
        {
            get
            {
                return selectedKinectIndex;
            }
            set
            {
                if (selectedKinectIndex != value)
                {
                    selectedKinectIndex = value;
                    //RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
