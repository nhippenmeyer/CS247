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
        private ObservableCollection<string> recordingDevices;
        private int selectedRecordingDeviceIndex;

        // Commands
        private ICommand continueCommand;

        public WelcomeViewModel()
        {
            this.recordingDevices = new ObservableCollection<string>();
            this.continueCommand = new RelayCommand(() => MoveToSantorum());
        }

        public ICommand ContinueCommand { get { return continueCommand; } }

        public void Activated(object state)
        {
            this.recordingDevices.Clear();
            for (int n = 0; n < 10; n++)
            {
                this.recordingDevices.Add("FUCKSHIT" + n);
            }
                    /*
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                this.recordingDevices.Add(WaveIn.GetCapabilities(n).ProductName);
            }
                    */
        }

        private void MoveToSantorum()
        {
            Messenger.Default.Send(new NavigateMessage(SantorumViewModel.ViewName, SelectedIndex));
        }

        public ObservableCollection<string> RecordingDevices 
        {
            get { return recordingDevices; }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedRecordingDeviceIndex;
            }
            set
            {
                if (selectedRecordingDeviceIndex != value)
                {
                    selectedRecordingDeviceIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
