using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace SkeletalTracking
{
    class CustomController1 : SkeletonController
    {
        public CustomController1(MainWindow win) : base(win){}

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {
            foreach (var target in targets)
            {
                Target cur = target.Value;
                int targetID = cur.id; //ID in range [1..5]

                //Scale the joints to the size of the window
                Joint rightElbow = skeleton.Joints[JointID.ElbowRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

                double deltaX_arm = rightHand.Position.X - rightElbow.Position.X;
                double deltaY_arm = rightHand.Position.Y - rightElbow.Position.Y;

                double slope_arm = (deltaY_arm / deltaX_arm);
                double intercept_arm = rightElbow.Position.Y - slope_arm * rightElbow.Position.X;
                double distance = Math.Abs(cur.getYPosition() - slope_arm * cur.getXPosition() - intercept_arm) / Math.Sqrt(Math.Pow(slope_arm, 2) + 1);


                //If we have a hit in a reasonable range, highlight the target
                if (distance < 20)
                {
                    if ((Math.Sign(deltaX_arm) == -1 && cur.getXPosition() < rightHand.Position.X) ||
                         Math.Sign(deltaX_arm) == 1 && cur.getXPosition() > rightHand.Position.X)
                    {
                        cur.setTargetSelected();
                    }
                }
                else
                {
                    cur.setTargetUnselected();
                }
            }
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {

            /* YOUR CODE HERE */

        }
    }
}
