using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFWGKTA
{
    public class AppState
    {
        private AudioKinectModel kinect;

        public AppState(AudioKinectModel kinect)
        {
            this.kinect = kinect;
        }

        public AudioKinectModel Kinect { get { return kinect; } }
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
