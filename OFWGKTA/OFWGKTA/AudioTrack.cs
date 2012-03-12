using NAudio.Mixer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace OFWGKTA
{
    public enum AudioTrackState
    {
        Monitoring,
        Recording,
        StopRecording,
        Loaded,
        Playing
    }

    /**
     * A model representing a single audio track
     * Supports recording, writing to a file, and playing back
     * TODO: support loading existing files
     */
    public class AudioTrack
    {
        /**
         * Constants
         */
        private const uint maxRecordingLengthInSeconds = 60;
        private static WaveFormat waveFormat = new WaveFormat(44100, 1);
        private static long maxFileLength = waveFormat.AverageBytesPerSecond * maxRecordingLengthInSeconds;

        /** 
         * Instance variables
         */
        WaveFileWriter writer;     
        WaveIn waveIn;
        WaveOut waveOut;
        int recordingDeviceIndex;
        private string waveFileName;
        TimeSpan playTime;

        // TODO: event for updating time?

        /**
         * Constructors
         */ 
        public AudioTrack(int recordingDeviceIndex)
        {
            this.recordingDeviceIndex = recordingDeviceIndex;

            // TODO: get rid of?
            sampleAggregator = new SampleAggregator();
            sampleAggregator.NotificationCount = waveFormat.SampleRate / 10;

            this.State = AudioTrackState.Monitoring;
        }

        // TODO: debug me
        public AudioTrack(string waveFileName)
        {
            this.waveFileName = waveFileName;

            this.State = AudioTrackState.Loaded;
        }

        /**
         * State property
         */
        AudioTrackState state;
        public AudioTrackState State
        {
            get
            {
                return state;
            }
            set
            {
                // init -> monitoring
                // OR
                // loaded -> monitoring
                if ((waveIn == null
                     ||
                     state == AudioTrackState.Loaded)
                    &&
                    value == AudioTrackState.Monitoring)
                {
                    waveIn = new WaveIn();
                    waveIn.DeviceNumber = recordingDeviceIndex;
                    waveIn.DataAvailable += waveIn_DataAvailable;
                    waveIn.WaveFormat = waveFormat;
                    waveIn.StartRecording();

                    TryGetVolumeControl();
                }

                // monitoring -> recording
                else if (state == AudioTrackState.Monitoring
                         &&
                         value == AudioTrackState.Recording)
                {
                    this.waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
                    writer = new WaveFileWriter(waveFileName, waveFormat);
                    waveIn.RecordingStopped += new EventHandler(waveIn_RecordingStopped);
                }

                // recording -> stop recording
                else if (state == AudioTrackState.Recording
                         &&
                         value == AudioTrackState.StopRecording)
                {
                    waveIn.StopRecording();
                }

                // stop recording -> loaded
                // ONLY called from writeBufferToFile
                // NOT to be set from anywhere else
                else if (state == AudioTrackState.StopRecording
                         &&
                         value == AudioTrackState.Loaded)
                {
                    writer.Dispose();
                    writer = null;

                    waveIn.DataAvailable -= waveIn_DataAvailable;
                    waveIn.RecordingStopped -= new EventHandler(waveIn_RecordingStopped);
                    waveIn.Dispose();
                    waveIn = null;
                }

                // loaded -> playing
                else if (state == AudioTrackState.Loaded
                         &&
                         value == AudioTrackState.Playing)
                {

                    waveOut = new WaveOut();
                    WaveFileReader waveReader = new WaveFileReader(this.waveFileName);
                    
                    TrimWaveStream inStream = new TrimWaveStream(waveReader);
                    inStream.CurrentTime = new TimeSpan(0); //#ticks = time * (1sec / 100ns)

                    waveOut.Init(inStream);
                    waveOut.PlaybackStopped += new EventHandler(waveOut_PlaybackStopped);
                    waveOut.Play();

                    playTime = DateTime.Now.TimeOfDay;
                }

                // playing -> loaded
                else if (state == AudioTrackState.Playing
                         &&
                         value == AudioTrackState.Loaded)
                {
                    waveOut.Stop();
                }

                // throw exception
                else
                {
                    throw new InvalidOperationException("Can't transition to  " + value.ToString() + " state from " + state.ToString() + " state");
                }

                // set state value
                state = value;
            }
        }


        void waveOut_PlaybackStopped(object sender, EventArgs e)
        {
            this.state = AudioTrackState.Loaded;
        }

        /**
         * SampleAggregator property
         */
        SampleAggregator sampleAggregator;
        public SampleAggregator SampleAggregator
        {
            get
            {
                return sampleAggregator;
            }
        }


        /**
         * WaveIn event handlers
         */

        // called from waveIn_DataAvailable
        private void writeBufferToFile(byte[] buffer, int bytesRecorded)
        {
            int toWrite = (int)Math.Min(maxFileLength - writer.Length, bytesRecorded);
            if (toWrite > 0)
            {
                writer.WriteData(buffer, 0, bytesRecorded);
            }
            else
            {
                // done writing
                this.state = AudioTrackState.Loaded;
            }
        }

        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;

            // write buffer to file if recording
            if (this.state == AudioTrackState.Recording
                ||
                this.state == AudioTrackState.StopRecording)
            {
                writeBufferToFile(buffer, bytesRecorded);
            }
            
            // add sample to sample aggregator
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }

        void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            // save wave file
            AudioSaver saver = new AudioSaver(this.waveFileName);

            // TODO: allow trimming recording?
            //saver.TrimFromStart = PositionToTimeSpan(LeftPosition);
            //saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition);

            // TODO: generate a more meaningful unique filename 
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Guid.NewGuid().ToString() + ".wav");
            saver.SaveFileFormat = SaveFileFormat.Wav;
            saver.SaveAudio(fileName);

            this.State = AudioTrackState.Loaded;
        }

        public TimeSpan Time
        {
            get
            {
                if (this.State == AudioTrackState.Playing)
                {
                    return DateTime.Now.TimeOfDay.Subtract(playTime);
                }
                else if (this.State == AudioTrackState.Recording)
                {
                    return TimeSpan.FromSeconds((double)writer.Length / writer.WaveFormat.AverageBytesPerSecond);
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        UnsignedMixerControl volumeControl;
    
        private void TryGetVolumeControl()
        {
            int waveInDeviceNumber = this.recordingDeviceIndex;

            var mixerLine = waveIn.GetMixerLine();
            //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    this.volumeControl = control as UnsignedMixerControl;
                    MicrophoneLevel = desiredVolume;
                    break;
                }
            }
        }

        double desiredVolume = 100;
        public double MicrophoneLevel
        {
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                if (volumeControl != null)
                {
                    volumeControl.Percent = value;
                }
            }
        }
    }
}
