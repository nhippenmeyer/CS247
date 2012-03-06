using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;
using Coding4Fun.Kinect.Wpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;

namespace OFWGKTA
{
    public class FreePlayKinectModel : KinectModel
    {
        // Normal kinect related parameters
        protected bool isRecorder = false;
        protected Stream fileStream;
        protected Runtime kinectRuntime;
        protected SkeletonRecorder recorder = new SkeletonRecorder();
        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;

        public void Beep(object sender, MenuEventArgs e)
        {
            Console.Beep();
        }

        public FreePlayKinectModel(Stream fileStream) : base()
        {
            Messenger.Default.Register<ShuttingDownMessage>(this, (message) => OnShuttingDown(message));
            if (fileStream != null)
            {
                this.fileStream = fileStream;
                isRecorder = true;
                recorder.Start(fileStream);
            }

            if (Runtime.Kinects.Count > 0)
            {
                kinectRuntime = Runtime.Kinects[0];
                kinectRuntime.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex);
                kinectRuntime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

            }
        }

        public override void Destroy()
        {
            if (kinectRuntime != null)
            {
                kinectRuntime.SkeletonFrameReady -= SkeletonFrameReady;
                kinectRuntime.Uninitialize();
            }

            if (isRecorder)
            {
                recorder.Stop();
                if (this.fileStream != null)
                {
                    this.fileStream.Close();
                }
            }
        }

        private void OnShuttingDown(ShuttingDownMessage message)
        {
            Destroy();
        }

        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;
            if (isRecorder)
            {
                recorder.Record(allSkeletons);
            }

            // Get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();

            if (skeleton != null)
            {
                // Set positions on our joints of interest
                
                Head = GetScaledPosition(skeleton.Joints[JointID.Head]);
                HandLeft = GetScaledPosition(skeleton.Joints[JointID.HandLeft]);
                HandRight = GetScaledPosition(skeleton.Joints[JointID.HandRight]);
                ShoulderCenter = GetScaledPosition(skeleton.Joints[JointID.ShoulderCenter]);
                ShoulderRight = GetScaledPosition(skeleton.Joints[JointID.ShoulderRight]);
                ShoulderLeft = GetScaledPosition(skeleton.Joints[JointID.ShoulderLeft]);
                AnkleRight = GetScaledPosition(skeleton.Joints[JointID.AnkleRight]);
                AnkleLeft = GetScaledPosition(skeleton.Joints[JointID.AnkleLeft]);
                FootLeft = GetScaledPosition(skeleton.Joints[JointID.FootLeft]);
                FootRight = GetScaledPosition(skeleton.Joints[JointID.FootRight]);
                WristLeft = GetScaledPosition(skeleton.Joints[JointID.WristLeft]);
                WristRight = GetScaledPosition(skeleton.Joints[JointID.WristRight]);
                ElbowLeft = GetScaledPosition(skeleton.Joints[JointID.ElbowLeft]);
                ElbowRight = GetScaledPosition(skeleton.Joints[JointID.ElbowRight]);
                KneeLeft = GetScaledPosition(skeleton.Joints[JointID.KneeLeft]);
                KneeRight = GetScaledPosition(skeleton.Joints[JointID.KneeRight]);
                HipCenter = GetScaledPosition(skeleton.Joints[JointID.HipCenter]);

                if (SkeletonUpdated != null)
                {
                    SkeletonUpdated(this, new SkeletonEventArgs()
                    {
                        LeftHandPosition = skeleton.Joints[JointID.HandLeft].Position,
                        RightHandPosition = skeleton.Joints[JointID.HandRight].Position
                    });
                }
            }
        }
    }
}