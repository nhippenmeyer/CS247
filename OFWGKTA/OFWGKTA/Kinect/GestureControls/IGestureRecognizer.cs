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
        void Add(KinectModel kinect);
        void Disable();
        void Enable();
        bool IsClutched { get; set; }
    }
}
