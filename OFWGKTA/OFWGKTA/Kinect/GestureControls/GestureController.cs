using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OFWGKTA
{
    class GestureController 
    {
        private Collection<IGestureRecognizer> recognizers = new ObservableCollection<IGestureRecognizer>();

        public GestureController()
        {
                
        }

        public void Add(IGestureRecognizer recognizer)
        {
            recognizer.PropertyChanged += KinectListener;
            this.recognizers.Add(recognizer);

        }

        void KinectListener(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsClutched")
            {
                Console.Beep();
            }
        }
    }
}
