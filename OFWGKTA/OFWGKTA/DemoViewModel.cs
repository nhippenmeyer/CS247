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
    class DemoViewModel : KinectViewModelBase, IView
    {
        public const string ViewName = "DemoViewModel";

        // Instance variables
        private string applicationMode; 

        private MenuRecognizer menuRecognizerHoriz;
        private MenuRecognizer menuRecognizerVert;
        private StateRecognizer stateRecognizer;

        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }

        private ObservableCollection<MenuOption> menuListHoriz = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListHoriz { get { return this.menuListHoriz; } }
        private ObservableCollection<MenuOption> menuListVert = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuListVert { get { return this.menuListVert; } }

        public DemoViewModel()
        {
            this.menuListHoriz.Add(new MenuOption("Play", null, 4));
            this.menuListHoriz.Add(new MenuOption("Rewind", null, 4));
            this.menuListHoriz.Add(new MenuOption("Start Recording", null, 4));
            this.menuListHoriz.Add(new MenuOption("Stop Recording", null, 4));
            this.MenuRecognizerHoriz = new MenuRecognizer(this.MenuListHoriz.Count, 100);

            this.menuListVert.Add(new MenuOption("Play", null, 4));
            this.menuListVert.Add(new MenuOption("Rewind", null, 4));
            this.menuListVert.Add(new MenuOption("Start Recording", null, 4));
            this.menuListVert.Add(new MenuOption("Stop Recording", null, 4));
            this.MenuRecognizerVert = new MenuRecognizer(this.MenuListVert.Count, 100, false);

            this.StateRecognizer = new StateRecognizer();

            this.gestureController.Add(this.menuRecognizerHoriz);
            this.gestureController.Add(this.menuRecognizerVert);
            this.gestureController.Add(this.stateRecognizer);

            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        #region de/activated
        public void Activated(object state)
        {
            DemoAppState curState = (DemoAppState)state;
            if (this.Kinect == null) { this.Kinect = (KinectModel)curState.Kinect; }
            this.ApplicationMode = curState.ApplicationMode;
            if (this.Kinect != null)
            {
                this.Kinect.SkeletonUpdated += Kinect_SkeletonUpdated;
            }
        }

        public void Deactivated()
        {
            if (this.Kinect != null)
            {
                this.Kinect.SkeletonUpdated -= Kinect_SkeletonUpdated;
            }
        }
        #endregion

        private void ReturnToWelcome()
        {
            kinect.Destroy();
            kinect.Dispose();
            Messenger.Default.Send(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (this.stateRecognizer.IsOnStage)
            {
                this.gestureController.Update(Kinect);
            }
        }

        #region Properties
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

        public StateRecognizer StateRecognizer
        {
            get { return stateRecognizer; }
            set
            {
                if (this.stateRecognizer != value)
                {
                    this.stateRecognizer = value;
                    RaisePropertyChanged("StateRecognizer");
                }
            }
        }

        public MenuRecognizer MenuRecognizerHoriz
        {
            get { return menuRecognizerHoriz; }
            set
            {
                if (this.menuRecognizerHoriz != value)
                {
                    this.menuRecognizerHoriz = value;
                    RaisePropertyChanged("MenuRecognizerHoriz");
                }
            }
        }

        public MenuRecognizer MenuRecognizerVert
        {
            get { return menuRecognizerVert; }
            set
            {
                if (this.menuRecognizerVert != value)
                {
                    this.menuRecognizerVert = value;
                    RaisePropertyChanged("MenuRecognizerVert");
                }
            }
        }
        #endregion
    }
}
