using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Kinect.Toolbox.Voice
{
    public class VoiceCommander : IDisposable
    {
        const string RecognizerId = "SR_MS_en-US_Kinect_10.0";
        Thread workingThread;
        readonly Choices choices;
        bool isRunning;
        SpeechRecognitionEngine speechRecognitionEngine;

        public event Action<string> OrderDetected;

        public VoiceCommander(params string[] orders)
        {
            choices = new Choices();
            choices.Add(orders);
        }

        public void Start()
        {
            workingThread = new Thread(Record);
            workingThread.IsBackground = true;
            workingThread.SetApartmentState(ApartmentState.MTA);
            workingThread.Start();  
        }

        void Record()
        {
            using (KinectAudioSource source = new KinectAudioSource
            {
                FeatureMode = true,
                AutomaticGainControl = false,
                SystemMode = SystemMode.OptibeamArrayOnly
            })
            {
                RecognizerInfo recognizerInfo = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();

                if (recognizerInfo == null)
                    return;

                speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

                var gb = new GrammarBuilder {Culture = recognizerInfo.Culture};
                gb.Append(choices);

                var grammar = new Grammar(gb);

                speechRecognitionEngine.LoadGrammar(grammar);
                using (Stream sourceStream = source.Start())
                {
                    speechRecognitionEngine.SetInputToAudioStream(sourceStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

                    isRunning = true;
                    while (isRunning)
                    {
                        RecognitionResult result = speechRecognitionEngine.Recognize();
                        if (result != null && OrderDetected != null && result.Confidence > 0.7)
                            OrderDetected(result.Text);
                    }
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Dispose()
        {
            Stop();

            if (speechRecognitionEngine != null)
            {
                speechRecognitionEngine.Dispose();
            }
        }
    }
}
