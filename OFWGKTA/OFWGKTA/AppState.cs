using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFWGKTA
{
    public class AppState
    {
        public KinectModel Kinect { get; private set; }
        public SpeechRecognizer SpeechRecognizer { get; private set; }
        public int MicIndex { get; private set; }

        public AppState(KinectModel kinect, SpeechRecognizer speechRecognizer, int micIndex)
        {
            this.SpeechRecognizer = speechRecognizer;
            this.Kinect = kinect;
            this.MicIndex = micIndex;
        }
    }

    public class DemoAppState 
    {
        public string ApplicationMode { get; private set; }
        public KinectModel Kinect { get; private set; }

        public DemoAppState(string applicationMode, KinectModel kinect)
        {
            this.ApplicationMode = applicationMode;
            this.Kinect = kinect;
        }

    }
}
