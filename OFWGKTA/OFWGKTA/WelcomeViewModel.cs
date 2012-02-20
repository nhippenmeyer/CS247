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
            this.applicationModes.Add("Audio App");
        }

        public ObservableCollection<string> ApplicationModes{ get { return applicationModes; } }

        private void MoveToHomeScreen()
        {
            Stream fileStream;
            switch (this.applicationModes[SelectedIndex])
            {
                case ("Replay"):
                    OpenFileDialog openFileDialog = new OpenFileDialog { };
                    openFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenRead(openFileDialog.FileName);
                        var curState = new DemoAppState(this.applicationModes[SelectedIndex], new ReplayKinectModel(fileStream));
                        Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                    }
                    catch { }
                    break;
                case ("Record"):
                    SaveFileDialog saveFileDialog = new SaveFileDialog { };
                    saveFileDialog.ShowDialog();
                    try
                    {
                        fileStream = File.OpenWrite(saveFileDialog.FileName);
                        var curState = new DemoAppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(fileStream));
                        Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                    }
                    catch { }
                    break;
                case ("Free Use"):
                    {
                        var curState = new DemoAppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(null));
                        Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                        break;
                    }
                case ("Audio App"):
                    {
                        List<string> list = new List<string> { "color", "wireframe", "shape", "exit" };
                        var curState = new AppState(new AudioKinectModel(list, null));
                        Messenger.Default.Send(new NavigateMessage(HomeViewModel.ViewName, curState));
                        break;
                    }
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
