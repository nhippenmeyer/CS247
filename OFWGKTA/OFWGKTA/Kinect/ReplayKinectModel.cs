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
                        Head = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.HandLeft):
                        HandLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.HandRight):
                        //HandRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.ShoulderCenter):
                        ShoulderCenter = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.ShoulderRight):
                        ShoulderRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.ShoulderLeft):
                        ShoulderLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.AnkleLeft):
                        AnkleLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.AnkleRight):
                        AnkleRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.FootRight):
                        FootRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.FootLeft):
                        FootLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.WristLeft):
                        WristLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.WristRight):
                        WristRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.KneeRight):
                        KneeRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.KneeLeft):
                        KneeLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.ElbowLeft):
                        ElbowLeft = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.ElbowRight):
                        ElbowRight = skeleton.Joints.ElementAt(i).Position;;
                        break;
                    case (JointID.HipCenter):
                        HipCenter = skeleton.Joints.ElementAt(i).Position;;
                        break;
                }
            }
        }
    }
}
