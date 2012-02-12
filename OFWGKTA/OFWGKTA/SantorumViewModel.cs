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
    class SantorumViewModel : ViewModelBase, IView
    {
        public const string ViewName = "SantorumView";

        // Instance variables
        private int priorIndex; 
        private ObservableCollection<string> connectedKinects;
        private int selectedKinectIndex;

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        public SantorumViewModel()
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
            PriorIndex = (int)state;
            this.connectedKinects.Clear();
            this.connectedKinects.Add("Shit");
            this.connectedKinects.Add("Balls");
            this.connectedKinects.Add("Fuck");
            this.connectedKinects.Add("Ass");
        }

        public int PriorIndex
        {
            get
            {
                return priorIndex;
            }
            set
            {
                if (priorIndex != value)
                {
                    priorIndex = value;
                    RaisePropertyChanged("PriorIndex");
                }
            }
        }
            
        public ObservableCollection<string> ConnectedKinects 
        {
            get { return connectedKinects; }
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
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
