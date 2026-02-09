using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class CollectionToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> collection)
        {
            return string.Join(", ", collection);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
