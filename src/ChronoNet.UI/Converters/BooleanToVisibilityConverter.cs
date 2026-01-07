using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public bool UseHidden { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return DependencyProperty.UnsetValue;

            if (Invert)
                boolValue = !boolValue;

            return boolValue
                ? Visibility.Visible
                : (UseHidden ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return DependencyProperty.UnsetValue;

            var result = visibility == Visibility.Visible;
            return Invert ? !result : result;
        }
    }
}
