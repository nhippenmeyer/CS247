using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace OFWGKTA
{
    [ValueConversion(typeof(bool), typeof(Color))]
    public class StageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool onStage = (bool)value;
            if (onStage)
            {
                return Colors.Green;
            }
            else
            {
                return Colors.Red;
            }
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
    [ValueConversion(typeof(float), typeof(float))]
    public class CoordinateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float param;
            if (parameter != null)
            {
                param = (float)parameter;
            }
            else
            {
                param = -10;
            }

            float coord = (float)value;
            return coord + param;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float translatedCoord = (float)value;
            return translatedCoord + 10;
        }
    }

    [ValueConversion(typeof(float), typeof(float))]
    public class NormalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float param;
            if (parameter != null)
            {
                param = (float)parameter;
            }
            else
            {
                param = 640;
            }

            float coord = (float)value;
            return coord/param;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float translatedCoord = (float)value;
            return translatedCoord + 10;
        }
    }

    [ValueConversion(typeof(string), typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double param;
            if (parameter != null)
            {
                param = (double) double.Parse((string)parameter);
            }
            else
            {
                param = -10;
            }

            double coord = (double)value;
            return coord + param;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double translatedCoord = (double)value;
            return translatedCoord + 10;
        }
    }
}
