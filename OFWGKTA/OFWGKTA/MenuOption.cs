using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;

namespace OFWGKTA
{
    public class MenuOption
    {
        private int numOptions;

        public string Label { get; private set; }
        public RelayCommand Command { get; private set; }
        public int NumOptions { get; private set; }

        public MenuOption(string label, RelayCommand command, int numOptions)
        {
            this.Label = label;
            this.Command = command;
            this.NumOptions = numOptions;
        }
    }
}
