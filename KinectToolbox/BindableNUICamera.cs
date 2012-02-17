using Microsoft.Research.Kinect.Nui;

namespace Kinect.Toolbox
{
    public class BindableNUICamera : Notifier
    {
        readonly Camera nuiCamera;
        public int ElevationAngle
        {
            get { return nuiCamera.ElevationAngle; }
            set
            {
                if (nuiCamera.ElevationAngle == value)
                    return;

                if (value > ElevationMaximum)
                    value = ElevationMaximum;

                if (value < ElevationMinimum)
                    value = ElevationMinimum;

                nuiCamera.TrySetElevationAngle(value);

                RaisePropertyChanged(() => ElevationAngle);                
            }
        }

        public int ElevationMaximum
        {
            get { return Camera.ElevationMaximum; }
        }


        public int ElevationMinimum
        {
            get { return Camera.ElevationMinimum; }
        }

        public BindableNUICamera(Camera nuiCamera)
        {
            this.nuiCamera = nuiCamera;
        }
    }
}
