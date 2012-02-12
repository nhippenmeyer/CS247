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
    class WelcomeViewModel : ViewModelBase, IView
    {
        public const string ViewName = "WelcomeView";

        // Instance variables
        private ObservableCollection<string> applicationModes;
        private int selectedApplicationModeIndex;

        // Commands
        private ICommand continueCommand;

        public WelcomeViewModel()
        {
            this.applicationModes = new ObservableCollection<string>();
            this.continueCommand = new RelayCommand(() => MoveToSantorum());
        }

        public ICommand ContinueCommand { get { return continueCommand; } }

        public void Activated(object state)
        {
            this.applicationModes.Clear();
            this.applicationModes.Add("Record");
            this.applicationModes.Add("Replay");
            this.applicationModes.Add("Free Use");
        }

        public ObservableCollection<string> ApplicationModes{ get { return applicationModes; } }

        private void MoveToSantorum()
        {
            Messenger.Default.Send(new NavigateMessage(HomeScreenViewModel.ViewName, this.applicationModes[SelectedIndex]));
        }
        
        public int SelectedIndex
        {
            get
            {
                return selectedApplicationModeIndex;
            }
            set
            {
                if (selectedApplicationModeIndex != value)
                {
                    selectedApplicationModeIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
