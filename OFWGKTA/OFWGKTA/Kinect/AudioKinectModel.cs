using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace OFWGKTA
{
    public class AudioKinectModel : FreePlayKinectModel 
    {
        // Kinect audio related parameters
        EventHandler<SpeechRecognizedEventArgs> speechCallback = null;
        KinectAudioSource speechSource;
        SpeechRecognitionEngine speechEngine;
        RecognizerInfo speechRecInfo;
        Grammar speechGrammar = null;
        Stream stream;

        // Kinect related variables for UI
        static int appWidth = 640;
        static double stageSize = .20; // portion of center of screen treated as stage, whole screen = 1
        static double stageLeft = ((1 - stageSize) / 2) * appWidth;
        static double stageRight = appWidth - stageLeft;

        bool isOnStage = false;

        public AudioKinectModel(List<string> wordsToRecognize, EventHandler<SpeechRecognizedEventArgs> speechCallback) : base(null)
        {
            SkeletonUpdated += new EventHandler<SkeletonEventArgs>(ParseSkeletonUpdate);
            //SwipeDetected += new EventHandler<SwipeEventArgs>(SwipeGestureCallback);

            if (wordsToRecognize != null && wordsToRecognize.Count > 0)
            {
                string RecognizerId = "SR_MS_en-US_Kinect_10.0";
                speechSource = new KinectAudioSource();

                speechSource.FeatureMode = true;
                speechSource.AutomaticGainControl = false;
                speechSource.SystemMode = SystemMode.OptibeamArrayOnly;

                this.speechRecInfo = (from r in SpeechRecognitionEngine.InstalledRecognizers() where r.Id == RecognizerId select r).FirstOrDefault();

                SetGrammar(wordsToRecognize);
                SetSpeechCallback(speechCallback);

                if (speechCallback != null)
                {
                    this.speechCallback = speechCallback;
                    speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(speechCallback);
                }

                stream = speechSource.Start();

                speechEngine.SetInputToAudioStream(stream,
                              new SpeechAudioFormatInfo(
                                  EncodingFormat.Pcm, 16000, 16, 1,
                                  32000, 2, null));

                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        public void ParseSkeletonUpdate(object sender, SkeletonEventArgs e)
        {
            // set all properties i'd like to be constantly updated on skeleton update
            // i'll know we updated the skeleton, so i should compute whether or not it's on stage
            IsOnStage = !(Head.X < stageLeft || Head.X > stageRight);

            // Feed points to gesture recognizer
            swipeGestureRecognizer.Add(HandRight, kinectRuntime.SkeletonEngine);
        }

        public void SwipeGestureCallback(object sender, SwipeEventArgs e)
        {
            // do something
        }

        // Sets words to be recognized by kinect, so they can be checked for in speechCallback
        public void SetGrammar(List<string> wordsToRecognize)
        {
            if (this.speechGrammar != null)
            {
                speechEngine.UnloadGrammar(this.speechGrammar);
            }

            if (wordsToRecognize != null && wordsToRecognize.Count > 0)
            {
                var choices = new Choices();
                foreach (var word in wordsToRecognize)
                {
                    choices.Add(word);
                }

                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = this.speechRecInfo.Culture;
                gb.Append(choices);

                this.speechGrammar = new Grammar(gb);
                speechEngine = new SpeechRecognitionEngine(this.speechRecInfo.Id);
                speechEngine.LoadGrammar(this.speechGrammar);
            }
        }

        // Sets callback function that should detect words in grammar
        public void SetSpeechCallback(EventHandler<SpeechRecognizedEventArgs> speechCallback)
        {
            if (this.speechCallback != null)
            {
                speechEngine.SpeechRecognized -= this.speechCallback;
            }
            if (speechCallback != null)
            {
                this.speechCallback = speechCallback;
                speechEngine.SpeechRecognized += speechCallback;
            }
        }

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
