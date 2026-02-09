using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class BooleanToYesNoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? (b ? "Да" : "Нет") : "Н/Д";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
