using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;

namespace OFWGKTA
{
    public class MenuOption
    {
        private string label;
        private RelayCommand command;

        public string Label { get { return this.label; } private set { this.label = value; } }
        public RelayCommand Command { get { return this.command; } private set { this.command = value; } }

        public MenuOption(string label, RelayCommand command)
        {
            this.Label = label;
            this.Command = command;
        }
    }
}
