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
                IGestureRecognizer gr_sender = (IGestureRecognizer)sender;
                if (gr_sender.IsClutched)
                {
                    foreach (IGestureRecognizer gr in this.recognizers)
                    {
                        if (gr != sender)
                        {
                            gr.Disable();
                        }
                    }
                }
                else
                {
                    foreach (IGestureRecognizer gr in this.recognizers)
                    {
                        gr.Enable();
                    }
                }
            }
        }
    }
}
