using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OFWGKTA
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        void buttonLoaded(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = (SettingsViewModel)DataContext;
            vm.backButton = (Button)sender;
        }

        void sliderLoaded_MicLevel(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = (SettingsViewModel)DataContext;
            vm.sliderMicLevel = (Slider)sender;
        }

        void sliderLoaded_Bpm(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = (SettingsViewModel)DataContext;
            vm.sliderBpm = (Slider)sender;
        }

    }
}
