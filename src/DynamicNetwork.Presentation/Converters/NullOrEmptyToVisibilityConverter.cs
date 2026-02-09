using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value switch
        {
            null => false,
            IEnumerable<object> collection => collection.Any(),
            string str => !string.IsNullOrWhiteSpace(str),
            _ => true
        };

        if (parameter?.ToString() == "Inverse")
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}