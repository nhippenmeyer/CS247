using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFWGKTA
{
    class NavigateMessage
    {
        public string TargetView { get; private set; }
        public object State { get; private set; }

        public NavigateMessage(string targetView, object state)
        {
            this.TargetView = targetView;
            this.State = state;
        }
    }

    class ShuttingDownMessage
    {
        public string CurrentViewName { get; private set; }
        public ShuttingDownMessage(string currentViewName)
        {
            this.CurrentViewName = currentViewName;
        }
    }
}