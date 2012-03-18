using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using System.Windows.Controls;

namespace OFWGKTA
{
    public class MenuOption : ViewModelBase
    {
        private string label;
        private string image;
        public RelayCommand Command { get; private set; }
        public int NumOptions { get; private set; }
        public MenuRecognizer MenuRecognizer { get; private set; }
        private bool isEnabled = true;

        public MenuOption(string image, RelayCommand command, int numOptions, MenuRecognizer menuRecognizer)
		{
            this.Image = image;
            this.Command = command;
            this.NumOptions = numOptions;
            this.MenuRecognizer = menuRecognizer;

            MenuRecognizer.PropertyChanged += OnPercentDepressedChanged;
		}

        /*
        public MenuOption(string label, RelayCommand command, int numOptions, MenuRecognizer menuRecognizer)
        {
            this.Label = label;
            this.Command = command;
            this.NumOptions = numOptions;
            this.MenuRecognizer = menuRecognizer;

            MenuRecognizer.PropertyChanged += OnPercentDepressedChanged;
        }
         * */

        public double PercentDepressed
        {
            get { return MenuRecognizer.PercentDepressed; }
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set {
                if (this.isEnabled != value)
                {
                    this.isEnabled = value;
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }

        public string Image {
            get
            {
                return this.image;
            }

            set
            {
                if (this.image != value)
                {
                    this.image = value;
                    RaisePropertyChanged("Image");
                }
            }
        }

        public string Label {
            get
            {
                return this.label;
            }

            set
            {
                if (this.label != value)
                {
                    this.label = value;
                    RaisePropertyChanged("Label");
                }
            }
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
