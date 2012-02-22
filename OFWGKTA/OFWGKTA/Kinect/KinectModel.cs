using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox.Record;
using Microsoft.Research.Kinect.Nui;
using GalaSoft.MvvmLight;
using Coding4Fun.Kinect.Wpf;
using System.Globalization;
using System.Collections.ObjectModel;
using Kinect.Toolbox;

namespace OFWGKTA
{
    public class KinectModel : ViewModelBase
    {
        private Vector head;
        private Vector handLeft;
        private Vector handRight;
        private Vector shoulderCenter;
        private Vector shoulderRight;
        private Vector shoulderLeft;
        private Vector ankleRight;
        private Vector ankleLeft;
        private Vector footLeft;
        private Vector footRight;
        private Vector wristLeft;
        private Vector wristRight;
        private Vector elbowLeft;
        private Vector elbowRight;
        private Vector kneeLeft;
        private Vector kneeRight;
        private Vector hipCenter;

        protected readonly SwipeGestureDetector swipeGestureRecognizer = new SwipeGestureDetector();
        public event EventHandler<SwipeEventArgs> SwipeDetected;
        private string gesture = "";

        public KinectModel() : base()
        {
            swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;
        }

        public virtual void Destroy()
        {
            swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;    
        }

        public void OnGestureDetected(string gesture)
        {
            Gesture = gesture;
            if (SwipeDetected != null)
            {
                SwipeDetected(this, new SwipeEventArgs()
                {
                    Gesture = gesture
                });
            }
        }

        void SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e) { }
        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) { }

        protected Vector GetScaledPosition(Joint joint)
        {
            Joint temp;
            temp = joint.ScaleTo(640, 480, 1.5f, 1.5f);
            return temp.Position;
        }

        public Vector Head
        {
            get { return head; }
            set
            {
                if (!head.Equals(value))
                {
                    head = value;
                    RaisePropertyChanged("Head");
                }
            }
        }

        public Vector HandLeft
        {
            get { return handLeft; }
            set
            {
                if (!handLeft.Equals(value))
                {
                    handLeft = value;
                    RaisePropertyChanged("HandLeft");
                }
            }
        }

        public Vector HandRight
        {
            get { return handRight; }
            set
            {
                if (!handRight.Equals(value))
                {
                    handRight = value;
                    RaisePropertyChanged("HandRight");
                }
            }
        }

        public Vector ShoulderCenter
        {
            get { return shoulderCenter; }
            set
            {
                if (!shoulderCenter.Equals(value))
                {
                    shoulderCenter = value;
                    RaisePropertyChanged("ShoulderCenter");
                }
            }
        }

        public Vector ShoulderRight
        {
            get { return shoulderRight; }
            set
            {
                if (!shoulderRight.Equals(value))
                {
                    shoulderRight = value;
                    RaisePropertyChanged("ShoulderRight");
                }
            }
        }

        public Vector ShoulderLeft
        {
            get { return shoulderLeft; }
            set
            {
                if (!shoulderLeft.Equals(value))
                {
                    shoulderLeft = value;
                    RaisePropertyChanged("ShoulderLeft");
                }
            }
        }

        public Vector AnkleRight
        {
            get { return ankleRight; }
            set
            {
                if (!ankleRight.Equals(value))
                {
                    ankleRight = value;
                    RaisePropertyChanged("AnkleRight");
                }
            }
        }

        public Vector AnkleLeft
        {
            get { return ankleLeft; }
            set
            {
                if (!ankleLeft.Equals(value))
                {
                    ankleLeft = value;
                    RaisePropertyChanged("AnkleLeft");
                }
            }
        }

        public Vector FootLeft
        {
            get { return footLeft; }
            set
            {
                if (!footLeft.Equals(value))
                {
                    footLeft = value;
                    RaisePropertyChanged("FootLeft");
                }
            }
        }

        public Vector FootRight
        {
            get { return footRight; }
            set
            {
                if (!footRight.Equals(value))
                {
                    footRight = value;
                    RaisePropertyChanged("FootRight");
                }
            }
        }

        public Vector WristLeft
        {
            get { return wristLeft; }
            set
            {
                if (!wristLeft.Equals(value))
                {
                    wristLeft = value;
                    RaisePropertyChanged("WristLeft");
                }
            }
        }

        public Vector WristRight
        {
            get { return wristRight; }
            set
            {
                if (!wristRight.Equals(value))
                {
                    wristRight = value;
                    RaisePropertyChanged("WristRight");
                }
            }
        }

        public Vector ElbowLeft
        {
            get { return elbowLeft; }
            set
            {
                if (!elbowLeft.Equals(value))
                {
                    elbowLeft = value;
                    RaisePropertyChanged("ElbowLeft");
                }
            }
        }

        public Vector ElbowRight
        {
            get { return elbowRight; }
            set
            {
                if (!elbowRight.Equals(value))
                {
                    elbowRight = value;
                    RaisePropertyChanged("ElbowRight");
                }
            }
        }

        public Vector KneeLeft
        {
            get { return kneeLeft; }
            set
            {
                if (!kneeLeft.Equals(value))
                {
                    kneeLeft = value;
                    RaisePropertyChanged("KneeLeft");
                }
            }
        }

        public Vector KneeRight
        {
            get { return kneeRight; }
            set
            {
                if (!kneeRight.Equals(value))
                {
                    kneeRight = value;
                    RaisePropertyChanged("KneeRight");
                }
            }
        }

        public Vector HipCenter
        {
            get { return hipCenter; }
            set
            {
                if (!hipCenter.Equals(value))
                {
                    hipCenter = value;
                    RaisePropertyChanged("HipCenter");
                }
            }
        }

        public string Gesture
        {
            get { return gesture; }
            set
            {
                if (!gesture.Equals(value))
                {
                    gesture = value;
                    RaisePropertyChanged("Gesture");
                }
            }
        }
    }

    public class SkeletonEventArgs : EventArgs
    {
        public Vector RightHandPosition { get; set; }
    }

    public class SwipeEventArgs : EventArgs
    {
        public string Gesture { get; set; }
    }
}
    