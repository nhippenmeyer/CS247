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

namespace OFWGKTA 
{
    class HomeScreenViewModel : ViewModelBase, IView
    {
        public const string ViewName = "HomeScreenViewModel";

        // Instance variables
        public KinectModel kinectModel;
        private string applicationMode; 
        private ObservableCollection<string> connectedKinects;
        private int selectedKinectIndex;

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public HomeScreenViewModel()
        {
            this.connectedKinects = new ObservableCollection<string>();
            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        private void ReturnToWelcome()
        {
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, SelectedIndex));
        }

        public void Activated(object state)
        {
            ApplicationMode = (string)state;
            kinectModel = new FreePlayKinectModel(null);
            RaisePropertyChanged("Kinect");
            this.connectedKinects.Clear();
            this.connectedKinects.Add("Shit");
            this.connectedKinects.Add("Balls");
            this.connectedKinects.Add("Fuck");
            this.connectedKinects.Add("Ass");
        }

        public ObservableCollection<string> ConnectedKinects 
        {
            get { return connectedKinects; }
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
