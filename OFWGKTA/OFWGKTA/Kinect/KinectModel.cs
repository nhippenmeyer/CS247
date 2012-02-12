using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox.Record;
using Microsoft.Research.Kinect.Nui;
using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace OFWGKTA
{
    class KinectModel : ViewModelBase, INotifyPropertyChanged
    {
        protected bool kinectIsConnected = false;
        protected Runtime kinectRuntime;
        Vector head;
        Vector handLeft;
        Vector handRight;
        Vector shoulderCenter;
        Vector shoulderRight;
        Vector shoulderLeft;
        Vector ankleRight;
        Vector ankleLeft;
        Vector footLeft;
        Vector footRight;
        Vector wristLeft;
        Vector wristRight;
        Vector elbowLeft;
        Vector elbowRight;
        Vector kneeLeft;
        Vector kneeRight;
        Vector hipCenter;

//        public event PropertyChangedEventHandler PropertyChanged;

        public KinectModel()
        {

        }

        void SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e) { }
        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) { }

        public Vector Head
        {
            get { return head; }
            set
            {
                if (!this.head.Equals(value))
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
                //if (!this.handLeft.Equals(value))
                //{
                    handLeft = value;
                    RaisePropertyChanged("HandLeft");
                //}
            }
        }

        public Vector HandRight
        {
            get { return handRight; }
            set
            {
                handRight = value;
                handRight.X = handRight.X * 500;
                handRight.Y = handRight.Y * 500;
                RaisePropertyChanged("HandRight");
            }
        }

        public Vector ShoulderCenter
        {
            get { return shoulderCenter; }
            set
            {
                if (!this.shoulderCenter.Equals(value))
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
                if (!this.shoulderRight.Equals(value))
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
                if (!this.shoulderLeft.Equals(value))
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
                if (!this.ankleRight.Equals(value))
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
                if (!this.ankleLeft.Equals(value))
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
                if (!this.footLeft.Equals(value))
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
                if (!this.footRight.Equals(value))
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
                if (!this.wristLeft.Equals(value))
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
                if (!this.wristRight.Equals(value))
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
                if (!this.elbowLeft.Equals(value))
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
                if (!this.elbowRight.Equals(value))
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
                if (!this.kneeLeft.Equals(value))
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
                if (!this.kneeRight.Equals(value))
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
                if (!this.hipCenter.Equals(value))
                {
                    hipCenter = value;
                    RaisePropertyChanged("HipCenter");
                }
            }
        }

    }
}
    