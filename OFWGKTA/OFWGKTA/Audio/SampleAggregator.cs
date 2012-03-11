﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace OFWGKTA
{
    public class SampleAggregator
    {
        // volume
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        public event EventHandler Restart = delegate { };
        public event EventHandler Start = delegate { };
        public event EventHandler Stop = delegate { };

        public float maxValue;
        public float minValue;

        public int NotificationCount { get; set; }
        int count;

        public SampleAggregator()
        {
        }

        public void RaiseStart()
        {
            Start(this, EventArgs.Empty);
        }

        public void RaiseStop()
        {
            Stop(this, EventArgs.Empty);
        }

        public void RaiseRestart()
        {
            Restart(this, EventArgs.Empty);
        }

        private void Reset()
        {
            count = 0;
            maxValue = minValue = 0;            
        }

        public void Add(float value)
        {
            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
            count++;
            if (count >= NotificationCount && NotificationCount > 0)
            {
                if (MaximumCalculated != null)
                {
                    MaximumCalculated(this, new MaxSampleEventArgs(minValue, maxValue));
                }
                Reset();
            }   
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            this.MaxSample = maxValue;
            this.MinSample = minValue;
        }
        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }
}