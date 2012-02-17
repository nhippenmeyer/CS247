using Microsoft.Research.Kinect.Nui;
using System.IO;

namespace Kinect.Toolbox.Record
{
    public class ReplaySkeletonFrame
    {
        public Vector FloorClipPlane { get; private set; }
        public int FrameNumber { get; private set; }
        public Vector NormalToGravity { get; private set; }
        public SkeletonFrameQuality Quality { get; private set; }
        public ReplaySkeletonData[] Skeletons { get; private set; }
        public long TimeStamp { get; private set; }

        public ReplaySkeletonFrame(SkeletonFrame frame)
        {
            FloorClipPlane = frame.FloorClipPlane;
            FrameNumber = frame.FrameNumber;
            NormalToGravity = frame.NormalToGravity;
            Quality = frame.Quality;
            TimeStamp = frame.TimeStamp;
            Skeletons = new ReplaySkeletonData[frame.Skeletons.Length];
            
            for (int index = 0; index < Skeletons.Length; index++)
            {
                Skeletons[index] = frame.Skeletons[index];
            }
        }

        internal ReplaySkeletonFrame(BinaryReader reader, int frameNumber)
        {
            TimeStamp = reader.ReadInt64();
            FloorClipPlane = reader.ReadVector();
            Quality = (SkeletonFrameQuality) reader.ReadInt32();
            NormalToGravity = reader.ReadVector();
            FrameNumber = frameNumber;

            int skeletonsCount = reader.ReadInt32();

            Skeletons = new ReplaySkeletonData[skeletonsCount];

            for (int index = 0; index < skeletonsCount; index++)
            {
                Skeletons[index] = new ReplaySkeletonData(reader);
            }
        }

        public static implicit operator ReplaySkeletonFrame(SkeletonFrame frame)
        {
            return new ReplaySkeletonFrame(frame);
        }
    }
}
