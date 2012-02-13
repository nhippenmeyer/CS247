using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox.Record;

namespace OFWGKTA
{
    class ReplayKinectModel : KinectModel
    {
        public ReplayKinectModel(Stream replayFileStream) : base()
        {
            SkeletonReplay skeletonReplay = new SkeletonReplay(replayFileStream);
            skeletonReplay.SkeletonFrameReady += new EventHandler<ReplaySkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            skeletonReplay.Start();
        }

        void SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ReplaySkeletonData skeleton = e.SkeletonFrame.Skeletons[0];
            for (int i = 0; i < skeleton.Joints.Count; i++)
            {
                switch (skeleton.Joints.ElementAt(i).ID)
                {
                    case (JointID.Head):
                        Head = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.HandLeft):
                        HandLeft = GetScaledPosition(skeleton.Joints.ElementAt(i));
                        break;
                    case (JointID.HandRight):
                        HandRight = GetScaledPosition(skeleton.Joints.ElementAt(i));
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
        }
    }
}
