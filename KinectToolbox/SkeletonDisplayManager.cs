using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Kinect.Toolbox.Record;
using Microsoft.Research.Kinect.Nui;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Kinect.Toolbox
{
    public class SkeletonDisplayManager
    {
        readonly Canvas rootCanvas;
        readonly SkeletonEngine skeletonEngine;

        public SkeletonDisplayManager(SkeletonEngine engine, Canvas root)
        {
            rootCanvas = root;
            skeletonEngine = engine;
        }

        void GetCoordinates(JointID jointID, IEnumerable<Joint> joints, out float x, out float y)
        {
            var joint = joints.Where(j => j.ID == jointID).First();

            skeletonEngine.SkeletonToDepthImage(joint.Position, out x, out y);

            x = (float)(x * rootCanvas.ActualWidth);
            y = (float)(y * rootCanvas.ActualHeight);
        }

        void Plot(JointID centerID, List<Joint> joints)
        {
            float centerX;
            float centerY;

            GetCoordinates(centerID, joints, out centerX, out centerY);

            const double diameter = 8;

            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeLineJoin = PenLineJoin.Round
            };

            Canvas.SetLeft(ellipse, centerX - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centerY - ellipse.Height / 2);

            rootCanvas.Children.Add(ellipse);
        }

        void Plot(JointID centerID, JointID baseID, List<Joint> joints)
        {
            float centerX;
            float centerY;

            GetCoordinates(centerID, joints, out centerX, out centerY);

            float baseX;
            float baseY;

            GetCoordinates(baseID, joints, out baseX, out baseY);

            double diameter = Math.Abs(baseY - centerY);

            Ellipse ellipse = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 4.0,
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeLineJoin = PenLineJoin.Round
            };

            Canvas.SetLeft(ellipse, centerX - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centerY - ellipse.Height / 2);

            rootCanvas.Children.Add(ellipse);
        }

        void Trace(JointID sourceID, JointID destinationID, List<Joint> joints)
        {
            float sourceX;
            float sourceY;

            GetCoordinates(sourceID, joints, out sourceX, out sourceY);

            float destinationX;
            float destinationY;

            GetCoordinates(destinationID, joints, out destinationX, out destinationY);

            Line line = new Line
                            {
                                X1 = sourceX,
                                Y1 = sourceY,
                                X2 = destinationX,
                                Y2 = destinationY,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                StrokeThickness = 4.0,                                
                                Stroke = new SolidColorBrush(Colors.Green),
                                StrokeLineJoin = PenLineJoin.Round
                            };


            rootCanvas.Children.Add(line);
        }

        public void Draw(ReplaySkeletonFrame frame)
        {
            rootCanvas.Children.Clear();
            foreach (ReplaySkeletonData skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                Plot(JointID.HandLeft, skeleton.Joints);
                Trace(JointID.HandLeft, JointID.WristLeft, skeleton.Joints);
                Plot(JointID.WristLeft, skeleton.Joints);
                Trace(JointID.WristLeft, JointID.ElbowLeft, skeleton.Joints);
                Plot(JointID.ElbowLeft, skeleton.Joints);
                Trace(JointID.ElbowLeft, JointID.ShoulderLeft, skeleton.Joints);
                Plot(JointID.ShoulderLeft, skeleton.Joints);
                Trace(JointID.ShoulderLeft, JointID.ShoulderCenter, skeleton.Joints);
                Plot(JointID.ShoulderCenter, skeleton.Joints);

                Trace(JointID.ShoulderCenter, JointID.Head, skeleton.Joints);

                Plot(JointID.Head, JointID.ShoulderCenter, skeleton.Joints);

                Trace(JointID.ShoulderCenter, JointID.ShoulderRight, skeleton.Joints);
                Plot(JointID.ShoulderRight, skeleton.Joints);
                Trace(JointID.ShoulderRight, JointID.ElbowRight, skeleton.Joints);
                Plot(JointID.ElbowRight, skeleton.Joints);
                Trace(JointID.ElbowRight, JointID.WristRight, skeleton.Joints);
                Plot(JointID.WristRight, skeleton.Joints);
                Trace(JointID.WristRight, JointID.HandRight, skeleton.Joints);
                Plot(JointID.HandRight, skeleton.Joints);

                Trace(JointID.ShoulderCenter, JointID.Spine, skeleton.Joints);
                Plot(JointID.Spine, skeleton.Joints);
                Trace(JointID.Spine, JointID.HipCenter, skeleton.Joints);
                Plot(JointID.HipCenter, skeleton.Joints);

                Trace(JointID.HipCenter, JointID.HipLeft, skeleton.Joints);
                Plot(JointID.HipLeft, skeleton.Joints);
                Trace(JointID.HipLeft, JointID.KneeLeft, skeleton.Joints);
                Plot(JointID.KneeLeft, skeleton.Joints);
                Trace(JointID.KneeLeft, JointID.AnkleLeft, skeleton.Joints);
                Plot(JointID.AnkleLeft, skeleton.Joints);
                Trace(JointID.AnkleLeft, JointID.FootLeft, skeleton.Joints);
                Plot(JointID.FootLeft, skeleton.Joints);

                Trace(JointID.HipCenter, JointID.HipRight, skeleton.Joints);
                Plot(JointID.HipRight, skeleton.Joints);
                Trace(JointID.HipRight, JointID.KneeRight, skeleton.Joints);
                Plot(JointID.KneeRight, skeleton.Joints);
                Trace(JointID.KneeRight, JointID.AnkleRight, skeleton.Joints);
                Plot(JointID.AnkleRight, skeleton.Joints);
                Trace(JointID.AnkleRight, JointID.FootRight, skeleton.Joints);
                Plot(JointID.FootRight, skeleton.Joints);
            }
        }
    }
}
