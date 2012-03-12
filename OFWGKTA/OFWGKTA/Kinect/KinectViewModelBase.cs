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

        public SpeechRecognizer SpeechRecognizer
        {
            get { return this.speechRecognizer; }
            set
            {
                this.speechRecognizer = value;
                RaisePropertyChanged("SpeechRecognizer");
            }
        }

        public KinectModel Kinect
        {
            get { return this.kinect; }
            set
            {
                this.kinect = value;
                RaisePropertyChanged("Kinect");
            }
        }
    }
}
