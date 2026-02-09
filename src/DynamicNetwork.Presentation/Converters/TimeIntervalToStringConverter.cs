using DynamicNetwork.Domain.Graph;
using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class TimeIntervalToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TimeInterval interval && interval != TimeInterval.Empty
            ? $"[{interval.Start} — {interval.End}]"
            : "[не определён]";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}