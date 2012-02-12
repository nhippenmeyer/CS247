using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;

namespace OFWGKTA
{
    class FreePlayKinectModel : KinectModel
    {
        bool isRecording = false;
        SkeletonRecorder skeletonRecorder = new SkeletonRecorder();

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
                Head = skeleton.Joints[JointID.Head].Position;
                HandLeft = skeleton.Joints[JointID.HandLeft].Position;
                HandRight = skeleton.Joints[JointID.HandRight].Position;
                ShoulderCenter = skeleton.Joints[JointID.ShoulderCenter].Position;
                ShoulderRight = skeleton.Joints[JointID.ShoulderRight].Position;
                ShoulderLeft = skeleton.Joints[JointID.ShoulderLeft].Position;
                AnkleRight = skeleton.Joints[JointID.AnkleRight].Position;
                AnkleLeft = skeleton.Joints[JointID.AnkleLeft].Position;
                FootLeft = skeleton.Joints[JointID.FootLeft].Position;
                FootRight = skeleton.Joints[JointID.FootRight].Position;
                WristLeft = skeleton.Joints[JointID.WristLeft].Position;
                WristRight = skeleton.Joints[JointID.WristRight].Position;
                ElbowLeft = skeleton.Joints[JointID.ElbowLeft].Position;
                ElbowRight = skeleton.Joints[JointID.ElbowRight].Position;
                KneeLeft = skeleton.Joints[JointID.KneeLeft].Position;
                KneeRight = skeleton.Joints[JointID.KneeRight].Position;
                HipCenter = skeleton.Joints[JointID.HipCenter].Position;
            }
        }
    }
}
