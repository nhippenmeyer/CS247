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
using System.Timers;

namespace SkeletalTracking
{
    class CustomController2 : SkeletonController
    {
        private bool selectMode;
        private double centerX;
        private double rangeX;


        public CustomController2(MainWindow win) : base(win)
        {
            rightHandTimer = null;
            rightHandTarget = null;
            rightHandTargetID = -1;
            selectMode = false;
            rangeX = 100.0;
        }

        public override void processSkeletonFrame(SkeletonData skeleton, Dictionary<int, Target> targets)
        {
            Joint leftHand = skeleton.Joints[JointID.HandLeft].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
            Joint head = skeleton.Joints[JointID.ShoulderCenter].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);

            // If we are currently in selection mode
            if (selectMode)
            {
                // Leave selection mode if left hand is below head
                if (leftHand.Position.Y > head.Position.Y)
                {
                    selectMode = false;
                    foreach (var target in targets)
                    {
                        target.Value.setTargetUnselected();
                    }
                }
                // Compute which target to select based on horizontal distance of right hand from center position
                // Note: Target keys must be in the range 1..targets.Count
                else
                {
                    Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                    double deltaX = rightHand.Position.X - centerX;;
                    int targetToSelect = Math.Min(targets.Count, Math.Max(1, (int)Math.Ceiling((deltaX / rangeX + 0.5) * targets.Count)));
                    foreach (var target in targets)
                    {
                        if (target.Key == targetToSelect)
                        {
                            if (rightHandTargetID < 0)
                            {
                                startTimer(target.Value, 1.5);
                            }
                            else if (rightHandTargetID != target.Key)
                            {
                                stopTimer(rightHandTarget);
                                startTimer(target.Value, 1.5);
                            }
                        }
                        else
                        {
                            target.Value.setTargetUnselected();
                        }
                    }
                }
            }

            // Not in selection mode
            else
            {
                // Enter selection mode if left hand is above head
                if (leftHand.Position.Y < head.Position.Y)
                {
                    selectMode = true;
                    Joint rightHand = skeleton.Joints[JointID.HandRight].ScaleTo(640, 480, window.k_xMaxJointScale, window.k_yMaxJointScale);
                    centerX = rightHand.Position.X;
                }
            }
        }

        public override void controllerActivated(Dictionary<int, Target> targets)
        {

            /* YOUR CODE HERE */

        }
    }
}
