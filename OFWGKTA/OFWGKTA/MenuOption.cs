using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace OFWGKTA
{
    public class MenuOption : ViewModelBase
    {
        public string Label { get; private set; }
        public RelayCommand Command { get; private set; }
        public int NumOptions { get; private set; }
        public MenuRecognizer MenuRecognizer { get; private set; }

        public MenuOption(string label, RelayCommand command, int numOptions, MenuRecognizer menuRecognizer)
        {
            this.Label = label;
            this.Command = command;
            this.NumOptions = numOptions;
            this.MenuRecognizer = menuRecognizer;

            MenuRecognizer.PropertyChanged += OnPercentDepressedChanged;
        }

        public double PercentDepressed
        {
            get { return MenuRecognizer.PercentDepressed; }
        }

        void OnPercentDepressedChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PercentDepressed")
            {
                RaisePropertyChanged("PercentDepressed");
            }
        }
    }
}
