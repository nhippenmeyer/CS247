using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace OFWGKTA
{
    public enum AudioTrackState
    {
        Stopped,
        Monitoring,
        Playing,
        Recording,
        RequestedStop
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
        private const WaveFormat waveFormat = new WaveFormat(44100, 1);
        private const long maxFileLength = waveFormat.AverageBytesPerSecond * maxRecordingLengthInSeconds

        /**
         * Public audio track state property
         */
        AudioTrackState state;
        public AudioTrackState State
        {
            get
            {
                return state;
            }
            // TODO? set 
        }


        /**
         * Writer instance variable and its helper method
         */
        WaveFileWriter writer;
        
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
                // TODO:
                Stop();
            }
        }

        
        /**
         * Reader instance variable
         */
        WaveFileReader reader;


        /**
         * WaveIn iVar, handler and helper
         */
        WaveIn waveIn;
        
        // ...
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;

            if (this.state == AudioTrackState.Recording
                ||
                this.state == AudioTrackState.RequestedStop)
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

        
        /**
         * Other iVars
         */ 
        
        SampleAggregator sampleAggregator;
        int recordingDeviceIndex;

        
        /**
         * Constructor
         */ 
        AudioTrack(int recordingDeviceIndex)
        {
            this.recordingDeviceIndex = recordingDeviceIndex;
            
            this.sampleAggregator = new SampleAggregator();
            this.sampleAggregator.NotificationCount = waveFormat.SampleRate / 10;

            this.state = AudioTrackState.Stopped;
        }

    }
}
