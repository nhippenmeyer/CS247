using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFWGKTA
{
    public class AppState
    {
        private AudioKinectModel kinect;
        public AudioKinectModel Kinect { get { return kinect; } }

        private int micIndex;
        public int MicIndex { get { return micIndex; } }

        public AppState(AudioKinectModel kinect, int micIndex)
        {
            this.kinect = kinect;
            this.micIndex = micIndex;
        }
    }

    public class DemoAppState 
    {
        private string applicationMode;
        private KinectModel kinect;

        public DemoAppState(string applicationMode, KinectModel kinect)
        {
            this.applicationMode = applicationMode;
            this.kinect = kinect;
        }

        public string ApplicationMode { get { return applicationMode; } }
        public KinectModel Kinect { get { return kinect; } }
    }
}
