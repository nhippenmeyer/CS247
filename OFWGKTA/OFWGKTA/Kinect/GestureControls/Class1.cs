using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace OFWGKTA 
{
    class GestureRecognizer : ViewModelBase, IGestureRecognizer
    {
        bool isClutched;
        public bool IsClutched
        {
            get { return this.isClutched; }
            set {
                if (this.isClutched != value)
                {


        }
            
    }
}
