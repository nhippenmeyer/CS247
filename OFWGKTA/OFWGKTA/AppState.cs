using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFWGKTA
{
    public class AppState 
    {
        private string applicationMode;
        private KinectModel kinect;

        public AppState(string applicationMode, KinectModel kinect)
        {
            this.applicationMode = applicationMode;
            this.kinect = kinect;
        }

        public string ApplicationMode { get { return applicationMode; } }
        public KinectModel Kinect { get { return kinect; } }
    }
}
