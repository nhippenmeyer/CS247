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
    class SpeechRecognizer 
    {
        // Kinect audio related parameters
        EventHandler<SpeechRecognizedEventArgs> speechCallback = null;
        KinectAudioSource speechSource;
        SpeechRecognitionEngine speechEngine;
        RecognizerInfo speechRecInfo;
        Grammar speechGrammar = null;
        Stream stream;

        public SpeechRecognizer(List<string> wordsToRecognize, EventHandler<SpeechRecognizedEventArgs> speechCallback)
        {
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
    }
}
