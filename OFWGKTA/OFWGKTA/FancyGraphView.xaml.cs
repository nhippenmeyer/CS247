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
using Visiblox.Charts;
using System.Timers;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace OFWGKTA
{

    public class SamplePoint
    {
        public int sampleNum { get; set; }
        public double sampleVal { get; set; }
    }

    public class BindableSamplePointCollection : ObservableCollection<SamplePoint> { }

    /// <summary>
    /// Interaction logic for FancyGraphView.xaml
    /// </summary>
    public partial class FancyGraphView : UserControl
    {
        private static Random random = new Random();

        public FancyGraphView()
        {
            InitializeComponent();
        }
    }
}
