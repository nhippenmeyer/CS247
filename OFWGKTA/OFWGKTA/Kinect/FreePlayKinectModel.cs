using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;
using Coding4Fun.Kinect.Wpf;

namespace OFWGKTA
{
    class FreePlayKinectModel : KinectModel
    {
        bool isRecording = false;
        public SkeletonRecorder skeletonRecorder = new SkeletonRecorder();

        public FreePlayKinectModel(Stream fileStream) : base()
        {
            if (fileStream != null)
            {
                isRecording = true;
                skeletonRecorder.Start(fileStream);
            }

            if (Runtime.Kinects.Count == 0)
            {
                // No Kinect connected
            }
            else
            {
                kinectIsConnected = true;
                kinectRuntime = Runtime.Kinects[0];
                // Initialize to do skeletal tracking
                // Add event to receive skeleton data, commented out for playback
                kinectRuntime.Initialize(RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex);
                kinectRuntime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }

        }

        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;
            if (isRecording)
            {
                skeletonRecorder.Record(allSkeletons);
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
            }
        }
    }
}
