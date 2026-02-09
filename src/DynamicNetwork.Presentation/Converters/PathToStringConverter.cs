using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class PathToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> path && path.Any())
        {
            return string.Join(" → ", path);
        }
        return "Пустой путь";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}   
