using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace OFWGKTA
{
    class KinectViewModelBase : ViewModelBase 
    {
        protected KinectModel kinect = null;
        protected SpeechRecognizer speechRecognizer = null;
        protected GestureController gestureController = new GestureController();
        protected StateRecognizer stateRecognizer = new StateRecognizer();

        public StateRecognizer StateRecognizer
        {
            get { return this.stateRecognizer; }
            set
            {
                this.stateRecognizer = value;
                RaisePropertyChanged("StateRecognizer");
            }
        }

        public SpeechRecognizer SpeechRecognizer
        {
            get { return this.speechRecognizer; }
            set
            {
                if (this.speechRecognizer != value)
                {
                    this.speechRecognizer = value;
                    RaisePropertyChanged("SpeechRecognizer");
                }
            }
        }

        public KinectModel Kinect
        {
            get { return this.kinect; }
            set
            {
                if (this.kinect != value)
                {
                    this.kinect = value;
                    RaisePropertyChanged("Kinect");
                }
            }
        }
    }
}
