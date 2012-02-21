using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace OFWGKTA
{
    class KinectViewModelBase : ViewModelBase 
    {
        protected AudioKinectModel kinect;

        public AudioKinectModel Kinect { 
            get { return kinect; }
            set
            {
                kinect = value;
                RaisePropertyChanged("Kinect");
            }
        }
    }
}
