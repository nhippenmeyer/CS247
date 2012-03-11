using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace OFWGKTA
{
    public interface IGestureRecognizer: INotifyPropertyChanged
    {
        void Update(KinectModel kinect);
        void Disable();
        void Enable();
        bool Disabled { get; }
        bool IsClutched { get; }
    }
}
