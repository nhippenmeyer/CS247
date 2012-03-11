using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Research.Kinect.Nui;

namespace OFWGKTA
{
    [ValueConversion(typeof(Vector), typeof(Vector))]
    public class VectorScaler : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector input = (Vector)value;
            input.X /= 2;
            input.X += 160;
            input.Y /= 2;
            input.Y += 200;
            return input;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector input = (Vector)value;
            return input;
        }
    }

    [ValueConversion(typeof(bool), typeof(Color))]
    public class StageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool onStage = (bool)value;
            if (onStage)
            {
                return Colors.White;
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
                param = float.Parse((string)parameter);
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
                param = (double)double.Parse((string)parameter);
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

    [ValueConversion(typeof(int), typeof(int))]
    public class OptionWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 630 / (int)value - 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 630 / ((int)value + 10);
        }
    }
}
