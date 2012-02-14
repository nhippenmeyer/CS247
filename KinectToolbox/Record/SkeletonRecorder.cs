using System;
using System.IO;
using Microsoft.Research.Kinect.Nui;

namespace Kinect.Toolbox.Record
{
    public class SkeletonRecorder
    {
        Stream recordStream;
        BinaryWriter writer;
        DateTime referenceTime;

        public void Start(Stream stream)
        {
            recordStream = stream;
            writer = new BinaryWriter(recordStream);

            referenceTime = DateTime.Now;
        }

        public void Record(SkeletonFrame frame)
        {
            if (writer == null)
                throw new Exception("You must call Start before calling Record");

            TimeSpan timeSpan = DateTime.Now.Subtract(referenceTime);
            referenceTime = DateTime.Now;
            writer.Write((long)timeSpan.TotalMilliseconds);
            writer.Write(frame.FloorClipPlane);
            writer.Write((int)frame.Quality);
            writer.Write(frame.NormalToGravity);

            writer.Write(frame.Skeletons.Length);

            foreach (SkeletonData skeleton in frame.Skeletons)
            {
                writer.Write((int)skeleton.TrackingState);
                writer.Write(skeleton.Position);
                writer.Write(skeleton.TrackingID);
                writer.Write(skeleton.UserIndex);
                writer.Write((int)skeleton.Quality);

                writer.Write(skeleton.Joints.Count);
                foreach (Joint joint in skeleton.Joints)
                {
                    writer.Write((int)joint.ID);
                    writer.Write((int)joint.TrackingState);
                    writer.Write(joint.Position);
                }
            }
        }

        public void Stop()
        {
            if (writer == null)
                throw new Exception("You must call Start before calling Stop");

            writer.Close();
            writer.Dispose();

            recordStream.Dispose();
            recordStream = null;
        }
    }
}
