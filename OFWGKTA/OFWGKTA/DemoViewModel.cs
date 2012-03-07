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

        private MenuRecognizer menuRecognizer;
        private MenuRecognizer menuRecognizer2;
        private GestureController gestureController = new GestureController();
        
        // Commands
        private ICommand goBackCommand;
        public ICommand GoBackCommand { get { return goBackCommand; } }
        private ObservableCollection<MenuOption> menuList = new ObservableCollection<MenuOption>();
        public ObservableCollection<MenuOption> MenuList { get { return this.menuList; } }

        public DemoViewModel()
        {
            this.menuList.Add(new MenuOption("hi", null));
            this.menuList.Add(new MenuOption("what", null));
            this.menuList.Add(new MenuOption("hello", null));

            this.MenuRecognizer = new MenuRecognizer(this.MenuList.Count, 100);
            this.MenuRecognizer2 = new MenuRecognizer(this.MenuList.Count, 100, false);
            this.gestureController.Add(this.menuRecognizer);
            this.gestureController.Add(this.menuRecognizer2);

            this.goBackCommand = new RelayCommand(() => ReturnToWelcome());
        }

        public void Activated(object state)
        {
            DemoAppState curState = (DemoAppState)state;
            this.Kinect = curState.Kinect;
            this.Kinect.SkeletonUpdated += new EventHandler<SkeletonEventArgs>(Kinect_SkeletonUpdated);
            this.ApplicationMode = curState.ApplicationMode;
        }


        void Kinect_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            this.menuRecognizer.Add(Kinect);
            this.menuRecognizer2.Add(Kinect);
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

        public MenuRecognizer MenuRecognizer
        {
            get { return menuRecognizer; }
            set
            {
                if (this.menuRecognizer != value)
                {
                    this.menuRecognizer = value;
                    RaisePropertyChanged("MenuRecognizer");
                }
            }
        }

        public MenuRecognizer MenuRecognizer2
        {
            get { return menuRecognizer2; }
            set
            {
                if (this.menuRecognizer2 != value)
                {
                    this.menuRecognizer2 = value;
                    RaisePropertyChanged("MenuRecognizer2");
                }
            }
        }
    }
}
