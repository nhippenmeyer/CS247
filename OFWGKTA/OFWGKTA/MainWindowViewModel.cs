using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Data;
using System.Globalization;

namespace OFWGKTA
{
    class MainWindowViewModel : ViewModelBase
    {
        Dictionary<string, FrameworkElement> views;
        private FrameworkElement currentView;
        private string currentViewName;

        public MainWindowViewModel()
        {
            views = new Dictionary<string, FrameworkElement>();

            // Register callbacks given broadcast messages
            Messenger.Default.Register<NavigateMessage>(this, (message) => OnNavigate(message));
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
            
            // Set up the view for each viewmodel
            SetupView(WelcomeViewModel.ViewName, new WelcomeView(), new WelcomeViewModel());
            SetupView(HomeViewModel.ViewName, new HomeView(), new HomeViewModel());
            SetupView(DemoViewModel.ViewName, new DemoView(), new DemoViewModel());
            SetupView(SettingsViewModel.ViewName, new SettingsView(), new SettingsViewModel());
            SetupView(MicRecordViewModel.ViewName, new MicRecordView(), new MicRecordViewModel());
            SetupView(FancyGraphViewModel.ViewName, new FancyGraphView(), new FancyGraphViewModel());

            // Send message that welcome view is ready to be displayed
            Messenger.Default.Send<NavigateMessage>(new NavigateMessage(WelcomeViewModel.ViewName, null));
        }

        private void SetupView(string viewName, FrameworkElement view, ViewModelBase viewModel)
        {
            view.DataContext = viewModel;
            views.Add(viewName, view);
        }

        private void OnNavigate(NavigateMessage message) 
        {
            // Deactivated allows us to clean up Kinect stuff if necessary
            if (this.CurrentView != null)
            {
                ((IView)this.CurrentView.DataContext).Deactivated();
            }

            // Change the view to the target view
            this.CurrentView = views[message.TargetView];
            this.currentViewName = message.TargetView;

            // Activated allows us to set up the Kinect stuff if necessary
            ((IView)this.CurrentView.DataContext).Activated(message.State);
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
           
        }

        public FrameworkElement CurrentView
        {
            get { return currentView; }
            set
            {
                if (this.currentView != value)
                {
                    currentView = value;
                    RaisePropertyChanged("CurrentView");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Messenger.Default.Send(new ShuttingDownMessage(currentViewName));
            ((IDisposable)CurrentView.DataContext).Dispose();
            base.Dispose(disposing);
        }
    }
}
