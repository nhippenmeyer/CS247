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
            this.continueCommand = new RelayCommand(() => MoveToHomeScreen());
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

        private void MoveToHomeScreen()
        {
            AppState curState = null;
            Stream fileStream;
            bool success = true;
            switch (this.applicationModes[SelectedIndex])
            {
                case ("Replay"):
                    OpenFileDialog openFileDialog = new OpenFileDialog { };
                    openFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenRead(openFileDialog.FileName);
                        curState = new AppState(this.applicationModes[SelectedIndex], new ReplayKinectModel(fileStream));
                    }
                    catch
                    {
                        success = false;
                    }
                    break;
                case ("Record"):
                    SaveFileDialog saveFileDialog = new SaveFileDialog { };
                    saveFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenWrite(saveFileDialog.FileName);
                        curState = new AppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(fileStream));
                    }
                    catch
                    {
                        success = false;
                    }
                    break;
                case ("Free Use"):
                    curState = new AppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(null)); 
                    break;
            }
            if (success)
            {
                Messenger.Default.Send(new NavigateMessage(HomeScreenViewModel.ViewName, curState));
            }
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
