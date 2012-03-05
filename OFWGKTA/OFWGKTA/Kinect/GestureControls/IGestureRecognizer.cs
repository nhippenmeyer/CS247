using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace OFWGKTA
{
    public interface IGestureRecognizer
    {
        void Add(KinectModel kinect);
        void Disable();
        void Enable();
        protected bool isClutched;
        public bool IsClutched { get; set; }
    }
}
