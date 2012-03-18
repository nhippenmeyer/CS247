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
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        void buttonLoaded_Quit(object sender, RoutedEventArgs e)
        {
            HomeViewModel vm = (HomeViewModel)DataContext;
            vm.quitButton = (Button)sender;
        }

        void buttonLoaded_Start(object sender, RoutedEventArgs e)
        {
            HomeViewModel vm = (HomeViewModel)DataContext;
            vm.startButton = (Button)sender;
        }
    }
}
