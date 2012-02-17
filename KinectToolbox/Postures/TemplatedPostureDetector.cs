using System.IO;
using Kinect.Toolbox.Record;

namespace Kinect.Toolbox
{
    public class TemplatedPostureDetector : PostureDetector
    {
        const float Epsilon = 0.02f;
        const float MinimalScore = 0.95f;
        const float MinimalSize = 0.1f;
        readonly LearningMachine learningMachine;
        readonly string postureName;

        public LearningMachine LearningMachine
        {
            get { return learningMachine; }
        }

        public TemplatedPostureDetector(string postureName, Stream kbStream) : base(4)
        {
            this.postureName = postureName;
            learningMachine = new LearningMachine(kbStream);
        }

        public override void TrackPostures(ReplaySkeletonData skeleton)
        {
            if (LearningMachine.Match(skeleton.Joints.ToListOfVector2(), Epsilon, MinimalScore, MinimalSize))
                RaisePostureDetected(postureName);
        }

        public void AddTemplate(ReplaySkeletonData skeleton)
        {
            RecordedPath recordedPath = new RecordedPath(skeleton.Joints.Count);

            recordedPath.Points.AddRange(skeleton.Joints.ToListOfVector2());

            LearningMachine.AddPath(recordedPath);
        }

        public void SaveState(Stream kbStream)
        {
            LearningMachine.Persist(kbStream);
        }
    }
}
