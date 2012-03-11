using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace OFWGKTA
{
    class StateRecognizer : ViewModelBase, IGestureRecognizer
    {
        // Kinect related variables for UI
        static int appWidth = 640;
        static double stageSize = .20; // portion of center of screen treated as stage, whole screen = 1
        static double stageLeft = ((1 - stageSize) / 2) * appWidth;
        static double stageRight = appWidth - stageLeft;

        public bool Disabled { get; private set; }

        bool isOnStage = false;
        public StateRecognizer()
        {
            this.Disabled = false;
        }

        public void Update(KinectModel kinect)
        {
            IsOnStage = !(kinect.Head.X < stageLeft || kinect.Head.X > stageRight);
        }

        // Nothing necessary for most of these functions, since it's just noting 
        // whether or not the user is in a particular state/stance
        public void Disable() {}

        public void Enable() {}

        public bool IsClutched { get { return false; } }

        public bool IsOnStage
        {
            get { return isOnStage; }
            set
            {
                if (isOnStage != value)
                {
                    isOnStage = value;
                    RaisePropertyChanged("IsOnStage");
                }
            }
        }
    }
}
