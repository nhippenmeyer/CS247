using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace OFWGKTA
{
    [ValueConversion(typeof(float), typeof(float))]
    public class CoordinateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float coord = (float)value;
            return coord - 10;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float translatedCoord = (float)value;
            return translatedCoord + 10;
        }
    }
}
