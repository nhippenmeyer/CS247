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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox;

namespace OFWGKTA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DispatcherHelper.Initialize();
            var vm = new MainWindowViewModel();
            this.Closed += (s, e) => vm.Dispose();
            InitializeComponent();
            this.DataContext = vm;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
