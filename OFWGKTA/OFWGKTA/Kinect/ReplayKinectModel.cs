using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;
using Kinect.Toolbox;

namespace OFWGKTA
{
    class ReplayKinectModel : KinectModel
    {
        private SkeletonReplay replay;
        private Stream fileStream;
        private bool isReplaying = false;

        public bool IsReplaying { get { return isReplaying; } }

        public ReplayKinectModel(Stream fileStream) : base()
        {
            this.fileStream = fileStream;
            replay = new SkeletonReplay(fileStream);
            replay.SkeletonFrameReady += new EventHandler<ReplaySkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            replay.Start();
        }

        public override void Destroy()
        {
            replay.SkeletonFrameReady -= SkeletonFrameReady;
            replay.Stop();
            if (this.fileStream != null)
            {
                this.fileStream.Close();
            }
        }

        void SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ReplaySkeletonData skeleton = e.SkeletonFrame.Skeletons[0];

            // Retrieve the tracked skeleton
            foreach (var s in e.SkeletonFrame.Skeletons)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    skeleton = s;
                    break;
                }
            }

            Joint leftHandUnscaled = new Joint();
            Joint rightHandUnscaled = new Joint();

            Vector3 skeletonPosition = new Vector3(skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z);
            barycenterHelper.Add(skeletonPosition, skeleton.TrackingID);
            IsStable = barycenterHelper.IsStable(skeleton.TrackingID);

            for (int i = 0; i < skeleton.Joints.Count; i++)
            {
                switch (skeleton.Joints.ElementAt(i).ID)
                {
                    case (JointID.Head):
                        Head = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.HandLeft):
                        HandLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        leftHandUnscaled = skeleton.Joints.ElementAt(i);
                        break;
                    case (JointID.HandRight):
                        HandRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        rightHandUnscaled = skeleton.Joints.ElementAt(i);
                        break;
                    case (JointID.ShoulderCenter):
                        ShoulderCenter = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.ShoulderRight):
                        ShoulderRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.ShoulderLeft):
                        ShoulderLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.AnkleLeft):
                        AnkleLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.AnkleRight):
                        AnkleRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.FootRight):
                        FootRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.FootLeft):
                        FootLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.WristLeft):
                        WristLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.WristRight):
                        WristRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.KneeRight):
                        KneeRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.KneeLeft):
                        KneeLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.ElbowLeft):
                        ElbowLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.ElbowRight):
                        ElbowRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.HipCenter):
                        HipCenter = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                }
            }
            RaiseSkeletonUpdate(new SkeletonEventArgs()
            {
                LeftHandPosition = leftHandUnscaled.Position,
                RightHandPosition = rightHandUnscaled.Position
            });
        }
    }
}
